/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Identity;

namespace HCL.CS.SF.Domain.Entities.Api;

/// <summary>
/// Represents an authorization role within the identity system (e.g., "Admin", "User", "Operator").
/// Extends ASP.NET Identity's <see cref="IdentityRole{TKey}"/> with a description,
/// audit fields, and soft-delete support. Roles aggregate <see cref="RoleClaims"/>
/// that are automatically included in tokens for users who hold the role.
/// </summary>
public class Roles : IdentityRole<Guid>
{
    /// <summary>Human-readable description of the role's purpose and permissions.</summary>
    public string Description { get; set; }

    /// <summary>Soft-delete flag; when true the role is logically removed.</summary>
    public virtual bool IsDeleted { get; set; }

    /// <summary>UTC timestamp when this role was created.</summary>
    public virtual DateTime CreatedOn { get; set; }

    /// <summary>UTC timestamp of the last modification, or null if never modified.</summary>
    public virtual DateTime? ModifiedOn { get; set; }

    /// <summary>Identifier of the user or system that created this role.</summary>
    public virtual string CreatedBy { get; set; }

    /// <summary>Identifier of the user or system that last modified this role.</summary>
    public virtual string ModifiedBy { get; set; }
}
