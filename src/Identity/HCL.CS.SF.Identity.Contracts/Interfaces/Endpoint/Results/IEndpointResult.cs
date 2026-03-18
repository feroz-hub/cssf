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
/// Represents the result of processing an OAuth/OIDC endpoint request. Implementations
/// write the appropriate HTTP response (JSON, redirect, error page) to the <see cref="HttpContext"/>.
/// </summary>
public interface IEndpointResult
{
    /// <summary>
    /// Constructs and writes the HTTP response for this endpoint result.
    /// </summary>
    /// <param name="context">The current HTTP context to write the response to.</param>
    Task ConstructResponseAsync(HttpContext context);
}
