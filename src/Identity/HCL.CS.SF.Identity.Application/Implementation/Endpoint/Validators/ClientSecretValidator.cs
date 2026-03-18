/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using AutoMapper;
using Microsoft.AspNetCore.Http;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Validation;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Parsers;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Validators;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Validators;

/// <summary>
/// Validates OAuth 2.0 client authentication by loading the client, verifying the signing algorithm,
/// and comparing the provided secret against the stored secret.
/// Enforces that confidential clients must use client_secret_basic or client_secret_post.
/// </summary>
internal class ClientSecretValidator : IClientSecretValidator
{
    private readonly IRepository<Clients> clientRepository;
    private readonly IClientSecretParser clientSecretParser;
    private readonly ILoggerService loggerService;
    private readonly IMapper mapper;
    private readonly IResourceStringHandler resourceStringHandler;
    private readonly ISecretValidator secretValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientSecretValidator"/> class.
    /// </summary>
    public ClientSecretValidator(
        ILoggerInstance instance,
        IClientSecretParser clientSecretParser,
        IRepository<Clients> clientRepository,
        ISecretValidator secretValidator,
        IResourceStringHandler resourceStringHandler,
        IMapper mapper)
    {
        this.clientSecretParser = clientSecretParser;
        this.clientRepository = clientRepository;
        this.secretValidator = secretValidator;
        loggerService = instance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
        this.resourceStringHandler = resourceStringHandler;
        this.mapper = mapper;
    }

    /// <summary>
    /// Validates client credentials from the HTTP context. Parses the secret, loads the client,
    /// verifies the signing algorithm (RS256 or ES256), and validates the secret value.
    /// </summary>
    /// <param name="context">The HTTP context containing client credentials.</param>
    /// <returns>A <see cref="ClientSecretValidationModel"/> with the validated client or error details.</returns>
    public async Task<ClientSecretValidationModel> ValidateClientSecretAsync(HttpContext context)
    {
        loggerService.WriteTo(Log.Debug, "Entered into validate client secret.");

        var clientValidationResult = new ClientSecretValidationModel();

        var parsedSecret = await clientSecretParser.ParseAsync(context);
        if (parsedSecret == null || parsedSecret.IsError)
        {
            loggerService.WriteTo(Log.Error, "Client credentials not found.");
            clientValidationResult.ErrorDescription = "Client credentials not found.";
            return clientValidationResult;
        }

        // load client
        ClientsModel client = null;
        var clientsEntity = await clientRepository.GetAsync(
            client =>
                client.ClientId == parsedSecret.ClientId,
            new System.Linq.Expressions.Expression<Func<HCL.CS.SF.Domain.Entities.Endpoint.Clients, object>>[] { x => x.RedirectUris, x => x.PostLogoutRedirectUris });
        if (clientsEntity.ContainsAny())
        {
            var clientEntity = clientsEntity.ToList()[0];
            client = mapper.Map<Clients, ClientsModel>(clientEntity);
        }

        if (client == null)
        {
            loggerService.WriteTo(Log.Error, "Invalid client identifier.");
            clientValidationResult.ErrorDescription = "Invalid client identifier.";
            return clientValidationResult;
        }

        if (string.IsNullOrWhiteSpace(client.AllowedSigningAlgorithm))
            client.AllowedSigningAlgorithm = OpenIdConstants.Algorithms.RsaSha256;

        var supportedSigningAlgorithm =
            string.Equals(client.AllowedSigningAlgorithm, OpenIdConstants.Algorithms.RsaSha256,
                StringComparison.Ordinal)
            || string.Equals(client.AllowedSigningAlgorithm, OpenIdConstants.Algorithms.EcdsaSha256,
                StringComparison.Ordinal);
        if (!supportedSigningAlgorithm)
        {
            loggerService.WriteTo(Log.Error, "Client must use RS256 or ES256 signing algorithm. Client: {clientId}.",
                client.ClientId);
            clientValidationResult.ErrorDescription =
                resourceStringHandler.GetResourceString(EndpointErrorCodes.SigningAlgorithmIsInvalid);
            return clientValidationResult;
        }

        if (client.RequireClientSecret)
        {
            if (parsedSecret.Type == AuthenticationConstants.ParsedTypes.NoSecret || parsedSecret.Credential == null)
            {
                loggerService.WriteTo(Log.Error, "Client secret required but not provided for client: {clientId}.",
                    client.ClientId);
                clientValidationResult.ErrorDescription =
                    resourceStringHandler.GetResourceString(EndpointErrorCodes.ClientSecretInvalid);
                return clientValidationResult;
            }

            if (parsedSecret.ParseMethod != ParseMethods.Basic &&
                parsedSecret.ParseMethod != ParseMethods.Post)
            {
                loggerService.WriteTo(Log.Error,
                    "Confidential client authentication must use client_secret_basic or client_secret_post. Client: {clientId}.",
                    client.ClientId);
                clientValidationResult.ErrorDescription =
                    resourceStringHandler.GetResourceString(EndpointErrorCodes.ClientSecretInvalid);
                return clientValidationResult;
            }

            var isValid = await secretValidator.ValidateSecretAsync(client, parsedSecret);
            if (!isValid)
            {
                loggerService.WriteTo(Log.Error, "Client secret validation failed for client: {clientId}.",
                    client.ClientId);
                clientValidationResult.ErrorDescription =
                    resourceStringHandler.GetResourceString(EndpointErrorCodes.ClientSecretInvalid);
                return clientValidationResult;
            }
        }
        else
        {
            loggerService.WriteTo(Log.Debug, "Public client - skipping client secret validation.");
        }

        clientValidationResult = new ClientSecretValidationModel
        {
            IsError = false,
            Client = client,
            Secret = parsedSecret
        };
        return clientValidationResult;
    }
}
