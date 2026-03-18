/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCLCSSFInstallerMVC.Infrastructure.Configuration;

/// <summary>
/// Configuration options for the installation lock marker file that prevents reinstallation.
/// </summary>
public sealed class InstallerLockOptions
{
    /// <summary>
    /// Relative or absolute path to the JSON lock file written upon installation completion.
    /// Defaults to <c>App_Data/installer.lock.json</c>.
    /// </summary>
    public string MarkerFilePath { get; set; } = "App_Data/installer.lock.json";
}
