/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Enums;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Api;

/// <summary>
/// Partial interface extending <see cref="IUserAccountService"/> with two-factor authentication
/// configuration. Supports enabling/disabling 2FA, switching the 2FA method (email, SMS,
/// authenticator app), and listing available 2FA types.
/// </summary>
public partial interface IUserAccountService
{
    /// <summary>Enables or disables two-factor authentication for the specified user.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="enabled"><c>true</c> to enable 2FA; <c>false</c> to disable.</param>
    Task<FrameworkResult> SetTwoFactorEnabledAsync(Guid userId, bool enabled);

    /// <summary>Changes the user's two-factor authentication method.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="twoFactorType">The desired two-factor type (Email, SMS, AuthenticatorApp).</param>
    Task<FrameworkResult> UpdateUserTwoFactorTypeAsync(Guid userId, TwoFactorType twoFactorType);

    /// <summary>Retrieves all available two-factor authentication type names.</summary>
    Task<IList<string>> GetAllTwoFactorTypeAsync();
}
