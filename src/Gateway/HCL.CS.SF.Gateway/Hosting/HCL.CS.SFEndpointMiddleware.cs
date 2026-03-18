/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Results;

namespace HCL.CS.SF.ProxyService.Hosting;

/// <summary>
/// HTTP pipeline middleware that handles OAuth 2.0 / OpenID Connect protocol endpoints
/// (token, authorize, end-session, introspection, revocation, userinfo, discovery, JWKS).
/// Matches the incoming request path against registered endpoint models, checks whether
/// the endpoint is enabled in configuration, resolves the endpoint handler from DI,
/// and executes it. Also sets the framework base path and extracts the logged-in user
/// from the bearer token for logging context.
/// </summary>
public class HCLCSSFEndpointMiddleware
{
    /// <summary>
    /// Logger service for recording endpoint processing events and errors.
    /// </summary>
    private readonly ILoggerService loggerService;

    /// <summary>
    /// The next middleware delegate in the HTTP pipeline.
    /// </summary>
    private readonly RequestDelegate next;

    /// <summary>
    /// Token configuration settings that control which endpoints are enabled.
    /// </summary>
    private readonly TokenSettings tokenSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="HCLCSSFEndpointMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="instance">Logger instance factory for obtaining the default logger.</param>
    /// <param name="configSettings">The security framework configuration containing token settings.</param>
    public HCLCSSFEndpointMiddleware(RequestDelegate next, ILoggerInstance instance, HCLCSSFConfig configSettings)
    {
        this.next = next;
        tokenSettings = configSettings.TokenSettings;
        loggerService = instance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
    }

    /// <summary>
    /// Processes the HTTP request by setting framework context values, matching the request
    /// path to a registered OIDC endpoint, and invoking the matched endpoint handler.
    /// If no endpoint matches, the request continues down the pipeline.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <param name="endpoints">The collection of registered OIDC endpoint models.</param>
    /// <param name="session">The session management service for session tracking.</param>
    public async Task InvokeAsync(HttpContext httpContext, IEnumerable<SecurityEndpointModel> endpoints,
        ISessionManagementService session)
    {
        // Store the base path and extract the authenticated user for logging context.
        SetHCLCSSFBasePath(httpContext);
        SetHCLCSSFLoggedUser(httpContext);

        try
        {
            // Attempt to find a matching endpoint handler for the current request path.
            var endpoint = Find(httpContext, endpoints);
            if (endpoint != null)
            {
                loggerService.WriteTo(Log.Debug, "Invoking endpoint: {endpointType} for {url}",
                    endpoint.GetType().FullName, httpContext.Request.Path.ToString());
                var result = await endpoint.ProcessAsync(httpContext);
                if (result != null) await result.ConstructResponseAsync(httpContext);

                return;
            }
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Exception while processing endpoint middleware request.");
            throw;
        }

        await next(httpContext);
    }

    /// <summary>
    /// Iterates through registered endpoint models to find one whose path matches the
    /// current request. Checks whether the matched endpoint is enabled in configuration
    /// before returning a handler.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="endpoints">The registered OIDC endpoint models.</param>
    /// <returns>The endpoint handler, or null if no match or the endpoint is disabled.</returns>
    private IEndpoint Find(HttpContext context, IEnumerable<SecurityEndpointModel> endpoints)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        foreach (var endpoint in endpoints)
        {
            var path = endpoint.Path;

            if (context.Request.Path.Equals(path, StringComparison.OrdinalIgnoreCase))
            {
                // Verify the endpoint is enabled in token configuration.
                var permission = GetPermissionForEndpoint(context.Request.Path);
                if (permission) return GetEndpointHandler(endpoint, context);

                return null;
            }
        }

        return null;
    }

    /// <summary>
    /// Checks whether the specified OIDC endpoint path is enabled in the token settings
    /// configuration. Unknown paths default to enabled.
    /// </summary>
    /// <param name="path">The endpoint path to check.</param>
    /// <returns><c>true</c> if the endpoint is enabled; otherwise, <c>false</c>.</returns>
    private bool GetPermissionForEndpoint(string path)
    {
        return path switch
        {
            OpenIdConstants.EndpointRoutePaths.Token => tokenSettings.EndpointsConfig.EnableTokenEndpoint,
            OpenIdConstants.EndpointRoutePaths.Authorize => tokenSettings.EndpointsConfig.EnableAuthorizeEndpoint,
            OpenIdConstants.EndpointRoutePaths.EndSession => tokenSettings.EndpointsConfig.EnableEndSessionEndpoint,
            OpenIdConstants.EndpointRoutePaths.Introspection => tokenSettings.EndpointsConfig
                .EnableIntrospectionEndpoint,
            OpenIdConstants.EndpointRoutePaths.Revocation =>
                tokenSettings.EndpointsConfig.EnableTokenRevocationEndpoint,
            OpenIdConstants.EndpointRoutePaths.UserInfo => tokenSettings.EndpointsConfig.EnableUserInfoEndpoint,
            _ => true
        };
    }

    /// <summary>
    /// Resolves the endpoint handler from the DI container using the handler type
    /// registered on the endpoint model.
    /// </summary>
    /// <param name="endpoint">The endpoint model containing the handler type.</param>
    /// <param name="context">The current HTTP context providing access to the DI container.</param>
    /// <returns>The resolved endpoint handler, or null if the type is not registered.</returns>
    private IEndpoint GetEndpointHandler(SecurityEndpointModel endpoint, HttpContext context)
    {
        if (context.RequestServices.GetService(endpoint.Handler) is IEndpoint handler) return handler;

        return null;
    }

    /// <summary>
    /// Stores the request's base path (without trailing slash) in <see cref="HttpContext.Items"/>
    /// so that downstream components can construct absolute URIs for OIDC responses.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    private void SetHCLCSSFBasePath(HttpContext httpContext)
    {
        var path = httpContext.Request.PathBase.Value;
        if (path != null && path.EndsWith("/")) path = path[..^1];

        httpContext.Items[AuthenticationConstants.EnvironmentPaths.HCLCSSFBasePath] = path;
    }

    /// <summary>
    /// Extracts the "sub" claim from the bearer token in the Authorization header
    /// and sets it as the logged user name on the logger service for correlation
    /// in subsequent log entries. Silently ignores malformed or missing tokens.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    private void SetHCLCSSFLoggedUser(HttpContext httpContext)
    {
        var authorizationHeader = httpContext.Request?.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authorizationHeader)) return;

        var header = authorizationHeader.Trim();
        if (!header.StartsWith(OpenIdConstants.AuthenticationSchemes.AuthorizationHeaderBearer,
                StringComparison.OrdinalIgnoreCase))
            return;

        // Strip the "Bearer " prefix to get the raw token value.
        var tokenValue = header[OpenIdConstants.AuthenticationSchemes.AuthorizationHeaderBearer.Length..].Trim();
        if (string.IsNullOrWhiteSpace(tokenValue)) return;

        var tokenHandler = new JwtSecurityTokenHandler();
        if (!tokenHandler.CanReadToken(tokenValue)) return;

        try
        {
            var parsedToken = tokenHandler.ReadJwtToken(tokenValue);
            var user = parsedToken.Claims.FirstOrDefault(x => x.Type == OpenIdConstants.ClaimTypes.Sub)?.Value;
            if (!string.IsNullOrWhiteSpace(user)) loggerService.SetLoggedUserName(user);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Debug, ex, "Unable to parse bearer token for logging context.");
        }
    }
}
