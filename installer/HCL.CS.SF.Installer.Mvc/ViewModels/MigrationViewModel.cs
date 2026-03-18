/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCLCSSFInstallerMVC.ViewModels;

/// <summary>
/// View model for wizard step 4 (Migration Execution).
/// Tracks whether migrations can be run and whether they have completed.
/// </summary>
public sealed class MigrationViewModel
{
    /// <summary>Whether the prerequisite (connection validation) has passed, allowing migration to run.</summary>
    public bool CanRun { get; set; }

    /// <summary>Whether all migrations have been applied successfully.</summary>
    public bool IsCompleted { get; set; }

    /// <summary>Error message if the migration failed.</summary>
    public string? ErrorMessage { get; set; }
}
