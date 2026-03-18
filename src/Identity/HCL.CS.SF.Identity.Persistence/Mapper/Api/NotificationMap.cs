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
/// EF Core entity type configuration that maps the <see cref="Notification"/> entity
/// to the "HCL.CS.SF_Notification" table. Configures notification type indexing,
/// property constraints for sender/recipient/message fields, and soft-delete query filter.
/// </summary>
public class NotificationMap
{
    /// <summary>
    /// Initializes the notification mapping.
    /// </summary>
    /// <param name="entityBuilder">The entity type builder for <see cref="Notification"/>.</param>
    public NotificationMap(EntityTypeBuilder<Notification> entityBuilder)
    {
        NotificationMapping(entityBuilder);
    }

    /// <summary>
    /// Configures table name, keys, indexes, property constraints, and soft-delete query filter.
    /// </summary>
    private void NotificationMapping(EntityTypeBuilder<Notification> entityBuilder)
    {
        if (entityBuilder != null)
        {
            // Primary Key
            entityBuilder.HasKey(t => t.Id);
            entityBuilder.HasIndex(t => t.Type).HasDatabaseName("IX_NOTI_TYPE");

            // Properties
            entityBuilder.Property(t => t.UserId).IsRequired();
            entityBuilder.Property(t => t.Type).IsRequired();
            entityBuilder.Property(t => t.Activity).HasMaxLength(255);
            entityBuilder.Property(t => t.Status).HasMaxLength(255);
            entityBuilder.Property(t => t.Sender).IsRequired().HasMaxLength(255);
            entityBuilder.Property(t => t.Recipient).IsRequired().HasMaxLength(255);
            entityBuilder.Property(t => t.MessageId).IsRequired().HasMaxLength(255);

            // Table & Column Mappings
            entityBuilder.ToTable("HCL.CS.SF_Notification");
            entityBuilder.Property(t => t.Id).HasColumnName("Id");
            entityBuilder.Property(t => t.IsDeleted).IsRequired().HasColumnName("IsDeleted");
            entityBuilder.Property(t => t.CreatedOn).IsRequired().HasColumnName("CreatedOn");
            entityBuilder.Property(t => t.CreatedBy).IsRequired().HasMaxLength(255).HasColumnName("CreatedBy");
            entityBuilder.Property(t => t.ModifiedOn).HasColumnName("ModifiedOn");
            entityBuilder.Property(t => t.ModifiedBy).HasMaxLength(255).HasColumnName("ModifiedBy");
            entityBuilder.HasQueryFilter(m => EF.Property<bool>(m, "IsDeleted") == false);

            entityBuilder.Ignore(x => x.RowVersion);
        }
    }
}
