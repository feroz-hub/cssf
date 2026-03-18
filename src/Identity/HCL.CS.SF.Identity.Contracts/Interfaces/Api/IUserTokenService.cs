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
/// Partial interface extending <see cref="IUserAccountService"/> with token generation and
/// verification operations for email confirmation, phone confirmation, password reset,
/// general-purpose user tokens, and two-factor authentication codes (email and SMS).
/// </summary>
public partial interface IUserAccountService
{
    /// <summary>Generates and sends an email confirmation token to the user.</summary>
    /// <param name="username">The username of the account.</param>
    Task<FrameworkResult> GenerateEmailConfirmationTokenAsync(string username);

    /// <summary>Verifies an email confirmation token for the user.</summary>
    /// <param name="username">The username of the account.</param>
    /// <param name="emailToken">The email confirmation token to verify.</param>
    Task<FrameworkResult> VerifyEmailConfirmationTokenAsync(string username, string emailToken);

    /// <summary>Generates and sends a phone number confirmation token via SMS.</summary>
    /// <param name="username">The username of the account.</param>
    Task<FrameworkResult> GeneratePhoneNumberConfirmationTokenAsync(string username);

    /// <summary>Verifies a phone number confirmation token for the user.</summary>
    /// <param name="username">The username of the account.</param>
    /// <param name="smsToken">The SMS confirmation token to verify.</param>
    Task<FrameworkResult> VerifyPhoneNumberConfirmationTokenAsync(string username, string smsToken);

    /// <summary>Generates and sends a password reset token via the specified notification channel.</summary>
    /// <param name="username">The username of the account.</param>
    /// <param name="notificationType">The notification channel (email or SMS).</param>
    Task<FrameworkResult> GeneratePasswordResetTokenAsync(string username,
        NotificationTypes notificationType = NotificationTypes.Email);

    /// <summary>Generates a general-purpose user token for custom verification flows.</summary>
    /// <param name="username">The username of the account.</param>
    /// <param name="purpose">The purpose of the token (e.g., "AccountUnlock").</param>
    /// <param name="templateName">The notification template name to use.</param>
    /// <param name="notificationType">The notification channel (email or SMS).</param>
    Task<FrameworkResult> GenerateUserTokenAsync(string username, string purpose, string templateName,
        NotificationTypes notificationType = NotificationTypes.Email);

    /// <summary>Verifies a general-purpose user token.</summary>
    /// <param name="username">The username of the account.</param>
    /// <param name="purpose">The purpose the token was generated for.</param>
    /// <param name="token">The token value to verify.</param>
    Task<FrameworkResult> VerifyUserTokenAsync(string username, string purpose, string token);

    /// <summary>Generates and sends a two-factor authentication code via email.</summary>
    /// <param name="username">The username of the account.</param>
    Task<FrameworkResult> GenerateEmailTwoFactorTokenAsync(string username);

    /// <summary>Verifies an email-based two-factor authentication code.</summary>
    /// <param name="username">The username of the account.</param>
    /// <param name="emailToken">The two-factor email code to verify.</param>
    Task<FrameworkResult> VerifyEmailTwoFactorTokenAsync(string username, string emailToken);

    /// <summary>Generates and sends a two-factor authentication code via SMS.</summary>
    /// <param name="username">The username of the account.</param>
    Task<FrameworkResult> GenerateSmsTwoFactorTokenAsync(string username);

    /// <summary>Verifies an SMS-based two-factor authentication code.</summary>
    /// <param name="username">The username of the account.</param>
    /// <param name="smsToken">The two-factor SMS code to verify.</param>
    Task<FrameworkResult> VerifySmsTwoFactorTokenAsync(string username, string smsToken);
}
