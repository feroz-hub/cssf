/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace HCL.CS.SF.Admin.UI.Interfaces;

public interface ITokenService
{
    Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);

    Task<string?> GetRefreshTokenAsync(CancellationToken cancellationToken = default);

    Task<string?> GetIdTokenAsync(CancellationToken cancellationToken = default);

    Task<DateTimeOffset?> GetAccessTokenExpiresAtAsync(CancellationToken cancellationToken = default);

    Task<bool> RefreshTokenAsync(CancellationToken cancellationToken = default);

    Task<bool> RefreshTokenAsync(
        AuthenticationProperties properties,
        ClaimsPrincipal principal,
        bool persistCookie,
        CancellationToken cancellationToken = default);

    Task<bool> RevokeTokenAsync(CancellationToken cancellationToken = default);
}
