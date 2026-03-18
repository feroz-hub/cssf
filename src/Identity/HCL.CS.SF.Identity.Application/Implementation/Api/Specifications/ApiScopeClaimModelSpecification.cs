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
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.Service.Implementation.Api.Validators;

namespace HCL.CS.SF.Service.Implementation.Api.Specifications;

/// <summary>
/// Validates API scope claim models, ensuring scope ID is valid, claim type is present
/// and within length limits, and no duplicate scope claims exist for the same scope.
/// </summary>

public sealed class ApiScopeClaimModelSpecification : BaseDomainModelValidator<ApiScopeClaimsModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiScopeClaimModelSpecification"/> class.
    /// </summary>
    internal ApiScopeClaimModelSpecification(CrudMode crudMode, IRepository<ApiScopeClaims> apiScopeClaimsRepository)
    {
        Add("CheckValidApiScopeId", new Rule<ApiScopeClaimsModel>(
            new IsValidIdentifier<ApiScopeClaimsModel>(model => model.ApiScopeId),
            ApiErrorCodes.ApiScopeIdInvalid));

        Add("CheckValidApiScopeClaimType", new Rule<ApiScopeClaimsModel>(
            new IsNotNull<ApiScopeClaimsModel>(model => model.Type),
            ApiErrorCodes.ApiScopeClaimTypeRequired));

        Add("CheckValidApiScopeClaimTypeLength", new Rule<ApiScopeClaimsModel>(
            new IsValid255CharLength<ApiScopeClaimsModel>(model => model.Type),
            ApiErrorCodes.ApiScopeClaimTypeTooLong));

        if (crudMode == CrudMode.Add)
        {
            Add("CheckValidApiScopeClaimCreatedBy", new Rule<ApiScopeClaimsModel>(
                new IsValid255CharLength<ApiScopeClaimsModel>(model => model.CreatedBy),
                ApiErrorCodes.CreatedByTooLong));

            Add("CheckDuplicateApiScopeClaims", new Rule<ApiScopeClaimsModel>(
                new ApiScopeClaimDuplicateCheck(apiScopeClaimsRepository),
                ApiErrorCodes.ApiScopeClaimsAlreadyExists));
        }
    }

    /// <summary>
    /// Checks that a claim with the same type does not already exist for the given API scope.
    /// </summary>

    private class ApiScopeClaimDuplicateCheck : ISpecification<ApiScopeClaimsModel>
    {
        private readonly IRepository<ApiScopeClaims> apiScopeClaimsRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiScopeClaimDuplicateCheck"/> class.
        /// </summary>
        internal ApiScopeClaimDuplicateCheck(IRepository<ApiScopeClaims> apiScopeClaimsRepository)
        {
            this.apiScopeClaimsRepository = apiScopeClaimsRepository;
        }

        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(ApiScopeClaimsModel model)
        {
            var duplicateExists = apiScopeClaimsRepository.DuplicateExistsAsync(apiScopeClaim =>
                    apiScopeClaim.ApiScopeId == model.ApiScopeId &&
                    apiScopeClaim.Type == model.Type)
                .GetAwaiter().GetResult();
            if (duplicateExists) return false;

            return true;
        }
    }
}
