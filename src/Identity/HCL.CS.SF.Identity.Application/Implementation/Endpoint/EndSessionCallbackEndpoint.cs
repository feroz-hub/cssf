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
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Implementation.Endpoint.Results;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Results;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Validators;

namespace HCL.CS.SF.Service.Implementation.Endpoint;

/// <summary>
/// Implements the OpenID Connect End Session Callback Endpoint.
/// Handles the callback after logout to render front-channel logout iframes
/// for all clients that participated in the user's session (OpenID Connect Front-Channel Logout 1.0).
/// </summary>
internal class EndSessionCallbackEndpoint : IEndpoint
{
    private readonly IEndSessionRequestValidator endSessionRequestValidator;
    private readonly ILoggerService loggerService;
    private readonly IResourceStringHandler resourceStringHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="EndSessionCallbackEndpoint"/> class.
    /// </summary>
    public EndSessionCallbackEndpoint(
        ILoggerInstance instance,
        IEndSessionRequestValidator endSessionRequestValidator,
        IResourceStringHandler resourceStringHandler)
    {
        this.endSessionRequestValidator = endSessionRequestValidator;
        loggerService = instance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
        this.resourceStringHandler = resourceStringHandler;
    }

    /// <summary>
    /// Processes the end-session callback request. Validates the callback parameters
    /// and generates an HTML page containing front-channel logout iframes for each client.
    /// </summary>
    /// <param name="context">The HTTP context for the callback request.</param>
    /// <returns>An <see cref="EndSessionCallbackResult"/> with front-channel logout iframes.</returns>
    public async Task<IEndpointResult> ProcessAsync(HttpContext context)
    {
        if (!HttpMethods.IsGet(context.Request.Method))
        {
            loggerService.WriteTo(Log.Error, "Invalid HTTP request for session callback endpoint.");
            return OpenIdConstants.Errors.InvalidRequest.Error(
                resourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidHttpRequest));
        }

        loggerService.WriteTo(Log.Debug, "Processing sign out callback request");

        // TODO - Need to identify the front channel logout flow (how it works)
        var requestCollection = context.Request.Query.ConvertCollection();
        var result = await endSessionRequestValidator.ValidateCallbackRequestAsync(requestCollection);
        if (!result.IsError)
            loggerService.WriteTo(Log.Debug, "Successful sign out callback.");
        else
            loggerService.WriteTo(Log.Error, "Error validating sign out callback: {error}", result.ErrorCode);

        return new EndSessionCallbackResult(result);
    }
}
