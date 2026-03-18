/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Models.Endpoint.Response;

namespace HCL.CS.SF.Domain.Models.Endpoint.Validation;

/// <summary>
/// Result of validating a token (typically a refresh token) during the token refresh flow.
/// Contains the resolved refresh token record and the associated client if validation succeeds.
/// </summary>
public class TokenValidationModel : ErrorResponseModel
{
    /// <summary>The persisted refresh token record retrieved from the token store.</summary>
    public SecurityTokensModel RefreshToken { get; set; }

    /// <summary>The client that originally obtained the refresh token.</summary>
    public ClientsModel Client { get; set; }
}
