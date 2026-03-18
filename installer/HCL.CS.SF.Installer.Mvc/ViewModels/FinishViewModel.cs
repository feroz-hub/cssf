/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCLCSSFInstallerMVC.ViewModels;

/// <summary>
/// View model for wizard step 6 (Completion / Installed).
/// Shows installation status, generated client credentials, and a summary message.
/// </summary>
public sealed class FinishViewModel
{
    /// <summary>Whether the installation process has been finalized.</summary>
    public bool InstallationCompleted { get; set; }

    /// <summary>Whether this page is being shown because setup was already locked.</summary>
    public bool AlreadyInstalled { get; set; }

    /// <summary>Human-readable status message displayed on the completion page.</summary>
    public string? Message { get; set; }

    /// <summary>Name of the database provider used for the installation.</summary>
    public string? DatabaseProvider { get; set; }

    /// <summary>Generated OAuth client ID to show the administrator.</summary>
    public string? ClientId { get; set; }

    /// <summary>Generated OAuth client secret (plain-text) to show the administrator.</summary>
    public string? ClientSecret { get; set; }

    /// <summary>UTC timestamp when the installation was completed.</summary>
    public DateTimeOffset? CompletedOnUtc { get; set; }
}
