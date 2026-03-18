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
using HCL.CS.SF.Domain.Models.Api;

namespace HCL.CS.SF.DomainServices.Repository.Api;

/// <summary>
/// Repository contract for managing claims attached to roles (role-based access control).
/// Implementations handle CRUD operations on role claims and support querying the combined
/// roles-and-claims structure for a given user.
/// </summary>
public interface IRoleClaimsRepository
{
    /// <summary>Inserts a new role claim.</summary>
    /// <param name="entity">The role claim entity to insert.</param>
    Task InsertAsync(RoleClaims entity);

    /// <summary>Updates an existing role claim.</summary>
    /// <param name="entity">The role claim entity with updated values.</param>
    Task UpdateAsync(RoleClaims entity);

    /// <summary>Deletes a role claim by its integer identifier.</summary>
    /// <param name="roleClaimId">The primary key of the role claim.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task DeleteAsync(int roleClaimId, CancellationToken cancellationToken = default);

    /// <summary>Deletes all claims associated with the specified role.</summary>
    /// <param name="roleId">The unique identifier of the role whose claims should be removed.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task DeleteAsync(Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>Deletes the specified role claim entity.</summary>
    /// <param name="entity">The role claim entity to delete.</param>
    Task DeleteAsync(RoleClaims entity);

    /// <summary>Retrieves all claims associated with the specified role.</summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<IList<RoleClaims>> GetClaimsAsync(Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>Finds the primary key of a role claim matching the specified role and claim.</summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="claim">The claim to search for.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<int> FindIdByClaimAsync(Guid roleId, Claim claim, CancellationToken cancellationToken = default);

    /// <summary>Finds the role claim entity matching the specified role and claim.</summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="claim">The claim to search for.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<RoleClaims> FindRoleByClaimAsync(Guid roleId, Claim claim, CancellationToken cancellationToken = default);

    /// <summary>Finds a role claim entity by its primary key.</summary>
    /// <param name="roleClaimId">The primary key of the role claim.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<RoleClaims> FindRoleClaimByIdAsync(int roleClaimId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves the combined roles and their claim types for the specified user.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<IList<UserRoleClaimTypesModel>> GetRolesAndClaimsForUser(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Persists all pending role claim changes.</summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<FrameworkResult> SaveChangesAsync(CancellationToken cancellationToken = default);
}
