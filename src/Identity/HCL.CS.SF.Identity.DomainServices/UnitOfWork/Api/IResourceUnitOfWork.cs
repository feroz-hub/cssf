/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Threading;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Entities.Api;

namespace HCL.CS.SF.DomainServices.UnitOfWork.Api;

/// <summary>
/// Unit of work that coordinates transactional persistence for OAuth/OIDC resource management.
/// Groups repositories for API resources, API scopes, identity resources, and their associated
/// claims under a single save boundary.
/// </summary>
public interface IResourceUnitOfWork
{
    /// <summary>Gets the repository for API resource entities.</summary>
    IRepository<ApiResources> ApiResourcesRepository { get; }

    /// <summary>Gets the repository for API resource claim entities.</summary>
    IRepository<ApiResourceClaims> ApiResourceClaimsRepository { get; }

    /// <summary>Gets the repository for API scope entities.</summary>
    IRepository<ApiScopes> ApiScopesRepository { get; }

    /// <summary>Gets the repository for API scope claim entities.</summary>
    IRepository<ApiScopeClaims> ApiScopeClaimsRepository { get; }

    /// <summary>Gets the repository for identity resource entities (e.g., openid, profile).</summary>
    IRepository<IdentityResources> IdentityResourcesRepository { get; }

    /// <summary>Gets the repository for identity resource claim entities.</summary>
    IRepository<IdentityClaims> IdentityClaimsRepository { get; }

    /// <summary>Persists all pending changes across all resource repositories using soft-delete semantics.</summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<FrameworkResult> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Persists all pending changes using hard-delete semantics (physical removal).</summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<FrameworkResult> SaveChangesWithHardDeleteAsync(CancellationToken cancellationToken = default);
}
