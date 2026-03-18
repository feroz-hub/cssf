/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Enums;

/// <summary>
/// Defines the OAuth 2.0 / OIDC application types that determine how a registered client interacts
/// with the authorization server (e.g., redirect-based vs. native flows, public vs. confidential clients).
/// </summary>
public enum ApplicationType
{
    /// <summary>
    /// A server-side web application (confidential client) that can securely store client secrets
    /// and uses the Authorization Code flow with server-to-server token exchange.
    /// </summary>
    RegularWeb = 1,

    /// <summary>
    /// A browser-based single-page application (public client) that cannot securely store secrets
    /// and must use PKCE (Proof Key for Code Exchange) with the Authorization Code flow.
    /// </summary>
    SinglePageApp = 2,

    /// <summary>
    /// A native/mobile application (public client) that uses custom URI schemes or loopback redirects
    /// and must use PKCE with the Authorization Code flow.
    /// </summary>
    Native = 3,

    /// <summary>
    /// A machine-to-machine service (confidential client) with no user interaction,
    /// using the Client Credentials grant type to obtain access tokens.
    /// </summary>
    Service = 4
}

/// <summary>
/// Defines the OAuth 2.0 grant types supported by the authorization server.
/// Each grant type represents a different mechanism for obtaining access tokens.
/// </summary>
public enum GrantType
{
    /// <summary>
    /// The Authorization Code grant type (RFC 6749 Section 4.1). The client exchanges an authorization code
    /// (obtained via user browser redirect) for access and refresh tokens. Recommended for web and native apps.
    /// </summary>
    AuthorizationCode = 1,

    /// <summary>
    /// The Resource Owner Password Credentials grant type (RFC 6749 Section 4.3). The client directly collects
    /// the user's username and password. Only recommended for trusted first-party applications.
    /// </summary>
    Password = 2,

    /// <summary>
    /// The Client Credentials grant type (RFC 6749 Section 4.4). The client authenticates with its own credentials
    /// (client_id and client_secret) without a user context. Used for machine-to-machine communication.
    /// </summary>
    ClientCredentials = 3,

    /// <summary>
    /// The Refresh Token grant type (RFC 6749 Section 6). The client exchanges a previously issued refresh token
    /// for a new access token without requiring the user to re-authenticate.
    /// </summary>
    RefreshToken = 4
}

/// <summary>
/// Defines the format of access tokens issued by the authorization server.
/// </summary>
public enum AccessTokenType
{
    /// <summary>
    /// JSON Web Token (JWT) format. The access token is a self-contained, signed JWT
    /// that resource servers can validate locally without calling the introspection endpoint.
    /// </summary>
    JWT = 1 // TODO: Enum JWT is never used.
    // Reference = 2  // Planned for V2 Release.
}

/// <summary>
/// Defines the Content Security Policy (CSP) level used for securing HTTP responses
/// from the authorization server endpoints.
/// </summary>
public enum CspLevel
{
    /// <summary>
    /// CSP Level 1 - basic content security policy with limited directive support.
    /// </summary>
    One = 0,

    /// <summary>
    /// CSP Level 2 - enhanced content security policy with nonce-based and hash-based script/style allowlists.
    /// </summary>
    Two = 1
}

/// <summary>
/// Defines the cryptographic signing algorithms supported for JWT token signing.
/// These correspond to the "alg" header parameter in JSON Web Signatures (JWS) as defined in RFC 7518.
/// </summary>
public enum SigningAlgorithm
{
    /// <summary>
    /// RSA signature with SHA-256 hash (asymmetric). The most widely supported algorithm for OAuth/OIDC.
    /// </summary>
    RS256 = 1,

    /// <summary>
    /// RSA signature with SHA-384 hash (asymmetric). Provides stronger security than RS256.
    /// </summary>
    RS384 = 2,

    /// <summary>
    /// RSA signature with SHA-512 hash (asymmetric). Provides the strongest RSA-based security.
    /// </summary>
    RS512 = 3,

    /// <summary>
    /// HMAC with SHA-256 (symmetric). Uses a shared secret for both signing and verification.
    /// </summary>
    HS256 = 4,

    /// <summary>
    /// HMAC with SHA-384 (symmetric). Uses a shared secret with a longer hash output.
    /// </summary>
    HS384 = 5,

    /// <summary>
    /// HMAC with SHA-512 (symmetric). Uses a shared secret with the longest hash output.
    /// </summary>
    HS512 = 6,

    /// <summary>
    /// ECDSA with P-256 curve and SHA-256 hash (asymmetric). Compact signatures with strong security.
    /// </summary>
    ES256 = 7,

    /// <summary>
    /// ECDSA with P-384 curve and SHA-384 hash (asymmetric). Stronger elliptic curve security.
    /// </summary>
    ES384 = 8,

    /// <summary>
    /// ECDSA with P-521 curve and SHA-512 hash (asymmetric). Maximum elliptic curve security.
    /// </summary>
    ES512 = 9,

    /// <summary>
    /// RSA-PSS with SHA-256 (asymmetric). A probabilistic RSA signature scheme resistant to certain attacks on PKCS#1 v1.5.
    /// </summary>
    PS256 = 10,

    /// <summary>
    /// RSA-PSS with SHA-384 (asymmetric). Stronger probabilistic RSA signature variant.
    /// </summary>
    PS384 = 11,

    /// <summary>
    /// RSA-PSS with SHA-512 (asymmetric). Strongest probabilistic RSA signature variant.
    /// </summary>
    PS512 = 12
}

/// <summary>
/// Defines the methods used by OAuth 2.0 clients to authenticate when calling the token endpoint.
/// Corresponds to the "token_endpoint_auth_method" client metadata as defined in RFC 7591.
/// </summary>
public enum ParseMethods
{
    /// <summary>
    /// HTTP Basic authentication (RFC 7617). The client_id and client_secret are sent in the
    /// Authorization header as a Base64-encoded "client_id:client_secret" pair.
    /// </summary>
    Basic = 0,

    /// <summary>
    /// POST body authentication. The client_id and client_secret are sent as form parameters
    /// in the request body of the token endpoint call.
    /// </summary>
    Post = 1,

    /// <summary>
    /// JWT-based client authentication (RFC 7523). The client authenticates by sending a signed JWT
    /// assertion using its client secret as the HMAC key.
    /// </summary>
    JwtSecret = 2
}
