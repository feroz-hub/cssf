/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.UnitOfWork.Api;

namespace HCL.CS.SF.Infrastructure.Data.UnitOfWork.Api;

/// <summary>
/// Unit of work that coordinates transactional operations across identity resource and API resource
/// repositories. Provides lazy-initialized repositories for IdentityResources, IdentityClaims,
/// ApiResources, ApiResourceClaims, ApiScopes, and ApiScopeClaims, all sharing a single
/// database context to ensure atomic commits.
/// </summary>
internal class ResourceUnitOfWork : BaseDispose, IResourceUnitOfWork
{
    private readonly IApplicationDbContext context;
    private IRepository<ApiResourceClaims> apiClaimsRepository;
    private IRepository<ApiResources> apiResourcesRepository;
    private IRepository<ApiScopeClaims> apiScopeClaimsRepository;
    private IRepository<ApiScopes> apiScopesRepository;
    private IRepository<IdentityClaims> identityClaimsRepository;

    private IRepository<IdentityResources> identityResourcesRepository;

    /// <summary>
    /// Initializes a new instance of <see cref="ResourceUnitOfWork"/> with the given database context.
    /// </summary>
    /// <param name="context">The application database context shared across all repositories.</param>
    public ResourceUnitOfWork(IApplicationDbContext context)
    {
        this.context = context;
    }

    /// <summary>Gets the lazy-initialized repository for identity resources.</summary>
    public IRepository<IdentityResources> IdentityResourcesRepository
    {
        get
        {
            if (identityResourcesRepository != null) return identityResourcesRepository;
            identityResourcesRepository = new BaseRepository<IdentityResources>(context);
            return identityResourcesRepository;
        }
    }

    /// <summary>Gets the lazy-initialized repository for identity claims.</summary>
    public IRepository<IdentityClaims> IdentityClaimsRepository
    {
        get
        {
            if (identityClaimsRepository != null) return identityClaimsRepository;
            identityClaimsRepository = new BaseRepository<IdentityClaims>(context);
            return identityClaimsRepository;
        }
    }

    /// <summary>Gets the lazy-initialized repository for API resources.</summary>
    public IRepository<ApiResources> ApiResourcesRepository
    {
        get
        {
            if (apiResourcesRepository != null) return apiResourcesRepository;
            apiResourcesRepository = new BaseRepository<ApiResources>(context);
            return apiResourcesRepository;
        }
    }

    /// <summary>Gets the lazy-initialized repository for API resource claims.</summary>
    public IRepository<ApiResourceClaims> ApiResourceClaimsRepository
    {
        get
        {
            if (apiClaimsRepository != null) return apiClaimsRepository;
            apiClaimsRepository = new BaseRepository<ApiResourceClaims>(context);
            return apiClaimsRepository;
        }
    }

    /// <summary>Gets the lazy-initialized repository for API scopes.</summary>
    public IRepository<ApiScopes> ApiScopesRepository
    {
        get
        {
            if (apiScopesRepository != null) return apiScopesRepository;
            apiScopesRepository = new BaseRepository<ApiScopes>(context);
            return apiScopesRepository;
        }
    }

    /// <summary>Gets the lazy-initialized repository for API scope claims.</summary>
    public IRepository<ApiScopeClaims> ApiScopeClaimsRepository
    {
        get
        {
            if (apiScopeClaimsRepository != null) return apiScopeClaimsRepository;
            apiScopeClaimsRepository = new BaseRepository<ApiScopeClaims>(context);
            return apiScopeClaimsRepository;
        }
    }

    /// <summary>
    /// Commits all pending resource changes as a single transaction with soft-delete support.
    /// </summary>
    public async Task<FrameworkResult> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Commits all pending resource changes with hard-delete (physical removal) support.
    /// </summary>
    public async Task<FrameworkResult> SaveChangesWithHardDeleteAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesWithHardDeleteAsync(cancellationToken);
    }
}
