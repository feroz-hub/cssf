/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.ProxyService.Routes.Extension;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api.Wrapper;

namespace HCL.CS.SF.ProxyService.Routes;

/// <summary>
/// Partial class of <see cref="ApiGateway"/> containing route handlers for notification
/// management operations. Handles routes for querying notification logs, templates,
/// provider configurations, and sending test notifications.
/// </summary>
internal partial class ApiGateway : BaseApiServiceInstance, IApiGateway
{
    /// <summary>
    /// Handles the route for retrieving notification logs with search/filter criteria.
    /// </summary>
    private async Task<bool> GetNotificationLogs(string jsonContent)
    {
        var request = jsonContent.JsonDeserialize<NotificationSearchRequestModel>();
        var result = await NotificationManagementService.GetNotificationLogsAsync(request);
        await GenerateApiResults(result);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving all notification templates.
    /// </summary>
    private async Task<bool> GetNotificationTemplates(string jsonContent)
    {
        var result = await NotificationManagementService.GetNotificationTemplatesAsync();
        await GenerateApiResults(result);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving a specific notification provider configuration by ID.
    /// </summary>
    private async Task<bool> GetProviderConfig(string jsonContent)
    {
        var request = jsonContent.JsonDeserialize<SetActiveProviderRequest>();
        var result = await NotificationManagementService.GetProviderConfigAsync(request.Id);
        await GenerateApiResults(result);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving all notification provider configurations.
    /// </summary>
    private async Task<bool> GetAllProviderConfigs(string jsonContent)
    {
        var result = await NotificationManagementService.GetAllProviderConfigsAsync();
        await GenerateApiResults(result);
        return true;
    }

    /// <summary>
    /// Handles the route for creating or updating a notification provider configuration.
    /// </summary>
    private async Task<bool> SaveProviderConfig(string jsonContent)
    {
        var request = jsonContent.JsonDeserialize<SaveProviderConfigRequest>();
        var frameworkResult = await NotificationManagementService.SaveProviderConfigAsync(request);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for setting the active notification provider.
    /// </summary>
    private async Task<bool> SetActiveProvider(string jsonContent)
    {
        var request = jsonContent.JsonDeserialize<SetActiveProviderRequest>();
        var frameworkResult = await NotificationManagementService.SetActiveProviderAsync(request);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for deleting a notification provider configuration.
    /// </summary>
    private async Task<bool> DeleteProviderConfig(string jsonContent)
    {
        var request = jsonContent.JsonDeserialize<DeleteProviderConfigRequest>();
        var frameworkResult = await NotificationManagementService.DeleteProviderConfigAsync(request);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for retrieving notification provider field definitions schema.
    /// </summary>
    private async Task<bool> GetProviderFieldDefinitions(string jsonContent)
    {
        var result = await NotificationManagementService.GetProviderFieldDefinitionsAsync();
        await GenerateApiResults(result);
        return true;
    }

    /// <summary>
    /// Handles the route for sending a test notification through the active provider.
    /// </summary>
    private async Task<bool> SendTestNotification(string jsonContent)
    {
        var request = jsonContent.JsonDeserialize<SendTestNotificationRequest>();
        var frameworkResult = await NotificationManagementService.SendTestNotificationAsync(request);
        return await GenerateApiResults(frameworkResult);
    }
}
