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
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Results;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Validators;

namespace HCL.CS.SF.Service.Implementation.Endpoint;

/// <summary>
/// Implements the OpenID Connect RP-Initiated Logout Endpoint (OpenID Connect RP-Initiated Logout 1.0).
/// Accepts GET or POST requests with an optional id_token_hint and post_logout_redirect_uri,
/// validates the request, and redirects the user to the configured logout page.
/// </summary>
internal class EndSessionEndpoint : IEndpoint
{
    private readonly IEndSessionRequestValidator endSessionRequestValidator;
    private readonly ILoggerService loggerService;
    private readonly IResourceStringHandler resourceStringHandler;
    private readonly ISessionManagementService sessionManagement;

    /// <summary>
    /// Initializes a new instance of the <see cref="EndSessionEndpoint"/> class.
    /// </summary>
    public EndSessionEndpoint(
        ILoggerInstance instance,
        IEndSessionRequestValidator endSessionRequestValidator,
        IResourceStringHandler resourceStringHandler,
        ISessionManagementService sessionManagement)
    {
        this.endSessionRequestValidator = endSessionRequestValidator;
        loggerService = instance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
        this.sessionManagement = sessionManagement;
        this.resourceStringHandler = resourceStringHandler;
    }

    /// <summary>
    /// Processes an OIDC end-session (logout) request. Validates the id_token_hint if provided,
    /// resolves the authenticated user, and returns an <see cref="EndSessionResult"/> for logout redirection.
    /// </summary>
    /// <param name="context">The HTTP context containing the logout request.</param>
    /// <returns>An <see cref="IEndpointResult"/> that redirects to the logout page.</returns>
    public async Task<IEndpointResult> ProcessAsync(HttpContext context)
    {
        loggerService.WriteTo(Log.Debug, "Processing logout Session request.");
        Dictionary<string, string> requestCollection = null;
        if (HttpMethods.IsGet(context.Request.Method))
        {
            requestCollection = context.Request.Query.ConvertCollection();
        }
        else if (HttpMethods.IsPost(context.Request.Method))
        {
            requestCollection = (await context.Request.ReadFormAsync()).ConvertCollection();
        }
        else
        {
            loggerService.WriteTo(Log.Error, "Invalid HTTP request for Session endpoint.");
            return OpenIdConstants.Errors.InvalidRequest.Error(
                resourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidHttpRequest));
        }

        var user = await sessionManagement.GetUserPrincipalFromContextAsync();
        var result = await endSessionRequestValidator.ValidateRequestAsync(requestCollection, user);
        if (result.IsError)
            loggerService.WriteTo(Log.Error, "Error processing end session request.");
        else
            loggerService.WriteTo(Log.Debug, "Success validating end session request.");

        return new EndSessionResult(result);
    }
}
