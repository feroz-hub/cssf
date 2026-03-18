/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCLCSSFInstallerMVC.Application.DTOs;

/// <summary>
/// Holds the database provider selection and connection string chosen during the installer wizard.
/// </summary>
public sealed class DatabaseConfigurationDto
{
    /// <summary>The database engine type (SqlServer, MySql, PostgreSql, or Sqlite).</summary>
    public DatabaseProviderType Provider { get; init; }

    /// <summary>ADO.NET connection string for the selected provider.</summary>
    public string ConnectionString { get; init; } = string.Empty;
}
