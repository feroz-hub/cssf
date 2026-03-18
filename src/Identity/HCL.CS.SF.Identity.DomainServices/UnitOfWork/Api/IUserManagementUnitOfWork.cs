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
using HCL.CS.SF.DomainServices.Repository.Api;

namespace HCL.CS.SF.DomainServices.UnitOfWork.Api;

/// <summary>
/// Unit of work that coordinates transactional persistence for all user-management operations.
/// Groups repositories for users, claims, tokens, roles, security questions, notifications,
/// and password history under a single save boundary with full concurrency control.
/// </summary>
public interface IUserManagementUnitOfWork
{
    /// <summary>Gets the repository for user entities.</summary>
    IUserRepository UserRepository { get; }

    /// <summary>Gets the repository for user claim entities.</summary>
    IUserClaimRepository UserClaimRepository { get; }

    /// <summary>Gets the repository for user token entities.</summary>
    IUserTokenRepository UserTokenRepository { get; }

    /// <summary>Gets the repository for user-role assignment entities.</summary>
    IUserRoleRepository UserRoleRepository { get; }

    /// <summary>Gets the repository for user security question answers.</summary>
    IRepository<UserSecurityQuestions> UserSecurityQuestionsRepository { get; }

    /// <summary>Gets the repository for notification log entries.</summary>
    IRepository<Notification> NotificationRepository { get; }

    /// <summary>Gets the repository for password history records.</summary>
    IRepository<PasswordHistory> PasswordHistoryRepository { get; }

    /// <summary>Gets the repository for system-defined security questions.</summary>
    IRepository<SecurityQuestions> SecurityQuestionsRepository { get; }

    /// <summary>Marks the entity as <c>Added</c> in the EF change tracker.</summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="entity">The entity to mark as added.</param>
    Task SetAddedStatusAsync<T>(T entity);

    /// <summary>Sets the original concurrency stamp value for optimistic concurrency checks.</summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="entity">The entity to configure.</param>
    /// <param name="concurrencyStamp">The concurrency stamp from the database.</param>
    Task SetConcurrencyOriginalValueAsync<T>(T entity, string concurrencyStamp);

    /// <summary>Marks the entity as <c>Modified</c> in the EF change tracker with a concurrency stamp.</summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="entity">The entity to mark as modified.</param>
    /// <param name="concurrencyStamp">The original concurrency stamp for optimistic concurrency.</param>
    Task SetModifiedStatusAsync<T>(T entity, string concurrencyStamp);

    /// <summary>Marks a single property of the entity as modified for partial updates.</summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="entity">The entity whose property changed.</param>
    /// <param name="property">The name of the modified property.</param>
    Task SetPropertyModifiedStatusAsync<T>(T entity, string property);

    /// <summary>Persists all pending changes across all repositories using soft-delete semantics.</summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<FrameworkResult> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Persists all pending changes using hard-delete semantics (physical removal).</summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<FrameworkResult> SaveChangesWithHardDeleteAsync(CancellationToken cancellationToken = default);
}
