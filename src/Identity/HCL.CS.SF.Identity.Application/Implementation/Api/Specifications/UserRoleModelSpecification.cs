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
using HCL.CS.SF.DomainServices.Wrappers;
using HCL.CS.SF.Service.Implementation.Api.Validators;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;

namespace HCL.CS.SF.Service.Implementation.Api.Specifications;

/// <summary>
/// Validates user-role assignment models, ensuring both the user and role exist
/// and that identifiers and audit fields are valid before allowing role assignment.
/// </summary>

internal sealed class UserRoleModelSpecification : BaseDomainModelValidator<UserRoleModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserRoleModelSpecification"/> class.
    /// </summary>
    internal UserRoleModelSpecification(CrudMode crudMode, UserManagerWrapper<Users> userManager,
        IRoleService roleService)
    {
        Add("CheckValidUserId", new Rule<UserRoleModel>(
            new IsValidIdentifier<UserRoleModel>(model => model.UserId),
            ApiErrorCodes.InvalidUserId));
        Add("CheckValidRoleId", new Rule<UserRoleModel>(
            new IsValidIdentifier<UserRoleModel>(model => model.RoleId),
            ApiErrorCodes.InvalidRoleId));
        Add("CheckCreatedByLength", new Rule<UserRoleModel>(
            new IsValid255CharLength<UserRoleModel>(model => model.CreatedBy),
            ApiErrorCodes.CreatedByTooLong));

        Add("CheckUserExist", new Rule<UserRoleModel>(
            new CheckUserExist(userManager),
            ApiErrorCodes.InvalidUserId));

        Add("CheckRoleExist", new Rule<UserRoleModel>(
            new CheckRoleExist(roleService),
            ApiErrorCodes.InvalidRoleId));

        if (crudMode == CrudMode.Update)
            Add("CheckModifiedByLength", new Rule<UserRoleModel>(
                new IsValid255CharLength<UserRoleModel>(model => model.ModifiedBy),
                ApiErrorCodes.ModifiedByTooLong));
    }

    /// <summary>
    /// Validates that the referenced user exists in the identity store.
    /// </summary>

    private class CheckUserExist : ISpecification<UserRoleModel>
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
        public bool IsSatisfiedBy(UserRoleModel model)
        {
            var userExist = userManager.FindByIdAsync(model.UserId.ToString()).GetAwaiter().GetResult();
            return userExist != null;
        }
    }

    /// <summary>
    /// Validates that the referenced role exists via the role service.
    /// </summary>

    private class CheckRoleExist : ISpecification<UserRoleModel>
    {
        private readonly IRoleService roleService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckRoleExist"/> class.
        /// </summary>
        internal CheckRoleExist(IRoleService roleService)
        {
            this.roleService = roleService;
        }

        /// <summary>
        /// Determines whether satisfied b.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
        public bool IsSatisfiedBy(UserRoleModel model)
        {
            var roleExist = roleService.GetRoleAsync(model.RoleId).GetAwaiter().GetResult();
            return roleExist != null;
        }
    }
}
