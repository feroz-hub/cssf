/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Endpoint.Request;

/// <summary>
/// Request model for the OIDC Discovery endpoint (/.well-known/openid-configuration).
/// Contains the base URL needed to construct the absolute URIs for all advertised endpoints.
/// </summary>
public class DiscoveryRequestModel
{
    /// <summary>The base URL of the identity server used to build absolute endpoint URIs in the discovery document.</summary>
    public virtual string BaseUrl { get; set; }
}
