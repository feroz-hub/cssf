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
/// View model for wizard step 2 (Connection String Input).
/// Collects the ADO.NET connection string for the previously selected database provider.
/// </summary>
public sealed class SetupConnectionViewModel
{
    /// <summary>The database provider selected in step 1.</summary>
    public DatabaseProviderType? Provider { get; set; }

    /// <summary>The ADO.NET connection string entered by the user.</summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>Whether the app is running inside a Docker container, used to suggest appropriate hostnames.</summary>
    public bool IsRunningInContainer { get; set; }
}
