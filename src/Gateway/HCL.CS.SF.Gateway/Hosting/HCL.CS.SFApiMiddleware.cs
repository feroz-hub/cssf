/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Http;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.ProxyService.Routes.Extension;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api.Wrapper;

namespace HCL.CS.SF.ProxyService.Hosting;

/// <summary>
/// HTTP pipeline middleware that intercepts incoming POST requests targeting the
/// security framework API base path (e.g., "/security/api/...") and dispatches them
/// to the <see cref="IApiGateway"/> for routing and processing. Non-API requests
/// and non-POST methods pass through to the next middleware in the pipeline.
/// On error, returns a standardized JSON error response with HTTP 400.
/// </summary>
public class HCLCSSFApiMiddleware
{
    /// <summary>
    /// Logger service for recording errors during API request processing.
    /// </summary>
    private readonly ILoggerService loggerService;

    /// <summary>
    /// The next middleware delegate in the HTTP pipeline.
    /// </summary>
    private readonly RequestDelegate next;

    /// <summary>
    /// Initializes a new instance of the <see cref="HCLCSSFApiMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="instance">Logger instance factory for obtaining the default logger.</param>
    public HCLCSSFApiMiddleware(
        RequestDelegate next,
        ILoggerInstance instance)
    {
        this.next = next;
        loggerService = instance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
    }

    /// <summary>
    /// Processes the HTTP request. If the request is a POST to the API base path,
    /// it is routed through the API gateway; otherwise, the request continues down the pipeline.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <param name="apiRoutewrapper">The API gateway responsible for dispatching API requests.</param>
    public async Task InvokeAsync(HttpContext httpContext, IApiGateway apiRoutewrapper)
    {
        try
        {
            if (HttpMethods.IsPost(httpContext.Request.Method))
            {
                var path = httpContext.Request.Path != null ? httpContext.Request.Path.Value : null;
                // Only intercept requests whose path starts with the security API base path.
                if (!string.IsNullOrWhiteSpace(path) &&
                    path.StartsWith(ApiRoutePathConstants.BasePath, StringComparison.OrdinalIgnoreCase))
                {
                    await apiRoutewrapper.ProcessRequest(httpContext);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            // Unwrap nested exceptions to find the most specific error description.
            var errorDescription = ResolveApiErrorDescription(ex);
            httpContext.Response.StatusCode = OpenIdConstants.HTTPStatusCodes.bad_request;
            await httpContext.Response.WriteResponseJsonAsync(new
            {
                error = OpenIdConstants.Errors.ServerError,
                error_description = errorDescription
            });
            loggerService.WriteToWithCaller(Log.Error, ex, "Exception while processing API middleware request.");
            return;
        }

        await next(httpContext);
    }

    /// <summary>
    /// Walks the exception chain to find the innermost exception message,
    /// which typically contains the most actionable error description.
    /// </summary>
    /// <param name="exception">The top-level exception.</param>
    /// <returns>The innermost non-empty exception message, or a generic fallback.</returns>
    private static string ResolveApiErrorDescription(Exception exception)
    {
        var current = exception;
        while (current.InnerException != null && !string.IsNullOrWhiteSpace(current.InnerException.Message))
        {
            current = current.InnerException;
        }

        return string.IsNullOrWhiteSpace(current.Message)
            ? "Request processing failed."
            : current.Message;
    }
}
