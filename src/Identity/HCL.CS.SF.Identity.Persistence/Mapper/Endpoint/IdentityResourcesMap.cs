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
/// EF Core entity type configuration that maps the <see cref="IdentityResources"/> entity
/// to the "HCL.CS.SF_IdentityResources" table. Configures a unique index on Name,
/// cascade-delete relationship to IdentityClaims, and soft-delete query filter.
/// </summary>
public class IdentityResourcesMap
{
    /// <summary>
    /// Initializes the identity resources mapping.
    /// </summary>
    /// <param name="entityBuilder">The entity type builder for <see cref="IdentityResources"/>.</param>
    public IdentityResourcesMap(EntityTypeBuilder<IdentityResources> entityBuilder)
    {
        IdentityResourcesMapping(entityBuilder);
    }

    /// <summary>
    /// Configures table name, keys, unique name index, cascade-delete to identity claims, and soft-delete query filter.
    /// </summary>
    private void IdentityResourcesMapping(EntityTypeBuilder<IdentityResources> entityBuilder)
    {
        if (entityBuilder != null)
        {
            entityBuilder.ToTable("HCL.CS.SF_IdentityResources");

            // Primary Key
            entityBuilder.HasKey(x => x.Id);
            entityBuilder.HasIndex(t => t.Name).HasDatabaseName("IX_IDRES_NAME").IsUnique();

            // Properties
            entityBuilder.Property(x => x.Name).HasMaxLength(255).IsRequired();
            entityBuilder.Property(x => x.DisplayName).HasMaxLength(255);

            // Table & Column Mappings
            entityBuilder.Property(x => x.IsDeleted).IsRequired().HasColumnName("IsDeleted");
            entityBuilder.Property(x => x.CreatedOn).IsRequired().HasColumnName("CreatedOn");
            entityBuilder.Property(x => x.CreatedBy).IsRequired().HasMaxLength(255).HasColumnName("CreatedBy");
            entityBuilder.Property(x => x.ModifiedOn).HasColumnName("ModifiedOn");
            entityBuilder.Property(x => x.ModifiedBy).HasMaxLength(255).HasColumnName("ModifiedBy");

            // Mappings
            entityBuilder.HasMany(x => x.IdentityClaims).WithOne(x => x.IdentityResource)
                .HasForeignKey(x => x.IdentityResourceId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            entityBuilder
                .HasQueryFilter(m => EF.Property<bool>(m, "IsDeleted") == false);
        }
    }
}
