/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.Models.Endpoint.Response;

namespace HCL.CS.SF.Domain.Models.Endpoint;

/// <summary>
/// Represents a client secret that has been extracted (parsed) from an incoming request.
/// The secret may come from the Authorization header (Basic auth), the request body (post body),
/// or another configured method. Used during client authentication at the token endpoint.
/// </summary>
public class ParsedSecretModel : ErrorResponseModel
{
    /// <summary>The client identifier extracted alongside the secret.</summary>
    public string ClientId { get; set; }

    /// <summary>The raw credential value (e.g., client secret string or certificate).</summary>
    public object Credential { get; set; }

    /// <summary>The type of secret (e.g., "SharedSecret", "X509Certificate").</summary>
    public string Type { get; set; }

    /// <summary>The method used to parse the secret from the request (e.g., BasicAuth, PostBody).</summary>
    public ParseMethods ParseMethod { get; set; }
}
