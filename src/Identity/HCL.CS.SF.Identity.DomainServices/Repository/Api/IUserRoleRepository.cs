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
/// Repository contract for managing user-role assignments.
/// Implementations handle the many-to-many relationship between users and roles.
/// </summary>
public interface IUserRoleRepository
{
    /// <summary>Assigns a role to a user by inserting a user-role record.</summary>
    /// <param name="entity">The user-role assignment to insert.</param>
    Task InsertAsync(UserRoles entity);

    /// <summary>Updates an existing user-role assignment.</summary>
    /// <param name="entity">The user-role assignment with updated values.</param>
    Task UpdateAsync(UserRoles entity);

    /// <summary>Removes a single user-role assignment.</summary>
    /// <param name="entity">The user-role assignment to delete.</param>
    Task DeleteAsync(UserRoles entity);

    /// <summary>Removes multiple user-role assignments in a single operation.</summary>
    /// <param name="entityList">The user-role assignments to delete.</param>
    Task DeleteAsync(IList<UserRoles> entityList);

    /// <summary>Retrieves a specific user-role assignment by user and role identifiers.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<UserRoles> GetUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves all role assignments for the specified user.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<IList<UserRoles>> GetUserRoleAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Persists all pending user-role changes.</summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<FrameworkResult> SaveChangesAsync(CancellationToken cancellationToken = default);
}
