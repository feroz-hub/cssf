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
/// Validates API resource claim models, ensuring the resource ID is valid, claim type
/// is present and within length limits, and no duplicate claims exist for the same resource.
/// </summary>

public sealed class ApiResourceClaimModelSpecification : BaseDomainModelValidator<ApiResourceClaimsModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResourceClaimModelSpecification"/> class.
    /// </summary>
    internal ApiResourceClaimModelSpecification(CrudMode crudMode,
        IRepository<ApiResourceClaims> apiResourceClaimRepository)
    {
        Add("CheckValidApiResourceId", new Rule<ApiResourceClaimsModel>(
            new IsValidIdentifier<ApiResourceClaimsModel>(model => model.ApiResourceId),
            ApiErrorCodes.ApiResourceIdInvalid));

        Add("CheckValidApiResourceClaimType", new Rule<ApiResourceClaimsModel>(
            new IsNotNull<ApiResourceClaimsModel>(model => model.Type),
            ApiErrorCodes.ApiResourceClaimTypeRequired));

        Add("CheckValidApiResourceClaimTypeLength", new Rule<ApiResourceClaimsModel>(
            new IsValid255CharLength<ApiResourceClaimsModel>(model => model.Type),
            ApiErrorCodes.ApiResourceClaimTypeTooLong));

        if (crudMode == CrudMode.Add)
        {
            Add("CheckValidApiResourceClaimCreatedBy", new Rule<ApiResourceClaimsModel>(
                new IsValid255CharLength<ApiResourceClaimsModel>(model => model.CreatedBy),
                ApiErrorCodes.CreatedByTooLong));

            Add("CheckDuplicateApiRecourceClaims", new Rule<ApiResourceClaimsModel>(
                new ApiResourceClaimDuplicateCheck(apiResourceClaimRepository),
                ApiErrorCodes.ApiResourceClaimAlreadyExists));
        }
    }

    /// <summary>
    /// Checks that a claim with the same type does not already exist for the given API resource.
    /// </summary>

    private class ApiResourceClaimDuplicateCheck : ISpecification<ApiResourceClaimsModel>
    {
        private readonly IRepository<ApiResourceClaims> apiResourceClaimRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResourceClaimDuplicateCheck"/> class.
        /// </summary>
        internal ApiResourceClaimDuplicateCheck(IRepository<ApiResourceClaims> apiResourceClaimRepository)
        {
            this.apiResourceClaimRepository = apiResourceClaimRepository;
        }

        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(ApiResourceClaimsModel model)
        {
            var duplicateExists = apiResourceClaimRepository.DuplicateExistsAsync(apiResourceClaim =>
                    apiResourceClaim.ApiResourceId == model.ApiResourceId &&
                    apiResourceClaim.Type == model.Type)
                .GetAwaiter().GetResult();
            if (duplicateExists) return false;

            return true;
        }
    }
}
