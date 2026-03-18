/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.ComponentModel;
using HCL.CS.SF.Domain.Models.Endpoint.Request;

namespace HCL.CS.SF.Domain.Models.Endpoint.Response;

/// <summary>
/// Response model returned from the OAuth 2.0 / OIDC authorization endpoint.
/// Contains the authorization code, tokens (for implicit/hybrid flows), and state
/// that are delivered to the client's redirect URI.
/// </summary>
public class AuthorizationResponseModel : ErrorResponseModel
{
    /// <summary>The validated authorization request that produced this response.</summary>
    public ValidatedAuthorizeRequestModel Request { get; set; }

    /// <summary>The client's registered redirect URI where this response is delivered.</summary>
    [DisplayName("redirect_uri")] public string RedirectUri => Request?.RedirectUri;

    /// <summary>The opaque state value echoed back to the client for CSRF protection.</summary>
    [DisplayName("state")] public string State => Request?.State;

    /// <summary>The granted scopes, space-delimited, as per RFC 6749.</summary>
    [DisplayName("scope")] public string Scope { get; set; }

    /// <summary>The OIDC ID token issued for implicit or hybrid flows.</summary>
    [DisplayName("id_token")] public string IdentityToken { get; set; }

    /// <summary>The OAuth 2.0 access token issued for implicit or hybrid flows.</summary>
    [DisplayName("access_token")] public string AccessToken { get; set; }

    /// <summary>The refresh token issued when offline_access scope is granted.</summary>
    [DisplayName("refresh_token")] public string RefreshToken { get; set; }

    /// <summary>The access token lifetime in seconds.</summary>
    public int AccessTokenLifetime { get; set; }

    /// <summary>The authorization code issued for the Authorization Code and Hybrid flows.</summary>
    [DisplayName("code")] public string Code { get; set; }

    /// <summary>The OIDC session state value used by the client for session management via check_session_iframe.</summary>
    [DisplayName("session_state")] public string SessionState { get; set; }
}
