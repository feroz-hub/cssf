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
/// Endpoint result that renders the JSON Web Key Set (JWKS) response per RFC 7517.
/// Returns the public signing keys wrapped in a { "keys": [...] } JSON structure.
/// </summary>
internal class JwksResult : IEndpointResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JwksResult"/> class.
    /// </summary>
    public JwksResult(List<JsonWebKeyResponseModel> webKeys, int expiry)
    {
        WebKeys = webKeys;
        Expiry = expiry;
    }

    /// <summary>
    /// Gets or sets the web keys.
    /// </summary>
    internal List<JsonWebKeyResponseModel> WebKeys { get; }

    /// <summary>
    /// Gets or sets the expiry.
    /// </summary>
    internal int Expiry { get; }

    /// <summary>
    /// Constructs the response.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>The operation result.</returns>
    public async Task ConstructResponseAsync(HttpContext context)
    {
        context.Response.SetResponseCache(Expiry);
        context.Response.StatusCode = OpenIdConstants.HTTPStatusCodes.success;
        await context.Response.WriteResponseJsonAsync(new { keys = WebKeys }, "application/json; charset=UTF-8");
    }
}
