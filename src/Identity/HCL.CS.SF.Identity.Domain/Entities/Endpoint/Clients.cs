/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Enums;

namespace HCL.CS.SF.Domain.Entities.Endpoint;

/// <summary>
/// Represents a registered OAuth2/OIDC client application.
/// Each client has its own credentials, allowed grant types, token lifetimes, and redirect URIs
/// that the identity server enforces during authorization and token issuance.
/// </summary>
public class Clients : BaseEntity
{
    /// <summary>The unique OAuth2 client identifier (sent as "client_id" in protocol requests).</summary>
    public string ClientId { get; set; }

    /// <summary>Human-readable display name for the client, shown on consent screens.</summary>
    public string ClientName { get; set; }

    /// <summary>URL of the client application's home page.</summary>
    public string ClientUri { get; set; }

    /// <summary>Unix epoch timestamp when the client_id was originally issued (RFC 7591).</summary>
    public long ClientIdIssuedAt { get; set; }

    /// <summary>Unix epoch timestamp when the client secret expires. Zero means it does not expire.</summary>
    public long ClientSecretExpiresAt { get; set; }

    /// <summary>Hashed client secret used for confidential client authentication.</summary>
    public string ClientSecret { get; set; }

    /// <summary>URI to the client's logo image, displayed on consent and login screens.</summary>
    public string LogoUri { get; set; }

    /// <summary>URI to the client's terms of service page.</summary>
    public string TermsOfServiceUri { get; set; }

    /// <summary>URI to the client's privacy policy page.</summary>
    public string PolicyUri { get; set; }

    /// <summary>Refresh token lifetime in seconds.</summary>
    public int RefreshTokenExpiration { get; set; }

    /// <summary>Access token lifetime in seconds.</summary>
    public int AccessTokenExpiration { get; set; }

    /// <summary>Identity (ID) token lifetime in seconds.</summary>
    public int IdentityTokenExpiration { get; set; }

    /// <summary>Logout token lifetime in seconds (used in back-channel logout).</summary>
    public int LogoutTokenExpiration { get; set; }

    /// <summary>Authorization code lifetime in seconds. Codes are short-lived by design (typically 60-300s).</summary>
    public int AuthorizationCodeExpiration { get; set; }

    /// <summary>Determines whether access tokens are JWTs (self-contained) or reference tokens (requiring introspection).</summary>
    public AccessTokenType AccessTokenType { get; set; }

    /// <summary>When true, the client must use PKCE (Proof Key for Code Exchange) during the authorization code flow.</summary>
    public bool RequirePkce { get; set; }

    /// <summary>When true, the "plain" code_challenge_method is permitted; otherwise only S256 is accepted.</summary>
    public bool IsPkceTextPlain { get; set; }

    /// <summary>When true, the client must authenticate with a client secret. False for public clients (SPAs, native apps).</summary>
    public bool RequireClientSecret { get; set; }

    /// <summary>Indicates a first-party (trusted) application that may skip the user consent screen.</summary>
    public bool IsFirstPartyApp { get; set; }

    /// <summary>When true, the client may request the "offline_access" scope to obtain refresh tokens.</summary>
    public bool AllowOfflineAccess { get; set; }

    /// <summary>Space-delimited list of scopes the client is permitted to request.</summary>
    public string AllowedScopes { get; set; }

    /// <summary>When true, access tokens may be delivered via the browser (implicit/hybrid flows). Should be false for confidential flows.</summary>
    public bool AllowAccessTokensViaBrowser { get; set; }

    /// <summary>Collection of allowed redirect URIs for authorization responses.</summary>
    public List<ClientRedirectUris> RedirectUris { get; set; }

    /// <summary>Collection of allowed post-logout redirect URIs.</summary>
    public List<ClientPostLogoutRedirectUris> PostLogoutRedirectUris { get; set; }

    /// <summary>The application type (e.g., Web, SPA, Native) per RFC 7591 dynamic registration.</summary>
    public ApplicationType ApplicationType { get; set; }

    /// <summary>The signing algorithm required for tokens issued to this client (e.g., "RS256", "ES256").</summary>
    public string AllowedSigningAlgorithm { get; set; }

    /// <summary>Comma-separated list of OAuth2 grant types this client supports (e.g., "authorization_code", "client_credentials").</summary>
    public string SupportedGrantTypes { get; set; }

    /// <summary>Comma-separated list of OAuth2 response types this client supports (e.g., "code", "token").</summary>
    public string SupportedResponseTypes { get; set; }

    /// <summary>When true, a session identifier (sid) claim is included in front-channel logout requests.</summary>
    public bool FrontChannelLogoutSessionRequired { get; set; }

    /// <summary>The URI the identity server sends a front-channel logout notification to via browser redirect.</summary>
    public string FrontChannelLogoutUri { get; set; }

    /// <summary>When true, a session identifier (sid) claim is included in back-channel logout tokens.</summary>
    public bool BackChannelLogoutSessionRequired { get; set; }

    /// <summary>The URI the identity server posts a back-channel logout token to (server-to-server).</summary>
    public string BackChannelLogoutUri { get; set; }

    /// <summary>Optional. When set, access tokens for this client use this value as the aud claim (e.g. rentflow.api, HCL.CS.SF.api).</summary>
    public string PreferredAudience { get; set; }
}
