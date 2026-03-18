/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Http;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Response;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Results;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Results;

/// <summary>
/// Endpoint result that renders the OAuth 2.0 token response as JSON.
/// Includes access_token, id_token, refresh_token, token_type, expires_in, and scope fields.
/// </summary>
public class TokenResult : IEndpointResult
{
    /// <summary>Initializes a new token result with the specified token response model.</summary>
    /// <param name="response">The token response containing access, identity, and refresh tokens.</param>
    public TokenResult(TokenResponseModel response)
    {
        Response = response;
    }

    /// <summary>Gets or sets the token response model containing the issued tokens.</summary>
    internal TokenResponseModel Response { get; set; }

    /// <summary>
    /// Writes the token response as JSON with no-cache headers per OAuth 2.0 Section 5.1.
    /// </summary>
    /// <param name="context">The HTTP context to write the response to.</param>
    public async Task ConstructResponseAsync(HttpContext context)
    {
        context.Response.SetResponseNoCache();

        var tokenResponseResultModel = new TokenResponseResultModel
        {
            id_token = Response.IdentityToken,
            access_token = Response.AccessToken,
            refresh_token = Response.RefreshToken,
            expires_in = Response.AccessTokenExpiresIn,
            token_type = OpenIdConstants.TokenResponseType.BearerTokenType,
            scope = Response.Scope
        };
        //context.Response.StatusCode = OpenIdConstants.HTTPStatusCodes.success;
        await context.Response.WriteResponseJsonAsync(tokenResponseResultModel);
    }
}
