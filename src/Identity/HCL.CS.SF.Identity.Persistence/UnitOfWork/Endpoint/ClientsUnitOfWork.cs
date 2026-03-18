/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.UnitOfWork.Endpoint;

namespace HCL.CS.SF.Infrastructure.Data.UnitOfWork.Endpoint;

/// <summary>
/// Unit of work that coordinates transactional operations across OAuth client repositories:
/// clients, redirect URIs, post-logout redirect URIs, and security tokens.
/// All repositories share a single database context to ensure atomic commits.
/// </summary>
internal class ClientsUnitOfWork : BaseDispose, IClientsUnitOfWork
{
    private readonly IApplicationDbContext context;

    private IRepository<Clients> clientRepository;
    private IRepository<ClientPostLogoutRedirectUris> postLogoutRedirectUrisRepository;
    private IRepository<ClientRedirectUris> redirectUrisRepository;
    private IRepository<SecurityTokens> securityTokensRepository;

    /// <summary>
    /// Initializes a new instance of <see cref="ClientsUnitOfWork"/>.
    /// </summary>
    /// <param name="context">The application database context shared across repositories.</param>
    public ClientsUnitOfWork(IApplicationDbContext context)
    {
        this.context = context;
    }

    /// <summary>Gets the lazy-initialized client repository.</summary>
    public IRepository<Clients> ClientRepository
    {
        get
        {
            if (clientRepository != null) return clientRepository;
            clientRepository = new BaseRepository<Clients>(context);
            return clientRepository;
        }
    }

    /// <summary>Gets the lazy-initialized client redirect URIs repository.</summary>
    public IRepository<ClientRedirectUris> RedirectUrisRepository
    {
        get
        {
            if (redirectUrisRepository != null) return redirectUrisRepository;
            redirectUrisRepository = new BaseRepository<ClientRedirectUris>(context);
            return redirectUrisRepository;
        }
    }

    /// <summary>Gets the lazy-initialized client post-logout redirect URIs repository.</summary>
    public IRepository<ClientPostLogoutRedirectUris> PostLogoutRedirectUrisRepository
    {
        get
        {
            if (postLogoutRedirectUrisRepository != null) return postLogoutRedirectUrisRepository;
            postLogoutRedirectUrisRepository = new BaseRepository<ClientPostLogoutRedirectUris>(context);
            return postLogoutRedirectUrisRepository;
        }
    }

    /// <summary>Gets the lazy-initialized security tokens repository.</summary>
    public IRepository<SecurityTokens> SecurityTokensRepository
    {
        get
        {
            if (securityTokensRepository != null) return securityTokensRepository;
            securityTokensRepository = new BaseRepository<SecurityTokens>(context);
            return securityTokensRepository;
        }
    }

    /// <summary>
    /// Commits all pending client changes as a single transaction with soft-delete support.
    /// </summary>
    public async Task<FrameworkResult> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Commits all pending client changes with hard-delete (physical removal) support.
    /// </summary>
    public async Task<FrameworkResult> SaveChangesWithHardDeleteAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesWithHardDeleteAsync(cancellationToken);
    }
}
