/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint;

namespace HCL.CS.SF.ProxyService.Routes.Extension;

/// <summary>
/// Provides extension methods for JSON serialization/deserialization, HTTP response writing,
/// cryptographic key handling, JWT token validation, and string conversion utilities
/// used across the API gateway routing layer.
/// </summary>
internal static class RouteExtension
{
    private const string InvalidCertificate = "Invalid certificate / No certificate found.";
    private const string UnsupportedAlgorithm = "Algorithm not supported.";
    private const string InvalidCertificateRsa = "Invalid Certificate - No RSA private key found.";
    private const string InvalidCertificatEcdsa = "Invalid Certificate - No ECDSA private key found.";

    /// <summary>
    /// JSON serializer options for API response output, suppressing null properties.
    /// </summary>
    private static readonly JsonSerializerOptions ResponseSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// JSON serializer options for request deserialization with null suppression
    /// and read-only property exclusion.
    /// </summary>
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        MaxDepth = 0,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        IgnoreReadOnlyProperties = true
    };

    /// <summary>
    /// Writes the given object as a JSON response body with UTF-8 encoding.
    /// </summary>
    /// <param name="response">The HTTP response to write to.</param>
    /// <param name="content">The object to serialize as JSON.</param>
    /// <param name="contentType">Optional content type override (defaults to application/json).</param>
    internal static async Task WriteResponseJsonAsync(this HttpResponse response, object content,
        string contentType = null)
    {
        var json = JsonSerializer.Serialize(content, ResponseSerializerOptions);

        response.ContentType = contentType ?? "application/json; charset=UTF-8";
        await response.WriteAsync(json);
    }

    /// <summary>
    /// Serializes an object to a JSON string using the standard serializer options.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="request">The object to serialize.</param>
    /// <returns>The JSON string representation.</returns>
    internal static string JsonSerialize<T>(T request)
    {
        return JsonSerializer.Serialize(request, SerializerOptions);
    }

    /// <summary>
    /// Deserializes a JSON string into the specified type using the standard serializer options.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="request">The JSON string to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    internal static T JsonDeserialize<T>(this string request)
    {
        return JsonSerializer.Deserialize<T>(request, SerializerOptions);
    }

    /// <summary>
    /// Creates symmetric (HMAC) signing credentials from a client secret for the given algorithm.
    /// Only HS* (HMAC-SHA) algorithms are supported.
    /// </summary>
    /// <param name="clientSecret">The client's shared secret.</param>
    /// <param name="algorithm">The signing algorithm (e.g., HS256).</param>
    /// <returns>The signing credentials, or null if no algorithm is specified.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the secret or algorithm is invalid.</exception>
    internal static SigningCredentials GetSymmetricCredentials(this string clientSecret, string algorithm)
    {
        SigningCredentials credentials = null;
        if (!string.IsNullOrWhiteSpace(algorithm))
        {
            // Only HMAC-SHA algorithms (HS256, HS384, HS512) use symmetric keys.
            if (algorithm.StartsWith("HS"))
            {
                if (!string.IsNullOrWhiteSpace(clientSecret))
                {
                    var securityKey = Encoding.ASCII.GetBytes(clientSecret);
                    credentials = new SigningCredentials(new SymmetricSecurityKey(securityKey), algorithm);
                }
                else
                {
                    throw new ArgumentNullException(nameof(clientSecret), "No client secret found");
                }
            }
            else
            {
                throw new ArgumentNullException(nameof(algorithm), "Invalid Algorithm");
            }
        }

        return credentials;
    }

    /// <summary>
    /// Retrieves asymmetric signing credentials from the key store for the specified algorithm.
    /// Looks up the X.509 certificate associated with the algorithm and generates
    /// verification credentials from it.
    /// </summary>
    /// <param name="keyStore">The dictionary mapping algorithms to their asymmetric key info.</param>
    /// <param name="algorithm">The signing algorithm (e.g., RS256, ES256).</param>
    /// <returns>The signing credentials, or null if no algorithm is specified.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the certificate is missing or the algorithm is unsupported.</exception>
    internal static SigningCredentials GetAsymmetricVerificationCredentials(
        this Dictionary<string, AsymmetricKeyInfoModel> keyStore, string algorithm)
    {
        SigningCredentials credentials = null;
        if (!string.IsNullOrWhiteSpace(algorithm))
        {
            if (keyStore.Values.Count > 0)
            {
                var certificate = keyStore[algorithm].Certificate;
                if (certificate == null) throw new InvalidOperationException(InvalidCertificate);

                credentials = certificate.GenerateAsymmetricVerificationCredentials(algorithm);
                // Assign the configured key ID for JWK matching.
                if (credentials?.Key != null && !string.IsNullOrWhiteSpace(keyStore[algorithm].KeyId))
                    credentials.Key.KeyId = keyStore[algorithm].KeyId;
            }
            else
            {
                throw
                    new InvalidOperationException(
                        UnsupportedAlgorithm); // TODO: Talk to team on correcting this message text.
            }
        }

        return credentials;
    }

    /// <summary>
    /// Generates asymmetric signing credentials from an X.509 certificate for the specified algorithm.
    /// Supports RSA-SHA256 and ECDSA-SHA256 algorithms.
    /// </summary>
    /// <param name="certificate">The X.509 certificate containing the public key.</param>
    /// <param name="algorithm">The signing algorithm.</param>
    /// <returns>The signing credentials backed by the certificate's key.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the certificate lacks the required key type.</exception>
    internal static SigningCredentials GenerateAsymmetricVerificationCredentials(this X509Certificate2 certificate,
        string algorithm)
    {
        SecurityKey securityKey = null;
        if (string.Equals(algorithm, OpenIdConstants.Algorithms.RsaSha256, StringComparison.Ordinal))
        {
            if (certificate != null)
            {
                var rsa = certificate.GetRSAPublicKey();
                if (rsa == null) throw new InvalidOperationException(InvalidCertificateRsa);

                securityKey = new X509SecurityKey(certificate);
            }
        }
        else if (string.Equals(algorithm, OpenIdConstants.Algorithms.EcdsaSha256, StringComparison.Ordinal))
        {
            if (certificate != null)
            {
                var ecdsa = certificate.GetECDsaPublicKey();
                if (ecdsa == null) throw new InvalidOperationException(InvalidCertificatEcdsa);

                securityKey = new X509SecurityKey(certificate);
            }
        }
        else
        {
            throw new InvalidOperationException(UnsupportedAlgorithm);
        }

        var signingCredentials = new SigningCredentials(securityKey, algorithm);
        return signingCredentials;
    }

    /// <summary>
    /// Resolves the appropriate security key for validating a client's access tokens
    /// based on the client's configured signing algorithm. Falls back to RSA-SHA256
    /// when no algorithm is explicitly configured.
    /// </summary>
    /// <param name="keyStore">The key store containing asymmetric key information by algorithm.</param>
    /// <param name="client">The client entity whose signing algorithm to use.</param>
    /// <returns>The security key for token validation, or null if unavailable.</returns>
    internal static SecurityKey GetTokenKey(this Dictionary<string, AsymmetricKeyInfoModel> keyStore, Clients client)
    {
        if (!string.IsNullOrWhiteSpace(client.AllowedSigningAlgorithm))
        {
            // Use the client's explicitly configured algorithm.
            if (string.Equals(client.AllowedSigningAlgorithm, OpenIdConstants.Algorithms.RsaSha256,
                    StringComparison.Ordinal)
                || string.Equals(client.AllowedSigningAlgorithm, OpenIdConstants.Algorithms.EcdsaSha256,
                    StringComparison.Ordinal))
            {
                if (keyStore.Values.Count <= 0) return null;

                var signingCredentials = keyStore.GetAsymmetricVerificationCredentials(client.AllowedSigningAlgorithm);
                if (signingCredentials != null) return signingCredentials.Key;

                return null;
            }
        }
        else
        {
            // Default to RSA-SHA256 when no algorithm is specified on the client.
            if (keyStore.Values.Count > 0)
            {
                var signingCredentials =
                    keyStore.GetAsymmetricVerificationCredentials(OpenIdConstants.Algorithms.RsaSha256);
                if (signingCredentials != null) return signingCredentials.Key;
            }
        }

        return null;
    }

    /// <summary>
    /// Validates a JWT using an asymmetric signing key. Verifies issuer, audience,
    /// signing key, and lifetime. Uses a custom lifetime validator that checks expiration
    /// against UTC now.
    /// </summary>
    /// <param name="token">The raw JWT string.</param>
    /// <param name="signingKey">The asymmetric security key for signature verification.</param>
    /// <param name="issuer">The expected token issuer.</param>
    /// <param name="audience">The expected token audience.</param>
    /// <returns>A tuple of the validated JWT and the resulting claims principal.</returns>
    internal static (JwtSecurityToken, ClaimsPrincipal) ValidateAsymmetricToken(this string token,
        SecurityKey signingKey, string issuer, string audience)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            LifetimeValidator = CustomLifetimeValidator,
            CryptoProviderFactory = new CryptoProviderFactory
            {
                CacheSignatureProviders = false
            }
        };

        var principal =
            new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out var rawValidatedToken);
        return ((JwtSecurityToken)rawValidatedToken, principal);
    }

    /// <summary>
    /// Validates a JWT using a symmetric (HMAC) key. Verifies issuer, audience,
    /// signing key, and lifetime.
    /// </summary>
    /// <param name="token">The raw JWT string.</param>
    /// <param name="key">The shared secret key.</param>
    /// <param name="issuer">The expected token issuer.</param>
    /// <param name="audience">The expected token audience.</param>
    /// <returns>A tuple of the validated JWT and the resulting claims principal.</returns>
    internal static (JwtSecurityToken, ClaimsPrincipal) ValidateSymmetricJwtToken(this string token, string key,
        string issuer, string audience)
    {
        var securityKey = Encoding.ASCII.GetBytes(key);
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(securityKey),
            ValidateLifetime = true,
            RequireExpirationTime = true,
            LifetimeValidator = CustomLifetimeValidator,
            CryptoProviderFactory = new CryptoProviderFactory
            {
                CacheSignatureProviders = false
            }
        };
        var principal =
            new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out var rawValidatedToken);
        return ((JwtSecurityToken)rawValidatedToken, principal);
    }

    /// <summary>
    /// Custom lifetime validator that checks whether the token's expiration time is
    /// in the future relative to UTC now. Returns false if no expiration is set.
    /// </summary>
    /// <param name="notBefore">The "not before" time from the token (unused).</param>
    /// <param name="expires">The expiration time from the token.</param>
    /// <param name="tokenToValidate">The token being validated (unused).</param>
    /// <param name="param">The validation parameters (unused).</param>
    /// <returns><c>true</c> if the token has not expired; otherwise, <c>false</c>.</returns>
    internal static bool CustomLifetimeValidator(DateTime? notBefore, DateTime? expires, SecurityToken tokenToValidate,
        TokenValidationParameters param)
    {
        if (expires != null) return expires > DateTime.UtcNow;

        return false;
    }

    /// <summary>
    /// Converts a collection of strings into a single space-separated string,
    /// following the OAuth 2.0 convention for representing scopes.
    /// </summary>
    /// <param name="list">The list of strings to join.</param>
    /// <returns>A space-separated string, or empty if the list is null.</returns>
    internal static string ConvertSpaceSeparatedString(this IEnumerable<string> list)
    {
        if (list == null) return string.Empty;

        var sb = new StringBuilder(100);
        foreach (var element in list) sb.Append(element + " ");

        return sb.ToString().Trim();
    }
}
