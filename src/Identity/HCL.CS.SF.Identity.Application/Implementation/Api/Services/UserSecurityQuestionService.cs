/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Identity;
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
/// Partial class extension of UserAccountService providing security question management.
/// Security questions serve as a knowledge-based authentication factor for account recovery
/// (e.g., unlocking locked accounts). Answers are hashed before storage using the configured
/// password hasher to prevent plaintext exposure. Supports CRUD for both the question catalog
/// and per-user question-answer bindings.
/// </summary>

public partial class UserAccountService : SecurityBase, IUserAccountService
{
    /// <summary>
    /// Adds a new security questio.
    /// </summary>
    /// <param name="securityQuestionModel">The security question model.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> AddSecurityQuestionAsync(SecurityQuestionModel securityQuestionModel)
    {
        if (securityQuestionModel == null)
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidSecurityQuestionModel);

        try
        {
            var modelSpecification =
                new SecurityQuestionModelSpecification(CrudMode.Add, securityQuestionModel, userManagementUnitOfWork);
            var modelValidation = await modelSpecification.ValidateAsync(securityQuestionModel);
            if (modelSpecification.IsValid)
            {
                var entity = mapper.Map<SecurityQuestionModel, SecurityQuestions>(securityQuestionModel);
                loggerService.WriteTo(Log.Debug, "Entered in add security questions : " + entity.Question);
                await userManagementUnitOfWork.SecurityQuestionsRepository.InsertAsync(entity);
                return await userManagementUnitOfWork.SecurityQuestionsRepository.SaveChangesAsync();
            }

            return frameworkResultService.Failed<FrameworkResult>(modelValidation.ErrorCode);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while adding security question.");
            throw;
        }
    }

    /// <summary>
    /// Updates an existing security questio.
    /// </summary>
    /// <param name="securityQuestionModel">The security question model.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> UpdateSecurityQuestionAsync(SecurityQuestionModel securityQuestionModel)
    {
        if (securityQuestionModel == null)
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidSecurityQuestionModel);

        try
        {
            var modelSpecification =
                new SecurityQuestionModelSpecification(CrudMode.Update, securityQuestionModel,
                    userManagementUnitOfWork);
            var modelValidation = await modelSpecification.ValidateAsync(securityQuestionModel);
            if (modelSpecification.IsValid)
            {
                var securityQuestionEntity =
                    await userManagementUnitOfWork.SecurityQuestionsRepository.GetAsync(securityQuestionModel.Id);
                if (securityQuestionEntity != null)
                {
                    securityQuestionEntity = mapper.Map(securityQuestionModel, securityQuestionEntity);
                    loggerService.WriteTo(Log.Debug,
                        "Entered in Update security question : " + securityQuestionEntity.Question);
                    await userManagementUnitOfWork.SecurityQuestionsRepository.UpdateAsync(securityQuestionEntity);
                    return await userManagementUnitOfWork.SecurityQuestionsRepository.SaveChangesAsync();
                }

                return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidSecurityQuestionId);
            }

            return frameworkResultService.Failed<FrameworkResult>(modelValidation.ErrorCode);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while updating security question.");
            throw;
        }
    }

    /// <summary>
    /// Deletes the specified security questio.
    /// </summary>
    /// <param name="securityQuestionId">The security question id.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> DeleteSecurityQuestionAsync(Guid securityQuestionId)
    {
        if (!securityQuestionId.IsValid())
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidSecurityQuestionId);

        try
        {
            var isUsersSecurityQuestionExists =
                await userManagementUnitOfWork.UserSecurityQuestionsRepository.ActiveRecordExistsAsync(x =>
                    x.SecurityQuestionId == securityQuestionId);
            if (isUsersSecurityQuestionExists)
                return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.SecurityQuestionUserAssociated);

            var securityQuestions =
                await userManagementUnitOfWork.SecurityQuestionsRepository.GetAsync(securityQuestionId);
            if (securityQuestions != null)
            {
                loggerService.WriteTo(Log.Debug, "Entered in delete security question : " + securityQuestions.Question);
                await userManagementUnitOfWork.SecurityQuestionsRepository.DeleteAsync(securityQuestions);
                return await userManagementUnitOfWork.SecurityQuestionsRepository.SaveChangesAsync();
            }

            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.SecurityQuestionNotExists);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while deleting security question.");
            throw;
        }
    }

    /// <summary>
    /// Retrieves the all security questio.
    /// </summary>
    /// <returns>The matching collection of results.</returns>
    public virtual async Task<IList<SecurityQuestionModel>> GetAllSecurityQuestionsAsync()
    {
        try
        {
            var securityQuestions = await userManagementUnitOfWork.SecurityQuestionsRepository.GetAllAsync();
            if (securityQuestions.ContainsAny())
            {
                loggerService.WriteTo(Log.Debug,
                    "Entered in Get all security questions : " + securityQuestions.ToList().Count);
                return mapper.Map<List<SecurityQuestions>, List<SecurityQuestionModel>>(securityQuestions.ToList());
            }

            return frameworkResultService.EmptyResult<IList<SecurityQuestionModel>>(ApiErrorCodes.NoRecordsFound);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while getting security questions.");
            throw;
        }
    }

    /// <summary>
    /// Retrieves the user security questio.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <returns>The matching collection of results.</returns>
    public virtual async Task<IList<UserSecurityQuestionModel>> GetUserSecurityQuestionsAsync(Guid userId)
    {
        if (!userId.IsValid()) frameworkResultService.Throw(ApiErrorCodes.InvalidUserId);

        try
        {
            var userSecurityQuestions =
                await userSecurityQuestionsRepository.GetAsync(userQuestion => userQuestion.User.Id == userId);
            if (userSecurityQuestions.ContainsAny())
            {
                loggerService.WriteTo(Log.Debug,
                    "Entered in Get User security questions : Count : " + userSecurityQuestions.Count);
                var result = mapper.Map<IList<UserSecurityQuestions>, IList<UserSecurityQuestionModel>>(
                    userSecurityQuestions);
                return result;
            }

            return frameworkResultService.EmptyResult<IList<UserSecurityQuestionModel>>(ApiErrorCodes.NoRecordsFound);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while getting user security questions.");
            throw;
        }
    }

    /// <summary>
    /// Adds a new user security questio.
    /// </summary>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> AddUserSecurityQuestionAsync(
        UserSecurityQuestionModel userSecurityQuestionModel)
    {
        try
        {
            var userSecurityQuestionSpecification = new UserSecurityQuestionModelSpecification(CrudMode.Add,
                securityConfig.SystemSettings.UserConfig, userManager, userManagementUnitOfWork);
            var userSecurityQuestionModelValidation =
                await userSecurityQuestionSpecification.ValidateAsync(userSecurityQuestionModel);
            if (userSecurityQuestionSpecification.IsValid)
            {
                var userSecurityQuestions =
                    mapper.Map<UserSecurityQuestionModel, UserSecurityQuestions>(userSecurityQuestionModel);
                var duplicateExists =
                    await userSecurityQuestionsRepository.DuplicateExistsAsync(entity =>
                        entity.SecurityQuestionId == userSecurityQuestionModel.SecurityQuestionId &&
                        entity.UserId == userSecurityQuestionModel.UserId);
                if (!duplicateExists)
                {
                    loggerService.WriteTo(Log.Debug,
                        "Entered in Add User security question : " + userSecurityQuestions.SecurityQuestionId);
                    userSecurityQuestions.Answer =
                        passwordHasher.HashPassword(null, userSecurityQuestions.Answer.ToUpper());
                    await userSecurityQuestionsRepository.InsertAsync(userSecurityQuestions);
                    return await userSecurityQuestionsRepository.SaveChangesAsync();
                }

                return frameworkResultService.Failed<FrameworkResult>(
                    ApiErrorCodes.SecurityQuestionAlreadyExistsForUser);
            }

            return frameworkResultService.Failed<FrameworkResult>(userSecurityQuestionModelValidation.ErrorCode);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while adding user security questions.");
            throw;
        }
    }

    /// <summary>
    /// Adds a new user security questio.
    /// </summary>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> AddUserSecurityQuestionAsync(
        IList<UserSecurityQuestionModel> userSecurityQuestionModels)
    {
        if (!userSecurityQuestionModels.ContainsAny())
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserSecurityQuestionModel);

        try
        {
            var userSecurityQuestionSpecification = new UserSecurityQuestionModelSpecification(CrudMode.Add,
                securityConfig.SystemSettings.UserConfig, userManager, userManagementUnitOfWork);
            var userSecurityQuestionModelValidation =
                await userSecurityQuestionSpecification.ValidateAsync(userSecurityQuestionModels);
            if (userSecurityQuestionSpecification.IsValid)
            {
                loggerService.WriteTo(Log.Debug,
                    "Entered in Add user security questions method : Count :" + userSecurityQuestionModels.Count);
                var userSecurityQuestionsEntity =
                    mapper.Map<IList<UserSecurityQuestionModel>, IList<UserSecurityQuestions>>(
                        userSecurityQuestionModels);
                foreach (var userSecurityQuestion in userSecurityQuestionsEntity)
                {
                    var duplicateExists =
                        await userManagementUnitOfWork.UserSecurityQuestionsRepository.DuplicateExistsAsync(entity =>
                            entity.SecurityQuestionId == userSecurityQuestion.SecurityQuestionId &&
                            entity.UserId == userSecurityQuestion.UserId);
                    if (!duplicateExists)
                    {
                        userSecurityQuestion.Answer =
                            passwordHasher.HashPassword(null, userSecurityQuestion.Answer.ToUpper());
                        await userManagementUnitOfWork.UserSecurityQuestionsRepository.InsertAsync(
                            userSecurityQuestion);
                    }
                    else
                    {
                        return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes
                            .SecurityQuestionAlreadyExistsForUser);
                    }
                }

                return await userManagementUnitOfWork.UserSecurityQuestionsRepository.SaveChangesAsync();
            }

            return frameworkResultService.Failed(userSecurityQuestionModelValidation);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while adding user security questions.");
            throw;
        }
    }

    /// <summary>
    /// Updates an existing user security questio.
    /// </summary>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> UpdateUserSecurityQuestionAsync(
        UserSecurityQuestionModel userSecurityQuestionModel)
    {
        try
        {
            var userSecurityQuestionSpecification = new UserSecurityQuestionModelSpecification(CrudMode.Update,
                securityConfig.SystemSettings.UserConfig, userManager, userManagementUnitOfWork);
            var userSecurityQuestionModelValidation =
                await userSecurityQuestionSpecification.ValidateAsync(userSecurityQuestionModel);
            if (userSecurityQuestionSpecification.IsValid)
            {
                loggerService.WriteTo(Log.Debug, "Entered in Update security questions method.");
                var questionResult = await userManagementUnitOfWork.UserSecurityQuestionsRepository.GetAsync(entity =>
                    entity.SecurityQuestionId == userSecurityQuestionModel.SecurityQuestionId &&
                    entity.UserId == userSecurityQuestionModel.UserId);
                if (questionResult.ContainsAny())
                {
                    var userSecurityQuestions = mapper.Map(userSecurityQuestionModel, questionResult.FirstOrDefault());
                    userSecurityQuestions.Answer =
                        passwordHasher.HashPassword(null, userSecurityQuestions.Answer.ToUpper());
                    await userManagementUnitOfWork.UserSecurityQuestionsRepository.UpdateAsync(userSecurityQuestions);
                    return await userManagementUnitOfWork.UserSecurityQuestionsRepository.SaveChangesAsync();
                }

                return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.SecurityQuestionNotExistsForUser);
            }

            return frameworkResultService.Failed<FrameworkResult>(userSecurityQuestionModelValidation.ErrorCode);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while updating user security questions.");
            throw;
        }
    }

    /// <summary>
    /// Deletes the specified user security questio.
    /// </summary>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> DeleteUserSecurityQuestionAsync(
        UserSecurityQuestionModel userSecurityQuestionModel)
    {
        if (userSecurityQuestionModel == null)
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserSecurityQuestionModel);

        if (!userSecurityQuestionModel.Id.IsValid())
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidSecurityQuestionId);

        try
        {
            loggerService.WriteTo(Log.Debug, "Entered in Delete user security questions method.");
            var userSecurityQuestions =
                await userManagementUnitOfWork.UserSecurityQuestionsRepository.GetAsync(userSecurityQuestionModel.Id);
            if (userSecurityQuestions != null)
            {
                await userManagementUnitOfWork.UserSecurityQuestionsRepository.DeleteAsync(userSecurityQuestions);
                var instance = await userManagementUnitOfWork.UserSecurityQuestionsRepository.SaveChangesAsync();
                return instance;
            }

            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.SecurityQuestionNotExistsForUser);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while deleting user security questions.");
            throw;
        }
    }

    /// <summary>
    /// Deletes the specified user security questio.
    /// </summary>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> DeleteUserSecurityQuestionAsync(
        IList<UserSecurityQuestionModel> userSecurityQuestionModels)
    {
        if (!userSecurityQuestionModels.ContainsAny())
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserSecurityQuestionModel);

        try
        {
            loggerService.WriteTo(Log.Debug,
                "Entered in Delete user security questions method : Count : " + userSecurityQuestionModels.Count);
            foreach (var userSecurityQuestionModel in userSecurityQuestionModels)
            {
                var userSecurityQuestions =
                    await userManagementUnitOfWork.UserSecurityQuestionsRepository.GetAsync(
                        userSecurityQuestionModel.Id);
                if (userSecurityQuestions != null)
                    await userManagementUnitOfWork.UserSecurityQuestionsRepository.DeleteAsync(userSecurityQuestions);
                else
                    return frameworkResultService.Failed<FrameworkResult>(
                        ApiErrorCodes.SecurityQuestionNotExistsForUser);
            }

            return await userManagementUnitOfWork.UserSecurityQuestionsRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while deleting user security questions.");
            throw;
        }
    }

    private async Task AddUserQuestionAsync(UserSecurityQuestionModel userSecurityQuestionModel)
    {
        var userSecurityQuestions =
            mapper.Map<UserSecurityQuestionModel, UserSecurityQuestions>(userSecurityQuestionModel);
        userSecurityQuestions.Answer = passwordHasher.HashPassword(null, userSecurityQuestions.Answer.ToUpper());
        await userManagementUnitOfWork.UserSecurityQuestionsRepository.InsertAsync(userSecurityQuestions);
    }

    private async Task<FrameworkResult> UpdateUserQuestionAsync(UserSecurityQuestionModel userSecurityQuestionModel)
    {
        var securityQuestionlst = await userManagementUnitOfWork.UserSecurityQuestionsRepository.GetAsync(entity =>
            entity.Id == userSecurityQuestionModel.Id);
        if (securityQuestionlst.ContainsAny())
        {
            var userSecurityQuestions = securityQuestionlst.FirstOrDefault();
            userSecurityQuestions.ModifiedBy = userSecurityQuestionModel.ModifiedBy;
            userSecurityQuestions.SecurityQuestionId = userSecurityQuestionModel.SecurityQuestionId;
            userSecurityQuestions.Answer = passwordHasher.HashPassword(null, userSecurityQuestions.Answer.ToUpper());
            await userManagementUnitOfWork.UserSecurityQuestionsRepository.UpdateAsync(userSecurityQuestions);
            return frameworkResultService.Succeeded();
        }

        return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserSecurityQuestionModel);
    }

    private async Task<FrameworkResult> VerifySecurityQuestions(Guid userId,
        IList<UserSecurityQuestionModel> userSecurityQuestionModel)
    {
        if (userSecurityQuestionModel == null)
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserSecurityQuestionModel);

        loggerService.WriteTo(Log.Debug, "Entered in verify security questions method.");
        var userSecurityQuestions =
            mapper.Map<IList<UserSecurityQuestionModel>, IList<UserSecurityQuestions>>(userSecurityQuestionModel);

        foreach (var securityQuestion in userSecurityQuestions)
            if (userId == securityQuestion.UserId)
            {
                var userQuestionList =
                    await userManagementUnitOfWork.UserSecurityQuestionsRepository
                        .GetAsync(
                            x => x.SecurityQuestionId == securityQuestion.SecurityQuestionId &&
                                 x.UserId == securityQuestion.UserId,
                            x => new { x.Answer });
                if (userQuestionList.ContainsAny())
                    foreach (var userQuestion in userQuestionList)
                    {
                        var compareResult = passwordHasher.VerifyHashedPassword(null, userQuestion.Answer,
                            securityQuestion.Answer.ToUpper());
                        if (compareResult == PasswordVerificationResult.Failed)
                            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes
                                .AnswerDoesNotMatchForSecurityQuestion);
                    }
            }
            else
            {
                return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserIdForSecurityQuestion);
            }

        return frameworkResultService.Succeeded();
    }
}
