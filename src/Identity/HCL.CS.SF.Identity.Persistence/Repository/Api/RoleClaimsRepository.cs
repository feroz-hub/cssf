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
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Repository.Api;

namespace HCL.CS.SF.Infrastructure.Data.Repository.Api;

/// <summary>
/// Repository for role claims data operations. Provides methods to add, update, and remove
/// claims from roles, look up claims by role ID or claim type/value, and retrieve the
/// aggregated role-claim assignments for a specific user.
/// </summary>
internal class RoleClaimsRepository : BaseDispose, IRoleClaimsRepository
{
    private readonly IApplicationDbContext context;

    /// <summary>
    /// Initializes a new instance of <see cref="RoleClaimsRepository"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public RoleClaimsRepository(IApplicationDbContext context)
    {
        this.context = context;
    }

    /// <summary>
    /// Adds a new claim to a role.
    /// </summary>
    /// <param name="entity">The role claim entity to insert.</param>
    public Task InsertAsync(RoleClaims entity)
    {
        context.RoleClaims.Add(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes a role claim by its integer ID with row-version concurrency control.
    /// </summary>
    public async Task DeleteAsync(int roleClaimId, CancellationToken cancellationToken = default)
    {
        var entity = await context.RoleClaims.FindAsync(new object[] { roleClaimId }, cancellationToken);
        if (entity != null)
        {
            context.SetRowVersionStatus(entity, entity.RowVersion);
            context.RoleClaims.Remove(entity);
        }
    }

    /// <summary>
    /// Deletes all claims associated with a specific role.
    /// </summary>
    public async Task DeleteAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        IList<RoleClaims> entity = await context.RoleClaims.Where(x => x.RoleId == roleId).ToListAsync(cancellationToken);
        if (entity.Count > 0) context.RoleClaims.RemoveRange(entity);
    }

    /// <summary>
    /// Deletes a single role claim entity with row-version concurrency control.
    /// </summary>
    public Task DeleteAsync(RoleClaims entity)
    {
        context.SetRowVersionStatus(entity, entity.RowVersion);
        context.RoleClaims.Remove(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates a role claim entity by attaching it and marking it as modified.
    /// </summary>
    public Task UpdateAsync(RoleClaims entity)
    {
        context.RoleClaims.Attach(entity);
        context.SetModifiedStatus(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves all claims for a specific role.
    /// </summary>
    public async Task<IList<RoleClaims>> GetClaimsAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await context.RoleClaims.Where(x => x.RoleId == roleId).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Finds the integer ID of a role claim matching the specified role, claim type, and claim value.
    /// Returns 0 if not found.
    /// </summary>
    public async Task<int> FindIdByClaimAsync(Guid roleId, Claim claim, CancellationToken cancellationToken = default)
    {
        var roleClaim = await context.RoleClaims.FirstOrDefaultAsync(entity =>
            entity.RoleId == roleId && entity.ClaimType == claim.Type && entity.ClaimValue == claim.Value, cancellationToken);
        if (roleClaim != null) return roleClaim.Id;

        return 0;
    }

    /// <summary>
    /// Finds a role claim entity matching the specified role, claim type, and claim value.
    /// </summary>
    public async Task<RoleClaims> FindRoleByClaimAsync(Guid roleId, Claim claim, CancellationToken cancellationToken = default)
    {
        return await context.RoleClaims.FirstOrDefaultAsync(entity =>
            entity.RoleId == roleId && entity.ClaimType == claim.Type && entity.ClaimValue == claim.Value, cancellationToken);
    }

    /// <summary>
    /// Finds a role claim entity by its integer ID.
    /// </summary>
    public async Task<RoleClaims> FindRoleClaimByIdAsync(int roleClaimId, CancellationToken cancellationToken = default)
    {
        return await context.RoleClaims.FirstOrDefaultAsync(entity => entity.Id == roleClaimId, cancellationToken);
    }

    /// <summary>
    /// Retrieves all role-claim combinations for a specific user by joining Users -> UserRoles -> Roles -> RoleClaims.
    /// Used during token generation to gather claims inherited through role assignments.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A flat list of user/role/claim type/value projections.</returns>
    public async Task<IList<UserRoleClaimTypesModel>> GetRolesAndClaimsForUser(Guid userId, CancellationToken cancellationToken = default)
    {
        // Multi-table join: Users -> UserRoles -> Roles -> RoleClaims
        // Filters out entries where ClaimType is null (roles with no claims)
        var userRoleClaims = await (from users in context.Users.AsNoTracking()
                join userRoles in context.UserRoles.AsNoTracking()
                    on users.Id equals userRoles.UserId into userRoleJoin
                from userRoleModel in userRoleJoin.DefaultIfEmpty()
                where userRoleModel.UserId == userId
                join roles in context.Roles.AsNoTracking()
                    on userRoleModel.RoleId equals roles.Id into rolesJoin
                from rolesModel in rolesJoin.DefaultIfEmpty()
                join roleClaims in context.RoleClaims.AsNoTracking()
                    on rolesModel.Id equals roleClaims.RoleId into roleClaimJoin
                from userRoleClaimModel in roleClaimJoin.DefaultIfEmpty()
                where userRoleClaimModel.ClaimType != null
                select new UserRoleClaimTypesModel
                {
                    UserId = users.Id,
                    UserName = users.UserName,
                    RoleName = rolesModel.Name,
                    RoleClaimType = userRoleClaimModel.ClaimType,
                    RoleClaimValue = userRoleClaimModel.ClaimValue
                })
            .ToListAsync(cancellationToken);

        return userRoleClaims;
    }

    /// <summary>
    /// Persists all pending role claim changes.
    /// </summary>
    public async Task<FrameworkResult> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}
