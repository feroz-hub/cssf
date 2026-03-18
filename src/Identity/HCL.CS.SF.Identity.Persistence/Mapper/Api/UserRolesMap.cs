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
/// EF Core entity type configuration that maps the <see cref="UserRoles"/> entity
/// to the "HCL.CS.SF_UserRoles" table. Uses a composite primary key of Id, UserId,
/// and RoleId to represent the many-to-many relationship between users and roles.
/// </summary>
public class UserRolesMap
{
    /// <summary>
    /// Initializes the user roles mapping.
    /// </summary>
    /// <param name="entityBuilder">The entity type builder for <see cref="UserRoles"/>.</param>
    public UserRolesMap(EntityTypeBuilder<UserRoles> entityBuilder)
    {
        UserRoleMapping(entityBuilder);
    }

    /// <summary>
    /// Configures table name, composite key, column mappings, and soft-delete query filter.
    /// </summary>
    private void UserRoleMapping(EntityTypeBuilder<UserRoles> entityBuilder)
    {
        if (entityBuilder != null)
        {
            // Primary Key
            entityBuilder.HasKey(r => new { r.Id, r.UserId, r.RoleId });

            // Properties
            // Table & Column Mappings
            entityBuilder.ToTable("HCL.CS.SF_UserRoles");
            entityBuilder.Property(t => t.Id).HasColumnName("Id");
            entityBuilder.Property(t => t.IsDeleted).IsRequired().HasColumnName("IsDeleted");
            entityBuilder.Property(t => t.CreatedOn).IsRequired().HasColumnName("CreatedOn");
            entityBuilder.Property(t => t.CreatedBy).IsRequired().HasMaxLength(255).HasColumnName("CreatedBy");
            entityBuilder.Property(t => t.ModifiedOn).HasColumnName("ModifiedOn");
            entityBuilder.Property(t => t.ModifiedBy).HasMaxLength(255).HasColumnName("ModifiedBy");
            entityBuilder.HasQueryFilter(m => EF.Property<bool>(m, "IsDeleted") == false);
        }
    }
}
