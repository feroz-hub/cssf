/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Text.Json.Serialization;

namespace HCL.CS.SF.Domain.Models.Endpoint.Response;

/// <summary>
/// Represents a single JSON Web Key (JWK) as defined in RFC 7517.
/// Exposed via the OIDC discovery endpoint's jwks_uri so relying parties
/// can retrieve public keys to validate token signatures.
/// </summary>
public class JsonWebKeyResponseModel
{
    /// <summary>The key type (e.g., "RSA", "EC") as defined in RFC 7517 Section 4.1.</summary>
    [JsonPropertyName("kty")] public string Kty { get; set; }

    /// <summary>The intended use of the key: "sig" for signing or "enc" for encryption.</summary>
    [JsonPropertyName("use")] public string Use { get; set; }

    /// <summary>The key identifier used to match a specific key during token signature verification.</summary>
    [JsonPropertyName("kid")] public string Kid { get; set; }

    /// <summary>The X.509 certificate SHA-1 thumbprint, base64url-encoded.</summary>
    [JsonPropertyName("x5t")] public string X5t { get; set; }

    /// <summary>The RSA public exponent, base64url-encoded.</summary>
    [JsonPropertyName("e")] public string E { get; set; }

    /// <summary>The RSA modulus, base64url-encoded.</summary>
    [JsonPropertyName("n")] public string N { get; set; }

    /// <summary>The X.509 certificate chain containing the public key.</summary>
    [JsonPropertyName("x5c")] public IList<string> X5c { get; set; }

    /// <summary>The algorithm intended for use with this key (e.g., "RS256", "ES256").</summary>
    [JsonPropertyName("alg")] public string Alg { get; set; }

    /// <summary>The X coordinate for elliptic curve keys, base64url-encoded.</summary>
    [JsonPropertyName("x")] public string X { get; set; }

    /// <summary>The Y coordinate for elliptic curve keys, base64url-encoded.</summary>
    [JsonPropertyName("y")] public string Y { get; set; }

    /// <summary>The elliptic curve name (e.g., "P-256") for EC key types.</summary>
    [JsonPropertyName("crv")] public string Crv { get; set; }
}
