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
/// Response model for the OAuth 2.0 Token Introspection endpoint (RFC 7662).
/// Resource servers use this endpoint to determine whether an access or refresh token
/// is active and to retrieve metadata about the token.
/// </summary>
public class IntrospectionResponseModel
{
    /// <summary>Indicates whether the token is currently active (not expired, not revoked).</summary>
    [DisplayName("active")] public bool Active { get; set; }

    /// <summary>The OAuth 2.0 client identifier to which the token was issued.</summary>
    [DisplayName("client_id")] public string ClientId { get; set; }

    /// <summary>The human-readable username of the resource owner who authorized the token.</summary>
    [DisplayName("username")] public string UserName { get; set; }

    /// <summary>Space-delimited list of scopes associated with the token.</summary>
    [DisplayName("scope")] public string Scope { get; set; }

    /// <summary>The subject identifier (sub claim) of the resource owner.</summary>
    [DisplayName("sub")] public string SubjectId { get; set; }

    /// <summary>The intended audience (aud claim) for the token, typically the API resource.</summary>
    [DisplayName("aud")] public string Audience { get; set; }

    /// <summary>The issuer (iss claim) that created and signed the token.</summary>
    [DisplayName("iss")] public string Issuer { get; set; }

    /// <summary>The token expiration time as a Unix epoch timestamp.</summary>
    [DisplayName("exp")] public string ExpiresAt { get; set; }

    /// <summary>The time at which the token was issued as a Unix epoch timestamp.</summary>
    [DisplayName("iat")] public string IssuedAt { get; set; }
}
