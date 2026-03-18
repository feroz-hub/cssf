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
/// EF Core entity type configuration that maps the <see cref="Users"/> entity
/// to the "HCL.CS.SF_Users" table. Configures user profile properties (name, email, phone),
/// security fields (password hash, security stamp, concurrency stamp), login tracking
/// (LastLoginDateTime, LastLogoutDateTime), and soft-delete query filter.
/// </summary>
public class UsersMap
{
    /// <summary>
    /// Initializes the users mapping.
    /// </summary>
    /// <param name="entityBuilder">The entity type builder for <see cref="Users"/>.</param>
    public UsersMap(EntityTypeBuilder<Users> entityBuilder)
    {
        UserMapping(entityBuilder);
    }

    /// <summary>
    /// Configures table name, keys, property constraints, column mappings, and soft-delete query filter.
    /// </summary>
    private void UserMapping(EntityTypeBuilder<Users> entityBuilder)
    {
        if (entityBuilder != null)
        {
            entityBuilder.ToTable("HCL.CS.SF_Users");

            // Primary Key
            entityBuilder.HasKey(t => t.Id);

            // Properties
            entityBuilder.Property(t => t.UserName).IsRequired().HasMaxLength(255);
            entityBuilder.Property(t => t.NormalizedUserName).IsRequired().HasMaxLength(255);
            entityBuilder.Property(t => t.FirstName).IsRequired().HasMaxLength(255);
            entityBuilder.Property(t => t.LastName).HasMaxLength(255);
            entityBuilder.Property(t => t.Email).IsRequired().HasMaxLength(255);
            entityBuilder.Property(t => t.NormalizedEmail).IsRequired().HasMaxLength(255);
            entityBuilder.Property(t => t.PhoneNumber).HasMaxLength(15);
            entityBuilder.Property(t => t.ConcurrencyStamp).HasMaxLength(255);
            entityBuilder.Property(t => t.SecurityStamp).HasMaxLength(255);

            // Table & Column Mappings
            entityBuilder.Property(t => t.Id).HasColumnName("Id");
            entityBuilder.Property(t => t.EmailConfirmed).IsRequired().HasColumnName("EmailConfirmed");
            entityBuilder.Property(t => t.PhoneNumberConfirmed).HasColumnName("PhoneNumberConfirmed");
            entityBuilder.Property(t => t.DateOfBirth).HasColumnName("DateOfBirth");
            entityBuilder.Property(t => t.PasswordHash).IsRequired().HasColumnName("PasswordHash");
            entityBuilder.Property(t => t.LastPasswordChangedDate).HasColumnName("LastPasswordChangedDate");
            entityBuilder.Property(t => t.RequiresDefaultPasswordChange).HasColumnName("RequiresDefaultPasswordChange");
            entityBuilder.Property(t => t.LastLoginDateTime).HasColumnName("LastLoginDateTime");
            entityBuilder.Property(t => t.LastLogoutDateTime).HasColumnName("LastLogoutDateTime");
            entityBuilder.Property(t => t.IdentityProviderType).HasColumnName("IdentityProviderType");
            entityBuilder.Property(t => t.IsDeleted).IsRequired().HasColumnName("IsDeleted");
            entityBuilder.Property(t => t.CreatedOn).IsRequired().HasColumnName("CreatedOn");
            entityBuilder.Property(t => t.CreatedBy).IsRequired().HasMaxLength(255).HasColumnName("CreatedBy");
            entityBuilder.Property(t => t.ModifiedOn).HasColumnName("ModifiedOn");
            entityBuilder.Property(t => t.ModifiedBy).HasMaxLength(255).HasColumnName("ModifiedBy");
            entityBuilder.HasQueryFilter(m => EF.Property<bool>(m, "IsDeleted") == false);
        }
    }
}
