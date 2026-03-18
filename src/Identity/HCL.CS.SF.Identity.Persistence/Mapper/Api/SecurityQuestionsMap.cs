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
/// EF Core entity type configuration that maps the <see cref="SecurityQuestions"/> entity
/// to the "HCL.CS.SF_SecurityQuestions" table. Enforces a unique index on the question text
/// to prevent duplicates and applies a soft-delete query filter.
/// </summary>
public class SecurityQuestionsMap
{
    /// <summary>
    /// Initializes the security questions mapping.
    /// </summary>
    /// <param name="entityBuilder">The entity type builder for <see cref="SecurityQuestions"/>.</param>
    public SecurityQuestionsMap(EntityTypeBuilder<SecurityQuestions> entityBuilder)
    {
        SecurityQuestionsMapping(entityBuilder);
    }

    /// <summary>
    /// Configures table name, keys, unique question index, and soft-delete query filter.
    /// </summary>
    private void SecurityQuestionsMapping(EntityTypeBuilder<SecurityQuestions> entityBuilder)
    {
        if (entityBuilder != null)
        {
            // Primary Key
            entityBuilder.HasKey(t => t.Id);
            entityBuilder.HasIndex(t => t.Question).HasDatabaseName("IX_SEC_QUESTION").IsUnique();

            // Properties
            entityBuilder.Property(t => t.Question).IsRequired().HasMaxLength(255);

            // Table & Column Mappings
            entityBuilder.ToTable("HCL.CS.SF_SecurityQuestions");
            entityBuilder.Property(t => t.Id).HasColumnName("Id");
            entityBuilder.Property(t => t.IsDeleted).IsRequired().HasColumnName("IsDeleted");
            entityBuilder.Property(t => t.CreatedOn).IsRequired().HasColumnName("CreatedOn");
            entityBuilder.Property(t => t.CreatedBy).IsRequired().HasMaxLength(255).HasColumnName("CreatedBy");
            entityBuilder.Property(t => t.ModifiedBy).HasMaxLength(255).HasColumnName("ModifiedBy");
            entityBuilder.Property(t => t.ModifiedOn).HasColumnName("ModifiedOn");
            entityBuilder.HasQueryFilter(m => EF.Property<bool>(m, "IsDeleted") == false);
        }
    }
}
