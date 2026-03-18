/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.DomainServices.Infra;
using static HCL.CS.SF.Domain.Constants.Endpoint.OpenIdConstants;

namespace HCL.CS.SF.Infrastructure.Resources;

/// <summary>
/// Manages asymmetric cryptographic keys used for token signing and validation.
/// Validates certificates, checks algorithm compatibility, and maintains an in-memory key store
/// indexed by signing algorithm name.
/// </summary>
internal class KeyStore : IKeyStore
{
    /// <summary>
    /// Validates and adds a collection of asymmetric key information entries into a dictionary keyed by algorithm name.
    /// Each certificate is checked for a private key, expiration, supported algorithm, and algorithm-certificate compatibility.
    /// </summary>
    /// <param name="asymmetricKeyInfo">The collection of asymmetric key entries to validate and store.</param>
    /// <returns>A dictionary mapping uppercase algorithm names (e.g., "RS256") to their corresponding <see cref="AsymmetricKeyInfoModel"/>.</returns>
    /// <exception cref="Exception">
    /// Thrown when a certificate lacks a private key, or when a duplicate algorithm is added.
    /// </exception>
    public Dictionary<string, AsymmetricKeyInfoModel> Add(IEnumerable<AsymmetricKeyInfoModel> asymmetricKeyInfo)
    {
        var keyStore = new Dictionary<string, AsymmetricKeyInfoModel>();
        var validationMessage = string.Empty;
        var securityKeys = asymmetricKeyInfo.ToList();

        if (securityKeys.Any())
            foreach (var securityKey in securityKeys)
            {
                // Accumulate an error if the certificate does not contain a private key
                if (!securityKey.Certificate.HasPrivateKey)
                    validationMessage += securityKey.KeyId + Environment.NewLine + securityKey.Algorithm;

                // Run additional validity checks (expiration, algorithm support, cert-algorithm match)
                validationMessage += CheckCertificateValidity(securityKey);

                if (string.IsNullOrWhiteSpace(validationMessage))
                {
                    // Resolve the enum value to its uppercase string name for use as the dictionary key
                    var name = Enum.GetName(typeof(SigningAlgorithm), securityKey.Algorithm)?.ToUpper();

                    if (name != null && !keyStore.ContainsKey(name))
                        keyStore.Add(name, securityKey);
                    else
                        throw new Exception("Same Key/algorithm already added");
                }
            }

        if (!string.IsNullOrWhiteSpace(validationMessage))
            throw new Exception("Certificate does not have a private key :" + validationMessage);

        return keyStore;
    }

    /// <summary>
    /// Determines whether the specified algorithm string matches any of the statically defined algorithm constants.
    /// </summary>
    /// <param name="algorithm">The algorithm name to look up (e.g., "RS256").</param>
    /// <returns><c>true</c> if the algorithm exists among the <see cref="Algorithms"/> constants; otherwise, <c>false</c>.</returns>
    public static bool AlgorithmExists(string algorithm)
    {
        var algorithmExists = false;
        var type = typeof(Algorithms);
        // Reflect over all public static fields on the Algorithms class to find a matching value
        var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
        foreach (var fi in fields)
            if (fi.GetValue(null)?.ToString() == algorithm)
            {
                algorithmExists = true;
                break;
            }

        return algorithmExists;
    }

    /// <summary>
    /// Performs a series of validity checks on a certificate: expiration, algorithm support, and certificate-algorithm compatibility.
    /// </summary>
    /// <param name="securityKey">The asymmetric key entry whose certificate is validated.</param>
    /// <returns>A concatenated string of validation error messages. Empty if the certificate is valid.</returns>
    private static string CheckCertificateValidity(AsymmetricKeyInfoModel securityKey)
    {
        var validationMessage = string.Empty;
        if (securityKey.Certificate.NotAfter < DateTime.UtcNow) validationMessage += "Certificate Expired";

        var name = Enum.GetName(typeof(SigningAlgorithm), securityKey.Algorithm)?.ToUpper();
        if (!AlgorithmExists(name)) validationMessage += "Algorithm type not supported";

        if (!VerifyCertificate(securityKey)) validationMessage += "Certificate and Algorithm type mismatch";

        return validationMessage;
    }

    /// <summary>
    /// Verifies that the certificate's key type matches the expected signing algorithm family.
    /// RSA-based algorithms (RS*, PS*) require an RSA private key; ECDSA algorithms (ES*) require an ECDsa private key.
    /// </summary>
    /// <param name="securityKey">The asymmetric key entry to verify.</param>
    /// <returns><c>true</c> if the certificate contains a private key matching the algorithm family; otherwise, <c>false</c>.</returns>
    private static bool VerifyCertificate(AsymmetricKeyInfoModel securityKey)
    {
        var result = false;
        var name = Enum.GetName(typeof(SigningAlgorithm), securityKey.Algorithm)?.ToUpper();

        // RSA and RSA-PSS algorithms require an RSA private key
        if (name != null && (name.StartsWith("RS") || name.StartsWith("PS")))
        {
            using var privateKey = securityKey.Certificate.GetRSAPrivateKey();
            if (privateKey != null) result = true;
        }
        // ECDSA algorithms require an ECDsa private key
        else if (name != null && name.StartsWith("ES"))
        {
            using var privateKey = securityKey.Certificate.GetECDsaPrivateKey();
            if (privateKey != null) result = true;
        }

        return result;
    }
}
