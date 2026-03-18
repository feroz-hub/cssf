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
/// EF Core entity type configuration that maps the <see cref="ClientRedirectUris"/> entity
/// to the "HCL.CS.SF_ClientRedirectUris" table. Enforces a unique constraint on
/// (ClientId, RedirectUri) to prevent duplicate redirect URIs per client.
/// </summary>
public class ClientRedirectUrisMap
{
    /// <summary>
    /// Initializes the client redirect URIs mapping.
    /// </summary>
    /// <param name="entityBuilder">The entity type builder for <see cref="ClientRedirectUris"/>.</param>
    public ClientRedirectUrisMap(EntityTypeBuilder<ClientRedirectUris> entityBuilder)
    {
        ClientRedirectUrisMapping(entityBuilder);
    }

    /// <summary>
    /// Configures table name, keys, unique index, property constraints, and soft-delete query filter.
    /// </summary>
    private void ClientRedirectUrisMapping(EntityTypeBuilder<ClientRedirectUris> entityBuilder)
    {
        if (entityBuilder != null)
        {
            entityBuilder.ToTable("HCL.CS.SF_ClientRedirectUris");

            // Primary Key
            entityBuilder.HasKey(x => x.Id);
            entityBuilder.HasIndex(x => new { x.ClientId, x.RedirectUri }).IsUnique();

            // Properties
            entityBuilder.Property(x => x.RedirectUri).IsRequired().HasMaxLength(510).HasColumnName("RedirectUri");

            // Table & Column Mappings
            entityBuilder.Property(x => x.IsDeleted).IsRequired().HasColumnName("IsDeleted");
            entityBuilder.Property(x => x.CreatedOn).IsRequired().HasColumnName("CreatedOn");
            entityBuilder.Property(x => x.CreatedBy).IsRequired().HasMaxLength(255).HasColumnName("CreatedBy");
            entityBuilder.Property(x => x.ModifiedOn).HasColumnName("ModifiedOn");
            entityBuilder.Property(x => x.ModifiedBy).HasMaxLength(255).HasColumnName("ModifiedBy");
            entityBuilder.HasQueryFilter(m => EF.Property<bool>(m, "IsDeleted") == false);
        }
    }
}
