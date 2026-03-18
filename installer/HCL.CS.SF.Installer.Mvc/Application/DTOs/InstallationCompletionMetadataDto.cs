/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCLCSSFInstallerMVC.Application.DTOs;

/// <summary>
/// Metadata persisted in the installer lock marker file when installation completes.
/// Includes the timestamp, database provider used, and generated client credentials.
/// </summary>
public sealed class InstallationCompletionMetadataDto
{
    /// <summary>UTC timestamp when the installation was finalized.</summary>
    public DateTimeOffset CompletedOnUtc { get; init; }

    /// <summary>Name of the database provider that was used (e.g., "SqlServer", "PostgreSql").</summary>
    public string? DatabaseProvider { get; init; }

    /// <summary>Generated OAuth client ID, or <c>null</c> if seed was skipped.</summary>
    public string? ClientId { get; init; }

    /// <summary>Generated OAuth client secret (plain-text), or <c>null</c> if seed was skipped.</summary>
    public string? ClientSecret { get; init; }
}
