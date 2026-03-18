/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Http;
using HCL.CS.SF.Domain.Models.Endpoint;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Parsers;

/// <summary>
/// Extracts client credentials (client ID and secret) from an incoming HTTP request.
/// Implementations support various authentication methods such as HTTP Basic authentication
/// and form-post body parameters as defined by the OAuth 2.0 specification.
/// </summary>
public interface IClientSecretParser
{
    /// <summary>
    /// Parses the HTTP context to extract the client credentials.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A <see cref="ParsedSecretModel"/> containing the client ID and credential, or <c>null</c> if not found.</returns>
    Task<ParsedSecretModel> ParseAsync(HttpContext context);
}
