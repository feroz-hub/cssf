/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCLCSSFInstallerMVC.Application.Abstractions;
using HCLCSSFInstallerMVC.Application.DTOs;
using HCLCSSFInstallerMVC.Application.Exceptions;

namespace HCLCSSFInstallerMVC.Application.Services;

/// <summary>
/// Core installer service that coordinates the multi-step setup workflow.
/// Each step enforces prerequisite checks and guards against reinstallation.
/// </summary>
public sealed class InstallerService : IInstallerService
{
    private readonly IDatabaseMigrationService _databaseMigrationService;
    private readonly IInstallationGateService _installationGateService;
    private readonly ISeedDataService _seedDataService;
    private readonly IInstallerStateStore _stateStore;

    /// <summary>
    /// Initializes the service with the required infrastructure dependencies.
    /// </summary>
    public InstallerService(
        IDatabaseMigrationService databaseMigrationService,
        ISeedDataService seedDataService,
        IInstallerStateStore stateStore,
        IInstallationGateService installationGateService)
    {
        _databaseMigrationService = databaseMigrationService;
        _seedDataService = seedDataService;
        _stateStore = stateStore;
        _installationGateService = installationGateService;
    }

    /// <inheritdoc />
    public Task<bool> IsInstallationCompletedAsync(CancellationToken cancellationToken)
    {
        return _installationGateService.IsInstallationCompletedAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<InstallerSessionState> GetStateAsync(CancellationToken cancellationToken)
    {
        return _stateStore.GetAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveDatabaseConfigurationAsync(DatabaseConfigurationDto configuration,
        CancellationToken cancellationToken)
    {
        await EnsureInstallationIsAllowedAsync(cancellationToken);

        var state = await _stateStore.GetAsync(cancellationToken);
        // Reset all downstream step flags when configuration changes
        state.DatabaseConfiguration = configuration;
        state.DatabaseConnectionValidated = false;
        state.MigrationCompleted = false;
        state.SeedResult = null;

        await _stateStore.SaveAsync(state, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ConnectionValidationResultDto> ValidateDatabaseConnectionAsync(
        CancellationToken cancellationToken)
    {
        await EnsureInstallationIsAllowedAsync(cancellationToken);

        var state = await _stateStore.GetAsync(cancellationToken);
        if (state.DatabaseConfiguration is null)
            throw new InstallerWorkflowException("Database configuration is required before validating connection.");

        var result =
            await _databaseMigrationService.ValidateConnectionAsync(state.DatabaseConfiguration, cancellationToken);
        state.DatabaseConnectionValidated = result.Succeeded;

        // On failure, invalidate downstream steps so the user must re-run them
        if (!result.Succeeded)
        {
            state.MigrationCompleted = false;
            state.SeedResult = null;
        }

        await _stateStore.SaveAsync(state, cancellationToken);
        return result;
    }

    /// <inheritdoc />
    public async Task<MigrationExecutionResultDto> RunMigrationsAsync(CancellationToken cancellationToken)
    {
        await EnsureInstallationIsAllowedAsync(cancellationToken);

        var state = await _stateStore.GetAsync(cancellationToken);
        if (state.DatabaseConfiguration is null)
            throw new InstallerWorkflowException("Database configuration is required before running migrations.");

        if (!state.DatabaseConnectionValidated)
            throw new InstallerWorkflowException("Database connection must be validated before running migrations.");

        var result = await _databaseMigrationService.RunMigrationsAsync(state.DatabaseConfiguration, cancellationToken);
        state.MigrationCompleted = result.Succeeded;

        if (!result.Succeeded) state.SeedResult = null;

        await _stateStore.SaveAsync(state, cancellationToken);
        return result;
    }

    /// <inheritdoc />
    public async Task<SeedExecutionResultDto> SeedInitialDataAsync(SeedConfigurationDto configuration,
        CancellationToken cancellationToken)
    {
        await EnsureInstallationIsAllowedAsync(cancellationToken);

        var state = await _stateStore.GetAsync(cancellationToken);
        if (state.DatabaseConfiguration is null)
            throw new InstallerWorkflowException("Database configuration is required before seeding data.");

        if (!state.MigrationCompleted)
            throw new InstallerWorkflowException("Migrations must complete before seeding data.");

        var result = await _seedDataService.SeedAsync(state.DatabaseConfiguration, configuration, cancellationToken);
        state.SeedResult = result;

        if (result.Succeeded)
            await _installationGateService.MarkInstallationCompletedAsync(
                new InstallationCompletionMetadataDto
                {
                    CompletedOnUtc = DateTimeOffset.UtcNow,
                    DatabaseProvider = state.DatabaseConfiguration.Provider.ToString(),
                    ClientId = result.GeneratedClientId,
                    ClientSecret = result.GeneratedClientSecret
                },
                cancellationToken);

        await _stateStore.SaveAsync(state, cancellationToken);
        return result;
    }

    /// <inheritdoc />
    public async Task MarkInstallationCompletedWithoutSeedAsync(CancellationToken cancellationToken)
    {
        await EnsureInstallationIsAllowedAsync(cancellationToken);

        var state = await _stateStore.GetAsync(cancellationToken);
        if (state.DatabaseConfiguration is null)
            throw new InstallerWorkflowException("Database configuration is required.");
        if (!state.MigrationCompleted)
            throw new InstallerWorkflowException("Migrations must complete before finishing without seed.");

        await _installationGateService.MarkInstallationCompletedAsync(
            new InstallationCompletionMetadataDto
            {
                CompletedOnUtc = DateTimeOffset.UtcNow,
                DatabaseProvider = state.DatabaseConfiguration.Provider.ToString(),
                ClientId = null,
                ClientSecret = null
            },
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<InstallationCompletionMetadataDto?> GetInstallationCompletionMetadataAsync(
        CancellationToken cancellationToken)
    {
        return _installationGateService.GetCompletionMetadataAsync(cancellationToken);
    }

    /// <summary>
    /// Guard that throws <see cref="InstallationAlreadyCompletedException"/> if the lock marker exists.
    /// </summary>
    private async Task EnsureInstallationIsAllowedAsync(CancellationToken cancellationToken)
    {
        if (await _installationGateService.IsInstallationCompletedAsync(cancellationToken))
            throw new InstallationAlreadyCompletedException();
    }
}
