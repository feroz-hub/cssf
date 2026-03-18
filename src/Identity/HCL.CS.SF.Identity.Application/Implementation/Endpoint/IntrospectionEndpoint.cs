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
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.Domain.Models.Endpoint.Response;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.DomainServices.Wrappers;
using HCL.CS.SF.Service.Extension;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Implementation.Endpoint.Results;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Results;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Validators;

namespace HCL.CS.SF.Service.Implementation.Endpoint;

/// <summary>
/// Implements the OAuth 2.0 Token Introspection Endpoint (RFC 7662).
/// Allows resource servers to query the authorization server about the state
/// of an access or refresh token, determining whether it is active and retrieving its metadata.
/// </summary>
internal class IntrospectionEndpoint : IEndpoint
{
    private readonly IRepository<Clients> clientRepository;
    private readonly IClientSecretValidator clientSecretValidator;
    private readonly IIntrospectionRequestValidator introspectionRequestValidator;
    private readonly ILoggerService loggerService;
    private readonly IResourceStringHandler resourceStringHandler;
    private readonly UserManagerWrapper<Users> userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntrospectionEndpoint"/> class.
    /// </summary>
    public IntrospectionEndpoint(
        ILoggerInstance instance,
        IIntrospectionRequestValidator introspectionRequestValidator,
        IClientSecretValidator clientSecretValidator,
        UserManagerWrapper<Users> userManager,
        IResourceStringHandler resourceStringHandler,
        IRepository<Clients> clientRepository)
    {
        this.introspectionRequestValidator = introspectionRequestValidator;
        this.clientSecretValidator = clientSecretValidator;
        loggerService = instance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
        this.userManager = userManager;
        this.resourceStringHandler = resourceStringHandler;
        this.clientRepository = clientRepository;
    }

    /// <summary>
    /// Processes a token introspection request. Validates the client, introspects the token,
    /// and returns token metadata (active status, scopes, expiration, subject, etc.).
    /// </summary>
    /// <param name="context">The HTTP context containing the introspection request.</param>
    /// <returns>An <see cref="IntrospectionResult"/> with token metadata or inactive status.</returns>
    public async Task<IEndpointResult> ProcessAsync(HttpContext context)
    {
        loggerService.WriteTo(Log.Debug, "Starting introspection request.");

        if (!HttpMethods.IsPost(context.Request.Method))
        {
            loggerService.WriteTo(Log.Error, "Invalid HTTP request for token endpoint.");
            return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
        }

        if (!context.Request.CheckHeaderContentType())
        {
            loggerService.WriteTo(Log.Error, "Invalid HTTP request for token endpoint.");
            return new StatusCodeResult(HttpStatusCode.UnsupportedMediaType);
        }

        var validationResult = new ValidatedIntrospectionRequestModel();

        var requestCollection = (await context.Request.ReadFormAsync()).ConvertCollection();

        // validate client
        var clientResult = await clientSecretValidator.ValidateClientSecretAsync(context);
        if (clientResult.Client == null)
        {
            loggerService.WriteTo(Log.Error, "Client not found.");
            validationResult.ErrorCode = OpenIdConstants.Errors.InvalidClient;
            return validationResult.ErrorCode.Error(
                resourceStringHandler.GetResourceString(EndpointErrorCodes.ClientDoesNotExist));
        }

        // request validation
        loggerService.WriteTo(Log.Debug, "Calling into introspection request validator: {type}",
            introspectionRequestValidator.GetType().FullName);

        validationResult =
            await introspectionRequestValidator.ValidateIntrospectionRequestAsync(requestCollection,
                clientResult.Client);

        // create response object.
        var response = new IntrospectionResponseModel();
        switch (validationResult.IsError)
        {
            // render result
            case false:
            {
                response.Active = true;
                if (validationResult.TokenType == OpenIdConstants.TokenResponseType.AccessToken)
                {
                    Users user = null;
                    response.ClientId = validationResult.ClientId;
                    if (!string.IsNullOrWhiteSpace(validationResult.UserId) && validationResult.UserId.IsGuid())
                    {
                        user = await userManager.FindByIdAsync(validationResult.UserId);
                        if (user != null)
                        {
                            response.UserName = user.UserName;
                            response.SubjectId = user.Id.ToString();
                        }
                        else
                        {
                            var clientsEntity =
                                await clientRepository.GetAsync(client => client.ClientId == validationResult.UserId);
                            if (clientsEntity.ContainsAny())
                            {
                                response.UserName = clientsEntity.ToList()[0].ClientName;
                                if (string.IsNullOrWhiteSpace(response.ClientId))
                                    response.ClientId = validationResult.UserId;
                            }
                        }
                    }

                    response.Audience = validationResult.DecodedToken.Audiences.ConvertSpaceSeparatedString();
                    response.Issuer = validationResult.DecodedToken.Issuer;
                    response.Scope = validationResult.Scopes;
                    response.IssuedAt = validationResult.DecodedToken.IssuedAt.ToUnixTime().ToString();
                    response.ExpiresAt = validationResult.ExpiresAt.HasValue
                        ? validationResult.ExpiresAt.ToString()
                        : string.Empty;
                }
                else
                {
                    response.IssuedAt = validationResult.IssuedAt.ToString();
                    response.ExpiresAt = validationResult.ExpiresAt.HasValue
                        ? validationResult.ExpiresAt.ToString()
                        : string.Empty;
                }

                loggerService.WriteTo(Log.Debug, validationResult.Active.ToString());
                break;
            }

            case true:
                response.Active = false;
                loggerService.WriteTo(Log.Debug, "The token is not active." + validationResult.ErrorCode);
                return new IntrospectionResult(response);
        }

        loggerService.WriteTo(Log.Debug, "Introspection request processed successfully.");
        return new IntrospectionResult(response);
    }
}
