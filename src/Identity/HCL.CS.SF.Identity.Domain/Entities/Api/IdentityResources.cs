/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Entities.Api;

/// <summary>
/// Represents an OIDC identity resource (e.g., "openid", "profile", "email").
/// Identity resources map to sets of user claims that are included in the ID token
/// when the client requests the corresponding scope.
/// </summary>
public class IdentityResources : BaseEntity
{
    /// <summary>Unique scope name for this identity resource (e.g., "openid", "profile", "email").</summary>
    public virtual string Name { get; set; }

    /// <summary>Human-readable display name shown on consent screens.</summary>
    public virtual string DisplayName { get; set; }

    /// <summary>Brief description of what user data this identity resource exposes.</summary>
    public virtual string Description { get; set; }

    /// <summary>Whether this identity resource is active. Disabled resources cannot be requested by clients.</summary>
    public virtual bool Enabled { get; set; }

    /// <summary>When true, the user cannot deselect this resource on the consent screen.</summary>
    public virtual bool Required { get; set; }

    /// <summary>When true, this resource is visually highlighted on the consent screen.</summary>
    public virtual bool Emphasize { get; set; }

    /// <summary>The claim types that belong to this identity resource and are emitted in ID tokens.</summary>
    public virtual List<IdentityClaims> IdentityClaims { get; set; }
}
