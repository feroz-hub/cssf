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
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Request;
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
/// Implements the OAuth 2.0 Authorization Callback Endpoint.
/// Called after the user authenticates (e.g., login page redirect). Restores the original
/// authorization request from a saved return URL, validates the user verification code,
/// and completes the authorization code flow by issuing an authorization code to the client.
/// </summary>
internal class AuthorizeCallBackEndpoint : AuthorizationCodeBase, IEndpoint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizeCallBackEndpoint"/> class.
    /// </summary>
    public AuthorizeCallBackEndpoint(
        ILoggerInstance instance,
        IResourceScopeValidator resourceScopeValidator,
        IAuthorizationService authorizationService,
        IRepository<Clients> clientRepository,
        IMapper mapper,
        ISessionManagementService session,
        HCLCSSFConfig tokenSettings,
        SignInManagerWrapper<Users> csSignInManager,
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
    /// Processes the authorization callback after user authentication.
    /// Restores the saved authorize request, validates the user verification code,
    /// and completes the authorization code flow. Cleans up the return-URL token afterward.
    /// </summary>
    /// <param name="context">The HTTP context containing the callback query parameters.</param>
    /// <returns>An <see cref="IEndpointResult"/> with the authorization code or error redirect.</returns>
    public async Task<IEndpointResult> ProcessAsync(HttpContext context)
    {
        if (!HttpMethods.IsGet(context.Request.Method))
        {
            LoggerService.WriteTo(Log.Error, "Invalid HTTP method for authorize endpoint.");
            return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
        }

        var requestCollection = context.Request.Query.ConvertCollection();
        var redirectUrlId =
            requestCollection.GetValueFromDictionary(AuthenticationConstants.AuthCodeStore.ReturnUrlCode);
        var savedUrlCollection = await AuthorizationService.ValidateReturnUrlAsync(redirectUrlId);
        if (!string.IsNullOrWhiteSpace(redirectUrlId) && savedUrlCollection == null)
        {
            LoggerService.WriteTo(Log.Warning,
                "Authorize callback could not restore the saved authorize request. " +
                $"returnUrlId={redirectUrlId}");
            var invalidRequest = new ValidatedAuthorizeRequestModel
            {
                RequestRawData = requestCollection
            };
            return invalidRequest.Error(
                SessionService,
                OpenIdConstants.Errors.InvalidRequest,
                "Authorization request context expired or is invalid. Start sign-in again.");
        }

        var shouldDeleteReturnUrl = savedUrlCollection != null;
        if (savedUrlCollection != null)
        {
            requestCollection = savedUrlCollection;
            var actualUser = await SessionService.GetUserPrincipalFromContextAsync();
            var actualUserName = actualUser?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(actualUserName))
                LoggerService.WriteTo(Log.Warning,
                    "Unable to resolve authenticated user in authorize callback. Falling back to normal authorize navigation.");

            var verificationCode = context.Request.Query
                .ConvertCollection()
                .GetValueFromDictionary(AuthenticationConstants.AuthCodeStore.UserVerificationCode);
            if (!string.IsNullOrWhiteSpace(actualUserName) && !string.IsNullOrWhiteSpace(verificationCode))
            {
                var securityToken = await AuthorizationService.ValidateVerificationCodeAsync(verificationCode);
                if (securityToken != null
                    && string.Equals(securityToken.TokenValue, actualUserName, StringComparison.OrdinalIgnoreCase)
                    && securityToken.CreationTime.AddSeconds(securityToken.ExpiresAt) >= DateTime.UtcNow)
                    await AuthorizationService.DeleteSecurityTokenByTokenValueAsync(verificationCode);
                else
                    LoggerService.WriteTo(Log.Warning,
                        "User verification code mismatch/expired. Continuing with authenticated session.");
            }
            else
            {
                LoggerService.WriteTo(Log.Warning,
                    "User verification code missing or user session unavailable. Continuing with authorize navigation.");
            }
        }

        try
        {
            var result = await ProcessAuthorizeRequestAsync(context, requestCollection);
            LoggerService.WriteTo(Log.Debug, "End Authorize Request");
            return result;
        }
        finally
        {
            if (shouldDeleteReturnUrl && Guid.TryParse(redirectUrlId, out var returnUrlTokenId))
                try
                {
                    await AuthorizationService.DeleteSecurityTokenByIdAsync(returnUrlTokenId);
                }
                catch (Exception ex)
                {
                    LoggerService.WriteToWithCaller(Log.Warning, ex,
                        "Failed to delete return-url token in authorize callback cleanup.");
                }
        }
    }
}
