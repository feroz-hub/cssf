/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Api;

/// <summary>
/// Pagination metadata model used across list/search API responses.
/// Provides page-based navigation information for paginated result sets.
/// </summary>
public class PagingModel
{
    /// <summary>The total number of items across all pages.</summary>
    public int TotalItems { get; set; }

    /// <summary>The number of items returned per page.</summary>
    public int ItemsPerPage { get; set; }

    /// <summary>The current page number (1-based).</summary>
    public int CurrentPage { get; set; }

    /// <summary>The total number of pages, computed from total items and items per page.</summary>
    public int TotalPages => (int)Math.Ceiling((decimal)TotalItems / ItemsPerPage);

    /// <summary>The number of page links to display in the pagination UI.</summary>
    public int TotalDisplayPages { get; set; }
}
