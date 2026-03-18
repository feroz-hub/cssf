/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.DemoServerApp.Services.ExternalAuth;

public sealed class ExternalAuthResult
{
    public bool Succeeded { get; init; }

    public bool RequiresTwoFactor { get; init; }

    public string Message { get; init; } = string.Empty;

    public string RedirectUrl { get; init; } = string.Empty;
}
