/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.ComponentModel.DataAnnotations;

namespace HCL.CS.SF.DemoServerApp.Options;

public sealed class GoogleOidcOptions
{
    public const string SectionName = "Authentication:Google";

    public bool Enabled { get; set; }

    [Required]
    public string ClientId { get; set; } = string.Empty;

    [Required]
    public string ClientSecret { get; set; } = string.Empty;

    [Required]
    public string Authority { get; set; } = "https://accounts.google.com";

    [Required]
    public string MetadataAddress { get; set; } = "https://accounts.google.com/.well-known/openid-configuration";

    [Required]
    public string CallbackPath { get; set; } = "/auth/external/google/signin-callback";

    public string[] AllowedRedirectHosts { get; set; } = Array.Empty<string>();
}
