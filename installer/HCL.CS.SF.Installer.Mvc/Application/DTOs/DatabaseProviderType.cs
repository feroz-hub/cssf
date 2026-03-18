/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCLCSSFInstallerMVC.Application.DTOs;

/// <summary>
/// Enumerates the database engines supported by the installer for schema creation and data seeding.
/// </summary>
public enum DatabaseProviderType
{
    /// <summary>Microsoft SQL Server.</summary>
    SqlServer = 1,
    /// <summary>MySQL / MariaDB.</summary>
    MySql = 2,
    /// <summary>PostgreSQL.</summary>
    PostgreSql = 3,
    /// <summary>SQLite (file-based).</summary>
    Sqlite = 4
}
