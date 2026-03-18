/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Service.Extension;

/// <summary>
/// Extension methods for <see cref="Guid"/> validation and parsing helpers.
/// </summary>
internal static class GuidExtension
{
    /// <summary>
    /// Determines whether the specified GUID is valid (non-empty and non-default).
    /// </summary>
    /// <param name="id">The GUID to validate.</param>
    /// <returns><c>true</c> if the GUID is neither <see cref="Guid.Empty"/> nor default; otherwise, <c>false</c>.</returns>
    internal static bool IsValid(this Guid id)
    {
        return id != Guid.Empty && id != default;
    }

    /// <summary>
    /// Determines whether the specified string can be parsed as a valid GUID.
    /// </summary>
    /// <param name="id">The string to attempt to parse.</param>
    /// <returns><c>true</c> if the string is a valid GUID representation; otherwise, <c>false</c>.</returns>
    internal static bool IsGuid(this string id)
    {
        return Guid.TryParse(id, out _);
    }
}
