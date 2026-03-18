/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;

namespace HCL.CS.SF.Domain.Models.Endpoint;

/// <summary>
/// Represents an OAuth 2.0 authorization code issued during the Authorization Code flow.
/// The code is a short-lived, single-use credential that the client exchanges at the token
/// endpoint for access, refresh, and identity tokens.
/// </summary>
public class AuthorizationCodeModel
{
    /// <summary>Unique identifier for this authorization code record.</summary>
    public virtual Guid Id { get; set; } = default;

    /// <summary>The opaque state value provided by the client to maintain request-response correlation and CSRF protection.</summary>
    public virtual string State { get; set; }

    /// <summary>The OAuth 2.0 client identifier that requested this authorization code.</summary>
    public virtual string ClientId { get; set; }

    /// <summary>The redirect URI to which the authorization code was delivered.</summary>
    public virtual string RedirectUri { get; set; }

    /// <summary>Indicates whether the original request included the "openid" scope, making it an OIDC authentication request.</summary>
    public bool IsOpenId { get; set; }

    /// <summary>UTC timestamp when this authorization code was created.</summary>
    public DateTime CreationTime { get; set; }

    /// <summary>Maximum lifetime of the authorization code in seconds before it expires.</summary>
    public int Lifetime { get; set; }

    /// <summary>The authenticated user principal associated with this authorization code.</summary>
    public ClaimsPrincipal Subject { get; set; }

    /// <summary>The nonce value from the original authorization request, used to mitigate replay attacks in OIDC.</summary>
    public string Nonce { get; set; }

    /// <summary>The session identifier linking this code to the user's authentication session.</summary>
    public string SessionId { get; set; }

    /// <summary>The PKCE code challenge provided by the client during the authorization request.</summary>
    public string CodeChallenge { get; set; }

    /// <summary>The PKCE code challenge method (e.g., "S256" or "plain") used to transform the code verifier.</summary>
    public string CodeChallengeMethod { get; set; }

    /// <summary>The list of scopes requested by the client in the authorization request.</summary>
    public List<string> RequestedScopes { get; set; }

    /// <summary>The parsed and validated scope breakdown (identity resources, API scopes, etc.) for this request.</summary>
    public AllowedScopesParserModel AllowedScopesParserModel { get; set; }

    /// <summary>Aggregated token generation details including user, client, and resource information.</summary>
    public TokenDetailsModel TokenDetails { get; set; }
}
