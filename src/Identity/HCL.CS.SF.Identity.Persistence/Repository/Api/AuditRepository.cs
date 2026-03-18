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
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Repository.Api;
using Microsoft.EntityFrameworkCore;

//using LinqKit;

namespace HCL.CS.SF.Infrastructure.Data.Repository.Api;

/// <summary>
/// Repository for audit trail data operations. Provides methods to insert audit records,
/// retrieve paginated and filtered audit history, and count audit entries.
/// Supports filtering by created-by user, action type, date range, and free-text search.
/// </summary>
internal class AuditRepository : BaseRepository<AuditTrail>, IAuditRepository
{
    private readonly IApplicationDbContext context;

    /// <summary>
    /// Initializes a new instance of <see cref="AuditRepository"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public AuditRepository(IApplicationDbContext context)
        : base(context)
    {
        this.context = context;
    }

    /// <summary>
    /// Inserts an audit trail record directly into the AuditTrail DbSet.
    /// </summary>
    /// <param name="entity">The audit trail entity to insert.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public override Task InsertAsync(AuditTrail entity, CancellationToken cancellationToken = default)
    {
        context.AuditTrail.Add(entity);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override async Task<FrameworkResult> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Returns the total count of all audit trail records.
    /// </summary>
    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await context.AuditTrail.CountAsync(cancellationToken);
    }

    /// <summary>
    /// Returns the count of audit records matching the specified search filters.
    /// </summary>
    public async Task<int> GetFilteredCountAsync(AuditSearchRequestModel auditSearchModule, CancellationToken cancellationToken = default)
    {
        var query = ApplyAuditFilters(context.AuditTrail.AsNoTracking(), auditSearchModule);
        return await query.CountAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a paginated, filtered list of audit trail records ordered by creation date.
    /// </summary>
    public async Task<IList<AuditTrail>> GetAuditDetailsAsync(AuditSearchRequestModel auditSearchModule, CancellationToken cancellationToken = default)
    {
        auditSearchModule.Page.CurrentPage =
            auditSearchModule.Page.CurrentPage == 0 ? 1 : auditSearchModule.Page.CurrentPage;
        var recordsToSkip = (auditSearchModule.Page.CurrentPage - 1) * auditSearchModule.Page.ItemsPerPage;

        var query = ApplyAuditFilters(context.AuditTrail.AsNoTracking(), auditSearchModule);

        return await query
            .OrderBy(s => s.CreatedOn)
            .Skip(recordsToSkip)
            .Take(auditSearchModule.Page.ItemsPerPage)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Applies search filters (created-by, free-text search in old/new values, date range,
    /// and action type) to an audit trail query.
    /// </summary>
    private static IQueryable<AuditTrail> ApplyAuditFilters(IQueryable<AuditTrail> query, AuditSearchRequestModel auditSearchModule)
    {
        if (!string.IsNullOrWhiteSpace(auditSearchModule.CreatedBy))
            query = query.Where(s => s.CreatedBy.Equals(auditSearchModule.CreatedBy));

        var searchValue = auditSearchModule.SearchValue?.Trim();
        if (!string.IsNullOrWhiteSpace(searchValue))
            query = query.Where(s =>
                (!string.IsNullOrWhiteSpace(s.NewValue) && s.NewValue.Contains(searchValue)) ||
                (!string.IsNullOrWhiteSpace(s.OldValue) && s.OldValue.Contains(searchValue)));

        if (auditSearchModule.FromDate != null && auditSearchModule.ToDate == null)
            query = query.Where(s => s.CreatedOn == auditSearchModule.FromDate);

        if (auditSearchModule.FromDate != null && auditSearchModule.ToDate != null)
            query = query.Where(s => s.CreatedOn >= auditSearchModule.FromDate && s.CreatedOn <= auditSearchModule.ToDate);

        if (auditSearchModule.ActionType != AuditType.None)
            query = query.Where(s => s.ActionType == auditSearchModule.ActionType);

        return query;
    }
}
