/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Threading;
using Microsoft.EntityFrameworkCore;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Repository.Api;

namespace HCL.CS.SF.Infrastructure.Data.Repository.Api;

/// <summary>
/// Repository for role data operations. Provides methods to update, delete, and list roles,
/// and to check for duplicate role names across both active and soft-deleted records.
/// </summary>
internal class RoleRepository : BaseDispose, IRoleRepository
{
    private readonly IApplicationDbContext context;

    /// <summary>
    /// Initializes a new instance of <see cref="RoleRepository"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public RoleRepository(IApplicationDbContext context)
    {
        this.context = context;
    }

    /// <summary>
    /// Updates a role entity by attaching it and marking it as modified.
    /// </summary>
    public Task UpdateAsync(Roles entity)
    {
        context.Roles.Attach(entity);
        context.SetModifiedStatus(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes a role entity from the change tracker for deletion.
    /// </summary>
    public Task DeleteAsync(Roles entity)
    {
        context.Roles.Remove(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves all active (non-deleted) roles as a no-tracking read.
    /// </summary>
    public async Task<IList<Roles>> GetAllRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await context.Roles.AsNoTracking().ToListAsync(cancellationToken);
        return roles;
    }

    /// <summary>
    /// Checks whether a role with the given name exists, including soft-deleted records.
    /// Used to prevent re-creation of previously deleted role names.
    /// </summary>
    public async Task<bool> ExistsByNameIncludingDeletedAsync(string roleName, CancellationToken cancellationToken = default)
    {
        return await context.Roles
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Name == roleName, cancellationToken);
    }

    /// <summary>
    /// Persists all pending role changes.
    /// </summary>
    public async Task<FrameworkResult> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}
