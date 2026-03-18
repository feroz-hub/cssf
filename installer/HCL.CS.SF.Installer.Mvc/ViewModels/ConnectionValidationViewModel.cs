/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCLCSSFInstallerMVC.Application.DTOs;

namespace HCLCSSFInstallerMVC.ViewModels;

/// <summary>
/// View model for wizard step 3 (Connection Validation).
/// Displays the connection string being tested and the validation result.
/// </summary>
public sealed class ConnectionValidationViewModel
{
    /// <summary>Whether a database configuration has been saved in an earlier step.</summary>
    public bool HasConfiguration { get; set; }

    /// <summary>The selected database provider.</summary>
    public DatabaseProviderType? Provider { get; set; }

    /// <summary>The connection string under test.</summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>Whether the application is running inside a Docker container.</summary>
    public bool IsRunningInContainer { get; set; }

    /// <summary>Whether a validation attempt has been executed.</summary>
    public bool IsValidated { get; set; }

    /// <summary>Whether the most recent validation attempt succeeded.</summary>
    public bool IsSuccessful { get; set; }

    /// <summary>Error message from the validation attempt, if it failed.</summary>
    public string? ErrorMessage { get; set; }
}
