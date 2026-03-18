/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Enums;
using static HCL.CS.SF.Domain.Constants.Endpoint.OpenIdConstants;

namespace HCL.CS.SF.Domain.Models.Endpoint;

/// <summary>
/// Represents a registered OAuth 2.0 / OIDC client application.
/// Contains the full client configuration including grant types, token lifetimes,
/// redirect URIs, PKCE settings, and logout configuration as defined by RFC 6749,
/// RFC 7591 (Dynamic Client Registration), and OpenID Connect specifications.
/// </summary>
public class ClientsModel : BaseModel
{
    /// <summary>The unique OAuth 2.0 client identifier issued during registration.</summary>
    public string ClientId { get; set; }

    /// <summary>The human-readable display name of the client application.</summary>
    public string ClientName { get; set; }

    /// <summary>The URL of the client application's home page.</summary>
    public string ClientUri { get; set; }

    /// <summary>The UTC timestamp when the client identifier was issued.</summary>
    public DateTime ClientIdIssuedAt { get; set; }

    /// <summary>The UTC timestamp when the client secret expires. A value of epoch zero means it does not expire.</summary>
    public DateTime ClientSecretExpiresAt { get; set; }

    /// <summary>The client secret used for confidential client authentication at the token endpoint.</summary>
    public string ClientSecret { get; set; }

    /// <summary>URI pointing to the client application's logo for consent screen display.</summary>
    public string LogoUri { get; set; }

    /// <summary>URI pointing to the client's terms of service document.</summary>
    public string TermsOfServiceUri { get; set; }

    /// <summary>URI pointing to the client's privacy policy document.</summary>
    public string PolicyUri { get; set; }

    /// <summary>Refresh token lifetime in seconds. Defaults to 86400 (24 hours).</summary>
    public int RefreshTokenExpiration { get; set; } = 86400;

    /// <summary>Access token lifetime in seconds. Defaults to 900 (15 minutes).</summary>
    public int AccessTokenExpiration { get; set; } = 900;

    /// <summary>Identity token lifetime in seconds. Defaults to 3600 (1 hour).</summary>
    public int IdentityTokenExpiration { get; set; } = 3600;

    /// <summary>Logout token lifetime in seconds for back-channel logout. Defaults to 1800 (30 minutes).</summary>
    public int LogoutTokenExpiration { get; set; } = 1800;

    /// <summary>Authorization code lifetime in seconds. Defaults to 600 (10 minutes).</summary>
    public int AuthorizationCodeExpiration { get; set; } = 600;

    /// <summary>The format of access tokens issued to this client (JWT or Reference).</summary>
    public AccessTokenType AccessTokenType { get; set; } = AccessTokenType.JWT;

    /// <summary>Indicates whether this client must use PKCE (Proof Key for Code Exchange) per RFC 7636.</summary>
    public bool RequirePkce { get; set; }

    /// <summary>Indicates whether the client is allowed to use the "plain" PKCE code challenge method instead of "S256".</summary>
    public bool IsPkceTextPlain { get; set; }

    /// <summary>Indicates whether this client must authenticate with a client secret at the token endpoint.</summary>
    public bool RequireClientSecret { get; set; } = true;

    /// <summary>Indicates whether this is a first-party (trusted) application that can skip the consent screen.</summary>
    public bool IsFirstPartyApp { get; set; } = true;

    /// <summary>Indicates whether this client is allowed to request the offline_access scope for refresh tokens.</summary>
    public bool AllowOfflineAccess { get; set; }

    /// <summary>Indicates whether access tokens can be transmitted via the browser (required for implicit flow).</summary>
    public bool AllowAccessTokensViaBrowser { get; set; }

    /// <summary>The type of application (e.g., Web, SPA, Native, Machine) which affects allowed grant types.</summary>
    public ApplicationType ApplicationType { get; set; }

    /// <summary>The signing algorithm used for tokens issued to this client. Defaults to RS256.</summary>
    public string AllowedSigningAlgorithm { get; set; } = Algorithms.RsaSha256;

    /// <summary>Indicates whether the front-channel logout requires the session ID (sid claim) in the logout request.</summary>
    public bool FrontChannelLogoutSessionRequired { get; set; } = false;

    /// <summary>The front-channel logout URI where the identity server renders an iframe to trigger client-side logout.</summary>
    public string FrontChannelLogoutUri { get; set; }

    /// <summary>Indicates whether the back-channel logout requires the session ID (sid claim) in the logout token.</summary>
    public bool BackChannelLogoutSessionRequired { get; set; }

    /// <summary>The back-channel logout URI where the identity server sends an HTTP POST with a logout token.</summary>
    public string BackChannelLogoutUri { get; set; }

    /// <summary>The OAuth 2.0 grant types this client is allowed to use (e.g., authorization_code, client_credentials).</summary>
    public List<string> SupportedGrantTypes { get; set; }

    /// <summary>The OAuth 2.0 response types this client is allowed to request (e.g., code, token, id_token).</summary>
    public List<string> SupportedResponseTypes { get; set; }

    /// <summary>The scopes this client is permitted to request.</summary>
    public List<string> AllowedScopes { get; set; }

    /// <summary>The registered redirect URIs where authorization responses can be sent.</summary>
    public List<ClientRedirectUrisModel> RedirectUris { get; set; }

    /// <summary>The registered post-logout redirect URIs where the user is sent after signing out.</summary>
    public List<ClientPostLogoutRedirectUrisModel> PostLogoutRedirectUris { get; set; }

    /// <summary>Optional. When set, access tokens for this client use this value as the aud claim (e.g. rentflow.api, HCL.CS.SF.api).</summary>
    public string PreferredAudience { get; set; }
}
