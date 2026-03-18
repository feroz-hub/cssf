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
using HCL.CS.SF.Domain.Models.Api;

namespace HCL.CS.SF.DomainServices.Repository.Api;

/// <summary>
/// Repository contract for persisting and querying audit trail records.
/// Implementations store security-relevant events (logins, changes, access attempts) and
/// support paginated, filtered retrieval for compliance and troubleshooting.
/// </summary>
public interface IAuditRepository
{
    /// <summary>Inserts a new audit trail record.</summary>
    /// <param name="entity">The audit trail entry to persist.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task InsertAsync(AuditTrail entity, CancellationToken cancellationToken = default);

    /// <summary>Returns the total count of all audit trail records.</summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the count of audit trail records matching the specified search criteria.</summary>
    /// <param name="auditSearchModule">The search/filter criteria.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<int> GetFilteredCountAsync(AuditSearchRequestModel auditSearchModule, CancellationToken cancellationToken = default);

    /// <summary>Persists all pending audit trail changes.</summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<FrameworkResult> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Retrieves audit trail records matching the specified search criteria with pagination.</summary>
    /// <param name="auditSearchModule">The search/filter criteria including pagination parameters.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<IList<AuditTrail>> GetAuditDetailsAsync(AuditSearchRequestModel auditSearchModule, CancellationToken cancellationToken = default);
}
