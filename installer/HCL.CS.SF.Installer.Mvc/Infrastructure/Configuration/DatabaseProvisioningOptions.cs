/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCLCSSFInstallerMVC.Infrastructure.Configuration;

/// <summary>
/// Configuration options controlling whether the installer is allowed to automatically create
/// the target database if it does not already exist.
/// </summary>
public sealed class DatabaseProvisioningOptions
{
    /// <summary>When <c>true</c>, provisioners will create the database if it is missing. Defaults to <c>true</c>.</summary>
    public bool AllowDatabaseCreation { get; set; } = true;
}
