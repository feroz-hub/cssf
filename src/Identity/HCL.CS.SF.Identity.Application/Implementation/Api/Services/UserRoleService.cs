/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.Domain.Models.Api.Response;
using HCL.CS.SF.Service.Extension;
using HCL.CS.SF.Service.Implementation.Api.Specifications;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;

namespace HCL.CS.SF.Service.Implementation.Api.Services;

/// <summary>
/// Partial class extension of UserAccountService providing RBAC (Role-Based Access Control)
/// user-role management. Handles assigning roles to users, removing role assignments,
/// querying user roles, and resolving the full permission set (role claims) for a user.
/// </summary>

public partial class UserAccountService : SecurityBase, IUserAccountService
{
    /// <summary>
    /// Adds a new user role.
    /// </summary>
    /// <param name="userRoleModel">The user role model.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> AddUserRoleAsync(UserRoleModel userRoleModel)
    {
        if (userRoleModel == null) return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.UserRoleIsNull);

        try
        {
            var userRoleModelSpecification = new UserRoleModelSpecification(CrudMode.Add, userManager, roleService);
            var userRoleModelValidation = await userRoleModelSpecification.ValidateAsync(userRoleModel);
            if (userRoleModelSpecification.IsValid)
            {
                var userRole =
                    await userManagementUnitOfWork.UserRoleRepository.GetUserRoleAsync(userRoleModel.UserId,
                        userRoleModel.RoleId);
                if (userRole != null)
                    return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.UserRoleAlreadyExists);

                var userRoleEntity = mapper.Map<UserRoleModel, UserRoles>(userRoleModel);
                loggerService.WriteTo(Log.Debug, "Entered in Add User Role ");
                await userManagementUnitOfWork.UserRoleRepository.InsertAsync(userRoleEntity);
                return await userManagementUnitOfWork.UserRoleRepository.SaveChangesAsync();
            }

            return frameworkResultService.Failed<FrameworkResult>(userRoleModelValidation.ErrorCode);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to add user role.");
            throw;
        }
    }

    /// <summary>
    /// Adds a new user role.
    /// </summary>
    /// <param name="modelList">The model list.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> AddUserRolesAsync(IList<UserRoleModel> modelList)
    {
        if (!modelList.ContainsAny())
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.UserRoleIsNull);

        var checkUserRoleModelList = modelList.GroupBy(x => new { x.UserId, x.RoleId })
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToList();

        if (checkUserRoleModelList.Count > 0)
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.DuplicateUserRoleInput);

        try
        {
            var userRoleModelSpecification = new UserRoleModelSpecification(CrudMode.Add, userManager, roleService);
            var userRoleModelValidation = await userRoleModelSpecification.ValidateAsync(modelList);
            if (userRoleModelSpecification.IsValid)
            {
                loggerService.WriteTo(Log.Debug, "Entered in Add User Roles : Count : " + modelList.Count);
                var userRoleEntity = mapper.Map<IList<UserRoleModel>, IList<UserRoles>>(modelList);
                foreach (var entity in userRoleEntity)
                {
                    var userRole =
                        await userManagementUnitOfWork.UserRoleRepository.GetUserRoleAsync(entity.UserId,
                            entity.RoleId);
                    if (userRole != null)
                        return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.UserRoleAlreadyExists);

                    await userManagementUnitOfWork.UserRoleRepository.InsertAsync(entity);
                }

                return await userManagementUnitOfWork.UserRoleRepository.SaveChangesAsync();
            }

            return frameworkResultService.Failed(userRoleModelValidation);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to add user role.");
            throw;
        }
    }

    /// <summary>
    /// Deletes the specified user role.
    /// </summary>
    /// <param name="userRoleModel">The user role model.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> RemoveUserRoleAsync(UserRoleModel userRoleModel)
    {
        loggerService.WriteTo(Log.Debug, "Entered in Remove user roles");
        return await DeleteUserRole(userRoleModel, false);
    }

    /// <summary>
    /// Deletes the specified user role.
    /// </summary>
    /// <param name="modelList">The model list.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> RemoveUserRolesAsync(IList<UserRoleModel> modelList)
    {
        if (modelList.IsNull()) return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.UserRoleIsNull);

        try
        {
            FrameworkResult result = null;
            loggerService.WriteTo(Log.Debug, "Entered in Remove multiple user roles : Count : " + modelList.Count);
            foreach (var userRoleModel in modelList)
            {
                result = await DeleteUserRole(userRoleModel, true);
                if (result.Status == ResultStatus.Failed) return result;
            }

            return await userManagementUnitOfWork.UserRoleRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to remove user role.");
            throw;
        }
    }

    /// <summary>
    /// Retrieves the user role.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <returns>The matching collection of results.</returns>
    public virtual async Task<IList<string>> GetUserRolesAsync(Guid userId)
    {
        if (!userId.IsValid()) frameworkResultService.Throw(ApiErrorCodes.UserIdRequired);

        try
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                var userRoles = (List<string>)await userManager.GetRolesAsync(user);
                if (userRoles.ContainsAny())
                {
                    loggerService.WriteTo(Log.Debug, "Entered in Get user roles : Count : " + userRoles.Count);
                    return userRoles.ToList();
                }

                return frameworkResultService.EmptyResult<IList<string>>(ApiErrorCodes.NoRecordsFound);
            }

            frameworkResultService.Throw(ApiErrorCodes.InvalidUserId);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to retrieve user roles.");
            throw;
        }

        return frameworkResultService.EmptyResult<IList<string>>(ApiErrorCodes.NoRecordsFound);
    }

    /// <summary>
    /// Retrieves the users in role.
    /// </summary>
    /// <param name="roleName">The role name.</param>
    /// <returns>The matching collection of results.</returns>
    public virtual async Task<IList<UserModel>> GetUsersInRoleAsync(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName)) frameworkResultService.Throw(ApiErrorCodes.RoleNameRequired);

        try
        {
            var userList = await userManagementUnitOfWork.UserRepository.GetUsersInRoleAsync(roleName);
            if (userList.ContainsAny())
            {
                loggerService.WriteTo(Log.Debug, "Entered in Get users in role : " + userList.Count);
                return mapper.Map<List<Users>, List<UserModel>>(userList.ToList());
            }

            return frameworkResultService.EmptyResult<IList<UserModel>>(ApiErrorCodes.NoRecordsFound);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to retrieve users against the role specified.");
            throw;
        }
    }

    /// <summary>
    /// Retrieves the user role claims by id.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <returns>The operation result.</returns>
    public virtual async Task<UserPermissionsResponseModel> GetUserRoleClaimsByIdAsync(Guid userId)
    {
        try
        {
            return await FetchUserRoleClaimsByIdAsync(userId);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to retrieve user permissions.");
            throw;
        }
    }

    /// <summary>
    /// Retrieves the user role claims by name.
    /// </summary>
    /// <param name="userName">The user name.</param>
    /// <returns>The operation result.</returns>
    public virtual async Task<UserPermissionsResponseModel> GetUserRoleClaimsByNameAsync(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName)) frameworkResultService.Throw(ApiErrorCodes.InvalidUsername);

        try
        {
            var user = await userManager.FindByNameAsync(userName);
            if (user != null) return await FetchUserRoleClaimsByIdAsync(user.Id);

            return frameworkResultService.EmptyResult<UserPermissionsResponseModel>(ApiErrorCodes.NoRecordsFound);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to retrieve user role claims.");
            throw;
        }
    }

    private async Task<UserPermissionsResponseModel> FetchUserRoleClaimsByIdAsync(Guid userId)
    {
        var rolesOfUser = await GetUserRolesAsync(userId);
        if (rolesOfUser.ContainsAny())
        {
            var rolePermissionsList = new List<UserRoleClaimsModel>();
            foreach (var role in rolesOfUser)
            {
                var roleModel = await roleService.GetRoleAsync(role);
                if (roleModel != null)
                {
                    var claimList = (List<RoleClaimModel>)await roleService.GetRoleClaimAsync(roleModel);
                    rolePermissionsList.Add(new UserRoleClaimsModel
                    {
                        RoleId = roleModel.Id,
                        RoleName = roleModel.Name,
                        Claims = claimList
                    });
                }
            }

            return new UserPermissionsResponseModel
            {
                UserId = userId,
                RolePermissions = rolePermissionsList
            };
        }

        return frameworkResultService.EmptyResult<UserPermissionsResponseModel>(ApiErrorCodes.NoRecordsFound);
    }

    private async Task<FrameworkResult> DeleteUserRole(UserRoleModel userRoleModel, bool isCollection)
    {
        if (userRoleModel == null) return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.UserRoleIsNull);

        try
        {
            var userRoleEntity =
                await userManagementUnitOfWork.UserRoleRepository.GetUserRoleAsync(userRoleModel.UserId,
                    userRoleModel.RoleId);
            if (userRoleEntity != null)
            {
                await userManagementUnitOfWork.UserRoleRepository.DeleteAsync(userRoleEntity);
                if (!isCollection) return await userManagementUnitOfWork.UserRoleRepository.SaveChangesAsync();
            }
            else
            {
                return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.UserRoleNotExists);
            }
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to remove user role.");
            throw;
        }

        return frameworkResultService.Succeeded();
    }
}
