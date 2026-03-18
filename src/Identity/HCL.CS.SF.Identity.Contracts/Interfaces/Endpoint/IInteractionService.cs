/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.Domain.Models.Endpoint.Response;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;

/// <summary>
/// Provides UI-facing interaction capabilities for the authorization server, including
/// user verification code construction, logout context retrieval, and error context look-up.
/// Used by login/consent/logout pages to drive the OIDC user experience.
/// </summary>
public interface IInteractionService
{
    /// <summary>
    /// Constructs a user verification code URL for device authorization or email verification flows.
    /// </summary>
    /// <param name="returnUrl">The URL to redirect to after verification.</param>
    /// <param name="userVerficationCode">The verification code to embed in the URL.</param>
    /// <returns>The fully constructed verification URL.</returns>
    Task<string> ConstructUserVerificationCode(string returnUrl, string userVerficationCode);

    /// <summary>
    /// Retrieves the logout context for the specified logout identifier, including client information
    /// and post-logout redirect URI.
    /// </summary>
    /// <param name="logoutId">The logout session identifier.</param>
    /// <returns>The logout request details.</returns>
    Task<LogoutRequestModel> GetLogoutContextAsync(string logoutId);

    /// <summary>
    /// Retrieves the error context for the specified error identifier, used to display
    /// authorization error details to the user.
    /// </summary>
    /// <param name="errorId">The error identifier.</param>
    /// <returns>The error response details.</returns>
    Task<ErrorResponseModel> GetErrorContextAsync(string errorId);
}
