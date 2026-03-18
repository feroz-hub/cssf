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
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.DomainServices.UnitOfWork.Api;
using HCL.CS.SF.Service.Implementation.Api.Validators;

namespace HCL.CS.SF.Service.Implementation.Api.Specifications;

/// <summary>
/// Validates security question models for CRUD operations, ensuring question text is present,
/// within length limits, and not a duplicate of an existing question.
/// </summary>

internal sealed class SecurityQuestionModelSpecification : BaseDomainModelValidator<SecurityQuestionModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityQuestionModelSpecification"/> class.
    /// </summary>
    internal SecurityQuestionModelSpecification(CrudMode crudMode, SecurityQuestionModel securityQuestionModel,
        IUserManagementUnitOfWork userManagementUnitOfWork)
    {
        Add("CheckValidQuestion", new Rule<SecurityQuestionModel>(
            new IsNotNull<SecurityQuestionModel>(model => model.Question),
            ApiErrorCodes.InvalidSecurityQuestion));
        Add("CheckValidQuestionLength", new Rule<SecurityQuestionModel>(
            new IsValid255CharLength<SecurityQuestionModel>(model => model.Question),
            ApiErrorCodes.SecurityQuestionTooLong));
        Add("CheckCreatedByLength", new Rule<SecurityQuestionModel>(
            new IsValid255CharLength<SecurityQuestionModel>(model => model.CreatedBy),
            ApiErrorCodes.CreatedByTooLong));

        switch (crudMode)
        {
            case CrudMode.Add:
                Add("CheckSecurityQuestionEntry", new Rule<SecurityQuestionModel>(
                    new CheckSecurityQuestionEntry(securityQuestionModel, userManagementUnitOfWork),
                    ApiErrorCodes.SecurityQuestionAlreadyExists));
                break;
            case CrudMode.Update:
                Add("CheckValidSecurityQuestionId", new Rule<SecurityQuestionModel>(
                    new IsValidIdentifier<SecurityQuestionModel>(model => model.Id),
                    ApiErrorCodes.InvalidSecurityQuestionId));
                Add("CheckModifiedByLength", new Rule<SecurityQuestionModel>(
                    new IsValid255CharLength<SecurityQuestionModel>(model => model.ModifiedBy),
                    ApiErrorCodes.ModifiedByTooLong));

                Add("CheckSecurityQuestionEntry", new Rule<SecurityQuestionModel>(
                    new CheckSecurityQuestionEntry(securityQuestionModel, userManagementUnitOfWork),
                    ApiErrorCodes.SecurityQuestionAlreadyExists));
                break;
            case CrudMode.Delete:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(crudMode), crudMode, null);
        }
    }

    /// <summary>
    /// Checks that no security question with the same text already exists in the repository.
    /// </summary>

    private class CheckSecurityQuestionEntry : ISpecification<SecurityQuestionModel>
    {
        private readonly SecurityQuestionModel securityQuestionModel1;
        private readonly IUserManagementUnitOfWork userManagementUnitOfWork;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckSecurityQuestionEntry"/> class.
        /// </summary>
        internal CheckSecurityQuestionEntry(SecurityQuestionModel securityQuestionModel,
            IUserManagementUnitOfWork userManagementUnitOfWork)
        {
            this.userManagementUnitOfWork = userManagementUnitOfWork;
            securityQuestionModel1 = securityQuestionModel;
        }

        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(SecurityQuestionModel model)
        {
            var duplicateExists = userManagementUnitOfWork.SecurityQuestionsRepository
                .DuplicateExistsAsync(x => x.Question == securityQuestionModel1.Question).GetAwaiter().GetResult();
            return !duplicateExists;
        }
    }
}
