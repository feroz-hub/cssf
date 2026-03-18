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
using Microsoft.IdentityModel.Tokens;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
// TODO: Need to move hardcoded text in this file below to resources. Currently FrameworkResultService is an interface but this file is an extension.

/// <summary>
/// JWT token utility extension methods for token signing, verification, and header generation.
/// Supports symmetric (HMAC) and asymmetric (RSA, ECDSA) token operations,
/// including JWT validation with configurable issuer, audience, and lifetime checks.
/// </summary>
internal static class TokenExtension
{
    // TODO: Need to move constants in this file below to resources.
    private const string CertificateExpired = "Certificate has expired.";
    private const string NoRsaKeyFound = "Invalid Certificate - No RSA private key found.";
    private const string NoEcdsaKeyFound = "Invalid Certificate - No ECDSA private key found.";
    private const string RsaXmlEmpty = "RSA xml content is empty.";
    private const string EmptySigningAlgorithm = "Signing algorithm is empty.";

    /// <summary>
    /// Retrieves the token ke.
    /// </summary>
    /// <returns>The operation result.</returns>
    internal static SecurityKey GetTokenKey(this Dictionary<string, AsymmetricKeyInfoModel> keyStore,
        ClientsModel client)
    {
        if (!string.IsNullOrWhiteSpace(client.AllowedSigningAlgorithm))
        {
            if (client.AllowedSigningAlgorithm.StartsWith("HS"))
            {
                var signingCredentials = client.ClientSecret.GetSymmetricCredentials(client.AllowedSigningAlgorithm);

                if (signingCredentials != null) return signingCredentials.Key;
            }
            else if (client.AllowedSigningAlgorithm.StartsWith("RS") ||
                     client.AllowedSigningAlgorithm.StartsWith("PS") ||
                     client.AllowedSigningAlgorithm.StartsWith("ES"))
            {
                if (keyStore.Values.Count > 0)
                {
                    var signingCredentials =
                        keyStore.GetAsymmetricVerificationCredentials(client.AllowedSigningAlgorithm);
                    if (signingCredentials != null) return signingCredentials.Key;
                }
            }
        }
        else
        {
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
    /// Generates toke.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="payload">The payload.</param>
    /// <returns>The operation result.</returns>
    internal static string GenerateToken(this JwtHeader header, JwtPayload payload)
    {
        var secToken = new JwtSecurityToken(header, payload);
        var handler = new JwtSecurityTokenHandler();
        var tokenString = handler.WriteToken(secToken);
        if (header.Alg == "none" && tokenString.Substring(tokenString.Length - 1, 1) == ".")
            tokenString = tokenString.Remove(tokenString.Length - 1, 1);

        return tokenString;
    }

    /// <summary>
    /// Validates a JWT token using symmetric key (HMAC) signing, verifying issuer, audience, and lifetime.
    /// </summary>
    /// <param name="token">The JWT token string to validate.</param>
    /// <param name="key">The symmetric secret key used for validation.</param>
    /// <param name="issuer">The expected token issuer.</param>
    /// <param name="audience">The expected token audience.</param>
    /// <returns>A tuple of the parsed <see cref="JwtSecurityToken"/> and the validated <see cref="ClaimsPrincipal"/>.</returns>
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
    /// Validates a client_secret_jwt assertion token using the client secret as the symmetric key.
    /// </summary>
    /// <param name="token">The JWT token string to validate.</param>
    /// <param name="clientSecret">The client secret used as the symmetric signing key.</param>
    /// <param name="issuer">The expected token issuer (client_id).</param>
    /// <param name="audience">The expected token audience (token endpoint URL).</param>
    /// <returns>A tuple of the parsed <see cref="JwtSecurityToken"/> and the validated <see cref="ClaimsPrincipal"/>.</returns>
    internal static (JwtSecurityToken, ClaimsPrincipal) ValidateClientSecretJwtToken(this string token,
        string clientSecret, string issuer, string audience)
    {
        var securityKey = Encoding.ASCII.GetBytes(clientSecret);
        var validationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(securityKey),
            ValidateLifetime = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true
        };
        var principal =
            new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out var rawValidatedToken);
        return ((JwtSecurityToken)rawValidatedToken, principal);
    }

    /// <summary>
    /// Validates a JWT token using asymmetric (RSA/ECDSA) key signing, verifying issuer, audience, and lifetime.
    /// </summary>
    /// <param name="token">The JWT token string to validate.</param>
    /// <param name="signingKey">The asymmetric public key used for signature verification.</param>
    /// <param name="issuer">The expected token issuer.</param>
    /// <param name="audience">The expected token audience.</param>
    /// <returns>A tuple of the parsed <see cref="JwtSecurityToken"/> and the validated <see cref="ClaimsPrincipal"/>.</returns>
    internal static (JwtSecurityToken, ClaimsPrincipal) ValidateAsymmetricToken(this string token,
        SecurityKey signingKey, string issuer, string audience)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            TryAllIssuerSigningKeys = true,
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
    /// Generates symmetric jwt header.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="algorithm">The algorithm.</param>
    /// <returns>The operation result.</returns>
    internal static JwtHeader GenerateSymmetricJwtHeader(SecurityKey key, string algorithm)
    {
        var signingCredentials = new SigningCredentials(key, algorithm);
        return new JwtHeader(signingCredentials);
    }

    /// <summary>
    /// Reads the x509 certificate.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="password">The password.</param>
    /// <returns>The operation result.</returns>
    internal static X509Certificate2 ReadX509Certificate(string path, string password)
    {
        var certificate = new X509Certificate2(path, password,
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
        if (certificate.NotAfter < DateTime.UtcNow) throw new InvalidOperationException(CertificateExpired);

        return certificate;
    }

    /// <summary>
    /// Generates rsa asymmetric jwt header.
    /// </summary>
    /// <param name="certificate">The certificate.</param>
    /// <param name="algorithm">The algorithm.</param>
    /// <returns>The operation result.</returns>
    internal static JwtHeader GenerateRsaAsymmetricJwtHeader(X509Certificate2 certificate, string algorithm)
    {
        using var rsa = certificate.GetRSAPrivateKey();
        if (rsa == null) throw new InvalidOperationException(NoRsaKeyFound);

        SecurityKey securityKey = new RsaSecurityKey(rsa);
        var signingCredentials = new SigningCredentials(securityKey, algorithm);
        var header = new JwtHeader(signingCredentials);
        return header;
    }

    /// <summary>
    /// Generates e c dsa asymmetric jwt header.
    /// </summary>
    /// <param name="certificate">The certificate.</param>
    /// <param name="algorithm">The algorithm.</param>
    /// <returns>The operation result.</returns>
    internal static JwtHeader GenerateECDsaAsymmetricJwtHeader(X509Certificate2 certificate, string algorithm)
    {
        using var ecdsa = certificate.GetECDsaPrivateKey();
        if (ecdsa == null) throw new InvalidOperationException(NoEcdsaKeyFound);

        SecurityKey securityKey = new ECDsaSecurityKey(ecdsa);
        var signingCredentials = new SigningCredentials(securityKey, algorithm);
        var header = new JwtHeader(signingCredentials);
        return header;
    }

    /// <summary>
    /// Generates jwt header without algorithm.
    /// </summary>
    /// <returns>The operation result.</returns>
    internal static JwtHeader GenerateJwtHeaderWithoutAlgorithm()
    {
        var header = new JwtHeader
        {
            ["alg"] = "none",
            ["typ"] = "at+jwt"
        };
        return header;
    }

    /// <summary>
    /// Generates rsa jwt header.
    /// </summary>
    /// <param name="privateRsaXml">The private rsa xml.</param>
    /// <param name="algorithm">The algorithm.</param>
    /// <returns>The operation result.</returns>
    internal static JwtHeader GenerateRsaJwtHeader(string privateRsaXml, string algorithm)
    {
        if (string.IsNullOrWhiteSpace(privateRsaXml)) throw new InvalidOperationException(RsaXmlEmpty);

        if (string.IsNullOrWhiteSpace(algorithm)) throw new InvalidOperationException(EmptySigningAlgorithm);

        var privateKey = privateRsaXml.GetRsaSecurityKey();

        var signingCredentials = new SigningCredentials(privateKey, algorithm);
        return new JwtHeader(signingCredentials);
    }

    /// <summary>
    /// Generates ecdsa jwt header.
    /// </summary>
    /// <param name="privateKey">The private key.</param>
    /// <param name="algorithm">The algorithm.</param>
    /// <returns>The operation result.</returns>
    internal static JwtHeader GenerateEcdsaJwtHeader(string privateKey, string algorithm)
    {
        using var ecdsa = CertificateExtension.LoadEcdsaPrivateKey(CertificateExtension.FromHexString(privateKey));
        SecurityKey securityKey = new ECDsaSecurityKey(ecdsa);
        var signingCredentials = new SigningCredentials(securityKey, algorithm);
        var header = new JwtHeader(signingCredentials);
        return header;
    }

    /// <summary>
    /// Performs the custom lifetime validator operation.
    /// </summary>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    internal static bool CustomLifetimeValidator(DateTime? notBefore, DateTime? expires, SecurityToken tokenToValidate,
        TokenValidationParameters param)
    {
        if (expires != null) return expires > DateTime.UtcNow;

        return false;
    }
}
