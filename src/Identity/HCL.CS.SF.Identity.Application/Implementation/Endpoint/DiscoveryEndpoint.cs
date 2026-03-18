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
using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Implementation.Endpoint.Results;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Results;

namespace HCL.CS.SF.Service.Implementation.Endpoint;

/// <summary>
/// Implements the OpenID Connect Discovery Endpoint (RFC 8414 / OpenID Connect Discovery 1.0).
/// Returns the provider's metadata document at /.well-known/openid-configuration,
/// including issuer, supported endpoints, grant types, scopes, claims, and signing algorithms.
/// </summary>
internal class DiscoveryEndpoint : IEndpoint
{
    /// <summary>Service that generates the OpenID Provider metadata document.</summary>
    private readonly IDiscoveryService discoveryService;
    /// <summary>Logger service for diagnostic and error logging.</summary>
    private readonly ILoggerService loggerService;
    /// <summary>Token configuration settings including caching lifetime.</summary>
    private readonly TokenSettings tokenSettings;

    /// <summary>
    /// Initializes the Discovery Endpoint.
    /// </summary>
    public DiscoveryEndpoint(
        ILoggerInstance instance,
        IDiscoveryService discoveryService,
        HCLCSSFConfig tokenSettings)
    {
        this.discoveryService = discoveryService;
        this.tokenSettings = tokenSettings.TokenSettings;
        loggerService = instance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
    }

    /// <summary>
    /// Processes a discovery metadata request and returns the OpenID Provider Configuration.
    /// The response is cacheable based on the configured caching lifetime.
    /// </summary>
    /// <param name="context">The HTTP context for the discovery request.</param>
    /// <returns>A <see cref="DiscoveryResult"/> containing the provider metadata JSON.</returns>
    public async Task<IEndpointResult> ProcessAsync(HttpContext context)
    {
        loggerService.WriteTo(Log.Debug, "Processing discovery request.");

        var request = new DiscoveryRequestModel
        {
            BaseUrl = context.GetHCLCSSFHost().IncludeEndSlash()
        };
        var result = await discoveryService.GenerateDiscoveryMetaData(request);

        loggerService.WriteTo(Log.Debug, "Discovery request successfully processed.");

        return new DiscoveryResult(result, tokenSettings.TokenConfig.CachingLifetime);
    }
}
