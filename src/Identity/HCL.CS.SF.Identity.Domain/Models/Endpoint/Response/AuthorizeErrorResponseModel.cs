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
/// Error response model specific to the OAuth 2.0 authorization endpoint.
/// Extends the base error response with authorization-specific fields such as
/// state, redirect URI, and response mode as defined in RFC 6749 Section 4.1.2.1.
/// </summary>
public class AuthorizeErrorResponseModel : ErrorResponseModel
{
    /// <summary>A URI identifying a human-readable web page with information about the error.</summary>
    [DisplayName("error_uri")] public virtual string ErrorUri { get; set; }

    /// <summary>The state value from the client's authorization request, echoed back for correlation.</summary>
    [DisplayName("state")] public virtual string State { get; set; }

    // TODO Update the below 4 values where it is applicable.

    /// <summary>A trace identifier for correlating this error with server-side logs.</summary>
    [DisplayName("trace_id")] public string TraceId { get; set; }

    /// <summary>The client identifier from the authorization request that caused the error.</summary>
    [DisplayName("client_id")] public string ClientId { get; set; }

    /// <summary>The redirect URI from the authorization request, used to determine where to send the error.</summary>
    [DisplayName("redirect_uri")] public string RedirectUri { get; set; }

    /// <summary>The response mode (e.g., "query", "fragment", "form_post") that dictates how the error is delivered.</summary>
    [DisplayName("response_mode")] public string ResponseMode { get; set; }
}
