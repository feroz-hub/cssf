/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Models.Endpoint.Validation;

namespace HCL.CS.SF.Domain.Models.Endpoint.Request;

/// <summary>
/// Represents a validated OAuth 2.0 Token Revocation request (RFC 7009).
/// The client submits a token and optional type hint to invalidate an access or refresh token.
/// </summary>
public class ValidatedRevocationRequestModel : ValidatedBaseModel
{
    /// <summary>A hint about the type of token being revoked: "access_token" or "refresh_token".</summary>
    public string TokenTypeHint { get; set; }

    /// <summary>The token value to be revoked.</summary>
    public string Token { get; set; }
}
