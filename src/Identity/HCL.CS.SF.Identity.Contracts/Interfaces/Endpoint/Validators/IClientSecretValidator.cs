/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Http;
using HCL.CS.SF.Domain.Models.Endpoint.Validation;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Validators;

/// <summary>
/// Orchestrates client authentication by parsing client credentials from the HTTP request
/// and validating them against the registered client's stored secrets.
/// </summary>
public interface IClientSecretValidator
{
    /// <summary>
    /// Extracts and validates the client credentials from the HTTP context.
    /// </summary>
    /// <param name="context">The current HTTP context containing the client credentials.</param>
    /// <returns>A <see cref="ClientSecretValidationModel"/> indicating the validation outcome and client details.</returns>
    Task<ClientSecretValidationModel> ValidateClientSecretAsync(HttpContext context);
}
