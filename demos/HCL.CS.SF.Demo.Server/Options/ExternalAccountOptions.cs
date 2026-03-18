/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.DemoServerApp.Options;

public sealed class ExternalAccountOptions
{
    public const string SectionName = "Authentication:ExternalAccount";

    public bool AutoProvisionEnabled { get; set; }

    public string[] AllowedDomains { get; set; } = Array.Empty<string>();

    public Dictionary<string, string[]> AllowedDomainsByTenant { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
