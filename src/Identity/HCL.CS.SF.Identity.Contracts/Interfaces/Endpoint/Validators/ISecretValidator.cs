/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Models.Endpoint;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Validators;

/// <summary>
/// Validates a client's secret (e.g., shared secret, certificate) against the stored credential.
/// Implementations should support the configured secret types (hashed, plain, X.509).
/// </summary>
public interface ISecretValidator
{
    /// <summary>
    /// Validates the parsed secret against the client's registered credentials.
    /// </summary>
    /// <param name="client">The registered client containing stored secrets.</param>
    /// <param name="parsedSecret">The secret extracted from the request.</param>
    /// <returns><c>true</c> if the secret is valid; otherwise <c>false</c>.</returns>
    Task<bool> ValidateSecretAsync(ClientsModel client, ParsedSecretModel parsedSecret);
}
