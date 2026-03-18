/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Constants.Endpoint;

namespace HCL.CS.SF.Domain.Configurations.Endpoint;

/// <summary>
/// Configuration settings for user-facing interaction pages during OAuth/OIDC flows.
/// These URLs control where the authorization server redirects users for login, logout, and error display.
/// Bound from the "UserInteraction" section of the endpoint configuration.
/// </summary>
public class UserInteractionConfig
{
    /// <summary>
    /// Gets or sets the URL of the login page where unauthenticated users are redirected
    /// during an authorization request (e.g., "/account/login").
    /// </summary>
    public string LoginUrl { get; set; }

    /// <summary>
    /// Gets or sets the query string parameter name used to pass the return URL back to the login page.
    /// After successful authentication, the user is redirected to this URL to resume the OAuth flow.
    /// Defaults to the standard "returnUrl" parameter.
    /// </summary>
    public string LoginReturnUrlParameter { get; set; } =
        AuthenticationConstants.ApplicationUIConstants.DefaultRoutePathParams.Login;

    /// <summary>
    /// Gets or sets the URL of the logout page where users are redirected to confirm sign-out
    /// (e.g., "/account/logout").
    /// </summary>
    public string LogoutUrl { get; set; }

    /// <summary>
    /// Gets or sets the query string parameter name used to pass the logout session identifier
    /// to the logout page. Defaults to "logoutId".
    /// </summary>
    public string LogoutIdParameter { get; set; } =
        AuthenticationConstants.ApplicationUIConstants.DefaultRoutePathParams.Logout;

    /// <summary>
    /// Gets or sets the URL of the error page where users are redirected when an OAuth/OIDC error occurs
    /// (e.g., "/home/error").
    /// </summary>
    public string ErrorUrl { get; set; }

    /// <summary>
    /// Gets or sets the query string parameter name used to pass the error identifier to the error page.
    /// Defaults to "errorId".
    /// </summary>
    public string ErrorIdParameter { get; set; } =
        AuthenticationConstants.ApplicationUIConstants.DefaultRoutePathParams.Error;
}
