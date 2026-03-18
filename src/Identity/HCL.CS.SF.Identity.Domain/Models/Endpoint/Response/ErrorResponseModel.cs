/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.ComponentModel;

namespace HCL.CS.SF.Domain.Models.Endpoint.Response;

/// <summary>
/// Base error response model for OAuth 2.0 / OIDC endpoint responses.
/// Follows the error response format defined in RFC 6749 Section 5.2.
/// Many endpoint response models inherit from this to carry error state.
/// </summary>
public class ErrorResponseModel
{
    /// <summary>Indicates whether this response represents an error condition. Defaults to true.</summary>
    public bool IsError { get; set; } = true;

    /// <summary>The OAuth 2.0 error code (e.g., "invalid_request", "invalid_client", "unauthorized_client").</summary>
    [DisplayName("error")] public string ErrorCode { get; set; }

    /// <summary>A human-readable description providing additional detail about the error.</summary>
    [DisplayName("error_description")] public string ErrorDescription { get; set; }
}

/// <summary>
/// Deserialization-friendly error response model with JSON-matching property names.
/// Used for parsing raw error responses from OAuth 2.0 endpoints.
/// </summary>
public class ErrorResponseResultModel
{
    /// <summary>The OAuth 2.0 error code.</summary>
    public string error { get; set; }

    /// <summary>A human-readable error description.</summary>
    public string error_description { get; set; }
}
