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
/// Repository contract for OIDC identity resource operations (e.g., openid, profile, email).
/// Extends <see cref="IRepository{TEntity}"/> with look-ups by name and scope-based queries
/// used during token issuance to determine which claims to include.
/// </summary>
public interface IIdentityResourceRepository : IRepository<IdentityResources>
{
    /// <summary>Retrieves an identity resource by its unique identifier, including related claims.</summary>
    /// <param name="identityResourceId">The unique identifier of the identity resource.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<IdentityResources> GetIdentityResourcesAsync(Guid identityResourceId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves an identity resource by its name (e.g., "openid", "profile").</summary>
    /// <param name="identityResourceName">The name of the identity resource.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<IdentityResources> GetIdentityResourcesAsync(string identityResourceName, CancellationToken cancellationToken = default);

    /// <summary>Retrieves all identity resources that match the requested scopes, used during scope validation.</summary>
    /// <param name="requestScopes">The list of scope names requested by the client.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<IList<IdentityResourcesByScopesModel>> GetAllIdentityResourcesByScopesAsync(IList<string> requestScopes, CancellationToken cancellationToken = default);
}
