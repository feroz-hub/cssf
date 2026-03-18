/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Models.Endpoint.Request;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;

/// <summary>
/// Generates the OIDC discovery document (/.well-known/openid-configuration) containing
/// the authorization server's endpoints, supported grant types, signing algorithms, and scopes.
/// </summary>
public interface IDiscoveryService
{
    /// <summary>
    /// Generates the discovery metadata dictionary for the given request context.
    /// </summary>
    /// <param name="request">The discovery request containing the issuer URL and base path.</param>
    /// <returns>A dictionary of metadata entries to be serialized as the discovery JSON document.</returns>
    Task<Dictionary<string, object>> GenerateDiscoveryMetaData(DiscoveryRequestModel request);
}
