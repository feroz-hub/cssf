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
/// Validates API scope models for create/update operations. API scopes define the permissions
/// that OAuth2 clients can request; this ensures scope names are unique, required fields are
/// present, and nested scope claims are properly structured.
/// </summary>

internal sealed class ApiScopeModelSpecification : BaseDomainModelValidator<ApiScopesModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiScopeModelSpecification"/> class.
    /// </summary>
    internal ApiScopeModelSpecification(CrudMode crudMode, IRepository<ApiScopes> apiScopeRepository)
    {
        Add("CheckValidApiResourceId", new Rule<ApiScopesModel>(
            new IsValidIdentifier<ApiScopesModel>(model => model.ApiResourceId),
            ApiErrorCodes.ApiResourceIdInvalid));

        Add("CheckValidApiScopeName", new Rule<ApiScopesModel>(
            new IsNotNull<ApiScopesModel>(model => model.Name),
            ApiErrorCodes.ApiScopeNameRequired));

        Add("CheckValidApiScopeNameLength", new Rule<ApiScopesModel>(
            new IsValid255CharLength<ApiScopesModel>(model => model.Name),
            ApiErrorCodes.ApiScopeNameTooLong));

        Add("CheckValidApiScopeDisplayNameLength", new Rule<ApiScopesModel>(
            new IsValid255CharLength<ApiScopesModel>(model => model.DisplayName),
            ApiErrorCodes.ApiScopeDisplayNameTooLong));

        Add("CheckValidApiScopeCreatedByLength", new Rule<ApiScopesModel>(
            new IsValid255CharLength<ApiScopesModel>(model => model.CreatedBy),
            ApiErrorCodes.CreatedByTooLong));

        switch (crudMode)
        {
            case CrudMode.Add:
                AddRules(apiScopeRepository);
                break;
            case CrudMode.Update:
                UpdateRules(apiScopeRepository);
                break;
        }
    }

    private void AddRules(IRepository<ApiScopes> apiScopeRepository)
    {
        // Api scope claim
        Add("CheckValidApiScopeClaimType", new Rule<ApiScopesModel>(
            new ApiScopeClaimsTypeCheck(),
            ApiErrorCodes.ApiScopeClaimTypeRequired));
        Add("CheckValidApiScopeClaimTypeAndCreatedbyLength", new Rule<ApiScopesModel>(
            new ApiScopeClaimsTypeLengthCheck(),
            ApiErrorCodes.ApiScopeClaimTypeOrCreatedbyTooLong));
        Add("CheckDuplicateApiRecourceEntry", new Rule<ApiScopesModel>(
            new ApiScopeDuplicateCheck(apiScopeRepository),
            ApiErrorCodes.ApiScopeAlreadyExists));
    }

    private void UpdateRules(IRepository<ApiScopes> apiScopeRepository)
    {
        // Api scope
        Add("CheckValidApiScopeId", new Rule<ApiScopesModel>(
            new IsValidIdentifier<ApiScopesModel>(model => model.Id),
            ApiErrorCodes.ApiScopeIdInvalid));
        Add("CheckValidApiScopeModifiedByLength", new Rule<ApiScopesModel>(
            new IsValid255CharLength<ApiScopesModel>(model => model.ModifiedBy),
            ApiErrorCodes.ModifiedByTooLong));

        // Api scope claim
        Add("CheckValidApiScopeClaim", new Rule<ApiScopesModel>(
            new ApiScopeClaimsCheck(),
            ApiErrorCodes.InvalidApiScopeClaimTypeOrScopeId));
        Add("CheckValidApiScopeClaimTypeLength", new Rule<ApiScopesModel>(
            new ApiScopeClaimsTypeLengthCheck(),
            ApiErrorCodes.ApiScopeClaimTypeOrCreatedbyTooLong));
        Add("CheckValidApiScopeClaimModifiedByLength", new Rule<ApiScopesModel>(
            new IsValid255CharLengths<ApiScopesModel>(model =>
                model.ApiScopeClaims.ContainsAny() ? model.ApiScopeClaims.ConvertAll(x => x.ModifiedBy) : null),
            ApiErrorCodes.ApiScopeClaimModifiedbyTooLong));
        Add("CheckActiveApiScopeEntry", new Rule<ApiScopesModel>(
            new ApiScopeActiveCheck(apiScopeRepository),
            ApiErrorCodes.ApiScopeNameInvalid));
    }

    /// <summary>
    /// Ensures all nested API scope claims have a non-empty type.
    /// </summary>

    private class ApiScopeClaimsTypeCheck : ISpecification<ApiScopesModel>
    {
        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(ApiScopesModel model)
        {
            if (model.ApiScopeClaims.ContainsAny())
                foreach (var apiScopeClaims in model.ApiScopeClaims)
                    if (string.IsNullOrWhiteSpace(apiScopeClaims.Type))
                        return false;

            return true;
        }
    }

    /// <summary>
    /// Validates that scope claim types and createdBy fields do not exceed 255 characters.
    /// </summary>

    private class ApiScopeClaimsTypeLengthCheck : ISpecification<ApiScopesModel>
    {
        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(ApiScopesModel model)
        {
            if (model.ApiScopeClaims.ContainsAny())
                foreach (var apiScopeClaims in model.ApiScopeClaims)
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
    /// Validates that nested scope claims have valid scope IDs and non-empty types (for updates).
    /// </summary>

    private class ApiScopeClaimsCheck : ISpecification<ApiScopesModel>
    {
        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(ApiScopesModel model)
        {
            if (model.ApiScopeClaims.ContainsAny())
                foreach (var apiScopeClaims in model.ApiScopeClaims)
                {
                    if (!apiScopeClaims.ApiScopeId.IsValid()) return false;

                    if (string.IsNullOrWhiteSpace(apiScopeClaims.Type)) return false;
                }

            return true;
        }
    }

    /// <summary>
    /// Checks that no active API scope with the same name already exists.
    /// </summary>

    private class ApiScopeDuplicateCheck : ISpecification<ApiScopesModel>
    {
        private readonly IRepository<ApiScopes> apiScopeRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiScopeDuplicateCheck"/> class.
        /// </summary>
        internal ApiScopeDuplicateCheck(IRepository<ApiScopes> apiScopeRepository)
        {
            this.apiScopeRepository = apiScopeRepository;
        }

        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(ApiScopesModel model)
        {
            var duplicateExists = apiScopeRepository.DuplicateExistsAsync(apiScope => apiScope.Name == model.Name)
                .GetAwaiter().GetResult();
            if (duplicateExists) return false;

            return true;
        }
    }

    /// <summary>
    /// Validates that the API scope being updated still exists as an active record.
    /// </summary>

    private class ApiScopeActiveCheck : ISpecification<ApiScopesModel>
    {
        private readonly IRepository<ApiScopes> apiScopeRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiScopeActiveCheck"/> class.
        /// </summary>
        internal ApiScopeActiveCheck(IRepository<ApiScopes> apiScopeRepository)
        {
            this.apiScopeRepository = apiScopeRepository;
        }

        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(ApiScopesModel model)
        {
            var isResourceExists = apiScopeRepository.ActiveRecordExistsAsync(apiscope => apiscope.Id == model.Id)
                .GetAwaiter().GetResult();
            if (isResourceExists) return true;

            return false;
        }
    }
}
