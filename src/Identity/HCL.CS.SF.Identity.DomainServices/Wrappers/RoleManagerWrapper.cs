/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace HCL.CS.SF.DomainServices.Wrappers;

/// <summary>
/// Custom wrapper around ASP.NET Core Identity's <see cref="RoleManager{TRole}"/>.
/// Provides an extension point for role-management operations such as custom validation
/// or audit logging beyond what the default <see cref="RoleManager{TRole}"/> offers.
/// </summary>
/// <typeparam name="TRole">The type representing a role in the identity system.</typeparam>
public class RoleManagerWrapper<TRole> : RoleManager<TRole>
    where TRole : class
{
    /// <summary>
    /// Initializes a new instance of <see cref="RoleManagerWrapper{TRole}"/> with all required Identity dependencies.
    /// </summary>
    public RoleManagerWrapper(
        IRoleStore<TRole> store,
        IEnumerable<IRoleValidator<TRole>> roleValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        ILogger<RoleManagerWrapper<TRole>> logger)
        : base(store, roleValidators, keyNormalizer, errors, logger)
    {
    }
}
