/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HCL.CS.SF.Domain.Entities.Endpoint;

namespace HCL.CS.SF.Infrastructure.Data.Mapper.Endpoint;

/// <summary>
/// EF Core entity type configuration that maps the <see cref="Clients"/> entity
/// to the "HCL.CS.SF_Clients" table. Configures a unique composite index on (ClientId, ClientSecret),
/// cascade-delete relationships to RedirectUris and PostLogoutRedirectUris, and soft-delete query filter.
/// </summary>
public class ClientsMap
{
    /// <summary>
    /// Initializes the clients mapping.
    /// </summary>
    /// <param name="entityBuilder">The entity type builder for <see cref="Clients"/>.</param>
    public ClientsMap(EntityTypeBuilder<Clients> entityBuilder)
    {
        ClientsMapping(entityBuilder);
    }

    /// <summary>
    /// Configures table name, keys, unique index, cascade-delete relationships, and soft-delete query filter.
    /// </summary>
    private void ClientsMapping(EntityTypeBuilder<Clients> entityBuilder)
    {
        if (entityBuilder != null)
        {
            entityBuilder.ToTable("HCL.CS.SF_Clients");

            // Primary Key
            entityBuilder.HasKey(x => x.Id);
            entityBuilder.HasIndex(x => new { x.ClientId, x.ClientSecret }).HasDatabaseName("IX_CLI_CLID_CLSEC")
                .IsUnique();

            // Properties
            entityBuilder.Property(x => x.ClientId).HasMaxLength(128).IsRequired();
            entityBuilder.Property(x => x.ClientName).HasMaxLength(255);
            entityBuilder.Property(x => x.ClientSecret).HasMaxLength(128);
            entityBuilder.Property(x => x.PreferredAudience).HasMaxLength(300);

            // Table & Column Mappings
            entityBuilder.Property(x => x.IsDeleted).IsRequired().HasColumnName("IsDeleted");
            entityBuilder.Property(x => x.CreatedOn).IsRequired().HasColumnName("CreatedOn");
            entityBuilder.Property(x => x.CreatedBy).IsRequired().HasMaxLength(255).HasColumnName("CreatedBy");
            entityBuilder.Property(x => x.ModifiedOn).HasColumnName("ModifiedOn");
            entityBuilder.Property(x => x.ModifiedBy).HasMaxLength(255).HasColumnName("ModifiedBy");

            // Mappings
            entityBuilder.HasMany(x => x.RedirectUris).WithOne(x => x.Client)
                .HasForeignKey(x => x.ClientId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            entityBuilder.HasMany(x => x.PostLogoutRedirectUris).WithOne(x => x.Client)
                .HasForeignKey(x => x.ClientId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            entityBuilder.HasQueryFilter(m => EF.Property<bool>(m, "IsDeleted") == false);
        }
    }
}
