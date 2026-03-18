/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace HCL.CS.SF.Domain.Entities.Api;

/// <summary>
/// Represents the many-to-many association between a user and a role.
/// Extends ASP.NET Identity's <see cref="IdentityUserRole{TKey}"/> with time-bounded
/// validity, audit fields, and soft-delete. The ValidFrom/ValidTo window enables
/// temporary role assignments (e.g., granting admin access for a limited period).
/// </summary>
public class UserRoles : IdentityUserRole<Guid>
{
    /// <summary>Surrogate primary key for this user-role assignment.</summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public virtual Guid Id { get; set; }

    /// <summary>Optional start date for this role assignment; null means effective immediately.</summary>
    public virtual DateTime? ValidFrom { get; set; }

    /// <summary>Optional expiration date for this role assignment; null means no expiration.</summary>
    public virtual DateTime? ValidTo { get; set; }

    /// <summary>Soft-delete flag; when true the role assignment is logically removed.</summary>
    public virtual bool IsDeleted { get; set; }

    /// <summary>UTC timestamp when this role assignment was created.</summary>
    public virtual DateTime CreatedOn { get; set; }

    /// <summary>UTC timestamp of the last modification, or null if never modified.</summary>
    public virtual DateTime? ModifiedOn { get; set; }

    /// <summary>Identifier of the user or system that created this role assignment.</summary>
    public virtual string CreatedBy { get; set; }

    /// <summary>Identifier of the user or system that last modified this role assignment.</summary>
    public virtual string ModifiedBy { get; set; }

    /// <summary>Optimistic concurrency token to detect conflicting updates.</summary>
    [Timestamp] public byte[] RowVersion { get; set; }
}
