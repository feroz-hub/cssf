/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using DomainValidation.Interfaces.Specification;
using DomainValidation.Validation;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.Service.Implementation.Api.Validators;

namespace HCL.CS.SF.Service.Implementation.Api.Specifications;

/// <summary>
/// Validates individual role claim models, ensuring role ID is valid, claim type and value
/// are present, and that claim values reference existing API scopes (except capabilities claims).
/// </summary>

internal sealed class RoleClaimModelSpecification : BaseDomainModelValidator<RoleClaimModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RoleClaimModelSpecification"/> class.
    /// </summary>
    internal RoleClaimModelSpecification(IRepository<ApiScopes> apiScopeRepository)
    {
        Add("CheckRoleClaimModel", new Rule<RoleClaimModel>(
            new IsNotNull<RoleClaimModel>(model => model),
            ApiErrorCodes.InvalidRoleClaim));
        Add("CheckRoleId", new Rule<RoleClaimModel>(
            new IsValidIdentifier<RoleClaimModel>(model => model.RoleId),
            ApiErrorCodes.InvalidRoleId));
        Add("CheckValidClaimType", new Rule<RoleClaimModel>(
            new IsNotNull<RoleClaimModel>(model => model.ClaimType),
            ApiErrorCodes.RoleClaimTypeRequired));
        Add("CheckValidClaimValue", new Rule<RoleClaimModel>(
            new IsNotNull<RoleClaimModel>(model => model.ClaimValue),
            ApiErrorCodes.RoleClaimValueRequired));
        Add("CheckRoleClaimCreatedByLength", new Rule<RoleClaimModel>(
            new IsValid255CharLength<RoleClaimModel>(model => model.CreatedBy),
            ApiErrorCodes.CreatedByTooLong));

        Add("CheckValidRoleClaim", new Rule<RoleClaimModel>(
            new CheckValidRoleClaim(apiScopeRepository),
            EndpointErrorCodes.InvalidScopeClaims));
    }

    /// <summary>
    /// Validates that the claim type is not empty.
    /// </summary>

    internal class RoleClaimsTypeCheck : ISpecification<RoleClaimModel>
    {
        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(RoleClaimModel model)
        {
            if (string.IsNullOrWhiteSpace(model.ClaimType)) return false;

            return true;
        }
    }

    /// <summary>
    /// Validates that the claim value is not empty.
    /// </summary>

    internal class RoleClaimsValueCheck : ISpecification<RoleClaimModel>
    {
        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(RoleClaimModel model)
        {
            if (string.IsNullOrWhiteSpace(model.ClaimValue)) return false;

            return true;
        }
    }

    /// <summary>
    /// Validates that the role claim value references an existing API scope, unless it is
    /// a capabilities claim (which uses free-form values like "health:read").
    /// </summary>

    private class CheckValidRoleClaim : ISpecification<RoleClaimModel>
    {
        private readonly IRepository<ApiScopes> apiScopeRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckValidRoleClaim"/> class.
        /// </summary>
        internal CheckValidRoleClaim(IRepository<ApiScopes> apiScopeRepository)
        {
            this.apiScopeRepository = apiScopeRepository;
        }

        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="roleClaimModel">The role claim model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(RoleClaimModel? roleClaimModel)
        {
            if (roleClaimModel == null) return true;
            // Capabilities claim values are free-form (e.g. "health:read"); they are not ApiScope names.
            if (string.Equals(roleClaimModel.ClaimType, OpenIdConstants.ClaimTypes.Capabilities, StringComparison.OrdinalIgnoreCase))
                return true;
            var commonScopes = apiScopeRepository.GetAllAsync().GetAwaiter().GetResult().Select(x => x.Name.Trim());

            List<string> allowedScopes =
            [
                roleClaimModel.ClaimValue
            ];
            if (!allowedScopes.Count.Equals(allowedScopes.Distinct().Count())) return false;
            return !allowedScopes.Except(commonScopes.ToList()).Any();
        }
    }
}
