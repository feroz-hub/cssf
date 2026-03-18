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
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.Domain.Models.Endpoint.Response;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;
using static HCL.CS.SF.Domain.Constants.Endpoint.AuthenticationConstants;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Services;

/// <summary>
/// Service that manages user interaction contexts during the OAuth 2.0 / OIDC flow.
/// Provides methods to retrieve error contexts, logout contexts, and construct
/// user verification code URLs for the authorize callback flow.
/// </summary>
internal class InteractionService : SecurityBase, IInteractionService
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ILoggerService loggerService;
    private readonly ISessionManagementService session;

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractionService"/> class.
    /// </summary>
    public InteractionService(
        ILoggerInstance instance,
        IHttpContextAccessor httpContextAccessor,
        ISessionManagementService session)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.session = session;
        loggerService = instance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
    }

    /// <summary>
    /// Retrieves and decrypts the error context from a data-protected error ID.
    /// Used by the error page to display authorization error details to the user.
    /// </summary>
    /// <param name="errorId">The data-protected error identifier.</param>
    /// <returns>The deserialized error response model, or null if errorId is empty.</returns>
    public async Task<ErrorResponseModel> GetErrorContextAsync(string errorId)
    {
        if (!string.IsNullOrWhiteSpace(errorId))
        {
            var result = await errorId.UnProtectDataAsync<ErrorResponseModel>();
            return result;
        }

        return null;
    }

    /// <summary>
    /// Retrieves the logout context from a data-protected logout ID.
    /// Builds the end-session callback URL with front-channel logout iframe sources.
    /// </summary>
    /// <param name="logoutId">The data-protected logout identifier.</param>
    /// <returns>A <see cref="LogoutRequestModel"/> containing the logout context and callback URL.</returns>
    public async Task<LogoutRequestModel> GetLogoutContextAsync(string logoutId)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(logoutId))
            {
                loggerService.WriteTo(Log.Debug, "Entered into get logout context.");
                var logoutMessageModel = await logoutId.UnProtectDataAsync<LogoutMessageModel>();
                var endSessionCallBackUrl =
                    await GetEndSessionCallbackUrlAsync(httpContextAccessor.HttpContext, logoutMessageModel);
                return new LogoutRequestModel(endSessionCallBackUrl, logoutMessageModel);
            }
            return null;
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Constructs a return URL with an appended user verification code query parameter.
    /// Used to link a verified user identity to the authorize callback flow.
    /// </summary>
    /// <param name="returnUrl">The base return URL for the authorize callback.</param>
    /// <param name="userVerficationCode">The user verification code to append.</param>
    /// <returns>The return URL with the verification code appended, or "/" if no URL is provided.</returns>
    public Task<string> ConstructUserVerificationCode(string returnUrl, string userVerficationCode)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            if (!string.IsNullOrWhiteSpace(userVerficationCode))
            {
                loggerService.WriteTo(Log.Debug, "Entered into construct user verification code");
                return Task.FromResult(returnUrl.AddQueryString(AuthCodeStore.UserVerificationCode, userVerficationCode));
            }

            return Task.FromResult(returnUrl);
        }

        return Task.FromResult("/");
    }

    private async Task<string> GetEndSessionCallbackUrlAsync(HttpContext context,
        LogoutMessageModel logoutMessage = null)
    {
        var user = await session.GetUserPrincipalFromContextAsync();
        var contextSubjectId = user?.GetSubjectId();
        LogoutMessageModel messageModel = null;

        if (logoutMessage?.ClientIdCollection?.ContainsAny() == true)
        {
            var clientIdCollection = logoutMessage?.ClientIdCollection;
            if (contextSubjectId == logoutMessage?.SubjectId)
            {
                var clientList = await session.GetClientListAsync();
                clientIdCollection = clientIdCollection.Union(clientList);
                clientIdCollection = clientIdCollection.Distinct();
            }

            messageModel = new LogoutMessageModel
            {
                SubjectId = logoutMessage.SubjectId,
                SessionId = logoutMessage.SessionId,
                ClientIdCollection = clientIdCollection
            };
        }
        else if (contextSubjectId != null)
        {
            var clientIdCollection = await session.GetClientListAsync();
            if (clientIdCollection.ContainsAny())
                messageModel = new LogoutMessageModel
                {
                    SubjectId = logoutMessage?.SubjectId,
                    SessionId = await session.GetSessionId(),
                    ClientIdCollection = clientIdCollection
                };
        }

        if (messageModel != null)
        {
            var logoutId = await messageModel.ProtectDataAsync();
            var endSessionCallbackUrl = context.GetHCLCSSFBaseUrl().IncludeEndSlash() +
                                        OpenIdConstants.EndpointRoutePaths.EndSessionCallback;
            endSessionCallbackUrl =
                endSessionCallbackUrl.AddQueryString(ApplicationUIConstants.DefaultRoutePathParams.EndSessionCallback,
                    logoutId);
            return endSessionCallbackUrl;
        }

        return null;
    }
}
