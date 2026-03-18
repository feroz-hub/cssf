/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCLCSSFInstallerMVC.Application.DTOs;

/// <summary>
/// Result of the database seed operation, including generated OAuth client credentials on success.
/// </summary>
public sealed class SeedExecutionResultDto
{
    /// <summary>Whether the seed completed without errors.</summary>
    public bool Succeeded { get; init; }

    /// <summary>Diagnostic message when seeding failed; <c>null</c> on success.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>The auto-generated OAuth client ID (Base64-encoded random bytes).</summary>
    public string? GeneratedClientId { get; init; }

    /// <summary>The auto-generated OAuth client secret (plain-text, before SHA-256 hashing for storage).</summary>
    public string? GeneratedClientSecret { get; init; }
}
