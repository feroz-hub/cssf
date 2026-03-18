/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using HCL.CS.SF.Domain.Models.Endpoint.Validation;

namespace HCL.CS.SF.Domain.Models.Endpoint.Request;

/// <summary>
/// Represents a validated OAuth 2.0 Token Introspection request (RFC 7662).
/// Contains the decoded token metadata after the submitted token has been validated.
/// </summary>
public class ValidatedIntrospectionRequestModel : ValidatedBaseModel
{
    /// <summary>Indicates whether the introspected token is currently active (valid and not revoked).</summary>
    public bool Active { get; set; } = true;

    /// <summary>The space-delimited scopes associated with the introspected token.</summary>
    public string Scopes { get; set; }

    /// <summary>The security key used to validate the token's signature.</summary>
    public SecurityKey Key { get; set; }

    /// <summary>The decoded JWT after signature validation.</summary>
    public JwtSecurityToken DecodedToken { get; set; }

    /// <summary>The type of the introspected token (e.g., "access_token", "refresh_token").</summary>
    public string TokenType { get; set; }

    /// <summary>The user identifier extracted from the introspected token.</summary>
    public string UserId { get; set; }

    /// <summary>The token expiration time as a Unix epoch timestamp.</summary>
    public long? ExpiresAt { get; set; }

    /// <summary>The token issuance time as a Unix epoch timestamp.</summary>
    public long? IssuedAt { get; set; }
}
