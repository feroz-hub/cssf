/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Models;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.Domain.Models.Endpoint.Response;
using HCL.CS.SF.Domain.Models.Endpoint.Validation;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;

/// <summary>
/// Core token generation and lifecycle service. Handles access token and refresh token creation,
/// refresh token validation, token revocation, user token cleanup, and back-channel logout
/// token generation.
/// </summary>
public interface ITokenGenerationService
{
    /// <summary>Processes a validated token request and generates the token response (access token, refresh token, id token).</summary>
    /// <param name="tokenRequest">The validated token request model.</param>
    Task<TokenResponseModel> ProcessTokenAsync(ValidatedTokenRequestModel tokenRequest);

    /// <summary>Validates a refresh token and returns the associated token data for reissuance.</summary>
    /// <param name="refreshTokenKey">The refresh token string to validate.</param>
    /// <param name="client">The client presenting the refresh token.</param>
    Task<TokenValidationModel> ValidateRefreshTokenAsync(string refreshTokenKey, ClientsModel client);

    /// <summary>Removes all persisted tokens for the specified user (e.g., on account deletion).</summary>
    /// <param name="userId">The user identifier whose tokens should be removed.</param>
    Task<FrameworkResult> RemoveUserTokensAsync(string userId);

    /// <summary>Revokes a specific token per a validated revocation request.</summary>
    /// <param name="revocationRequest">The validated revocation request.</param>
    Task<FrameworkResult> RevokeTokenAsync(ValidatedRevocationRequestModel revocationRequest);

    /// <summary>Generates a signed JWT logout token for back-channel logout notifications.</summary>
    /// <param name="backChannelLogoutModel">The back-channel logout context.</param>
    Task<string> GenerateBackChannelLogoutTokenAsync(BackChannelLogoutModel backChannelLogoutModel);
}
