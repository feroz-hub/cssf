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
/// Repository for identity resource data operations. Provides methods to retrieve OIDC identity
/// resources (openid, profile, email, etc.) with their associated claims, by ID, name, or
/// filtered by requested scopes for token generation.
/// </summary>
internal class IdentityResourceRepository : BaseRepository<IdentityResources>, IIdentityResourceRepository
{
    private readonly IApplicationDbContext context;

    /// <summary>
    /// Initializes a new instance of <see cref="IdentityResourceRepository"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public IdentityResourceRepository(IApplicationDbContext context)
        : base(context)
    {
        this.context = context;
    }

    /// <summary>
    /// Retrieves an identity resource by ID with its associated identity claims eagerly loaded.
    /// </summary>
    public async Task<IdentityResources> GetIdentityResourcesAsync(Guid identityResourceId, CancellationToken cancellationToken = default)
    {
        var identityResources = await context.IdentityResources
            .Include(ac => ac.IdentityClaims)
            .FirstOrDefaultAsync(res => res.Id == identityResourceId, cancellationToken);
        return identityResources;
    }

    /// <summary>
    /// Retrieves an identity resource by name with its associated identity claims eagerly loaded.
    /// </summary>
    public async Task<IdentityResources> GetIdentityResourcesAsync(string identityResourceName, CancellationToken cancellationToken = default)
    {
        var identityResources = await context.IdentityResources
            .Include(ac => ac.IdentityClaims)
            .FirstOrDefaultAsync(res => res.Name == identityResourceName, cancellationToken);
        return identityResources;
    }

    /// <summary>
    /// Retrieves a flattened projection of identity resources and their claims filtered by
    /// the requested scope names. Used during ID token generation to determine which identity
    /// claims to include.
    /// </summary>
    /// <param name="requestedScopes">The list of requested scope names to match against resource names.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A flat list of identity resource/claim combinations matching the requested scopes.</returns>
    public async Task<IList<IdentityResourcesByScopesModel>> GetAllIdentityResourcesByScopesAsync(
        IList<string> requestedScopes, CancellationToken cancellationToken = default)
    {
        // Left join identity resources to their claims so resources with no claims are still returned
        var identityResources = await (from idRes in context.IdentityResources.AsNoTracking()
            where requestedScopes.Contains(idRes.Name)
            join idResClaims in context.IdentityClaims.AsNoTracking()
                on idRes.Id equals idResClaims.IdentityResourceId into idModelJoin
            from idModel in idModelJoin.DefaultIfEmpty()
            select new IdentityResourcesByScopesModel
            {
                IdentityResourceId = idRes.Id,
                IdentityResourceName = idRes.Name,
                IdentityResourceClaimType = idModel.Type,
                IdentityResourceClaimAliasType = idModel.AliasType
            }).ToListAsync(cancellationToken);

        return identityResources;
    }
}
