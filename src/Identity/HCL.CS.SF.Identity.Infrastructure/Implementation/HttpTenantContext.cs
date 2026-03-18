/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Http;
using HCL.CS.SF.DomainServices.Infra;

namespace HCL.CS.SF.Infrastructure.Services.Implementation;

/// <summary>
/// Resolves the current tenant identifier from the HTTP request context.
/// Supports extraction from the <c>X-Tenant-Id</c> header or from user claims
/// (<c>tenant_id</c>, <c>tenant</c>, or <c>tid</c>).
/// </summary>
internal class HttpTenantContext : ITenantContext
{
    /// <summary>
    /// Accessor for the current HTTP context.
    /// </summary>
    private readonly IHttpContextAccessor httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpTenantContext"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor for reading request headers and claims.</param>
    public HttpTenantContext(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the tenant identifier for the current request. Resolution order:
    /// 1. <c>X-Tenant-Id</c> request header
    /// 2. User claims: <c>tenant_id</c>, <c>tenant</c>, or <c>tid</c>
    /// Returns <see cref="string.Empty"/> if no tenant can be determined.
    /// </summary>
    public string TenantId
    {
        get
        {
            var context = httpContextAccessor.HttpContext;
            if (context == null) return string.Empty;

            // Priority 1: Check for explicit tenant header
            var headerValue = context.Request.Headers["X-Tenant-Id"].ToString();
            if (!string.IsNullOrWhiteSpace(headerValue)) return headerValue;

            // Priority 2: Fall back to tenant claims in the authenticated user principal
            var claimValue = context.User?.FindFirst("tenant_id")?.Value
                             ?? context.User?.FindFirst("tenant")?.Value
                             ?? context.User?.FindFirst("tid")?.Value;

            return claimValue ?? string.Empty;
        }
    }
}
