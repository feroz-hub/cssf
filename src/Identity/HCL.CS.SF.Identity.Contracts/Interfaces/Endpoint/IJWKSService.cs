/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Models.Endpoint.Response;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;

/// <summary>
/// Provides the JSON Web Key Set (JWKS) for the authorization server's signing keys.
/// The JWKS endpoint allows relying parties to retrieve the public keys needed to
/// verify JWT signatures.
/// </summary>
public interface IJWKSService
{
    /// <summary>
    /// Retrieves all active public signing keys as JSON Web Key response models.
    /// </summary>
    /// <returns>A list of <see cref="JsonWebKeyResponseModel"/> entries for the JWKS document.</returns>
    Task<IList<JsonWebKeyResponseModel>> ProcessJWKSInformations();
}
