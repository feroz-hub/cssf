/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Response;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Services;

/// <summary>
/// Service that generates the JSON Web Key Set (JWKS) response per RFC 7517.
/// Extracts public key parameters (RSA modulus/exponent or ECDSA curve points)
/// from the configured signing certificates and formats them as JWK entries.
/// </summary>
internal class JWKSService : SecurityBase, IJWKSService
{
    private readonly Dictionary<string, AsymmetricKeyInfoModel> keyStore;
    private readonly ILoggerService loggerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="JWKSService"/> class.
    /// </summary>
    public JWKSService(
        ILoggerInstance instance,
        Dictionary<string, AsymmetricKeyInfoModel> keyStore)
    {
        this.keyStore = keyStore;
        loggerService = instance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
    }

    /// <summary>
    /// Builds the JWKS response containing all public signing keys.
    /// Supports X.509 certificates (RSA and ECDSA), raw RSA/ECDSA security keys, and JsonWebKey objects.
    /// Each key entry includes key type (kty), usage (sig), algorithm, key ID, and public key parameters.
    /// </summary>
    /// <returns>A list of <see cref="JsonWebKeyResponseModel"/> representing the public key set.</returns>
    public Task<IList<JsonWebKeyResponseModel>> ProcessJWKSInformations()
    {
        loggerService.WriteTo(Log.Debug, "Entered into processing jwks information.");

        var keySet = new List<JsonWebKeyResponseModel>();
        foreach (var keyvalue in keyStore)
        {
            var key = keyvalue.Value.Certificate.GenerateAsymmetricVerificationCredentials(keyvalue.Key);

            if (key.Key is X509SecurityKey x509Key)
            {
                var cert64 = Convert.ToBase64String(x509Key.Certificate.RawData);
                var thumbprint = x509Key.Certificate.GetCertHash().Encode();

                using var rsa = x509Key.Certificate.GetRSAPublicKey();
                if (rsa != null)
                {
                    var parameters = rsa.ExportParameters(false);
                    var exponent = parameters.Exponent.Encode();
                    var modulus = parameters.Modulus.Encode();

                    var rsaJsonWebKey = new JsonWebKeyResponseModel
                    {
                        Kty = "RSA",
                        Use = "sig",
                        Kid = keyvalue.Value.KeyId,
                        X5t = thumbprint,
                        E = exponent,
                        N = modulus,
                        X5c = new[] { cert64 },
                        Alg = key.Algorithm
                    };
                    keySet.Add(rsaJsonWebKey);
                }
                else
                {
                    using var ecdsa = x509Key.Certificate.GetECDsaPublicKey();
                    if (ecdsa != null)
                    {
                        var parameters = ecdsa.ExportParameters(false);
                        var x = parameters.Q.X.Encode();
                        var y = parameters.Q.Y.Encode();

                        var ecdsaJsonWebKey = new JsonWebKeyResponseModel
                        {
                            Kty = "EC",
                            Use = "sig",
                            Kid = keyvalue.Value.KeyId,
                            X5t = thumbprint,
                            X = x,
                            Y = y,
                            Crv = parameters.Curve.GetCrvValueFromCurve(),
                            X5c = new[] { cert64 },
                            Alg = key.Algorithm
                        };
                        keySet.Add(ecdsaJsonWebKey);
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"key type: {x509Key.Certificate.PublicKey.Oid?.FriendlyName ?? "unknown"} not supported.");
                    }
                }
            }
            else if (key.Key is RsaSecurityKey rsaKey)
            {
                var parameters = rsaKey.Rsa?.ExportParameters(false) ?? rsaKey.Parameters;
                var exponent = parameters.Exponent.Encode();
                var modulus = parameters.Modulus.Encode();

                var webKey = new JsonWebKeyResponseModel
                {
                    Kty = "RSA",
                    Use = "sig",
                    Kid = keyvalue.Value.KeyId,
                    E = exponent,
                    N = modulus,
                    Alg = key.Algorithm
                };

                keySet.Add(webKey);
            }
            else if (key.Key is ECDsaSecurityKey ecdsaKey)
            {
                var parameters = ecdsaKey.ECDsa.ExportParameters(false);
                var x = parameters.Q.X.Encode();
                var y = parameters.Q.Y.Encode();

                var ecdsaJsonWebKey = new JsonWebKeyResponseModel
                {
                    Kty = "EC",
                    Use = "sig",
                    Kid = keyvalue.Value.KeyId,
                    X = x,
                    Y = y,
                    Crv = parameters.Curve.GetCrvValueFromCurve(),
                    Alg = key.Algorithm
                };
                keySet.Add(ecdsaJsonWebKey);
            }
            else if (key.Key is JsonWebKey jsonWebKey)
            {
                var webKey = new JsonWebKeyResponseModel
                {
                    Kty = jsonWebKey.Kty,
                    Use = jsonWebKey.Use ?? "sig",
                    Kid = keyvalue.Value.KeyId,
                    X5t = jsonWebKey.X5t,
                    E = jsonWebKey.E,
                    N = jsonWebKey.N,
                    X5c = jsonWebKey.X5c?.Count == 0 ? null : jsonWebKey.X5c.ToArray(),
                    Alg = jsonWebKey.Alg,
                    Crv = jsonWebKey.Crv,
                    X = jsonWebKey.X,
                    Y = jsonWebKey.Y
                };

                keySet.Add(webKey);
            }
        }

        return Task.FromResult<IList<JsonWebKeyResponseModel>>(keySet);
    }
}
