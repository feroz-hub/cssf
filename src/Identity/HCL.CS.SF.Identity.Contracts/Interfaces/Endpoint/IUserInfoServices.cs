/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Models.Endpoint.Request;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;

/// <summary>
/// Processes OIDC UserInfo endpoint requests. Retrieves the user's claims based on
/// the validated access token and returns them as a dictionary for JSON serialization.
/// </summary>
public interface IUserInfoServices
{
    /// <summary>
    /// Processes a validated UserInfo request and returns the user's claims as key-value pairs.
    /// </summary>
    /// <param name="userInfoRequestValidation">The validated UserInfo request containing subject and scope information.</param>
    /// <returns>A dictionary of claim names to claim values for the authenticated user.</returns>
    Task<Dictionary<string, object>> ProcessUserInfoAsync(ValidatedUserInfoRequestModel userInfoRequestValidation);
}
