/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Threading;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Entities.Endpoint;

namespace HCL.CS.SF.DomainServices.UnitOfWork.Endpoint;

/// <summary>
/// Unit of work that coordinates transactional persistence across client-related repositories.
/// Groups the client entity, its redirect URIs, post-logout redirect URIs, and security tokens
/// under a single save boundary to ensure data consistency.
/// </summary>
public interface IClientsUnitOfWork
{
    /// <summary>Gets the repository for OAuth/OIDC client entities.</summary>
    IRepository<Clients> ClientRepository { get; }

    /// <summary>Gets the repository for client post-logout redirect URIs.</summary>
    IRepository<ClientPostLogoutRedirectUris> PostLogoutRedirectUrisRepository { get; }

    /// <summary>Gets the repository for client redirect URIs.</summary>
    IRepository<ClientRedirectUris> RedirectUrisRepository { get; }

    /// <summary>Gets the repository for persisted security tokens.</summary>
    IRepository<SecurityTokens> SecurityTokensRepository { get; }

    /// <summary>Persists all pending changes across all repositories using soft-delete semantics.</summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<FrameworkResult> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Persists all pending changes using hard-delete semantics (physical removal).</summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<FrameworkResult> SaveChangesWithHardDeleteAsync(CancellationToken cancellationToken = default);
}
