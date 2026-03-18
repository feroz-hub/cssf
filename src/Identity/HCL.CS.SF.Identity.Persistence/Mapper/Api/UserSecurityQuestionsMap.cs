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
/// EF Core entity type configuration that maps the <see cref="UserSecurityQuestions"/> entity
/// to the "HCL.CS.SF_UserSecurityQuestions" table. Links users to their selected security
/// questions and answers with a unique constraint on (UserId, SecurityQuestionId).
/// </summary>
public class UserSecurityQuestionsMap
{
    /// <summary>
    /// Initializes the user security questions mapping.
    /// </summary>
    /// <param name="entityBuilder">The entity type builder for <see cref="UserSecurityQuestions"/>.</param>
    public UserSecurityQuestionsMap(EntityTypeBuilder<UserSecurityQuestions> entityBuilder)
    {
        UserSecurityQuestionsMapping(entityBuilder);
    }

    /// <summary>
    /// Configures table name, keys, unique indexes, and soft-delete query filter.
    /// </summary>
    private void UserSecurityQuestionsMapping(EntityTypeBuilder<UserSecurityQuestions> entityBuilder)
    {
        if (entityBuilder != null)
        {
            // Primary Key
            entityBuilder.HasKey(t => t.Id);
            entityBuilder.HasIndex(t => new { t.UserId, t.SecurityQuestionId }).HasDatabaseName("IX_USRSEC_UID_QUEID")
                .IsUnique();
            entityBuilder.HasIndex(t => new { t.SecurityQuestionId }).HasDatabaseName("IX_USRSEC_QUEID");

            // Properties
            entityBuilder.Property(t => t.Answer).IsRequired().HasMaxLength(255);

            // Table & Column Mappings
            entityBuilder.ToTable("HCL.CS.SF_UserSecurityQuestions");
            entityBuilder.Property(t => t.UserId).IsRequired().HasColumnName("UserId");
            entityBuilder.Property(t => t.SecurityQuestionId).IsRequired().HasColumnName("SecurityQuestionId");
            entityBuilder.Property(t => t.CreatedOn).IsRequired().HasColumnName("CreatedOn");
            entityBuilder.Property(t => t.CreatedBy).IsRequired().HasMaxLength(255).HasColumnName("CreatedBy");
            entityBuilder.Property(t => t.ModifiedOn).HasColumnName("ModifiedOn");
            entityBuilder.Property(t => t.ModifiedBy).HasMaxLength(255).HasColumnName("ModifiedBy");
            entityBuilder.HasQueryFilter(m => EF.Property<bool>(m, "IsDeleted") == false);
        }
    }
}
