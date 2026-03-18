/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.EntityFrameworkCore;
using HCLCSSFInstallerMVC.Application.DTOs;
using HCLCSSFInstallerMVC.Infrastructure.Persistence.Data;

namespace HCLCSSFInstallerMVC.Infrastructure.Services;

/// <summary>
/// Utility methods for building EF Core DbContext options and creating provider-specific
/// migration DbContext instances based on the selected database provider.
/// </summary>
internal static class DatabaseProviderUtilities
{
    /// <summary>
    /// Builds <see cref="DbContextOptions{ApplicationDbContext}"/> for the given provider and connection string.
    /// Used for connection validation and seed operations.
    /// </summary>
    public static DbContextOptions<ApplicationDbContext> BuildApplicationOptions(DatabaseConfigurationDto configuration)
    {
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

        switch (configuration.Provider)
        {
            case DatabaseProviderType.SqlServer:
                builder.UseSqlServer(configuration.ConnectionString);
                break;
            case DatabaseProviderType.MySql:
                builder.UseMySql(configuration.ConnectionString,
                    ServerVersion.AutoDetect(configuration.ConnectionString));
                break;
            case DatabaseProviderType.PostgreSql:
                builder.UseNpgsql(configuration.ConnectionString);
                break;
            case DatabaseProviderType.Sqlite:
                builder.UseSqlite(configuration.ConnectionString);
                break;
            default:
                throw new InvalidOperationException($"Unsupported database provider: {configuration.Provider}");
        }

        return builder.Options;
    }

    /// <summary>
    /// Creates a provider-specific DbContext instance for running migrations.
    /// Each provider has its own DbContext subclass to hold provider-specific migration history.
    /// </summary>
    public static DbContext CreateMigrationDbContext(DatabaseConfigurationDto configuration)
    {
        return configuration.Provider switch
        {
            DatabaseProviderType.SqlServer => CreateSqlServerMigrationDbContext(configuration.ConnectionString),
            DatabaseProviderType.MySql => CreateMySqlMigrationDbContext(configuration.ConnectionString),
            DatabaseProviderType.PostgreSql => CreatePostgreSqlMigrationDbContext(configuration.ConnectionString),
            DatabaseProviderType.Sqlite => CreateSqliteMigrationDbContext(configuration.ConnectionString),
            _ => throw new InvalidOperationException($"Unsupported database provider: {configuration.Provider}")
        };
    }

    private static SqlServerApplicationDbContext CreateSqlServerMigrationDbContext(string connectionString)
    {
        var builder = new DbContextOptionsBuilder<SqlServerApplicationDbContext>();
        builder.UseSqlServer(connectionString);
        return new SqlServerApplicationDbContext(builder.Options);
    }

    private static MySqlApplicationDbContext CreateMySqlMigrationDbContext(string connectionString)
    {
        var builder = new DbContextOptionsBuilder<MySqlApplicationDbContext>();
        builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        return new MySqlApplicationDbContext(builder.Options);
    }

    private static PostgreSqlApplicationDbcontext CreatePostgreSqlMigrationDbContext(string connectionString)
    {
        var builder = new DbContextOptionsBuilder<PostgreSqlApplicationDbcontext>();
        builder.UseNpgsql(connectionString);
        return new PostgreSqlApplicationDbcontext(builder.Options);
    }

    private static SqLiteApplicationDbContext CreateSqliteMigrationDbContext(string connectionString)
    {
        var builder = new DbContextOptionsBuilder<SqLiteApplicationDbContext>();
        builder.UseSqlite(connectionString);
        return new SqLiteApplicationDbContext(builder.Options);
    }
}
