/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.DemoClientMvc.Services;

public sealed class EndpointTokenFlowResult
{
    public string? AccessToken { get; set; }

    public string? IdToken { get; set; }

    public string? RefreshToken { get; set; }

    public int ExpiresIn { get; set; }

    public string? TokenType { get; set; }

    public string? ErrorCode { get; set; }

    public string? ErrorDescription { get; set; }

    public IReadOnlyDictionary<string, string> Claims { get; set; } = new Dictionary<string, string>();

    public bool Succeeded => !string.IsNullOrWhiteSpace(AccessToken) && string.IsNullOrWhiteSpace(ErrorCode);
}
