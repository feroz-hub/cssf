/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.ComponentModel;

namespace HCL.CS.SF.Domain.Models.Endpoint.Response;

/// <summary>
/// Response model returned from the OAuth 2.0 token endpoint.
/// Contains the access token, optional identity and refresh tokens, and associated metadata
/// as defined in RFC 6749 Section 5.1.
/// </summary>
public class TokenResponseModel
{
    /// <summary>The type of token issued, typically "Bearer" (RFC 6750).</summary>
    [DisplayName("token_type")] public string TokenType { get; set; }

    /// <summary>The OIDC ID token containing user identity claims, issued when "openid" scope is requested.</summary>
    [DisplayName("id_token")] public string IdentityToken { get; set; }

    /// <summary>The OAuth 2.0 access token used to access protected resources.</summary>
    [DisplayName("access_token")] public string AccessToken { get; set; }

    /// <summary>The lifetime of the access token in seconds.</summary>
    [DisplayName("expires_in")] public int AccessTokenExpiresIn { get; set; }

    /// <summary>The refresh token used to obtain new access tokens without re-authentication.</summary>
    [DisplayName("refresh_token")] public string RefreshToken { get; set; }

    /// <summary>Space-delimited list of scopes granted for the access token.</summary>
    [DisplayName("scope")] public string Scope { get; set; }

    /// <summary>The state value echoed from the authorization request, if applicable.</summary>
    [DisplayName("state")] public string State { get; set; } = null;
}

/// <summary>
/// Deserialization-friendly token response model with JSON-matching property names.
/// Used for parsing raw token endpoint responses from external or internal HTTP calls.
/// </summary>
public class TokenResponseResultModel
{
    /// <summary>The OIDC ID token.</summary>
    public string id_token { get; set; }

    /// <summary>The OAuth 2.0 access token.</summary>
    public string access_token { get; set; }

    /// <summary>The access token lifetime in seconds.</summary>
    public int expires_in { get; set; }

    /// <summary>The token type, typically "Bearer".</summary>
    public string token_type { get; set; }

    /// <summary>The refresh token for obtaining new access tokens.</summary>
    public string refresh_token { get; set; }

    /// <summary>Space-delimited granted scopes.</summary>
    public string scope { get; set; }
}
