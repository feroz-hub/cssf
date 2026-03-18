/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.Service.Extension;
using HCL.CS.SF.Service.Implementation.Api.Specifications;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;

namespace HCL.CS.SF.Service.Implementation.Api.Services;

/// <summary>
/// Partial class extension of UserAccountService providing user claim management.
/// User claims are key-value pairs attached to user identities that flow into OAuth2/OIDC tokens.
/// Supports adding, removing, and replacing both regular user claims and admin-level claims,
/// with duplicate detection and batch operations.
/// </summary>

public partial class UserAccountService : SecurityBase, IUserAccountService
{
    /// <summary>
    /// Adds a new claim.
    /// </summary>
    /// <param name="userClaimModels">The user claim models.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> AddClaimAsync(IList<UserClaimModel> userClaimModels)
    {
        if (!userClaimModels.ContainsAny())
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserClaims);

        try
        {
            var userClaimModelSpecification = new UserClaimModelSpecification(CrudMode.Add);
            var userClaimModelValidation = await userClaimModelSpecification.ValidateAsync(userClaimModels);
            if (userClaimModelSpecification.IsValid)
            {
                loggerService.WriteTo(Log.Debug, "Entered in add user claims : Count : " + userClaimModels.Count);
                foreach (var userclaims in userClaimModels)
                {
                    userclaims.IsAdminClaim = false;
                    var result = await AddUserClaimAsync(userclaims, false);
                    if (result.Status == ResultStatus.Failed) return result;
                }

                return await userManagementUnitOfWork.SaveChangesAsync();
            }

            return frameworkResultService.Failed(userClaimModelValidation);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while adding user claims.");
            throw;
        }
    }

    /// <summary>
    /// Adds a new admin claim.
    /// </summary>
    /// <param name="userClaimModels">The user claim models.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> AddAdminClaimAsync(IList<UserClaimModel> userClaimModels)
    {
        if (!userClaimModels.ContainsAny())
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserClaims);

        try
        {
            var userClaimModelSpecification = new UserClaimModelSpecification(CrudMode.Add);
            var userClaimModelValidation = await userClaimModelSpecification.ValidateAsync(userClaimModels);
            if (userClaimModelSpecification.IsValid)
            {
                loggerService.WriteTo(Log.Debug, "Entered in add user claims : Count : " + userClaimModels.Count);
                foreach (var userClaims in userClaimModels)
                {
                    userClaims.IsAdminClaim = true;
                    var result = await AddUserClaimAsync(userClaims, false);
                    if (result.Status == ResultStatus.Failed) return result;
                }

                return await userManagementUnitOfWork.SaveChangesAsync();
            }

            return frameworkResultService.Failed(userClaimModelValidation);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while adding user claims.");
            throw;
        }
    }

    /// <summary>
    /// Adds a new claim.
    /// </summary>
    /// <param name="userClaimModel">The user claim model.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> AddClaimAsync(UserClaimModel userClaimModel)
    {
        try
        {
            var userClaimModelSpecification = new UserClaimModelSpecification(CrudMode.Add);
            var userClaimModelValidation = await userClaimModelSpecification.ValidateAsync(userClaimModel);
            if (userClaimModelSpecification.IsValid)
            {
                loggerService.WriteTo(Log.Debug,
                    "Entered into Add user claims : ClaimType = " + userClaimModel.ClaimType);
                userClaimModel.IsAdminClaim = false;
                return await AddUserClaimAsync(userClaimModel, true);
            }

            return frameworkResultService.Failed<FrameworkResult>(userClaimModelValidation.ErrorCode);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while adding user claims.");
            throw;
        }
    }

    /// <summary>
    /// Adds a new admin claim.
    /// </summary>
    /// <param name="userClaimModel">The user claim model.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> AddAdminClaimAsync(UserClaimModel userClaimModel)
    {
        try
        {
            var userClaimModelSpecification = new UserClaimModelSpecification(CrudMode.Add);
            var userClaimModelValidation = await userClaimModelSpecification.ValidateAsync(userClaimModel);
            if (userClaimModelSpecification.IsValid)
            {
                loggerService.WriteTo(Log.Debug,
                    "Entered into Add user claims : ClaimType = " + userClaimModel.ClaimType);
                userClaimModel.IsAdminClaim = true;
                return await AddUserClaimAsync(userClaimModel, true);
            }

            return frameworkResultService.Failed<FrameworkResult>(userClaimModelValidation.ErrorCode);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while adding user claims.");
            throw;
        }
    }

    /// <summary>
    /// Deletes the specified claim.
    /// </summary>
    /// <param name="userClaimModel">The user claim model.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> RemoveClaimAsync(IList<UserClaimModel> userClaimModel)
    {
        if (!userClaimModel.ContainsAny())
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserClaims);

        try
        {
            var userClaimModelSpecification = new UserClaimModelSpecification(CrudMode.Delete);
            var userClaimModelValidation = await userClaimModelSpecification.ValidateAsync(userClaimModel);
            if (userClaimModelSpecification.IsValid)
            {
                loggerService.WriteTo(Log.Debug,
                    "Entered into removing list of user claims : Count : " + userClaimModel.Count);
                foreach (var userclaims in userClaimModel)
                {
                    var result = await RemoveUserClaims(userclaims, false);
                    if (result.Status == ResultStatus.Failed) return result;
                }

                var csResult = await userManagementUnitOfWork.SaveChangesAsync();
                return csResult;
            }

            return frameworkResultService.Failed(userClaimModelValidation);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while removing user claims.");
            throw;
        }
    }

    /// <summary>
    /// Deletes the specified claim.
    /// </summary>
    /// <param name="userClaimModel">The user claim model.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> RemoveClaimAsync(UserClaimModel userClaimModel)
    {
        try
        {
            var userClaimModelSpecification = new UserClaimModelSpecification(CrudMode.Delete);
            var userClaimModelValidation = await userClaimModelSpecification.ValidateAsync(userClaimModel);
            if (userClaimModelSpecification.IsValid)
            {
                loggerService.WriteTo(Log.Debug, "Entered into remove user claims");
                return await RemoveUserClaims(userClaimModel, true);
            }

            return frameworkResultService.Failed<FrameworkResult>(userClaimModelValidation.ErrorCode);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while removing user claims.");
            throw;
        }
    }

    /// <summary>
    /// Deletes the specified admin claim.
    /// </summary>
    /// <param name="userClaimModel">The user claim model.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> RemoveAdminClaimAsync(IList<UserClaimModel> userClaimModel)
    {
        if (!userClaimModel.ContainsAny())
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserClaims);

        try
        {
            var userClaimModelSpecification = new UserClaimModelSpecification(CrudMode.Delete);
            var userClaimModelValidation = await userClaimModelSpecification.ValidateAsync(userClaimModel);
            if (userClaimModelSpecification.IsValid)
            {
                loggerService.WriteTo(Log.Debug,
                    "Entered into removing list of user claims : Count : " + userClaimModel.Count);
                foreach (var userclaims in userClaimModel)
                {
                    var result = await RemoveUserClaims(userclaims, false, true);
                    if (result.Status == ResultStatus.Failed) return result;
                }

                return await userManagementUnitOfWork.SaveChangesAsync();
            }

            return frameworkResultService.Failed(userClaimModelValidation);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while removing user claims.");
            throw;
        }
    }

    /// <summary>
    /// Deletes the specified admin claim.
    /// </summary>
    /// <param name="userClaimModel">The user claim model.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> RemoveAdminClaimAsync(UserClaimModel userClaimModel)
    {
        try
        {
            var userClaimModelSpecification = new UserClaimModelSpecification(CrudMode.Delete);
            var userClaimModelValidation = await userClaimModelSpecification.ValidateAsync(userClaimModel);
            if (userClaimModelSpecification.IsValid)
            {
                loggerService.WriteTo(Log.Debug, "Entered into remove user claims");
                return await RemoveUserClaims(userClaimModel, true, true);
            }

            return frameworkResultService.Failed<FrameworkResult>(userClaimModelValidation.ErrorCode);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while removing user claims.");
            throw;
        }
    }

    /// <summary>
    /// Performs the replace claim operation.
    /// </summary>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> ReplaceClaimAsync(UserClaimModel existingUserClaimModel,
        UserClaimModel newUserClaimModel)
    {
        try
        {
            var userClaimModelSpecification = new UserClaimModelSpecification(CrudMode.Delete);
            var existingUserClaimModelValidation =
                await userClaimModelSpecification.ValidateAsync(existingUserClaimModel);
            if (userClaimModelSpecification.IsValid)
            {
                loggerService.WriteTo(Log.Debug, "Entered into replace user claims");
                var newUserClaimModelValidation = await userClaimModelSpecification.ValidateAsync(newUserClaimModel);
                if (userClaimModelSpecification.IsValid)
                {
                    var userClaim = new Claim(existingUserClaimModel.ClaimType, existingUserClaimModel.ClaimValue);
                    var userclaims = await userManagementUnitOfWork.UserClaimRepository.FindIdByClaimAsync(
                        existingUserClaimModel.UserId,
                        userClaim);
                    if (userclaims != null && userclaims.Id > 0)
                    {
                        loggerService.WriteTo(Log.Debug,
                            "Entered in replace User claim -  ClaimType :{userclaims.ClaimType}, ClaimValue: {userclaims.ClaimValue}");
                        userclaims.ClaimType = newUserClaimModel.ClaimType;
                        userclaims.ClaimValue = newUserClaimModel.ClaimValue;
                        await userManagementUnitOfWork.UserClaimRepository.UpdateAsync(userclaims);
                        var csResult = await userManagementUnitOfWork.UserClaimRepository.SaveChangesAsync();
                        return csResult;
                    }

                    return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserClaims);
                }

                return frameworkResultService.Failed<FrameworkResult>(newUserClaimModelValidation.ErrorCode);
            }

            return frameworkResultService.Failed<FrameworkResult>(existingUserClaimModelValidation.ErrorCode);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while replacing user claims.");
            throw;
        }
    }

    /// <summary>
    /// Retrieves the claim.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <returns>The matching collection of results.</returns>
    public virtual async Task<IList<Claim>> GetClaimsAsync(Guid userId)
    {
        if (!userId.IsValid()) frameworkResultService.Throw(ApiErrorCodes.InvalidUserId);

        try
        {
            var user = await FindByIdAsync(userId);
            if (user != null)
            {
                var claimList = await userManager.GetClaimsAsync(user);
                if (claimList.ContainsAny())
                {
                    loggerService.WriteTo(Log.Debug, "Entered in Get Claims : Count : " + claimList.Count);
                    return claimList.ToList();
                }
            }

            return frameworkResultService.EmptyResult<IList<Claim>>(ApiErrorCodes.NoRecordsFound);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while getting user claims.");
            throw;
        }
    }

    /// <summary>
    /// Retrieves the user claim.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <returns>The matching collection of results.</returns>
    public virtual async Task<IList<UserClaimModel>> GetUserClaimsAsync(Guid userId)
    {
        if (!userId.IsValid()) frameworkResultService.Throw(ApiErrorCodes.InvalidUserId);

        try
        {
            var user = await FindByIdAsync(userId);
            if (user != null)
            {
                var userClaims = await userManagementUnitOfWork.UserClaimRepository.GetClaimsAsync(userId);
                if (userClaims.ContainsAny())
                {
                    loggerService.WriteTo(Log.Debug, "Entered in Get User Claims : Count : " + userClaims.Count);
                    var userClaimList = mapper.Map<List<UserClaims>, List<UserClaimModel>>(userClaims.ToList());
                    return userClaimList;
                }
            }

            return frameworkResultService.EmptyResult<List<UserClaimModel>>(ApiErrorCodes.NoRecordsFound);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while getting user claims.");
            throw;
        }
    }

    /// <summary>
    /// Retrieves the admin user claim.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <returns>The matching collection of results.</returns>
    public virtual async Task<IList<UserClaimModel>> GetAdminUserClaimsAsync(Guid userId)
    {
        if (!userId.IsValid()) frameworkResultService.Throw(ApiErrorCodes.InvalidUserId);

        try
        {
            var user = await FindByIdAsync(userId);
            if (user != null)
            {
                var userClaims = await userManagementUnitOfWork.UserClaimRepository.GetAdminUserClaimsAsync(userId);
                if (userClaims.ContainsAny())
                {
                    loggerService.WriteTo(Log.Debug, "Entered in Get User Claims : Count : " + userClaims.Count);
                    var userClaimList = mapper.Map<List<UserClaims>, List<UserClaimModel>>(userClaims.ToList());
                    return userClaimList;
                }
            }

            return frameworkResultService.EmptyResult<List<UserClaimModel>>(ApiErrorCodes.NoRecordsFound);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while getting user claims.");
            throw;
        }
    }

    private async Task<FrameworkResult> AddUserClaimAsync(UserClaimModel userClaimModel, bool isAlsoSave)
    {
        var user = await FindByIdAsync(userClaimModel.UserId);
        if (user != null)
        {
            var userClaim = new Claim(userClaimModel.ClaimType, userClaimModel.ClaimValue);
            var userclaims = await userManagementUnitOfWork.UserClaimRepository.FindIdByClaimAsync(
                userClaimModel.UserId,
                userClaim);
            if (userclaims == null)
            {
                var userClaimEntity = mapper.Map<UserClaimModel, UserClaims>(userClaimModel);
                await userManagementUnitOfWork.UserClaimRepository.InsertAsync(userClaimEntity);
                if (isAlsoSave)
                {
                    var csResult = await userManagementUnitOfWork.UserClaimRepository.SaveChangesAsync();
                    return csResult;
                }
            }
            else
            {
                return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.UserClaimsAlreadyExists);
            }
        }
        else
        {
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserId);
        }

        return frameworkResultService.Succeeded();
    }

    private async Task AddUserClaimsAsync(UserClaimModel userClaimModel)
    {
        var userClaimEntity = mapper.Map<UserClaimModel, UserClaims>(userClaimModel);
        await userManagementUnitOfWork.UserClaimRepository.InsertAsync(userClaimEntity);
    }

    private async Task<FrameworkResult> UpdateClaimAsync(UserClaimModel userClaimModel)
    {
        var userclaims = await userManagementUnitOfWork.UserClaimRepository.FindUserClaimByIdAsync(userClaimModel.Id);
        if (userclaims != null)
        {
            userclaims.ClaimType = userClaimModel.ClaimType;
            userclaims.ClaimValue = userClaimModel.ClaimValue;
            await userManagementUnitOfWork.UserClaimRepository.UpdateAsync(userclaims);
            return frameworkResultService.Succeeded();
        }

        return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserClaims);
    }

    private async Task<FrameworkResult> RemoveUserClaims(UserClaimModel userClaimModel, bool isAlsoSaves,
        bool isAdminClaim = false)
    {
        try
        {
            var userClaim = new Claim(userClaimModel.ClaimType, userClaimModel.ClaimValue);
            var userclaims = await userManagementUnitOfWork.UserClaimRepository.FindIdByClaimAsync(
                userClaimModel.UserId,
                userClaim,
                isAdminClaim);
            if (userclaims != null && userclaims.Id > 0)
            {
                await userManagementUnitOfWork.UserClaimRepository.DeleteAsync(userclaims.Id);
                if (isAlsoSaves)
                {
                    var csResult = await userManagementUnitOfWork.UserClaimRepository.SaveChangesAsync();
                    return csResult;
                }
            }
            else
            {
                return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserClaims);
            }
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while removing claims.");
            throw;
        }

        return frameworkResultService.Succeeded();
    }
}
