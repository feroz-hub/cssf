/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Comparers;

/// <summary>
/// Equality comparer for <see cref="Claim"/> objects.
/// Compares claims by type and value (case-insensitive) to enable deduplication of user claims
/// in the UserInfo response and token generation.
/// </summary>
internal class ClaimsComparer : IEqualityComparer<Claim>
{
    /// <summary>
    /// Determines whether two instances are equal.
    /// </summary>
    /// <param name="sourceClaim">The source claim.</param>
    /// <param name="targetClaim">The target claim.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool Equals(Claim sourceClaim, Claim targetClaim)
    {
        if (ReferenceEquals(sourceClaim, targetClaim)) return true;

        if (ReferenceEquals(sourceClaim, null) || ReferenceEquals(targetClaim, null)) return false;

        return sourceClaim.Type.ToLower() == targetClaim.Type.ToLower() &&
               sourceClaim.Value.ToLower() == targetClaim.Value.ToLower();
    }

    /// <summary>
    /// Retrieves the hash code.
    /// </summary>
    /// <param name="claim">The claim.</param>
    /// <returns>The operation result.</returns>
    public int GetHashCode(Claim claim)
    {
        if (ReferenceEquals(claim, null)) return 0;

        var hashClaimType = claim.Type == null ? 0 : claim.Type.GetHashCode();
        var hashClaimValue = claim.Value.GetHashCode();
        return hashClaimType ^ hashClaimValue;
    }
}
