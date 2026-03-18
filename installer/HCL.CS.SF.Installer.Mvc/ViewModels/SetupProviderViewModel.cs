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
/// View model for wizard step 1 (Database Provider Selection).
/// Captures the user's choice of database engine.
/// </summary>
public sealed class SetupProviderViewModel
{
    /// <summary>The selected database provider (SqlServer, MySql, PostgreSql, or Sqlite).</summary>
    public DatabaseProviderType? Provider { get; set; }
}
