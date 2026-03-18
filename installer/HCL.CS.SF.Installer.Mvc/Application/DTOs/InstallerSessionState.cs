/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCLCSSFInstallerMVC.Application.DTOs;

/// <summary>
/// Tracks the installer wizard progress across HTTP requests.
/// Stored in the encrypted session via <see cref="HCLCSSFInstallerMVC.Application.Abstractions.IInstallerStateStore"/>.
/// </summary>
public sealed class InstallerSessionState
{
    /// <summary>Database provider and connection string chosen in steps 1-2.</summary>
    public DatabaseConfigurationDto? DatabaseConfiguration { get; set; }

    /// <summary>Whether the connection validation step (step 3) passed successfully.</summary>
    public bool DatabaseConnectionValidated { get; set; }

    /// <summary>Whether the migration step (step 4) completed successfully.</summary>
    public bool MigrationCompleted { get; set; }

    /// <summary>Result of the seed step (step 5), including generated client credentials.</summary>
    public SeedExecutionResultDto? SeedResult { get; set; }
}
