/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Diagnostics;
using System.Text.Json;

namespace HCLCSSFInstallerMVC.Middleware;

/// <summary>
/// Catches all unhandled exceptions during request processing.
/// For HTML requests, redirects to the /error page. For API requests,
/// returns a 500 problem+json response with the trace identifier.
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes the middleware with the next delegate and logger.
    /// </summary>
    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the next middleware and catches any unhandled exceptions.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
            _logger.LogError(ex, "Unhandled installer exception. TraceId: {TraceId}", traceId);

            if (context.Response.HasStarted) throw;

            context.Response.Clear();

            if (IsHtmlRequest(context.Request))
            {
                context.Response.StatusCode = StatusCodes.Status302Found;
                context.Response.Headers.Location = "/error";
                return;
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var problem = new
            {
                title = "Internal Server Error",
                status = 500,
                traceId
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }

    /// <summary>
    /// Determines if the request prefers HTML by checking the Accept header for text/html.
    /// </summary>
    private static bool IsHtmlRequest(HttpRequest request)
    {
        if (request.Headers.TryGetValue("Accept", out var acceptValues))
            return acceptValues.Any(value => value?.Contains("text/html", StringComparison.OrdinalIgnoreCase) == true);

        return false;
    }
}
