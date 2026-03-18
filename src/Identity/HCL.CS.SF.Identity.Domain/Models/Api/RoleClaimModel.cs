/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Api;

/// <summary>
/// Represents a claim (permission) associated with a security role.
/// Role claims define the permissions granted to users who hold the role,
/// and are embedded in access tokens for RBAC authorization.
/// </summary>
public class RoleClaimModel : BaseTrailModel
{
    /// <summary>The auto-generated identifier for this role claim.</summary>
    public virtual int Id { get; set; }

    /// <summary>The unique identifier of the role this claim belongs to.</summary>
    public virtual Guid RoleId { get; set; }

    /// <summary>The claim type (e.g., "permission", "capability").</summary>
    public virtual string ClaimType { get; set; }

    /// <summary>The claim value (e.g., "users.read", "users.write").</summary>
    public virtual string ClaimValue { get; set; }
}
