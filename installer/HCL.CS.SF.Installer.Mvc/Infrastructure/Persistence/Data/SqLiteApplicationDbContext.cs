/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.Infrastructure.Data;
using HCL.CS.SF.Infrastructure.Data.Mapper.Api;
using HCL.CS.SF.Infrastructure.Data.Mapper.Endpoint;

namespace HCLCSSFInstallerMVC.Infrastructure.Persistence.Data;

/// <summary>
/// SQLite-specific DbContext used for running EF Core migrations against a SQLite file database.
/// Applies SQLite-compatible timestamp conversions for RowVersion concurrency tokens.
/// </summary>
public class SqLiteApplicationDbContext :
    IdentityDbContext<Users, Roles, Guid, UserClaims, UserRoles, UserLogins, RoleClaims, UserTokens>
{
    /// <summary>Parameterless constructor for design-time tooling.</summary>
    public SqLiteApplicationDbContext()
    {
    }

    /// <summary>Initializes the context with SQLite-specific options.</summary>
    public SqLiteApplicationDbContext(DbContextOptions<SqLiteApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditTrail> AuditTrail { get; set; }
    public virtual DbSet<SecurityQuestions> SecurityQuestions { get; set; }
    public virtual DbSet<UserSecurityQuestions> UserSecurityQuestions { get; set; }
    public virtual DbSet<PasswordHistory> PasswordHistory { get; set; }
    public virtual DbSet<Notification> Notification { get; set; }
    public virtual DbSet<ExternalIdentities> ExternalIdentities { get; set; }

    public virtual DbSet<Clients> Clients { get; set; }
    public virtual DbSet<ClientRedirectUris> RedirectUris { get; set; }
    public virtual DbSet<ClientPostLogoutRedirectUris> PostLogoutRedirectUris { get; set; }

    public override DbSet<Roles> Roles { get; set; }
    public override DbSet<RoleClaims> RoleClaims { get; set; }
    public override DbSet<UserClaims> UserClaims { get; set; }

    public virtual DbSet<ApiResources> ApiResources { get; set; }
    public virtual DbSet<ApiResourceClaims> ApiResourceClaims { get; set; }
    public virtual DbSet<ApiScopes> ApiScopes { get; set; }
    public virtual DbSet<ApiScopeClaims> ApiScopeClaims { get; set; }

    public virtual DbSet<IdentityResources> IdentityResources { get; set; }
    public virtual DbSet<IdentityClaims> IdentityClaims { get; set; }

    public virtual DbSet<SecurityTokens> SecurityTokens { get; set; }
    public virtual DbSet<NotificationProviderConfig> NotificationProviderConfig { get; set; }
    public virtual DbSet<ExternalAuthProviderConfig> ExternalAuthProviderConfig { get; set; }

    /// <summary>
    /// Configures entity mappings with restrict-delete foreign keys and SQLite BLOB-based
    /// timestamp conversions for RowVersion concurrency tokens.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            relationship.DeleteBehavior = DeleteBehavior.Restrict;

        // Order should not be changed
        // API
        new AuditTrailMap(modelBuilder.Entity<AuditTrail>());
        new UsersMap(modelBuilder.Entity<Users>());
        new SecurityQuestionsMap(modelBuilder.Entity<SecurityQuestions>());
        new UserSecurityQuestionsMap(modelBuilder.Entity<UserSecurityQuestions>());
        new PasswordHistoryMap(modelBuilder.Entity<PasswordHistory>());
        new UserClaimsMap(modelBuilder.Entity<UserClaims>());
        new UserLoginsMap(modelBuilder.Entity<UserLogins>());
        new UserTokensMap(modelBuilder.Entity<UserTokens>());
        new RolesMap(modelBuilder.Entity<Roles>());
        new RoleClaimsMap(modelBuilder.Entity<RoleClaims>());
        new UserRolesMap(modelBuilder.Entity<UserRoles>());
        new NotificationMap(modelBuilder.Entity<Notification>());
        new ExternalIdentitiesMap(modelBuilder.Entity<ExternalIdentities>());

        // Endpoints
        new ClientsMap(modelBuilder.Entity<Clients>());
        new ClientRedirectUrisMap(modelBuilder.Entity<ClientRedirectUris>());
        new ClientPostLogoutRedirectUrisMap(modelBuilder.Entity<ClientPostLogoutRedirectUris>());
        new IdentityResourcesMap(modelBuilder.Entity<IdentityResources>());
        new IdentityClaimsMap(modelBuilder.Entity<IdentityClaims>());
        new ApiResourcesMap(modelBuilder.Entity<ApiResources>());
        new ApiResourceClaimsMap(modelBuilder.Entity<ApiResourceClaims>());
        new ApiScopesMap(modelBuilder.Entity<ApiScopes>());
        new ApiScopeClaimsMap(modelBuilder.Entity<ApiScopeClaims>());
        new SecurityTokensMap(modelBuilder.Entity<SecurityTokens>());
        new NotificationProviderConfigMap(modelBuilder.Entity<NotificationProviderConfig>());
        new ExternalAuthProviderConfigMap(modelBuilder.Entity<ExternalAuthProviderConfig>());

        modelBuilder.Entity<SecurityQuestions>()
            .Property(p => p.RowVersion)
            .IsRowVersion()
            .HasConversion(new SqliteTimestampConverter())
            .HasColumnType("BLOB")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<UserSecurityQuestions>()
            .Property(p => p.RowVersion)
            .IsRowVersion()
            .HasConversion(new SqliteTimestampConverter())
            .HasColumnType("BLOB")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<PasswordHistory>()
            .Property(p => p.RowVersion)
            .IsRowVersion()
            .HasConversion(new SqliteTimestampConverter())
            .HasColumnType("BLOB")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<UserClaims>()
            .Property(p => p.RowVersion)
            .IsRowVersion()
            .HasConversion(new SqliteTimestampConverter())
            .HasColumnType("BLOB")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<RoleClaims>()
            .Property(p => p.RowVersion)
            .IsRowVersion()
            .HasConversion(new SqliteTimestampConverter())
            .HasColumnType("BLOB")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<UserRoles>()
            .Property(p => p.RowVersion)
            .IsRowVersion()
            .HasConversion(new SqliteTimestampConverter())
            .HasColumnType("BLOB")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<Notification>()
            .Property(p => p.RowVersion)
            .IsRowVersion()
            .HasConversion(new SqliteTimestampConverter())
            .HasColumnType("BLOB")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<Clients>()
            .Property(p => p.RowVersion)
            .IsRowVersion()
            .HasConversion(new SqliteTimestampConverter())
            .HasColumnType("BLOB")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<ClientRedirectUris>()
            .Property(p => p.RowVersion)
            .IsRowVersion()
            .HasConversion(new SqliteTimestampConverter())
            .HasColumnType("BLOB")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");


        modelBuilder.Entity<ClientPostLogoutRedirectUris>()
            .Property(p => p.RowVersion)
            .IsRowVersion()
            .HasConversion(new SqliteTimestampConverter())
            .HasColumnType("BLOB")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<IdentityResources>()
            .Property(p => p.RowVersion)
            .IsRowVersion()
            .HasConversion(new SqliteTimestampConverter())
            .HasColumnType("BLOB")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<IdentityClaims>()
            .Property(p => p.RowVersion)
            .IsRowVersion()
            .HasConversion(new SqliteTimestampConverter())
            .HasColumnType("BLOB")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<ApiResources>()
            .Property(p => p.RowVersion)
            .IsRowVersion()
            .HasConversion(new SqliteTimestampConverter())
            .HasColumnType("BLOB")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<ApiResourceClaims>()
            .Property(p => p.RowVersion)
            .IsRowVersion()
            .HasConversion(new SqliteTimestampConverter())
            .HasColumnType("BLOB")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<ApiScopes>()
            .Property(p => p.RowVersion)
            .IsRowVersion()
            .HasConversion(new SqliteTimestampConverter())
            .HasColumnType("BLOB")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<ApiScopeClaims>()
            .Property(p => p.RowVersion)
            .IsRowVersion()
            .HasConversion(new SqliteTimestampConverter())
            .HasColumnType("BLOB")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<SecurityTokens>()
            .Property(p => p.RowVersion)
            .IsRowVersion()
            .HasConversion(new SqliteTimestampConverter())
            .HasColumnType("BLOB")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
