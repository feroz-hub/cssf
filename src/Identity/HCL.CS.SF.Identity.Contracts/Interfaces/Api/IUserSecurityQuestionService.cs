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
/// Partial interface extending <see cref="IUserAccountService"/> with security question management.
/// Provides CRUD for system-level security questions and user-specific security question answers,
/// used for knowledge-based account recovery and unlock flows.
/// </summary>
public partial interface IUserAccountService
{
    /// <summary>Adds a new system-level security question.</summary>
    /// <param name="securityQuestionModel">The security question to add.</param>
    Task<FrameworkResult> AddSecurityQuestionAsync(SecurityQuestionModel securityQuestionModel);

    /// <summary>Updates an existing system-level security question.</summary>
    /// <param name="securityQuestionModel">The security question with updated text.</param>
    Task<FrameworkResult> UpdateSecurityQuestionAsync(SecurityQuestionModel securityQuestionModel);

    /// <summary>Deletes a system-level security question by its identifier.</summary>
    /// <param name="securityQuestionId">The unique identifier of the question to delete.</param>
    Task<FrameworkResult> DeleteSecurityQuestionAsync(Guid securityQuestionId);

    /// <summary>Retrieves all system-level security questions.</summary>
    Task<IList<SecurityQuestionModel>> GetAllSecurityQuestionsAsync();

    /// <summary>Adds a single security question answer for a user.</summary>
    /// <param name="userSecurityQuestionModel">The user's question and answer.</param>
    Task<FrameworkResult> AddUserSecurityQuestionAsync(UserSecurityQuestionModel userSecurityQuestionModel);

    /// <summary>Adds multiple security question answers for a user in a single operation.</summary>
    /// <param name="userSecurityQuestionModels">The list of user question/answer pairs.</param>
    Task<FrameworkResult> AddUserSecurityQuestionAsync(IList<UserSecurityQuestionModel> userSecurityQuestionModels);

    /// <summary>Updates a user's security question answer.</summary>
    /// <param name="userSecurityQuestionModel">The question/answer to update.</param>
    Task<FrameworkResult> UpdateUserSecurityQuestionAsync(UserSecurityQuestionModel userSecurityQuestionModel);

    /// <summary>Deletes a single user security question answer.</summary>
    /// <param name="userSecurityQuestionModel">The question/answer to delete.</param>
    Task<FrameworkResult> DeleteUserSecurityQuestionAsync(UserSecurityQuestionModel userSecurityQuestionModel);

    /// <summary>Deletes multiple user security question answers.</summary>
    /// <param name="userSecurityQuestionModels">The question/answer pairs to delete.</param>
    Task<FrameworkResult> DeleteUserSecurityQuestionAsync(IList<UserSecurityQuestionModel> userSecurityQuestionModels);

    /// <summary>Retrieves all security question answers for the specified user.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    Task<IList<UserSecurityQuestionModel>> GetUserSecurityQuestionsAsync(Guid userId);
}
