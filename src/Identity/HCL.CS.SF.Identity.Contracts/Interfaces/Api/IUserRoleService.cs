/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.Domain.Models.Api.Response;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Api;

/// <summary>
/// Partial interface extending <see cref="IUserAccountService"/> with user-role assignment operations.
/// Supports adding/removing role assignments, querying roles for a user, and retrieving
/// the combined roles-and-claims permissions structure.
/// </summary>
public partial interface IUserAccountService
{
    /// <summary>Assigns a single role to a user.</summary>
    /// <param name="userRoleModel">The user-role assignment details.</param>
    Task<FrameworkResult> AddUserRoleAsync(UserRoleModel userRoleModel);

    /// <summary>Assigns multiple roles to a user in a single operation.</summary>
    /// <param name="modelList">The list of user-role assignments.</param>
    Task<FrameworkResult> AddUserRolesAsync(IList<UserRoleModel> modelList);

    /// <summary>Removes a single role from a user.</summary>
    /// <param name="userRoleModel">The user-role assignment to remove.</param>
    Task<FrameworkResult> RemoveUserRoleAsync(UserRoleModel userRoleModel);

    /// <summary>Removes multiple roles from a user in a single operation.</summary>
    /// <param name="modelList">The list of user-role assignments to remove.</param>
    Task<FrameworkResult> RemoveUserRolesAsync(IList<UserRoleModel> modelList);

    /// <summary>Retrieves the role names assigned to the specified user.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    Task<IList<string>> GetUserRolesAsync(Guid userId);

    /// <summary>Retrieves all users assigned to the specified role.</summary>
    /// <param name="roleName">The name of the role.</param>
    Task<IList<UserModel>> GetUsersInRoleAsync(string roleName);

    /// <summary>Retrieves the combined roles and claims permissions for the specified user by ID.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    Task<UserPermissionsResponseModel> GetUserRoleClaimsByIdAsync(Guid userId);

    /// <summary>Retrieves the combined roles and claims permissions for the specified user by username.</summary>
    /// <param name="userName">The username of the user.</param>
    Task<UserPermissionsResponseModel> GetUserRoleClaimsByNameAsync(string userName);
}
