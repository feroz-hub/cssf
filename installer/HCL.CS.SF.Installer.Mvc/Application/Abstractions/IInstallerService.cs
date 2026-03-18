/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCLCSSFInstallerMVC.Application.DTOs;

namespace HCLCSSFInstallerMVC.Application.Abstractions;

/// <summary>
/// Orchestrates the multi-step installer workflow: database configuration, connection validation,
/// migration execution, seed data generation, and installation completion.
/// </summary>
public interface IInstallerService
{
    /// <summary>
    /// Checks whether the installation has already been completed and locked.
    /// </summary>
    Task<bool> IsInstallationCompletedAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the current installer session state including database configuration and step progress.
    /// </summary>
    Task<InstallerSessionState> GetStateAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Step 1/2: Persists the database provider and connection string, resetting downstream step flags.
    /// </summary>
    Task SaveDatabaseConfigurationAsync(DatabaseConfigurationDto configuration, CancellationToken cancellationToken);

    /// <summary>
    /// Step 3: Tests connectivity to the configured database and updates session state accordingly.
    /// </summary>
    Task<ConnectionValidationResultDto> ValidateDatabaseConnectionAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Step 4: Applies EF Core migrations and post-migration schema patches against the configured database.
    /// </summary>
    Task<MigrationExecutionResultDto> RunMigrationsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Step 5: Seeds master data, the initial admin user, and the first OAuth client into the database.
    /// On success the installation is marked as completed.
    /// </summary>
    Task<SeedExecutionResultDto> SeedInitialDataAsync(SeedConfigurationDto configuration,
        CancellationToken cancellationToken);

    /// <summary>
    /// Alternative to Step 5: Marks the installation as completed without seeding any data.
    /// </summary>
    Task MarkInstallationCompletedWithoutSeedAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the metadata written at installation completion (timestamp, provider, client credentials).
    /// </summary>
    Task<InstallationCompletionMetadataDto?>
        GetInstallationCompletionMetadataAsync(CancellationToken cancellationToken);
}
