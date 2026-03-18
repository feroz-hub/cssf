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
/// Repository for user-role assignment data operations. Provides methods to add, update,
/// delete, and query user-role relationships.
/// </summary>
internal class UserRoleRepository : BaseDispose, IUserRoleRepository
{
    private readonly IApplicationDbContext context;

    /// <summary>
    /// Initializes a new instance of <see cref="UserRoleRepository"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public UserRoleRepository(IApplicationDbContext context)
    {
        this.context = context;
    }

    /// <summary>
    /// Assigns a role to a user by adding a UserRoles entity.
    /// </summary>
    public Task InsertAsync(UserRoles entity)
    {
        context.UserRoles.Add(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates a user-role assignment entity.
    /// </summary>
    public virtual Task UpdateAsync(UserRoles entity)
    {
        context.UserRoles.Update(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes a list of user-role assignments.
    /// </summary>
    public Task DeleteAsync(IList<UserRoles> entityList)
    {
        foreach (var entity in entityList) context.UserRoles.Remove(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes a single user-role assignment.
    /// </summary>
    public Task DeleteAsync(UserRoles entity)
    {
        context.UserRoles.Remove(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves a specific user-role assignment by user ID and role ID.
    /// </summary>
    public async Task<UserRoles> GetUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        var entity = await context.UserRoles.Where(roleClaimEntity => roleClaimEntity.UserId == userId &&
                                                                      roleClaimEntity.RoleId == roleId)
            .FirstOrDefaultAsync(cancellationToken);
        return entity;
    }

    /// <summary>
    /// Retrieves all role assignments for a specific user.
    /// </summary>
    public async Task<IList<UserRoles>> GetUserRoleAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var entity = await context.UserRoles.Where(roleClaimEntity => roleClaimEntity.UserId == userId).ToListAsync(cancellationToken);
        return entity;
    }

    /// <summary>
    /// Persists all pending user-role changes.
    /// </summary>
    public async Task<FrameworkResult> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}
