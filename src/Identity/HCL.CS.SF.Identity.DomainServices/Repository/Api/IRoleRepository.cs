/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Threading;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Entities.Api;

namespace HCL.CS.SF.DomainServices.Repository.Api;

/// <summary>
/// Repository contract for role persistence operations.
/// Provides role update, deletion, retrieval, and duplicate-name checks that include
/// soft-deleted records to prevent name reuse conflicts.
/// </summary>
public interface IRoleRepository
{
    /// <summary>Updates an existing role entity.</summary>
    /// <param name="entity">The role entity with updated values.</param>
    Task UpdateAsync(Roles entity);

    /// <summary>Soft-deletes the specified role entity.</summary>
    /// <param name="entity">The role entity to delete.</param>
    Task DeleteAsync(Roles entity);

    /// <summary>Retrieves all active (non-deleted) roles.</summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<IList<Roles>> GetAllRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>Checks whether a role with the specified name exists, including soft-deleted records.</summary>
    /// <param name="roleName">The role name to check.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns><c>true</c> if a role with the given name exists (active or deleted).</returns>
    Task<bool> ExistsByNameIncludingDeletedAsync(string roleName, CancellationToken cancellationToken = default);

    /// <summary>Persists all pending role changes.</summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<FrameworkResult> SaveChangesAsync(CancellationToken cancellationToken = default);
}
