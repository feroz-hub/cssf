/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Diagnostics.Metrics;

namespace HCL.CS.SF.ProxyService.Hosting;

/// <summary>
/// Provides OpenTelemetry-compatible metrics instruments for recording HTTP request
/// counts and durations at the gateway level. Metrics are tagged with HTTP method,
/// normalized route group, and response status code for dashboard aggregation.
/// </summary>
internal static class HCLCSSFMetrics
{
    /// <summary>
    /// The meter instance that owns all gateway observability instruments.
    /// </summary>
    private static readonly Meter Meter = new("HCL.CS.SF.Hosting.Observability", "1.0.0");

    /// <summary>
    /// Counter tracking the total number of HTTP requests processed by the gateway.
    /// </summary>
    private static readonly Counter<long> RequestCounter =
        Meter.CreateCounter<long>("HCL.CS.SF.http.server.requests");

    /// <summary>
    /// Histogram tracking the duration of HTTP requests in milliseconds.
    /// </summary>
    private static readonly Histogram<double> RequestDurationMs =
        Meter.CreateHistogram<double>("HCL.CS.SF.http.server.duration.ms", "ms");

    /// <summary>
    /// Records a completed HTTP request by incrementing the request counter and
    /// recording the duration in the histogram, both tagged with method, route, and status code.
    /// </summary>
    /// <param name="method">The HTTP method (GET, POST, etc.).</param>
    /// <param name="routeGroup">The normalized route group (e.g., "/security/api").</param>
    /// <param name="statusCode">The HTTP response status code.</param>
    /// <param name="durationMs">The request duration in milliseconds.</param>
    internal static void RecordRequest(string method, string routeGroup, int statusCode, double durationMs)
    {
        RequestCounter.Add(
            1,
            new KeyValuePair<string, object?>("method", method),
            new KeyValuePair<string, object?>("route", routeGroup),
            new KeyValuePair<string, object?>("status_code", statusCode));

        RequestDurationMs.Record(
            durationMs,
            new KeyValuePair<string, object?>("method", method),
            new KeyValuePair<string, object?>("route", routeGroup),
            new KeyValuePair<string, object?>("status_code", statusCode));
    }
}
