/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Http;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Api.Wrapper;

/// <summary>
/// API gateway that intercepts and routes incoming HTTP requests to the appropriate
/// internal endpoint handlers. Implementations validate the request, apply middleware
/// logic (e.g., rate limiting, token validation), and delegate to the endpoint router.
/// </summary>
public interface IApiGateway
{
    /// <summary>
    /// Processes an incoming HTTP request through the gateway pipeline.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <returns><c>true</c> if the request was handled by the gateway; otherwise <c>false</c>.</returns>
    Task<bool> ProcessRequest(HttpContext httpContext);
}
