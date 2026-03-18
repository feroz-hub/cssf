/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.SharedKernel;

/// <summary>
/// Provides guard-clause helper methods that validate method arguments and throw
/// standard exceptions when preconditions are not met.
/// </summary>
public static class Guard
{
    /// <summary>
    /// Ensures that <paramref name="value"/> is not <c>null</c>.
    /// </summary>
    /// <typeparam name="T">The reference type of the value being checked.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="parameterName">The name of the calling parameter, used in the exception message.</param>
    /// <returns>The non-null <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static T AgainstNull<T>(T? value, string parameterName) where T : class
    {
        return value ?? throw new ArgumentNullException(parameterName);
    }

    /// <summary>
    /// Ensures that <paramref name="value"/> is not <c>null</c>, empty, or whitespace-only.
    /// </summary>
    /// <param name="value">The string value to validate.</param>
    /// <param name="parameterName">The name of the calling parameter, used in the exception message.</param>
    /// <returns>The validated, non-empty <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null, empty, or whitespace.</exception>
    public static string AgainstNullOrWhiteSpace(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", parameterName);
        }

        return value;
    }
}
