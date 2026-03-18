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
/// Records an external login association for a user (e.g., Google login, Facebook login).
/// Extends ASP.NET Identity's <see cref="IdentityUserLogin{TKey}"/> with an explicit primary key,
/// audit fields, and soft-delete support. Each entry maps an external provider + provider key
/// to a local user account.
/// </summary>
public class UserLogins : IdentityUserLogin<Guid>
{
    /// <summary>Surrogate primary key for this login record.</summary>
    public virtual Guid Id { get; set; }

    /// <summary>Soft-delete flag; when true the login association is logically removed.</summary>
    public virtual bool IsDeleted { get; set; }

    /// <summary>UTC timestamp when this login association was created.</summary>
    public virtual DateTime CreatedOn { get; set; }

    /// <summary>UTC timestamp of the last modification, or null if never modified.</summary>
    public virtual DateTime? ModifiedOn { get; set; }

    /// <summary>Identifier of the user or system that created this login record.</summary>
    public virtual string CreatedBy { get; set; }

    /// <summary>Identifier of the user or system that last modified this login record.</summary>
    public virtual string ModifiedBy { get; set; }
}
