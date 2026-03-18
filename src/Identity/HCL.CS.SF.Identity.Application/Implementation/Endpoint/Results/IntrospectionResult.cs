/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Http;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Response;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Results;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Results;

/// <summary>
/// Endpoint result that renders the OAuth 2.0 Token Introspection response (RFC 7662) as JSON.
/// </summary>
internal class IntrospectionResult : IEndpointResult
{
    /// <summary>Initializes a new introspection result with the specified response model.</summary>
    /// <param name="response">The introspection response containing active status and token metadata.</param>
    public IntrospectionResult(IntrospectionResponseModel response)
    {
        Response = response;
    }

    /// <summary>Gets the introspection response model.</summary>
    internal IntrospectionResponseModel Response { get; }

    /// <summary>
    /// Writes the introspection response as JSON with HTTP 200 status.
    /// </summary>
    /// <param name="context">The HTTP context to write the response to.</param>
    public async Task ConstructResponseAsync(HttpContext context)
    {
        context.Response.SetResponseNoCache();
        context.Response.StatusCode = OpenIdConstants.HTTPStatusCodes.success;
        if (Response != null) await context.Response.WriteResponseJsonAsync(Response);
    }
}
