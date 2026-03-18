/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.Service.Implementation.Api.Services;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api.Wrapper;

namespace HCL.CS.SF.ProxyService.Proxy;

/// <summary>
/// Gateway proxy for the notification management service. Routes requests to the backend
/// <see cref="NotificationManagementService"/> after enforcing API-level permission validation.
/// Manages notification provider configurations, notification templates, logs, and test sends.
/// For test notifications, extracts the current user's ID from the JWT to satisfy
/// the foreign key constraint on the notification log.
/// </summary>
public sealed class NotificationManagementProxyService : NotificationManagementService, INotificationManagementService
{
    /// <summary>
    /// Validator that checks whether the caller has permission to invoke the requested operation.
    /// </summary>
    private readonly IApiValidator apiValidator;

    /// <summary>
    /// Service used to construct failure responses when validation fails.
    /// </summary>
    private readonly IFrameworkResultService frameworkResult;

    /// <summary>
    /// Provides access to the current HTTP context for extracting JWT claims.
    /// </summary>
    private readonly IHttpContextAccessor httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationManagementProxyService"/> class.
    /// </summary>
    public NotificationManagementProxyService(
        IRepository<Notification> notificationRepository,
        IRepository<NotificationProviderConfig> providerConfigRepository,
        HCLCSSFConfig HCLCSSFConfig,
        IFrameworkResultService frameworkResult,
        ILoggerInstance loggerInstance,
        IEnumerable<IEmailProvider> emailProviders,
        IEnumerable<ISmsProvider> smsProviders,
        IApiValidator apiValidator,
        IHttpContextAccessor httpContextAccessor)
        : base(
            notificationRepository,
            providerConfigRepository,
            HCLCSSFConfig,
            frameworkResult,
            loggerInstance,
            emailProviders,
            smsProviders)
    {
        this.apiValidator = apiValidator;
        this.frameworkResult = frameworkResult;
        this.httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Retrieves notification logs using the provided search criteria after permission validation.
    /// </summary>
    /// <param name="request">The search criteria for filtering notification logs.</param>
    /// <returns>A paginated response containing matching notification log entries.</returns>
    public override async Task<NotificationLogResponseModel> GetNotificationLogsAsync(NotificationSearchRequestModel request)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetNotificationLogsAsync(request);
    }

    /// <summary>
    /// Retrieves all notification templates after permission validation.
    /// </summary>
    /// <returns>The response containing all configured notification templates.</returns>
    public override async Task<NotificationTemplateResponseModel> GetNotificationTemplatesAsync()
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetNotificationTemplatesAsync();
    }

    /// <summary>
    /// Retrieves a specific notification provider configuration by ID after permission validation.
    /// </summary>
    /// <param name="id">The unique identifier of the provider configuration.</param>
    /// <returns>The provider configuration model.</returns>
    public override async Task<ProviderConfigModel> GetProviderConfigAsync(Guid id)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetProviderConfigAsync(id);
    }

    /// <summary>
    /// Retrieves all notification provider configurations after permission validation.
    /// </summary>
    /// <returns>A list of all provider configuration models.</returns>
    public override async Task<List<ProviderConfigModel>> GetAllProviderConfigsAsync()
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetAllProviderConfigsAsync();
    }

    /// <summary>
    /// Creates or updates a notification provider configuration after permission validation.
    /// </summary>
    /// <param name="request">The provider configuration to save.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> SaveProviderConfigAsync(SaveProviderConfigRequest request)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.SaveProviderConfigAsync(request);
    }

    /// <summary>
    /// Sets the active notification provider after permission validation.
    /// </summary>
    /// <param name="request">The request identifying which provider to activate.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> SetActiveProviderAsync(SetActiveProviderRequest request)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.SetActiveProviderAsync(request);
    }

    /// <summary>
    /// Deletes a notification provider configuration after permission validation.
    /// </summary>
    /// <param name="request">The request identifying the provider to delete.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> DeleteProviderConfigAsync(DeleteProviderConfigRequest request)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.DeleteProviderConfigAsync(request);
    }

    /// <summary>
    /// Retrieves the field definitions schema for notification provider configuration
    /// forms after permission validation.
    /// </summary>
    /// <returns>The field definitions response.</returns>
    public override async Task<ProviderFieldDefinitionsResponse> GetProviderFieldDefinitionsAsync()
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetProviderFieldDefinitionsAsync();
    }

    /// <summary>
    /// Sends a test notification after permission validation.
    /// Extracts the current user's ID from the JWT bearer token to satisfy
    /// the foreign key constraint on the notification log record.
    /// </summary>
    /// <param name="request">The test notification request containing recipient and message details.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating whether the notification was sent.</returns>
    public override async Task<FrameworkResult> SendTestNotificationAsync(SendTestNotificationRequest request)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        // Extract current user ID from JWT for notification logging (FK constraint)
        var authorization = httpContextAccessor.HttpContext?.Request?.Headers[HeaderNames.Authorization].ToString();
        if (!string.IsNullOrWhiteSpace(authorization) &&
            AuthenticationHeaderValue.TryParse(authorization, out var headerValue))
        {
            var token = new JwtSecurityToken(headerValue.Parameter);
            var subClaim = token.Claims.FirstOrDefault(c => c.Type == OpenIdConstants.ClaimTypes.Sub);
            if (subClaim != null && Guid.TryParse(subClaim.Value, out var userId))
                request.UserId = userId;
        }

        return await base.SendTestNotificationAsync(request);
    }
}
