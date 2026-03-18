/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Services;

/// <summary>
/// Manages user authentication sessions for the OpenID Connect provider.
/// Handles session cookie creation/binding, session ID management, authenticated user principal retrieval,
/// and tracking of client IDs that have participated in the current session (for logout coordination).
/// </summary>
internal class SessionManagementService : SecurityBase, ISessionManagementService
{
    private const string ClientListKey = "client_list";
    private readonly TokenSettings configSettings;
    private readonly IFrameworkResultService frameworkResultService;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ILoggerService loggerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionManagementService"/> class.
    /// </summary>
    public SessionManagementService(
        IHttpContextAccessor httpContextAccessor,
        IFrameworkResultService frameworkResultService,
        IAuthenticationHandlerProvider authenticationHandler,
        IAuthenticationSchemeProvider authenticationScheme,
        ILoggerInstance instance,
        HCLCSSFConfig tokenSettings)
    {
        this.frameworkResultService = frameworkResultService;
        configSettings = tokenSettings.TokenSettings;
        this.httpContextAccessor = httpContextAccessor;
        AuthenticationHandler = authenticationHandler;
        AuthenticationScheme = authenticationScheme;
        loggerService = instance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
    }

    /// <summary>Gets or sets the authentication scheme provider for resolving the default scheme.</summary>
    public IAuthenticationSchemeProvider AuthenticationScheme { get; set; }

    /// <summary>Gets or sets the authentication handler provider for retrieving scheme-specific handlers.</summary>
    public IAuthenticationHandlerProvider AuthenticationHandler { get; set; }

    /// <summary>Gets or sets the cached authenticated user principal for the current request.</summary>
    public ClaimsPrincipal Principal { get; set; }

    /// <summary>Gets or sets the cached authentication properties (including session ID) for the current request.</summary>
    public AuthenticationProperties Properties { get; set; }

    /// <summary>
    /// Retrieves the authenticated user's ClaimsPrincipal from the current HTTP context's authentication handler.
    /// </summary>
    /// <returns>The authenticated user's ClaimsPrincipal, or null if not authenticated.</returns>
    public virtual async Task<ClaimsPrincipal> GetUserPrincipalFromContextAsync()
    {
        var authPrinciple = await GetAuthenticationAsync();
        return authPrinciple?.Principal;
    }

    /// <summary>
    /// Retrieves the authentication properties from the current HTTP context.
    /// </summary>
    /// <returns>The authentication properties, or null if not authenticated.</returns>
    public virtual async Task<AuthenticationProperties> GetPropertiesFromContextAsync()
    {
        var authProperties = await GetAuthenticationAsync();
        return authProperties?.Properties;
    }

    /// <summary>
    /// Retrieves the authenticated principal and properties from the HTTP context's authentication handler.
    /// Caches the result for subsequent calls within the same request.
    /// </summary>
    /// <returns>An <see cref="AuthenticationPropertiesModel"/> with principal and properties, or null if not authenticated.</returns>
    public virtual async Task<AuthenticationPropertiesModel> GetAuthenticationAsync()
    {
        if (Principal == null || Properties == null)
        {
            var authenticationScheme = await GetAuthenticationSchemeAsync();
            if (authenticationScheme == null)
                frameworkResultService.Throw(ApiErrorCodes.NoDefaultAuthenticateSchemeFound);

            var handler =
                await AuthenticationHandler.GetHandlerAsync(httpContextAccessor.HttpContext, authenticationScheme);
            if (handler == null) frameworkResultService.Throw(ApiErrorCodes.NoAuthenticationHandlerConfigured);

            var result = await handler.AuthenticateAsync();
            if (result != null && result.Succeeded)
            {
                Principal = result.Principal;
                Properties = result.Properties;
                return new AuthenticationPropertiesModel
                    { Principal = Principal, Properties = Properties, IsError = false };
            }
        }
        else
        {
            return new AuthenticationPropertiesModel
                { Principal = Principal, Properties = Properties, IsError = false };
        }

        return null;
    }

    /// <summary>
    /// Retrieves the name of the default authentication scheme configured for the application.
    /// </summary>
    /// <returns>The scheme name, or null if no default scheme is configured.</returns>
    public virtual async Task<string> GetAuthenticationSchemeAsync()
    {
        var httpContext = httpContextAccessor.HttpContext;
        var authenticationSchemeProvider =
            httpContext.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
        var defaultAuthenticationScheme = await authenticationSchemeProvider.GetDefaultAuthenticateSchemeAsync();
        if (defaultAuthenticationScheme == null) return null;

        return defaultAuthenticationScheme.Name;
    }

    /// <summary>
    /// Retrieves the current session ID from the authentication properties.
    /// </summary>
    /// <returns>The session identifier string, or null if no session exists.</returns>
    public async Task<string> GetSessionId()
    {
        var properties = await GetPropertiesFromContextAsync();
        return properties?.GetSessionId();
    }

    /// <summary>
    /// Creates or reuses a session ID and binds it to the authentication cookie.
    /// Generates a new session ID if the subject has changed or no session exists.
    /// </summary>
    /// <param name="principal">The authenticated user's claims principal.</param>
    /// <param name="properties">The authentication properties to store the session ID in.</param>
    public async Task CreateAndBindSessionCookieAsync(ClaimsPrincipal principal, AuthenticationProperties properties)
    {
        if (principal == null || properties == null) frameworkResultService.Throw(EndpointErrorCodes.ArgumentNullError);

        var principleSubjectId = principal.GetSubjectId();
        var user = await GetUserPrincipalFromContextAsync();
        var contextSubjectId = user != null ? user.GetSubjectId() : string.Empty;
        if (properties.GetSessionId() == null || contextSubjectId != principleSubjectId)
        {
            var sessionValue = AuthenticationConstants.KeySize24.RandomString();
            properties.SetSessionId(sessionValue);
        }

        Principal = principal;
        Properties = properties;
    }

    /// <summary>
    /// Adds a client ID to the session's client list. Used to track which OAuth clients
    /// have active sessions, enabling front-channel and back-channel logout notifications.
    /// </summary>
    /// <param name="clientId">The OAuth 2.0 client_id to add to the session.</param>
    public async Task AddClientAsync(string clientId)
    {
        if (clientId == null) frameworkResultService.Throw(EndpointErrorCodes.ArgumentNullError);

        var authenticationModel = await GetAuthenticationAsync();
        if (authenticationModel.Properties != null)
        {
            loggerService.WriteTo(Log.Debug, "Entered into add client : " + clientId);
            var clientList = GetClientCollection(authenticationModel.Properties);
            if (!clientList.Contains(clientId))
            {
                var collection = clientList.ToList();
                collection.Add(clientId);
                var value = EncryptionExtension.Encode(collection);
                if (value == null)
                    Properties.Items.Remove(ClientListKey);
                else
                    Properties.Items[ClientListKey] = value;
            }
        }

        await UpdateSessionCookie();
    }

    /// <summary>
    /// Retrieves the list of client IDs that have participated in the current session.
    /// Used during logout to notify all clients via front-channel or back-channel logout.
    /// </summary>
    /// <returns>A list of client IDs in the current session.</returns>
    public async Task<IList<string>> GetClientListAsync()
    {
        var authenticationModel = await GetAuthenticationAsync();

        try
        {
            if (authenticationModel != null) return GetClientCollection(authenticationModel.Properties);
        }
        catch (Exception)
        {
            RemoveClientList(Properties);
            await UpdateSessionCookie();
        }

        return new List<string>();
    }

    private static IList<string> GetClientCollection(AuthenticationProperties properties)
    {
        if (properties?.Items.ContainsKey(ClientListKey) == true)
        {
            var value = properties.Items[ClientListKey];
            return EncryptionExtension.DecodeList(value);
        }

        return new List<string>();
    }

    private static void RemoveClientList(AuthenticationProperties properties)
    {
        properties?.Items.Remove(ClientListKey);
    }

    private async Task UpdateSessionCookie()
    {
        var authenticationModel = await GetAuthenticationAsync();

        if (authenticationModel == null || (authenticationModel != null &&
                                            (authenticationModel.Principal == null ||
                                             authenticationModel.Properties == null)))
            frameworkResultService.Throw(EndpointErrorCodes.InvalidOperation);

        var scheme = await GetAuthenticationSchemeAsync();
        loggerService.WriteTo(Log.Debug, "Entered into update session cookie : " + scheme);
        await httpContextAccessor.HttpContext.SignInAsync(scheme, authenticationModel.Principal,
            authenticationModel.Properties);
    }
}
