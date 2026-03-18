/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.DomainServices.Infra;

/// <summary>
/// Provides localized resource string look-ups used for error messages and UI text.
/// Implementations should load strings from resource files or a string table keyed by culture.
/// </summary>
public interface IResourceStringHandler
{
    /// <summary>
    /// Retrieves a localized resource string by its identifier.
    /// </summary>
    /// <param name="id">The resource string key.</param>
    /// <param name="skipError">When <c>true</c>, returns <c>null</c> instead of throwing if the key is not found.</param>
    /// <returns>The localized string value.</returns>
    string GetResourceString(string id, bool skipError = false);

    /// <summary>
    /// Performs a reverse look-up to find the resource key associated with the given value.
    /// </summary>
    /// <param name="value">The resource string value to search for.</param>
    /// <returns>The corresponding resource key, or <c>null</c> if not found.</returns>
    string GetResourceKeyByValue(string value);
}
