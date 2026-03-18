/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Threading;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Repository.Api;

namespace HCL.CS.SF.Infrastructure.Data.Repository.Api;

/// <summary>
/// Repository for user data operations. Provides methods to delete, update, and query users,
/// retrieve users by role, look up users including soft-deleted records, and control the
/// Identity framework's AutoSaveChanges behavior through the underlying UserStore.
/// </summary>
internal class UserRepository : BaseDispose, IUserRepository
{
    private readonly IApplicationDbContext context;

    private readonly
        UserStore<Users, Roles, ApplicationDbContext, Guid, UserClaims, UserRoles, UserLogins, UserTokens, RoleClaims>
        userStoreWrapper;

    /// <summary>
    /// Initializes a new instance of <see cref="UserRepository"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="userStoreWrapper">The ASP.NET Core Identity user store, cast to the concrete type for AutoSaveChanges control.</param>
    public UserRepository(IApplicationDbContext context,
        IUserStore<Users> userStoreWrapper)
    {
        this.context = context;
        // We can only have control on user store by the below approach. Tried with interface, extension methods which not working in autosavechanges.
        this.userStoreWrapper =
            userStoreWrapper as UserStore<Users, Roles, ApplicationDbContext, Guid, UserClaims, UserRoles, UserLogins,
                UserTokens, RoleClaims>;
    }

    /// <summary>
    /// Soft-deletes a user by marking IsDeleted and removing from the change tracker.
    /// </summary>
    public Task DeleteAsync(Users entity)
    {
        entity.IsDeleted = true;
        context.Users.Remove(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Performs a partial update on a user entity, marking only the specified properties as modified.
    /// </summary>
    public virtual Task UpdateAsync(Users entity, string[] affectedProperties)
    {
        context.Users.Attach(entity);
        foreach (var property in affectedProperties) context.SetPropertyModifiedStatus(entity, property);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves all users assigned to a specific role, filtered by valid role assignment dates
    /// (ValidFrom/ValidTo must encompass the current UTC time).
    /// </summary>
    public async Task<IList<Users>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken = default)
    {
        // Sub-query: find user IDs with active (time-valid) role assignments for the given role name
        var roleUserIdsQuery =
            from role in context.Roles.AsNoTracking()
            from userRole in context.UserRoles.AsNoTracking()
            where role.Id == userRole.RoleId && role.Name == roleName &&
                  userRole.ValidFrom <= DateTime.UtcNow && userRole.ValidTo >= DateTime.UtcNow
            select userRole.UserId;

        var userList = await context.Users.AsNoTracking().Where(user => roleUserIdsQuery.Contains(user.Id)).ToListAsync(cancellationToken);
        return userList;
    }

    /// <summary>
    /// Finds a user by username including soft-deleted records, bypassing global query filters.
    /// Used to detect previously deleted usernames during re-registration.
    /// </summary>
    public async Task<Users?> FindByUserNameIncludingDeletedAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.UserName == userName, cancellationToken);
    }

    /// <summary>
    /// Marks an entity's change-tracker state as Added.
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
    /// Enables the Identity UserStore's AutoSaveChanges, causing Identity operations to save immediately.
    /// </summary>
    public Task EnableIdentityAutoSaveChanges()
    {
        userStoreWrapper.AutoSaveChanges = true;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Disables the Identity UserStore's AutoSaveChanges, allowing changes to be batched in a unit of work.
    /// </summary>
    public Task DisableIdentityAutoSaveChanges()
    {
        userStoreWrapper.AutoSaveChanges = false;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Persists all pending user changes.
    /// </summary>
    public async Task<FrameworkResult> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves all active (non-deleted) users as a no-tracking read.
    /// </summary>
    public async Task<IList<Users>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await context.Users.AsNoTracking().ToListAsync(cancellationToken);
        return users;
    }
}
