/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Models.Api;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Api;

/// <summary>
/// Service for managing application roles and role-based claims. Provides CRUD operations
/// for roles, claim assignment to roles, and querying the combined role-claim structure
/// for authorization decisions.
/// </summary>
public interface IRoleService
{
    /// <summary>Creates a new application role.</summary>
    /// <param name="roleModel">The role to create.</param>
    Task<FrameworkResult> CreateRoleAsync(RoleModel roleModel);

    /// <summary>Updates an existing application role.</summary>
    /// <param name="roleModel">The role with updated details.</param>
    Task<FrameworkResult> UpdateRoleAsync(RoleModel roleModel);

    /// <summary>Deletes a role by its unique identifier.</summary>
    /// <param name="roleId">The unique identifier of the role to delete.</param>
    Task<FrameworkResult> DeleteRoleAsync(Guid roleId);

    /// <summary>Deletes a role by its name.</summary>
    /// <param name="roleName">The name of the role to delete.</param>
    Task<FrameworkResult> DeleteRoleAsync(string roleName);

    /// <summary>Retrieves a role by its unique identifier.</summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    Task<RoleModel> GetRoleAsync(Guid roleId);

    /// <summary>Retrieves a role by its name.</summary>
    /// <param name="roleName">The name of the role.</param>
    Task<RoleModel> GetRoleAsync(string roleName);

    /// <summary>Retrieves all application roles.</summary>
    Task<IList<RoleModel>> GetAllRolesAsync();

    /// <summary>Adds a single claim to a role.</summary>
    /// <param name="roleClaimModel">The role claim to add.</param>
    Task<FrameworkResult> AddRoleClaimAsync(RoleClaimModel roleClaimModel);

    /// <summary>Adds multiple claims to a role in a single operation.</summary>
    /// <param name="roleClaimsModel">The role claims to add.</param>
    Task<FrameworkResult> AddRoleClaimsAsync(IList<RoleClaimModel> roleClaimsModel);

    /// <summary>Retrieves the combined roles and claim types for the specified user.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    Task<IList<UserRoleClaimTypesModel>> GetRolesAndClaimsForUser(Guid userId);

    /// <summary>Removes a role claim by its primary key.</summary>
    /// <param name="roleClaimId">The primary key of the role claim.</param>
    Task<FrameworkResult> RemoveRoleClaimAsync(int roleClaimId);

    /// <summary>Removes a role claim by its type and value.</summary>
    /// <param name="roleClaimModel">The role claim to remove.</param>
    Task<FrameworkResult> RemoveRoleClaimAsync(RoleClaimModel roleClaimModel);

    /// <summary>Removes multiple role claims in a single operation.</summary>
    /// <param name="roleClaimsModel">The role claims to remove.</param>
    Task<FrameworkResult> RemoveRoleClaimsAsync(IList<RoleClaimModel> roleClaimsModel);

    /// <summary>Retrieves all claims associated with the specified role.</summary>
    /// <param name="roleModel">The role to query claims for.</param>
    Task<IList<RoleClaimModel>> GetRoleClaimAsync(RoleModel roleModel);
}
