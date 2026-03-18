/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Threading;
using Microsoft.EntityFrameworkCore;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Repository.Api;

namespace HCL.CS.SF.Infrastructure.Data.Repository.Api;

/// <summary>
/// Repository for security token write operations. Provides bulk-update commands to consume
/// (mark as used) authorization codes and refresh tokens using EF Core's ExecuteUpdateAsync
/// for efficient single-roundtrip updates without loading entities into memory.
/// </summary>
internal class SecurityTokenCommandRepository : ISecurityTokenCommandRepository
{
    private readonly IApplicationDbContext context;

    /// <summary>
    /// Initializes a new instance of <see cref="SecurityTokenCommandRepository"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public SecurityTokenCommandRepository(IApplicationDbContext context)
    {
        this.context = context;
    }

    /// <summary>
    /// Marks an authorization code as consumed by setting ConsumedAt and ConsumedTime.
    /// Only consumes codes that have not already been consumed.
    /// </summary>
    /// <param name="id">The security token ID.</param>
    /// <param name="consumedAt">The UTC timestamp of consumption.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The number of rows affected (1 if consumed, 0 if already consumed or not found).</returns>
    public async Task<int> ConsumeAuthorizationCodeAsync(Guid id, DateTime consumedAt, CancellationToken cancellationToken = default)
    {
        return await context.SecurityTokens
            .Where(entity => entity.Id == id
                             && entity.TokenType == OpenIdConstants.TokenType.AuthorizationCode
                             && entity.ConsumedAt == null
                             && entity.ConsumedTime == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(entity => entity.ConsumedAt, consumedAt)
                .SetProperty(entity => entity.ConsumedTime, consumedAt), cancellationToken);
    }

    /// <summary>
    /// Marks a refresh token as consumed. Only consumes tokens that have not already been
    /// consumed and have not been flagged for token reuse detection.
    /// </summary>
    /// <param name="id">The security token ID.</param>
    /// <param name="consumedAt">The UTC timestamp of consumption.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The number of rows affected.</returns>
    public async Task<int> ConsumeRefreshTokenAsync(Guid id, DateTime consumedAt, CancellationToken cancellationToken = default)
    {
        return await context.SecurityTokens
            .Where(entity => entity.Id == id
                             && entity.TokenType == OpenIdConstants.TokenType.RefreshToken
                             && entity.ConsumedAt == null
                             && entity.ConsumedTime == null
                             && !entity.TokenReuseDetected)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(entity => entity.ConsumedAt, consumedAt)
                .SetProperty(entity => entity.ConsumedTime, consumedAt)
                .SetProperty(entity => entity.TokenReuseDetected, false), cancellationToken);
    }

    /// <summary>
    /// Consumes all active (unconsumed) refresh tokens for a specific subject and client combination.
    /// Used during token rotation to invalidate all prior refresh tokens for the session.
    /// </summary>
    /// <param name="subjectId">The user's subject identifier.</param>
    /// <param name="clientId">The OAuth client identifier.</param>
    /// <param name="consumedAt">The UTC timestamp of consumption.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The number of refresh tokens consumed.</returns>
    public async Task<int> ConsumeActiveRefreshTokensAsync(string subjectId, string clientId, DateTime consumedAt, CancellationToken cancellationToken = default)
    {
        return await context.SecurityTokens
            .Where(entity => entity.SubjectId == subjectId
                             && entity.ClientId == clientId
                             && entity.TokenType == OpenIdConstants.TokenType.RefreshToken
                             && entity.ConsumedAt == null
                             && entity.ConsumedTime == null
                             && !entity.TokenReuseDetected)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(entity => entity.ConsumedAt, consumedAt)
                .SetProperty(entity => entity.ConsumedTime, consumedAt), cancellationToken);
    }
}
