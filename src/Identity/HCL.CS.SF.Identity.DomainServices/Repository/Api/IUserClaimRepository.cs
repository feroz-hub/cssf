/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using System.Threading;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Entities.Api;

namespace HCL.CS.SF.DomainServices.Repository.Api;

/// <summary>
/// Repository contract for managing claims directly associated with users.
/// Supports both regular user claims and elevated admin-level claims used for
/// administrative API authorization.
/// </summary>
public interface IUserClaimRepository
{
    /// <summary>Inserts a new user claim.</summary>
    /// <param name="entity">The user claim entity to insert.</param>
    Task InsertAsync(UserClaims entity);

    /// <summary>Updates an existing user claim.</summary>
    /// <param name="entity">The user claim entity with updated values.</param>
    Task UpdateAsync(UserClaims entity);

    /// <summary>Deletes a user claim by its integer primary key.</summary>
    /// <param name="id">The primary key of the user claim.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Deletes multiple user claims in a single operation.</summary>
    /// <param name="entityList">The user claim entities to delete.</param>
    Task DeleteAsync(IList<UserClaims> entityList);

    /// <summary>Finds a user claim matching the specified user, claim type/value, and admin flag.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="claim">The claim to search for.</param>
    /// <param name="isAdminClaim">Whether to search admin claims only.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<UserClaims> FindIdByClaimAsync(Guid userId, Claim claim, bool isAdminClaim = false, CancellationToken cancellationToken = default);

    /// <summary>Finds a user claim entity by its primary key.</summary>
    /// <param name="userClaimId">The primary key of the user claim.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<UserClaims> FindUserClaimByIdAsync(int userClaimId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves all claims for the specified user.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<IList<UserClaims>> GetClaimsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves admin-level claims for the specified user.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="getOnlyAdminClaim">When <c>true</c>, returns only admin claims; otherwise returns all.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<IList<UserClaims>> GetAdminUserClaimsAsync(Guid userId, bool getOnlyAdminClaim = true, CancellationToken cancellationToken = default);

    /// <summary>Persists all pending user claim changes.</summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<FrameworkResult> SaveChangesAsync(CancellationToken cancellationToken = default);
}
