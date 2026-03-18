/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Admin.UI.Models.Api;

public class SecurityTokensModel
{
    public string Id { get; set; } = string.Empty;

    public string? Key { get; set; }

    public string? TokenType { get; set; }

    public string? TokenValue { get; set; }

    public DateTime? ConsumedTime { get; set; }

    public string? ConsumedAt { get; set; }

    public bool TokenReuseDetected { get; set; }

    public string? ClientId { get; set; }

    public string? SessionId { get; set; }

    public string? UserId { get; set; }

    public string? SubjectId { get; set; }

    public DateTime? CreationTime { get; set; }

    public DateTime? ExpiresAt { get; set; }
}

public class RevokeTokenRequest
{
    public string Token { get; set; } = string.Empty;

    public string TokenTypeHint { get; set; } = string.Empty;
}
