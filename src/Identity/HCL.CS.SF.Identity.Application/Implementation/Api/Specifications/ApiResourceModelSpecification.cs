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
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.DomainServices.Repository.Api;
using HCL.CS.SF.Service.Extension;
using HCL.CS.SF.Service.Implementation.Api.Validators;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;

namespace HCL.CS.SF.Service.Implementation.Api.Specifications;

/// <summary>
/// Validates API resource models for create/update operations. API resources represent
/// protected APIs in the OAuth2/OIDC ecosystem; this validator ensures resource names are unique,
/// nested scopes and claims are correctly structured, and field lengths are within database limits.
/// </summary>

internal sealed class ApiResourceModelSpecification : BaseDomainModelValidator<ApiResourcesModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResourceModelSpecification"/> class.
    /// </summary>
    internal ApiResourceModelSpecification(CrudMode crudMode, IApiResourceRepository apiResourceRepository)
    {
        Add("CheckValidApiResourceName", new Rule<ApiResourcesModel>(
            new IsNotNull<ApiResourcesModel>(model => model.Name),
            ApiErrorCodes.ApiResourceNameRequired));

        Add("CheckApiResourceNameLength", new Rule<ApiResourcesModel>(
            new IsValid255CharLength<ApiResourcesModel>(model => model.Name),
            ApiErrorCodes.ApiResourceNameTooLong));

        Add("CheckApiResourceDisplayNameLength", new Rule<ApiResourcesModel>(
            new IsValid255CharLength<ApiResourcesModel>(model => model.DisplayName),
            ApiErrorCodes.ApiResourceDisplayNameTooLong));

        Add("CheckApiResourceCreatedByLength", new Rule<ApiResourcesModel>(
            new IsValid255CharLength<ApiResourcesModel>(model => model.CreatedBy),
            ApiErrorCodes.CreatedByTooLong));

        switch (crudMode)
        {
            case CrudMode.Add:
                AddRules(apiResourceRepository);
                break;
            case CrudMode.Update:
                UpdateRules();
                break;
        }
    }

    private void AddRules(IApiResourceRepository apiResourceRepository)
    {
        // Api resource claim
        Add("CheckValidApiResourceClaimType", new Rule<ApiResourcesModel>(
            new ApiResourceClaimsTypeCheck(),
            ApiErrorCodes.ApiResourceClaimTypeRequired));
        Add("CheckValidApiResourceClaimTypeLength", new Rule<ApiResourcesModel>(
            new IsValid255CharLengths<ApiResourcesModel>(model =>
                model.ApiResourceClaims.ContainsAny() ? model.ApiResourceClaims.ConvertAll(x => x.Type) : null),
            ApiErrorCodes.ApiResourceClaimTypeTooLong));
        Add("CheckValidApiResourceClaimCreatedBy", new Rule<ApiResourcesModel>(
            new IsValid255CharLengths<ApiResourcesModel>(model =>
                model.ApiResourceClaims.ContainsAny() ? model.ApiResourceClaims.ConvertAll(x => x.CreatedBy) : null),
            ApiErrorCodes.ApiResourceClaimCreatedByTooLong));

        // Api scope
        Add("CheckValidApiResourceScopeName", new Rule<ApiResourcesModel>(
            new ApiScopesNameCheck(),
            ApiErrorCodes.ApiScopeNameRequired));
        Add("CheckValidApiResourceScopeLength", new Rule<ApiResourcesModel>(
            new ApiScopesLengthCheck(),
            ApiErrorCodes.ApiScopeNameOrDisplayNameOrCreatedbyTooLong));

        // Api scope claims
        Add("CheckValidApiResourceScopeClaimType", new Rule<ApiResourcesModel>(
            new ApiScopeClaimsTypeCheck(),
            ApiErrorCodes.ApiScopeClaimTypeRequired));
        Add("CheckValidApiResourceScopeClaimTypeAndCreatedbyLength", new Rule<ApiResourcesModel>(
            new ApiScopeClaimsTypeLengthCheck(),
            ApiErrorCodes.ApiScopeClaimTypeOrCreatedbyTooLong));
        Add("CheckDuplicateApiRecourceEntry", new Rule<ApiResourcesModel>(
            new ApiResourceDuplicateCheck(apiResourceRepository),
            ApiErrorCodes.ApiResourceAlreadyExists));
    }

    private void UpdateRules()
    {
        // Api resource
        Add("CheckValidApiResourceId", new Rule<ApiResourcesModel>(
            new IsValidIdentifier<ApiResourcesModel>(model => model.Id),
            ApiErrorCodes.ApiResourceIdInvalid));
        Add("CheckApiResourceModifiedByLength", new Rule<ApiResourcesModel>(
            new IsValid255CharLength<ApiResourcesModel>(model => model.ModifiedBy),
            ApiErrorCodes.ModifiedByTooLong));

        // Api resource claim
        Add("CheckValidApiResourceClaim", new Rule<ApiResourcesModel>(
            new ApiResourceClaimsCheck(),
            ApiErrorCodes.InvalidApiResourceClaimTypeOrResourceId));
        Add("CheckValidApiResourceClaimTypeLength", new Rule<ApiResourcesModel>(
            new IsValid255CharLengths<ApiResourcesModel>(model =>
                model.ApiResourceClaims.ContainsAny() ? model.ApiResourceClaims.ConvertAll(x => x.Type) : null),
            ApiErrorCodes.ApiResourceClaimTypeTooLong));
        Add("CheckValidApiResourceClaimCreatedModifiedByLength", new Rule<ApiResourcesModel>(
            new ApiResourceClaimsCreatedModifiedByCheck(),
            ApiErrorCodes.ApiResourceClaimCreatedbyOrModifiedByTooLong));

        // Api scope
        Add("CheckValidApiResourceScope", new Rule<ApiResourcesModel>(
            new ApiScopesCheck(),
            ApiErrorCodes.InvalidApiScopeNameOrResourceId));
        Add("CheckValidApiResourceScopeLength", new Rule<ApiResourcesModel>(
            new ApiScopesLengthCheck(),
            ApiErrorCodes.ApiScopeNameOrDisplayNameOrCreatedbyTooLong));
        Add("CheckValidApiResourceScopeModifiedByLength", new Rule<ApiResourcesModel>(
            new IsValid255CharLengths<ApiResourcesModel>(model =>
                model.ApiScopes.ContainsAny() ? model.ApiScopes.ConvertAll(x => x.ModifiedBy) : null),
            ApiErrorCodes.ApiScopeModifiedbyTooLong));

        // Api scope claims
        Add("CheckValidApiResourceScopeClaim", new Rule<ApiResourcesModel>(
            new ApiScopeClaimsCheck(),
            ApiErrorCodes.InvalidApiScopeClaimTypeOrScopeId));
        Add("CheckValidApiResourceScopeClaimTypeLength", new Rule<ApiResourcesModel>(
            new ApiScopeClaimsTypeLengthCheck(),
            ApiErrorCodes.ApiScopeClaimTypeOrCreatedbyTooLong));
        Add("CheckValidApiResourceScopeClaimModifiedByLength", new Rule<ApiResourcesModel>(
            new ApiScopeClaimsModifiedByLengthCheck(),
            ApiErrorCodes.ApiScopeClaimModifiedbyTooLong));
    }

    /// <summary>
    /// Ensures all API resource claims have a non-empty type.
    /// </summary>

    internal class ApiResourceClaimsTypeCheck : ISpecification<ApiResourcesModel>
    {
        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(ApiResourcesModel model)
        {
            if (model.ApiResourceClaims.ContainsAny())
                foreach (var apiClaim in model.ApiResourceClaims)
                    if (string.IsNullOrWhiteSpace(apiClaim.Type))
                        return false;

            return true;
        }
    }

    /// <summary>
    /// Validates that createdBy and modifiedBy fields on API resource claims are within 255 characters.
    /// </summary>

    internal class ApiResourceClaimsCreatedModifiedByCheck : ISpecification<ApiResourcesModel>
    {
        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(ApiResourcesModel model)
        {
            if (model.ApiResourceClaims.ContainsAny())
                foreach (var apiClaim in model.ApiResourceClaims)
                {
                    if (!string.IsNullOrWhiteSpace(apiClaim.CreatedBy) &&
                        apiClaim.CreatedBy.Length > Constants.ColumnLength255) return false;

                    if (!string.IsNullOrWhiteSpace(apiClaim.ModifiedBy) &&
                        apiClaim.ModifiedBy.Length > Constants.ColumnLength255) return false;
                }

            return true;
        }
    }

    /// <summary>
    /// Ensures all nested API scopes have a non-empty name.
    /// </summary>

    internal class ApiScopesNameCheck : ISpecification<ApiResourcesModel>
    {
        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(ApiResourcesModel model)
        {
            if (model.ApiScopes.ContainsAny())
                foreach (var apiScope in model.ApiScopes)
                    if (string.IsNullOrWhiteSpace(apiScope.Name))
                        return false;

            return true;
        }
    }

    /// <summary>
    /// Validates that scope name, displayName, and createdBy do not exceed 255 characters.
    /// </summary>

    internal class ApiScopesLengthCheck : ISpecification<ApiResourcesModel>
    {
        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(ApiResourcesModel model)
        {
            if (model.ApiScopes.ContainsAny())
                foreach (var apiScope in model.ApiScopes)
                {
                    if (!string.IsNullOrWhiteSpace(apiScope.Name) && apiScope.Name.Length > Constants.ColumnLength255)
                        return false;

                    if (!string.IsNullOrWhiteSpace(apiScope.DisplayName) &&
                        apiScope.DisplayName.Length > Constants.ColumnLength255) return false;

                    if (!string.IsNullOrWhiteSpace(apiScope.CreatedBy) &&
                        apiScope.CreatedBy.Length > Constants.ColumnLength255) return false;
                }

            return true;
        }
    }

    /// <summary>
    /// Ensures all nested scope claims have a non-empty type.
    /// </summary>

    internal class ApiScopeClaimsTypeCheck : ISpecification<ApiResourcesModel>
    {
        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(ApiResourcesModel model)
        {
            if (model.ApiScopes.ContainsAny())
                foreach (var apiScope in model.ApiScopes)
                    if (apiScope.ApiScopeClaims.ContainsAny())
                        foreach (var apiScopeClaims in apiScope.ApiScopeClaims)
                            if (string.IsNullOrWhiteSpace(apiScopeClaims.Type))
                                return false;

            return true;
        }
    }

    /// <summary>
    /// Validates scope claim type and createdBy lengths.
    /// </summary>

    internal class ApiScopeClaimsTypeLengthCheck : ISpecification<ApiResourcesModel>
    {
        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(ApiResourcesModel model)
        {
            if (model.ApiScopes.ContainsAny())
                foreach (var apiScope in model.ApiScopes)
                    if (apiScope.ApiScopeClaims.ContainsAny())
                        foreach (var apiScopeClaims in apiScope.ApiScopeClaims)
                        {
                            if (!string.IsNullOrWhiteSpace(apiScopeClaims.Type) &&
                                apiScopeClaims.Type.Length > Constants.ColumnLength255) return false;

                            if (!string.IsNullOrWhiteSpace(apiScopeClaims.CreatedBy) &&
                                apiScopeClaims.CreatedBy.Length > Constants.ColumnLength255) return false;
                        }

            return true;
        }
    }

    /// <summary>
    /// Validates that scope claim modifiedBy fields are within 255 characters.
    /// </summary>

    internal class ApiScopeClaimsModifiedByLengthCheck : ISpecification<ApiResourcesModel>
    {
        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(ApiResourcesModel model)
        {
            if (model.ApiScopes.ContainsAny())
                foreach (var apiScope in model.ApiScopes)
                    if (apiScope.ApiScopeClaims.ContainsAny())
                        foreach (var apiScopeClaims in apiScope.ApiScopeClaims)
                            if (!string.IsNullOrWhiteSpace(apiScopeClaims.ModifiedBy) &&
                                apiScopeClaims.ModifiedBy.Length > Constants.ColumnLength255)
                                return false;

            return true;
        }
    }

    /// <summary>
    /// Validates that resource claims have valid resource IDs and non-empty types (for updates).
    /// </summary>

    internal class ApiResourceClaimsCheck : ISpecification<ApiResourcesModel>
    {
        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(ApiResourcesModel model)
        {
            if (model.ApiResourceClaims.ContainsAny())
                foreach (var apiClaim in model.ApiResourceClaims)
                {
                    if (!apiClaim.ApiResourceId.IsValid()) return false;

                    if (string.IsNullOrWhiteSpace(apiClaim.Type)) return false;
                }

            return true;
        }
    }

    /// <summary>
    /// Validates that scopes have valid resource IDs and non-empty names (for updates).
    /// </summary>

    internal class ApiScopesCheck : ISpecification<ApiResourcesModel>
    {
        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(ApiResourcesModel model)
        {
            if (model.ApiScopes.ContainsAny())
                foreach (var apiScope in model.ApiScopes)
                {
                    if (!apiScope.ApiResourceId.IsValid()) return false;

                    if (string.IsNullOrWhiteSpace(apiScope.Name)) return false;
                }

            return true;
        }
    }

    /// <summary>
    /// Validates that scope claims have valid scope IDs and non-empty types (for updates).
    /// </summary>

    internal class ApiScopeClaimsCheck : ISpecification<ApiResourcesModel>
    {
        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(ApiResourcesModel model)
        {
            if (model.ApiScopes.ContainsAny())
                foreach (var apiScope in model.ApiScopes)
                    if (apiScope.ApiScopeClaims.ContainsAny())
                        foreach (var apiScopeClaims in apiScope.ApiScopeClaims)
                        {
                            if (!apiScopeClaims.ApiScopeId.IsValid()) return false;

                            if (string.IsNullOrWhiteSpace(apiScopeClaims.Type)) return false;
                        }

            return true;
        }
    }

    /// <summary>
    /// Checks that no active API resource with the same name already exists.
    /// </summary>

    internal class ApiResourceDuplicateCheck : ISpecification<ApiResourcesModel>
    {
        private readonly IApiResourceRepository apiResourceRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResourceDuplicateCheck"/> class.
        /// </summary>
        internal ApiResourceDuplicateCheck(IApiResourceRepository apiResourceRepository)
        {
            this.apiResourceRepository = apiResourceRepository;
        }

        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(ApiResourcesModel model)
        {
            var duplicateExists = apiResourceRepository
                .DuplicateExistsAsync(apiResource => apiResource.Name == model.Name).GetAwaiter().GetResult();
            if (duplicateExists) return false;

            return true;
        }
    }
}
