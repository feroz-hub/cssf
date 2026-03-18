/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Api;

/// <summary>
/// Represents a claim type associated with an OIDC identity resource.
/// When a scope like "profile" is granted, the identity claims mapped to that resource
/// (e.g., "name", "family_name") are included in the identity token.
/// </summary>
public class IdentityClaimsModel : BaseModel
{
    /// <summary>The unique identifier of the parent identity resource.</summary>
    public virtual Guid IdentityResourceId { get; set; }

    /// <summary>The standard claim type name (e.g., "name", "email", "given_name").</summary>
    public virtual string Type { get; set; }

    /// <summary>An optional alias for the claim type, allowing custom claim naming in tokens.</summary>
    public virtual string AliasType { get; set; }
}
