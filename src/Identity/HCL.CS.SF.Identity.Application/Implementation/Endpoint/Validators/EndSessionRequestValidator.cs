/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using AutoMapper;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.DomainServices.UnitOfWork.Endpoint;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Implementation.Endpoint.Specifications;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Validators;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Validators;

/// <summary>
/// Validates OpenID Connect end-session (logout) requests.
/// Handles both the initial logout request (with optional id_token_hint) and
/// the callback request that triggers front-channel logout iframes.
/// </summary>
internal class EndSessionRequestValidator : IEndSessionRequestValidator
{
    private readonly IRepository<Clients> clientRepository;
    private readonly TokenSettings configSettings;
    private readonly IFrameworkResultService frameworkResultService;
    private readonly Dictionary<string, AsymmetricKeyInfoModel> keyStore;
    private readonly ILoggerService loggerService;
    private readonly IMapper mapper;
    private readonly ISessionManagementService sessionManagementService;
    private readonly IClientsUnitOfWork unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="EndSessionRequestValidator"/> class.
    /// </summary>
    public EndSessionRequestValidator(
        ILoggerInstance instance,
        IRepository<Clients> clientRepository,
        IMapper mapper,
        ISessionManagementService sessionManagementService,
        HCLCSSFConfig tokenSettings,
        IClientsUnitOfWork unitOfWork,
        IFrameworkResultService frameworkResultService,
        Dictionary<string, AsymmetricKeyInfoModel> keyStore)
    {
        this.clientRepository = clientRepository;
        this.mapper = mapper;
        this.sessionManagementService = sessionManagementService;
        configSettings = tokenSettings.TokenSettings;
        this.unitOfWork = unitOfWork;
        this.keyStore = keyStore;
        this.frameworkResultService = frameworkResultService;
        loggerService = instance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
    }

    /// <summary>
    /// Validates the end-session callback request. Decrypts the session data and
    /// resolves front-channel logout URLs for all clients in the session.
    /// </summary>
    /// <param name="requestCollection">The callback query parameters.</param>
    /// <returns>A validated callback model with front-channel logout URLs.</returns>
    public async Task<ValidatedEndSessionCallbackRequestModel> ValidateCallbackRequestAsync(
        Dictionary<string, string> requestCollection)
    {
        var result = new ValidatedEndSessionCallbackRequestModel
        {
            IsError = true
        };

        var sessionId =
            requestCollection.GetValueFromDictionary(OpenIdConstants.RoutePathParameters.EndSessionCallback);
        var logoutSession = await sessionId.UnProtectDataAsync<LogoutMessageModel>();
        if (logoutSession.ClientIdCollection.ContainsAny())
        {
            result.IsError = false;
            result.FrontChannelLogoutUrls = await GetFrontChannelLogoutUrlsAsync(logoutSession);
            result.TokenConfigOptions = configSettings;
        }
        else
        {
            result.ErrorCode = "Failed to read end Session callback message";
        }

        return result;
    }

    /// <summary>
    /// Validates an end-session request. If an id_token_hint is present, validates the identity token
    /// and resolves the client. Otherwise, resolves the client from the request parameters.
    /// </summary>
    /// <param name="requestCollection">The request parameters.</param>
    /// <param name="user">The currently authenticated user.</param>
    /// <returns>A validated end-session request model.</returns>
    public async Task<ValidatedEndSessionRequestModel> ValidateRequestAsync(
        Dictionary<string, string> requestCollection, ClaimsPrincipal user)
    {
        loggerService.WriteTo(Log.Debug, "Entered into end session request validation.");
        var validatedRequestModel = new ValidatedEndSessionRequestModel
        {
            RequestRawData = requestCollection,
            TokenConfigOptions = configSettings,
            Subject = user
        };

        var idTokenHint = requestCollection.GetValueFromDictionary(OpenIdConstants.EndSessionRequest.IdTokenHint);
        if (!string.IsNullOrWhiteSpace(idTokenHint))
        {
            var endSessionRequestValidation = new EndSessionRequestSpecification(unitOfWork, clientRepository, mapper,
                sessionManagementService, keyStore);
            var errorModel = await endSessionRequestValidation.ValidateAsync(validatedRequestModel);
            if (!endSessionRequestValidation.IsValid)
            {
                errorModel = frameworkResultService.Failed(errorModel.ErrorCode, errorModel.ErrorMessage);
                validatedRequestModel.IsError = true;
                validatedRequestModel.ErrorCode = errorModel.ErrorCode;
                validatedRequestModel.ErrorDescription = errorModel.ErrorMessage;
                return validatedRequestModel;
            }
        }
        else
        {
            validatedRequestModel.SessionId = await sessionManagementService.GetSessionId();
            validatedRequestModel.ClientIds = await sessionManagementService.GetClientListAsync();

            var clientId = requestCollection.GetValueFromDictionary(OpenIdConstants.AuthorizeRequest.ClientId);
            if (!string.IsNullOrWhiteSpace(clientId))
            {
                var clientsEntity = await clientRepository.GetAsync(
                    client => client.ClientId == clientId,
                    new System.Linq.Expressions.Expression<Func<HCL.CS.SF.Domain.Entities.Endpoint.Clients, object>>[] { x => x.RedirectUris, x => x.PostLogoutRedirectUris });

                if (clientsEntity.ContainsAny())
                {
                    validatedRequestModel.Client = mapper.Map<Clients, ClientsModel>(clientsEntity[0]);
                    validatedRequestModel.ClientId = validatedRequestModel.Client.ClientId;

                    var postLogoutRedirectUri =
                        requestCollection.GetValueFromDictionary(OpenIdConstants.EndSessionRequest.PostLogoutRedirectUri);
                    if (!string.IsNullOrWhiteSpace(postLogoutRedirectUri))
                    {
                        var allowedPostLogoutRedirectUris =
                            validatedRequestModel.Client.PostLogoutRedirectUris?.ConvertAll(x => x.PostLogoutRedirectUri)
                            ?? new List<string>();

                        if (allowedPostLogoutRedirectUris.Contains(postLogoutRedirectUri))
                        {
                            validatedRequestModel.PostLogOutUri = postLogoutRedirectUri;
                            var state = requestCollection.GetValueFromDictionary(OpenIdConstants.EndSessionRequest.State);
                            if (!string.IsNullOrWhiteSpace(state))
                            {
                                validatedRequestModel.State = state;
                            }
                        }
                        else
                        {
                            validatedRequestModel.IsError = true;
                            validatedRequestModel.ErrorCode = OpenIdConstants.Errors.InvalidRequest;
                            validatedRequestModel.ErrorDescription = EndpointErrorCodes.InvalidPostLogoutRedirectUri;
                            return validatedRequestModel;
                        }
                    }
                }
            }
        }

        return validatedRequestModel;
    }

    /// <summary>
    /// Resolves front-channel logout URLs for all clients in the logout session.
    /// Appends session ID and issuer parameters when the client requires front-channel logout session support.
    /// </summary>
    /// <param name="logoutMessage">The logout message containing the list of client IDs in the session.</param>
    /// <returns>An enumerable of front-channel logout URLs to be rendered as hidden iframes.</returns>
    public async Task<IEnumerable<string>> GetFrontChannelLogoutUrlsAsync(LogoutMessageModel logoutMessage)
    {
        var frontChannelUrls = new List<string>();
        foreach (var clientId in logoutMessage.ClientIdCollection)
            if (!string.IsNullOrWhiteSpace(clientId))
            {
                var clientCollection = await clientRepository.GetAsync(
                    client => client.ClientId == clientId,
                    x => new { x.FrontChannelLogoutSessionRequired, x.FrontChannelLogoutUri });
                if (clientCollection.ContainsAny())
                {
                    var client = clientCollection.FirstOrDefault();
                    var frontChannelUrl = client.FrontChannelLogoutUri;
                    if (!string.IsNullOrWhiteSpace(frontChannelUrl))
                    {
                        if (client.FrontChannelLogoutSessionRequired)
                        {
                            frontChannelUrl = frontChannelUrl.AddQueryString(OpenIdConstants.EndSessionRequest.Sid,
                                logoutMessage.SessionId);
                            frontChannelUrl = frontChannelUrl.AddQueryString(OpenIdConstants.EndSessionRequest.Issuer,
                                configSettings.TokenConfig.IssuerUri);
                        }

                        frontChannelUrls.Add(frontChannelUrl);
                    }
                }
            }

        if (frontChannelUrls.ContainsAny())
        {
            var msg = frontChannelUrls.Aggregate((x, y) => x + ", " + y);
            loggerService.WriteTo(Log.Debug, "Client front-channel logout URLs: {0}", msg);
        }
        else
        {
            loggerService.WriteTo(Log.Debug, "No client front-channel logout URLs");
        }

        return frontChannelUrls;
    }
}
