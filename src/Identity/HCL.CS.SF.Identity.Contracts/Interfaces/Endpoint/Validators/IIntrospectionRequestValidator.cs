/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Request;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Validators;

/// <summary>
/// Validates OAuth 2.0 token introspection requests per RFC 7662.
/// Implementations verify the token parameter and ensure the requesting client is authorized
/// to introspect the token.
/// </summary>
public interface IIntrospectionRequestValidator
{
    /// <summary>
    /// Validates a token introspection request from the authenticated client.
    /// </summary>
    /// <param name="requestCollection">The parsed request parameters (token, token_type_hint).</param>
    /// <param name="client">The authenticated client making the introspection request.</param>
    /// <returns>A validated introspection request model.</returns>
    Task<ValidatedIntrospectionRequestModel> ValidateIntrospectionRequestAsync(
        Dictionary<string, string> requestCollection, ClientsModel client);
}
