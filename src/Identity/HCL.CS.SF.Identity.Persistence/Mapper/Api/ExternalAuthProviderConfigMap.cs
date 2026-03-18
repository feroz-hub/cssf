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
/// EF Core entity type configuration that maps the <see cref="ExternalAuthProviderConfig"/> entity
/// to the "HCL.CS.SF_ExternalAuthProviderConfig" table. Configures unique provider name indexing,
/// composite indexes for enabled-provider lookups, property constraints, soft-delete query filter,
/// and column mappings for audit fields.
/// </summary>
public class ExternalAuthProviderConfigMap
{
    /// <summary>
    /// Initializes the external auth provider configuration mapping.
    /// </summary>
    /// <param name="entityBuilder">The entity type builder for <see cref="ExternalAuthProviderConfig"/>.</param>
    public ExternalAuthProviderConfigMap(EntityTypeBuilder<ExternalAuthProviderConfig> entityBuilder)
    {
        ExternalAuthProviderConfigMapping(entityBuilder);
    }

    /// <summary>
    /// Configures table name, keys, indexes, property constraints, and soft-delete query filter.
    /// </summary>
    private void ExternalAuthProviderConfigMapping(EntityTypeBuilder<ExternalAuthProviderConfig> entityBuilder)
    {
        if (entityBuilder != null)
        {
            // Primary Key
            entityBuilder.HasKey(t => t.Id);
            entityBuilder.HasIndex(t => t.ProviderName).IsUnique().HasDatabaseName("IX_EAPC_PROVIDER");
            entityBuilder.HasIndex(t => new { t.ProviderName, t.IsEnabled }).HasDatabaseName("IX_EAPC_PROVIDER_ENABLED");

            // Properties
            entityBuilder.Property(t => t.ProviderName).IsRequired().HasMaxLength(50);
            entityBuilder.Property(t => t.ProviderType).IsRequired();
            entityBuilder.Property(t => t.IsEnabled).IsRequired();
            entityBuilder.Property(t => t.ConfigJson).IsRequired();
            entityBuilder.Property(t => t.AutoProvisionEnabled).IsRequired();
            entityBuilder.Property(t => t.AllowedDomains).HasMaxLength(2000);
            entityBuilder.Property(t => t.LastTestedOn);
            entityBuilder.Property(t => t.LastTestSuccess);

            // Table & Column Mappings
            entityBuilder.ToTable("HCL.CS.SF_ExternalAuthProviderConfig");
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
