/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Api;

/// <summary>
/// Flattened view of the relationship between OIDC identity resources and their claim types.
/// Used during token generation to determine which user claims to include in the identity token
/// based on the granted identity scopes (e.g., "profile" maps to "name", "family_name", etc.).
/// </summary>
public class IdentityResourcesByScopesModel
{
    /// <summary>The unique identifier of the identity resource.</summary>
    public virtual Guid IdentityResourceId { get; set; }

    /// <summary>The scope name of the identity resource (e.g., "openid", "profile", "email").</summary>
    public virtual string IdentityResourceName { get; set; }

    /// <summary>The standard claim type associated with this identity resource (e.g., "name", "email").</summary>
    public virtual string IdentityResourceClaimType { get; set; }

    /// <summary>An optional alias for the claim type, used for custom claim name mapping in tokens.</summary>
    public virtual string IdentityResourceClaimAliasType { get; set; }
}
