/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Entities.Endpoint;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Comparers;

/// <summary>
/// Equality comparer for client redirect URIs.
/// Compares by client ID and URI value (case-insensitive) for deduplication during authorization validation.
/// </summary>
internal class ClientRedirectUriComparer : IEqualityComparer<ClientRedirectUris>
{
    /// <summary>
    /// Determines whether two instances are equal.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool Equals(ClientRedirectUris x, ClientRedirectUris y)
    {
        if (x.ClientId == y.ClientId && x.RedirectUri.ToLower() == y.RedirectUri.ToLower()) return true;

        return false;
    }

    /// <summary>
    /// Retrieves the hash code.
    /// </summary>
    /// <param name="obj">The obj.</param>
    /// <returns>The operation result.</returns>
    public int GetHashCode(ClientRedirectUris obj)
    {
        return obj.Id.GetHashCode();
    }
}
