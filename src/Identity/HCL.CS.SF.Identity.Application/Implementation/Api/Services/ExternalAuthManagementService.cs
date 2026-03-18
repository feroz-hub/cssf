/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Text.Json;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;

namespace HCL.CS.SF.Service.Implementation.Api.Services;

/// <summary>
/// Service for managing external authentication provider configurations (e.g., Google OIDC, SAML providers).
/// Supports CRUD operations on provider configurations, connectivity testing against provider metadata
/// endpoints, and field definition retrieval for dynamic admin UI rendering. Provider secrets
/// are masked in responses to prevent credential exposure.
/// </summary>

public class ExternalAuthManagementService : SecurityBase, IExternalAuthManagementService
{
    private readonly IRepository<ExternalAuthProviderConfig> _providerConfigRepository;
    private readonly IFrameworkResultService _frameworkResult;
    private readonly ILoggerService _loggerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalAuthManagementService"/> class.
    /// </summary>
    /// <param name="providerConfigRepository">Repository for external auth provider configuration persistence.</param>
    /// <param name="frameworkResult">Service for constructing standardized operation results.</param>
    /// <param name="loggerInstance">Logger factory for obtaining the default logger.</param>
    public ExternalAuthManagementService(
        IRepository<ExternalAuthProviderConfig> providerConfigRepository,
        IFrameworkResultService frameworkResult,
        ILoggerInstance loggerInstance)
    {
        _providerConfigRepository = providerConfigRepository;
        _frameworkResult = frameworkResult;
        _loggerService = loggerInstance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
    }

    /// <summary>
    /// Retrieves all configured external authentication providers with secrets masked.
    /// </summary>
    /// <returns>A list of provider configuration models with sensitive fields partially redacted.</returns>

    public virtual async Task<List<ExternalAuthProviderConfigModel>> GetAllProvidersAsync()
    {
        try
        {
            var configs = await _providerConfigRepository.GetAllAsync();
            return configs.Select(c => MapToModel(c, maskSecrets: true)).ToList();
        }
        catch (Exception ex)
        {
            _loggerService.WriteToWithCaller(Log.Error, ex, "Failed to retrieve external auth providers.");
            throw;
        }
    }

    /// <summary>
    /// Retrieves a specific external authentication provider configuration by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the provider configuration.</param>
    /// <returns>The provider configuration with secrets masked.</returns>

    public virtual async Task<ExternalAuthProviderConfigModel> GetProviderAsync(Guid id)
    {
        try
        {
            var config = await _providerConfigRepository.GetAsync(id);
            if (config == null)
                _frameworkResult.Throw(ApiErrorCodes.InvalidOrNullObject);

            return MapToModel(config, maskSecrets: true);
        }
        catch (Exception ex)
        {
            _loggerService.WriteToWithCaller(Log.Error, ex, "Failed to retrieve external auth provider.");
            throw;
        }
    }

    /// <summary>
    /// Creates or updates an external authentication provider configuration.
    /// Serializes provider settings as JSON for flexible schema-free storage.
    /// </summary>
    /// <param name="request">The save request containing provider name, type, settings, and flags.</param>
    /// <returns>The result of the save operation.</returns>

    public virtual async Task<FrameworkResult> SaveProviderAsync(SaveExternalAuthProviderRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.ProviderName))
                return _frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.InvalidOrNullObject);

            if (request.Settings == null || request.Settings.Count == 0)
                return _frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.InvalidOrNullObject);

            var configJson = JsonSerializer.Serialize(request.Settings);

            if (request.Id.HasValue && request.Id.Value != Guid.Empty)
            {
                var existing = await _providerConfigRepository.GetAsync(request.Id.Value);
                if (existing == null)
                    return _frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.InvalidOrNullObject);

                existing.ProviderName = request.ProviderName;
                existing.ProviderType = request.ProviderType;
                existing.IsEnabled = request.IsEnabled;
                existing.ConfigJson = configJson;
                existing.AutoProvisionEnabled = request.AutoProvisionEnabled;
                existing.AllowedDomains = request.AllowedDomains;
                await _providerConfigRepository.UpdateAsync(existing);
            }
            else
            {
                var newConfig = new ExternalAuthProviderConfig
                {
                    Id = Guid.NewGuid(),
                    ProviderName = request.ProviderName,
                    ProviderType = request.ProviderType,
                    IsEnabled = request.IsEnabled,
                    ConfigJson = configJson,
                    AutoProvisionEnabled = request.AutoProvisionEnabled,
                    AllowedDomains = request.AllowedDomains,
                    CreatedBy = "Admin"
                };

                await _providerConfigRepository.InsertAsync(newConfig);
            }

            return await _providerConfigRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _loggerService.WriteToWithCaller(Log.Error, ex, "Failed to save external auth provider.");
            throw;
        }
    }

    /// <summary>
    /// Deletes an external authentication provider configuration. Disabled providers
    /// must be disabled before deletion to prevent accidental removal of active auth flows.
    /// </summary>
    /// <param name="request">The delete request containing the provider ID.</param>
    /// <returns>The result of the deletion operation.</returns>

    public virtual async Task<FrameworkResult> DeleteProviderAsync(DeleteExternalAuthProviderRequest request)
    {
        try
        {
            var config = await _providerConfigRepository.GetAsync(request.Id);
            if (config == null)
                return _frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.InvalidOrNullObject);

            if (config.IsEnabled)
                return _frameworkResult.ConstructFailed(ApiErrorCodes.InvalidOrNullObject,
                    "Cannot delete an enabled provider. Disable it first.");

            await _providerConfigRepository.DeleteAsync(config);
            return await _providerConfigRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _loggerService.WriteToWithCaller(Log.Error, ex, "Failed to delete external auth provider.");
            throw;
        }
    }

    /// <summary>
    /// Tests connectivity to an external authentication provider by fetching its OIDC metadata endpoint.
    /// Updates the provider's last-tested timestamp and success status.
    /// </summary>
    /// <param name="request">The test request containing the provider ID to test.</param>
    /// <returns>Success if the metadata endpoint is reachable; failure otherwise.</returns>

    public virtual async Task<FrameworkResult> TestProviderAsync(TestExternalAuthProviderRequest request)
    {
        try
        {
            var config = await _providerConfigRepository.GetAsync(request.Id);
            if (config == null)
                return _frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.InvalidOrNullObject);

            var settings = DeserializeSettings(config.ConfigJson);
            var metadataAddress = settings.GetValueOrDefault("MetadataAddress",
                "https://accounts.google.com/.well-known/openid-configuration");

            bool testSuccess;
            try
            {
                using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                var response = await httpClient.GetAsync(metadataAddress);
                testSuccess = response.IsSuccessStatusCode;
            }
            catch
            {
                testSuccess = false;
            }

            config.LastTestedOn = DateTime.UtcNow;
            config.LastTestSuccess = testSuccess;
            await _providerConfigRepository.UpdateAsync(config);
            await _providerConfigRepository.SaveChangesAsync();

            if (!testSuccess)
                return _frameworkResult.ConstructFailed(ApiErrorCodes.InvalidOrNullObject,
                    "Failed to reach provider metadata endpoint.");

            return _frameworkResult.Succeeded();
        }
        catch (Exception ex)
        {
            _loggerService.WriteToWithCaller(Log.Error, ex, "Failed to test external auth provider.");
            throw;
        }
    }

    /// <summary>
    /// Returns field definitions for supported external auth provider types, enabling
    /// the admin UI to dynamically render provider configuration forms.
    /// </summary>
    /// <returns>Field definitions and defaults for each supported provider type.</returns>

    public virtual Task<ExternalAuthFieldDefinitionsResponse> GetFieldDefinitionsAsync()
    {
        var response = new ExternalAuthFieldDefinitionsResponse
        {
            Providers = ExternalAuthProviderConstants.ProviderFields,
            Defaults = ExternalAuthProviderConstants.ProviderDefaults
        };

        return Task.FromResult(response);
    }

    /// <summary>
    /// Maps a provider config entity to a model, optionally masking password-type fields
    /// by replacing all but the first 4 characters with asterisks.
    /// </summary>

    private static ExternalAuthProviderConfigModel MapToModel(ExternalAuthProviderConfig config, bool maskSecrets)
    {
        var settings = DeserializeSettings(config.ConfigJson);

        if (maskSecrets && ExternalAuthProviderConstants.ProviderFields.TryGetValue(config.ProviderName, out var fields))
        {
            foreach (var field in fields)
            {
                if (field.InputType == "password" && settings.ContainsKey(field.Key))
                {
                    var value = settings[field.Key];
                    if (!string.IsNullOrEmpty(value) && value.Length > 4)
                        settings[field.Key] = value[..4] + "****";
                    else if (!string.IsNullOrEmpty(value))
                        settings[field.Key] = "****";
                }
            }
        }

        return new ExternalAuthProviderConfigModel
        {
            Id = config.Id,
            ProviderName = config.ProviderName,
            ProviderType = config.ProviderType,
            IsEnabled = config.IsEnabled,
            Settings = settings,
            AutoProvisionEnabled = config.AutoProvisionEnabled,
            AllowedDomains = config.AllowedDomains,
            LastTestedOn = config.LastTestedOn,
            LastTestSuccess = config.LastTestSuccess
        };
    }

    /// <summary>
    /// Safely deserializes a JSON config string to a dictionary, returning empty on failure.
    /// </summary>

    private static Dictionary<string, string> DeserializeSettings(string configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
            return new Dictionary<string, string>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(configJson)
                ?? new Dictionary<string, string>();
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }
}
