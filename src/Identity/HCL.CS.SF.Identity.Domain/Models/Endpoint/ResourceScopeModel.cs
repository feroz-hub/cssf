/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Endpoint;

/// <summary>
/// Encapsulates the scope and client context for a resource access request.
/// Used during scope validation to correlate the raw request data, requested scopes,
/// the authenticated client, and the resource owner.
/// </summary>
public class ResourceScopeModel
{
    /// <summary>The raw key-value pairs from the incoming request (e.g., form or query parameters).</summary>
    public Dictionary<string, string> RawData { get; set; }

    /// <summary>The list of scope values requested by the client.</summary>
    public List<string> RequestedScope { get; set; }

    /// <summary>The authenticated client making the resource request.</summary>
    public ClientsModel Client { get; set; }

    /// <summary>The username of the resource owner, if applicable.</summary>
    public string UserName { get; set; }
}
