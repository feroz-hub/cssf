/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.Domain.Models.Endpoint.Validation;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Validators;

/// <summary>
/// Validates OAuth 2.0 token endpoint requests across all supported grant types
/// (authorization_code, client_credentials, refresh_token, etc.).
/// Implementations verify grant-type-specific parameters, scope, and PKCE challenges.
/// </summary>
public interface ITokenRequestValidator
{
    /// <summary>
    /// Validates a token request against the authenticated client's configuration.
    /// </summary>
    /// <param name="requestCollection">The parsed request parameters (grant_type, code, redirect_uri, etc.).</param>
    /// <param name="clientValidationModel">The result of client authentication.</param>
    /// <returns>A validated token request model containing the resolved grant details.</returns>
    Task<ValidatedTokenRequestModel> ValidateTokenRequestAsync(Dictionary<string, string> requestCollection,
        ClientSecretValidationModel clientValidationModel);
}
