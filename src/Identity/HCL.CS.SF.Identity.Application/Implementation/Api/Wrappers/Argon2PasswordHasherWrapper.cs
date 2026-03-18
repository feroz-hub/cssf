/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Cryptography;
using System.Text;
using Isopoh.Cryptography.Argon2;
using Isopoh.Cryptography.SecureArray;
using Microsoft.AspNetCore.Identity;

namespace HCL.CS.SF.Service.Implementation.Api.Wrappers;

/// <summary>
/// Argon2-based password hasher that replaces the default ASP.NET Identity PBKDF2 hasher.
/// Uses Argon2id (data-independent addressing) with configurable memory cost, time cost,
/// and parallelism lanes to provide resistance against GPU and ASIC brute-force attacks.
/// This is critical for protecting user credentials at rest in the identity database.
/// </summary>
/// <typeparam name="TUser">The user entity type managed by ASP.NET Identity.</typeparam>

public class Argon2PasswordHasherWrapper<TUser> : IPasswordHasher<TUser>
    where TUser : class
{
    /// <summary>Number of parallel lanes for the Argon2 computation.</summary>

    private readonly int lanes = 5;
    /// <summary>Cryptographically secure random number generator for salt generation.</summary>

    private readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

    /// <summary>
    /// Hashes a plaintext password using Argon2id with a 16-byte random salt.
    /// The result is an encoded Argon2 hash string that includes algorithm parameters,
    /// salt, and hash output -- suitable for storage in the identity database.
    /// </summary>
    /// <param name="user">The user entity (unused by Argon2 but required by the interface).</param>
    /// <param name="password">The plaintext password to hash.</param>
    /// <returns>An Argon2-encoded hash string containing salt, parameters, and derived key.</returns>

    public string HashPassword(TUser user, string password)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var salt = new byte[16];

        rng.GetBytes(salt);

        var config = new Argon2Config
        {
            Type = Argon2Type.DataIndependentAddressing,
            Version = Argon2Version.Nineteen,
            TimeCost = 10,
            MemoryCost = 32768,
            Lanes = lanes,
            Threads = Environment.ProcessorCount, // higher than "Lanes" doesn't help (or hurt)
            Password = passwordBytes,
            Salt = salt, // >= 8 bytes if not null
            HashLength = 20 // >= 4
        };

        var argon2A = new Argon2(config);
        string hashString;
        using (var hashA = argon2A.Hash())
        {
            hashString = config.EncodeString(hashA.Buffer);
        }

        return hashString;
    }

    /// <summary>
    /// Verifies a plaintext password against a stored Argon2 hash. Performs constant-time
    /// comparison to prevent timing side-channel attacks during credential verification.
    /// </summary>
    /// <param name="user">The user entity (unused by Argon2 but required by the interface).</param>
    /// <param name="hashedPassword">The stored Argon2 hash string to verify against.</param>
    /// <param name="providedPassword">The plaintext password provided during authentication.</param>
    /// <returns>Success if the password matches; Failed otherwise.</returns>

    public PasswordVerificationResult VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(providedPassword);

        var configOfPasswordToVerify = new Argon2Config { Password = passwordBytes, Threads = 1 };
        SecureArray<byte> hashB = null;
        try
        {
            if (configOfPasswordToVerify.DecodeString(hashedPassword, out hashB) && hashB != null)
            {
                var argon2ToVerify = new Argon2(configOfPasswordToVerify);
                using (var hashToVerify = argon2ToVerify.Hash())
                {
                    if (!Argon2.FixedTimeEquals(hashB, hashToVerify)) return PasswordVerificationResult.Failed;
                }
            }
        }
        finally
        {
            hashB?.Dispose();
        }

        if (Argon2.Verify(hashedPassword, passwordBytes, lanes)) return PasswordVerificationResult.Success;

        return PasswordVerificationResult.Failed;
    }
}
