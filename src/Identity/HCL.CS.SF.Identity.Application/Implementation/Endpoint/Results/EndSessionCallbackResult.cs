/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Http;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Results;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Results;

/// <summary>
/// Endpoint result that renders the end-session callback HTML page.
/// Generates hidden iframes for each client's front-channel logout URI
/// to trigger client-side session cleanup per OpenID Connect Front-Channel Logout 1.0.
/// </summary>
internal class EndSessionCallbackResult : IEndpointResult
{
    private readonly ValidatedEndSessionCallbackRequestModel request;

    /// <summary>
    /// Initializes a new instance of the <see cref="EndSessionCallbackResult"/> class.
    /// </summary>
    public EndSessionCallbackResult(ValidatedEndSessionCallbackRequestModel request)
    {
        this.request = request;
    }

    /// <summary>
    /// Constructs the response.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>The operation result.</returns>
    public async Task ConstructResponseAsync(HttpContext context)
    {
        if (request.IsError)
        {
            context.Response.StatusCode = OpenIdConstants.HTTPStatusCodes.GetHttpStatusCode(request.ErrorCode);
        }
        else
        {
            context.Response.SetResponseNoCache();
            AddContentSecurityPolicyHeaders(context);

            var html = GenerateFrontchannelIFrameHtml();
            await context.Response.WriteHtmlResponseAsync(html);
        }
    }

    private void AddContentSecurityPolicyHeaders(HttpContext context)
    {
        if (request.TokenConfigOptions.AuthenticationConfig.RequireCspFrameSrcForSignout)
        {
            string frameSources = null;
            var origins = request.FrontChannelLogoutUrls?.Select(x => x.GetOrigin());
            if (origins.ContainsAny()) frameSources = origins.Distinct().Aggregate((x, y) => $"{x} {y}");

            // the hash matches the embedded style element being used below
            context.Response.AddStyleCspHeaders(CspLevel.Two, "sha256-u+OupXgfekP+x/f6rMdoEAspPCYUtca912isERnoEjY=",
                frameSources);
        }
    }

    private string GenerateFrontchannelIFrameHtml()
    {
        string frontchannelIFrameHtml = null;
        if (request.FrontChannelLogoutUrls.ContainsAny())
        {
            var frontchannelUrls = request.FrontChannelLogoutUrls.Select(url => $"<iframe src='{url}'></iframe>");
            frontchannelIFrameHtml = frontchannelUrls.Aggregate((x, y) => x + y);
        }

        return
            $"<!DOCTYPE html><html><style>iframe{{display:none;width:0;height:0;}}</style><body>{frontchannelIFrameHtml}</body></html>";
    }
}
