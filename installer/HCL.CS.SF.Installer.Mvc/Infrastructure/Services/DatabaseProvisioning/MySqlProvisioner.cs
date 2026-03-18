/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using MySql.Data.MySqlClient;
using HCLCSSFInstallerMVC.Application.Abstractions;
using HCLCSSFInstallerMVC.Application.DTOs;

namespace HCLCSSFInstallerMVC.Infrastructure.Services.DatabaseProvisioning;

/// <summary>
/// Provisions a MySQL/MariaDB database by checking INFORMATION_SCHEMA.SCHEMATA
/// and issuing CREATE DATABASE IF NOT EXISTS when auto-creation is allowed.
/// </summary>
public sealed class MySqlProvisioner : IDatabaseProvisioner
{
    private readonly bool _allowDatabaseCreation;
    private readonly ILogger<MySqlProvisioner> _logger;

    /// <summary>
    /// Initializes the MySQL provisioner.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <param name="allowDatabaseCreation">Whether creating a missing database is permitted.</param>
    public MySqlProvisioner(ILogger<MySqlProvisioner> logger, bool allowDatabaseCreation)
    {
        _logger = logger;
        _allowDatabaseCreation = allowDatabaseCreation;
    }

    /// <inheritdoc />
    public async Task EnsureDatabaseExistsAsync(DatabaseConfigurationDto configuration,
        CancellationToken cancellationToken)
    {
        // Extract the target database name and build an admin connection without a default database
        var targetBuilder = new MySqlConnectionStringBuilder(configuration.ConnectionString);
        if (string.IsNullOrWhiteSpace(targetBuilder.Database))
            throw new InvalidOperationException("Database name is required in MySQL connection string.");

        var targetDatabase = targetBuilder.Database;
        var adminBuilder = new MySqlConnectionStringBuilder(configuration.ConnectionString)
        {
            Database = string.Empty
        };

        _logger.LogInformation(
            "Ensuring MySQL database exists. Database: {Database}. AllowDatabaseCreation: {AllowDatabaseCreation}",
            targetDatabase,
            _allowDatabaseCreation);

        await using var connection = new MySqlConnection(adminBuilder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var existsCommand = connection.CreateCommand();
        existsCommand.CommandText = """
                                    SELECT SCHEMA_NAME
                                    FROM INFORMATION_SCHEMA.SCHEMATA
                                    WHERE SCHEMA_NAME = @databaseName
                                    LIMIT 1;
                                    """;
        existsCommand.Parameters.AddWithValue("@databaseName", targetDatabase);

        var exists = await existsCommand.ExecuteScalarAsync(cancellationToken) is not null;
        if (exists)
        {
            _logger.LogInformation("MySQL database already exists. Database: {Database}", targetDatabase);
            return;
        }

        if (!_allowDatabaseCreation)
            throw new InvalidOperationException(
                $"Database '{targetDatabase}' does not exist and automatic creation is disabled.");

        var escapedIdentifier = targetDatabase.Replace("`", "``", StringComparison.Ordinal);
        await using var createCommand = connection.CreateCommand();
        createCommand.CommandText = $"CREATE DATABASE IF NOT EXISTS `{escapedIdentifier}`;";
        await createCommand.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogInformation("Created MySQL database. Database: {Database}", targetDatabase);
    }
}
