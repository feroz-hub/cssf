/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Models.Endpoint;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Validators;

/// <summary>
/// Validates that the scopes requested by a client are allowed and correspond to registered
/// API resources and identity resources. Used during authorize and token request validation.
/// </summary>
public interface IResourceScopeValidator
{
    /// <summary>
    /// Checks that the requested scopes are a subset of the scopes allowed for the client.
    /// </summary>
    /// <param name="clientScopes">The scopes the client is permitted to request.</param>
    /// <param name="requestedScopes">The scopes actually requested.</param>
    /// <returns><c>true</c> if all requested scopes are allowed for the client.</returns>
    Task<bool> ValidateRequestedScopeWithClientAsync(IList<string> clientScopes, IList<string> requestedScopes);

    /// <summary>
    /// Validates the requested scopes against registered resources and returns the parsed scope details.
    /// </summary>
    /// <param name="resourceScopeModel">The resource scope data to validate.</param>
    /// <returns>An <see cref="AllowedScopesParserModel"/> containing the validated scope breakdown.</returns>
    Task<AllowedScopesParserModel> ValidateRequestedScopesAsync(ResourceScopeModel resourceScopeModel);
}
