/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.DomainServices.Wrappers;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Implementation.Endpoint.Results;
using HCL.CS.SF.Service.Implementation.Endpoint.Specifications;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Parsers;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Results;

namespace HCL.CS.SF.Service.Implementation.Endpoint;

/// <summary>
/// Implements the OpenID Connect UserInfo Endpoint (OpenID Connect Core 1.0 Section 5.3).
/// Returns claims about the authenticated user based on the scopes granted in the access token.
/// The access token is provided via the Authorization header (Bearer) or POST body.
/// </summary>
internal class UserInfoEndpoint : IEndpoint
{
    private readonly IRepository<Clients> clientRepository;
    private readonly TokenSettings configSettings;
    private readonly IFrameworkResultService frameworkResultService;
    private readonly Dictionary<string, AsymmetricKeyInfoModel> keyStore;
    private readonly ILoggerService loggerService;
    private readonly IMapper mapper;
    private readonly IRepository<SecurityTokens> securityTokenRepository;
    private readonly ITokenParser tokenParser;
    private readonly IUserInfoServices userInfoServices;
    private readonly UserManagerWrapper<Users> userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserInfoEndpoint"/> class.
    /// </summary>
    public UserInfoEndpoint(
        ILoggerInstance instance,
        ITokenParser tokenParser,
        IRepository<Clients> clientRepository,
        IMapper mapper,
        UserManagerWrapper<Users> userManager,
        Dictionary<string, AsymmetricKeyInfoModel> keyStore,
        IUserInfoServices userInfoServices,
        IFrameworkResultService frameworkResultService,
        HCLCSSFConfig settings,
        IRepository<SecurityTokens> securityTokenRepository)
    {
        this.tokenParser = tokenParser;
        this.clientRepository = clientRepository;
        this.mapper = mapper;
        this.userManager = userManager;
        this.keyStore = keyStore ?? throw new ArgumentNullException(nameof(keyStore));
        this.userInfoServices = userInfoServices;
        configSettings = settings.TokenSettings;
        this.frameworkResultService = frameworkResultService;
        this.securityTokenRepository = securityTokenRepository;
        loggerService = instance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
    }

    /// <summary>
    /// Processes a UserInfo request. Extracts and validates the access token,
    /// verifies the client and user claims, then returns the user's identity claims
    /// filtered by the scopes present in the token.
    /// </summary>
    /// <param name="context">The HTTP context containing the UserInfo request.</param>
    /// <returns>A <see cref="UserInfoResult"/> with user claims or an error.</returns>
    public async Task<IEndpointResult> ProcessAsync(HttpContext context)
    {
        loggerService.WriteTo(Log.Debug, "Processing user information request.");

        if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsPost(context.Request.Method))
        {
            loggerService.WriteTo(Log.Debug, "Invalid HTTP request for userinfo endpoint.");
            return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
        }

        // validate access token
        var token = await tokenParser.ParseAsync(context);
        if (string.IsNullOrWhiteSpace(token))
            return OpenIdConstants.Errors.InvalidToken.UserInfoError("No access token found.");

        // validate request
        var userInfoRequestValidation = new ValidatedUserInfoRequestModel
        {
            Token = token,
            TokenConfigOptions = configSettings
        };

        var requestValidator = new UserInfoRequestSpecification(clientRepository, mapper, keyStore, userManager,
            configSettings, securityTokenRepository);
        var validationResult = await requestValidator.ValidateAsync(userInfoRequestValidation);
        if (!requestValidator.IsValid)
        {
            validationResult = frameworkResultService.Failed(validationResult.ErrorCode, validationResult.ErrorMessage);
            return validationResult.ErrorCode.UserInfoError(validationResult.ErrorMessage);
        }

        // create response
        var collection = await userInfoServices.ProcessUserInfoAsync(userInfoRequestValidation);

        // return result
        return new UserInfoResult(collection, null);
    }
}
