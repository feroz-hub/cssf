/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.ProxyService.Hosting;

/// <summary>
/// Defines constant values used by the observability middleware for correlation ID
/// propagation via HTTP headers and <see cref="Microsoft.AspNetCore.Http.HttpContext.Items"/>.
/// </summary>
internal static class ObservabilityConstants
{
    /// <summary>
    /// The HTTP header name used to propagate correlation IDs between services.
    /// Inbound requests may provide this header; the gateway always echoes it in responses.
    /// </summary>
    internal const string CorrelationIdHeaderName = "X-Correlation-ID";

    /// <summary>
    /// The key used to store the resolved correlation ID in <see cref="Microsoft.AspNetCore.Http.HttpContext.Items"/>
    /// so that downstream middleware and handlers can access it without re-parsing the header.
    /// </summary>
    internal const string CorrelationIdItemKey = "HCL.CS.SF:correlation-id";
}
