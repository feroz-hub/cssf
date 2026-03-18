/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Models.Api;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Api;

/// <summary>
/// Administrative service for managing notification providers and templates.
/// Supports CRUD operations on provider configurations (email/SMS), template retrieval,
/// active provider selection, and test notification sending.
/// </summary>
public interface INotificationManagementService
{
    /// <summary>Retrieves notification log entries matching the search criteria with pagination.</summary>
    /// <param name="request">The search/filter criteria.</param>
    Task<NotificationLogResponseModel> GetNotificationLogsAsync(NotificationSearchRequestModel request);

    /// <summary>Retrieves all configured notification templates.</summary>
    Task<NotificationTemplateResponseModel> GetNotificationTemplatesAsync();

    /// <summary>Retrieves a specific provider configuration by its identifier.</summary>
    /// <param name="id">The unique identifier of the provider configuration.</param>
    Task<ProviderConfigModel> GetProviderConfigAsync(Guid id);

    /// <summary>Retrieves all registered provider configurations.</summary>
    Task<List<ProviderConfigModel>> GetAllProviderConfigsAsync();

    /// <summary>Creates or updates a notification provider configuration.</summary>
    /// <param name="request">The provider configuration to save.</param>
    Task<FrameworkResult> SaveProviderConfigAsync(SaveProviderConfigRequest request);

    /// <summary>Sets the active notification provider for the specified notification type.</summary>
    /// <param name="request">The request specifying which provider to activate.</param>
    Task<FrameworkResult> SetActiveProviderAsync(SetActiveProviderRequest request);

    /// <summary>Deletes a notification provider configuration.</summary>
    /// <param name="request">The request identifying the provider to delete.</param>
    Task<FrameworkResult> DeleteProviderConfigAsync(DeleteProviderConfigRequest request);

    /// <summary>Retrieves the field definitions required to configure each supported provider type.</summary>
    Task<ProviderFieldDefinitionsResponse> GetProviderFieldDefinitionsAsync();

    /// <summary>Sends a test notification using the specified provider configuration.</summary>
    /// <param name="request">The test notification request including recipient and provider details.</param>
    Task<FrameworkResult> SendTestNotificationAsync(SendTestNotificationRequest request);
}
