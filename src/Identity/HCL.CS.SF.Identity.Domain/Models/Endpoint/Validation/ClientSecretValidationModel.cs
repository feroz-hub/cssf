/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Models.Endpoint.Response;

namespace HCL.CS.SF.Domain.Models.Endpoint.Validation;

/// <summary>
/// Result of validating a client's secret during client authentication.
/// Contains the resolved client and the parsed secret if authentication succeeds.
/// Used at the token, revocation, and introspection endpoints.
/// </summary>
public class ClientSecretValidationModel : ErrorResponseModel
{
    /// <summary>The authenticated client whose secret was validated.</summary>
    public ClientsModel Client { get; set; }

    /// <summary>The parsed secret that was successfully validated against the client's stored secret.</summary>
    public ParsedSecretModel Secret { get; set; }
}
