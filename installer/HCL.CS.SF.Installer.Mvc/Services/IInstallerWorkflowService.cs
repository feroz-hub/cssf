/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCLCSSFInstallerMVC.ViewModels;

namespace HCLCSSFInstallerMVC.Services;

/// <summary>
/// Bridges the MVC controller and the domain-level installer service by mapping between
/// view models and DTOs. Each method corresponds to a wizard step action.
/// </summary>
public interface IInstallerWorkflowService
{
    /// <summary>Checks whether installation has been completed.</summary>
    Task<bool> IsInstallationCompletedAsync(CancellationToken cancellationToken);

    /// <summary>Step 1 GET: Returns the current provider selection for display.</summary>
    Task<SetupProviderViewModel> GetProviderSelectionAsync(CancellationToken cancellationToken);

    /// <summary>Step 1 POST: Persists the selected database provider.</summary>
    Task SaveProviderSelectionAsync(SetupProviderViewModel model, CancellationToken cancellationToken);

    /// <summary>Step 2 GET: Returns the connection string configuration for display.</summary>
    Task<SetupConnectionViewModel> GetConnectionConfigurationAsync(CancellationToken cancellationToken);

    /// <summary>Step 2 POST: Persists the connection string.</summary>
    Task SaveConnectionConfigurationAsync(SetupConnectionViewModel model, CancellationToken cancellationToken);

    /// <summary>Step 3 GET: Returns the connection validation status for display.</summary>
    Task<ConnectionValidationViewModel> GetConnectionValidationAsync(CancellationToken cancellationToken);

    /// <summary>Step 3 POST: Executes the database connection test.</summary>
    Task<ConnectionValidationViewModel> ValidateConnectionAsync(CancellationToken cancellationToken);

    /// <summary>Step 4 GET: Returns the migration execution status for display.</summary>
    Task<MigrationViewModel> GetMigrationViewModelAsync(CancellationToken cancellationToken);

    /// <summary>Step 4 POST: Runs EF Core migrations against the configured database.</summary>
    Task<MigrationViewModel> RunMigrationAsync(CancellationToken cancellationToken);

    /// <summary>Step 5 GET: Returns the seed form defaults for display.</summary>
    Task<SeedStepViewModel> GetSeedViewModelAsync(CancellationToken cancellationToken);

    /// <summary>Step 5 POST: Executes database seeding with the supplied admin and client data.</summary>
    Task<SeedStepViewModel> ExecuteSeedAsync(SeedStepViewModel model, CancellationToken cancellationToken);

    /// <summary>Step 5 alternative: Marks installation complete without seeding data.</summary>
    Task SkipSeedAsync(CancellationToken cancellationToken);

    /// <summary>Step 6 GET: Returns the completion summary with generated credentials.</summary>
    Task<FinishViewModel> GetCompletionViewModelAsync(CancellationToken cancellationToken);
}
