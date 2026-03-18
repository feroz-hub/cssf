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
/// EF Core entity type configuration that maps the <see cref="UserTokens"/> entity
/// to the "HCL.CS.SF_UserTokens" table. Uses a composite key of UserId, LoginProvider,
/// and Name to store authentication tokens such as refresh tokens and 2FA recovery codes.
/// </summary>
public class UserTokensMap
{
    /// <summary>
    /// Initializes the user tokens mapping.
    /// </summary>
    /// <param name="entityBuilder">The entity type builder for <see cref="UserTokens"/>.</param>
    public UserTokensMap(EntityTypeBuilder<UserTokens> entityBuilder)
    {
        UserTokenMapping(entityBuilder);
    }

    /// <summary>
    /// Configures table name, composite key, and property constraints.
    /// </summary>
    private void UserTokenMapping(EntityTypeBuilder<UserTokens> entityBuilder)
    {
        if (entityBuilder != null)
        {
            // Primary Key
            entityBuilder.HasKey(t => new { t.UserId, t.LoginProvider, t.Name });

            // Properties
            entityBuilder.Property(t => t.LoginProvider).IsRequired().HasMaxLength(255);
            entityBuilder.Property(t => t.Name).IsRequired().HasMaxLength(255);

            // Table & Column Mappings
            entityBuilder.ToTable("HCL.CS.SF_UserTokens");
            entityBuilder.Property(t => t.UserId).IsRequired().HasColumnName("UserId");
            entityBuilder.Property(t => t.Value).IsRequired().HasColumnName("Value");
        }
    }
}
