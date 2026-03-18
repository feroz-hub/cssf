/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Http;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Results;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Results;

/// <summary>
/// Endpoint result that renders the OpenID Connect Discovery metadata document as JSON.
/// Sets cache headers based on the configured caching lifetime.
/// </summary>
internal class DiscoveryResult : IEndpointResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DiscoveryResult"/> class.
    /// </summary>
    public DiscoveryResult(Dictionary<string, object> metadataValues, int cacheExpiry)
    {
        DiscoveryMetadata = metadataValues;
        CacheExpiry = cacheExpiry;
    }

    /// <summary>
    /// Gets or sets the discovery metadata.
    /// </summary>
    internal Dictionary<string, object> DiscoveryMetadata { get; set; }

    /// <summary>
    /// Gets or sets the cache expiry.
    /// </summary>
    internal int CacheExpiry { get; set; }

    /// <summary>
    /// Constructs the response.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>The operation result.</returns>
    public async Task ConstructResponseAsync(HttpContext context)
    {
        context.Response.SetResponseCache(CacheExpiry);
        context.Response.StatusCode = OpenIdConstants.HTTPStatusCodes.success;
        await context.Response.WriteResponseJsonAsync(DiscoveryMetadata);
    }
}
