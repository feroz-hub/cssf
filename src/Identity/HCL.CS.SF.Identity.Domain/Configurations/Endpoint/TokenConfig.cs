/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Configurations.Endpoint;

/// <summary>
/// Configuration settings for token issuance and cryptographic key management.
/// Controls how the authorization server mints JWTs and exposes its signing key metadata.
/// Bound from the "Token" section of the endpoint configuration.
/// </summary>
public class TokenConfig
{
    /// <summary>
    /// Gets or sets the issuer URI ("iss" claim) embedded in all tokens issued by this server.
    /// Must match the value that resource servers expect when validating tokens.
    /// </summary>
    public string IssuerUri { get; set; }

    // Canonical API audience that resource servers must enforce.
    /// <summary>
    /// Gets or sets the canonical API audience identifier ("aud" claim) that resource servers
    /// must validate when accepting access tokens. Typically set to the base URL or logical name
    /// of the protected API.
    /// </summary>
    public string ApiIdentifier { get; set; }

    /// <summary>
    /// Gets or sets the duration (in seconds) that the JWKS (JSON Web Key Set) response is cached
    /// by clients and intermediaries. Defaults to 3600 seconds (1 hour).
    /// </summary>
    public int CachingLifetime { get; set; } = 3600;

    /// <summary>
    /// Gets or sets whether the JWKS endpoint is publicly accessible.
    /// When true, the signing keys are exposed at the well-known JWKS URI for token validation.
    /// Defaults to true.
    /// </summary>
    public bool ShowKeySet { get; set; } = true;

    /// <summary>
    /// Gets or sets the default access token expiration time in seconds.
    /// Defaults to 60 seconds.
    /// </summary>
    public int TokenExpiration { get; set; } = 60;

    /// <summary>
    /// Gets or sets the length (in bits) of auto-generated client secrets.
    /// Defaults to 256 bits.
    /// </summary>
    public int ClientSecretLength { get; set; } = 256;

    /// <summary>
    /// Gets or sets the number of days before an auto-generated client secret expires.
    /// Defaults to 60 days.
    /// </summary>
    public int ClientSecretExpirationInDays { get; set; } = 60;
}

// TODO To implement feature to disable endpoint when set to false

/// <summary>
/// Configuration settings that control which OAuth 2.0 / OIDC endpoints are enabled on the authorization server.
/// Setting a property to false disables the corresponding endpoint. Bound from the "Endpoints" configuration section.
/// </summary>
public class EndpointsConfig
{
    /// <summary>
    /// Gets or sets whether the Authorization endpoint (/authorize) is enabled.
    /// This endpoint initiates the Authorization Code and Implicit flows.
    /// </summary>
    public bool EnableAuthorizeEndpoint { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the JWKS (JSON Web Key Set) endpoint is enabled.
    /// Resource servers use this endpoint to retrieve public signing keys for token validation.
    /// </summary>
    public bool EnableJWKSEndpoint { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the Token endpoint (/token) is enabled.
    /// This endpoint exchanges authorization codes, refresh tokens, or client credentials for access tokens.
    /// </summary>
    public bool EnableTokenEndpoint { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the UserInfo endpoint (/userinfo) is enabled.
    /// This endpoint returns claims about the authenticated user when presented with a valid access token.
    /// </summary>
    public bool EnableUserInfoEndpoint { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the OpenID Connect Discovery endpoint (/.well-known/openid-configuration) is enabled.
    /// Clients use this endpoint to auto-discover the server's capabilities, endpoints, and supported features.
    /// </summary>
    public bool EnableDiscoveryEndpoint { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the End Session endpoint (/endsession) is enabled.
    /// This endpoint handles OIDC-compliant logout requests (RP-Initiated Logout).
    /// </summary>
    public bool EnableEndSessionEndpoint { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the Token Revocation endpoint (/revocation) is enabled.
    /// This endpoint allows clients to revoke previously issued access or refresh tokens (RFC 7009).
    /// </summary>
    public bool EnableTokenRevocationEndpoint { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the Token Introspection endpoint (/introspect) is enabled.
    /// Resource servers use this endpoint to determine the active state and metadata of a token (RFC 7662).
    /// </summary>
    public bool EnableIntrospectionEndpoint { get; set; } = true;

    /// <summary>
    /// Gets or sets whether front-channel logout is supported.
    /// When true, the server can send logout notifications to clients via browser-based redirects.
    /// </summary>
    public bool FrontchannelLogoutSupported { get; set; } = false;

    /// <summary>
    /// Gets or sets whether front-channel logout requires a session identifier (sid) claim.
    /// When true, the sid claim must be present in the logout notification.
    /// </summary>
    public bool FrontchannelLogoutSessionRequired { get; set; } = false;

    /// <summary>
    /// Gets or sets whether back-channel logout is supported.
    /// When true, the server can send logout tokens directly to client back-channel logout URIs.
    /// </summary>
    public bool BackchannelLogoutSupported { get; set; } = false;

    /// <summary>
    /// Gets or sets whether back-channel logout requires a session identifier (sid) claim
    /// in the logout token.
    /// </summary>
    public bool BackchannelLogoutSessionRequired { get; set; } = false;
}

/// <summary>
/// Configuration settings that define the allowed minimum and maximum expiration times (in seconds)
/// for various token types. These bounds are enforced during client registration and token issuance
/// to prevent excessively short-lived or long-lived tokens.
/// </summary>
public class TokenExpiration
{
    /// <summary>
    /// Gets or sets the minimum allowed access token lifetime in seconds. Defaults to 60 (1 minute).
    /// </summary>
    public int MinAccessTokenExpiration { get; set; } = 60;

    /// <summary>
    /// Gets or sets the maximum allowed access token lifetime in seconds. Defaults to 900 (15 minutes).
    /// </summary>
    public int MaxAccessTokenExpiration { get; set; } = 900;

    /// <summary>
    /// Gets or sets the minimum allowed identity token (id_token) lifetime in seconds. Defaults to 60 (1 minute).
    /// </summary>
    public int MinIdentityTokenExpiration { get; set; } = 60;

    /// <summary>
    /// Gets or sets the maximum allowed identity token (id_token) lifetime in seconds. Defaults to 3600 (1 hour).
    /// </summary>
    public int MaxIdentityTokenExpiration { get; set; } = 3600;

    /// <summary>
    /// Gets or sets the minimum allowed refresh token lifetime in seconds. Defaults to 300 (5 minutes).
    /// </summary>
    public int MinRefreshTokenExpiration { get; set; } = 300;

    /// <summary>
    /// Gets or sets the maximum allowed refresh token lifetime in seconds. Defaults to 86400 (24 hours).
    /// </summary>
    public int MaxRefreshTokenExpiration { get; set; } = 86400;

    /// <summary>
    /// Gets or sets the minimum allowed authorization code lifetime in seconds. Defaults to 60 (1 minute).
    /// </summary>
    public int MinAuthorizationCodeExpiration { get; set; } = 60;

    /// <summary>
    /// Gets or sets the maximum allowed authorization code lifetime in seconds. Defaults to 600 (10 minutes).
    /// </summary>
    public int MaxAuthorizationCodeExpiration { get; set; } = 600;

    /// <summary>
    /// Gets or sets the minimum allowed logout token lifetime in seconds. Defaults to 1800 (30 minutes).
    /// </summary>
    public int MinLogoutTokenExpiration { get; set; } = 1800;

    /// <summary>
    /// Gets or sets the maximum allowed logout token lifetime in seconds. Defaults to 86400 (24 hours).
    /// </summary>
    public int MaxLogoutTokenExpiration { get; set; } = 86400;
}
