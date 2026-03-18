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
/// EF Core entity type configuration that maps the <see cref="ExternalIdentities"/> entity
/// to the "HCL.CS.SF_ExternalIdentities" table. Configures the foreign key to Users,
/// unique composite index on Provider/Issuer/Subject to prevent duplicate linkages,
/// and indexes for user and tenant-email lookups.
/// </summary>
public class ExternalIdentitiesMap
{
    /// <summary>
    /// Initializes the external identities mapping.
    /// </summary>
    /// <param name="entityBuilder">The entity type builder for <see cref="ExternalIdentities"/>.</param>
    public ExternalIdentitiesMap(EntityTypeBuilder<ExternalIdentities> entityBuilder)
    {
        ExternalIdentityMapping(entityBuilder);
    }

    /// <summary>
    /// Configures table name, keys, foreign key to Users, indexes, and soft-delete query filter.
    /// </summary>
    private static void ExternalIdentityMapping(EntityTypeBuilder<ExternalIdentities> entityBuilder)
    {
        if (entityBuilder == null) return;

        entityBuilder.ToTable("HCL.CS.SF_ExternalIdentities");

        entityBuilder.HasKey(x => x.Id);

        entityBuilder.Property(x => x.UserId).IsRequired();
        entityBuilder.Property(x => x.TenantId).HasMaxLength(128);
        entityBuilder.Property(x => x.Provider).IsRequired().HasMaxLength(64);
        entityBuilder.Property(x => x.Issuer).IsRequired().HasMaxLength(256);
        entityBuilder.Property(x => x.Subject).IsRequired().HasMaxLength(256);
        entityBuilder.Property(x => x.Email).IsRequired().HasMaxLength(255);
        entityBuilder.Property(x => x.EmailVerified).IsRequired();
        entityBuilder.Property(x => x.LinkedAt).IsRequired();
        entityBuilder.Property(x => x.LastSignInAt);
        entityBuilder.Property(x => x.IsDeleted).IsRequired();
        entityBuilder.Property(x => x.CreatedOn).IsRequired();
        entityBuilder.Property(x => x.CreatedBy).IsRequired().HasMaxLength(255);
        entityBuilder.Property(x => x.ModifiedBy).HasMaxLength(255);

        entityBuilder.HasOne<Users>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        entityBuilder.HasIndex(x => new { x.Provider, x.Issuer, x.Subject })
            .IsUnique()
            .HasDatabaseName("IX_EXTID_PROVIDER_ISSUER_SUBJECT");

        entityBuilder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_EXTID_USERID");

        entityBuilder.HasIndex(x => new { x.TenantId, x.Email })
            .HasDatabaseName("IX_EXTID_TENANT_EMAIL");

        entityBuilder.HasQueryFilter(x => EF.Property<bool>(x, "IsDeleted") == false);
    }
}
