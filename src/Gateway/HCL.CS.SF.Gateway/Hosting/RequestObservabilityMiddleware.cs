/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.DomainServices.Infra;

namespace HCL.CS.SF.ProxyService.Hosting;

/// <summary>
/// HTTP pipeline middleware that emits a structured log entry and optional metrics
/// for every completed request. Captures correlation ID, tenant ID, user ID, route,
/// status code, and latency. All PII fields are redacted via <see cref="LogRedactionHelper"/>
/// before being written to logs. Metrics recording can be toggled via configuration.
/// </summary>
public class RequestObservabilityMiddleware
{
    /// <summary>
    /// Fallback application logger used when the framework logger is unavailable.
    /// </summary>
    private readonly ILogger<RequestObservabilityMiddleware> applicationLogger;

    /// <summary>
    /// Framework-level structured logger for writing request completion events.
    /// </summary>
    private readonly ILoggerService loggerService;

    /// <summary>
    /// Whether OpenTelemetry-compatible metrics recording is enabled (from configuration).
    /// </summary>
    private readonly bool metricsEnabled;

    /// <summary>
    /// The next middleware delegate in the HTTP pipeline.
    /// </summary>
    private readonly RequestDelegate next;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestObservabilityMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="applicationLogger">The ASP.NET Core logger for this middleware.</param>
    /// <param name="loggerInstance">Logger instance factory for obtaining the framework logger.</param>
    /// <param name="configuration">Application configuration for reading observability settings.</param>
    public RequestObservabilityMiddleware(
        RequestDelegate next,
        ILogger<RequestObservabilityMiddleware> applicationLogger,
        ILoggerInstance loggerInstance,
        IConfiguration configuration)
    {
        this.next = next;
        this.applicationLogger = applicationLogger;

        loggerService = loggerInstance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
        metricsEnabled = configuration.GetValue("HCL.CS.SF:Observability:EnableMetrics", false);
    }

    /// <summary>
    /// Invokes the next middleware, then records a structured log entry and optional metrics
    /// with the request's correlation ID, tenant, user, route, status code, and latency.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="tenantContext">The resolved tenant context for multi-tenant logging.</param>
    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        var startTimestamp = Stopwatch.GetTimestamp();

        try
        {
            await next(context);
        }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(startTimestamp);
            var latencyMs = elapsed.TotalMilliseconds;

            // Resolve and redact all fields before logging.
            var correlationId = ResolveCorrelationId(context);
            var tenantId = LogRedactionHelper.GetSafeTenantId(tenantContext.TenantId);
            var userId = LogRedactionHelper.GetSafeUserId(ResolveUserId(context.User));
            var route = context.Request.Path.HasValue ? context.Request.Path.Value : "/";
            var statusCode = context.Response.StatusCode;

            WriteStructuredRequestLog(correlationId, tenantId, userId, route, statusCode, latencyMs);

            if (metricsEnabled)
            {
                // Normalize the route to a coarse group to avoid high-cardinality metric labels.
                var routeGroup = NormalizeRouteGroup(route);
                HCLCSSFMetrics.RecordRequest(context.Request.Method, routeGroup, statusCode, latencyMs);
            }
        }
    }

    /// <summary>
    /// Writes a structured log entry for the completed request using the framework logger
    /// if available, falling back to the ASP.NET Core application logger.
    /// </summary>
    private void WriteStructuredRequestLog(
        string correlationId,
        string tenantId,
        string userId,
        string? route,
        int statusCode,
        double latencyMs)
    {
        const string template =
            "request_completed correlationId={CorrelationId} tenantId={TenantId} userId={UserId} route={Route} statusCode={StatusCode} latencyMs={LatencyMs}";

        var normalizedRoute = string.IsNullOrWhiteSpace(route) ? "/" : route;
        var roundedLatencyMs = Math.Round(latencyMs, 2);

        if (loggerService != null)
        {
            loggerService.WriteTo(
                Log.Information,
                template,
                correlationId,
                tenantId,
                userId,
                normalizedRoute,
                statusCode,
                roundedLatencyMs);
            return;
        }

        applicationLogger.LogInformation(
            template,
            correlationId,
            tenantId,
            userId,
            normalizedRoute,
            statusCode,
            roundedLatencyMs);
    }

    /// <summary>
    /// Resolves the correlation ID for this request. Prefers the value stored in
    /// <see cref="HttpContext.Items"/> by the <see cref="CorrelationIdMiddleware"/>,
    /// then falls back to the trace identifier, and finally generates a new GUID.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A non-empty correlation ID string.</returns>
    private static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Items.TryGetValue(ObservabilityConstants.CorrelationIdItemKey, out var correlationId) &&
            correlationId is string correlation)
            return correlation;

        if (!string.IsNullOrWhiteSpace(context.TraceIdentifier)) return context.TraceIdentifier;

        return Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// Extracts the user identifier from the claims principal, checking the "sub" claim first,
    /// then the standard NameIdentifier claim, and finally the identity name.
    /// </summary>
    /// <param name="principal">The authenticated claims principal, or null.</param>
    /// <returns>The user ID string, or null if not authenticated.</returns>
    private static string? ResolveUserId(ClaimsPrincipal principal)
    {
        if (principal?.Identity?.IsAuthenticated != true) return null;

        return principal.FindFirst("sub")?.Value
               ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? principal.Identity?.Name;
    }

    /// <summary>
    /// Normalizes a request path into a coarse route group to prevent high-cardinality
    /// metric labels. API paths are collapsed to "/security/api", other paths under
    /// "/security/" retain only the second segment, and all other paths retain only the root.
    /// </summary>
    /// <param name="route">The raw request path.</param>
    /// <returns>A normalized route group string.</returns>
    private static string NormalizeRouteGroup(string? route)
    {
        if (string.IsNullOrWhiteSpace(route)) return "/";

        if (route.StartsWith(ApiRoutePathConstants.BasePath, StringComparison.OrdinalIgnoreCase))
            return "/security/api";

        if (route.StartsWith("/security/", StringComparison.OrdinalIgnoreCase))
        {
            var segments = route.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length >= 2) return $"/security/{segments[1].ToLowerInvariant()}";

            return "/security";
        }

        var rootSegment = route.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return string.IsNullOrWhiteSpace(rootSegment) ? "/" : $"/{rootSegment.ToLowerInvariant()}";
    }
}
