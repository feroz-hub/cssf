/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Response;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Results;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Results;

/// <summary>
/// Endpoint result that renders an OAuth 2.0 error response as JSON.
/// Sets appropriate HTTP status codes and WWW-Authenticate headers per the OAuth 2.0 spec.
/// </summary>
public class ErrorResult : IEndpointResult
{
    /// <summary>Initializes a new error result with the specified error response model.</summary>
    /// <param name="errorResponse">The error response containing error code and description.</param>
    public ErrorResult(ErrorResponseModel errorResponse)
    {
        ErrorResponse = errorResponse;
    }

    /// <summary>Gets or sets the error response model containing error code and description.</summary>
    internal ErrorResponseModel ErrorResponse { get; set; }

    /// <summary>
    /// Writes the error response as JSON with appropriate HTTP status code and headers.
    /// Sets WWW-Authenticate header for invalid_client errors per OAuth 2.0 spec.
    /// </summary>
    /// <param name="context">The HTTP context to write the response to.</param>
    public async Task ConstructResponseAsync(HttpContext context)
    {
        context.Response.SetResponseNoCache();

        if (string.Equals(ErrorResponse.ErrorCode, OpenIdConstants.Errors.InvalidClient, StringComparison.Ordinal))
            context.Response.Headers[HeaderNames.WWWAuthenticate] = "Basic realm=\"token\"";

        var error = new ErrorResponseResultModel
        {
            error = ErrorResponse.ErrorCode,
            error_description = ErrorResponse.ErrorDescription
        };
        context.Response.StatusCode = OpenIdConstants.HTTPStatusCodes.GetHttpStatusCode(ErrorResponse.ErrorCode);
        await context.Response.WriteResponseJsonAsync(error);
    }
}
