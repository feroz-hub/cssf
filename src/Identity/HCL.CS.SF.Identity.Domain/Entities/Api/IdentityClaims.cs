/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Entities.Api;

/// <summary>
/// Maps a claim type to an <see cref="IdentityResources"/> entry.
/// When a client requests an identity scope (e.g., "profile"), the associated identity claims
/// (e.g., "given_name", "family_name") are included in the ID token.
/// </summary>
public class IdentityClaims : BaseEntity
{
    /// <summary>Foreign key referencing the parent identity resource.</summary>
    public virtual Guid IdentityResourceId { get; set; }

    /// <summary>The standard OIDC claim type (e.g., "sub", "email", "given_name").</summary>
    public virtual string Type { get; set; }

    /// <summary>An optional alias for the claim type, used when the internal claim name differs from the OIDC standard name.</summary>
    public virtual string AliasType { get; set; }

    /// <summary>Navigation property to the parent identity resource.</summary>
    public virtual IdentityResources IdentityResource { get; set; }
}
