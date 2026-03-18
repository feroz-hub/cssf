/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Models.Api.Response;
using HCL.CS.SF.Domain.Models.Endpoint.Validation;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Api;

/// <summary>
/// Authentication service providing sign-in, sign-out, and multi-factor authentication
/// capabilities. Supports password-based authentication, email/SMS/authenticator 2FA,
/// recovery codes, authenticator app setup, and resource-owner password (ROP) credential validation.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>Authenticates a user with username and password.</summary>
    /// <param name="username">The username.</param>
    /// <param name="password">The user's password.</param>
    Task<SignInResponseModel> PasswordSignInAsync(string username, string password);

    /// <summary>Authenticates a user with username, password, and a TOTP authenticator token (combined 2FA).</summary>
    /// <param name="username">The username.</param>
    /// <param name="password">The user's password.</param>
    /// <param name="twoFactorAuthenticatorToken">The TOTP token from the authenticator app.</param>
    Task<SignInResponseModel> PasswordSignInAsync(string username, string password, string twoFactorAuthenticatorToken);

    /// <summary>Completes two-factor authentication using an email-delivered code.</summary>
    /// <param name="code">The two-factor code received via email.</param>
    Task<SignInResponseModel> TwoFactorEmailSignInAsync(string code);

    /// <summary>Completes two-factor authentication using an SMS-delivered code.</summary>
    /// <param name="code">The two-factor code received via SMS.</param>
    Task<SignInResponseModel> TwoFactorSmsSignInAsync(string code);

    /// <summary>Completes two-factor authentication using a TOTP code from an authenticator app.</summary>
    /// <param name="code">The TOTP code from the authenticator app.</param>
    Task<SignInResponseModel> TwoFactorAuthenticatorAppSignInAsync(string code);

    /// <summary>Completes two-factor authentication using a recovery code.</summary>
    /// <param name="recoveryCode">The recovery code to consume.</param>
    Task<SignInResponseModel> TwoFactorRecoveryCodeSignInAsync(string recoveryCode);

    /// <summary>Signs out the currently authenticated user and clears the session.</summary>
    Task<FrameworkResult> SignOutAsync();

    /// <summary>Checks whether the specified claims principal represents a signed-in user.</summary>
    /// <param name="principal">The claims principal to check.</param>
    Task<bool> IsUserSignedInAsync(ClaimsPrincipal principal);

    /// <summary>Generates the authenticator app setup information (QR code, manual key) for a user.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="applicationName">The application name displayed in the authenticator app.</param>
    Task<AuthenticatorAppSetupResponseModel> SetupAuthenticatorAppAsync(Guid userId, string applicationName);

    /// <summary>Verifies the authenticator app setup by validating a TOTP token.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="token">The TOTP token to verify.</param>
    Task<AuthenticatorAppResponseModel> VerifyAuthenticatorAppSetupAsync(Guid userId, string token);

    /// <summary>Resets (removes) the authenticator app configuration for a user.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    Task<FrameworkResult> ResetAuthenticatorAppAsync(Guid userId);

    /// <summary>Generates a new set of recovery codes for the user, replacing any existing ones.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    Task<IEnumerable<string>> GenerateRecoveryCodesAsync(Guid userId);

    /// <summary>Returns the number of remaining unused recovery codes for the user.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    Task<int> CountRecoveryCodesAsync(Guid userId);
    // ! @cond

    /// <summary>Validates resource-owner password (ROP) credentials for the OAuth token endpoint.</summary>
    /// <param name="validationModel">The ROP validation model containing username and password.</param>
    Task<RopValidationModel> RopValidateCredentialsAsync(RopValidationModel validationModel);

    // ! @endcond
}
