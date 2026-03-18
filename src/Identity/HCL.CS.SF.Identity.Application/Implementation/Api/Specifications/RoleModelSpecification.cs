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
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.Service.Extension;
using HCL.CS.SF.Service.Implementation.Api.Validators;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;

namespace HCL.CS.SF.Service.Implementation.Api.Specifications;

/// <summary>
/// Validates RBAC role models for create/update operations. Ensures role names are present,
/// role claims reference valid API scopes (except for capabilities claims which are free-form),
/// and field lengths are within database limits.
/// </summary>

internal sealed class RoleModelSpecification : BaseDomainModelValidator<RoleModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RoleModelSpecification"/> class.
    /// </summary>
    internal RoleModelSpecification(CrudMode crudMode, IRepository<ApiScopes> apiScopeRepository)
    {
        // Role
        Add("CheckRoleNameExists", new Rule<RoleModel>(
            new IsNotNull<RoleModel>(model => model.Name),
            ApiErrorCodes.InvalidRoleName));
        Add("CheckRoleNameLength", new Rule<RoleModel>(
            new IsValid255CharLength<RoleModel>(model => model.Name),
            ApiErrorCodes.RoleNameTooLong));
        Add("CheckRoleCreatedByLength", new Rule<RoleModel>(
            new IsValid255CharLength<RoleModel>(model => model.CreatedBy),
            ApiErrorCodes.CreatedByTooLong));

        // Role Claim
        Add("RoleClaimsCheck", new Rule<RoleModel>(
            new RoleClaimsCheck(),
            ApiErrorCodes.InvalidRoleClaimValueOrClaimType));
        Add("CheckValidRoleClaimCreatedBy", new Rule<RoleModel>(
            new IsValid255CharLengths<RoleModel>(model =>
                model.RoleClaims.ContainsAny() ? model.RoleClaims.ConvertAll(x => x.CreatedBy) : null),
            ApiErrorCodes.RoleClaimCreatedByTooLong));

        Add("CheckValidRoleClaim", new Rule<RoleModel>(
            new CheckValidRoleClaim(apiScopeRepository),
            EndpointErrorCodes.InvalidScopeClaims));

        if (crudMode == CrudMode.Update) UpdateRules();
    }

    private void UpdateRules()
    {
        Add("CheckValidRoleId", new Rule<RoleModel>(
            new IsValidIdentifier<RoleModel>(model => model.Id),
            ApiErrorCodes.InvalidRoleId));
        Add("CheckRoleModifiedByLength", new Rule<RoleModel>(
            new IsValid255CharLength<RoleModel>(model => model.ModifiedBy),
            ApiErrorCodes.ModifiedByTooLong));

        Add("CheckValidRoleClaimRoleId", new Rule<RoleModel>(
            new RoleClaimsRoleIdCheck(),
            ApiErrorCodes.InvalidRoleClaimRoleId));
        Add("CheckValidRoleClaimModifiedBy", new Rule<RoleModel>(
            new IsValid255CharLengths<RoleModel>(model =>
                model.RoleClaims.ContainsAny() ? model.RoleClaims.ConvertAll(x => x.ModifiedBy) : null),
            ApiErrorCodes.RoleClaimModifiedByTooLong));
    }

    /// <summary>
    /// Validates that all role claims have non-empty claim type and claim value.
    /// </summary>

    private class RoleClaimsCheck : ISpecification<RoleModel>
    {
        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(RoleModel model)
        {
            if (!model.RoleClaims.ContainsAny()) return true;
            foreach (var roleClaims in model.RoleClaims)
            {
                if (string.IsNullOrWhiteSpace(roleClaims.ClaimType)) return false;

                if (string.IsNullOrWhiteSpace(roleClaims.ClaimValue)) return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Validates that all role claims have a valid role ID (for updates).
    /// </summary>

    private class RoleClaimsRoleIdCheck : ISpecification<RoleModel>
    {
        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(RoleModel model)
        {
            return !model.RoleClaims.ContainsAny() || model.RoleClaims.All(claim => claim.RoleId.IsValid());
        }
    }

    /// <summary>
    /// Validates that role claim values reference existing API scopes (except capabilities claims
    /// which are free-form) and that there are no duplicate claim values.
    /// </summary>

    private class CheckValidRoleClaim : ISpecification<RoleModel>
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
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(RoleModel model)
        {
            if (!model.RoleClaims.ContainsAny()) return true;
            // Only validate ClaimValues that are scope names; capabilities claim values are free-form.
            var scopeValueClaims = model.RoleClaims
                .Where(rc => !string.Equals(rc.ClaimType, OpenIdConstants.ClaimTypes.Capabilities, StringComparison.OrdinalIgnoreCase))
                .Select(rc => rc.ClaimValue)
                .ToList();
            if (scopeValueClaims.Count == 0) return true;
            var commonScopes = apiScopeRepository.GetAllAsync().GetAwaiter().GetResult().Select(x => x.Name.Trim()).ToList();
            if (!scopeValueClaims.Count.Equals(scopeValueClaims.Distinct().Count())) return false;
            return !scopeValueClaims.Except(commonScopes).Any();
        }
    }
}
