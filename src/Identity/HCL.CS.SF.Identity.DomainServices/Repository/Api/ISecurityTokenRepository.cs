/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Threading;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.Domain.Models.Endpoint;

namespace HCL.CS.SF.DomainServices.Repository.Api;

/// <summary>
/// Read-side repository contract for querying persisted security tokens (authorization codes,
/// refresh tokens, etc.) with support for pagination, date-range filtering, and client/user filtering.
/// </summary>
public interface ISecurityTokenRepository
{
    /// <summary>
    /// Retrieves security tokens matching the specified filters with pagination.
    /// </summary>
    /// <param name="page">Paging parameters (page number, page size).</param>
    /// <param name="option">The type of security tokens to retrieve (active, all, etc.).</param>
    /// <param name="fromdate">Optional start of the date range filter.</param>
    /// <param name="todate">Optional end of the date range filter.</param>
    /// <param name="clientIds">Optional list of client identifiers to filter by.</param>
    /// <param name="userIds">Optional list of user identifiers to filter by.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<IList<TokenModel>> GetSecurityTokenAsync(PagingModel page,
        SecurityTokenOption option,
        DateTime? fromdate = null,
        DateTime? todate = null,
        IList<string> clientIds = null,
        IList<string> userIds = null,
        CancellationToken cancellationToken = default);
}
