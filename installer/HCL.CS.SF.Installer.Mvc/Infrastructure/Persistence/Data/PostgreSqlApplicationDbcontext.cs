/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.Infrastructure.Data.Mapper.Api;
using HCL.CS.SF.Infrastructure.Data.Mapper.Endpoint;

namespace HCLCSSFInstallerMVC.Infrastructure.Persistence.Data;

/// <summary>
/// PostgreSQL-specific DbContext used for running EF Core migrations against a PostgreSQL database.
/// Applies xid-based RowVersion conversions required by PostgreSQL's optimistic concurrency model.
/// </summary>
public class PostgreSqlApplicationDbcontext :
    IdentityDbContext<Users, Roles, Guid, UserClaims, UserRoles, UserLogins, RoleClaims, UserTokens>
{
    /// <summary>Parameterless constructor for design-time tooling.</summary>
    public PostgreSqlApplicationDbcontext()
    {
    }

    /// <summary>Initializes the context with PostgreSQL-specific options.</summary>
    public PostgreSqlApplicationDbcontext(DbContextOptions<PostgreSqlApplicationDbcontext> options)
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
    /// Configures entity mappings with restrict-delete foreign keys and PostgreSQL xid-based
    /// RowVersion conversions for optimistic concurrency.
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

        var converter = new ValueConverter<byte[], long>(
            v => BitConverter.ToInt64(v, 0),
            v => BitConverter.GetBytes(v));

        modelBuilder.Entity<SecurityQuestions>()
            .Property(_ => _.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .HasConversion(converter);

        modelBuilder.Entity<UserSecurityQuestions>()
            .Property(_ => _.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .HasConversion(converter);

        modelBuilder.Entity<PasswordHistory>()
            .Property(_ => _.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .HasConversion(converter);

        modelBuilder.Entity<UserClaims>()
            .Property(_ => _.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .HasConversion(converter);

        modelBuilder.Entity<RoleClaims>()
            .Property(_ => _.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .HasConversion(converter);

        modelBuilder.Entity<UserRoles>()
            .Property(_ => _.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .HasConversion(converter);

        modelBuilder.Entity<Notification>()
            .Property(_ => _.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .HasConversion(converter);

        modelBuilder.Entity<Clients>()
            .Property(_ => _.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .HasConversion(converter);

        modelBuilder.Entity<ClientRedirectUris>()
            .Property(_ => _.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .HasConversion(converter);

        modelBuilder.Entity<ClientPostLogoutRedirectUris>()
            .Property(_ => _.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .HasConversion(converter);

        modelBuilder.Entity<IdentityResources>()
            .Property(_ => _.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .HasConversion(converter);

        modelBuilder.Entity<IdentityClaims>()
            .Property(_ => _.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .HasConversion(converter);

        modelBuilder.Entity<ApiResources>()
            .Property(_ => _.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .HasConversion(converter);

        modelBuilder.Entity<ApiResourceClaims>()
            .Property(_ => _.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .HasConversion(converter);

        modelBuilder.Entity<ApiScopes>()
            .Property(_ => _.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .HasConversion(converter);

        modelBuilder.Entity<ApiScopeClaims>()
            .Property(_ => _.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .HasConversion(converter);

        modelBuilder.Entity<SecurityTokens>()
            .Property(_ => _.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .HasConversion(converter);
    }
}
