/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Api;

/// <summary>
/// Represents an OIDC identity resource (e.g., "openid", "profile", "email", "address").
/// Identity resources map to sets of user claims that are included in the identity token
/// when the corresponding scope is granted. Defined by the OpenID Connect specification.
/// </summary>
public class IdentityResourcesModel : BaseModel
{
    /// <summary>The unique scope name of the identity resource (e.g., "openid", "profile").</summary>
    public virtual string Name { get; set; }

    /// <summary>The human-readable display name shown on consent screens.</summary>
    public virtual string DisplayName { get; set; }

    /// <summary>A description of the identity resource shown on consent screens.</summary>
    public virtual string Description { get; set; }

    /// <summary>Indicates whether this identity resource is enabled and can be requested.</summary>
    public virtual bool Enabled { get; set; } = true;

    /// <summary>Indicates whether this scope must be granted (cannot be deselected on consent screen).</summary>
    public virtual bool Required { get; set; } = false;

    /// <summary>Indicates whether this scope should be visually emphasized on the consent screen.</summary>
    public virtual bool Emphasize { get; set; } = false;

    /// <summary>The claim types that are included in identity tokens when this scope is granted.</summary>
    public virtual List<IdentityClaimsModel> IdentityClaims { get; set; }
}
