/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Api.Response;

/// <summary>
/// Response model that aggregates all permissions for a user across their assigned roles.
/// Used to determine the user's effective permissions for RBAC authorization decisions.
/// </summary>
public class UserPermissionsResponseModel
{
    /// <summary>The unique identifier of the user whose permissions are being returned.</summary>
    public virtual Guid UserId { get; set; }

    /// <summary>The list of role-based permissions, grouped by role.</summary>
    public virtual IList<UserRoleClaimsModel> RolePermissions { get; set; }
}

/// <summary>
/// Groups the permission claims belonging to a single role assignment.
/// Each role contains a collection of claims that define what actions the role holder is authorized to perform.
/// </summary>
public class UserRoleClaimsModel
{
    /// <summary>The unique identifier of the role.</summary>
    public virtual Guid RoleId { get; set; }

    /// <summary>The display name of the role.</summary>
    public virtual string RoleName { get; set; }

    /// <summary>The permission claims associated with this role.</summary>
    public virtual IList<RoleClaimModel> Claims { get; set; }
}
