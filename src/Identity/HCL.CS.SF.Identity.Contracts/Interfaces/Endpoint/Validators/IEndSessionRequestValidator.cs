/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using HCL.CS.SF.Domain.Models.Endpoint.Request;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Validators;

/// <summary>
/// Validates OIDC end-session (logout) requests and end-session callback requests.
/// Implementations verify the id_token_hint, post_logout_redirect_uri, and state parameters
/// per the OpenID Connect RP-Initiated Logout specification.
/// </summary>
public interface IEndSessionRequestValidator
{
    /// <summary>
    /// Validates an end-session request initiated by the user/RP.
    /// </summary>
    /// <param name="requestCollection">The parsed request parameters.</param>
    /// <param name="user">The currently authenticated user's claims principal.</param>
    /// <returns>A validated end-session request model.</returns>
    Task<ValidatedEndSessionRequestModel> ValidateRequestAsync(Dictionary<string, string> requestCollection,
        ClaimsPrincipal user);

    /// <summary>
    /// Validates an end-session callback request (front-channel logout notification).
    /// </summary>
    /// <param name="requestCollection">The parsed callback request parameters.</param>
    /// <returns>A validated end-session callback request model.</returns>
    Task<ValidatedEndSessionCallbackRequestModel> ValidateCallbackRequestAsync(
        Dictionary<string, string> requestCollection);
}
