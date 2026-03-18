/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Api.Response;

/// <summary>
/// Response model for authenticator app (TOTP) enrollment and verification operations.
/// Returns the result of enabling/verifying an authenticator app along with recovery codes.
/// </summary>
public class AuthenticatorAppResponseModel
{
    /// <summary>Indicates whether the authenticator app operation succeeded.</summary>
    public bool Succeeded { get; set; } = false;

    /// <summary>A human-readable message describing the result of the operation.</summary>
    public string Message { get; set; }

    /// <summary>One-time recovery codes generated when the authenticator app is first enabled, for account recovery.</summary>
    public IEnumerable<string> RecoveryCodes { get; set; }
}
