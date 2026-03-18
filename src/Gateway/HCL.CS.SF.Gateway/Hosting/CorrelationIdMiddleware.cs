/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Http;

namespace HCL.CS.SF.ProxyService.Hosting;

/// <summary>
/// HTTP pipeline middleware that ensures every request has a correlation ID for
/// distributed tracing. If the inbound request includes a valid X-Correlation-ID header,
/// it is reused; otherwise, a new GUID is generated. The correlation ID is stored in
/// <see cref="HttpContext.TraceIdentifier"/>, <see cref="HttpContext.Items"/>, and
/// echoed back in the response header for end-to-end traceability.
/// </summary>
public class CorrelationIdMiddleware
{
    /// <summary>
    /// The next middleware delegate in the HTTP pipeline.
    /// </summary>
    private readonly RequestDelegate next;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationIdMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    public CorrelationIdMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    /// <summary>
    /// Resolves the correlation ID from the inbound header or generates a new one,
    /// then propagates it to the trace identifier, context items, and response header.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var inboundCorrelationId = context.Request.Headers[ObservabilityConstants.CorrelationIdHeaderName].ToString();
        var correlationId = ResolveCorrelationId(inboundCorrelationId);

        // Store the correlation ID in multiple locations for downstream access.
        context.TraceIdentifier = correlationId;
        context.Items[ObservabilityConstants.CorrelationIdItemKey] = correlationId;
        context.Response.Headers[ObservabilityConstants.CorrelationIdHeaderName] = correlationId;

        await next(context);
    }

    /// <summary>
    /// Validates the inbound correlation ID and either returns it (if valid) or creates a new one.
    /// </summary>
    /// <param name="correlationId">The correlation ID from the inbound request header.</param>
    /// <returns>A validated or newly generated correlation ID.</returns>
    private static string ResolveCorrelationId(string? correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId)) return CreateCorrelationId();

        var value = correlationId.Trim();
        if (!IsValidCorrelationId(value)) return CreateCorrelationId();

        return value;
    }

    /// <summary>
    /// Creates a new correlation ID as a 32-character hex GUID without hyphens.
    /// </summary>
    /// <returns>A new correlation ID string.</returns>
    private static string CreateCorrelationId()
    {
        return Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// Validates that a correlation ID contains only safe characters (alphanumeric, hyphens,
    /// underscores, periods) and is within a reasonable length (1-128 characters)
    /// to prevent header injection attacks.
    /// </summary>
    /// <param name="correlationId">The correlation ID to validate.</param>
    /// <returns><c>true</c> if the value is safe to use; otherwise, <c>false</c>.</returns>
    private static bool IsValidCorrelationId(string correlationId)
    {
        if (correlationId.Length is <= 0 or > 128) return false;

        foreach (var character in correlationId)
        {
            if (char.IsLetterOrDigit(character)) continue;
            if (character is '-' or '_' or '.') continue;
            return false;
        }

        return true;
    }
}
