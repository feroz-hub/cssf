/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Api.Response;

/// <summary>
/// Paginated response model for audit trail queries.
/// Returns a page of audit log entries along with pagination metadata.
/// </summary>
public class AuditResponseModel
{
    /// <summary>The list of audit trail entries for the current page.</summary>
    public List<AuditTrailModel> AuditList { get; set; }

    /// <summary>Pagination metadata including total items, current page, and page count.</summary>
    public PagingModel PageInfo { get; set; }
}
