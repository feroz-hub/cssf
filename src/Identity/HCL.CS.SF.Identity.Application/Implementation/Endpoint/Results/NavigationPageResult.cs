/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Http;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.Service.Extension;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Results;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Results;

/// <summary>
/// Endpoint result that redirects the user to the login page when authentication is required.
/// Constructs the return URL (pointing back to the authorize callback endpoint)
/// and appends it as a query parameter to the configured login URL.
/// </summary>
internal class NavigationPageResult : IEndpointResult
{
    private readonly TokenSettings configSettings;
    private readonly Guid returnUrlId;

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationPageResult"/> class.
    /// </summary>
    public NavigationPageResult(ValidatedAuthorizeRequestModel request, TokenSettings tokenConfig, Guid returnUrlId)
    {
        AuthorizeRequest = request;
        configSettings = tokenConfig;
        this.returnUrlId = returnUrlId;
    }

    private ValidatedAuthorizeRequestModel AuthorizeRequest { get; }

    /// <summary>
    /// Constructs the response.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>The operation result.</returns>
    public Task ConstructResponseAsync(HttpContext context)
    {
        var returnUrl = "/" + OpenIdConstants.EndpointRoutePaths.AuthorizeCallback;
        if (returnUrlId.IsValid())
            returnUrl = returnUrl.AddQueryString(AuthenticationConstants.AuthCodeStore.ReturnUrlCode,
                Convert.ToString(returnUrlId));
        else
            returnUrl = returnUrl.FormatQueryString(AuthorizeRequest.RequestRawData.PrepareQueryString());

        var loginUrl = configSettings.UserInteractionConfig.LoginUrl;
        if (!loginUrl.CheckLocalUrl())
            // this converts the relative redirect path to an absolute one if we're
            // redirecting to a different server
            returnUrl = context.GetHCLCSSFHost().IncludeEndSlash() + returnUrl.RemoveFrontSlash();

        var url = loginUrl.AddQueryString(configSettings.UserInteractionConfig.LoginReturnUrlParameter, returnUrl);
        context.Response.RedirectToAbsoluteUrl(url);
        return Task.CompletedTask;
    }
}
