/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Api;

/// <summary>
/// Flattened view of a user's role-based claim assignments.
/// Used during token generation to resolve which role claims to include in access tokens
/// based on the user's role memberships and each role's associated claim types.
/// </summary>
public class UserRoleClaimTypesModel
{
    /// <summary>The unique identifier of the user.</summary>
    public virtual Guid UserId { get; set; }

    /// <summary>The username of the user.</summary>
    public virtual string UserName { get; set; }

    /// <summary>The name of the role assigned to the user.</summary>
    public virtual string RoleName { get; set; }

    /// <summary>The claim type from the role's claims (e.g., "permission").</summary>
    public virtual string RoleClaimType { get; set; }

    /// <summary>The claim value from the role's claims (e.g., "users.read").</summary>
    public virtual string RoleClaimValue { get; set; }
}
