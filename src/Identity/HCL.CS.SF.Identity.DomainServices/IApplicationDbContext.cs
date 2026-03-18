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
using HCL.CS.SF.Domain.Entities.Endpoint;

namespace HCL.CS.SF.DomainServices;

/// <summary>
/// Abstraction over the Entity Framework <c>DbContext</c> used by the identity domain.
/// Implementations expose <see cref="DbSet{TEntity}"/> properties for every aggregate root and
/// provide persistence operations including soft-delete-aware saves and EF change-tracking helpers.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>Gets or sets the audit trail entries.</summary>
    DbSet<AuditTrail> AuditTrail { get; set; }

    /// <summary>Gets or sets the system-defined security questions.</summary>
    DbSet<SecurityQuestions> SecurityQuestions { get; set; }

    /// <summary>Gets or sets the user-answered security questions.</summary>
    DbSet<UserSecurityQuestions> UserSecurityQuestions { get; set; }

    /// <summary>Gets or sets the password history records used to enforce password reuse policies.</summary>
    DbSet<PasswordHistory> PasswordHistory { get; set; }

    /// <summary>Gets or sets the identity users.</summary>
    DbSet<Users> Users { get; set; }

    /// <summary>Gets or sets the claims associated with users.</summary>
    DbSet<UserClaims> UserClaims { get; set; }

    /// <summary>Gets or sets the external login records for users.</summary>
    DbSet<UserLogins> UserLogins { get; set; }

    /// <summary>Gets or sets the authentication tokens stored for users.</summary>
    DbSet<UserTokens> UserTokens { get; set; }

    /// <summary>Gets or sets the application roles.</summary>
    DbSet<Roles> Roles { get; set; }

    /// <summary>Gets or sets the claims associated with roles.</summary>
    DbSet<RoleClaims> RoleClaims { get; set; }

    /// <summary>Gets or sets the user-role assignments.</summary>
    DbSet<UserRoles> UserRoles { get; set; }

    /// <summary>Gets or sets the external identity provider records.</summary>
    DbSet<ExternalIdentities> ExternalIdentities { get; set; }

    /// <summary>Gets or sets the registered OAuth/OIDC clients.</summary>
    DbSet<Clients> Clients { get; set; }

    /// <summary>Gets or sets the notification log entries.</summary>
    DbSet<Notification> Notification { get; set; }

    /// <summary>Gets or sets the API resource definitions.</summary>
    DbSet<ApiResources> ApiResources { get; set; }

    /// <summary>Gets or sets the claims associated with API resources.</summary>
    DbSet<ApiResourceClaims> ApiResourceClaims { get; set; }

    /// <summary>Gets or sets the API scope definitions.</summary>
    DbSet<ApiScopes> ApiScopes { get; set; }

    /// <summary>Gets or sets the claims associated with API scopes.</summary>
    DbSet<ApiScopeClaims> ApiScopeClaims { get; set; }

    /// <summary>Gets or sets the identity resource definitions (e.g., openid, profile).</summary>
    DbSet<IdentityResources> IdentityResources { get; set; }

    /// <summary>Gets or sets the claims associated with identity resources.</summary>
    DbSet<IdentityClaims> IdentityClaims { get; set; }

    /// <summary>Gets or sets the persisted security tokens (authorization codes, refresh tokens, etc.).</summary>
    DbSet<SecurityTokens> SecurityTokens { get; set; }

    /// <summary>
    /// Returns a <see cref="DbSet{TEntity}"/> for the specified entity type.
    /// </summary>
    /// <typeparam name="T">The entity type, which must derive from <see cref="BaseEntity"/>.</typeparam>
    DbSet<T> Set<T>() where T : BaseEntity;

    /// <summary>
    /// Persists all pending changes, applying soft-delete logic where applicable.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    Task<FrameworkResult> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists all pending changes using hard deletes (physical removal from the database).
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    Task<FrameworkResult> SaveChangesWithHardDeleteAsync(CancellationToken cancellationToken = default);

    /// <summary>Marks the specified entry as <c>Added</c> in the EF change tracker.</summary>
    /// <param name="entry">The entity to mark.</param>
    void SetAddedStatus(object entry);

    /// <summary>Marks the specified entry as <c>Modified</c> in the EF change tracker.</summary>
    /// <param name="entry">The entity to mark.</param>
    void SetModifiedStatus(object entry);

    /// <summary>Marks a single property of the entry as modified for partial updates.</summary>
    /// <param name="entry">The entity whose property changed.</param>
    /// <param name="property">The name of the modified property.</param>
    void SetPropertyModifiedStatus(object entry, string property);

    /// <summary>Sets the original concurrency stamp value for optimistic concurrency checks.</summary>
    /// <param name="entry">The entity to configure.</param>
    /// <param name="dbConcurrencyStamp">The concurrency stamp value from the database.</param>
    void SetConcurrencyOriginalValue(object entry, string dbConcurrencyStamp);

    /// <summary>Sets the current concurrency stamp value on the entity for concurrency control.</summary>
    /// <param name="entry">The entity to configure.</param>
    /// <param name="dbConcurrencyStamp">The concurrency stamp value.</param>
    void SetConcurrencyStatus(object entry, string dbConcurrencyStamp);

    /// <summary>Sets the row version stamp on the entity for database-level optimistic concurrency.</summary>
    /// <param name="entry">The entity to configure.</param>
    /// <param name="dbRowVersionStamp">The row version byte array from the database.</param>
    void SetRowVersionStatus(object entry, byte[] dbRowVersionStamp);
}
