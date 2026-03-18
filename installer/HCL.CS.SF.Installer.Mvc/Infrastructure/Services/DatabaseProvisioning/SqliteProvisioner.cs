/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.Data.Sqlite;
using HCLCSSFInstallerMVC.Application.Abstractions;
using HCLCSSFInstallerMVC.Application.DTOs;

namespace HCLCSSFInstallerMVC.Infrastructure.Services.DatabaseProvisioning;

/// <summary>
/// Provisions a SQLite database. SQLite creates the database file automatically when
/// the connection is opened, so this provisioner is a no-op that logs the data source path.
/// </summary>
public sealed class SqliteProvisioner : IDatabaseProvisioner
{
    private readonly ILogger<SqliteProvisioner> _logger;

    /// <summary>
    /// Initializes the SQLite provisioner.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    public SqliteProvisioner(ILogger<SqliteProvisioner> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task EnsureDatabaseExistsAsync(DatabaseConfigurationDto configuration, CancellationToken cancellationToken)
    {
        var builder = new SqliteConnectionStringBuilder(configuration.ConnectionString);
        _logger.LogInformation(
            "SQLite provisioning does not require explicit database creation. DataSource: {DataSource}",
            builder.DataSource);

        return Task.CompletedTask;
    }
}
