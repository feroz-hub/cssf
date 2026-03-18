/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCLCSSFInstallerMVC.Application.DTOs;

/// <summary>
/// Result of applying EF Core migrations during the installer workflow.
/// </summary>
public sealed class MigrationExecutionResultDto
{
    /// <summary>Whether all migrations and post-migration patches were applied successfully.</summary>
    public bool Succeeded { get; init; }

    /// <summary>Diagnostic message when migration failed; <c>null</c> on success.</summary>
    public string? ErrorMessage { get; init; }
}
