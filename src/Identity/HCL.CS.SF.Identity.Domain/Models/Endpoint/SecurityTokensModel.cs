/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Endpoint;

/// <summary>
/// Represents a persisted security token (e.g., refresh token, reference access token)
/// stored by the identity server. Used for token lifecycle management including
/// validation, consumption, expiration, and replay/reuse detection.
/// </summary>
public class SecurityTokensModel : BaseModel
{
    /// <summary>The unique lookup key for this token in the persistence store.</summary>
    public virtual string Key { get; set; }

    /// <summary>The type of token (e.g., "refresh_token", "access_token").</summary>
    public virtual string TokenType { get; set; }

    /// <summary>The serialized token value or handle.</summary>
    public virtual string TokenValue { get; set; }

    /// <summary>The UTC timestamp when the token was first consumed (used). Null if not yet consumed.</summary>
    public virtual DateTime? ConsumedTime { get; set; }

    /// <summary>Alternative consumed-at timestamp for backward compatibility.</summary>
    public virtual DateTime? ConsumedAt { get; set; }

    /// <summary>Indicates whether token reuse was detected, signaling a potential token theft scenario.</summary>
    public virtual bool TokenReuseDetected { get; set; }

    /// <summary>The OAuth 2.0 client identifier that the token was issued to.</summary>
    public virtual string ClientId { get; set; }

    /// <summary>The authentication session identifier associated with this token.</summary>
    public virtual string SessionId { get; set; }

    /// <summary>The user identifier of the resource owner.</summary>
    public virtual string UserId { get; set; }

    /// <summary>The subject identifier (sub claim) of the resource owner.</summary>
    public virtual string SubjectId { get; set; }

    /// <summary>The UTC timestamp when this token was created.</summary>
    public virtual DateTime CreationTime { get; set; }

    /// <summary>The token expiration time in seconds from the creation time.</summary>
    public virtual int ExpiresAt { get; set; }
}
