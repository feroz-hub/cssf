/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace HCL.CS.SF.Domain.Entities.Api;

/// <summary>
/// Represents a claim (key-value attribute) associated with a specific user.
/// Extends ASP.NET Identity's <see cref="IdentityUserClaim{TKey}"/> with admin-only flag,
/// audit fields, and soft-delete. User claims are emitted in ID and access tokens
/// based on the requested scopes and resource claim mappings.
/// </summary>
public class UserClaims : IdentityUserClaim<Guid>
{
    /// <summary>When true, this claim was assigned by an administrator and should not be self-editable by the user.</summary>
    public virtual bool IsAdminClaim { get; set; } = false;

    /// <summary>Soft-delete flag; when true the claim is logically removed.</summary>
    public virtual bool IsDeleted { get; set; }

    /// <summary>UTC timestamp when this claim was created.</summary>
    public virtual DateTime CreatedOn { get; set; }

    /// <summary>UTC timestamp of the last modification, or null if never modified.</summary>
    public virtual DateTime? ModifiedOn { get; set; }

    /// <summary>Identifier of the user or system that created this claim.</summary>
    public virtual string CreatedBy { get; set; }

    /// <summary>Identifier of the user or system that last modified this claim.</summary>
    public virtual string ModifiedBy { get; set; }

    /// <summary>Optimistic concurrency token to detect conflicting updates.</summary>
    [Timestamp] public byte[] RowVersion { get; set; }
}
