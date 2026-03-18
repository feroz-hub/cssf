/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Entities.Endpoint;

/// <summary>
/// Represents a persisted security token issued during OAuth2/OIDC flows (e.g., authorization codes, refresh tokens, reference tokens).
/// Tokens are stored server-side to support validation, revocation, and replay-detection scenarios.
/// </summary>
public class SecurityTokens : BaseEntity
{
    /// <summary>Unique lookup key used to locate this token in the persisted store.</summary>
    public virtual string Key { get; set; }

    /// <summary>The type of token (e.g., "authorization_code", "refresh_token", "reference_token").</summary>
    public virtual string TokenType { get; set; }

    /// <summary>The serialized token payload or handle value.</summary>
    public virtual string TokenValue { get; set; }

    /// <summary>The OAuth2 client identifier that the token was issued to.</summary>
    public virtual string ClientId { get; set; }

    /// <summary>The authenticated session identifier that produced this token.</summary>
    public virtual string SessionId { get; set; }

    /// <summary>The subject (user) identifier that the token represents.</summary>
    public virtual string SubjectId { get; set; }

    /// <summary>UTC timestamp when the token was originally created.</summary>
    public virtual DateTime CreationTime { get; set; }

    /// <summary>Token lifetime in seconds from creation time.</summary>
    public virtual int ExpiresAt { get; set; }

    /// <summary>UTC timestamp when the token was first consumed (e.g., authorization code exchanged for tokens).</summary>
    public virtual DateTime? ConsumedTime { get; set; }

    /// <summary>UTC timestamp marking when the token was consumed; used alongside <see cref="ConsumedTime"/> for audit tracking.</summary>
    public virtual DateTime? ConsumedAt { get; set; }

    /// <summary>
    /// Flag indicating that an already-consumed token was presented again.
    /// When true, all tokens in the grant should be revoked to mitigate token theft (RFC 6819 countermeasure).
    /// </summary>
    public virtual bool TokenReuseDetected { get; set; }
}
