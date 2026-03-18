/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Threading;
using Microsoft.EntityFrameworkCore;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Repository.Api;

namespace HCL.CS.SF.Infrastructure.Data.Repository.Api;

/// <summary>
/// Repository for API resource data operations. Provides methods to retrieve API resources
/// with their associated claims and scopes, look up resources by name or ID, and query
/// resources by requested OAuth scopes for token generation.
/// </summary>
internal class ApiResourceRepository : BaseRepository<ApiResources>, IApiResourceRepository
{
    private readonly IApplicationDbContext context;

    /// <summary>
    /// Initializes a new instance of <see cref="ApiResourceRepository"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public ApiResourceRepository(IApplicationDbContext context)
        : base(context)
    {
        this.context = context;
    }

    /// <summary>
    /// Retrieves a tracked API resource entity by ID for update operations (without includes).
    /// </summary>
    /// <param name="apiResourceId">The ID of the API resource.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The API resource entity, or null if not found.</returns>
    public async Task<ApiResources> GetApiResourceForUpdateAsync(Guid apiResourceId, CancellationToken cancellationToken = default)
    {
        return await context.ApiResources
            .FirstOrDefaultAsync(apires => apires.Id == apiResourceId, cancellationToken);
    }

    /// <summary>
    /// Retrieves an API resource by ID with all related claims, scopes, and scope claims eagerly loaded.
    /// Uses split queries to avoid Cartesian product explosion.
    /// </summary>
    /// <param name="apiResourceId">The ID of the API resource.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The API resource with navigation properties, or null if not found.</returns>
    public async Task<ApiResources> GetApiResourcesAsync(Guid apiResourceId, CancellationToken cancellationToken = default)
    {
        var apiResources = await context.ApiResources
            .Include(ac => ac.ApiResourceClaims)
            .Include(asc => asc.ApiScopes)
            .ThenInclude(asc => asc.ApiScopeClaims)
            .AsSplitQuery()
            .FirstOrDefaultAsync(apires => apires.Id == apiResourceId, cancellationToken);
        return apiResources;
    }

    /// <summary>
    /// Retrieves an API resource by name with all related claims, scopes, and scope claims eagerly loaded.
    /// </summary>
    /// <param name="apiResourceName">The unique name of the API resource.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The API resource with navigation properties, or null if not found.</returns>
    public async Task<ApiResources> GetApiResourcesAsync(string apiResourceName, CancellationToken cancellationToken = default)
    {
        var apiResources = await context.ApiResources
            .Include(ac => ac.ApiResourceClaims)
            .Include(asc => asc.ApiScopes)
            .ThenInclude(asc => asc.ApiScopeClaims)
            .AsSplitQuery()
            .FirstOrDefaultAsync(api => api.Name == apiResourceName, cancellationToken);
        return apiResources;
    }

    /// <summary>
    /// Retrieves all API resources with their claims, scopes, and scope claims as a no-tracking read.
    /// </summary>
    public async Task<IList<ApiResources>> GetAllApiResourcesAsync(CancellationToken cancellationToken = default)
    {
        var apiResources = await context.ApiResources
            .Include(ac => ac.ApiResourceClaims)
            .Include(asc => asc.ApiScopes)
            .ThenInclude(asc => asc.ApiScopeClaims)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return apiResources;
    }

    /// <summary>
    /// Retrieves all API scopes with their associated scope claims as a no-tracking read.
    /// </summary>
    public async Task<IList<ApiScopes>> GetAllApiScopesAsync(CancellationToken cancellationToken = default)
    {
        var apiScopes = await context.ApiScopes
            .Include(ac => ac.ApiScopeClaims)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return apiScopes;
    }

    /// <summary>
    /// Retrieves a flattened projection of API resources, their claims, scopes, and scope claims
    /// filtered by the requested OAuth scope names. Used during token generation to determine
    /// which claims to include in access tokens.
    /// </summary>
    /// <param name="requestedScopes">The list of requested scope names to match against.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A flat list of API resource/scope/claim combinations matching the requested scopes.</returns>
    public async Task<IList<ApiResourcesByScopesModel>> GetAllApiResourcesByScopesAsync(IList<string> requestedScopes, CancellationToken cancellationToken = default)
    {
        // Multi-join query: ApiResources -> ApiResourceClaims, ApiScopes -> ApiScopeClaims
        // Uses left joins (DefaultIfEmpty) so resources without claims or scopes are still included
        var apiResources = await (from apiRes in context.ApiResources.AsNoTracking()
            join apiResClaims in context.ApiResourceClaims.AsNoTracking()
                on apiRes.Id equals apiResClaims.ApiResourceId into apiResClaimJoin
            from apiResModel in apiResClaimJoin.DefaultIfEmpty()
            join apiScopes in context.ApiScopes.AsNoTracking()
                on apiRes.Id equals apiScopes.ApiResourceId into apiScopesJoin
            from apiScopeModel in apiScopesJoin.DefaultIfEmpty()
            where requestedScopes.Contains(apiScopeModel.Name) || requestedScopes.Contains(apiRes.Name)
            join apiScopeClaims in context.ApiScopeClaims.AsNoTracking()
                on apiScopeModel.Id equals apiScopeClaims.ApiScopeId into apiScopeClaimJoin
            from apiModel in apiScopeClaimJoin.DefaultIfEmpty()
            select new ApiResourcesByScopesModel
            {
                ApiResourceId = apiRes.Id,
                ApiResourceName = apiRes.Name,
                ApiResourceClaimType = apiResModel.Type,
                ApiScopeName = apiScopeModel.Name,
                ApiScopeClaimType = apiModel.Type
            }).ToListAsync(cancellationToken);

        return apiResources;
    }
}
