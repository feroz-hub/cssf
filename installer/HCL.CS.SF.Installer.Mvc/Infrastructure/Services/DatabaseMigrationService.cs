/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using HCLCSSFInstallerMVC.Application.Abstractions;
using HCLCSSFInstallerMVC.Application.DTOs;
using HCLCSSFInstallerMVC.Infrastructure.Configuration;
using HCLCSSFInstallerMVC.Infrastructure.Persistence.Data;
using HCLCSSFInstallerMVC.Infrastructure.Services.DatabaseProvisioning;

namespace HCLCSSFInstallerMVC.Infrastructure.Services;

/// <summary>
/// Implements database connection validation and EF Core migration execution.
/// Provisions the database if needed, runs all pending migrations, and applies
/// post-migration DDL patches for schema additions introduced after the initial migration set.
/// </summary>
public sealed class DatabaseMigrationService : IDatabaseMigrationService
{
    private readonly ILogger<DatabaseMigrationService> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly DatabaseProvisioningOptions _provisioningOptions;

    /// <summary>
    /// Initializes the service with logging and provisioning configuration.
    /// </summary>
    public DatabaseMigrationService(
        ILogger<DatabaseMigrationService> logger,
        ILoggerFactory loggerFactory,
        IOptions<DatabaseProvisioningOptions> provisioningOptions)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _provisioningOptions = provisioningOptions.Value;
    }

    /// <inheritdoc />
    public async Task<ConnectionValidationResultDto> ValidateConnectionAsync(
        DatabaseConfigurationDto configuration,
        CancellationToken cancellationToken)
    {
        try
        {
            var provisioner = CreateProvisioner(configuration);
            await provisioner.EnsureDatabaseExistsAsync(configuration, cancellationToken);

            var options = DatabaseProviderUtilities.BuildApplicationOptions(configuration);
            await using var dbContext = new ApplicationDbContext(options);

            await dbContext.Database.OpenConnectionAsync(cancellationToken);
            await dbContext.Database.CloseConnectionAsync();

            _logger.LogInformation(
                "Database connectivity validated for provider {Provider}.",
                configuration.Provider);

            return new ConnectionValidationResultDto { Succeeded = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Database connection validation failed for provider {Provider}. AllowDatabaseCreation: {AllowDatabaseCreation}.",
                configuration.Provider,
                _provisioningOptions.AllowDatabaseCreation);
            return new ConnectionValidationResultDto
            {
                Succeeded = false,
                ErrorMessage = GetFailureMessage(ex,
                    "Invalid connection string, missing database, or database endpoint is unavailable.")
            };
        }
    }

    /// <inheritdoc />
    public async Task<MigrationExecutionResultDto> RunMigrationsAsync(
        DatabaseConfigurationDto configuration,
        CancellationToken cancellationToken)
    {
        try
        {
            var provisioner = CreateProvisioner(configuration);
            await provisioner.EnsureDatabaseExistsAsync(configuration, cancellationToken);

            await using var migrationContext = DatabaseProviderUtilities.CreateMigrationDbContext(configuration);
            migrationContext.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));
            await migrationContext.Database.MigrateAsync(cancellationToken);
            await EnsureSecurityTokenColumnsAsync(migrationContext, configuration.Provider, cancellationToken);
            await EnsureExternalIdentitySchemaAsync(migrationContext, configuration.Provider, cancellationToken);
            await EnsurePreferredAudienceColumnAsync(migrationContext, configuration.Provider, cancellationToken);
            await EnsureNotificationProviderConfigSchemaAsync(migrationContext, configuration.Provider, cancellationToken);
            await EnsureExternalAuthProviderConfigSchemaAsync(migrationContext, configuration.Provider, cancellationToken);

            _logger.LogInformation("Database migrations completed successfully for provider {Provider}.",
                configuration.Provider);

            return new MigrationExecutionResultDto { Succeeded = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Migration execution failed for provider {Provider}. AllowDatabaseCreation: {AllowDatabaseCreation}.",
                configuration.Provider,
                _provisioningOptions.AllowDatabaseCreation);
            return new MigrationExecutionResultDto
            {
                Succeeded = false,
                ErrorMessage = GetFailureMessage(ex,
                    "Migration execution failed. Verify credentials, provider configuration, and database permissions.")
            };
        }
    }

    /// <summary>Creates the appropriate database provisioner for the configured provider.</summary>
    private IDatabaseProvisioner CreateProvisioner(DatabaseConfigurationDto configuration)
    {
        return DatabaseProvisionerFactory.Create(
            configuration.Provider.ToString(),
            _loggerFactory,
            _provisioningOptions.AllowDatabaseCreation);
    }

    /// <summary>
    /// Post-migration patch: adds ConsumedAt and TokenReuseDetected columns to SecurityTokens
    /// if they do not already exist. Uses provider-specific DDL syntax.
    /// </summary>
    private async Task EnsureSecurityTokenColumnsAsync(
        DbContext migrationContext,
        DatabaseProviderType provider,
        CancellationToken cancellationToken)
    {
        var statements = provider switch
        {
            DatabaseProviderType.SqlServer => new[]
            {
                "IF COL_LENGTH('HCL.CS.SF_SecurityTokens', 'ConsumedAt') IS NULL ALTER TABLE [HCL.CS.SF_SecurityTokens] ADD [ConsumedAt] datetime2 NULL;",
                "IF COL_LENGTH('HCL.CS.SF_SecurityTokens', 'TokenReuseDetected') IS NULL ALTER TABLE [HCL.CS.SF_SecurityTokens] ADD [TokenReuseDetected] bit NOT NULL CONSTRAINT [DF_HCL.CS.SF_SecurityTokens_TokenReuseDetected] DEFAULT 0;"
            },
            DatabaseProviderType.MySql => new[]
            {
                "ALTER TABLE `HCL.CS.SF_SecurityTokens` ADD COLUMN IF NOT EXISTS `ConsumedAt` datetime(6) NULL;",
                "ALTER TABLE `HCL.CS.SF_SecurityTokens` ADD COLUMN IF NOT EXISTS `TokenReuseDetected` tinyint(1) NOT NULL DEFAULT FALSE;"
            },
            DatabaseProviderType.PostgreSql => new[]
            {
                "ALTER TABLE \"HCL.CS.SF_SecurityTokens\" ADD COLUMN IF NOT EXISTS \"ConsumedAt\" timestamp without time zone NULL;",
                "ALTER TABLE \"HCL.CS.SF_SecurityTokens\" ADD COLUMN IF NOT EXISTS \"TokenReuseDetected\" boolean NOT NULL DEFAULT FALSE;"
            },
            DatabaseProviderType.Sqlite => new[]
            {
                "ALTER TABLE \"HCL.CS.SF_SecurityTokens\" ADD COLUMN \"ConsumedAt\" TEXT NULL;",
                "ALTER TABLE \"HCL.CS.SF_SecurityTokens\" ADD COLUMN \"TokenReuseDetected\" INTEGER NOT NULL DEFAULT 0;"
            },
            _ => Array.Empty<string>()
        };

        foreach (var statement in statements)
            try
            {
                await migrationContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
            }
            catch (Exception ex) when (provider == DatabaseProviderType.Sqlite
                                       && ex.Message.Contains("duplicate column name",
                                           StringComparison.OrdinalIgnoreCase))
            {
                // SQLite lacks portable IF NOT EXISTS support for ADD COLUMN across versions.
            }
    }

    /// <summary>
    /// Post-migration patch: adds the PreferredAudience column to the Clients table
    /// if it does not already exist.
    /// </summary>
    private async Task EnsurePreferredAudienceColumnAsync(
        DbContext migrationContext,
        DatabaseProviderType provider,
        CancellationToken cancellationToken)
    {
        var statements = provider switch
        {
            DatabaseProviderType.SqlServer => new[]
            {
                "IF COL_LENGTH('HCL.CS.SF_Clients', 'PreferredAudience') IS NULL ALTER TABLE [HCL.CS.SF_Clients] ADD [PreferredAudience] nvarchar(300) NULL;"
            },
            DatabaseProviderType.MySql => new[]
            {
                "ALTER TABLE `HCL.CS.SF_Clients` ADD COLUMN IF NOT EXISTS `PreferredAudience` varchar(300) NULL;"
            },
            DatabaseProviderType.PostgreSql => new[]
            {
                "ALTER TABLE \"HCL.CS.SF_Clients\" ADD COLUMN IF NOT EXISTS \"PreferredAudience\" varchar(300) NULL;"
            },
            DatabaseProviderType.Sqlite => new[]
            {
                "ALTER TABLE \"HCL.CS.SF_Clients\" ADD COLUMN \"PreferredAudience\" TEXT NULL;"
            },
            _ => Array.Empty<string>()
        };

        foreach (var statement in statements)
            try
            {
                await migrationContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
            }
            catch (Exception ex) when (provider == DatabaseProviderType.Sqlite
                                        && ex.Message.Contains("duplicate column name",
                                            StringComparison.OrdinalIgnoreCase))
            {
                // SQLite lacks IF NOT EXISTS for ADD COLUMN.
            }
    }

    /// <summary>
    /// Returns a user-friendly failure message, surfacing the exception message for known
    /// exception types and using a generic default otherwise.
    /// </summary>
    private static string GetFailureMessage(Exception exception, string defaultMessage)
    {
        return exception switch
        {
            NotSupportedException => exception.Message,
            InvalidOperationException => exception.Message,
            _ => defaultMessage
        };
    }

    /// <summary>
    /// Post-migration patch: creates the ExternalIdentities table and its indexes
    /// if the table does not already exist.
    /// </summary>
    private async Task EnsureExternalIdentitySchemaAsync(
        DbContext migrationContext,
        DatabaseProviderType provider,
        CancellationToken cancellationToken)
    {
        var statements = provider switch
        {
            DatabaseProviderType.SqlServer => new[]
            {
                """
                IF OBJECT_ID(N'[HCL.CS.SF_ExternalIdentities]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [HCL.CS.SF_ExternalIdentities] (
                        [Id] uniqueidentifier NOT NULL,
                        [IsDeleted] bit NOT NULL CONSTRAINT [DF_HCL.CS.SF_ExternalIdentities_IsDeleted] DEFAULT (0),
                        [CreatedOn] datetime2 NOT NULL,
                        [ModifiedOn] datetime2 NULL,
                        [CreatedBy] nvarchar(255) NOT NULL,
                        [ModifiedBy] nvarchar(255) NULL,
                        [RowVersion] rowversion,
                        [UserId] uniqueidentifier NOT NULL,
                        [TenantId] nvarchar(128) NULL,
                        [Provider] nvarchar(64) NOT NULL,
                        [Issuer] nvarchar(256) NOT NULL,
                        [Subject] nvarchar(256) NOT NULL,
                        [Email] nvarchar(255) NOT NULL,
                        [EmailVerified] bit NOT NULL,
                        [LinkedAt] datetime2 NOT NULL,
                        [LastSignInAt] datetime2 NULL,
                        CONSTRAINT [PK_HCL.CS.SF_ExternalIdentities] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_HCL.CS.SF_ExternalIdentities_HCL.CS.SF_Users_UserId]
                            FOREIGN KEY ([UserId]) REFERENCES [HCL.CS.SF_Users] ([Id]) ON DELETE NO ACTION
                    );
                END;
                """,
                """
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EXTID_PROVIDER_ISSUER_SUBJECT' AND object_id = OBJECT_ID('HCL.CS.SF_ExternalIdentities'))
                    CREATE UNIQUE INDEX [IX_EXTID_PROVIDER_ISSUER_SUBJECT] ON [HCL.CS.SF_ExternalIdentities] ([Provider], [Issuer], [Subject]);
                """,
                """
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EXTID_USERID' AND object_id = OBJECT_ID('HCL.CS.SF_ExternalIdentities'))
                    CREATE INDEX [IX_EXTID_USERID] ON [HCL.CS.SF_ExternalIdentities] ([UserId]);
                """,
                """
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EXTID_TENANT_EMAIL' AND object_id = OBJECT_ID('HCL.CS.SF_ExternalIdentities'))
                    CREATE INDEX [IX_EXTID_TENANT_EMAIL] ON [HCL.CS.SF_ExternalIdentities] ([TenantId], [Email]);
                """
            },
            DatabaseProviderType.MySql => new[]
            {
                """
                CREATE TABLE IF NOT EXISTS `HCL.CS.SF_ExternalIdentities` (
                    `Id` char(36) NOT NULL,
                    `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
                    `CreatedOn` datetime(6) NOT NULL,
                    `ModifiedOn` datetime(6) NULL,
                    `CreatedBy` varchar(255) NOT NULL,
                    `ModifiedBy` varchar(255) NULL,
                    `RowVersion` varbinary(16) NULL,
                    `UserId` char(36) NOT NULL,
                    `TenantId` varchar(128) NULL,
                    `Provider` varchar(64) NOT NULL,
                    `Issuer` varchar(256) NOT NULL,
                    `Subject` varchar(256) NOT NULL,
                    `Email` varchar(255) NOT NULL,
                    `EmailVerified` tinyint(1) NOT NULL,
                    `LinkedAt` datetime(6) NOT NULL,
                    `LastSignInAt` datetime(6) NULL,
                    PRIMARY KEY (`Id`),
                    CONSTRAINT `FK_HCL.CS.SF_ExternalIdentities_HCL.CS.SF_Users_UserId`
                        FOREIGN KEY (`UserId`) REFERENCES `HCL.CS.SF_Users` (`Id`) ON DELETE RESTRICT
                );
                """,
                "CREATE UNIQUE INDEX `IX_EXTID_PROVIDER_ISSUER_SUBJECT` ON `HCL.CS.SF_ExternalIdentities` (`Provider`, `Issuer`, `Subject`);",
                "CREATE INDEX `IX_EXTID_USERID` ON `HCL.CS.SF_ExternalIdentities` (`UserId`);",
                "CREATE INDEX `IX_EXTID_TENANT_EMAIL` ON `HCL.CS.SF_ExternalIdentities` (`TenantId`, `Email`);"
            },
            DatabaseProviderType.PostgreSql => new[]
            {
                """
                CREATE TABLE IF NOT EXISTS "HCL.CS.SF_ExternalIdentities" (
                    "Id" uuid NOT NULL PRIMARY KEY,
                    "IsDeleted" boolean NOT NULL DEFAULT FALSE,
                    "CreatedOn" timestamp without time zone NOT NULL,
                    "ModifiedOn" timestamp without time zone NULL,
                    "CreatedBy" varchar(255) NOT NULL,
                    "ModifiedBy" varchar(255) NULL,
                    "RowVersion" bytea NULL,
                    "UserId" uuid NOT NULL,
                    "TenantId" varchar(128) NULL,
                    "Provider" varchar(64) NOT NULL,
                    "Issuer" varchar(256) NOT NULL,
                    "Subject" varchar(256) NOT NULL,
                    "Email" varchar(255) NOT NULL,
                    "EmailVerified" boolean NOT NULL,
                    "LinkedAt" timestamp without time zone NOT NULL,
                    "LastSignInAt" timestamp without time zone NULL,
                    CONSTRAINT "FK_HCL.CS.SF_ExternalIdentities_HCL.CS.SF_Users_UserId"
                        FOREIGN KEY ("UserId") REFERENCES "HCL.CS.SF_Users" ("Id") ON DELETE RESTRICT
                );
                """,
                """
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_EXTID_PROVIDER_ISSUER_SUBJECT"
                    ON "HCL.CS.SF_ExternalIdentities" ("Provider", "Issuer", "Subject");
                """,
                """
                CREATE INDEX IF NOT EXISTS "IX_EXTID_USERID"
                    ON "HCL.CS.SF_ExternalIdentities" ("UserId");
                """,
                """
                CREATE INDEX IF NOT EXISTS "IX_EXTID_TENANT_EMAIL"
                    ON "HCL.CS.SF_ExternalIdentities" ("TenantId", "Email");
                """
            },
            DatabaseProviderType.Sqlite => new[]
            {
                """
                CREATE TABLE IF NOT EXISTS "HCL.CS.SF_ExternalIdentities" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_HCL.CS.SF_ExternalIdentities" PRIMARY KEY,
                    "IsDeleted" INTEGER NOT NULL DEFAULT 0,
                    "CreatedOn" TEXT NOT NULL,
                    "ModifiedOn" TEXT NULL,
                    "CreatedBy" TEXT NOT NULL,
                    "ModifiedBy" TEXT NULL,
                    "RowVersion" BLOB NULL,
                    "UserId" TEXT NOT NULL,
                    "TenantId" TEXT NULL,
                    "Provider" TEXT NOT NULL,
                    "Issuer" TEXT NOT NULL,
                    "Subject" TEXT NOT NULL,
                    "Email" TEXT NOT NULL,
                    "EmailVerified" INTEGER NOT NULL,
                    "LinkedAt" TEXT NOT NULL,
                    "LastSignInAt" TEXT NULL,
                    CONSTRAINT "FK_HCL.CS.SF_ExternalIdentities_HCL.CS.SF_Users_UserId"
                        FOREIGN KEY ("UserId") REFERENCES "HCL.CS.SF_Users" ("Id") ON DELETE RESTRICT
                );
                """,
                """
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_EXTID_PROVIDER_ISSUER_SUBJECT"
                    ON "HCL.CS.SF_ExternalIdentities" ("Provider", "Issuer", "Subject");
                """,
                """
                CREATE INDEX IF NOT EXISTS "IX_EXTID_USERID"
                    ON "HCL.CS.SF_ExternalIdentities" ("UserId");
                """,
                """
                CREATE INDEX IF NOT EXISTS "IX_EXTID_TENANT_EMAIL"
                    ON "HCL.CS.SF_ExternalIdentities" ("TenantId", "Email");
                """
            },
            _ => Array.Empty<string>()
        };

        foreach (var statement in statements)
            try
            {
                await migrationContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
            }
            catch (Exception ex) when (provider == DatabaseProviderType.MySql
                                       && ex.Message.Contains("Duplicate key name", StringComparison.OrdinalIgnoreCase))
            {
                // MySQL CREATE INDEX does not support IF NOT EXISTS on older engines.
            }
    }

    /// <summary>
    /// Post-migration patch: creates the NotificationProviderConfig table and its indexes
    /// if the table does not already exist.
    /// </summary>
    private async Task EnsureNotificationProviderConfigSchemaAsync(
        DbContext migrationContext,
        DatabaseProviderType provider,
        CancellationToken cancellationToken)
    {
        var statements = provider switch
        {
            DatabaseProviderType.SqlServer => new[]
            {
                """
                IF OBJECT_ID(N'[HCL.CS.SF_NotificationProviderConfig]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [HCL.CS.SF_NotificationProviderConfig] (
                        [Id] uniqueidentifier NOT NULL,
                        [IsDeleted] bit NOT NULL CONSTRAINT [DF_HCL.CS.SF_NPC_IsDeleted] DEFAULT (0),
                        [CreatedOn] datetime2 NOT NULL,
                        [ModifiedOn] datetime2 NULL,
                        [CreatedBy] nvarchar(255) NOT NULL,
                        [ModifiedBy] nvarchar(255) NULL,
                        [ProviderName] nvarchar(50) NOT NULL,
                        [ChannelType] int NOT NULL,
                        [IsActive] bit NOT NULL,
                        [ConfigJson] nvarchar(max) NOT NULL,
                        [LastTestedOn] datetime2 NULL,
                        [LastTestSuccess] bit NULL,
                        CONSTRAINT [PK_HCL.CS.SF_NotificationProviderConfig] PRIMARY KEY ([Id])
                    );
                END;
                """,
                """
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_NPC_CHANNEL_TYPE' AND object_id = OBJECT_ID('HCL.CS.SF_NotificationProviderConfig'))
                    CREATE INDEX [IX_NPC_CHANNEL_TYPE] ON [HCL.CS.SF_NotificationProviderConfig] ([ChannelType]);
                """,
                """
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_NPC_CHANNEL_ACTIVE' AND object_id = OBJECT_ID('HCL.CS.SF_NotificationProviderConfig'))
                    CREATE INDEX [IX_NPC_CHANNEL_ACTIVE] ON [HCL.CS.SF_NotificationProviderConfig] ([ChannelType], [IsActive]);
                """
            },
            DatabaseProviderType.MySql => new[]
            {
                """
                CREATE TABLE IF NOT EXISTS `HCL.CS.SF_NotificationProviderConfig` (
                    `Id` char(36) NOT NULL,
                    `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
                    `CreatedOn` datetime(6) NOT NULL,
                    `ModifiedOn` datetime(6) NULL,
                    `CreatedBy` varchar(255) NOT NULL,
                    `ModifiedBy` varchar(255) NULL,
                    `ProviderName` varchar(50) NOT NULL,
                    `ChannelType` int NOT NULL,
                    `IsActive` tinyint(1) NOT NULL,
                    `ConfigJson` longtext NOT NULL,
                    `LastTestedOn` datetime(6) NULL,
                    `LastTestSuccess` tinyint(1) NULL,
                    PRIMARY KEY (`Id`)
                );
                """,
                "CREATE INDEX `IX_NPC_CHANNEL_TYPE` ON `HCL.CS.SF_NotificationProviderConfig` (`ChannelType`);",
                "CREATE INDEX `IX_NPC_CHANNEL_ACTIVE` ON `HCL.CS.SF_NotificationProviderConfig` (`ChannelType`, `IsActive`);"
            },
            DatabaseProviderType.PostgreSql => new[]
            {
                """
                CREATE TABLE IF NOT EXISTS "HCL.CS.SF_NotificationProviderConfig" (
                    "Id" uuid NOT NULL PRIMARY KEY,
                    "IsDeleted" boolean NOT NULL DEFAULT FALSE,
                    "CreatedOn" timestamp without time zone NOT NULL,
                    "ModifiedOn" timestamp without time zone NULL,
                    "CreatedBy" varchar(255) NOT NULL,
                    "ModifiedBy" varchar(255) NULL,
                    "ProviderName" varchar(50) NOT NULL,
                    "ChannelType" integer NOT NULL,
                    "IsActive" boolean NOT NULL,
                    "ConfigJson" text NOT NULL,
                    "LastTestedOn" timestamp without time zone NULL,
                    "LastTestSuccess" boolean NULL
                );
                """,
                """
                CREATE INDEX IF NOT EXISTS "IX_NPC_CHANNEL_TYPE"
                    ON "HCL.CS.SF_NotificationProviderConfig" ("ChannelType");
                """,
                """
                CREATE INDEX IF NOT EXISTS "IX_NPC_CHANNEL_ACTIVE"
                    ON "HCL.CS.SF_NotificationProviderConfig" ("ChannelType", "IsActive");
                """
            },
            DatabaseProviderType.Sqlite => new[]
            {
                """
                CREATE TABLE IF NOT EXISTS "HCL.CS.SF_NotificationProviderConfig" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_HCL.CS.SF_NotificationProviderConfig" PRIMARY KEY,
                    "IsDeleted" INTEGER NOT NULL DEFAULT 0,
                    "CreatedOn" TEXT NOT NULL,
                    "ModifiedOn" TEXT NULL,
                    "CreatedBy" TEXT NOT NULL,
                    "ModifiedBy" TEXT NULL,
                    "ProviderName" TEXT NOT NULL,
                    "ChannelType" INTEGER NOT NULL,
                    "IsActive" INTEGER NOT NULL,
                    "ConfigJson" TEXT NOT NULL,
                    "LastTestedOn" TEXT NULL,
                    "LastTestSuccess" INTEGER NULL
                );
                """,
                """
                CREATE INDEX IF NOT EXISTS "IX_NPC_CHANNEL_TYPE"
                    ON "HCL.CS.SF_NotificationProviderConfig" ("ChannelType");
                """,
                """
                CREATE INDEX IF NOT EXISTS "IX_NPC_CHANNEL_ACTIVE"
                    ON "HCL.CS.SF_NotificationProviderConfig" ("ChannelType", "IsActive");
                """
            },
            _ => Array.Empty<string>()
        };

        foreach (var statement in statements)
            try
            {
                await migrationContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
            }
            catch (Exception ex) when (provider == DatabaseProviderType.MySql
                                       && ex.Message.Contains("Duplicate key name", StringComparison.OrdinalIgnoreCase))
            {
                // MySQL CREATE INDEX does not support IF NOT EXISTS on older engines.
            }
    }

    /// <summary>
    /// Post-migration patch: creates the ExternalAuthProviderConfig table and its indexes
    /// if the table does not already exist.
    /// </summary>
    private async Task EnsureExternalAuthProviderConfigSchemaAsync(
        DbContext migrationContext,
        DatabaseProviderType provider,
        CancellationToken cancellationToken)
    {
        var statements = provider switch
        {
            DatabaseProviderType.SqlServer => new[]
            {
                """
                IF OBJECT_ID(N'[HCL.CS.SF_ExternalAuthProviderConfig]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [HCL.CS.SF_ExternalAuthProviderConfig] (
                        [Id] uniqueidentifier NOT NULL,
                        [IsDeleted] bit NOT NULL CONSTRAINT [DF_HCL.CS.SF_EAPC_IsDeleted] DEFAULT (0),
                        [CreatedOn] datetime2 NOT NULL,
                        [ModifiedOn] datetime2 NULL,
                        [CreatedBy] nvarchar(255) NOT NULL,
                        [ModifiedBy] nvarchar(255) NULL,
                        [ProviderName] nvarchar(50) NOT NULL,
                        [ProviderType] int NOT NULL,
                        [IsEnabled] bit NOT NULL,
                        [ConfigJson] nvarchar(max) NOT NULL,
                        [AutoProvisionEnabled] bit NOT NULL CONSTRAINT [DF_HCL.CS.SF_EAPC_AutoProvision] DEFAULT (0),
                        [AllowedDomains] nvarchar(2000) NULL,
                        [LastTestedOn] datetime2 NULL,
                        [LastTestSuccess] bit NULL,
                        CONSTRAINT [PK_HCL.CS.SF_ExternalAuthProviderConfig] PRIMARY KEY ([Id])
                    );
                END;
                """,
                """
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EAPC_PROVIDER' AND object_id = OBJECT_ID('HCL.CS.SF_ExternalAuthProviderConfig'))
                    CREATE UNIQUE INDEX [IX_EAPC_PROVIDER] ON [HCL.CS.SF_ExternalAuthProviderConfig] ([ProviderName]);
                """,
                """
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EAPC_PROVIDER_ENABLED' AND object_id = OBJECT_ID('HCL.CS.SF_ExternalAuthProviderConfig'))
                    CREATE INDEX [IX_EAPC_PROVIDER_ENABLED] ON [HCL.CS.SF_ExternalAuthProviderConfig] ([ProviderName], [IsEnabled]);
                """
            },
            DatabaseProviderType.MySql => new[]
            {
                """
                CREATE TABLE IF NOT EXISTS `HCL.CS.SF_ExternalAuthProviderConfig` (
                    `Id` char(36) NOT NULL,
                    `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
                    `CreatedOn` datetime(6) NOT NULL,
                    `ModifiedOn` datetime(6) NULL,
                    `CreatedBy` varchar(255) NOT NULL,
                    `ModifiedBy` varchar(255) NULL,
                    `ProviderName` varchar(50) NOT NULL,
                    `ProviderType` int NOT NULL,
                    `IsEnabled` tinyint(1) NOT NULL,
                    `ConfigJson` longtext NOT NULL,
                    `AutoProvisionEnabled` tinyint(1) NOT NULL DEFAULT 0,
                    `AllowedDomains` varchar(2000) NULL,
                    `LastTestedOn` datetime(6) NULL,
                    `LastTestSuccess` tinyint(1) NULL,
                    PRIMARY KEY (`Id`)
                );
                """,
                "CREATE UNIQUE INDEX `IX_EAPC_PROVIDER` ON `HCL.CS.SF_ExternalAuthProviderConfig` (`ProviderName`);",
                "CREATE INDEX `IX_EAPC_PROVIDER_ENABLED` ON `HCL.CS.SF_ExternalAuthProviderConfig` (`ProviderName`, `IsEnabled`);"
            },
            DatabaseProviderType.PostgreSql => new[]
            {
                """
                CREATE TABLE IF NOT EXISTS "HCL.CS.SF_ExternalAuthProviderConfig" (
                    "Id" uuid NOT NULL PRIMARY KEY,
                    "IsDeleted" boolean NOT NULL DEFAULT FALSE,
                    "CreatedOn" timestamp without time zone NOT NULL,
                    "ModifiedOn" timestamp without time zone NULL,
                    "CreatedBy" varchar(255) NOT NULL,
                    "ModifiedBy" varchar(255) NULL,
                    "ProviderName" varchar(50) NOT NULL,
                    "ProviderType" integer NOT NULL,
                    "IsEnabled" boolean NOT NULL,
                    "ConfigJson" text NOT NULL,
                    "AutoProvisionEnabled" boolean NOT NULL DEFAULT FALSE,
                    "AllowedDomains" varchar(2000) NULL,
                    "LastTestedOn" timestamp without time zone NULL,
                    "LastTestSuccess" boolean NULL
                );
                """,
                """
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_EAPC_PROVIDER"
                    ON "HCL.CS.SF_ExternalAuthProviderConfig" ("ProviderName");
                """,
                """
                CREATE INDEX IF NOT EXISTS "IX_EAPC_PROVIDER_ENABLED"
                    ON "HCL.CS.SF_ExternalAuthProviderConfig" ("ProviderName", "IsEnabled");
                """
            },
            DatabaseProviderType.Sqlite => new[]
            {
                """
                CREATE TABLE IF NOT EXISTS "HCL.CS.SF_ExternalAuthProviderConfig" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_HCL.CS.SF_ExternalAuthProviderConfig" PRIMARY KEY,
                    "IsDeleted" INTEGER NOT NULL DEFAULT 0,
                    "CreatedOn" TEXT NOT NULL,
                    "ModifiedOn" TEXT NULL,
                    "CreatedBy" TEXT NOT NULL,
                    "ModifiedBy" TEXT NULL,
                    "ProviderName" TEXT NOT NULL,
                    "ProviderType" INTEGER NOT NULL,
                    "IsEnabled" INTEGER NOT NULL,
                    "ConfigJson" TEXT NOT NULL,
                    "AutoProvisionEnabled" INTEGER NOT NULL DEFAULT 0,
                    "AllowedDomains" TEXT NULL,
                    "LastTestedOn" TEXT NULL,
                    "LastTestSuccess" INTEGER NULL
                );
                """,
                """
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_EAPC_PROVIDER"
                    ON "HCL.CS.SF_ExternalAuthProviderConfig" ("ProviderName");
                """,
                """
                CREATE INDEX IF NOT EXISTS "IX_EAPC_PROVIDER_ENABLED"
                    ON "HCL.CS.SF_ExternalAuthProviderConfig" ("ProviderName", "IsEnabled");
                """
            },
            _ => Array.Empty<string>()
        };

        foreach (var statement in statements)
            try
            {
                await migrationContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
            }
            catch (Exception ex) when (provider == DatabaseProviderType.MySql
                                       && ex.Message.Contains("Duplicate key name", StringComparison.OrdinalIgnoreCase))
            {
                // MySQL CREATE INDEX does not support IF NOT EXISTS on older engines.
            }
    }
}
