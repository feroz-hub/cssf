/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.Domain.Models.Endpoint;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Api;

/// <summary>
/// Administrative service for querying persisted security tokens (access tokens, refresh tokens).
/// Supports filtering by client, user, date range, and active/all status with optional pagination.
/// Used by the admin API for token monitoring and revocation workflows.
/// </summary>
public interface ISecurityTokenService
{
    /// <summary>Retrieves active security tokens for the specified clients.</summary>
    /// <param name="clientIds">The client identifiers to filter by.</param>
    /// <param name="page">Optional pagination parameters.</param>
    Task<IList<TokenModel>> GetClientsActiveSecurityTokensAsync(IList<string> clientIds, PagingModel page = null);

    /// <summary>Retrieves active security tokens for the specified users.</summary>
    /// <param name="userIds">The user identifiers to filter by.</param>
    /// <param name="page">Optional pagination parameters.</param>
    Task<IList<TokenModel>> GetUsersActiveSecurityTokensAsync(IList<string> userIds, PagingModel page = null);

    /// <summary>Retrieves active security tokens within the specified date range.</summary>
    /// <param name="fromdate">The start of the date range.</param>
    /// <param name="todate">The end of the date range.</param>
    /// <param name="page">Optional pagination parameters.</param>
    Task<IList<TokenModel>> GetActiveSecurityTokensAsync(DateTime fromdate, DateTime todate, PagingModel page = null);

    /// <summary>Retrieves all security tokens (active and consumed) within the specified date range.</summary>
    /// <param name="fromdate">The start of the date range.</param>
    /// <param name="todate">The end of the date range.</param>
    /// <param name="page">Optional pagination parameters.</param>
    Task<IList<TokenModel>> GetAllSecurityTokensAsync(DateTime fromdate, DateTime todate, PagingModel page = null);
}
