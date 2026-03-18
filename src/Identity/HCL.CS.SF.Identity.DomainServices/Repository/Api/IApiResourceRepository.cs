/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Threading;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Models.Api;

namespace HCL.CS.SF.DomainServices.Repository.Api;

/// <summary>
/// Repository contract for API resource and API scope persistence.
/// Extends <see cref="IRepository{TEntity}"/> with specialized queries for loading resources
/// by name, by ID (with or without child entities), and by requested scopes during token issuance.
/// </summary>
public interface IApiResourceRepository : IRepository<ApiResources>
{
    /// <summary>Load resource only (no scopes/claims). Use for update to avoid child RowVersion concurrency.</summary>
    /// <param name="apiResourceId">The unique identifier of the API resource.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<ApiResources> GetApiResourceForUpdateAsync(Guid apiResourceId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves an API resource by its unique identifier, including related scopes and claims.</summary>
    /// <param name="apiResourceId">The unique identifier of the API resource.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<ApiResources> GetApiResourcesAsync(Guid apiResourceId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves an API resource by its name, including related scopes and claims.</summary>
    /// <param name="apiResourceName">The name of the API resource.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<ApiResources> GetApiResourcesAsync(string apiResourceName, CancellationToken cancellationToken = default);

    /// <summary>Retrieves all registered API resources with their scopes and claims.</summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<IList<ApiResources>> GetAllApiResourcesAsync(CancellationToken cancellationToken = default);

    /// <summary>Retrieves all registered API scopes.</summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<IList<ApiScopes>> GetAllApiScopesAsync(CancellationToken cancellationToken = default);

    /// <summary>Retrieves API resources that contain scopes matching the requested scope list.</summary>
    /// <param name="requestedScopes">The scope names requested by the client.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<IList<ApiResourcesByScopesModel>> GetAllApiResourcesByScopesAsync(IList<string> requestedScopes, CancellationToken cancellationToken = default);
}
