/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Api;

/// <summary>
/// Represents a security role in the identity system's RBAC model.
/// Roles group permissions (as claims) and are assigned to users to control access
/// to protected resources and API operations.
/// </summary>
public class RoleModel : BaseModel
{
    /// <summary>The unique name of the role (e.g., "Admin", "Manager", "User").</summary>
    public virtual string Name { get; set; }

    /// <summary>A human-readable description of the role's purpose and permissions.</summary>
    public virtual string Description { get; set; }

    /// <summary>The permission claims associated with this role, defining what actions role holders can perform.</summary>
    public virtual List<RoleClaimModel> RoleClaims { get; set; }
}
