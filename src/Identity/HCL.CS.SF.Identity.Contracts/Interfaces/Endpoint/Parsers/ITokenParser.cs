/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Http;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Parsers;

/// <summary>
/// Extracts a bearer token from an incoming HTTP request. Implementations typically
/// check the Authorization header and/or the request body for the token value.
/// </summary>
public interface ITokenParser
{
    /// <summary>
    /// Parses the HTTP context to extract the bearer token string.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>The extracted token string, or <c>null</c> if no token was found.</returns>
    Task<string> ParseAsync(HttpContext context);
}
