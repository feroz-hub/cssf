/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Npgsql;
using HCLCSSFInstallerMVC.Application.Abstractions;
using HCLCSSFInstallerMVC.Application.DTOs;

namespace HCLCSSFInstallerMVC.Infrastructure.Services.DatabaseProvisioning;

/// <summary>
/// Provisions a PostgreSQL database by querying pg_database on the "postgres" admin database
/// and issuing CREATE DATABASE when auto-creation is allowed.
/// </summary>
public sealed class PostgreSqlProvisioner : IDatabaseProvisioner
{
    private readonly bool _allowDatabaseCreation;
    private readonly ILogger<PostgreSqlProvisioner> _logger;

    /// <summary>
    /// Initializes the PostgreSQL provisioner.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <param name="allowDatabaseCreation">Whether creating a missing database is permitted.</param>
    public PostgreSqlProvisioner(ILogger<PostgreSqlProvisioner> logger, bool allowDatabaseCreation)
    {
        _logger = logger;
        _allowDatabaseCreation = allowDatabaseCreation;
    }

    /// <inheritdoc />
    public async Task EnsureDatabaseExistsAsync(DatabaseConfigurationDto configuration,
        CancellationToken cancellationToken)
    {
        // Extract the target database name and build an admin connection to the "postgres" database
        var targetBuilder = new NpgsqlConnectionStringBuilder(configuration.ConnectionString);
        if (string.IsNullOrWhiteSpace(targetBuilder.Database))
            throw new InvalidOperationException("Database name is required in PostgreSQL connection string.");

        var targetDatabase = targetBuilder.Database;
        var adminBuilder = new NpgsqlConnectionStringBuilder(configuration.ConnectionString)
        {
            Database = "postgres"
        };

        _logger.LogInformation(
            "Ensuring PostgreSQL database exists. Database: {Database}. AllowDatabaseCreation: {AllowDatabaseCreation}",
            targetDatabase,
            _allowDatabaseCreation);

        await using var connection = new NpgsqlConnection(adminBuilder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var existsCommand = new NpgsqlCommand(
            "SELECT 1 FROM pg_database WHERE datname = @databaseName;",
            connection);
        existsCommand.Parameters.AddWithValue("databaseName", targetDatabase);

        var exists = await existsCommand.ExecuteScalarAsync(cancellationToken) is not null;
        if (exists)
        {
            _logger.LogInformation("PostgreSQL database already exists. Database: {Database}", targetDatabase);
            return;
        }

        if (!_allowDatabaseCreation)
            throw new InvalidOperationException(
                $"Database '{targetDatabase}' does not exist and automatic creation is disabled.");

        var escapedIdentifier = targetDatabase.Replace("\"", "\"\"", StringComparison.Ordinal);
        await using var createCommand = new NpgsqlCommand($"CREATE DATABASE \"{escapedIdentifier}\";", connection);
        await createCommand.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogInformation("Created PostgreSQL database. Database: {Database}", targetDatabase);
    }
}
