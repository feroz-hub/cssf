/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HCL.CS.SF.DomainServices.Wrappers;

/// <summary>
/// Custom wrapper around ASP.NET Core Identity's <see cref="SignInManager{TUser}"/>.
/// Allows the framework to inject its own <see cref="UserManagerWrapper{TUser}"/> and provides
/// an extension point for overriding or augmenting sign-in behavior (e.g., audit logging, custom 2FA flows).
/// </summary>
/// <typeparam name="TUser">The type representing a user in the identity system.</typeparam>
public class SignInManagerWrapper<TUser> : SignInManager<TUser>
    where TUser : class
{
    /// <summary>
    /// Initializes a new instance of <see cref="SignInManagerWrapper{TUser}"/> with all required Identity dependencies.
    /// </summary>
    public SignInManagerWrapper(
        UserManagerWrapper<TUser> userManager,
        IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<TUser> claimsFactory,
        IOptions<IdentityOptions> optionsAccessor,
        ILogger<SignInManagerWrapper<TUser>> logger,
        IAuthenticationSchemeProvider schemes,
        IUserConfirmation<TUser> confirmation)
        : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
    }
}
