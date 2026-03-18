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
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.DomainServices.Wrappers;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Implementation.Endpoint.Results;
using HCL.CS.SF.Service.Implementation.Endpoint.Specifications;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Results;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Validators;

namespace HCL.CS.SF.Service.Implementation.Endpoint;

/// <summary>
/// Abstract base class for authorization endpoints that handle the OAuth 2.0 Authorization Code flow.
/// Contains shared logic for validating authorize requests, checking user interaction requirements
/// (login, consent), and generating authorization codes with PKCE support.
/// </summary>
public abstract class AuthorizationCodeBase
{
    protected readonly IAuthorizationService AuthorizationService;
    private readonly IRepository<Clients> clientRepository;
    private readonly TokenSettings configSettings;

    protected readonly SignInManagerWrapper<Users> CsSignInManager;
    private readonly IFrameworkResultService frameworkResultService;

    protected readonly ILoggerService LoggerService;
    private readonly IMapper mapper;
    private readonly IResourceScopeValidator resourceScopeValidator;

    protected readonly ISessionManagementService SessionService;

    /// <summary>
    /// Initializes the authorization code base with required dependencies.
    /// </summary>
    protected AuthorizationCodeBase(
        ILoggerInstance instance,
        IResourceScopeValidator resourceScopeValidator,
        IAuthorizationService authorizationService,
        IRepository<Clients> clientRepository,
        IMapper mapper,
        ISessionManagementService session,
        HCLCSSFConfig tokenSettings,
        SignInManagerWrapper<Users> csSignInManager,
        IFrameworkResultService frameworkResultService)
    {
        if (instance is null) throw new ArgumentNullException(nameof(instance));

        if (tokenSettings is null) throw new ArgumentNullException(nameof(tokenSettings));

        LoggerService = instance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
        this.resourceScopeValidator =
            resourceScopeValidator ?? throw new ArgumentNullException(nameof(resourceScopeValidator));
        AuthorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        this.clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
        this.mapper = mapper;
        configSettings = tokenSettings.TokenSettings;
        CsSignInManager = csSignInManager ?? throw new ArgumentNullException(nameof(csSignInManager));
        this.frameworkResultService = frameworkResultService;
        SessionService = session;
    }

    /// <summary>
    /// Core authorization request processing logic shared by both the authorize and callback endpoints.
    /// Validates the request parameters (client, scopes, PKCE, response type, etc.),
    /// checks whether user interaction (login) is required, and if authenticated,
    /// generates an authorization code and returns it to the client's redirect URI.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="requestCollection">The parsed request parameters (from query string or form body).</param>
    /// <returns>An <see cref="IEndpointResult"/> containing the authorize response or error.</returns>
    public virtual async Task<IEndpointResult> ProcessAuthorizeRequestAsync(
        HttpContext context,
        Dictionary<string, string> requestCollection)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));

        var request = new ValidatedAuthorizeRequestModel
        {
            RequestRawData = requestCollection,
            TokenConfigOptions = configSettings
        };

        // TODO: assignment to be confirmed during unit testing.
        request.Subject = await SessionService.GetUserPrincipalFromContextAsync();
        request.SessionId = await SessionService.GetSessionId();

        var authorizeRequest = new AuthorizeRequestSpecification(resourceScopeValidator, clientRepository, mapper,
            SessionService, configSettings);
        var authorizeValidation = await authorizeRequest.ValidateAsync(request);
        if (!authorizeRequest.IsValid)
        {
            authorizeValidation =
                frameworkResultService.Failed(authorizeValidation.ErrorCode, authorizeValidation.ErrorMessage);
            return request.Error(SessionService, authorizeValidation.ErrorCode, authorizeValidation.ErrorMessage);
        }

        // Checking user interaction required - Out of scope - Consent, custom redirect
        LoggerService.WriteTo(Log.Debug, "Check if user interaction is required ");
        var interactionResult = await AuthorizationService.CheckNavigationAsync(request);
        if (interactionResult.IsError)
            return request.Error(SessionService, interactionResult.ErrorCode, interactionResult.ErrorDescription);

        if (interactionResult.IsLogin)
        {
            var requestId = await AuthorizationService.SaveReturnUrlAsync(request);
            return new NavigationPageResult(request, configSettings, requestId);
        }

        LoggerService.WriteTo(Log.Debug, "Generating authorize code response for grant type :" + request.GrantType);
        var response = await AuthorizationService.ProcessAuthorizationCodeAsync(request);
        response.IsError = false;
        return new AuthorizeResult(response, SessionService);
    }
}
