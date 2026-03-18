/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HCL.CS.SF.ProxyService.Hosting;

namespace HCL.CS.SF.Hosting.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IApplicationBuilder"/> and
/// <see cref="IEndpointRouteBuilder"/> to configure the HCL.CS.SF middleware pipeline,
/// including security headers, correlation IDs, request observability, endpoint routing,
/// API handling, and health-check endpoints.
/// </summary>
public static class HCLCSSFBuilder
{
    /// <summary>
    /// Key stored in <see cref="IApplicationBuilder.Properties"/> to ensure the
    /// observability pipeline (correlation ID + request observability) is registered at most once.
    /// </summary>
    private const string ObservabilityPipelineRegistrationKey = "HCL.CS.SF:observability-pipeline-registered";

    /// <summary>
    /// Registers HCL.CS.SF security response headers (HSTS, X-Frame-Options, etc.)
    /// This MUST be registered before UseHCLCSSFEndpoint and UseHCLCSSFApi so that
    /// every OAuth/OIDC and management API response carries the required headers.
    /// </summary>
    /// <param name="app">The application builder to add the middleware to.</param>
    /// <returns>The same <see cref="IApplicationBuilder"/> instance for chaining.</returns>
    public static IApplicationBuilder UseHCLCSSFSecurityHeaders(this IApplicationBuilder app)
    {
        // Register the security headers middleware early in the pipeline to cover all downstream responses.
        app.UseMiddleware<SecurityHeadersMiddleware>();
        return app;
    }

    /// <summary>
    /// Adds the correlation-ID middleware that propagates or generates a unique request
    /// identifier for distributed tracing across service boundaries.
    /// </summary>
    /// <param name="app">The application builder to add the middleware to.</param>
    /// <returns>The same <see cref="IApplicationBuilder"/> instance for chaining.</returns>
    public static IApplicationBuilder UseHCLCSSFCorrelationId(this IApplicationBuilder app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        return app;
    }

    /// <summary>
    /// Adds the request observability middleware that captures timing, status codes, and
    /// diagnostic metadata for each HTTP request.
    /// </summary>
    /// <param name="app">The application builder to add the middleware to.</param>
    /// <returns>The same <see cref="IApplicationBuilder"/> instance for chaining.</returns>
    public static IApplicationBuilder UseHCLCSSFRequestObservability(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestObservabilityMiddleware>();
        return app;
    }

    /// <summary>
    /// Registers the HCL.CS.SF endpoint middleware that handles OAuth 2.0 / OIDC protocol
    /// endpoints (authorize, token, userinfo, etc.). Automatically registers the observability
    /// pipeline if it has not been added yet.
    /// </summary>
    /// <param name="app">The application builder to add the middleware to.</param>
    /// <returns>The same <see cref="IApplicationBuilder"/> instance for chaining.</returns>
    public static IApplicationBuilder UseHCLCSSFEndpoint(this IApplicationBuilder app)
    {
        // Ensure correlation-ID and observability middleware are present before the endpoint middleware.
        EnsureObservabilityPipeline(app);
        app.UseMiddleware<HCLCSSFEndpointMiddleware>();
        return app;
    }

    /// <summary>
    /// Registers the HCL.CS.SF management API middleware that exposes administrative
    /// endpoints (clients, users, roles, resources, etc.). Automatically registers the
    /// observability pipeline if it has not been added yet.
    /// </summary>
    /// <param name="app">The application builder to add the middleware to.</param>
    /// <returns>The same <see cref="IApplicationBuilder"/> instance for chaining.</returns>
    public static IApplicationBuilder UseHCLCSSFApi(this IApplicationBuilder app)
    {
        // Ensure correlation-ID and observability middleware are present before the API middleware.
        EnsureObservabilityPipeline(app);
        app.UseMiddleware<HCLCSSFApiMiddleware>();
        return app;
    }

    /// <summary>
    /// Maps the standard health-check endpoints used by container orchestrators and load balancers.
    /// The liveness probe (<paramref name="liveRoute"/>) returns a simple "self" check, while
    /// the readiness probe (<paramref name="readyRoute"/>) includes database and cache dependency checks.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder to map the health checks onto.</param>
    /// <param name="liveRoute">Route for the liveness probe. Defaults to <c>/health/live</c>.</param>
    /// <param name="readyRoute">Route for the readiness probe. Defaults to <c>/health/ready</c>.</param>
    /// <returns>The same <see cref="IEndpointRouteBuilder"/> instance for chaining.</returns>
    public static IEndpointRouteBuilder MapHCLCSSFHealthChecks(
        this IEndpointRouteBuilder endpoints,
        string liveRoute = "/health/live",
        string readyRoute = "/health/ready")
    {
        // Liveness probe: only runs checks tagged "live" (the lightweight self-check).
        endpoints.MapHealthChecks(liveRoute, new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("live")
        });

        // Readiness probe: runs checks tagged "ready" (database + cache) with a JSON response writer.
        endpoints.MapHealthChecks(readyRoute, new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("ready"),
            ResponseWriter = WriteHealthResponseAsync
        });

        return endpoints;
    }

    /// <summary>
    /// Ensures the observability pipeline (correlation ID + request observability) is registered
    /// exactly once, regardless of how many times <see cref="UseHCLCSSFEndpoint"/> or
    /// <see cref="UseHCLCSSFApi"/> are called.
    /// </summary>
    /// <param name="app">The application builder to check and modify.</param>
    private static void EnsureObservabilityPipeline(IApplicationBuilder app)
    {
        // Guard: skip if the observability middleware has already been registered.
        if (app.Properties.ContainsKey(ObservabilityPipelineRegistrationKey)) return;

        // Register correlation-ID middleware first, then request observability middleware.
        app.UseHCLCSSFCorrelationId();
        app.UseHCLCSSFRequestObservability();
        app.Properties[ObservabilityPipelineRegistrationKey] = true;
    }

    /// <summary>
    /// Writes the health report as a JSON response containing the overall status, total duration,
    /// and per-check details (name, status, duration).
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <param name="report">The aggregated health report from all registered checks.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    private static Task WriteHealthResponseAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        // Build an anonymous object matching the expected JSON health-check response schema.
        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = Math.Round(report.TotalDuration.TotalMilliseconds, 2),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                durationMs = Math.Round(entry.Value.Duration.TotalMilliseconds, 2)
            })
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
