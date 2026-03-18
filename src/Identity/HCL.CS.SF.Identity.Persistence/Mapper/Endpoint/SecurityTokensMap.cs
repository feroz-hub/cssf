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
/// EF Core entity type configuration that maps the <see cref="SecurityTokens"/> entity
/// to the "HCL.CS.SF_SecurityTokens" table. Stores authorization codes, access tokens,
/// and refresh tokens with consumption tracking (ConsumedAt, ConsumedTime) and expiration.
/// Includes a composite index on (TokenType, Key) for efficient token lookups.
/// </summary>
public class SecurityTokensMap
{
    /// <summary>
    /// Initializes the security tokens mapping.
    /// </summary>
    /// <param name="entityBuilder">The entity type builder for <see cref="SecurityTokens"/>.</param>
    public SecurityTokensMap(EntityTypeBuilder<SecurityTokens> entityBuilder)
    {
        SecurityTokensMapping(entityBuilder);
    }

    /// <summary>
    /// Configures table name, keys, composite index, column mappings, and soft-delete query filter.
    /// </summary>
    private void SecurityTokensMapping(EntityTypeBuilder<SecurityTokens> entityBuilder)
    {
        if (entityBuilder != null)
        {
            entityBuilder.ToTable("HCL.CS.SF_SecurityTokens");

            // Primary Key
            entityBuilder.HasKey(x => x.Id);

            // Table & Column Mappings
            entityBuilder.Property(x => x.ConsumedTime).HasColumnName("ConsumedTime");
            entityBuilder.Property(x => x.ConsumedAt).HasColumnName("ConsumedAt");
            entityBuilder.Property(x => x.CreationTime).HasColumnName("CreationTime");
            entityBuilder.Property(x => x.ExpiresAt).HasColumnName("ExpiresAt");
            entityBuilder.Property(x => x.TokenReuseDetected).HasColumnName("TokenReuseDetected");
            entityBuilder.Property(x => x.IsDeleted).IsRequired().HasColumnName("IsDeleted");
            entityBuilder.Property(x => x.CreatedOn).IsRequired().HasColumnName("CreatedOn");
            entityBuilder.Property(x => x.CreatedBy).IsRequired().HasMaxLength(255).HasColumnName("CreatedBy");
            entityBuilder.Property(x => x.ModifiedOn).HasColumnName("ModifiedOn");
            entityBuilder.Property(x => x.ModifiedBy).HasMaxLength(255).HasColumnName("ModifiedBy");
            entityBuilder.HasIndex(x => new { x.TokenType, x.Key }).HasDatabaseName("IX_SECTOK_TOKTYPE_KEY");
            entityBuilder.HasQueryFilter(m => EF.Property<bool>(m, "IsDeleted") == false);

            entityBuilder.Ignore(x => x.RowVersion);
        }
    }
}
