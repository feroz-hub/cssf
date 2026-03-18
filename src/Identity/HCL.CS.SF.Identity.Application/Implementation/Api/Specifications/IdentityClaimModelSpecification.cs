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
/// Validates identity resource claim models, ensuring the identity resource ID is valid,
/// claim type is present and within length limits, and no duplicate claims exist.
/// </summary>

public sealed class IdentityClaimModelSpecification : BaseDomainModelValidator<IdentityClaimsModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityClaimModelSpecification"/> class.
    /// </summary>
    internal IdentityClaimModelSpecification(CrudMode crudMode, IRepository<IdentityClaims> identityClaimRepository)
    {
        Add("CheckValidIdentityResourceId", new Rule<IdentityClaimsModel>(
            new IsValidIdentifier<IdentityClaimsModel>(model => model.IdentityResourceId),
            ApiErrorCodes.IdentityResourceIdInvalid));

        Add("CheckValidIdentityResourceClaimType", new Rule<IdentityClaimsModel>(
            new IsNotNull<IdentityClaimsModel>(model => model.Type),
            ApiErrorCodes.IdentityResourceClaimTypeRequired));

        Add("CheckValidIdentityResourceClaimTypeLength", new Rule<IdentityClaimsModel>(
            new IsValid255CharLength<IdentityClaimsModel>(model => model.Type),
            ApiErrorCodes.IdentityResourceClaimTypeTooLong));

        if (crudMode == CrudMode.Add)
        {
            Add("CheckValidIdentityResourceClaimCreatedBy", new Rule<IdentityClaimsModel>(
                new IsValid255CharLength<IdentityClaimsModel>(model => model.CreatedBy),
                ApiErrorCodes.CreatedByTooLong));

            Add("CheckDuplicateIdentityRecourceClaims", new Rule<IdentityClaimsModel>(
                new IdentityResourceClaimDuplicateCheck(identityClaimRepository),
                ApiErrorCodes.IdentityResourceClaimAlreadyExists));
        }
    }

    /// <summary>
    /// Checks that a claim with the same type does not already exist for the given identity resource.
    /// </summary>

    private class IdentityResourceClaimDuplicateCheck : ISpecification<IdentityClaimsModel>
    {
        private readonly IRepository<IdentityClaims> identityResourceClaimRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityResourceClaimDuplicateCheck"/> class.
        /// </summary>
        internal IdentityResourceClaimDuplicateCheck(IRepository<IdentityClaims> identityResourceClaimRepository)
        {
            this.identityResourceClaimRepository = identityResourceClaimRepository;
        }

        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(IdentityClaimsModel model)
        {
            var duplicateExists = identityResourceClaimRepository.DuplicateExistsAsync(apiResourceClaim =>
                    apiResourceClaim.IdentityResourceId == model.IdentityResourceId &&
                    apiResourceClaim.Type == model.Type)
                .GetAwaiter().GetResult();
            if (duplicateExists) return false;

            return true;
        }
    }
}
