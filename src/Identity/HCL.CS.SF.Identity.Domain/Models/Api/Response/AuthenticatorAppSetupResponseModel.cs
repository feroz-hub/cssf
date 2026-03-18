/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Api.Response;

/// <summary>
/// Response model containing the data needed to set up a TOTP authenticator app.
/// The client displays the shared key or QR code (from the URI) for the user to scan.
/// </summary>
public class AuthenticatorAppSetupResponseModel
{
    /// <summary>The base32-encoded shared secret key for manual entry into the authenticator app.</summary>
    public string SharedKey { get; set; }

    /// <summary>The otpauth:// URI for generating a QR code that the authenticator app can scan.</summary>
    public string AuthenticatorUri { get; set; }

    /// <summary>A verification code used to confirm the authenticator app is configured correctly.</summary>
    public string VerificationCode { get; set; }
}
