/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Cryptography.X509Certificates;
using HCL.CS.SF.Domain.Enums;

namespace HCL.CS.SF.Domain.Models.Endpoint;

/// <summary>
/// Holds metadata about an asymmetric signing key used by the identity server
/// to sign JWTs (access tokens, identity tokens, logout tokens).
/// The key information is exposed via the JWKS endpoint for token verification.
/// </summary>
public class AsymmetricKeyInfoModel
{
    /// <summary>The key identifier (kid) used to match this key in JWT headers.</summary>
    public string KeyId { get; set; }

    /// <summary>The signing algorithm (e.g., RS256, ES256) used with this key.</summary>
    public SigningAlgorithm Algorithm { get; set; }

    /// <summary>The X.509 certificate containing the public/private key pair for token signing.</summary>
    public X509Certificate2 Certificate { get; set; }
}
