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
/// EF Core entity type configuration that maps the <see cref="UserLogins"/> entity
/// to the "HCL.CS.SF_UserLogins" table. Uses a composite key of LoginProvider, ProviderKey,
/// and UserId to uniquely identify external login associations.
/// </summary>
public class UserLoginsMap
{
    /// <summary>
    /// Initializes the user logins mapping.
    /// </summary>
    /// <param name="entityBuilder">The entity type builder for <see cref="UserLogins"/>.</param>
    public UserLoginsMap(EntityTypeBuilder<UserLogins> entityBuilder)
    {
        UserLoginMapping(entityBuilder);
    }

    /// <summary>
    /// Configures table name, composite key, property constraints, and soft-delete query filter.
    /// </summary>
    private void UserLoginMapping(EntityTypeBuilder<UserLogins> entityBuilder)
    {
        if (entityBuilder != null)
        {
            // Primary Key
            entityBuilder.HasKey(t => t.Id);
            entityBuilder.HasKey(l => new { l.LoginProvider, l.ProviderKey, l.UserId });

            // Properties
            entityBuilder.Property(t => t.LoginProvider).IsRequired().HasMaxLength(256);
            entityBuilder.Property(t => t.ProviderKey).IsRequired().HasMaxLength(256);

            // Table & Column Mappings
            entityBuilder.ToTable("HCL.CS.SF_UserLogins");
            entityBuilder.Property(t => t.UserId).IsRequired().HasColumnName("UserId");
            entityBuilder.Property(t => t.ProviderDisplayName).HasColumnName("ProviderDisplayName");
            entityBuilder.Property(t => t.IsDeleted).IsRequired().HasColumnName("IsDeleted");
            entityBuilder.Property(t => t.CreatedOn).IsRequired().HasColumnName("CreatedOn");
            entityBuilder.Property(t => t.CreatedBy).IsRequired().HasMaxLength(255).HasColumnName("CreatedBy");
            entityBuilder.Property(t => t.ModifiedOn).HasColumnName("ModifiedOn");
            entityBuilder.Property(t => t.ModifiedBy).HasMaxLength(255).HasColumnName("ModifiedBy");
            entityBuilder.HasQueryFilter(m => EF.Property<bool>(m, "IsDeleted") == false);
        }
    }
}
