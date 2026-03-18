/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Configurations.Endpoint;

/// <summary>
/// Configuration settings for authentication-related security policies on OAuth/OIDC endpoints.
/// Bound from the "Authentication" section of the endpoint configuration.
/// </summary>
public class AuthenticationConfig
{
    /// <summary>
    /// Gets or sets whether a Content-Security-Policy frame-src directive is required during sign-out.
    /// When true, the server includes a CSP header that restricts iframe sources to the client's
    /// post-logout redirect URI, preventing clickjacking attacks during front-channel logout.
    /// Defaults to true.
    /// </summary>
    public bool RequireCspFrameSrcForSignout { get; set; } = true;
}
