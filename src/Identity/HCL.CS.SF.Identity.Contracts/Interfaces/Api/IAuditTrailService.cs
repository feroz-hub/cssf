/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.Domain.Models.Api.Response;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Api;

/// <summary>
/// Service for recording and querying audit trail entries. Captures security-relevant events
/// (user actions, configuration changes, authentication attempts) and provides filtered,
/// paginated retrieval for compliance and security monitoring.
/// </summary>
public interface IAuditTrailService
{
    /// <summary>Records multiple audit trail entries in a single operation.</summary>
    /// <param name="audits">The audit trail entries to record.</param>
    Task<FrameworkResult> AddAuditTrailAsync(IEnumerable<AuditTrailModel> audits);

    /// <summary>Records a single audit trail entry.</summary>
    /// <param name="audit">The audit trail entry to record.</param>
    Task<FrameworkResult> AddAuditTrailAsync(AuditTrailModel audit);

    /// <summary>Retrieves audit trail entries matching the specified search criteria with pagination.</summary>
    /// <param name="auditSearchRequestModel">The search/filter criteria.</param>
    Task<AuditResponseModel> GetAuditDetailsAsync(AuditSearchRequestModel auditSearchRequestModel);

    //Task<AuditResponseModel> GetAuditDetailsAsync(string createdBy, DateTime? createdOn, PagingModel page);

    //Task<AuditResponseModel> GetAuditDetailsAsync(string createdBy, DateTime? fromDate, DateTime? toDate, PagingModel page);

    //Task<AuditResponseModel> GetAuditDetailsAsync(string createdBy, AuditType actionType, DateTime? fromDate, DateTime? toDate, PagingModel page);
}
