/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Threading;

namespace HCL.CS.SF.DomainServices.Repository.Api;

/// <summary>
/// Repository contract for write-side security token operations that mark tokens as consumed.
/// Implementations should execute direct database updates for performance, bypassing the EF change tracker.
/// </summary>
public interface ISecurityTokenCommandRepository
{
    /// <summary>
    /// Marks the authorization code identified by <paramref name="id"/> as consumed at the given timestamp.
    /// </summary>
    /// <param name="id">The unique identifier of the authorization code token.</param>
    /// <param name="consumedAt">The UTC timestamp when the code was consumed.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> ConsumeAuthorizationCodeAsync(Guid id, DateTime consumedAt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the refresh token identified by <paramref name="id"/> as consumed at the given timestamp.
    /// </summary>
    /// <param name="id">The unique identifier of the refresh token.</param>
    /// <param name="consumedAt">The UTC timestamp when the token was consumed.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> ConsumeRefreshTokenAsync(Guid id, DateTime consumedAt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks all active refresh tokens for the specified subject and client as consumed (token rotation/revocation).
    /// </summary>
    /// <param name="subjectId">The subject (user) identifier.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="consumedAt">The UTC timestamp when the tokens were consumed.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> ConsumeActiveRefreshTokensAsync(string subjectId, string clientId, DateTime consumedAt, CancellationToken cancellationToken = default);
}
