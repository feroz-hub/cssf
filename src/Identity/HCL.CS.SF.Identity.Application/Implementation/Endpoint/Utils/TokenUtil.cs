/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Utils;

/// <summary>
/// Utility class for checking token revocation status.
/// Queries the security token repository to determine whether an access or refresh token
/// has been revoked (deleted or marked as consumed/reuse-detected).
/// </summary>
internal class TokenUtil
{
    private readonly IRepository<SecurityTokens> securityTokenRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenUtil"/> class.
    /// </summary>
    internal TokenUtil(IRepository<SecurityTokens> securityTokenRepository)
    {
        this.securityTokenRepository = securityTokenRepository;
    }

    /// <summary>
    /// Checks whether a token has been revoked by looking up its presence in the security token store.
    /// For refresh tokens, also checks for consumption and reuse detection flags.
    /// </summary>
    /// <param name="token">The raw token string (JWT for access tokens, opaque handle for refresh tokens).</param>
    /// <param name="tokenType">Optional token type hint (access_token or refresh_token).</param>
    /// <returns>True if the token has been revoked or is not found; false if it is still active.</returns>
    internal async Task<bool> IsTokenRevoked(string token, string tokenType = null)
    {
        if (string.IsNullOrWhiteSpace(token)) return true;

        var tokenHash = token.ComputeSha256Hash();
        if (tokenType == OpenIdConstants.TokenResponseType.RefreshToken ||
            tokenType == OpenIdConstants.TokenType.RefreshToken)
        {
            var refreshToken = await securityTokenRepository.GetAsync(entity =>
                entity.TokenType == OpenIdConstants.TokenType.RefreshToken
                && entity.Key == tokenHash
                && !entity.TokenReuseDetected
                && entity.ConsumedAt == null
                && entity.ConsumedTime == null);
            return !refreshToken.ContainsAny();
        }

        if (tokenType == OpenIdConstants.TokenResponseType.AccessToken ||
            tokenType == OpenIdConstants.TokenType.AccessToken)
        {
            var accessToken = await securityTokenRepository.GetAsync(entity =>
                entity.TokenType == OpenIdConstants.TokenType.AccessToken
                && entity.TokenValue == token);
            return !accessToken.ContainsAny();
        }

        var tokenResult = await securityTokenRepository.GetAsync(entity =>
            (entity.TokenType == OpenIdConstants.TokenType.AccessToken && entity.TokenValue == token)
            || (entity.TokenType == OpenIdConstants.TokenType.RefreshToken
                && entity.Key == tokenHash
                && !entity.TokenReuseDetected
                && entity.ConsumedAt == null
                && entity.ConsumedTime == null));
        return !tokenResult.ContainsAny();
    }
}
