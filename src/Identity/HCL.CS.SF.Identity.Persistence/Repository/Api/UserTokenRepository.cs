/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Threading;
using Microsoft.EntityFrameworkCore;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Repository.Api;

namespace HCL.CS.SF.Infrastructure.Data.Repository.Api;

/// <summary>
/// Repository for user token data operations. Provides methods to delete user tokens
/// and query tokens by user ID, name, and login provider.
/// </summary>
internal class UserTokenRepository : BaseDispose, IUserTokenRepository
{
    private readonly IApplicationDbContext context;

    /// <summary>
    /// Initializes a new instance of <see cref="UserTokenRepository"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public UserTokenRepository(IApplicationDbContext context)
    {
        this.context = context;
    }

    /// <summary>
    /// Deletes a list of user token entities, marking each as deleted before removal.
    /// </summary>
    public Task DeleteAsync(IList<UserTokens> entityList)
    {
        foreach (var entity in entityList)
        {
            entity.IsDeleted = true;
            context.UserTokens.Remove(entity);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves all tokens for a specific user.
    /// </summary>
    public async Task<IList<UserTokens>> GetUserTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userToken = await context.UserTokens.Where(entity => entity.UserId == userId).ToListAsync(cancellationToken);
        if (userToken.Any()) return userToken;

        return null;
    }

    /// <summary>
    /// Retrieves user tokens matching the specified user ID, token name, and login provider.
    /// </summary>
    public async Task<IList<UserTokens>> GetUserTokenAsync(Guid userId, string name, string loginProvider, CancellationToken cancellationToken = default)
    {
        var userToken = await context.UserTokens.Where(entity =>
            entity.UserId == userId && entity.Name == name && entity.LoginProvider == loginProvider).ToListAsync(cancellationToken);
        if (userToken.Any()) return userToken;

        return null;
    }

    /// <summary>
    /// Persists all pending user token changes.
    /// </summary>
    public async Task<FrameworkResult> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}
