/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Configurations.Endpoint;

/// <summary>
/// Configuration settings that define the maximum allowed lengths for OAuth 2.0 / OIDC request parameters.
/// These restrictions protect against buffer overflow, denial-of-service, and injection attacks by
/// rejecting requests with excessively long parameter values. Bound from the "InputLengthRestrictions"
/// configuration section.
/// </summary>
public class InputLengthRestrictionsConfig
{
    /// <summary>
    /// Default maximum length (in characters) applied to most string parameters.
    /// </summary>
    private const int DefaultValue = 255;

    /// <summary>
    /// Gets or sets the maximum allowed length for the "client_id" parameter. Defaults to 255.
    /// </summary>
    public int ClientId { get; set; } = DefaultValue;

    /// <summary>
    /// Gets or sets the maximum allowed length for the "client_secret" parameter. Defaults to 255.
    /// </summary>
    public int ClientSecret { get; set; } = DefaultValue;

    /// <summary>
    /// Gets or sets the maximum allowed length for the "scope" parameter.
    /// Set higher (700) because scope strings can contain many space-delimited scope values.
    /// </summary>
    public int Scope { get; set; } = 700;

    /// <summary>
    /// Gets or sets the maximum allowed length for the "redirect_uri" parameter. Defaults to 255.
    /// </summary>
    public int RedirectUri { get; set; } = DefaultValue;

    /// <summary>
    /// Gets or sets the maximum allowed length for the "nonce" parameter used in OIDC authorization requests.
    /// Defaults to 20 characters.
    /// </summary>
    public int Nonce { get; set; } = 20;

    // OIDC state can be large (for example ASP.NET Core DataProtection payloads).
    /// <summary>
    /// Gets or sets the maximum allowed length for the "state" parameter.
    /// Set to 2048 because OIDC state values can contain large serialized payloads
    /// (e.g., ASP.NET Core DataProtection-encrypted data).
    /// </summary>
    public int State { get; set; } = 2048;

    /// <summary>
    /// Gets or sets the maximum allowed length for the "grant_type" parameter. Defaults to 255.
    /// </summary>
    public int GrantType { get; set; } = DefaultValue;

    /// <summary>
    /// Gets or sets the maximum allowed length for the "username" parameter in the Resource Owner Password flow. Defaults to 255.
    /// </summary>
    public int UserName { get; set; } = DefaultValue;

    /// <summary>
    /// Gets or sets the maximum allowed length for the "password" parameter in the Resource Owner Password flow. Defaults to 255.
    /// </summary>
    public int Password { get; set; } = DefaultValue;

    /// <summary>
    /// Gets or sets the maximum allowed length for the authorization code exchanged at the token endpoint.
    /// Defaults to 512 characters.
    /// </summary>
    public int AuthorizationCode { get; set; } = 512;

    /// <summary>
    /// Gets or sets the maximum allowed length for refresh token values. Defaults to 512 characters.
    /// </summary>
    public int RefreshToken { get; set; } = 512;

    /// <summary>
    /// Gets or sets the maximum allowed length for JWT assertions (e.g., client_assertion in private_key_jwt authentication).
    /// Defaults to 51200 characters to accommodate large signed JWTs.
    /// </summary>
    public int Jwt { get; set; } = 51200;

    /// <summary>
    /// Gets the minimum allowed length for the PKCE code_challenge parameter.
    /// Fixed at 43 characters per RFC 7636 specification.
    /// </summary>
    public int CodeChallengeMinLength { get; } = 43;

    /// <summary>
    /// Gets the maximum allowed length for the PKCE code_challenge parameter.
    /// Fixed at 128 characters per RFC 7636 specification.
    /// </summary>
    public int CodeChallengeMaxLength { get; } = 128;

    /// <summary>
    /// Gets the minimum allowed length for the PKCE code_verifier parameter.
    /// Fixed at 43 characters per RFC 7636 specification.
    /// </summary>
    public int CodeVerifierMinLength { get; } = 43;

    /// <summary>
    /// Gets the maximum allowed length for the PKCE code_verifier parameter.
    /// Fixed at 128 characters per RFC 7636 specification.
    /// </summary>
    public int CodeVerifierMaxLength { get; } = 128;
}
