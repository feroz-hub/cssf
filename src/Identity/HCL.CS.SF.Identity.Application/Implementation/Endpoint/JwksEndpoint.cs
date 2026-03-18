/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Net;
using Microsoft.AspNetCore.Http;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Models.Endpoint.Response;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.Service.Implementation.Endpoint.Results;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Results;

namespace HCL.CS.SF.Service.Implementation.Endpoint;

/// <summary>
/// Implements the JSON Web Key Set (JWKS) Endpoint (RFC 7517).
/// Returns the public keys used by the authorization server to sign tokens,
/// enabling clients and resource servers to verify JWT signatures.
/// </summary>
internal class JwksEndpoint : IEndpoint
{
    private readonly IJWKSService jwksService;
    private readonly ILoggerService loggerService;
    private readonly TokenSettings tokenSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwksEndpoint"/> class.
    /// </summary>
    public JwksEndpoint(
        ILoggerInstance instance,
        IJWKSService jwksService,
        HCLCSSFConfig settings)
    {
        tokenSettings = settings.TokenSettings;
        this.jwksService = jwksService;
        loggerService = instance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
    }

    /// <summary>
    /// Processes a JWKS request and returns the server's public signing keys.
    /// Only GET requests are accepted. Returns 404 if key discovery is disabled.
    /// </summary>
    /// <param name="context">The HTTP context for the JWKS request.</param>
    /// <returns>A <see cref="JwksResult"/> containing the public key set.</returns>
    public async Task<IEndpointResult> ProcessAsync(HttpContext context)
    {
        loggerService.WriteTo(Log.Debug, "Processing jwks endpoint request.");

        // validate HTTP
        if (!HttpMethods.IsGet(context.Request.Method))
        {
            loggerService.WriteTo(Log.Error, "Jwks endpoint supports only GET requests.");
            return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
        }

        if (!tokenSettings.TokenConfig.ShowKeySet)
        {
            loggerService.WriteTo(Log.Error, "Key discovery disabled.");
            return new StatusCodeResult(HttpStatusCode.NotFound);
        }

        // generate response
        var response = (List<JsonWebKeyResponseModel>)await jwksService.ProcessJWKSInformations();

        return new JwksResult(response, tokenSettings.TokenConfig.CachingLifetime);
    }
}
