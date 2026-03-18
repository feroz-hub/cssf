/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Contracts.Responses;

/// <summary>
/// Immutable response returned after a successful OAuth token request, containing the
/// access token, its type (e.g. "Bearer"), and the lifetime in seconds.
/// </summary>
/// <param name="AccessToken">The issued access token string.</param>
/// <param name="TokenType">The token type, typically "Bearer".</param>
/// <param name="ExpiresIn">The number of seconds until the token expires.</param>
public sealed record AuthTokenResponse(string AccessToken, string TokenType, long ExpiresIn);
