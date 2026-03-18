/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Api;

/// <summary>
/// Represents the assignment of a role to a user, with optional time-bound validity.
/// Links a user to a role in the RBAC model, optionally restricting the assignment
/// to a specific date range.
/// </summary>
public class UserRoleModel : BaseModel
{
    /// <summary>The unique identifier of the assigned role.</summary>
    public virtual Guid RoleId { get; set; }

    /// <summary>The unique identifier of the user receiving the role assignment.</summary>
    public virtual Guid UserId { get; set; }

    /// <summary>The UTC date from which the role assignment becomes active. Null means immediately active.</summary>
    public virtual DateTime? ValidFrom { get; set; }

    /// <summary>The UTC date after which the role assignment expires. Null means no expiration.</summary>
    public virtual DateTime? ValidTo { get; set; }
}
