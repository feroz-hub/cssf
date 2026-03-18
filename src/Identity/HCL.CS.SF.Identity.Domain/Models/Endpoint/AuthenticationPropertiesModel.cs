/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using HCL.CS.SF.Domain.Models.Endpoint.Response;

namespace HCL.CS.SF.Domain.Models.Endpoint;

/// <summary>
/// Encapsulates the result of authenticating a user, including the claims principal
/// and ASP.NET Core authentication properties (e.g., persistence, expiration).
/// Extends <see cref="ErrorResponseModel"/> to carry error state when authentication fails.
/// </summary>
public class AuthenticationPropertiesModel : ErrorResponseModel
{
    /// <summary>The authenticated user's claims principal containing identity and authorization claims.</summary>
    public ClaimsPrincipal Principal { get; set; }

    /// <summary>The ASP.NET Core authentication properties such as token persistence and expiration settings.</summary>
    public AuthenticationProperties Properties { get; set; }
}
