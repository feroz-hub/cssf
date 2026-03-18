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
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.DomainServices.Wrappers;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Implementation.Endpoint.Results;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Results;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Validators;

namespace HCL.CS.SF.Service.Implementation.Endpoint;

/// <summary>
/// Implements the OAuth 2.0 Authorization Endpoint (RFC 6749 Section 3.1).
/// Handles both GET and POST requests from the user-agent to initiate the authorization code flow.
/// Validates the client, redirect URI, scopes, PKCE parameters, and response type,
/// then either redirects to the login page or issues an authorization code.
/// </summary>
internal class AuthorizeEndpoint : AuthorizationCodeBase, IEndpoint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizeEndpoint"/> class.
    /// </summary>
    public AuthorizeEndpoint(
        ILoggerInstance instance,
        IResourceScopeValidator resourceScopeValidator,
        IAuthorizationService authorizationService,
        IRepository<Clients> clientRepository,
        IMapper mapper,
        ISessionManagementService session,
        SignInManagerWrapper<Users> csSignInManager,
        HCLCSSFConfig tokenSettings,
        IFrameworkResultService frameworkResultService)
        : base(
            instance,
            resourceScopeValidator,
            authorizationService,
            clientRepository,
            mapper,
            session,
            tokenSettings,
            csSignInManager,
            frameworkResultService)
    {
    }

    /// <summary>
    /// Processes an OAuth 2.0 authorization request. Accepts GET (query params) or POST (form body).
    /// Delegates to <see cref="AuthorizationCodeBase.ProcessAuthorizeRequestAsync"/> for core logic.
    /// </summary>
    /// <param name="context">The HTTP context containing the authorization request.</param>
    /// <returns>An <see cref="IEndpointResult"/> - either a redirect to login, an authorization code response, or an error.</returns>
    public async Task<IEndpointResult> ProcessAsync(HttpContext context)
    {
        Dictionary<string, string> requestCollection;
        if (HttpMethods.IsGet(context.Request.Method))
        {
            requestCollection = context.Request.Query.ConvertCollection();
        }
        else if (HttpMethods.IsPost(context.Request.Method))
        {
            if (!context.Request.CheckHeaderContentType())
                return new StatusCodeResult(HttpStatusCode.UnsupportedMediaType);

            requestCollection = context.Request.Form.ConvertCollection();
        }
        else
        {
            return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
        }

        var result = await ProcessAuthorizeRequestAsync(context, requestCollection);
        LoggerService.WriteTo(Log.Debug, "End of authorize request endpoint");

        return result;
    }
}
