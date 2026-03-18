/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Endpoint;

/// <summary>
/// Represents a token issued to a specific user and client, used for session tracking
/// and token management operations such as listing active sessions or revoking tokens.
/// </summary>
public class TokenModel
{
    /// <summary>The OAuth 2.0 client identifier that the token was issued to.</summary>
    public string ClientId { get; set; }

    /// <summary>The display name of the client application.</summary>
    public string ClientName { get; set; }

    /// <summary>The unique identifier of the user who owns this token.</summary>
    public Guid UserId { get; set; }

    /// <summary>The username of the token owner.</summary>
    public virtual string UserName { get; set; }

    /// <summary>The UTC timestamp of when the user logged in to obtain this token.</summary>
    public virtual DateTime? LoginDateTime { get; set; }

    /// <summary>The token value (access token or refresh token handle).</summary>
    public virtual string Token { get; set; }

    /// <summary>A hint about the token type (e.g., "access_token", "refresh_token") used for revocation.</summary>
    public virtual string TokenTypeHint { get; set; }
}
