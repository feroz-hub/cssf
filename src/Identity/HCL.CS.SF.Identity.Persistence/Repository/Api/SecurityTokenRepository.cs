/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Threading;
using Microsoft.EntityFrameworkCore;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Repository.Api;
using static HCL.CS.SF.Domain.Constants.Endpoint.OpenIdConstants;

namespace HCL.CS.SF.Infrastructure.Data.Repository.Api;

/// <summary>
/// Repository for security token read operations. Retrieves active access and refresh tokens
/// with pagination, filtered by client IDs, user IDs, date ranges, or a combination.
/// Joins SecurityTokens with Clients and Users to provide enriched token information.
/// </summary>
internal class SecurityTokenRepository : ISecurityTokenRepository
{
    private readonly IApplicationDbContext context;

    /// <summary>
    /// Initializes a new instance of <see cref="SecurityTokenRepository"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public SecurityTokenRepository(IApplicationDbContext context)
    {
        this.context = context;
    }

    /// <summary>
    /// Retrieves security tokens (access and refresh) with pagination, filtered by the specified option.
    /// Joins tokens with clients and users to return enriched <see cref="TokenModel"/> projections.
    /// Only returns non-expired tokens with a valid subject ID.
    /// </summary>
    /// <param name="page">Paging parameters; null to retrieve all matching tokens.</param>
    /// <param name="option">The filter strategy: by client, user, date range, or all.</param>
    /// <param name="fromdate">Optional start date for date-range filtering.</param>
    /// <param name="todate">Optional end date for date-range filtering.</param>
    /// <param name="clientIds">Client IDs to filter by (when option is Client).</param>
    /// <param name="userIds">User subject IDs to filter by (when option is User).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of token models matching the filter criteria.</returns>
    public async Task<IList<TokenModel>> GetSecurityTokenAsync(PagingModel page,
        SecurityTokenOption option,
        DateTime? fromdate = null,
        DateTime? todate = null,
        IList<string> clientIds = null,
        IList<string> userIds = null,
        CancellationToken cancellationToken = default)
    {
        var recordsToSkip = 0;
        IQueryable<TokenModel> result = null;
        var now = DateTime.UtcNow;
        if (page != null)
        {
            page.TotalItems = await context.SecurityTokens.CountAsync(cancellationToken);
            page.CurrentPage = page.CurrentPage == 0 ? 1 : page.CurrentPage;
            recordsToSkip = (page.CurrentPage - 1) * page.ItemsPerPage;
        }

        switch (option)
        {
            case SecurityTokenOption.Client:
                result = (from token in context.SecurityTokens.AsNoTracking()
                        join client in context.Clients.AsNoTracking() on token.ClientId equals client.ClientId
                        join user in context.Users.AsNoTracking() on token.SubjectId equals user.Id.ToString()
                        where clientIds.Contains(token.ClientId) &&
                              !string.IsNullOrWhiteSpace(token.SubjectId) &&
                              token.CreationTime.AddSeconds(token.ExpiresAt) > now &&
                              (token.TokenType == TokenType.RefreshToken || token.TokenType == TokenType.AccessToken)
                        select new TokenModel
                        {
                            ClientId = token.ClientId,
                            ClientName = client.ClientName,
                            Token = token.TokenType == TokenType.RefreshToken ? token.Key : token.TokenValue,
                            TokenTypeHint = token.TokenType == TokenType.RefreshToken
                                ? TokenType.RefreshToken
                                : TokenType.AccessToken,
                            UserName = user.UserName,
                            LoginDateTime = user.LastLoginDateTime
                        })
                    .OrderBy(s => s.ClientName);
                break;
            case SecurityTokenOption.User:
                result = (from token in context.SecurityTokens.AsNoTracking()
                        join client in context.Clients.AsNoTracking() on token.ClientId equals client.ClientId
                        join user in context.Users.AsNoTracking() on token.SubjectId equals user.Id.ToString()
                        where userIds.Contains(token.SubjectId) &&
                              !string.IsNullOrWhiteSpace(token.SubjectId) &&
                              token.CreationTime.AddSeconds(token.ExpiresAt) > now &&
                              (token.TokenType == TokenType.RefreshToken || token.TokenType == TokenType.AccessToken)
                        select new TokenModel
                        {
                            ClientId = token.ClientId,
                            ClientName = client.ClientName,
                            Token = token.TokenType == TokenType.RefreshToken ? token.Key : token.TokenValue,
                            TokenTypeHint = token.TokenType == TokenType.RefreshToken
                                ? TokenType.RefreshToken
                                : TokenType.AccessToken,
                            UserName = user.UserName,
                            LoginDateTime = user.LastLoginDateTime
                        })
                    .OrderBy(s => s.ClientName);
                break;
            case SecurityTokenOption.BetweenDates:
                result = (from token in context.SecurityTokens.AsNoTracking()
                        join client in context.Clients.AsNoTracking() on token.ClientId equals client.ClientId
                        join user in context.Users.AsNoTracking() on token.SubjectId equals user.Id.ToString()
                        where token.CreationTime >= fromdate && token.CreationTime <= todate &&
                              !string.IsNullOrWhiteSpace(token.SubjectId) &&
                              token.CreationTime.AddSeconds(token.ExpiresAt) > now &&
                              (token.TokenType == TokenType.RefreshToken || token.TokenType == TokenType.AccessToken)
                        select new TokenModel
                        {
                            ClientId = token.ClientId,
                            ClientName = client.ClientName,
                            Token = token.TokenType == TokenType.RefreshToken ? token.Key : token.TokenValue,
                            TokenTypeHint = token.TokenType == TokenType.RefreshToken
                                ? TokenType.RefreshToken
                                : TokenType.AccessToken,
                            UserName = user.UserName,
                            LoginDateTime = user.LastLoginDateTime
                        })
                    .OrderBy(s => s.ClientName);
                break;
            case SecurityTokenOption.All:
                result = (from token in context.SecurityTokens.AsNoTracking()
                        join client in context.Clients.AsNoTracking() on token.ClientId equals client.ClientId
                        join user in context.Users.AsNoTracking() on token.SubjectId equals user.Id.ToString()
                        where token.CreationTime >= fromdate && token.CreationTime <= todate &&
                              !string.IsNullOrWhiteSpace(token.SubjectId) &&
                              token.CreationTime.AddSeconds(token.ExpiresAt) > now &&
                              (token.TokenType == TokenType.RefreshToken || token.TokenType == TokenType.AccessToken)
                        select new TokenModel
                        {
                            ClientId = token.ClientId,
                            ClientName = client.ClientName,
                            Token = token.TokenType == TokenType.RefreshToken ? token.Key : token.TokenValue,
                            TokenTypeHint = token.TokenType == TokenType.RefreshToken
                                ? TokenType.RefreshToken
                                : TokenType.AccessToken,
                            UserName = user.UserName,
                            LoginDateTime = user.LastLoginDateTime
                        })
                    .OrderBy(s => s.ClientName);
                break;
        }

        if (page != null) return await result.Skip(recordsToSkip).Take(page.ItemsPerPage).ToListAsync(cancellationToken);

        return await result.ToListAsync(cancellationToken);
    }
}
