/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HCL.CS.SF.Domain.Entities.Api;

namespace HCL.CS.SF.Infrastructure.Data.Mapper.Endpoint;

/// <summary>
/// EF Core entity type configuration that maps the <see cref="ApiScopes"/> entity
/// to the "HCL.CS.SF_ApiScopes" table. Configures a unique composite index on
/// (ApiResourceId, Name), cascade-delete to ApiScopeClaims, and soft-delete query filter.
/// </summary>
public class ApiScopesMap
{
    /// <summary>
    /// Initializes the API scopes mapping.
    /// </summary>
    /// <param name="entityBuilder">The entity type builder for <see cref="ApiScopes"/>.</param>
    public ApiScopesMap(EntityTypeBuilder<ApiScopes> entityBuilder)
    {
        ApiScopesMapping(entityBuilder);
    }

    /// <summary>
    /// Configures table name, keys, unique index, cascade-delete to scope claims, and soft-delete query filter.
    /// </summary>
    private void ApiScopesMapping(EntityTypeBuilder<ApiScopes> entityBuilder)
    {
        if (entityBuilder != null)
        {
            entityBuilder.ToTable("HCL.CS.SF_ApiScopes");

            // Primary Key
            entityBuilder.HasKey(x => x.Id);
            entityBuilder.HasIndex(t => new { t.ApiResourceId, t.Name }).HasDatabaseName("IX_APISCO_SCOID_NAME")
                .IsUnique();

            // Properties
            entityBuilder.Property(x => x.ApiResourceId).IsRequired();

            // Table & Column Mappings
            entityBuilder.Property(x => x.Name).IsRequired().HasMaxLength(255).HasColumnName("Name");
            entityBuilder.Property(x => x.DisplayName).HasMaxLength(255).HasColumnName("DisplayName");
            entityBuilder.Property(x => x.IsDeleted).IsRequired().HasColumnName("IsDeleted");
            entityBuilder.Property(x => x.CreatedOn).IsRequired().HasColumnName("CreatedOn");
            entityBuilder.Property(x => x.CreatedBy).IsRequired().HasMaxLength(255).HasColumnName("CreatedBy");
            entityBuilder.Property(x => x.ModifiedOn).HasColumnName("ModifiedOn");
            entityBuilder.Property(x => x.ModifiedBy).HasMaxLength(255).HasColumnName("ModifiedBy");

            // Mappings
            entityBuilder.HasMany(x => x.ApiScopeClaims).WithOne(x => x.ApiScope)
                .HasForeignKey(x => x.ApiScopeId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            entityBuilder.HasQueryFilter(m => EF.Property<bool>(m, "IsDeleted") == false);
        }
    }
}
