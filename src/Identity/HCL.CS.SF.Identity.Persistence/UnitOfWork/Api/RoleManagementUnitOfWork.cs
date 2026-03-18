/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Repository.Api;
using HCL.CS.SF.DomainServices.UnitOfWork.Api;
using HCL.CS.SF.Infrastructure.Data.Repository.Api;

namespace HCL.CS.SF.Infrastructure.Data.UnitOfWork.Api;

/// <summary>
/// Unit of work that coordinates transactional operations across role and role-claims repositories.
/// Provides lazy-initialized repositories and entity state management methods for optimistic
/// concurrency, all sharing a single database context to ensure atomic commits.
/// </summary>
internal class RoleManagementUnitOfWork : BaseDispose, IRoleManagementUnitOfWork
{
    public readonly IApplicationDbContext context;
    private IRoleClaimsRepository roleClaimsRepository;
    private IRoleRepository roleRepository;

    /// <summary>
    /// Initializes a new instance of <see cref="RoleManagementUnitOfWork"/>.
    /// </summary>
    /// <param name="context">The application database context shared across repositories.</param>
    public RoleManagementUnitOfWork(IApplicationDbContext context)
    {
        this.context = context;
    }

    /// <summary>Gets the lazy-initialized role repository.</summary>
    public IRoleRepository RoleRepository
    {
        get
        {
            if (roleRepository != null) return roleRepository;
            roleRepository = new RoleRepository(context);
            return roleRepository;
        }
    }

    /// <summary>Gets the lazy-initialized role claims repository.</summary>
    public IRoleClaimsRepository RoleClaimsRepository
    {
        get
        {
            if (roleClaimsRepository != null) return roleClaimsRepository;
            roleClaimsRepository = new RoleClaimsRepository(context);
            return roleClaimsRepository;
        }
    }

    /// <summary>
    /// Marks an entity as Added in the change tracker.
    /// </summary>
    public Task SetAddedStatusAsync<T>(T entity)
    {
        context.SetAddedStatus(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sets the concurrency stamp and marks an entity as modified for optimistic concurrency.
    /// </summary>
    public Task SetModifiedStatusAsync<T>(T entity, string concurrencyStamp)
    {
        context.SetConcurrencyStatus(entity, concurrencyStamp);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Commits all pending role management changes as a single transaction.
    /// </summary>
    public async Task<FrameworkResult> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}
