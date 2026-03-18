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
/// EF Core entity type configuration that maps the <see cref="PasswordHistory"/> entity
/// to the "HCL.CS.SF_PasswordHistory" table. Tracks historical password hashes per user
/// to enforce password reuse policies. Ignores modification audit fields since password
/// history records are append-only.
/// </summary>
public class PasswordHistoryMap
{
    /// <summary>
    /// Initializes the password history mapping.
    /// </summary>
    /// <param name="entityBuilder">The entity type builder for <see cref="PasswordHistory"/>.</param>
    public PasswordHistoryMap(EntityTypeBuilder<PasswordHistory> entityBuilder)
    {
        PasswordHistoryMapping(entityBuilder);
    }

    /// <summary>
    /// Configures table name, keys, property constraints, and soft-delete query filter.
    /// </summary>
    private void PasswordHistoryMapping(EntityTypeBuilder<PasswordHistory> entityBuilder)
    {
        if (entityBuilder != null)
        {
            entityBuilder.ToTable("HCL.CS.SF_PasswordHistory");

            // Primary Key
            entityBuilder.HasKey(t => t.Id);

            // Properties
            entityBuilder.Property(t => t.Id).HasColumnName("Id");
            entityBuilder.Property(t => t.PasswordHash).IsRequired().HasMaxLength(255);
            entityBuilder.Property(t => t.UserId).IsRequired().HasColumnName("UserID");
            entityBuilder.Property(t => t.IsDeleted).IsRequired().HasColumnName("IsDeleted");
            entityBuilder.Property(t => t.ChangedOn).IsRequired().HasColumnName("ChangedOn");
            entityBuilder.Property(t => t.CreatedOn).IsRequired().HasColumnName("CreatedOn");
            entityBuilder.Property(t => t.CreatedBy).IsRequired().HasMaxLength(255).HasColumnName("CreatedBy");
            entityBuilder.HasQueryFilter(m => EF.Property<bool>(m, "IsDeleted") == false);

            entityBuilder.Ignore(x => x.ModifiedBy);
            entityBuilder.Ignore(x => x.ModifiedOn);
            entityBuilder.Ignore(x => x.RowVersion);
        }
    }
}
