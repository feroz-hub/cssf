/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Microsoft.IdentityModel.Tokens;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.ProxyService.Routes.Extension;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api.Wrapper;

namespace HCL.CS.SF.ProxyService.Routes;

/// <summary>
/// Central API gateway that receives all inbound HTTP POST requests to the security API
/// base path, validates bearer tokens (revocation check, signature verification, audience/issuer),
/// and dispatches them to the appropriate backend service handler. This is the main partial
/// class containing request processing, token validation, response generation, and the
/// complete handler dispatch table mapping URL paths to handler methods.
/// </summary>
internal partial class ApiGateway : BaseApiServiceInstance, IApiGateway
{
    /// <summary>
    /// Repository for looking up client configurations during token validation.
    /// </summary>
    private readonly IRepository<Clients> clientRepository;

    /// <summary>
    /// Provides access to the current HTTP context for writing API responses.
    /// </summary>
    private readonly IHttpContextAccessor httpContextAccessor;

    /// <summary>
    /// Asymmetric key store mapping algorithms to their certificate and key ID info.
    /// </summary>
    private readonly Dictionary<string, AsymmetricKeyInfoModel> keyStore;

    /// <summary>
    /// Logger service for recording token validation failures and debug information.
    /// </summary>
    private readonly ILoggerService loggerService;

    /// <summary>
    /// Repository for checking whether an access token is still active (not revoked).
    /// </summary>
    private readonly IRepository<SecurityTokens> securityTokenRepository;

    /// <summary>
    /// Token configuration settings (issuer URI, API identifier, etc.) for validation.
    /// </summary>
    private readonly TokenSettings tokenSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiGateway"/> class.
    /// </summary>
    public ApiGateway(
        IServiceProvider serviceProvider,
        HCLCSSFConfig HCLCSSFConfig,
        ILoggerInstance instance,
        Dictionary<string, AsymmetricKeyInfoModel> keyStore,
        IRepository<Clients> clientRepository,
        IHttpContextAccessor httpContextAccessor,
        IRepository<SecurityTokens> securityTokenRepository)
        : base(
            serviceProvider)
    {
        tokenSettings = HCLCSSFConfig.TokenSettings;
        this.clientRepository = clientRepository;
        this.keyStore = keyStore ?? throw new ArgumentNullException(nameof(keyStore));
        this.httpContextAccessor = httpContextAccessor;
        this.securityTokenRepository = securityTokenRepository;
        loggerService = instance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
    }

    /// <summary>
    /// Main entry point for processing an inbound API request. Performs the following steps:
    /// 1. Looks up the route definition by path to determine permission requirements.
    /// 2. For non-anonymous routes, validates the bearer token (presence, revocation, signature).
    /// 3. Reads the JSON request body from the input stream.
    /// 4. Dispatches to the matching handler method from the handler dictionary.
    /// </summary>
    /// <param name="httpContext">The current HTTP context containing the request.</param>
    /// <returns><c>true</c> if the request was processed successfully; otherwise, <c>false</c>.</returns>
    public virtual async Task<bool> ProcessRequest(HttpContext httpContext)
    {
        string path = httpContext.Request.Path;
        // Look up the API route model by exact path match.
        var api = ApiRoutePathConstants.ApiRouteModels.Find(x => x.Path == path);
        if (api != null)
        {
            var anonymousPermission = api.Permissions.Where(x => x.Contains(ApiPermissionConstants.Anonymous)).ToList();
            if (anonymousPermission == null || anonymousPermission.Count <= 0)
            {
                // Non-anonymous route: extract and validate the bearer token.
                var authorization = httpContext.Request.Headers[HeaderNames.Authorization];
                if (!AuthenticationHeaderValue.TryParse(authorization, out var headerValue))
                {
                    // No valid Authorization header found.
                    SetInvalidTokenResponse(httpContext.Response, "Access token is missing.");
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(headerValue.Parameter) && headerValue.Parameter.Contains("."))
                {
                    // Check whether the token has been revoked.
                    var result = await ValidateIsTokenActive(headerValue.Parameter, securityTokenRepository);
                    if (!result)
                    {
                        SetInvalidTokenResponse(httpContext.Response, "Access token is not active.");
                        return false;
                    }

                    // Validate the token's signature, issuer, audience, and algorithm.
                    result = await ValidateToken(headerValue.Parameter, httpContext);
                    if (!result)
                    {
                        // ValidateToken sets a more precise WWW-Authenticate response when available.
                        if (!httpContext.Response.Headers.ContainsKey(HeaderNames.WWWAuthenticate))
                            SetInvalidTokenResponse(httpContext.Response, "Access token validation failed.");
                        return false;
                    }
                }
                else
                {
                    SetInvalidTokenResponse(httpContext.Response, "Access token format is invalid.");
                    return false;
                }
            }
        }
        else
        {
            // No route definition found for this path.
            httpContext.Response.StatusCode = OpenIdConstants.HTTPStatusCodes.not_found;
            return false;
        }

        // Read the JSON request body for dispatch to the handler.
        string jsonContent;
        using (var inputStream = new StreamReader(httpContext.Request.Body, Encoding.UTF8))
        {
            jsonContent = await inputStream.ReadToEndAsync();
        }

        // Dispatch to the matching handler based on the request path.
        var apiHandler = GetHandlers();
        if (apiHandler.TryGetValue(httpContext.Request.Path, out var dynamicMethod))
            return await dynamicMethod(jsonContent);

        httpContext.Response.StatusCode = OpenIdConstants.HTTPStatusCodes.not_found;
        return false;
    }

    /// <summary>
    /// Checks whether the given access token exists in the security tokens repository,
    /// confirming it has not been revoked.
    /// </summary>
    /// <param name="accesstoken">The raw access token string.</param>
    /// <param name="securityTokenRepository">The repository to query for active tokens.</param>
    /// <returns><c>true</c> if the token is active; otherwise, <c>false</c>.</returns>
    private async Task<bool> ValidateIsTokenActive(string accesstoken,
        IRepository<SecurityTokens> securityTokenRepository)
    {
        var token = await securityTokenRepository.GetAsync(entity =>
            entity.TokenType == OpenIdConstants.TokenType.AccessToken
            && entity.TokenValue == accesstoken);
        if (token != null && token.Count > 0) return true;

        return false;
    }

    /// <summary>
    /// Validates the access token's signature, issuer, audience, algorithm, and client registration.
    /// Extracts the client_id claim from the JWT payload, looks up the client configuration,
    /// resolves the appropriate signing key, and performs cryptographic validation.
    /// Sets appropriate WWW-Authenticate error responses on failure.
    /// </summary>
    /// <param name="accesstoken">The raw access token string.</param>
    /// <param name="httpContext">The HTTP context for setting error responses.</param>
    /// <returns><c>true</c> if the token is valid; otherwise, <c>false</c>.</returns>
    private async Task<bool> ValidateToken(string accesstoken, HttpContext httpContext)
    {
        var parsedToken = new JwtSecurityToken(accesstoken);
        if (parsedToken.Claims != null)
        {
            // Extract client_id using raw JSON parsing first, then fall back to parsed claims.
            var clientId = TryReadJwtStringClaim(accesstoken, OpenIdConstants.ClaimTypes.ClientId)
                ?? (parsedToken.Payload.TryGetValue(OpenIdConstants.ClaimTypes.ClientId, out var rawClientId)
                ? rawClientId?.ToString()
                : parsedToken.Claims.FirstOrDefault(x => x.Type == OpenIdConstants.ClaimTypes.ClientId)?.Value);

            if (!string.IsNullOrWhiteSpace(clientId))
            {
                // Look up the client registration to get the allowed signing algorithm.
                var clientsList = await clientRepository.GetAsync(client => client.ClientId == clientId);
                if (clientsList != null && clientsList.Count > 0)
                {
                    var clientsEntity = clientsList[0];
                    // Resolve the signing key from the key store based on the client's algorithm.
                    var securityKey = keyStore.GetTokenKey(clientsEntity);
                    if (securityKey != null)
                    {
                        ClaimsPrincipal claimsPrincipal = null;
                        JwtSecurityToken validatedToken = null;
                        var algorithm = parsedToken.Header.TryGetValue("alg", out var algorithmValue)
                            ? algorithmValue?.ToString()
                            : null;
                        var expectedAudience = tokenSettings.TokenConfig.ApiIdentifier;
                        try
                        {
                            if (string.IsNullOrWhiteSpace(expectedAudience))
                            {
                                SetInvalidTokenResponse(httpContext.Response, "Access token audience is invalid.");
                                return false;
                            }

                            // Only RSA-SHA256 and ECDSA-SHA256 are supported for asymmetric validation.
                            if (!string.Equals(algorithm, OpenIdConstants.Algorithms.RsaSha256,
                                    StringComparison.Ordinal)
                                && !string.Equals(algorithm, OpenIdConstants.Algorithms.EcdsaSha256,
                                    StringComparison.Ordinal))
                            {
                                SetInvalidTokenResponse(httpContext.Response,
                                    "Access token signing algorithm is invalid.");
                                return false;
                            }

                            // Ensure the token's algorithm matches the client's configured algorithm.
                            if (!string.Equals(clientsEntity.AllowedSigningAlgorithm, algorithm,
                                    StringComparison.Ordinal))
                            {
                                SetInvalidTokenResponse(httpContext.Response,
                                    "Access token signing algorithm mismatch.");
                                return false;
                            }

                            (validatedToken, claimsPrincipal) = accesstoken.ValidateAsymmetricToken(
                                securityKey,
                                tokenSettings.TokenConfig.IssuerUri,
                                expectedAudience);
                            return true;
                        }
                        catch (Exception ex)
                        {
                            var subject = TryReadJwtStringClaim(accesstoken, OpenIdConstants.ClaimTypes.Sub)
                                ?? parsedToken.Claims?.FirstOrDefault(x => x.Type == OpenIdConstants.ClaimTypes.Sub)
                                    ?.Value;
                            loggerService.WriteToWithCaller(
                                Log.Error,
                                ex,
                                "Access token validation failed. client_id: " + clientId
                                + ", sub: " + (subject ?? "unknown")
                                + ", alg: " + (algorithm ?? "unknown")
                                + ", aud: " + expectedAudience);
                            SetInvalidTokenResponse(httpContext.Response, "Access token validation failed.");
                            return false;
                        }
                    }

                    SetInvalidTokenResponse(httpContext.Response, "Access token signing key is not available.");
                    return false;
                }

                SetInvalidTokenResponse(httpContext.Response, "Access token client is invalid.");
                return false;
            }

            SetInvalidTokenResponse(httpContext.Response, "Access token client_id claim is missing.");
            return false;
        }

        SetInvalidTokenResponse(httpContext.Response, "Access token is invalid.");
        return false;
    }

    /// <summary>
    /// Reads a specific claim from a JWT payload by Base64-decoding the payload segment
    /// and parsing the JSON directly, without full token validation. This approach avoids
    /// the overhead of signature verification when only claim extraction is needed.
    /// </summary>
    /// <param name="token">The raw JWT string.</param>
    /// <param name="claimType">The claim type to extract (e.g., "client_id", "sub").</param>
    /// <returns>The claim value as a string, or null if not found.</returns>
    private static string TryReadJwtStringClaim(string token, string claimType)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(claimType))
            return null;

        var sections = token.Split('.');
        if (sections.Length < 2)
            return null;

        try
        {
            var payloadBytes = Base64UrlEncoder.DecodeBytes(sections[1]);
            using var document = JsonDocument.Parse(payloadBytes);
            if (!document.RootElement.TryGetProperty(claimType, out var claimElement))
                return null;

            return claimElement.ValueKind switch
            {
                JsonValueKind.String => claimElement.GetString(),
                // For array claims, return the first string element.
                JsonValueKind.Array => claimElement.EnumerateArray()
                    .FirstOrDefault(element => element.ValueKind == JsonValueKind.String)
                    .GetString(),
                _ => claimElement.ToString()
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Sets a standardized 401 response with a WWW-Authenticate header containing
    /// the error description in the Bearer challenge format.
    /// </summary>
    /// <param name="response">The HTTP response to configure.</param>
    /// <param name="errorDescription">A human-readable error description.</param>
    private static void SetInvalidTokenResponse(HttpResponse response, string errorDescription)
    {
        var escapedDescription = EscapeHeaderValue(errorDescription);
        response.Headers[HeaderNames.WWWAuthenticate] =
            $"Bearer error=\"{OpenIdConstants.Errors.InvalidToken}\", error_description=\"{escapedDescription}\"";
        response.StatusCode = OpenIdConstants.HTTPStatusCodes.invalid_token;
    }

    /// <summary>
    /// Escapes backslashes and double quotes in a string for safe inclusion in HTTP header values.
    /// </summary>
    /// <param name="input">The string to escape.</param>
    /// <returns>The escaped string.</returns>
    private static string EscapeHeaderValue(string input)
    {
        return string.IsNullOrWhiteSpace(input)
            ? string.Empty
            : input.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    /// <summary>
    /// Writes a <see cref="FrameworkResult"/> as a JSON response. Returns HTTP 200 on success
    /// or HTTP 400 on failure.
    /// </summary>
    /// <param name="frameworkResult">The framework result to serialize.</param>
    /// <returns><c>true</c> if the result was successful; otherwise, <c>false</c>.</returns>
    private async Task<bool> GenerateApiResults(FrameworkResult frameworkResult)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (frameworkResult.Status == ResultStatus.Succeeded)
        {
            httpContext.Response.StatusCode = OpenIdConstants.HTTPStatusCodes.success;
            await httpContext.Response.WriteResponseJsonAsync(frameworkResult);
            return true;
        }

        httpContext.Response.StatusCode = OpenIdConstants.HTTPStatusCodes.bad_request;
        await httpContext.Response.WriteResponseJsonAsync(frameworkResult);
        return false;
    }

    /// <summary>
    /// Writes any typed result as a JSON response with HTTP 200.
    /// </summary>
    /// <typeparam name="T">The type of the result object.</typeparam>
    /// <param name="frameworkResult">The result object to serialize.</param>
    private async Task GenerateApiResults<T>(T frameworkResult)
    {
        var httpContext = httpContextAccessor.HttpContext;
        httpContext.Response.StatusCode = OpenIdConstants.HTTPStatusCodes.success;
        await httpContext.Response.WriteResponseJsonAsync(frameworkResult);
    }

    /// <summary>
    /// Builds the complete dispatch table mapping API route paths to their handler methods.
    /// Each entry maps a URL path constant to a method that deserializes the JSON body,
    /// invokes the appropriate backend service, and writes the response.
    /// Covers all service domains: API resources, identity resources, user accounts,
    /// roles, authentication, clients, audit trail, security tokens, notifications,
    /// and external auth management.
    /// </summary>
    /// <returns>A dictionary mapping route paths to async handler functions.</returns>
    private IDictionary<string, Func<string, Task<bool>>> GetHandlers()
    {
        return new Dictionary<string, Func<string, Task<bool>>>
        {
            // Api Resources
            { ApiRoutePathConstants.AddApiResource, AddApiResource },
            { ApiRoutePathConstants.UpdateApiResource, UpdateApiResource },
            { ApiRoutePathConstants.DeleteApiResourceById, DeleteApiResourceById },
            { ApiRoutePathConstants.DeleteApiResourceByName, DeleteApiResourceByName },
            { ApiRoutePathConstants.GetApiResourceById, GetApiResourceById },
            { ApiRoutePathConstants.GetApiResourceByName, GetApiResourceByName },
            { ApiRoutePathConstants.GetAllApiResources, GetAllApiResources },
            { ApiRoutePathConstants.AddApiResourceClaim, AddApiResourceClaim },
            { ApiRoutePathConstants.DeleteApiResourceClaimById, DeleteApiResourceClaimById },
            { ApiRoutePathConstants.DeleteApiResourceClaimByResourceIdAsync, DeleteApiResourceClaimByResourceIdAsync },
            { ApiRoutePathConstants.DeleteApiResourceClaimModel, DeleteApiResourceClaimModel },
            { ApiRoutePathConstants.GetApiResourceClaimsById, GetApiResourceClaimsById },
            { ApiRoutePathConstants.AddApiScope, AddApiScope },
            { ApiRoutePathConstants.UpdateApiScope, UpdateApiScope },
            { ApiRoutePathConstants.DeleteApiScopeById, DeleteApiScopeById },
            { ApiRoutePathConstants.DeleteApiScopeByName, DeleteApiScopeByName },
            { ApiRoutePathConstants.GetApiScopeByName, GetApiScopeByName },
            { ApiRoutePathConstants.GetApiScopeById, GetApiScopeById },
            { ApiRoutePathConstants.GetAllApiScopes, GetAllApiScopes },
            { ApiRoutePathConstants.AddApiScopeClaim, AddApiScopeClaim },
            { ApiRoutePathConstants.DeleteApiScopeClaimByScopeId, DeleteApiScopeClaimByScopeId },
            { ApiRoutePathConstants.DeleteApiScopeClaimById, DeleteApiScopeClaimById },
            { ApiRoutePathConstants.DeleteApiScopeClaimModel, DeleteApiScopeClaimModel },
            { ApiRoutePathConstants.GetApiScopeClaims, GetApiScopeClaims },
            { ApiRoutePathConstants.GetAllApiResourcesByScopesAsync, GetAllApiResourcesByScopesAsync },

            // Identity Reesource
            { ApiRoutePathConstants.AddIdentityResource, AddIdentityResource },
            { ApiRoutePathConstants.UpdateIdentityResource, UpdateIdentityResource },
            { ApiRoutePathConstants.DeleteIdentityResourceById, DeleteIdentityResourceById },
            { ApiRoutePathConstants.DeleteIdentityResourceByName, DeleteIdentityResourceByName },
            { ApiRoutePathConstants.GetIdentityResourceById, GetIdentityResourceById },
            { ApiRoutePathConstants.GetIdentityResourceByName, GetIdentityResourceByName },
            { ApiRoutePathConstants.GetAllIdentityResources, GetAllIdentityResources },
            { ApiRoutePathConstants.AddIdentityResourceClaim, AddIdentityResourceClaim },
            {
                ApiRoutePathConstants.DeleteIdentityResourceClaimByResourceId,
                DeleteIdentityResourceClaimByResourceIdAsync
            },
            { ApiRoutePathConstants.DeleteIdentityResourceClaimById, DeleteIdentityResourceClaimByIdAsync },
            { ApiRoutePathConstants.DeleteIdentityResourceClaimModel, DeleteIdentityResourceClaimModel },
            { ApiRoutePathConstants.GetIdentityResourceClaims, GetIdentityResourceClaims },

            // User account service
            { ApiRoutePathConstants.LockUser, LockUser },
            { ApiRoutePathConstants.LockUserWithEndDatePath, LockUserWithEndDate },
            { ApiRoutePathConstants.AddAdminClaim, AddAdminClaim },
            { ApiRoutePathConstants.AddAdminClaimList, AddAdminClaimList },
            { ApiRoutePathConstants.AddClaim, AddClaim },
            { ApiRoutePathConstants.AddClaimList, AddClaimList },
            { ApiRoutePathConstants.AddSecurityQuestion, AddSecurityQuestion },
            { ApiRoutePathConstants.AddUserRole, AddUserRole },
            { ApiRoutePathConstants.AddUserRolesList, AddUserRolesList },
            { ApiRoutePathConstants.AddUserSecurityQuestion, AddUserSecurityQuestion },
            { ApiRoutePathConstants.AddUserSecurityQuestionList, AddUserSecurityQuestionList },
            { ApiRoutePathConstants.ChangePassword, ChangePassword },
            { ApiRoutePathConstants.DeleteSecurityQuestion, DeleteSecurityQuestion },
            { ApiRoutePathConstants.DeleteUserById, DeleteUserById },
            { ApiRoutePathConstants.DeleteUserByName, DeleteUserByName },
            { ApiRoutePathConstants.DeleteUserSecurityQuestion, DeleteUserSecurityQuestion },
            { ApiRoutePathConstants.DeleteUserSecurityQuestionList, DeleteUserSecurityQuestionList },
            { ApiRoutePathConstants.GenerateEmailConfirmationToken, GenerateEmailConfirmationToken },
            { ApiRoutePathConstants.GeneratePhoneNumberConfirmationToken, GeneratePhoneNumberConfirmationToken },
            { ApiRoutePathConstants.GenerateEmailTwoFactorToken, GenerateEmailTwoFactorToken },
            { ApiRoutePathConstants.GeneratePasswordResetToken, GeneratePasswordResetToken },
            { ApiRoutePathConstants.GenerateSmsTwoFactorToken, GenerateSmsTwoFactorToken },
            { ApiRoutePathConstants.GenerateUserTokenAsync, GenerateUserTokenAsync },
            { ApiRoutePathConstants.GetAllSecurityQuestions, GetAllSecurityQuestions },
            { ApiRoutePathConstants.GetAllTwoFactorType, GetAllTwoFactorType },
            { ApiRoutePathConstants.GetClaims, GetClaims },
            { ApiRoutePathConstants.GetUserByEmail, GetUserByEmail },
            { ApiRoutePathConstants.GetUserById, GetUserById },
            { ApiRoutePathConstants.GetAllUsers, GetAllUsers },
            { ApiRoutePathConstants.GetUserByName, GetUserByName },
            { ApiRoutePathConstants.GetUserClaims, GetUserClaims },
            { ApiRoutePathConstants.GetAdminUserClaims, GetAdminUserClaims },
            { ApiRoutePathConstants.GetUserRoleClaimsById, GetUserRoleClaimsById },
            { ApiRoutePathConstants.GetUserRoleClaimsByName, GetUserRoleClaimsByName },
            { ApiRoutePathConstants.GetUserRoles, GetUserRoles },
            { ApiRoutePathConstants.GetUserSecurityQuestions, GetUserSecurityQuestions },
            { ApiRoutePathConstants.GetUsersForClaim, GetUsersForClaim },
            { ApiRoutePathConstants.GetUsersInRole, GetUsersInRole },
            { ApiRoutePathConstants.IsUserExistsByClaimPrincipal, IsUserExistsByClaimPrincipal },
            { ApiRoutePathConstants.IsUserExistsById, IsUserExistsById },
            { ApiRoutePathConstants.IsUserExistsByName, IsUserExistsByName },
            { ApiRoutePathConstants.RegisterUser, RegisterUser },
            { ApiRoutePathConstants.RemoveAdminClaim, RemoveAdminClaim },
            { ApiRoutePathConstants.RemoveAdminClaimList, RemoveAdminClaimList },
            { ApiRoutePathConstants.RemoveClaim, RemoveClaim },
            { ApiRoutePathConstants.RemoveClaimList, RemoveClaimList },
            { ApiRoutePathConstants.RemoveUserRole, RemoveUserRole },
            { ApiRoutePathConstants.RemoveUserRoleList, RemoveUserRoleList },
            { ApiRoutePathConstants.ReplaceClaim, ReplaceClaim }, // To do check
            { ApiRoutePathConstants.ResetPassword, ResetPassword },
            { ApiRoutePathConstants.SetTwoFactorEnabled, SetTwoFactorEnabled },
            { ApiRoutePathConstants.UnLockUser, UnLockUser },
            { ApiRoutePathConstants.UnLockUserByToken, UnLockUserByToken },
            { ApiRoutePathConstants.UnLockUserByuserSecurityQuestions, UnLockUserByuserSecurityQuestions },
            { ApiRoutePathConstants.UpdateSecurityQuestion, UpdateSecurityQuestion },
            { ApiRoutePathConstants.UpdateUser, UpdateUser },
            { ApiRoutePathConstants.UpdateUserSecurityQuestion, UpdateUserSecurityQuestion },
            { ApiRoutePathConstants.UpdateUserTwoFactorType, UpdateUserTwoFactorType },
            { ApiRoutePathConstants.VerifyEmailConfirmationToken, VerifyEmailConfirmationToken },
            { ApiRoutePathConstants.VerifyEmailTwoFactorToken, VerifyEmailTwoFactorToken },
            { ApiRoutePathConstants.VerifyPhoneNumberConfirmationToken, VerifyPhoneNumberConfirmationToken },
            { ApiRoutePathConstants.VerifySmsTwoFactorToken, VerifySmsTwoFactorToken },
            { ApiRoutePathConstants.VerifyUserToken, VerifyUserToken },

            // Role Services
            { ApiRoutePathConstants.CreateRole, CreateRole },
            { ApiRoutePathConstants.UpdateRole, UpdateRoleAsync },
            { ApiRoutePathConstants.DeleteRoleById, DeleteRoleById },
            { ApiRoutePathConstants.DeleteRoleByName, DeleteRoleByName },
            { ApiRoutePathConstants.GetRoleById, GetRoleById },
            { ApiRoutePathConstants.GetRoleByName, GetRoleByName },
            { ApiRoutePathConstants.GetAllRoles, GetAllRoles },
            { ApiRoutePathConstants.AddRoleClaim, AddRoleClaim },
            { ApiRoutePathConstants.AddRoleClaimList, AddRoleClaimList },
            { ApiRoutePathConstants.RemoveRoleClaimsById, RemoveRoleClaimById },
            { ApiRoutePathConstants.RemoveRoleClaim, RemoveRoleClaim },
            { ApiRoutePathConstants.RemoveRoleClaimsList, RemoveRoleClaimsList },
            { ApiRoutePathConstants.GetRoleClaim, GetRoleClaim },

            // Authentication Services
            { ApiRoutePathConstants.GenerateRecoveryCodes, GenerateRecoveryCodes },
            { ApiRoutePathConstants.IsUserSignedIn, IsUserSignedIn },
            { ApiRoutePathConstants.PasswordSignIn, PasswordSignIn },
            {
                ApiRoutePathConstants.PasswordSignInByTwoFactorAuthenticatorToken,
                PasswordSignInByTwoFactorAuthenticatorToken
            },
            { ApiRoutePathConstants.ResetAuthenticatorApp, ResetAuthenticatorApp },
            { ApiRoutePathConstants.RopValidateCredentials, RopValidateCredentials },
            { ApiRoutePathConstants.SetupAuthenticatorApp, SetupAuthenticatorApp },
            { ApiRoutePathConstants.SignOut, SignOut },
            { ApiRoutePathConstants.TwoFactorAuthenticatorAppSignIn, TwoFactorAuthenticatorAppSignIn },
            { ApiRoutePathConstants.TwoFactorEmailSignIn, TwoFactorEmailSignIn },
            { ApiRoutePathConstants.TwoFactorRecoveryCodeSignIn, TwoFactorRecoveryCodeSignIn },
            { ApiRoutePathConstants.TwoFactorSmsSignInAsync, TwoFactorSmsSignInAsync },
            { ApiRoutePathConstants.VerifyAuthenticatorAppSetup, VerifyAuthenticatorAppSetup },
            { ApiRoutePathConstants.CountRecoveryCodesAsync, CountRecoveryCodesAsync },

            // Client Service
            { ApiRoutePathConstants.DeleteClient, DeleteClient },
            { ApiRoutePathConstants.GenerateClientSecret, GenerateClientSecret },
            { ApiRoutePathConstants.GetAllClient, GetAllClient },
            { ApiRoutePathConstants.GetClient, GetClient },
            { ApiRoutePathConstants.RegisterClient, RegisterClient },
            { ApiRoutePathConstants.UpdateClient, UpdateClient },

            // Autdit Service
            { ApiRoutePathConstants.AddAuditTrail, AddAuditTrail },
            { ApiRoutePathConstants.AddAuditTrailModel, AddAuditTrailModel },
            { ApiRoutePathConstants.GetAuditDetails, GetAuditDetailsAsync },
            //{ ApiRoutePathConstants.GetAuditDetailsByCreatedOn, GetAuditDetailsByCreatedOn },
            //{ ApiRoutePathConstants.GetAuditDetailsByFromDate, GetAuditDetailsByFromDate },
            //{ ApiRoutePathConstants.GetAuditDetailsByActionType, GetAuditDetailsByActionType },

            // Security Token
            { ApiRoutePathConstants.GetActiveSecurityTokensByClientIds, GetActiveSecurityTokensByClientIds },
            { ApiRoutePathConstants.GetActiveSecurityTokensByUserIds, GetActiveSecurityTokensByUserIds },
            { ApiRoutePathConstants.GetActiveSecurityTokensBetweenDates, GetActiveSecurityTokensBetweenDates },
            { ApiRoutePathConstants.GetAllSecurityTokensBetweenDates, GetAllSecurityTokensBetweenDates },

            // Notification Management
            { ApiRoutePathConstants.GetNotificationLogs, GetNotificationLogs },
            { ApiRoutePathConstants.GetNotificationTemplates, GetNotificationTemplates },
            { ApiRoutePathConstants.GetProviderConfig, GetProviderConfig },
            { ApiRoutePathConstants.GetAllProviderConfigs, GetAllProviderConfigs },
            { ApiRoutePathConstants.SaveProviderConfig, SaveProviderConfig },
            { ApiRoutePathConstants.SetActiveProvider, SetActiveProvider },
            { ApiRoutePathConstants.DeleteProviderConfig, DeleteProviderConfig },
            { ApiRoutePathConstants.GetProviderFieldDefinitions, GetProviderFieldDefinitions },
            { ApiRoutePathConstants.SendTestNotification, SendTestNotification },

            // External Auth Management
            { ApiRoutePathConstants.GetAllExternalAuthProviders, GetAllExternalAuthProviders },
            { ApiRoutePathConstants.GetExternalAuthProvider, GetExternalAuthProvider },
            { ApiRoutePathConstants.SaveExternalAuthProvider, SaveExternalAuthProvider },
            { ApiRoutePathConstants.DeleteExternalAuthProvider, DeleteExternalAuthProvider },
            { ApiRoutePathConstants.TestExternalAuthProvider, TestExternalAuthProvider },
            { ApiRoutePathConstants.GetExternalAuthFieldDefinitions, GetExternalAuthFieldDefinitions }
        };
    }
}
