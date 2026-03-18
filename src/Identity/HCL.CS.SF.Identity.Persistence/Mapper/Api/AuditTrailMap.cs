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
/// EF Core entity type configuration that maps the <see cref="AuditTrail"/> entity to the
/// "HCL.CS.SF_AuditTrail" table. Configures the primary key, composite indexes for efficient
/// audit querying by date/user/action, and ignores soft-delete/modification audit fields
/// since audit records are immutable once created.
/// </summary>
public class AuditTrailMap
{
    /// <summary>
    /// Initializes the audit trail mapping by applying the entity configuration.
    /// </summary>
    /// <param name="entityBuilder">The entity type builder for <see cref="AuditTrail"/>.</param>
    public AuditTrailMap(EntityTypeBuilder<AuditTrail> entityBuilder)
    {
        AuditMapping(entityBuilder);
    }

    /// <summary>
    /// Configures table name, primary key, indexes, property constraints, and ignored properties.
    /// </summary>
    private void AuditMapping(EntityTypeBuilder<AuditTrail> entityBuilder)
    {
        if (entityBuilder != null)
        {
            entityBuilder.ToTable("HCL.CS.SF_AuditTrail");
            entityBuilder.HasKey(t => t.Id);
            entityBuilder.HasIndex(t => new { t.CreatedOn, t.CreatedBy }).HasDatabaseName("IX_AUD_CRON_CBBY");
            entityBuilder.HasIndex(t => new { t.CreatedOn, t.ActionType }).HasDatabaseName("IX_AUD_CRON_ACTY");
            entityBuilder.HasIndex(t => new { t.CreatedBy, t.ActionType }).HasDatabaseName("IX_AUD_CBBY_ACTY");

            entityBuilder.Property(t => t.ActionType).IsRequired().HasMaxLength(50);
            entityBuilder.Property(t => t.TableName).HasMaxLength(255);
            entityBuilder.Property(t => t.AffectedColumn);
            entityBuilder.Property(t => t.OldValue);
            entityBuilder.Property(t => t.NewValue);
            entityBuilder.Property(t => t.CreatedOn).IsRequired();
            entityBuilder.Property(t => t.CreatedBy).IsRequired().HasMaxLength(255);

            entityBuilder.Ignore(x => x.IsDeleted);
            entityBuilder.Ignore(x => x.ModifiedBy);
            entityBuilder.Ignore(x => x.ModifiedOn);
            entityBuilder.Ignore(x => x.RowVersion);
        }
    }
}
