/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using HCL.CS.SF.Domain.Entities.Api;

namespace HCL.CS.SF.Infrastructure.Data.Wrappers;

/// <summary>
/// Custom wrapper around <see cref="RoleStore{TRole, TContext, TKey, TUserRole, TRoleClaim}"/>
/// that disables AutoSaveChanges by default. This allows role operations to participate in
/// unit-of-work transactions rather than saving immediately on each Identity operation.
/// </summary>
public class RoleStoreWrapper : RoleStore<Roles, ApplicationDbContext, Guid, UserRoles, RoleClaims>
{
    /// <summary>
    /// Initializes a new instance of <see cref="RoleStoreWrapper"/> with AutoSaveChanges disabled.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public RoleStoreWrapper(ApplicationDbContext context) : base(context)
    {
        AutoSaveChanges = false;
    }
}
