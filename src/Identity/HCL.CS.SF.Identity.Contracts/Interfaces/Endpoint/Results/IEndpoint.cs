/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Http;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Results;

/// <summary>
/// Represents a protocol endpoint in the authorization server (e.g., /authorize, /token, /userinfo).
/// Implementations process the incoming HTTP request and return an <see cref="IEndpointResult"/>
/// that writes the appropriate response.
/// </summary>
public interface IEndpoint
{
    /// <summary>
    /// Processes the incoming HTTP request for this endpoint and produces a result.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>An <see cref="IEndpointResult"/> that writes the response when executed.</returns>
    Task<IEndpointResult> ProcessAsync(HttpContext context);
}
