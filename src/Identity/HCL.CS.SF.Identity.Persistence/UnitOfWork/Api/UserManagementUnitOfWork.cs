/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Repository.Api;
using HCL.CS.SF.DomainServices.UnitOfWork.Api;
using HCL.CS.SF.Infrastructure.Data.Repository.Api;

namespace HCL.CS.SF.Infrastructure.Data.UnitOfWork.Api;

/// <summary>
/// Unit of work that coordinates transactional operations across all user management repositories:
/// users, user claims, user roles, user tokens, notifications, password history, and security questions.
/// All repositories share a single database context to ensure changes are committed atomically.
/// Supports both soft-delete and hard-delete save operations.
/// </summary>
internal class UserManagementUnitOfWork : BaseDispose, IUserManagementUnitOfWork
{
    private readonly IApplicationDbContext context;

    private readonly
        UserStore<Users, Roles, ApplicationDbContext, Guid, UserClaims, UserRoles, UserLogins, UserTokens, RoleClaims>
        userStoreWrapper;

    private IRepository<Notification> notificationRepository;
    private IRepository<PasswordHistory> passwordHistoryRepository;
    private IRepository<SecurityQuestions> securityQuestionsRepository;
    private IUserClaimRepository userClaimRepository;
    private IUserRepository userRepository;
    private IUserRoleRepository userRoleRepository;
    private IRepository<UserSecurityQuestions> userSecurityQuestionsRepository;
    private IUserTokenRepository userTokenRepository;

    /// <summary>
    /// Initializes a new instance of <see cref="UserManagementUnitOfWork"/>.
    /// </summary>
    /// <param name="context">The application database context shared across all repositories.</param>
    /// <param name="userStoreWrapper">The ASP.NET Core Identity user store for AutoSaveChanges control.</param>
    public UserManagementUnitOfWork(IApplicationDbContext context, IUserStore<Users> userStoreWrapper)
    {
        this.context = context;
        // We can only have control on user store by the below approach. Tried with interface, extension methods which not working in autosavechanges.
        this.userStoreWrapper =
            userStoreWrapper as UserStore<Users, Roles, ApplicationDbContext, Guid, UserClaims, UserRoles, UserLogins,
                UserTokens, RoleClaims>;
    }

    /// <summary>Gets the lazy-initialized user repository.</summary>
    public IUserRepository UserRepository
    {
        get
        {
            if (userRepository != null) return userRepository;
            userRepository = new UserRepository(context, userStoreWrapper);
            return userRepository;
        }
    }

    /// <summary>Gets the lazy-initialized user claim repository.</summary>
    public IUserClaimRepository UserClaimRepository
    {
        get
        {
            if (userClaimRepository != null) return userClaimRepository;
            userClaimRepository = new UserClaimRepository(context);
            return userClaimRepository;
        }
    }

    /// <summary>Gets the lazy-initialized user token repository.</summary>
    public IUserTokenRepository UserTokenRepository
    {
        get
        {
            if (userTokenRepository != null) return userTokenRepository;
            userTokenRepository = new UserTokenRepository(context);
            return userTokenRepository;
        }
    }

    /// <summary>Gets the lazy-initialized user role repository.</summary>
    public IUserRoleRepository UserRoleRepository
    {
        get
        {
            if (userRoleRepository != null) return userRoleRepository;
            userRoleRepository = new UserRoleRepository(context);
            return userRoleRepository;
        }
    }

    /// <summary>Gets the lazy-initialized user security questions repository.</summary>
    public IRepository<UserSecurityQuestions> UserSecurityQuestionsRepository
    {
        get
        {
            if (userSecurityQuestionsRepository != null) return userSecurityQuestionsRepository;
            userSecurityQuestionsRepository = new BaseRepository<UserSecurityQuestions>(context);
            return userSecurityQuestionsRepository;
        }
    }

    /// <summary>Gets the lazy-initialized notification repository.</summary>
    public IRepository<Notification> NotificationRepository
    {
        get
        {
            if (notificationRepository != null) return notificationRepository;
            notificationRepository = new BaseRepository<Notification>(context);
            return notificationRepository;
        }
    }

    /// <summary>Gets the lazy-initialized password history repository.</summary>
    public IRepository<PasswordHistory> PasswordHistoryRepository
    {
        get
        {
            if (passwordHistoryRepository != null) return passwordHistoryRepository;
            passwordHistoryRepository = new BaseRepository<PasswordHistory>(context);
            return passwordHistoryRepository;
        }
    }

    /// <summary>Gets the lazy-initialized security questions repository.</summary>
    public IRepository<SecurityQuestions> SecurityQuestionsRepository
    {
        get
        {
            if (securityQuestionsRepository != null) return securityQuestionsRepository;
            securityQuestionsRepository = new BaseRepository<SecurityQuestions>(context);
            return securityQuestionsRepository;
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
    /// Sets the original concurrency stamp value without changing entity state.
    /// Used when the entity state is managed separately.
    /// </summary>
    public Task SetConcurrencyOriginalValueAsync<T>(T entity, string concurrencyStamp)
    {
        context.SetConcurrencyOriginalValue(entity, concurrencyStamp);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Marks a single property on an entity as modified for partial updates.
    /// </summary>
    public Task SetPropertyModifiedStatusAsync<T>(T entity, string property)
    {
        context.SetPropertyModifiedStatus(entity, property);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Commits all pending user management changes as a single transaction with soft-delete support.
    /// </summary>
    public async Task<FrameworkResult> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Commits all pending user management changes with hard-delete (physical removal) support.
    /// </summary>
    public async Task<FrameworkResult> SaveChangesWithHardDeleteAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesWithHardDeleteAsync(cancellationToken);
    }
}
