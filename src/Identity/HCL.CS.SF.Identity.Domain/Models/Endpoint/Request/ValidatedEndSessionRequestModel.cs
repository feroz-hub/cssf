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
/// Represents a validated OIDC end-session (logout) request.
/// The id_token_hint is decoded and validated to identify the user and session,
/// and the post-logout redirect URI is verified against the client's registration.
/// </summary>
public class ValidatedEndSessionRequestModel : ValidatedBaseModel
{
    /// <summary>The security key used to validate the id_token_hint signature.</summary>
    public SecurityKey Key { get; set; }

    /// <summary>The subject identifier (sub claim) extracted from the id_token_hint.</summary>
    public string SubjectId { get; set; }

    /// <summary>The decoded id_token_hint JWT after signature validation.</summary>
    public JwtSecurityToken DecodedToken { get; set; }

    /// <summary>The expiration time of the id_token_hint as a Unix epoch timestamp.</summary>
    public long ExpiresAt { get; set; }

    /// <summary>The validated post-logout redirect URI where the user will be sent after logout.</summary>
    public string PostLogOutUri { get; set; }

    /// <summary>The state value to be included in the post-logout redirect for client correlation.</summary>
    public string State { get; set; }

    /// <summary>The collection of client identifiers that share the user's session and need logout notification.</summary>
    public IEnumerable<string> ClientIds { get; set; }
}
