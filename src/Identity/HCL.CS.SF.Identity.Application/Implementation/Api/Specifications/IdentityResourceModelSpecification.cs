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
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.Service.Extension;
using HCL.CS.SF.Service.Implementation.Api.Validators;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;

namespace HCL.CS.SF.Service.Implementation.Api.Specifications;

/// <summary>
/// Validates identity resource models for create/update operations. Identity resources
/// represent standard OIDC scopes (e.g., openid, profile, email) and their associated claims.
/// Ensures resource names are unique and claim types are properly structured.
/// </summary>

internal sealed class IdentityResourceModelSpecification : BaseDomainModelValidator<IdentityResourcesModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityResourceModelSpecification"/> class.
    /// </summary>
    internal IdentityResourceModelSpecification(CrudMode crudMode,
        IRepository<IdentityResources> identityResourceRepository)
    {
        Add("CheckIdentityResourceName", new Rule<IdentityResourcesModel>(
            new IsNotNull<IdentityResourcesModel>(model => model.Name),
            ApiErrorCodes.IdentityResourceNameRequired));

        Add("CheckIdentityResourceNameLength", new Rule<IdentityResourcesModel>(
            new IsValid255CharLength<IdentityResourcesModel>(model => model.Name),
            ApiErrorCodes.IdentityResourceNameTooLong));

        Add("CheckIdentityResourceDisplayNameLength", new Rule<IdentityResourcesModel>(
            new IsValid255CharLength<IdentityResourcesModel>(model => model.DisplayName),
            ApiErrorCodes.IdentityDisplayNameTooLong));

        Add("CheckIdentityResourceCreatedByLength", new Rule<IdentityResourcesModel>(
            new IsValid255CharLength<IdentityResourcesModel>(model => model.CreatedBy),
            ApiErrorCodes.CreatedByTooLong));

        switch (crudMode)
        {
            case CrudMode.Add:
                AddRules(identityResourceRepository);
                break;
            case CrudMode.Update:
                UpdateRules();
                break;
        }
    }

    private void AddRules(IRepository<IdentityResources> identityResourceRepository)
    {
        // Identity resource.
        Add("CheckValidIdentityResourceClaimType", new Rule<IdentityResourcesModel>(
            new IdentityClaimsTypeCheck(),
            ApiErrorCodes.IdentityResourceClaimTypeRequired));
        Add("CheckValidIdentityResourceClaimTypeLength", new Rule<IdentityResourcesModel>(
            new IsValid255CharLengths<IdentityResourcesModel>(model =>
                model.IdentityClaims.ContainsAny() ? model.IdentityClaims.ConvertAll(x => x.Type) : null),
            ApiErrorCodes.IdentityResourceClaimTypeTooLong));
        Add("CheckValidIdentityResourceClaimCreatedBy", new Rule<IdentityResourcesModel>(
            new IsValid255CharLengths<IdentityResourcesModel>(model =>
                model.IdentityClaims.ContainsAny() ? model.IdentityClaims.ConvertAll(x => x.CreatedBy) : null),
            ApiErrorCodes.IdentityResourceClaimCreatedByTooLong));
        Add("CheckDuplicateIdentityRecourceEntry", new Rule<IdentityResourcesModel>(
            new IdentityResourceDuplicateCheck(identityResourceRepository),
            ApiErrorCodes.IdentityResourceAlreadyExists));
    }

    private void UpdateRules()
    {
        // Identity resource.
        Add("CheckIdentityResourceId", new Rule<IdentityResourcesModel>(
            new IsValidIdentifier<IdentityResourcesModel>(model => model.Id),
            ApiErrorCodes.IdentityResourceIdInvalid));
        Add("CheckIdentityResourceModifiedByLength", new Rule<IdentityResourcesModel>(
            new IsValid255CharLength<IdentityResourcesModel>(model => model.ModifiedBy),
            ApiErrorCodes.ModifiedByTooLong));

        // Identity resource claim.
        Add("CheckValidIdentityResourceClaim", new Rule<IdentityResourcesModel>(
            new IdentityResourceClaimsCheck(),
            ApiErrorCodes.InvalidIdentityResourceClaimTypeOrResourceId));
        Add("CheckValidIdentityResourceClaimTypeLength", new Rule<IdentityResourcesModel>(
            new IsValid255CharLengths<IdentityResourcesModel>(model =>
                model.IdentityClaims.ContainsAny() ? model.IdentityClaims.ConvertAll(x => x.Type) : null),
            ApiErrorCodes.IdentityResourceClaimTypeTooLong));
        Add("CheckValidIdentityResourceClaimCreatedModifiedByLength", new Rule<IdentityResourcesModel>(
            new IdentityResourceClaimsCreatedModifiedByCheck(),
            ApiErrorCodes.IdentityResourceClaimCreatedbyOrModifiedByTooLong));
    }

    /// <summary>
    /// Ensures all identity claims have a non-empty type.
    /// </summary>

    internal class IdentityClaimsTypeCheck : ISpecification<IdentityResourcesModel>
    {
        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(IdentityResourcesModel model)
        {
            if (model.IdentityClaims.ContainsAny())
                foreach (var identityClaim in model.IdentityClaims)
                    if (string.IsNullOrWhiteSpace(identityClaim.Type))
                        return false;

            return true;
        }
    }

    /// <summary>
    /// Checks that no active identity resource with the same name already exists.
    /// </summary>

    internal class IdentityResourceDuplicateCheck : ISpecification<IdentityResourcesModel>
    {
        private readonly IRepository<IdentityResources> identityResourceRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityResourceDuplicateCheck"/> class.
        /// </summary>
        internal IdentityResourceDuplicateCheck(IRepository<IdentityResources> identityResourceRepository)
        {
            this.identityResourceRepository = identityResourceRepository;
        }

        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(IdentityResourcesModel model)
        {
            var duplicateExists = identityResourceRepository
                .DuplicateExistsAsync(idResource => idResource.Name == model.Name)
                .GetAwaiter().GetResult();
            if (duplicateExists) return false;

            return true;
        }
    }

    /// <summary>
    /// Validates identity claims have valid resource IDs and non-empty types (for updates).
    /// </summary>

    internal class IdentityResourceClaimsCheck : ISpecification<IdentityResourcesModel>
    {
        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(IdentityResourcesModel model)
        {
            if (model.IdentityClaims.ContainsAny())
                foreach (var identityClaims in model.IdentityClaims)
                    if (!identityClaims.IdentityResourceId.IsValid() || string.IsNullOrWhiteSpace(identityClaims.Type))
                        return false;

            return true;
        }
    }

    /// <summary>
    /// Validates that createdBy and modifiedBy fields on identity claims are within 255 characters.
    /// </summary>

    internal class IdentityResourceClaimsCreatedModifiedByCheck : ISpecification<IdentityResourcesModel>
    {
        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(IdentityResourcesModel model)
        {
            if (model.IdentityClaims.ContainsAny())
                foreach (var apiClaim in model.IdentityClaims)
                {
                    if (!string.IsNullOrWhiteSpace(apiClaim.CreatedBy) &&
                        apiClaim.CreatedBy.Length > Constants.ColumnLength255) return false;

                    if (!string.IsNullOrWhiteSpace(apiClaim.ModifiedBy) &&
                        apiClaim.ModifiedBy.Length > Constants.ColumnLength255) return false;
                }

            return true;
        }
    }
}
