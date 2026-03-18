/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Enums;

namespace HCL.CS.SF.Domain.Models.Api.Response;

/// <summary>
/// Response model for user sign-in attempts in the identity server.
/// Captures the outcome of authentication including success, lockout, two-factor requirements,
/// and any error messages to drive the login flow.
/// </summary>
public class SignInResponseModel
{
    /// <summary>Indicates whether the sign-in was successful.</summary>
    public bool Succeeded { get; set; } = false;

    /// <summary>Indicates whether the account is locked out due to too many failed attempts.</summary>
    public bool IsLockedOut { get; set; } = false;

    /// <summary>Indicates whether sign-in is not allowed (e.g., email not confirmed).</summary>
    public bool IsNotAllowed { get; set; } = false;

    /// <summary>Indicates whether the user must complete two-factor authentication.</summary>
    public bool RequiresTwoFactor { get; set; } = false;

    /// <summary>The type of two-factor verification required (e.g., Email, SMS, Authenticator).</summary>
    public TwoFactorType TwoFactorVerificationMode { get; set; }

    /// <summary>Indicates whether the two-factor verification code has been sent to the user.</summary>
    public bool TwoFactorVerificationCodeSent { get; set; } = false;

    /// <summary>A human-readable message describing the sign-in result.</summary>
    public string Message { get; set; }

    /// <summary>An error code for programmatic handling of sign-in failures.</summary>
    public string ErrorCode { get; set; }

    /// <summary>The verification code for user identity confirmation (used in email/SMS verification flows).</summary>
    public string UserVerificationCode { get; set; }
}
