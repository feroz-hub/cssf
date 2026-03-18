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
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Implementation.Endpoint.Results;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Results;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Validators;

namespace HCL.CS.SF.Service.Implementation.Endpoint;

/// <summary>
/// Implements the OAuth 2.0 Token Revocation Endpoint (RFC 7009).
/// Allows clients to notify the authorization server that a previously obtained
/// access or refresh token is no longer needed, enabling immediate invalidation.
/// </summary>
internal class TokenRevocationEndpoint : IEndpoint
{
    private readonly IClientSecretValidator clientSecretValidator;
    private readonly ILoggerService loggerService;
    private readonly IResourceStringHandler resourceStringHandler;
    private readonly ITokenRevocationRequestValidator revocationRequestValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenRevocationEndpoint"/> class.
    /// </summary>
    public TokenRevocationEndpoint(
        ILoggerInstance instance,
        IClientSecretValidator clientSecretValidator,
        ITokenRevocationRequestValidator revocationRequestValidator,
        IResourceStringHandler resourceStringHandler)
    {
        this.clientSecretValidator = clientSecretValidator;
        this.revocationRequestValidator = revocationRequestValidator;
        loggerService = instance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
        this.resourceStringHandler = resourceStringHandler;
    }

    /// <summary>
    /// Processes a token revocation request. Validates the client, then revokes
    /// the specified token (access or refresh). Returns HTTP 200 on success per RFC 7009.
    /// </summary>
    /// <param name="context">The HTTP context containing the revocation request.</param>
    /// <returns>A <see cref="StatusCodeResult"/> with HTTP 200 on success or an error.</returns>
    public async Task<IEndpointResult> ProcessAsync(HttpContext context)
    {
        if (!HttpMethods.IsPost(context.Request.Method) || !context.Request.CheckHeaderContentType())
        {
            loggerService.WriteTo(Log.Debug, "Invalid HTTP request for token revocation endpoint.");
            return OpenIdConstants.Errors.InvalidRequest.Error(
                resourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidHttpRequest));
        }

        // validate client
        var clientResult = await clientSecretValidator.ValidateClientSecretAsync(context);
        if (clientResult.Client == null) return OpenIdConstants.Errors.InvalidClient.Error();

        loggerService.WriteTo(Log.Debug, "Processing token revocation request.");

        // validate request
        var requestCollection = (await context.Request.ReadFormAsync()).ConvertCollection();
        var requestResult =
            await revocationRequestValidator.ValidateRevocationRequestAsync(requestCollection, clientResult.Client);

        if (requestResult.IsError) return requestResult.ErrorCode.Error();

        return new StatusCodeResult(HttpStatusCode.OK);
    }
}
