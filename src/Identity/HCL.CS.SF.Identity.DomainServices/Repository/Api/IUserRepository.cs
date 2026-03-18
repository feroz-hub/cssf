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
/// Repository contract for user persistence operations that go beyond ASP.NET Identity's
/// built-in <c>UserManager</c>. Provides soft-delete-aware lookups, EF change-tracking control,
/// and the ability to toggle Identity's auto-save behavior for unit-of-work coordination.
/// </summary>
public interface IUserRepository
{
    /// <summary>Soft-deletes the specified user entity.</summary>
    /// <param name="entity">The user entity to delete.</param>
    Task DeleteAsync(Users entity);

    /// <summary>Partially updates the specified user, modifying only the listed properties.</summary>
    /// <param name="entity">The user entity with updated values.</param>
    /// <param name="affectedProperties">The names of properties that changed.</param>
    Task UpdateAsync(Users entity, string[] affectedProperties);

    /// <summary>Retrieves all active users assigned to the specified role.</summary>
    /// <param name="roleName">The name of the role.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<IList<Users>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken = default);

    /// <summary>Finds a user by username, including soft-deleted records, to prevent duplicate registrations.</summary>
    /// <param name="userName">The username to search for.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<Users?> FindByUserNameIncludingDeletedAsync(string userName, CancellationToken cancellationToken = default);

    /// <summary>Marks the entity as <c>Added</c> in the EF change tracker.</summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="entity">The entity to mark as added.</param>
    Task SetAddedStatusAsync<T>(T entity);

    /// <summary>Marks the entity as <c>Modified</c> in the EF change tracker with a concurrency stamp.</summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="entity">The entity to mark as modified.</param>
    /// <param name="concurrencyStamp">The original concurrency stamp for optimistic concurrency.</param>
    Task SetModifiedStatusAsync<T>(T entity, string concurrencyStamp);

    /// <summary>Persists all pending user changes.</summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<FrameworkResult> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Enables ASP.NET Identity's automatic <c>SaveChanges</c> after each operation.</summary>
    Task EnableIdentityAutoSaveChanges();

    /// <summary>Disables ASP.NET Identity's automatic <c>SaveChanges</c> to allow unit-of-work coordination.</summary>
    Task DisableIdentityAutoSaveChanges();

    /// <summary>Retrieves all active (non-deleted) users.</summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<IList<Users>> GetAllUsersAsync(CancellationToken cancellationToken = default);
}
