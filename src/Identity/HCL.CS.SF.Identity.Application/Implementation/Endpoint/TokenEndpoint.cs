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
/// Implements the OAuth 2.0 Token Endpoint (RFC 6749 Section 3.2).
/// Accepts POST requests with grant_type and client credentials, validates them,
/// and issues access tokens, refresh tokens, and/or identity tokens.
/// Supports authorization_code, client_credentials, password, refresh_token, and user_code grant types.
/// </summary>
public class TokenEndpoint : IEndpoint
{
    /// <summary>Validates client credentials (client_id + client_secret) from the request.</summary>
    private readonly IClientSecretValidator clientSecretValidator;
    /// <summary>Logger service for diagnostic and error logging.</summary>
    private readonly ILoggerService loggerService;
    /// <summary>Provides localized error and resource strings.</summary>
    private readonly IResourceStringHandler resourceStringHandler;
    /// <summary>Service responsible for generating access, identity, and refresh tokens.</summary>
    private readonly ITokenGenerationService tokenGenerationService;
    /// <summary>Validates the token request parameters based on the grant type.</summary>
    private readonly ITokenRequestValidator tokenRequestValidator;

    /// <summary>
    /// Initializes the Token Endpoint with required validators and services.
    /// </summary>
    /// <param name="instance">Logger factory instance.</param>
    /// <param name="clientSecretValidator">Validates OAuth 2.0 client authentication.</param>
    /// <param name="tokenRequestValidator">Validates token request parameters per grant type.</param>
    /// <param name="tokenGenerationService">Generates JWT access/identity/refresh tokens.</param>
    /// <param name="resourceStringHandler">Provides localized error message strings.</param>
    public TokenEndpoint(
        ILoggerInstance instance,
        IClientSecretValidator clientSecretValidator,
        ITokenRequestValidator tokenRequestValidator,
        ITokenGenerationService tokenGenerationService,
        IResourceStringHandler resourceStringHandler)
    {
        this.clientSecretValidator = clientSecretValidator;
        this.tokenRequestValidator = tokenRequestValidator;
        this.tokenGenerationService = tokenGenerationService;
        loggerService = instance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
        this.resourceStringHandler = resourceStringHandler;
    }

    /// <summary>
    /// Processes an incoming OAuth 2.0 token request.
    /// Validates HTTP method (must be POST), content type, client credentials, and request parameters,
    /// then delegates to the token generation service to produce the token response.
    /// </summary>
    /// <param name="context">The HTTP context containing the token request.</param>
    /// <returns>An <see cref="IEndpointResult"/> containing the token response or an OAuth error.</returns>
    public async Task<IEndpointResult> ProcessAsync(HttpContext context)
    {
        try
        {
            loggerService.WriteTo(Log.Debug, "Processing token request.");
            if (!HttpMethods.IsPost(context.Request.Method))
            {
                loggerService.WriteTo(Log.Error, "Invalid HTTP request for token endpoint.");
                return OpenIdConstants.Errors.InvalidRequest.Error(
                    resourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidHttpRequest));
            }

            if (!context.Request.CheckHeaderContentType())
            {
                loggerService.WriteTo(Log.Error, "Invalid HTTP request for token endpoint.");
                return OpenIdConstants.Errors.InvalidRequest.Error(
                    resourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidHttpRequest));
            }

            // validate client
            var clientResult = await clientSecretValidator.ValidateClientSecretAsync(context);
            if (clientResult.IsError) return OpenIdConstants.Errors.InvalidClient.Error(clientResult.ErrorDescription);

            // validate request
            var requestCollection = (await context.Request.ReadFormAsync()).ConvertCollection();
            var validatedTokenRequestModel =
                await tokenRequestValidator.ValidateTokenRequestAsync(requestCollection, clientResult);
            if (validatedTokenRequestModel.IsError)
                return validatedTokenRequestModel.ErrorCode.Error(validatedTokenRequestModel.ErrorDescription);
            // create response
            validatedTokenRequestModel.EndpointBaseUrl = context.GetHCLCSSFHost();
            loggerService.WriteTo(Log.Debug, "Generating token.");
            var response = await tokenGenerationService.ProcessTokenAsync(validatedTokenRequestModel);

            // Defensive guard: ensure we never send a null response into TokenResult.
            // When token generation fails (e.g. invalid/expired/invalidated refresh token),
            // return a proper OAuth error instead of throwing a NullReferenceException.
            if (response == null)
            {
                loggerService.WriteTo(Log.Error,
                    "Token generation returned null TokenResponseModel. Returning invalid_grant error.");

                return OpenIdConstants.Errors.InvalidGrant.Error(
                    resourceStringHandler.GetResourceString(EndpointErrorCodes.TokenIsNullOrInvalid));
            }

            // return result
            loggerService.WriteTo(Log.Debug, "Request successfully processed.");
            return new TokenResult(response);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Unhandled token endpoint exception.");
            return OpenIdConstants.Errors.ServerError.Error("Token request processing failed.");
        }
    }
}
