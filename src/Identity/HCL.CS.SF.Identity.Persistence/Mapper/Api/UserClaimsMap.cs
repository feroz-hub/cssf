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
/// EF Core entity type configuration that maps the <see cref="UserClaims"/> entity
/// to the "HCL.CS.SF_UserClaims" table. Maps claim type/value pairs to users and
/// distinguishes between regular and admin claims via the IsAdminClaim flag.
/// </summary>
public class UserClaimsMap
{
    /// <summary>
    /// Initializes the user claims mapping.
    /// </summary>
    /// <param name="entityBuilder">The entity type builder for <see cref="UserClaims"/>.</param>
    public UserClaimsMap(EntityTypeBuilder<UserClaims> entityBuilder)
    {
        UserClaimMapping(entityBuilder);
    }

    /// <summary>
    /// Configures table name, keys, column mappings, and soft-delete query filter.
    /// </summary>
    private void UserClaimMapping(EntityTypeBuilder<UserClaims> entityBuilder)
    {
        if (entityBuilder != null)
        {
            // Primary Key
            entityBuilder.HasKey(t => t.Id);

            // Table & Column Mappings
            entityBuilder.ToTable("HCL.CS.SF_UserClaims");
            entityBuilder.Property(t => t.Id).HasColumnName("Id");
            entityBuilder.Property(t => t.UserId).IsRequired().HasColumnName("UserId");
            entityBuilder.Property(t => t.ClaimType).HasColumnName("ClaimType");
            entityBuilder.Property(t => t.ClaimValue).HasColumnName("ClaimValue");
            entityBuilder.Property(t => t.IsAdminClaim).HasColumnName("IsAdminClaim");
            entityBuilder.Property(t => t.IsDeleted).IsRequired().HasColumnName("IsDeleted");
            entityBuilder.Property(t => t.CreatedOn).IsRequired().HasColumnName("CreatedOn");
            entityBuilder.Property(t => t.CreatedBy).IsRequired().HasMaxLength(255).HasColumnName("CreatedBy");
            entityBuilder.Property(t => t.ModifiedOn).HasColumnName("ModifiedOn");
            entityBuilder.Property(t => t.ModifiedBy).HasMaxLength(255).HasColumnName("ModifiedBy");
            entityBuilder.HasQueryFilter(m => EF.Property<bool>(m, "IsDeleted") == false);
        }
    }
}
