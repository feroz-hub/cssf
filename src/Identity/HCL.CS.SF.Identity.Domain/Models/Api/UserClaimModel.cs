/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Api;

/// <summary>
/// Represents a custom claim directly associated with a user.
/// User claims are included in identity and access tokens when the corresponding
/// scope or claim type is requested. Can be marked as admin-only claims.
/// </summary>
public class UserClaimModel : BaseTrailModel
{
    /// <summary>The auto-generated identifier for this user claim.</summary>
    public virtual int Id { get; set; }

    /// <summary>The unique identifier of the user this claim belongs to.</summary>
    public virtual Guid UserId { get; set; }

    /// <summary>The claim type (e.g., "department", "employee_id").</summary>
    public virtual string ClaimType { get; set; }

    /// <summary>The claim value (e.g., "Engineering", "12345").</summary>
    public virtual string ClaimValue { get; set; }

    /// <summary>Indicates whether this is an admin-managed claim that cannot be self-edited by the user.</summary>
    public virtual bool IsAdminClaim { get; set; } = false;
}
