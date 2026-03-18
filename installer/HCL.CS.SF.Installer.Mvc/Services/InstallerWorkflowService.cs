/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Enums;
using HCLCSSFInstallerMVC.Application.Abstractions;
using HCLCSSFInstallerMVC.Application.DTOs;
using HCLCSSFInstallerMVC.ViewModels;

namespace HCLCSSFInstallerMVC.Services;

/// <summary>
/// Maps between MVC view models and domain DTOs, providing the presentation-layer logic
/// for each installer wizard step. Detects container environments to suggest appropriate
/// connection strings.
/// </summary>
public sealed class InstallerWorkflowService : IInstallerWorkflowService
{
    private readonly IInstallerService _installerService;

    /// <summary>Detected once at startup: whether the app is running inside a Docker container.</summary>
    private static readonly bool IsRunningInContainer = string.Equals(
        Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
        "true",
        StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes the workflow service with the domain installer service.
    /// </summary>
    public InstallerWorkflowService(IInstallerService installerService)
    {
        _installerService = installerService;
    }

    /// <inheritdoc />
    public Task<bool> IsInstallationCompletedAsync(CancellationToken cancellationToken)
    {
        return _installerService.IsInstallationCompletedAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<SetupProviderViewModel> GetProviderSelectionAsync(CancellationToken cancellationToken)
    {
        var state = await _installerService.GetStateAsync(cancellationToken);
        return new SetupProviderViewModel { Provider = state.DatabaseConfiguration?.Provider };
    }

    /// <inheritdoc />
    public async Task SaveProviderSelectionAsync(SetupProviderViewModel model, CancellationToken cancellationToken)
    {
        var state = await _installerService.GetStateAsync(cancellationToken);
        await _installerService.SaveDatabaseConfigurationAsync(
            new DatabaseConfigurationDto
            {
                Provider = model.Provider ?? DatabaseProviderType.SqlServer,
                ConnectionString = state.DatabaseConfiguration?.ConnectionString ?? string.Empty
            },
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<SetupConnectionViewModel> GetConnectionConfigurationAsync(CancellationToken cancellationToken)
    {
        var state = await _installerService.GetStateAsync(cancellationToken);
        var provider = state.DatabaseConfiguration?.Provider;
        var connectionString = state.DatabaseConfiguration?.ConnectionString ?? string.Empty;
        if (string.IsNullOrWhiteSpace(connectionString))
            connectionString = GetSuggestedConnectionString(provider, IsRunningInContainer);

        return new SetupConnectionViewModel
        {
            Provider = provider,
            ConnectionString = connectionString,
            IsRunningInContainer = IsRunningInContainer
        };
    }

    /// <inheritdoc />
    public Task SaveConnectionConfigurationAsync(SetupConnectionViewModel model, CancellationToken cancellationToken)
    {
        return _installerService.SaveDatabaseConfigurationAsync(
            new DatabaseConfigurationDto
            {
                Provider = model.Provider ?? DatabaseProviderType.SqlServer,
                ConnectionString = model.ConnectionString.Trim()
            },
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ConnectionValidationViewModel> GetConnectionValidationAsync(CancellationToken cancellationToken)
    {
        var state = await _installerService.GetStateAsync(cancellationToken);

        return new ConnectionValidationViewModel
        {
            HasConfiguration = state.DatabaseConfiguration is not null,
            Provider = state.DatabaseConfiguration?.Provider,
            ConnectionString = state.DatabaseConfiguration?.ConnectionString ?? string.Empty,
            IsRunningInContainer = IsRunningInContainer,
            IsValidated = state.DatabaseConnectionValidated,
            IsSuccessful = state.DatabaseConnectionValidated
        };
    }

    /// <inheritdoc />
    public async Task<ConnectionValidationViewModel> ValidateConnectionAsync(CancellationToken cancellationToken)
    {
        var result = await _installerService.ValidateDatabaseConnectionAsync(cancellationToken);
        var viewModel = await GetConnectionValidationAsync(cancellationToken);

        viewModel.IsValidated = true;
        viewModel.IsSuccessful = result.Succeeded;
        viewModel.ErrorMessage = result.ErrorMessage;

        return viewModel;
    }

    /// <inheritdoc />
    public async Task<MigrationViewModel> GetMigrationViewModelAsync(CancellationToken cancellationToken)
    {
        var state = await _installerService.GetStateAsync(cancellationToken);

        return new MigrationViewModel
        {
            CanRun = state.DatabaseConnectionValidated,
            IsCompleted = state.MigrationCompleted
        };
    }

    /// <inheritdoc />
    public async Task<MigrationViewModel> RunMigrationAsync(CancellationToken cancellationToken)
    {
        var result = await _installerService.RunMigrationsAsync(cancellationToken);
        var model = await GetMigrationViewModelAsync(cancellationToken);
        model.ErrorMessage = result.ErrorMessage;
        model.IsCompleted = result.Succeeded;

        return model;
    }

    /// <inheritdoc />
    public async Task<SeedStepViewModel> GetSeedViewModelAsync(CancellationToken cancellationToken)
    {
        var state = await _installerService.GetStateAsync(cancellationToken);

        return new SeedStepViewModel
        {
            UseAuthorizationCodeGrant = true,
            UseCodeResponseType = true,
            UseDefaultScopes = true,
            IsCompleted = state.SeedResult?.Succeeded == true,
            GeneratedClientId = state.SeedResult?.GeneratedClientId,
            GeneratedClientSecret = state.SeedResult?.GeneratedClientSecret,
            ErrorMessage = state.SeedResult?.ErrorMessage
        };
    }

    /// <inheritdoc />
    public async Task<SeedStepViewModel> ExecuteSeedAsync(SeedStepViewModel model, CancellationToken cancellationToken)
    {
        var result = await _installerService.SeedInitialDataAsync(MapSeedConfiguration(model), cancellationToken);

        model.IsCompleted = result.Succeeded;
        model.GeneratedClientId = result.GeneratedClientId;
        model.GeneratedClientSecret = result.GeneratedClientSecret;
        model.ErrorMessage = result.ErrorMessage;

        return model;
    }

    /// <inheritdoc />
    public Task SkipSeedAsync(CancellationToken cancellationToken)
    {
        return _installerService.MarkInstallationCompletedWithoutSeedAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<FinishViewModel> GetCompletionViewModelAsync(CancellationToken cancellationToken)
    {
        var isCompleted = await _installerService.IsInstallationCompletedAsync(cancellationToken);
        var metadata = await _installerService.GetInstallationCompletionMetadataAsync(cancellationToken);

        return new FinishViewModel
        {
            InstallationCompleted = isCompleted,
            AlreadyInstalled = isCompleted,
            Message = isCompleted
                ? "HCL.CS.SF installation completed and locked against reinstallation."
                : "Installation is not completed yet.",
            DatabaseProvider = metadata?.DatabaseProvider,
            ClientId = metadata?.ClientId,
            ClientSecret = metadata?.ClientSecret,
            CompletedOnUtc = metadata?.CompletedOnUtc
        };
    }

    /// <summary>
    /// Maps the seed step view model into the domain SeedConfigurationDto,
    /// building grant types, response types, and splitting multi-line URI inputs.
    /// </summary>
    private static SeedConfigurationDto MapSeedConfiguration(SeedStepViewModel model)
    {
        return new SeedConfigurationDto
        {
            Client = new ClientConfigurationDto
            {
                ClientName = model.ClientName.Trim(),
                ClientUri = model.ClientUri.Trim(),
                GrantTypes = BuildGrantTypes(model),
                ResponseTypes = BuildResponseTypes(model),
                UseDefaultScopes = model.UseDefaultScopes,
                AllowedScopes = model.AllowedScopes.Trim(),
                RedirectUris = SplitLines(model.RedirectUris),
                PostLogoutRedirectUris = SplitLines(model.PostLogoutRedirectUris),
                FrontChannelLogoutUri = model.FrontChannelLogoutUri,
                BackChannelLogoutUri = model.BackChannelLogoutUri
            },
            AdminUser = new AdminUserConfigurationDto
            {
                UserName = model.UserName.Trim(),
                Password = model.Password,
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName,
                Email = model.Email.Trim(),
                PhoneNumber = model.PhoneNumber.Trim(),
                IdentityProvider = model.IdentityProvider.Equals("Ldap", StringComparison.OrdinalIgnoreCase)
                    ? IdentityProvider.Ldap
                    : IdentityProvider.Local
            }
        };
    }

    /// <summary>Builds the list of OAuth grant type strings from the view model checkboxes.</summary>
    private static IReadOnlyCollection<string> BuildGrantTypes(SeedStepViewModel model)
    {
        var grantTypes = new List<string>();

        if (model.UseAuthorizationCodeGrant) grantTypes.Add("authorization_code");

        if (model.UseClientCredentialsGrant) grantTypes.Add("client_credentials");

        if (model.UseRefreshTokenGrant) grantTypes.Add("refresh_token");

        if (model.UsePasswordGrant) grantTypes.Add("password");

        return grantTypes;
    }

    /// <summary>Builds the list of OAuth response type strings from the view model checkboxes.</summary>
    private static IReadOnlyCollection<string> BuildResponseTypes(SeedStepViewModel model)
    {
        var responseTypes = new List<string>();

        if (model.UseCodeResponseType) responseTypes.Add("code");

        return responseTypes;
    }

    /// <summary>Splits a multi-line string into distinct, trimmed, non-empty values.</summary>
    private static IReadOnlyCollection<string> SplitLines(string values)
    {
        if (string.IsNullOrWhiteSpace(values)) return Array.Empty<string>();

        return values
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>
    /// Returns a suggested connection string template for the selected provider.
    /// Uses container-aware hostnames when running inside Docker.
    /// </summary>
    private static string GetSuggestedConnectionString(DatabaseProviderType? provider, bool isRunningInContainer)
    {
        return provider switch
        {
            DatabaseProviderType.SqlServer => "Server=localhost;Database=HCL.CS.SF_identity;User Id=sa;Password=<strong-password>;Encrypt=True;TrustServerCertificate=False;",
            DatabaseProviderType.MySql => "Server=localhost;Port=3306;Database=HCL.CS.SF_identity;Uid=root;Pwd=<strong-password>;SslMode=Preferred;",
            DatabaseProviderType.PostgreSql when isRunningInContainer => "Host=postgres;Port=5432;Database=HCL.CS.SF;Username=HCL.CS.SF;Password=HCL.CS.SF;",
            DatabaseProviderType.PostgreSql => "Host=localhost;Port=55433;Database=HCL.CS.SF;Username=HCL.CS.SF;Password=HCL.CS.SF;",
            DatabaseProviderType.Sqlite => "Data Source=.data/HCL.CS.SF_identity.db;Mode=ReadWriteCreate;Cache=Shared;",
            _ => string.Empty
        };
    }
}
