/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Models.Endpoint.Validation;

namespace HCL.CS.SF.Domain.Models.Endpoint.Request;

/// <summary>
/// Represents a validated OAuth 2.0 token request submitted to the token endpoint.
/// Supports all grant types (authorization_code, client_credentials, refresh_token, password)
/// and carries the grant-specific data needed for token generation.
/// </summary>
public class ValidatedTokenRequestModel : ValidatedBaseModel
{
    /// <summary>The token issuer URI (iss claim) to embed in generated tokens.</summary>
    public string Issuer { get; set; }

    /// <summary>The nonce value from the original authorization request, included in the ID token.</summary>
    public string Nonce { get; set; }

    /// <summary>The state value from the original authorization request.</summary>
    public string State { get; set; }

    /// <summary>The OAuth 2.0 grant type (e.g., "authorization_code", "client_credentials", "refresh_token").</summary>
    public string GrantType { get; set; }

    /// <summary>The resource owner's username, used in the ROPC grant type.</summary>
    public string UserName { get; set; }

    /// <summary>The refresh token submitted for the refresh_token grant type.</summary>
    public string RequestedRefreshToken { get; set; }

    /// <summary>The authorization code model retrieved from the store, used in the authorization_code grant type.</summary>
    public AuthorizationCodeModel AuthorizationCode { get; set; }

    /// <summary>The access token value to be hashed for the at_hash claim in the ID token.</summary>
    public string AccessTokenToHash { get; set; }

    /// <summary>The authorization code value to be hashed for the c_hash claim in the ID token.</summary>
    public string AuthorizationCodeToHash { get; set; }

    /// <summary>The PKCE code verifier submitted by the client, validated against the stored code challenge.</summary>
    public string CodeVerifier { get; set; }

    /// <summary>Indicates whether this token request originated from the authorization endpoint (hybrid flow).</summary>
    public bool IsRequestFromAuthorizationEndpoint { get; set; } = false;

    /// <summary>The response types from the original authorization request (used in hybrid flow token generation).</summary>
    public List<string> ResponseTypes { get; set; }

    /// <summary>Aggregated token generation details including user, client, and resource information.</summary>
    public TokenDetailsModel TokenDetails { get; set; }
}
