/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Threading;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.Infrastructure.Data.Mapper.Api;
using HCL.CS.SF.Infrastructure.Data.Mapper.Endpoint;

namespace HCL.CS.SF.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context for the Security Framework identity system.
/// Extends ASP.NET Core IdentityDbContext to provide schema mappings for users, roles, claims,
/// OAuth/OIDC clients, API resources, identity resources, audit trails, notifications, and security tokens.
/// Implements soft-delete via the <see cref="BaseEntity.IsDeleted"/> flag, automatic audit stamping
/// (CreatedOn/ModifiedOn/CreatedBy/ModifiedBy), and UTC date-time normalization on all save operations.
/// </summary>
public class ApplicationDbContext :
    IdentityDbContext<Users, Roles, Guid, UserClaims, UserRoles, UserLogins, RoleClaims, UserTokens>,
    IApplicationDbContext
{
    /// <summary>
    /// Initializes a new instance of <see cref="ApplicationDbContext"/> using default options.
    /// </summary>
    public ApplicationDbContext()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ApplicationDbContext"/> with the specified database options.
    /// </summary>
    /// <param name="options">The options to configure the database context.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>Gets or sets the DbSet for OAuth client redirect URIs.</summary>
    public virtual DbSet<ClientRedirectUris> RedirectUris { get; set; }

    /// <summary>Gets or sets the DbSet for OAuth client post-logout redirect URIs.</summary>
    public virtual DbSet<ClientPostLogoutRedirectUris> PostLogoutRedirectUris { get; set; }

    /// <summary>Gets or sets the DbSet for audit trail records that track data changes.</summary>
    public virtual DbSet<AuditTrail> AuditTrail { get; set; }

    /// <summary>Gets or sets the DbSet for security questions used in account recovery.</summary>
    public virtual DbSet<SecurityQuestions> SecurityQuestions { get; set; }

    /// <summary>Gets or sets the DbSet for user-to-security-question answer mappings.</summary>
    public virtual DbSet<UserSecurityQuestions> UserSecurityQuestions { get; set; }

    /// <summary>Gets or sets the DbSet for user password history records.</summary>
    public virtual DbSet<PasswordHistory> PasswordHistory { get; set; }

    /// <summary>Gets or sets the DbSet for notification records (email, SMS, etc.).</summary>
    public virtual DbSet<Notification> Notification { get; set; }

    /// <summary>Gets or sets the DbSet for notification provider configurations (SMTP, Twilio, etc.).</summary>
    public virtual DbSet<NotificationProviderConfig> NotificationProviderConfig { get; set; }

    /// <summary>Gets or sets the DbSet for external authentication provider configurations (Google, etc.).</summary>
    public virtual DbSet<ExternalAuthProviderConfig> ExternalAuthProviderConfig { get; set; }

    /// <summary>Gets or sets the DbSet for external identity linkages between external providers and local users.</summary>
    public virtual DbSet<ExternalIdentities> ExternalIdentities { get; set; }

    /// <summary>Gets or sets the DbSet for registered OAuth/OIDC clients.</summary>
    public virtual DbSet<Clients> Clients { get; set; }

    /// <summary>Gets or sets the DbSet for application roles.</summary>
    public override DbSet<Roles> Roles { get; set; }

    /// <summary>Gets or sets the DbSet for claims assigned to roles.</summary>
    public override DbSet<RoleClaims> RoleClaims { get; set; }

    /// <summary>Gets or sets the DbSet for claims assigned to users.</summary>
    public override DbSet<UserClaims> UserClaims { get; set; }

    /// <summary>Gets or sets the DbSet for API resources used in token audience validation.</summary>
    public virtual DbSet<ApiResources> ApiResources { get; set; }

    /// <summary>Gets or sets the DbSet for claims associated with API resources.</summary>
    public virtual DbSet<ApiResourceClaims> ApiResourceClaims { get; set; }

    /// <summary>Gets or sets the DbSet for API scopes that define access boundaries.</summary>
    public virtual DbSet<ApiScopes> ApiScopes { get; set; }

    /// <summary>Gets or sets the DbSet for claims associated with API scopes.</summary>
    public virtual DbSet<ApiScopeClaims> ApiScopeClaims { get; set; }

    /// <summary>Gets or sets the DbSet for OIDC identity resources (openid, profile, email, etc.).</summary>
    public virtual DbSet<IdentityResources> IdentityResources { get; set; }

    /// <summary>Gets or sets the DbSet for claims associated with identity resources.</summary>
    public virtual DbSet<IdentityClaims> IdentityClaims { get; set; }

    /// <summary>Gets or sets the DbSet for security tokens (authorization codes, refresh tokens, etc.).</summary>
    public virtual DbSet<SecurityTokens> SecurityTokens { get; set; }

    /// <summary>
    /// Returns a <see cref="DbSet{TEntity}"/> for the specified entity type, implementing the <see cref="IApplicationDbContext"/> contract.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <returns>A DbSet for the requested entity type.</returns>
    DbSet<T> IApplicationDbContext.Set<T>()
    {
        return base.Set<T>();
    }

    /// <summary>
    /// Persists all pending changes with soft-delete support. Deleted entities are marked
    /// as <c>IsDeleted = true</c> rather than being physically removed from the database.
    /// Applies audit timestamps and normalizes DateTime values to UTC before saving.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public new async Task<FrameworkResult> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Apply audit fields (CreatedOn, ModifiedOn, etc.) with soft-delete enabled
            ApplyAuditState(true);
            NormalizeDateTimesToUtc();
            var changes = await base.SaveChangesAsync(cancellationToken);
            return BuildResult(changes);
        }
        catch (DbUpdateConcurrencyException)
        {
            return BuildFailedResult(ApiErrorCodes.ConcurrencyFailure, "Concurrency conflict while saving changes.");
        }
        catch (Exception ex)
        {
            return BuildFailedResult(ApiErrorCodes.InvalidOrNullObject, ex.Message);
        }
    }

    /// <summary>
    /// Persists all pending changes with hard-delete support. Deleted entities are physically
    /// removed from the database instead of being soft-deleted.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public async Task<FrameworkResult> SaveChangesWithHardDeleteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Apply audit fields with soft-delete disabled (hard delete)
            ApplyAuditState(false);
            NormalizeDateTimesToUtc();
            var changes = await base.SaveChangesAsync(cancellationToken);
            return BuildResult(changes);
        }
        catch (DbUpdateConcurrencyException)
        {
            return BuildFailedResult(ApiErrorCodes.ConcurrencyFailure, "Concurrency conflict while saving changes.");
        }
        catch (Exception ex)
        {
            return BuildFailedResult(ApiErrorCodes.InvalidOrNullObject, ex.Message);
        }
    }

    /// <summary>
    /// Marks an entity's change-tracker state as <see cref="EntityState.Added"/>.
    /// </summary>
    /// <param name="entry">The entity to mark as added.</param>
    public void SetAddedStatus(object entry)
    {
        Entry(entry).State = EntityState.Added;
    }

    /// <summary>
    /// Marks an entity's change-tracker state as <see cref="EntityState.Modified"/>.
    /// </summary>
    /// <param name="entry">The entity to mark as modified.</param>
    public void SetModifiedStatus(object entry)
    {
        Entry(entry).State = EntityState.Modified;
    }

    /// <summary>
    /// Marks a single property on an entity as modified for partial updates.
    /// </summary>
    /// <param name="entry">The entity containing the property.</param>
    /// <param name="property">The name of the property to mark as modified.</param>
    public void SetPropertyModifiedStatus(object entry, string property)
    {
        var entityEntry = Entry(entry);
        entityEntry.Property(property).IsModified = true;
    }

    /// <summary>
    /// Sets the original value of the ConcurrencyStamp property to enable optimistic concurrency checks.
    /// </summary>
    /// <param name="entry">The entity to configure.</param>
    /// <param name="dbConcurrencyStamp">The concurrency stamp value from the database.</param>
    public void SetConcurrencyOriginalValue(object entry, string dbConcurrencyStamp)
    {
        var entityEntry = Entry(entry);
        var concurrencyProperty = entityEntry.Metadata.FindProperty("ConcurrencyStamp");
        if (concurrencyProperty != null) entityEntry.Property("ConcurrencyStamp").OriginalValue = dbConcurrencyStamp;
    }

    /// <summary>
    /// Sets the concurrency stamp original value and marks the entity as modified for optimistic concurrency.
    /// </summary>
    /// <param name="entry">The entity to configure.</param>
    /// <param name="dbConcurrencyStamp">The concurrency stamp value from the database.</param>
    public void SetConcurrencyStatus(object entry, string dbConcurrencyStamp)
    {
        var entityEntry = Entry(entry);
        SetConcurrencyOriginalValue(entry, dbConcurrencyStamp);
        entityEntry.State = EntityState.Modified;
    }

    /// <summary>
    /// Sets the RowVersion original value and marks the entity as modified for row-version-based concurrency control.
    /// </summary>
    /// <param name="entry">The entity to configure.</param>
    /// <param name="dbRowVersionStamp">The row version value from the database.</param>
    public void SetRowVersionStatus(object entry, byte[] dbRowVersionStamp)
    {
        var entityEntry = Entry(entry);
        var rowVersionProperty = entityEntry.Metadata.FindProperty(nameof(BaseEntity.RowVersion));
        if (rowVersionProperty != null)
            entityEntry.Property(nameof(BaseEntity.RowVersion)).OriginalValue = dbRowVersionStamp;

        entityEntry.State = EntityState.Modified;
    }

    /// <inheritdoc />
    public override int SaveChanges()
    {
        NormalizeDateTimesToUtc();
        return base.SaveChanges();
    }

    /// <inheritdoc />
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        NormalizeDateTimesToUtc();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <inheritdoc />
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditState(true);
        NormalizeDateTimesToUtc();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// Configures the EF Core model: applies entity-to-table mappings, sets default delete behavior
    /// to Restrict, and applies provider-specific configurations for PostgreSQL and SQLite.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Default all foreign keys to Restrict to prevent accidental cascade deletes
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            relationship.DeleteBehavior = DeleteBehavior.Restrict;

        // API
        _ = new AuditTrailMap(modelBuilder.Entity<AuditTrail>());
        _ = new UsersMap(modelBuilder.Entity<Users>());
        _ = new SecurityQuestionsMap(modelBuilder.Entity<SecurityQuestions>());
        _ = new UserSecurityQuestionsMap(modelBuilder.Entity<UserSecurityQuestions>());
        _ = new PasswordHistoryMap(modelBuilder.Entity<PasswordHistory>());
        _ = new UserClaimsMap(modelBuilder.Entity<UserClaims>());
        _ = new UserLoginsMap(modelBuilder.Entity<UserLogins>());
        _ = new UserTokensMap(modelBuilder.Entity<UserTokens>());
        _ = new RolesMap(modelBuilder.Entity<Roles>());
        _ = new RoleClaimsMap(modelBuilder.Entity<RoleClaims>());
        _ = new UserRolesMap(modelBuilder.Entity<UserRoles>());
        _ = new NotificationMap(modelBuilder.Entity<Notification>());
        _ = new NotificationProviderConfigMap(modelBuilder.Entity<NotificationProviderConfig>());
        _ = new ExternalAuthProviderConfigMap(modelBuilder.Entity<ExternalAuthProviderConfig>());
        _ = new ExternalIdentitiesMap(modelBuilder.Entity<ExternalIdentities>());

        // Endpoint
        _ = new ClientsMap(modelBuilder.Entity<Clients>());
        _ = new ClientRedirectUrisMap(modelBuilder.Entity<ClientRedirectUris>());
        _ = new ClientPostLogoutRedirectUrisMap(modelBuilder.Entity<ClientPostLogoutRedirectUris>());
        _ = new IdentityResourcesMap(modelBuilder.Entity<IdentityResources>());
        _ = new IdentityClaimsMap(modelBuilder.Entity<IdentityClaims>());
        _ = new ApiResourcesMap(modelBuilder.Entity<ApiResources>());
        _ = new ApiResourceClaimsMap(modelBuilder.Entity<ApiResourceClaims>());
        _ = new ApiScopesMap(modelBuilder.Entity<ApiScopes>());
        _ = new ApiScopeClaimsMap(modelBuilder.Entity<ApiScopeClaims>());
        _ = new SecurityTokensMap(modelBuilder.Entity<SecurityTokens>());

        ApplyNullableStringMapping(modelBuilder);

        if (Database.IsNpgsql())
        {
            ApplyNpgsqlUserDateTimeMapping(modelBuilder);
            ApplyNpgsqlRowVersionMapping(modelBuilder);
        }
        else if (Database.IsSqlite()) ApplySqliteRowVersionMapping(modelBuilder);
    }

    /// <summary>
    /// Forces all non-key string properties to be nullable to accommodate legacy schema variations.
    /// </summary>
    private static void ApplyNullableStringMapping(ModelBuilder modelBuilder)
    {
        // Legacy schema has multiple optional TEXT columns; forcing nullable avoids runtime materialization failures.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        foreach (var property in entityType.GetProperties())
        {
            if (property.ClrType != typeof(string)) continue;

            if (property.IsPrimaryKey() || property.IsForeignKey()) continue;

            property.IsNullable = true;
        }
    }

    /// <summary>
    /// Builds a <see cref="FrameworkResult"/> based on the number of rows affected by the save operation.
    /// </summary>
    private static FrameworkResult BuildResult(int affectedRows)
    {
        if (affectedRows > 0) return new FrameworkResult { Status = ResultStatus.Succeeded };

        return BuildFailedResult(ApiErrorCodes.NoChangesWritten, "No changes written.");
    }

    /// <summary>
    /// Constructs a failed <see cref="FrameworkResult"/> with the given error code and description.
    /// </summary>
    private static FrameworkResult BuildFailedResult(string code, string description)
    {
        return new FrameworkResult
        {
            Status = ResultStatus.Failed,
            Errors = new List<FrameworkError>
            {
                new()
                {
                    Code = code,
                    Description = description
                }
            }
        };
    }

    /// <summary>
    /// Applies audit metadata (CreatedOn, ModifiedOn, CreatedBy, ModifiedBy) to tracked entities.
    /// When <paramref name="softDelete"/> is true, deleted entities are converted to modified
    /// entities with <c>IsDeleted = true</c> instead of being physically removed.
    /// </summary>
    /// <param name="softDelete">If true, intercepts deletes and converts them to soft-deletes.</param>
    private void ApplyAuditState(bool softDelete)
    {
        var utcNow = DateTime.UtcNow;
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedOn == default) entry.Entity.CreatedOn = utcNow;

                if (string.IsNullOrWhiteSpace(entry.Entity.CreatedBy)) entry.Entity.CreatedBy = "System";

                entry.Entity.ModifiedOn = null;
                if (entry.Entity.IsDeleted) entry.Entity.IsDeleted = false;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedOn = utcNow;
                if (string.IsNullOrWhiteSpace(entry.Entity.ModifiedBy))
                    entry.Entity.ModifiedBy = entry.Entity.CreatedBy;
            }
            else if (entry.State == EntityState.Deleted)
            {
                if (softDelete)
                {
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.ModifiedOn = utcNow;
                    if (string.IsNullOrWhiteSpace(entry.Entity.ModifiedBy))
                        entry.Entity.ModifiedBy = entry.Entity.CreatedBy;
                }
            }
    }

    /// <summary>
    /// Normalizes all DateTime property values on added or modified entities to UTC.
    /// Unspecified kinds are treated as UTC; Local kinds are converted to UTC.
    /// </summary>
    private void NormalizeDateTimesToUtc()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified)) continue;

            foreach (var property in entry.Properties)
            {
                var clrType = property.Metadata.ClrType;
                if (clrType != typeof(DateTime) && clrType != typeof(DateTime?)) continue;

                if (entry.State == EntityState.Modified && !property.IsModified) continue;

                if (property.CurrentValue is not DateTime value) continue;

                DateTime normalized;
                if (value.Kind == DateTimeKind.Unspecified)
                    normalized = DateTime.SpecifyKind(value, DateTimeKind.Utc);
                else if (value.Kind == DateTimeKind.Local)
                    normalized = value.ToUniversalTime();
                else
                    continue;

                if (normalized.Kind != value.Kind || normalized.Ticks != value.Ticks)
                    property.CurrentValue = normalized;
            }
        }
    }

    /// <summary>
    /// Configures PostgreSQL-specific "timestamp with time zone" column types for User date properties.
    /// </summary>
    private static void ApplyNpgsqlUserDateTimeMapping(ModelBuilder modelBuilder)
    {
        var user = modelBuilder.Entity<Users>();

        user.Property(x => x.DateOfBirth).HasColumnType("timestamp with time zone");
        user.Property(x => x.LastPasswordChangedDate).HasColumnType("timestamp with time zone");
        user.Property(x => x.LastLoginDateTime).HasColumnType("timestamp with time zone");
        user.Property(x => x.LastLogoutDateTime).HasColumnType("timestamp with time zone");
        user.Property(x => x.CreatedOn).HasColumnType("timestamp with time zone");
        user.Property(x => x.ModifiedOn).HasColumnType("timestamp with time zone");
    }

    /// <summary>
    /// Configures PostgreSQL row-version concurrency using the hidden "xmin" system column,
    /// converting byte[] RowVersion values to/from int64 for all concurrency-tracked entities.
    /// </summary>
    private static void ApplyNpgsqlRowVersionMapping(ModelBuilder modelBuilder)
    {
        var converter = new ValueConverter<byte[], long>(
            v => BitConverter.ToInt64(v, 0),
            v => BitConverter.GetBytes(v));

        var comparer = new ValueComparer<byte[]>(
            (left, right) =>
                left == right || (left != null && right != null && left.SequenceEqual(right)),
            value => value == null ? 0 : value.Aggregate(0, (hash, b) => HashCode.Combine(hash, b)),
            value => value == null ? null : value.ToArray());

        ConfigureNpgsqlRowVersion<SecurityQuestions>(modelBuilder, converter, comparer);
        ConfigureNpgsqlRowVersion<UserSecurityQuestions>(modelBuilder, converter, comparer);
        ConfigureNpgsqlRowVersion<PasswordHistory>(modelBuilder, converter, comparer);
        ConfigureNpgsqlRowVersion<UserClaims>(modelBuilder, converter, comparer);
        ConfigureNpgsqlRowVersion<RoleClaims>(modelBuilder, converter, comparer);
        ConfigureNpgsqlRowVersion<UserRoles>(modelBuilder, converter, comparer);
        ConfigureNpgsqlRowVersion<Notification>(modelBuilder, converter, comparer);
        ConfigureNpgsqlRowVersion<Clients>(modelBuilder, converter, comparer);
        ConfigureNpgsqlRowVersion<ClientRedirectUris>(modelBuilder, converter, comparer);
        ConfigureNpgsqlRowVersion<ClientPostLogoutRedirectUris>(modelBuilder, converter, comparer);
        ConfigureNpgsqlRowVersion<IdentityResources>(modelBuilder, converter, comparer);
        ConfigureNpgsqlRowVersion<IdentityClaims>(modelBuilder, converter, comparer);
        ConfigureNpgsqlRowVersion<ApiResources>(modelBuilder, converter, comparer);
        ConfigureNpgsqlRowVersion<ApiResourceClaims>(modelBuilder, converter, comparer);
        ConfigureNpgsqlRowVersion<ApiScopes>(modelBuilder, converter, comparer);
        ConfigureNpgsqlRowVersion<ApiScopeClaims>(modelBuilder, converter, comparer);
        ConfigureNpgsqlRowVersion<SecurityTokens>(modelBuilder, converter, comparer);
    }

    /// <summary>
    /// Configures a single entity type to use the PostgreSQL "xmin" column as its RowVersion concurrency token.
    /// </summary>
    private static void ConfigureNpgsqlRowVersion<TEntity>(
        ModelBuilder modelBuilder,
        ValueConverter<byte[], long> converter,
        ValueComparer<byte[]> comparer)
        where TEntity : class
    {
        var property = modelBuilder.Entity<TEntity>()
            .Property<byte[]>(nameof(BaseEntity.RowVersion))
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .HasConversion(converter);

        property.Metadata.SetValueComparer(comparer);
    }

    /// <summary>
    /// Configures SQLite-specific row-version handling by making RowVersion nullable and
    /// disabling automatic value generation, since SQLite lacks native row-versioning support.
    /// </summary>
    private static void ApplySqliteRowVersionMapping(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var rowVersionProperty = entityType.FindProperty(nameof(BaseEntity.RowVersion));
            if (rowVersionProperty == null) continue;

            rowVersionProperty.IsNullable = true;
            rowVersionProperty.ValueGenerated = ValueGenerated.Never;
        }
    }
}
