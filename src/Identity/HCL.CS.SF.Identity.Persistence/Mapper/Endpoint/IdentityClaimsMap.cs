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
/// EF Core entity type configuration that maps the <see cref="IdentityClaims"/> entity
/// to the "HCL.CS.SF_IdentityClaims" table. Configures a unique composite index on
/// (IdentityResourceId, Type) and supports an optional AliasType for claim type aliasing.
/// </summary>
public class IdentityClaimsMap
{
    /// <summary>
    /// Initializes the identity claims mapping.
    /// </summary>
    /// <param name="entityBuilder">The entity type builder for <see cref="IdentityClaims"/>.</param>
    public IdentityClaimsMap(EntityTypeBuilder<IdentityClaims> entityBuilder)
    {
        IdentityClaimsMapping(entityBuilder);
    }

    /// <summary>
    /// Configures table name, keys, unique index, property constraints, and soft-delete query filter.
    /// </summary>
    private void IdentityClaimsMapping(EntityTypeBuilder<IdentityClaims> entityBuilder)
    {
        if (entityBuilder != null)
        {
            entityBuilder.ToTable("HCL.CS.SF_IdentityClaims");

            // Primary Key
            entityBuilder.HasKey(x => x.Id);
            entityBuilder.HasIndex(t => new { t.IdentityResourceId, t.Type })
                .HasDatabaseName("IX_IDRESCLM_IDRESID_TYPE").IsUnique();

            // Properties
            entityBuilder.Property(x => x.IdentityResourceId).IsRequired();

            entityBuilder.Property(x => x.Type).HasMaxLength(255).IsRequired();
            entityBuilder.Property(x => x.AliasType).HasMaxLength(255);

            // Table & Column Mappings
            entityBuilder.Property(x => x.IsDeleted).IsRequired().HasColumnName("IsDeleted");
            entityBuilder.Property(x => x.CreatedOn).IsRequired().HasColumnName("CreatedOn");
            entityBuilder.Property(x => x.CreatedBy).IsRequired().HasMaxLength(255).HasColumnName("CreatedBy");
            entityBuilder.Property(x => x.ModifiedOn).HasColumnName("ModifiedOn");
            entityBuilder.Property(x => x.ModifiedBy).HasMaxLength(255).HasColumnName("ModifiedBy");
            entityBuilder.HasQueryFilter(m => EF.Property<bool>(m, "IsDeleted") == false);
        }
    }
}
