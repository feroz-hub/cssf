/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using HCL.CS.SF.Domain.Models.Endpoint.Validation;

namespace HCL.CS.SF.Domain.Models.Endpoint.Request;

/// <summary>
/// Represents a validated OAuth 2.0 / OIDC authorization request.
/// Contains all parameters from the authorize endpoint after validation, including
/// response type, scopes, PKCE challenge, and the authenticated user.
/// </summary>
public class ValidatedAuthorizeRequestModel : ValidatedBaseModel
{
    /// <summary>Initializes a new instance with empty scope and ACR collections.</summary>
    public ValidatedAuthorizeRequestModel()
    {
        RequestedScopes = new List<string>();
        AuthenticationContextReferenceClasses = new List<string>();
    }

    /// <summary>The OAuth 2.0 response type (e.g., "code", "token", "id_token", or combinations for hybrid flow).</summary>
    public string ResponseType { get; set; }

    /// <summary>The response mode indicating how results are delivered: "query", "fragment", or "form_post".</summary>
    public string ResponseMode { get; set; }

    /// <summary>The grant type inferred from the response type (e.g., "authorization_code", "implicit").</summary>
    public string GrantType { get; set; }

    /// <summary>The list of scope values requested in the authorization request.</summary>
    public List<string> RequestedScopes { get; set; }

    /// <summary>The opaque state value from the client for CSRF protection and request-response correlation.</summary>
    public string State { get; set; }

    /// <summary>Indicates whether this is an OIDC authentication request (the "openid" scope is present).</summary>
    public bool IsOpenIdRequest { get; set; } = false;

    /// <summary>Indicates whether the request targets API resources (as opposed to identity-only resources).</summary>
    public bool IsApiResourceRequest { get; set; }

    /// <summary>The nonce value used to associate the client session with the ID token for replay protection.</summary>
    public string Nonce { get; set; }

    /// <summary>The requested Authentication Context Class References (acr_values), indicating desired authentication levels.</summary>
    public List<string> AuthenticationContextReferenceClasses { get; set; }

    /// <summary>The prompt modes requested (e.g., "login", "consent", "none") controlling the authentication UX.</summary>
    public IEnumerable<string> PromptModes { get; set; } = Enumerable.Empty<string>();

    /// <summary>The maximum allowable authentication age in seconds. Forces re-authentication if exceeded.</summary>
    public int? MaxAge { get; set; }

    /// <summary>The PKCE code challenge value for public client protection (RFC 7636).</summary>
    public string CodeChallenge { get; set; }

    /// <summary>The PKCE code challenge method ("S256" or "plain").</summary>
    public string CodeChallengeMethod { get; set; }

    /// <summary>The currently authenticated user's claims principal from the session cookie.</summary>
    public ClaimsPrincipal User { get; set; }
}
