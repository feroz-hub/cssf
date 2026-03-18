/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Identity;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Api.Wrapper;

/// <summary>
/// Abstraction over the sign-in manager for non-cookie-based (API) authentication scenarios.
/// Allows password validation and two-factor verification without creating an ASP.NET
/// authentication cookie, making it suitable for REST API endpoints.
/// </summary>
/// <typeparam name="TUser">The type representing a user in the identity system.</typeparam>
public interface IWindowsSignInManagerWrapper<TUser>
    where TUser : class
{
    /// <summary>Validates the user's password without issuing an authentication cookie.</summary>
    /// <param name="user">The user entity to authenticate.</param>
    /// <param name="password">The password to verify.</param>
    /// <param name="lockoutOnFailure">Whether to increment the lockout counter on failure.</param>
    Task<SignInResult> PasswordSignInAsync(TUser user, string password, bool lockoutOnFailure);

    /// <summary>Validates a two-factor code from the specified provider (email or SMS).</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="provider">The two-factor provider name (e.g., "Email", "Phone").</param>
    /// <param name="code">The two-factor code to verify.</param>
    Task<SignInResult> TwoFactorSignInAsync(Guid userId, string provider, string code);

    /// <summary>Validates a TOTP code from an authenticator app.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="code">The authenticator app code to verify.</param>
    Task<SignInResult> TwoFactorAuthenticatorSignInAsync(Guid userId, string code);

    /// <summary>Validates a recovery code for two-factor authentication bypass.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="recoveryCode">The recovery code to verify and consume.</param>
    Task<SignInResult> TwoFactorRecoveryCodeSignInAsync(Guid userId, string recoveryCode);
}
