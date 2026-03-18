/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using DomainValidation.Interfaces.Specification;
using DomainValidation.Validation;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Configurations.Api;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.DomainServices.UnitOfWork.Api;
using HCL.CS.SF.DomainServices.Wrappers;
using HCL.CS.SF.Service.Implementation.Api.Validators;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Implementation.Endpoint.Validators;

namespace HCL.CS.SF.Service.Implementation.Api.Specifications;

/// <summary>
/// Validates user security question models for CRUD operations, ensuring
/// required fields (userId, questionId, answer) are present and valid,
/// answer length meets minimum requirements, and referenced user/question entities exist.
/// </summary>

internal sealed class UserSecurityQuestionModelSpecification : BaseDomainModelValidator<UserSecurityQuestionModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserSecurityQuestionModelSpecification"/> class.
    /// </summary>
    internal UserSecurityQuestionModelSpecification(CrudMode crudMode, UserConfig userConfig,
        UserManagerWrapper<Users> userManager, IUserManagementUnitOfWork userManagementUnitOfWork)
    {
        Add("CheckUserSecurityQuestionModel", new Rule<UserSecurityQuestionModel>(
            new IsNotNull<UserSecurityQuestionModel>(model => model),
            ApiErrorCodes.InvalidUserSecurityQuestionModel));
        Add("CheckValidUserId", new Rule<UserSecurityQuestionModel>(
            new IsValidIdentifier<UserSecurityQuestionModel>(model => model.UserId),
            ApiErrorCodes.InvalidUserIdForSecurityQuestion));
        Add("CheckValidSecurityQuestionId", new Rule<UserSecurityQuestionModel>(
            new IsValidIdentifier<UserSecurityQuestionModel>(model => model.SecurityQuestionId),
            ApiErrorCodes.InvalidSecurityQuestionId));
        Add("CheckUserSecurityQuestionAnswer", new Rule<UserSecurityQuestionModel>(
            new IsNotNull<UserSecurityQuestionModel>(model => model.Answer),
            ApiErrorCodes.UserSecurityAnswerIsRequired));
        Add("CheckUserSecurityQuestionAnswerLength", new Rule<UserSecurityQuestionModel>(
            new CheckLengthRestrictions<UserSecurityQuestionModel>(
                model => model.Answer,
                model => userConfig.MinSecurityAnswersLength,
                model => "<"),
            ApiErrorCodes.InvalidLengthForUserSecurityAnswer));
        Add("CheckUserSecurityQuestionCreatedByLength", new Rule<UserSecurityQuestionModel>(
            new IsValid255CharLength<UserSecurityQuestionModel>(model => model.CreatedBy),
            ApiErrorCodes.CreatedByTooLong));

        Add("CheckUserExist", new Rule<UserSecurityQuestionModel>(
            new CheckUserExist(userManager),
            ApiErrorCodes.InvalidUserId));

        Add("CheckSecurityQuestionExist", new Rule<UserSecurityQuestionModel>(
            new CheckSecurityQuestionExist(userManagementUnitOfWork),
            ApiErrorCodes.InvalidSecurityQuestionId));

        if (crudMode == CrudMode.Update)
            Add("CheckUserSecurityQuestionModifiedByLength", new Rule<UserSecurityQuestionModel>(
                new IsValid255CharLength<UserSecurityQuestionModel>(model => model.ModifiedBy),
                ApiErrorCodes.ModifiedByTooLong));
    }

    /// <summary>
    /// Validates that the security question answer meets the minimum length requirement.
    /// </summary>

    internal class AnswerLengthCheck(UserConfig userConfig) : ISpecification<UserSecurityQuestionModel>
    {
        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(UserSecurityQuestionModel entity)
        {
            if (entity.Answer.Length < userConfig.MinSecurityAnswersLength) return false;

            return true;
        }
    }

    /// <summary>
    /// Validates that the referenced user exists in the identity store.
    /// </summary>

    private class CheckUserExist : ISpecification<UserSecurityQuestionModel>
    {
        private readonly UserManagerWrapper<Users> userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckUserExist"/> class.
        /// </summary>
        internal CheckUserExist(UserManagerWrapper<Users> userManager)
        {
            this.userManager = userManager;
        }

        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(UserSecurityQuestionModel model)
        {
            var userExist = userManager.FindByIdAsync(model.UserId.ToString()).GetAwaiter().GetResult();
            return userExist != null;
        }
    }

    /// <summary>
    /// Validates that the referenced security question exists in the repository.
    /// </summary>

    private class CheckSecurityQuestionExist : ISpecification<UserSecurityQuestionModel>
    {
        private readonly IUserManagementUnitOfWork userManagementUnitOfWork;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckSecurityQuestionExist"/> class.
        /// </summary>
        internal CheckSecurityQuestionExist(IUserManagementUnitOfWork userManagementUnitOfWork)
        {
            this.userManagementUnitOfWork = userManagementUnitOfWork;
        }

        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(UserSecurityQuestionModel model)
        {
            var securityQuestions = userManagementUnitOfWork.SecurityQuestionsRepository
                .GetAsync(p => p.Id == model.SecurityQuestionId).GetAwaiter().GetResult();

            return securityQuestions.ContainsAny();
        }
    }
}
