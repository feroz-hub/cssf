/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Enums;

namespace HCL.CS.SF.Domain.Models.Api;

/// <summary>
/// Request model for searching and filtering audit trail entries.
/// Supports filtering by action type, date range, creator, and free-text search,
/// with pagination support.
/// </summary>
public class AuditSearchRequestModel
{
    /// <summary>Filter by audit action type (Create, Update, Delete). Defaults to None (all types).</summary>
    public AuditType ActionType { get; set; } = AuditType.None;

    /// <summary>Filter by the user or system that performed the audited action.</summary>
    public string CreatedBy { get; set; }

    /// <summary>The start of the date range filter (inclusive).</summary>
    public DateTime? FromDate { get; set; }

    /// <summary>The end of the date range filter (inclusive).</summary>
    public DateTime? ToDate { get; set; }

    /// <summary>Pagination parameters for the search results.</summary>
    public PagingModel Page { get; set; }

    /// <summary>Filter by exact creation date.</summary>
    public DateTime? CreatedOn { get; set; }

    /// <summary>Free-text search value applied across audit trail fields.</summary>
    public string SearchValue { get; set; }
}
