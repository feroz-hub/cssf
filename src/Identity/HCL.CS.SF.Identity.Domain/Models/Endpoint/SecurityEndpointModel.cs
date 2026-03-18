/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Http;

namespace HCL.CS.SF.Domain.Models.Endpoint;

/// <summary>
/// Maps an OAuth 2.0 / OIDC endpoint (e.g., authorize, token, userinfo, revocation)
/// to its URL path and the handler type responsible for processing requests at that path.
/// Used during middleware registration to route incoming requests to the correct endpoint handler.
/// </summary>
public class SecurityEndpointModel
{
    /// <summary>
    /// Initializes a new endpoint mapping with the given name, path, and handler type.
    /// </summary>
    /// <param name="name">The logical name of the endpoint (e.g., "Authorize", "Token").</param>
    /// <param name="path">The URL path for the endpoint (e.g., "/connect/authorize").</param>
    /// <param name="handlerType">The type of the handler class that processes requests at this endpoint.</param>
    public SecurityEndpointModel(string name, string path, Type handlerType)
    {
        Name = name;
        Path = path;
        Handler = handlerType;
    }

    /// <summary>The URL path where this endpoint is accessible.</summary>
    public PathString Path { get; set; }

    /// <summary>The logical name of this endpoint for identification and logging.</summary>
    public string Name { get; set; }

    /// <summary>The type of the handler class that processes requests at this endpoint.</summary>
    public Type Handler { get; set; }
}
