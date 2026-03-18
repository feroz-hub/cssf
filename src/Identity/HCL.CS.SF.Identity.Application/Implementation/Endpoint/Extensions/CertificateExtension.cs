/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Math;
using HCL.CS.SF.Domain.Models.Endpoint;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Extensions;

/// <summary>
/// Extension methods for X.509 certificate operations used in JWT token signing and verification.
/// Provides methods to generate asymmetric signing/verification credentials from certificates,
/// load certificates from the certificate store, and work with RSA and ECDSA key material.
/// </summary>
internal static class CertificateExtension
{
    // TODO: Need to move constants in this file below to resources.
    private const string InvalidCertificate = "Invalid certificate / No certificate found.";
    private const string UnsupportedAlgorithm = "Algorithm not supported.";
    private const string InvalidCertificateRsa = "Invalid Certificate - No RSA private key found.";
    private const string InvalidCertificatEcdsa = "Invalid Certificate - No ECDSA private key found.";
    private const string EmptySubject = "Subject name is empty in request.";
    private const string CertificateNotFound = "No certificate was found for subject name: ";
    private const string EcdsaCertificatePathInvalid = "ECDSA certificate path is empty.";
    private const string EcdsaCertificatePasswordInvalid = "ECDSA certificate password is empty.";
    private const string EcdsaCertificateEmpty = "ECDsa certificate is empty.";
    private const string EcdsaPublicKeyEmpty = "ECDsa public key is empty.";
    private const string JwtHeaderKeyValueEmpty = "Key value to generate JWT header is empty.";

    /// <summary>
    /// Retrieves the symmetric credential.
    /// </summary>
    /// <param name="clientSecret">The client secret.</param>
    /// <param name="algorithm">The algorithm.</param>
    /// <returns>The operation result.</returns>
    internal static SigningCredentials GetSymmetricCredentials(this string clientSecret, string algorithm)
    {
        SigningCredentials credentials = null;
        if (!string.IsNullOrWhiteSpace(algorithm))
        {
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
    /// Retrieves the asymmetric certificate hash.
    /// </summary>
    /// <returns>The operation result.</returns>
    internal static string GetAsymmetricCertificateHash(this Dictionary<string, AsymmetricKeyInfoModel> keyStore,
        string algorithm)
    {
        var certificateHash = string.Empty;
        if (!string.IsNullOrWhiteSpace(algorithm))
        {
            if (keyStore.Values.Count > 0)
            {
                var certificate = keyStore[algorithm].Certificate;
                if (certificate != null)
                    certificateHash = certificate.Thumbprint;
                //certificateHash = certificate.GetCertHash().Encode();
                else
                    throw new ArgumentNullException(nameof(certificate), "Invalid certificate / No certificate found");
            }
            else
            {
                throw new ArgumentNullException(nameof(keyStore), "No keys found in keyStore");
            }
        }
        else
        {
            throw new ArgumentNullException(nameof(keyStore), "Algorithm not supported.");
        }

        return certificateHash;
    }

    /// <summary>
    /// Retrieves the asymmetric signing credential.
    /// </summary>
    /// <returns>The operation result.</returns>
    internal static SigningCredentials GetAsymmetricSigningCredentials(
        this Dictionary<string, AsymmetricKeyInfoModel> keyStore, string algorithm)
    {
        SigningCredentials credentials = null;
        if (!string.IsNullOrWhiteSpace(algorithm))
        {
            if (keyStore.Values.Count > 0)
            {
                var certificate = keyStore[algorithm].Certificate;
                if (certificate == null) throw new InvalidOperationException(InvalidCertificate);

                credentials = certificate.GenerateAsymmetricSigningCredentials(algorithm);
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
    /// Retrieves the asymmetric verification credential.
    /// </summary>
    /// <returns>The operation result.</returns>
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
    /// Generates asymmetric signing credential.
    /// </summary>
    /// <returns>The operation result.</returns>
    internal static SigningCredentials GenerateAsymmetricSigningCredentials(this X509Certificate2 certificate,
        string algorithm)
    {
        SecurityKey securityKey = null;
        algorithm = algorithm.ToUpper();
        if (algorithm.StartsWith("RS") || algorithm.StartsWith("PS"))
        {
            if (certificate.HasPrivateKey)
            {
                var rsa = certificate.GetRSAPrivateKey();
                if (rsa == null) throw new InvalidOperationException(InvalidCertificateRsa);

                securityKey = new RsaSecurityKey(rsa);
            }
        }
        else if (algorithm.StartsWith("ES"))
        {
            if (certificate.HasPrivateKey)
            {
                var ecdsa = certificate.GetECDsaPrivateKey();
                if (ecdsa == null) throw new InvalidOperationException(InvalidCertificatEcdsa);

                securityKey = new ECDsaSecurityKey(ecdsa);
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
    /// Generates asymmetric verification credential.
    /// </summary>
    /// <returns>The operation result.</returns>
    internal static SigningCredentials GenerateAsymmetricVerificationCredentials(this X509Certificate2 certificate,
        string algorithm)
    {
        SecurityKey securityKey = null;
        algorithm = algorithm.ToUpper();
        if (algorithm.StartsWith("RS") || algorithm.StartsWith("PS"))
        {
            if (certificate != null)
            {
                var rsa = certificate.GetRSAPublicKey();
                if (rsa == null) throw new InvalidOperationException(InvalidCertificateRsa);

                securityKey = new X509SecurityKey(certificate);
            }
        }
        else if (algorithm.StartsWith("ES"))
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
    /// Retrieves the certificate from store.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="location">The location.</param>
    /// <param name="subjectName">The subject name.</param>
    /// <returns>The operation result.</returns>
    internal static X509Certificate2 GetCertificateFromStore(StoreName name, StoreLocation location, string subjectName)
    {
        if (string.IsNullOrWhiteSpace(subjectName)) throw new InvalidOperationException(EmptySubject);

        var store = new X509Store(name, location);
        X509Certificate2Collection certificates = null;
        store.Open(OpenFlags.ReadOnly);
        try
        {
            X509Certificate2 result = null;
            certificates = store.Certificates;
            for (var count = 0; count < certificates.Count; count++)
            {
                var cert = certificates[count];
                if (cert.SubjectName.Name != null && cert.SubjectName.Name.ToLower().Contains(subjectName.ToLower()))
                {
                    result = new X509Certificate2(cert);
                    break;
                }
            }

            if (result == null) throw new InvalidOperationException(CertificateNotFound + subjectName);

            return result;
        }
        finally
        {
            if (certificates != null)
                foreach (var cert in certificates)
                    cert.Reset();

            store.Close();
            store.Dispose();
        }
    }

    /// <summary>
    /// Retrieves the ecdsa public signing ke.
    /// </summary>
    /// <param name="ecdsaCertificatePath">The ecdsa certificate path.</param>
    /// <param name="password">The password.</param>
    /// <returns>The operation result.</returns>
    internal static SecurityKey GetEcdsaPublicSigningKey(string ecdsaCertificatePath, string password)
    {
        if (string.IsNullOrWhiteSpace(ecdsaCertificatePath))
            throw new InvalidOperationException(EcdsaCertificatePathInvalid);

        if (string.IsNullOrWhiteSpace(password)) throw new InvalidOperationException(EcdsaCertificatePasswordInvalid);

        var certificate = new X509Certificate2(ecdsaCertificatePath, password,
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
        return GetEcdsaPublicSigningKey(certificate);
    }

    /// <summary>
    /// Retrieves the ecdsa public signing ke.
    /// </summary>
    /// <param name="certificate">The certificate.</param>
    /// <returns>The operation result.</returns>
    internal static SecurityKey GetEcdsaPublicSigningKey(X509Certificate2 certificate)
    {
        if (certificate == null) throw new InvalidOperationException(EcdsaCertificateEmpty);

        var publicKey = certificate.GetECDsaPublicKey();
        SecurityKey rsaKey = new ECDsaSecurityKey(publicKey);
        return rsaKey;
    }

    /// <summary>
    /// Retrieves the ecdsa public signing ke.
    /// </summary>
    /// <param name="publicKey">The public key.</param>
    /// <returns>The operation result.</returns>
    internal static SecurityKey GetEcdsaPublicSigningKey(byte[] publicKey)
    {
        if (publicKey.Length == 0) throw new InvalidOperationException(EcdsaPublicKeyEmpty);

        var ecdsa = LoadEcdsaPublicKey(publicKey);
        var rsaKey = new ECDsaSecurityKey(ecdsa);
        return rsaKey;
    }

    /// <summary>
    /// Performs the from hex string operation.
    /// </summary>
    /// <param name="hex">The hex.</param>
    /// <returns>The operation result.</returns>
    internal static byte[] FromHexString(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) throw new InvalidOperationException(JwtHeaderKeyValueEmpty);

        var numberChars = hex.Length;
        var hexAsBytes = new byte[numberChars / 2];
        for (var count = 0; count < numberChars; count += 2)
            hexAsBytes[count / 2] = Convert.ToByte(hex.Substring(count, 2), 16);

        return hexAsBytes;
    }

    /// <summary>
    /// Loads the ecdsa public ke.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The operation result.</returns>
    internal static ECDsa LoadEcdsaPublicKey(byte[] key)
    {
        var pubKeyX = key.Skip(1).Take(32).ToArray();
        var pubKeyY = key.Skip(33).ToArray();

        return ECDsa.Create(new ECParameters
        {
            Curve = ECCurve.NamedCurves.nistP256,
            Q = new ECPoint
            {
                X = pubKeyX,
                Y = pubKeyY
            }
        });
    }

    /// <summary>
    /// Loads the ecdsa private ke.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The operation result.</returns>
    internal static ECDsa LoadEcdsaPrivateKey(byte[] key)
    {
        var mathKey = new BigInteger(+1, key);
        var parameters = SecNamedCurves.GetByName("secp256r1"); // TODO: Needs to be corrected.
        var ecPoint = parameters.G.Multiply(mathKey);
        var keyX = ecPoint.Normalize().XCoord.ToBigInteger().ToByteArrayUnsigned();
        var keyY = ecPoint.Normalize().YCoord.ToBigInteger().ToByteArrayUnsigned();

        return ECDsa.Create(new ECParameters
        {
            Curve = ECCurve.NamedCurves.nistP256,
            D = mathKey.ToByteArrayUnsigned(),
            Q = new ECPoint
            {
                X = keyX,
                Y = keyY
            }
        });
    }
}
