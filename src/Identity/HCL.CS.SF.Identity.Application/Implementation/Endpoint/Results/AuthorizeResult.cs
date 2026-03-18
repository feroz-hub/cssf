/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Response;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Results;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Results;

/// <summary>
/// Endpoint result that renders the OAuth 2.0 authorization response.
/// Supports query string redirect (response_mode=query) and form_post (response_mode=form_post).
/// Handles both success responses (authorization code, tokens) and error responses
/// (redirect to error page or client redirect URI with error parameters).
/// </summary>
internal class AuthorizeResult : IEndpointResult
{
    private const string UnsupportedMode = "Unsupported response mode.";

    private const string FormPostHtml = "<html><head><meta http-equiv='X-UA-Compatible' content='IE=edge' />" +
                                        "<base target='_self'/></head><body><form method='post' action='{uri}'>{body}<noscript><button>" +
                                        "Click to continue</button></noscript></form><script>window.addEventListener" +
                                        "('load', function(){document.forms[0].submit();});</script></body></html>";

    private readonly ISessionManagementService userSession;

    private HCLCSSFConfig frameworkConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizeResult"/> class.
    /// </summary>
    public AuthorizeResult(AuthorizationResponseModel authResponse, ISessionManagementService userSession)
    {
        AuthorizeResponse = authResponse;
        this.userSession = userSession;
    }

    /// <summary>
    /// Gets or sets the authorize response.
    /// </summary>
    internal AuthorizationResponseModel AuthorizeResponse { get; }

    /// <summary>
    /// Constructs the response.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>The operation result.</returns>
    public async Task ConstructResponseAsync(HttpContext context)
    {
        frameworkConfig = context.RequestServices.GetRequiredService<HCLCSSFConfig>();
        if (AuthorizeResponse.IsError)
            await ProcessErrorResponseAsync(context);
        else
            await ProcessResponseAsync(context);
    }

    private static void AddSecurityHeaders(HttpContext context)
    {
        var headers = context.Response.Headers;
        var redirectOrigin = context.Items["__authorize_redirect_origin"] as string;
        var formAction = "'self'";
        if (!string.IsNullOrWhiteSpace(redirectOrigin)) formAction += " " + redirectOrigin;

        // response_mode=form_post must allow posting to the client redirect URI origin.
        var csp = "default-src 'none'; " +
                  "script-src 'unsafe-inline' 'sha256-orD0/VhH8hLqrLxKHD/HUEMdwqX6/0ve7c5hspX5VJ8='; " +
                  "style-src 'unsafe-inline'; " +
                  "img-src data:; " +
                  "base-uri 'none'; " +
                  "frame-ancestors 'none'; " +
                  $"form-action {formAction}";

        headers["Content-Security-Policy"] = csp;
        headers["X-Content-Security-Policy"] = csp;
        headers["X-Frame-Options"] = "DENY";
        headers["X-Content-Type-Options"] = "nosniff";
        headers["Referrer-Policy"] = "no-referrer";
    }

    private static Dictionary<string, string> ConstructResponseCollection(AuthorizationResponseModel response)
    {
        var collection = new Dictionary<string, string>();

        if (response.IsError)
        {
            if (!string.IsNullOrWhiteSpace(response.ErrorCode)) collection.Add("error", response.ErrorCode);

            if (!string.IsNullOrWhiteSpace(response.ErrorDescription))
                collection.Add("error_description", response.ErrorDescription);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(response.Code)) collection.Add("code", response.Code);

            if (!string.IsNullOrWhiteSpace(response.IdentityToken)) collection.Add("id_token", response.IdentityToken);

            if (!string.IsNullOrWhiteSpace(response.RefreshToken))
                collection.Add("refresh_token", response.RefreshToken);

            if (!string.IsNullOrWhiteSpace(response.AccessToken))
            {
                collection.Add("access_token", response.AccessToken);
                collection.Add("token_type", "Bearer");
                collection.Add("expires_in", response.AccessTokenLifetime.ToString());
            }

            if (!string.IsNullOrWhiteSpace(response.Scope)) collection.Add("scope", response.Scope);
        }

        if (!string.IsNullOrWhiteSpace(response.State)) collection.Add("state", response.State);

        if (!string.IsNullOrWhiteSpace(response.SessionState)) collection.Add("session_state", response.SessionState);

        collection.Add("IsError", response.IsError.ToString());
        return collection;
    }

    private async Task ProcessErrorResponseAsync(HttpContext context)
    {
        var isSafeError =
            AuthorizeResponse.ErrorCode == OpenIdConstants.Errors.AccessDenied ||
            AuthorizeResponse.ErrorCode == OpenIdConstants.Errors.AccountSelectionRequired ||
            AuthorizeResponse.ErrorCode == OpenIdConstants.Errors.LoginRequired ||
            AuthorizeResponse.ErrorCode == OpenIdConstants.Errors.ConsentRequired ||
            AuthorizeResponse.ErrorCode == OpenIdConstants.Errors.InteractionRequired;

        if (isSafeError)
            await ProcessResponseAsync(context);
        else
            await RedirectToErrorPageAsync(context);
    }

    private async Task ProcessResponseAsync(HttpContext context)
    {
        if (!AuthorizeResponse.IsError) await userSession.AddClientAsync(AuthorizeResponse.Request.ClientId);

        await RenderAuthorizeResponseAsync(context);
    }

    private async Task RenderAuthorizeResponseAsync(HttpContext context)
    {
        if (AuthorizeResponse.Request.ResponseMode == OpenIdConstants.ResponseModes.Query)
        {
            context.Response.SetResponseNoCache();
            context.Response.Redirect(BuildRedirectUri());
        }
        else if (AuthorizeResponse.Request.ResponseMode == OpenIdConstants.ResponseModes.FormPost)
        {
            context.Response.SetResponseNoCache();
            var redirectOrigin = AuthorizeResponse.Request.RedirectUri?.GetOrigin();
            if (!string.IsNullOrWhiteSpace(redirectOrigin))
                context.Items["__authorize_redirect_origin"] = redirectOrigin;
            AddSecurityHeaders(context);
            await context.Response.WriteHtmlResponseAsync(GetFormPostHtml());
        }
        else
        {
            throw new InvalidOperationException(UnsupportedMode);
        }
    }

    private string BuildRedirectUri()
    {
        var uri = AuthorizeResponse.RedirectUri;
        var responseCollection = ConstructResponseCollection(AuthorizeResponse);
        var query = responseCollection.PrepareQueryString();

        if (AuthorizeResponse.Request.ResponseMode == OpenIdConstants.ResponseModes.Query
            || AuthorizeResponse.Request.ResponseMode == OpenIdConstants.ResponseModes.FormPost)
            uri = uri.FormatQueryString(query);
        else
            uri = uri.FormatFragmentString(query);

        if (AuthorizeResponse.IsError && !uri.Contains("#"))
            // TODO to be analyse
            // The fragment "#_=_" MUST be appended to the error redirect URI.
            // This prevents the browser from reattaching the fragment from a previous URI to the new location URI.
            uri += "#_=_";

        return uri;
    }

    private string GetFormPostHtml()
    {
        var html = FormPostHtml;

        var url = AuthorizeResponse.Request.RedirectUri;
        url = HtmlEncoder.Default.Encode(url);
        html = html.Replace("{uri}", url);
        var responseCollection = ConstructResponseCollection(AuthorizeResponse);
        html = html.Replace("{body}", responseCollection.PrepareFormPostString());

        return html;
    }

    private async Task RedirectToErrorPageAsync(HttpContext context)
    {
        var errorModel = new AuthorizeErrorResponseModel
        {
            TraceId = context.TraceIdentifier,
            ErrorCode = AuthorizeResponse.ErrorCode,
            ErrorDescription = AuthorizeResponse.ErrorDescription,
            ClientId = AuthorizeResponse.Request?.ClientId
        };

        if (AuthorizeResponse.RedirectUri != null && AuthorizeResponse.Request?.ResponseMode != null)
        {
            // if we have a valid redirect uri, then include it to the error page
            errorModel.RedirectUri = BuildRedirectUri();
            errorModel.ResponseMode = AuthorizeResponse.Request.ResponseMode;
        }

        var host = context.GetHCLCSSFBaseUrl().IncludeEndSlash();
        var errorUrl = frameworkConfig.TokenSettings.UserInteractionConfig.ErrorUrl;
        errorModel.ErrorUri = host + errorUrl;
        var errorId = await errorModel.ProtectDataAsync();
        var url = errorUrl.AddQueryString(frameworkConfig.TokenSettings.UserInteractionConfig.ErrorIdParameter,
            errorId);
        context.Response.RedirectToAbsoluteUrl(url);
    }
}
