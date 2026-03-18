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

namespace HCL.CS.SF.Infrastructure.Data.Mapper.Api;

/// <summary>
/// EF Core entity type configuration that maps the <see cref="Roles"/> entity
/// to the "HCL.CS.SF_Roles" table. Configures the role name, normalized name,
/// concurrency stamp, and optional description with soft-delete query filter.
/// </summary>
public class RolesMap
{
    /// <summary>
    /// Initializes the roles mapping.
    /// </summary>
    /// <param name="entityBuilder">The entity type builder for <see cref="Roles"/>.</param>
    public RolesMap(EntityTypeBuilder<Roles> entityBuilder)
    {
        RolesMapping(entityBuilder);
    }

    /// <summary>
    /// Configures table name, keys, property constraints, and soft-delete query filter.
    /// </summary>
    private void RolesMapping(EntityTypeBuilder<Roles> entityBuilder)
    {
        if (entityBuilder != null)
        {
            // Primary Key
            entityBuilder.HasKey(t => t.Id);

            // Properties
            entityBuilder.Property(t => t.Name).IsRequired().HasMaxLength(255);
            entityBuilder.Property(t => t.NormalizedName).IsRequired().HasMaxLength(255);
            entityBuilder.Property(t => t.ConcurrencyStamp).IsRequired().HasMaxLength(255);

            // Table & Column Mappings
            entityBuilder.ToTable("HCL.CS.SF_Roles");
            entityBuilder.Property(t => t.Id).HasColumnName("Id");
            entityBuilder.Property(t => t.Description).HasColumnName("Description");
            entityBuilder.Property(t => t.IsDeleted).IsRequired().HasColumnName("IsDeleted");
            entityBuilder.Property(t => t.CreatedOn).IsRequired().HasColumnName("CreatedOn");
            entityBuilder.Property(t => t.CreatedBy).IsRequired().HasMaxLength(255);
            entityBuilder.Property(t => t.ModifiedOn).HasColumnName("ModifiedOn");
            entityBuilder.Property(t => t.ModifiedBy).HasMaxLength(255).HasColumnName("ModifiedBy");
            entityBuilder.HasQueryFilter(m => EF.Property<bool>(m, "IsDeleted") == false);
        }
    }
}
