/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Threading;
using HCL.CS.SF.Domain;
using HCL.CS.SF.DomainServices.Repository.Api;

namespace HCL.CS.SF.DomainServices.UnitOfWork.Api;

/// <summary>
/// Unit of work that coordinates transactional persistence for role management operations.
/// Groups the role repository and role-claims repository under a single save boundary
/// and provides EF change-tracking helpers for optimistic concurrency.
/// </summary>
public interface IRoleManagementUnitOfWork
{
    /// <summary>Gets the repository for role entities.</summary>
    IRoleRepository RoleRepository { get; }

    /// <summary>Gets the repository for role claim entities.</summary>
    IRoleClaimsRepository RoleClaimsRepository { get; }

    /// <summary>Marks the entity as <c>Added</c> in the EF change tracker.</summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="entity">The entity to mark as added.</param>
    Task SetAddedStatusAsync<T>(T entity);

    /// <summary>Marks the entity as <c>Modified</c> in the EF change tracker with a concurrency stamp.</summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="entity">The entity to mark as modified.</param>
    /// <param name="concurrencyStamp">The original concurrency stamp for optimistic concurrency.</param>
    Task SetModifiedStatusAsync<T>(T entity, string concurrencyStamp);

    /// <summary>Persists all pending changes across role and role-claim repositories.</summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<FrameworkResult> SaveChangesAsync(CancellationToken cancellationToken = default);
}
