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

namespace HCL.CS.SF.DomainServices.Repository.Api;

/// <summary>
/// Repository contract for managing ASP.NET Identity user tokens (e.g., authenticator keys,
/// external login tokens). Used by <see cref="Wrappers.UserManagerWrapper{TUser}"/> for
/// TOTP authenticator key retrieval.
/// </summary>
public interface IUserTokenRepository
{
    /// <summary>Retrieves all tokens for the specified user.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<IList<UserTokens>> GetUserTokenAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves tokens for the specified user filtered by token name and login provider.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="name">The token name (e.g., "AuthenticatorKey").</param>
    /// <param name="loginProvider">The login provider that issued the token.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<IList<UserTokens>> GetUserTokenAsync(Guid userId, string name, string loginProvider, CancellationToken cancellationToken = default);

    /// <summary>Deletes multiple user tokens in a single operation.</summary>
    /// <param name="entityList">The user token entities to delete.</param>
    Task DeleteAsync(IList<UserTokens> entityList);

    /// <summary>Persists all pending user token changes.</summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<FrameworkResult> SaveChangesAsync(CancellationToken cancellationToken = default);
}
