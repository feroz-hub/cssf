/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Repository.Api;

namespace HCL.CS.SF.Infrastructure.Data.Repository.Api;

/// <summary>
/// Repository for user claims data operations. Provides methods to add, update, delete,
/// and query user claims. Distinguishes between regular user claims and admin claims
/// via the IsAdminClaim flag.
/// </summary>
internal class UserClaimRepository : BaseDispose, IUserClaimRepository
{
    private readonly IApplicationDbContext context;

    /// <summary>
    /// Initializes a new instance of <see cref="UserClaimRepository"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public UserClaimRepository(IApplicationDbContext context)
    {
        this.context = context;
    }

    /// <summary>
    /// Inserts a new user claim with IsDeleted set to false.
    /// </summary>
    public Task InsertAsync(UserClaims entity)
    {
        entity.IsDeleted = false;
        context.UserClaims.Add(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates a user claim entity by attaching it and marking it as modified.
    /// </summary>
    public Task UpdateAsync(UserClaims entity)
    {
        context.UserClaims.Attach(entity);
        context.SetModifiedStatus(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes a user claim by its integer ID.
    /// </summary>
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await context.UserClaims.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null) context.UserClaims.Remove(entity);
    }

    /// <summary>
    /// Deletes a list of user claim entities.
    /// </summary>
    public Task DeleteAsync(IList<UserClaims> entityList)
    {
        foreach (var entity in entityList) context.UserClaims.Remove(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves all non-admin claims for a specific user.
    /// </summary>
    public async Task<IList<UserClaims>> GetClaimsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userClaim = await context.UserClaims
            .Where(entity => entity.UserId == userId && entity.IsAdminClaim == false)
            .ToListAsync(cancellationToken);
        if (userClaim.Any()) return userClaim;

        return null;
    }

    /// <summary>
    /// Retrieves admin claims for a user, or all claims if <paramref name="getOnlyAdminClaim"/> is false.
    /// </summary>
    public async Task<IList<UserClaims>> GetAdminUserClaimsAsync(Guid userId, bool getOnlyAdminClaim = true, CancellationToken cancellationToken = default)
    {
        List<UserClaims> userClaims = getOnlyAdminClaim
            ? await context.UserClaims.Where(entity => entity.UserId == userId && entity.IsAdminClaim).ToListAsync(cancellationToken)
            : await context.UserClaims.Where(entity => entity.UserId == userId).ToListAsync(cancellationToken);

        if (userClaims.Any()) return userClaims;

        return null;
    }


    /// <summary>
    /// Finds a user claim matching the user ID, claim type, claim value, and admin flag.
    /// </summary>
    public async Task<UserClaims> FindIdByClaimAsync(Guid userId, Claim claim, bool isAdminClaim = false, CancellationToken cancellationToken = default)
    {
        var userClaim = await context.UserClaims.FirstOrDefaultAsync(entity => entity.UserId == userId &&
                                                                               entity.ClaimType == claim.Type &&
                                                                               entity.ClaimValue == claim.Value &&
                                                                               entity.IsAdminClaim == isAdminClaim, cancellationToken);

        if (userClaim != null) return userClaim;

        return null;
    }

    /// <summary>
    /// Finds a user claim entity by its integer ID.
    /// </summary>
    public async Task<UserClaims> FindUserClaimByIdAsync(int userClaimId, CancellationToken cancellationToken = default)
    {
        return await context.UserClaims.FirstOrDefaultAsync(entity => entity.Id == userClaimId, cancellationToken);
    }

    /// <summary>
    /// Persists all pending user claim changes.
    /// </summary>
    public async Task<FrameworkResult> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}
