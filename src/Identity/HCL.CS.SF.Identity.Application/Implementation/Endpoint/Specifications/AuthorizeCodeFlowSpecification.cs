/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using DomainValidation.Interfaces.Specification;
using DomainValidation.Validation;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.DomainServices.Wrappers;
using HCL.CS.SF.Service.Extension;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Implementation.Endpoint.Validators;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Specifications;

/// <summary>
/// Validates the token request for the OAuth 2.0 Authorization Code grant type.
/// Checks client authorization, retrieves and validates the authorization code (one-time use),
/// verifies client binding, code expiry, redirect_uri match, scope presence,
/// PKCE code_verifier, user existence, and user role mapping.
/// </summary>
internal sealed class AuthorizationCodeFlowSpecification : BaseRequestModelValidator<ValidatedTokenRequestModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationCodeFlowSpecification"/> class.
    /// </summary>
    internal AuthorizationCodeFlowSpecification(IAuthorizationService authorizationService,
        UserManagerWrapper<Users> userManager)
    {
        Add("CheckClientAuthorizedForGrantType", new Rule<ValidatedTokenRequestModel>(
            new CheckClientAuthorizedForGrantType<ValidatedTokenRequestModel>(new List<string>
            {
                AuthenticationConstants.GrantType.AuthorizationCode
            }),
            OpenIdConstants.Errors.UnauthorizedClient,
            EndpointErrorCodes.ClientNotAuthorizedForGrantType));

        Add("CheckAuthorizationCodeNull", new Rule<ValidatedTokenRequestModel>(
            new IsRequestNull<ValidatedTokenRequestModel>(request =>
                request.GetValue(OpenIdConstants.TokenRequest.Code)),
            OpenIdConstants.Errors.InvalidGrant,
            EndpointErrorCodes.AuthorizationCodeMissing));

        Add("CheckAuthorizationCodeLength", new Rule<ValidatedTokenRequestModel>(
            new CheckLengthRestrictions<ValidatedTokenRequestModel>(
                request => request.GetValue(OpenIdConstants.TokenRequest.Code),
                request => request.TokenConfigOptions.InputLengthRestrictionsConfig.AuthorizationCode,
                request => ">"),
            OpenIdConstants.Errors.InvalidGrant,
            EndpointErrorCodes.AuthorizationCodeTooLong));

        Add("CheckAuthorizationCode", new Rule<ValidatedTokenRequestModel>(
            new CheckAuthorizationCode(authorizationService),
            OpenIdConstants.Errors.InvalidGrant,
            EndpointErrorCodes.InvalidAuthorizationCode));

        Add("CheckClientBinding", new Rule<ValidatedTokenRequestModel>(
            new CheckClientBinding(),
            OpenIdConstants.Errors.InvalidGrant,
            EndpointErrorCodes.InvalidClientBinding));

        Add("CheckAuthorizationCodeExpiry", new Rule<ValidatedTokenRequestModel>(
            new CheckAuthorizationCodeExpiry(),
            OpenIdConstants.Errors.InvalidGrant,
            EndpointErrorCodes.AuthorizationCodeExpired));

        Add("CheckRedirectUri", new Rule<ValidatedTokenRequestModel>(
            new IsRequestNull<ValidatedTokenRequestModel>(request =>
                request.GetValue(OpenIdConstants.TokenRequest.RedirectUri)),
            OpenIdConstants.Errors.UnauthorizedClient,
            EndpointErrorCodes.RedirectUriMissing));

        Add("CompareRedirectUriFromAuthCode", new Rule<ValidatedTokenRequestModel>(
            new CompareRedirectUriFromAuthCode(),
            OpenIdConstants.Errors.InvalidGrant,
            EndpointErrorCodes.InvalidRedirectUri));

        Add("CheckScopesPresence", new Rule<ValidatedTokenRequestModel>(
            new CheckScopesPresence(),
            OpenIdConstants.Errors.InvalidRequest,
            EndpointErrorCodes.AuthorizationCodeScopeError));

        Add("CheckCodeVerifier", new Rule<ValidatedTokenRequestModel>(
            new CheckCodeVerifier(),
            OpenIdConstants.Errors.InvalidGrant,
            EndpointErrorCodes.InvalidCodeVerifier));

        Add("CheckUserExists", new Rule<ValidatedTokenRequestModel>(
            new CheckUserExistsByClaimsPrincipal(userManager),
            OpenIdConstants.Errors.InvalidGrant,
            EndpointErrorCodes.UserDoesNotExist));

        Add("CheckUserRoleMapped", new Rule<ValidatedTokenRequestModel>(
            new CheckUserRoleMapped(userManager),
            OpenIdConstants.Errors.InvalidRequest,
            EndpointErrorCodes.NoUserRoleMapped));
    }
}

/// <summary>
/// Specification that retrieves and validates the authorization code from persistent storage.
/// Uses one-time consumption semantics per RFC 6749 Section 4.1.2.
/// </summary>
internal class CheckAuthorizationCode : ISpecification<ValidatedTokenRequestModel>
{
    private readonly IAuthorizationService authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckAuthorizationCode"/> class.
    /// </summary>
    internal CheckAuthorizationCode(IAuthorizationService authorizationService)
    {
        this.authorizationService = authorizationService;
    }

    /// <summary>
    /// Retrieves the authorization code and populates the model with the deserialized code data.
    /// </summary>
    /// <param name="model">The token request model containing the authorization code parameter.</param>
    /// <returns>True if the code is valid and was successfully retrieved; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedTokenRequestModel model)
    {
        var authorizationCode = model.GetValue(OpenIdConstants.TokenRequest.Code);
        var authCodeModel = authorizationService.GetAuthorizationCodeAsync(authorizationCode).GetAwaiter().GetResult();
        if (authCodeModel == null) return false;

        model.AuthorizationCode = authCodeModel;
        return true;
    }
}

/// <summary>
/// Specification that verifies the authorization code was issued to the same client making the token request.
/// Prevents authorization code interception attacks per RFC 6749 Section 4.1.3.
/// </summary>
internal class CheckClientBinding : ISpecification<ValidatedTokenRequestModel>
{
    /// <summary>
    /// Compares the client_id from the authorization code with the authenticated client.
    /// </summary>
    /// <param name="model">The token request model.</param>
    /// <returns>True if the client IDs match; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedTokenRequestModel model)
    {
        if (model.AuthorizationCode.ClientId != model.Client.ClientId) return false;

        return true;
    }
}

/// <summary>
/// Specification that checks whether the authorization code has expired.
/// Authorization codes have a maximum lifetime (capped at 600 seconds per security best practices).
/// Also populates the model with session ID, subject, nonce, and state from the code.
/// </summary>
internal class CheckAuthorizationCodeExpiry : ISpecification<ValidatedTokenRequestModel>
{
    /// <summary>
    /// Validates the code is not expired and populates session/subject data from the code.
    /// </summary>
    /// <param name="model">The token request model.</param>
    /// <returns>True if the code is still valid; false if expired.</returns>
    public bool IsSatisfiedBy(ValidatedTokenRequestModel model)
    {
        if (model.AuthorizationCode.CreationTime.IsExpired(model.AuthorizationCode.Lifetime)) return false;

        // validate code expiration
        if (model.AuthorizationCode.CreationTime.IsExpired(model.Client.AuthorizationCodeExpiration)) return false;

        // populate Session id
        if (!string.IsNullOrWhiteSpace(model.AuthorizationCode.SessionId))
            model.SessionId = model.AuthorizationCode.SessionId;

        model.Subject = model.AuthorizationCode.Subject;
        model.Nonce = model.AuthorizationCode.Nonce;
        model.State = model.AuthorizationCode.State;
        return true;
    }
}

/// <summary>
/// Specification that verifies the redirect_uri in the token request matches the one used in the
/// original authorization request per RFC 6749 Section 4.1.3.
/// </summary>
internal class CompareRedirectUriFromAuthCode : ISpecification<ValidatedTokenRequestModel>
{
    /// <summary>
    /// Compares the redirect_uri from the token request against the one stored in the authorization code.
    /// </summary>
    /// <param name="model">The token request model.</param>
    /// <returns>True if the URIs match; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedTokenRequestModel model)
    {
        var redirectUri = model.GetValue(OpenIdConstants.TokenRequest.RedirectUri);
        if (redirectUri != null &&
            !redirectUri.Equals(model.AuthorizationCode.RedirectUri, StringComparison.Ordinal)) return false;

        return true;
    }
}

/// <summary>
/// Specification that ensures the authorization code has scope information attached.
/// Transfers the scope parser model and token details from the code to the token request.
/// </summary>
internal class CheckScopesPresence : ISpecification<ValidatedTokenRequestModel>
{
    /// <summary>
    /// Validates scopes are present in the authorization code and copies them to the request model.
    /// </summary>
    /// <param name="model">The token request model.</param>
    /// <returns>True if scopes are present; false if the scope parser model is null.</returns>
    public bool IsSatisfiedBy(ValidatedTokenRequestModel model)
    {
        // validate scopes are present
        if (model.AuthorizationCode.AllowedScopesParserModel == null) return false;

        model.AllowedScopesParserModel = model.AuthorizationCode.AllowedScopesParserModel;
        model.TokenDetails = model.AuthorizationCode.AllowedScopesParserModel.TokenDetails;
        return true;
    }
}

/// <summary>
/// Specification that ensures the PKCE code_verifier is present when the client requires PKCE.
/// </summary>
internal class CheckCodeVerifier : ISpecification<ValidatedTokenRequestModel>
{
    /// <summary>
    /// Validates code_verifier presence for PKCE-required clients and stores it on the model.
    /// </summary>
    /// <param name="model">The token request model.</param>
    /// <returns>True if code_verifier is present or not required; false if missing when required.</returns>
    public bool IsSatisfiedBy(ValidatedTokenRequestModel model)
    {
        var codeVerifier = model.GetValue(OpenIdConstants.TokenRequest.CodeVerifier);
        if (model.Client.RequirePkce)
        {
            if (string.IsNullOrWhiteSpace(codeVerifier)) return false;

            model.CodeVerifier = model.GetValue(OpenIdConstants.TokenRequest.CodeVerifier);
        }

        return true;
    }
}

/// <summary>
/// Specification that verifies the user referenced in the authorization code's ClaimsPrincipal
/// still exists in the identity store. Prevents token issuance for deleted users.
/// </summary>
internal class CheckUserExistsByClaimsPrincipal : ISpecification<ValidatedTokenRequestModel>
{
    private readonly UserManagerWrapper<Users> userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckUserExistsByClaimsPrincipal"/> class.
    /// </summary>
    internal CheckUserExistsByClaimsPrincipal(UserManagerWrapper<Users> userManager)
    {
        this.userManager = userManager;
    }

    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(ValidatedTokenRequestModel model)
    {
        // make sure user is enabled
        Users user = null;
        var id = model.AuthorizationCode.Subject.Identity as ClaimsIdentity;
        if (id != null)
        {
            var claim = id.FindFirst(OpenIdConstants.ClaimTypes.Sub);
            if (claim != null)
            {
                if (claim.Value.IsGuid()) user = userManager.FindByIdAsync(claim.Value).GetAwaiter().GetResult();

                if (user == null) user = userManager.FindByNameAsync(claim.Value).GetAwaiter().GetResult();
            }
        }

        if (user == null) return false;

        return true;
    }
}

/// <summary>
/// Specification that verifies the user has at least one role assigned.
/// Rejects token requests for users without any role mapping.
/// </summary>
internal class CheckUserRoleMapped : ISpecification<ValidatedTokenRequestModel>
{
    private readonly UserManagerWrapper<Users> userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckUserRoleMapped"/> class.
    /// </summary>
    internal CheckUserRoleMapped(UserManagerWrapper<Users> userManager)
    {
        this.userManager = userManager;
    }

    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(ValidatedTokenRequestModel model)
    {
        var id = model.AuthorizationCode.Subject.Identity as ClaimsIdentity;
        var claim = id.FindFirst(OpenIdConstants.ClaimTypes.Sub);
        if (claim != null && claim.Value.IsGuid())
        {
            var user = userManager.FindByIdAsync(claim.Value).GetAwaiter().GetResult();
            if (user != null)
            {
                // make sure user is enabled
                var userRoles = (List<string>)userManager.GetRolesAsync(user).GetAwaiter().GetResult();
                if (!userRoles.ContainsAny()) return false;
            }
        }

        return true;
    }
}
