/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.Extensions.Logging.Abstractions;
using HCLCSSFInstallerMVC.Application.Abstractions;

namespace HCLCSSFInstallerMVC.Infrastructure.Services.DatabaseProvisioning;

/// <summary>
/// Factory that creates the appropriate <see cref="IDatabaseProvisioner"/> implementation
/// based on the database provider name string.
/// </summary>
public static class DatabaseProvisionerFactory
{
    /// <summary>
    /// Creates a provisioner using defaults (NullLoggerFactory, database creation allowed).
    /// </summary>
    /// <param name="provider">Provider name (e.g., "SqlServer", "PostgreSql", "MySql", "Sqlite").</param>
    public static IDatabaseProvisioner Create(string provider)
    {
        return Create(provider, NullLoggerFactory.Instance, true);
    }

    /// <summary>
    /// Creates a provisioner for the specified provider with explicit logging and creation control.
    /// </summary>
    /// <param name="provider">Provider name (case-insensitive).</param>
    /// <param name="loggerFactory">Logger factory for the provisioner instance.</param>
    /// <param name="allowDatabaseCreation">Whether the provisioner may create the database if missing.</param>
    public static IDatabaseProvisioner Create(string provider, ILoggerFactory loggerFactory, bool allowDatabaseCreation)
    {
        var normalizedProvider = (provider ?? string.Empty).Trim().ToUpperInvariant();

        return normalizedProvider switch
        {
            "POSTGRESQL" or "POSTGRESSQL" or "POSTGRES" =>
                new PostgreSqlProvisioner(loggerFactory.CreateLogger<PostgreSqlProvisioner>(), allowDatabaseCreation),
            "SQLSERVER" =>
                new SqlServerProvisioner(loggerFactory.CreateLogger<SqlServerProvisioner>(), allowDatabaseCreation),
            "MYSQL" =>
                new MySqlProvisioner(loggerFactory.CreateLogger<MySqlProvisioner>(), allowDatabaseCreation),
            "SQLITE" =>
                new SqliteProvisioner(loggerFactory.CreateLogger<SqliteProvisioner>()),
            _ => throw new NotSupportedException($"Database provider '{provider}' is not supported for provisioning.")
        };
    }
}
