/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Http;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Results;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Results;

/// <summary>
/// Endpoint result that handles the OIDC end-session (logout) redirect.
/// Constructs a logout message, protects it with data protection, and redirects
/// the user to the configured logout page with the logout ID.
/// </summary>
internal class EndSessionResult : IEndpointResult
{
    private readonly ValidatedEndSessionRequestModel validatedRequest;

    /// <summary>
    /// Initializes a new instance of the <see cref="EndSessionResult"/> class.
    /// </summary>
    public EndSessionResult(ValidatedEndSessionRequestModel validatedRequest)
    {
        this.validatedRequest = validatedRequest;
    }

    /// <summary>
    /// Constructs the response.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>The operation result.</returns>
    public async Task ConstructResponseAsync(HttpContext context)
    {
        string logoutId = null;
        var request = validatedRequest.IsError ? null : validatedRequest;
        if (request != null)
        {
            var logoutMessage = await PrepareLogoutMessage(validatedRequest);
                if (logoutMessage.HasClient) logoutId = await logoutMessage.ProtectDataAsync();
        }

        var userInteractionConfig = validatedRequest.TokenConfigOptions?.UserInteractionConfig;

        // Always redirect through the internal logout page (e.g. /account/logout) when
        // logoutId is available. This ensures the session cookie is cleared via SignOutAsync
        // before the user is redirected to the client's post_logout_redirect_uri.
        // Previously, when PostLogOutUri was set, the redirect went directly to the client
        // without clearing HCL.CS.SF's auth cookie, causing subsequent logins to fail.
        string redirect;
        if (!string.IsNullOrWhiteSpace(logoutId) && !string.IsNullOrWhiteSpace(userInteractionConfig?.LogoutUrl))
        {
            redirect = userInteractionConfig.LogoutUrl;
        }
        else
        {
            redirect = validatedRequest.PostLogOutUri;
            if (string.IsNullOrWhiteSpace(redirect)) redirect = userInteractionConfig?.LogoutUrl;
            if (string.IsNullOrWhiteSpace(redirect)) redirect = "/";
        }

        if (redirect.IsLocalUrl()) redirect = context.GetHCLCSSFRelativePath(redirect);

        if (!string.IsNullOrWhiteSpace(logoutId) && !string.IsNullOrWhiteSpace(userInteractionConfig?.LogoutIdParameter))
            redirect = redirect.AddQueryString(
                userInteractionConfig.LogoutIdParameter, logoutId);

        context.Response.Redirect(redirect);
    }

    /// <summary>
    /// Constructs a <see cref="LogoutMessageModel"/> from the validated end-session request.
    /// Includes client ID, subject, session, client list, and post-logout redirect URI with state.
    /// </summary>
    /// <param name="request">The validated end-session request.</param>
    /// <returns>A logout message model for data protection and serialization.</returns>
    internal static Task<LogoutMessageModel> PrepareLogoutMessage(ValidatedEndSessionRequestModel request)
    {
        var logoutMessage = new LogoutMessageModel();
        if (request != null)
        {
            if (request.RequestRawData != null) logoutMessage.Parameters = request.RequestRawData;

            logoutMessage.ClientId = request.Client?.ClientId;
            logoutMessage.SubjectId = request.SubjectId;
            logoutMessage.SessionId = request.SessionId;
            logoutMessage.ClientIdCollection = request.ClientIds;

            if (request.PostLogOutUri != null)
            {
                logoutMessage.PostLogoutRedirectUri = request.PostLogOutUri;
                if (request.State != null)
                    logoutMessage.PostLogoutRedirectUri =
                        logoutMessage.PostLogoutRedirectUri.AddQueryString(OpenIdConstants.EndSessionRequest.State,
                            request.State);
            }
        }

        return Task.FromResult(logoutMessage);
    }
}
