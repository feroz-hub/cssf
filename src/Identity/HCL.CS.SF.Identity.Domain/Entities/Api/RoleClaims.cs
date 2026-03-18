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
/// Represents a claim assigned to a role. Extends ASP.NET Identity's <see cref="IdentityRoleClaim{TKey}"/>
/// with audit fields and soft-delete support. When a user is a member of a role,
/// its role claims are merged into the user's effective claim set during token issuance.
/// </summary>
public class RoleClaims : IdentityRoleClaim<Guid>
{
    /// <summary>Soft-delete flag; when true the claim is logically removed without physical deletion.</summary>
    public virtual bool IsDeleted { get; set; }

    /// <summary>UTC timestamp when this role claim was created.</summary>
    public virtual DateTime CreatedOn { get; set; }

    /// <summary>UTC timestamp of the last modification, or null if never modified.</summary>
    public virtual DateTime? ModifiedOn { get; set; }

    /// <summary>Identifier of the user or system that created this role claim.</summary>
    public virtual string CreatedBy { get; set; }

    /// <summary>Identifier of the user or system that last modified this role claim.</summary>
    public virtual string ModifiedBy { get; set; }

    /// <summary>Optimistic concurrency token to detect conflicting updates.</summary>
    [Timestamp] public byte[] RowVersion { get; set; }
}
