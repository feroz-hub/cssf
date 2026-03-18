/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Cryptography;
using System.Text;
using IdentityModel;
using Microsoft.IdentityModel.Tokens;
using HCL.CS.SF.Domain.Constants.Endpoint;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
// TODO: Need to move hardcoded data in this file below to resources. Currently FrameworkResultService is an interface but this file is an extension.

/// <summary>
/// Cryptographic utility extension methods for the OAuth/OIDC endpoint layer.
/// Provides AES encryption/decryption, SHA-256/SHA-512 hashing, base64url encoding/decoding,
/// PKCE hash computation, random string/byte generation, and JWT claim hash creation.
/// </summary>
public static class EncryptionExtension
{
    private const int LegacyPbkdf2Iterations = 1000;
    private const string IllegalStringToDecode = "Illegal base64url string specified for decoding.";
    private const string RsaXmlEmpty = "Rsa XML content is empty.";
    private const string InvalidSigningAlgorithm = "Invalid signing algorithm: ";

    /// <summary>
    /// Performs the aes encrypt operation.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="key">The key.</param>
    /// <param name="keySize">The key size.</param>
    /// <returns>The operation result.</returns>
    internal static string AesEncrypt(this string data, string key, int keySize)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var keyHashBytes = SHA256.Create().ComputeHash(keyBytes);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        var saltBytes = keySize.RandomBytes();
        var encryptedBytes = new byte[saltBytes.Length + dataBytes.Length];

        // Combine Salt + Text
        for (var i = 0; i < saltBytes.Length; i++) encryptedBytes[i] = saltBytes[i];

        for (var i = 0; i < dataBytes.Length; i++) encryptedBytes[i + saltBytes.Length] = dataBytes[i];

        encryptedBytes = AesEncrypt(encryptedBytes, keyHashBytes);
        return Convert.ToBase64String(encryptedBytes);
    }

    /// <summary>
    /// Performs the aes encrypt operation.
    /// </summary>
    /// <param name="dataBytes">The data bytes.</param>
    /// <param name="keyBytes">The key bytes.</param>
    /// <returns>The operation result.</returns>
    internal static byte[] AesEncrypt(byte[] dataBytes, byte[] keyBytes)
    {
        var saltBytes = Encoding.ASCII.GetBytes("iavcpquyxdaxganojgytbfhitzpptqbn");
        using var ms = new MemoryStream();
        //using RijndaelManaged aes = new RijndaelManaged();
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.BlockSize = 128;

        var key = new Rfc2898DeriveBytes(keyBytes, saltBytes, LegacyPbkdf2Iterations, HashAlgorithmName.SHA1);
        aes.Key = key.GetBytes(aes.KeySize / 8);
        aes.IV = key.GetBytes(aes.BlockSize / 8);

        aes.Mode = CipherMode.CBC;
        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
        {
            cs.Write(dataBytes, 0, dataBytes.Length);
            cs.Close();
        }

        return ms.ToArray();
    }

    /// <summary>
    /// Performs the aes decrypt operation.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="key">The key.</param>
    /// <param name="keySize">The key size.</param>
    /// <returns>The operation result.</returns>
    internal static string AesDecrypt(this string data, string key, int keySize)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var keyHashBytes = SHA256.Create().ComputeHash(keyBytes);
        var dataBytes = Convert.FromBase64String(data);
        var decryptedbytes = AesDecrypt(dataBytes, keyHashBytes);

        var saltLength = keySize;
        var resultbytes = new byte[decryptedbytes.Length - saltLength];
        for (var i = 0; i < resultbytes.Length; i++) resultbytes[i] = decryptedbytes[i + saltLength];

        return Encoding.UTF8.GetString(resultbytes);
    }

    /// <summary>
    /// Performs the aes decrypt operation.
    /// </summary>
    /// <param name="dataBytes">The data bytes.</param>
    /// <param name="keyBytes">The key bytes.</param>
    /// <returns>The operation result.</returns>
    internal static byte[] AesDecrypt(byte[] dataBytes, byte[] keyBytes)
    {
        var saltBytes = Encoding.ASCII.GetBytes("iavcpquyxdaxganojgytbfhitzpptqbn");
        using var ms = new MemoryStream();
        //using RijndaelManaged aes = new RijndaelManaged();
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.BlockSize = 128;

        var key = new Rfc2898DeriveBytes(keyBytes, saltBytes, LegacyPbkdf2Iterations, HashAlgorithmName.SHA1);
        aes.Key = key.GetBytes(aes.KeySize / 8);
        aes.IV = key.GetBytes(aes.BlockSize / 8);

        aes.Mode = CipherMode.CBC;

        using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
        {
            cs.Write(dataBytes, 0, dataBytes.Length);
            cs.Close();
        }

        return ms.ToArray();
    }

    /// <summary>
    /// Performs the sha256 operation.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>The operation result.</returns>
    public static string Sha256(this string input)
    {
        if (!string.IsNullOrWhiteSpace(input))
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha.ComputeHash(bytes);

            return Convert.ToBase64String(hash);
        }

        return string.Empty;
    }

    /// <summary>
    /// Performs the sha256 operation.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>The operation result.</returns>
    public static byte[] Sha256(this byte[] input)
    {
        if (input != null)
        {
            using var sha = SHA256.Create();
            return sha.ComputeHash(input);
        }

        return Array.Empty<byte>();
    }

    /// <summary>
    /// Performs the sha512 operation.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>The operation result.</returns>
    internal static string Sha512(this string input)
    {
        if (!string.IsNullOrWhiteSpace(input))
        {
            using var sha = SHA512.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha.ComputeHash(bytes);

            return Convert.ToBase64String(hash);
        }

        return string.Empty;
    }

    /// <summary>
    /// Performs the encode operation.
    /// </summary>
    /// <param name="arg">The arg.</param>
    /// <returns>The operation result.</returns>
    internal static string Encode(this byte[] arg)
    {
        var val = Convert.ToBase64String(arg); // Standard base64 encoder
        val = val.Split('=')[0]; // Remove any trailing '='s
        val = val.Replace('+', '-'); // 62nd char of encoding
        val = val.Replace('/', '_'); // 63rd char of encoding
        return val;
    }

    /// <summary>
    /// Performs the encode operation.
    /// </summary>
    /// <param name="inputs">The inputs.</param>
    /// <returns>The operation result.</returns>
    internal static string Encode(List<string> inputs)
    {
        if (inputs.ContainsAny())
        {
            var value = HttpContextExtension.JsonSerialize(inputs);
            var bytes = Encoding.UTF8.GetBytes(value);
            value = bytes.Encode();
            return value;
        }

        return null;
    }

    /// <summary>
    /// Performs the decode operation.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>The operation result.</returns>
    internal static byte[] Decode(this string input)
    {
        var val = input;
        val = val.Replace('-', '+'); // 62nd char of encoding
        val = val.Replace('_', '/'); // 63rd char of encoding

        switch (val.Length % 4)
        {
            case 0: break; // No pad chars in this case
            case 2: val += "=="; break; // Two pad chars
            case 3: val += "="; break; // One pad char
            default: throw new InvalidOperationException(IllegalStringToDecode);
        }

        return Convert.FromBase64String(val); // Standard base64 decoder
    }

    /// <summary>
    /// Performs the decode list operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The matching collection of results.</returns>
    internal static List<string> DecodeList(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            var bytes = value.Decode();
            value = Encoding.UTF8.GetString(bytes);
            var convertedValue = value.JsonDeserialize<string[]>();
            return convertedValue.ToList();
        }

        return new List<string>();
    }

    /// <summary>
    /// Retrieves the crv value from curve.
    /// </summary>
    /// <param name="curve">The curve.</param>
    /// <returns>The operation result.</returns>
    internal static string GetCrvValueFromCurve(this ECCurve curve)
    {
        return curve.Oid.Value switch
        {
            AuthenticationConstants.CurveOids.P256 => JsonWebKeyECTypes.P256,
            AuthenticationConstants.CurveOids.P384 => JsonWebKeyECTypes.P384,
            AuthenticationConstants.CurveOids.P521 => JsonWebKeyECTypes.P521,
            _ => throw new InvalidOperationException(
                $"Unsupported curve type of {curve.Oid.Value} - {curve.Oid.FriendlyName}")
        };
    }

    /// <summary>
    /// Creates a new hash claim value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="tokenSigningAlgorithm">The token signing algorithm.</param>
    /// <returns>The operation result.</returns>
    internal static string CreateHashClaimValue(this string value, string tokenSigningAlgorithm)
    {
        using var sha = tokenSigningAlgorithm.GetHashAlgorithmForSigningAlgorithm();
        var hash = sha.ComputeHash(Encoding.ASCII.GetBytes(value));
        var size = sha.HashSize / 8 / 2;

        var leftPart = new byte[size];
        Array.Copy(hash, leftPart, size);

        return Base64Url.Encode(leftPart);
    }

    /// <summary>
    /// Retrieves the hash algorithm for signing algorithm.
    /// </summary>
    /// <param name="signingAlgorithm">The signing algorithm.</param>
    /// <returns>The operation result.</returns>
    internal static HashAlgorithm GetHashAlgorithmForSigningAlgorithm(this string signingAlgorithm)
    {
        var signingAlgorithmBits = int.Parse(signingAlgorithm.Substring(signingAlgorithm.Length - 3));

        return signingAlgorithmBits switch
        {
            256 => SHA256.Create(),
            384 => SHA384.Create(),
            512 => SHA512.Create(),
            _ => throw new InvalidOperationException(InvalidSigningAlgorithm + signingAlgorithm)
        };
    }

    /// <summary>
    /// Retrieves the rsa security ke.
    /// </summary>
    /// <param name="rsaXML">The rsa x m l.</param>
    /// <returns>The operation result.</returns>
    internal static SecurityKey GetRsaSecurityKey(this string rsaXML)
    {
        if (string.IsNullOrWhiteSpace(rsaXML)) throw new InvalidOperationException(RsaXmlEmpty);

        using var rsa = RSA.Create();
        var keyXml = File.ReadAllText(rsaXML);
        rsa.FromXmlString(keyXml);
        var issuerSigningKey = new RsaSecurityKey(rsa);
        return issuerSigningKey;
    }

    /// <summary>
    /// Performs the compute sha256 hash operation.
    /// </summary>
    /// <param name="rawData">The raw data.</param>
    /// <returns>The operation result.</returns>
    internal static string ComputeSha256Hash(this string rawData)
    {
        // Create a SHA256
        using var sha256Hash = SHA256.Create();

        // ComputeHash - returns byte array
        var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

        // Convert byte array to a string
        var builder = new StringBuilder();
        foreach (var t in bytes) builder.Append(t.ToString("x2"));

        return builder.ToString();
    }

    /// <summary>
    /// Performs the random string operation.
    /// </summary>
    /// <param name="keyLength">The key length.</param>
    /// <returns>The operation result.</returns>
    public static string RandomString(this int keyLength)
    {
        using var rngCryptoServiceProvider = RandomNumberGenerator.Create();
        var randomBytes = new byte[keyLength];
        rngCryptoServiceProvider.GetBytes(randomBytes);
        return randomBytes.Encode();
    }

    /// <summary>
    /// Performs the random byte operation.
    /// </summary>
    /// <param name="keySize">The key size.</param>
    /// <returns>The operation result.</returns>
    internal static byte[] RandomBytes(this int keySize)
    {
        var saltLength = keySize;
        var ba = new byte[saltLength];
        RandomNumberGenerator.Create().GetBytes(ba);
        return ba;
    }
}
