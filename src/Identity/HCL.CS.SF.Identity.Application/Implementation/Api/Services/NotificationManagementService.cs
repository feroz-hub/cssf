/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Text.Json;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Configurations.Api;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;

namespace HCL.CS.SF.Service.Implementation.Api.Services;

/// <summary>
/// Service for managing notification infrastructure including email/SMS provider configurations,
/// notification delivery logs, and notification templates. Supports multiple email providers
/// (SMTP, SendGrid, etc.) with runtime switching, provider connectivity testing, and
/// paginated delivery log querying for monitoring notification health.
/// </summary>

public class NotificationManagementService : SecurityBase, INotificationManagementService
{
    private readonly IRepository<Notification> _notificationRepository;
    private readonly IRepository<NotificationProviderConfig> _providerConfigRepository;
    private readonly HCLCSSFConfig _HCLCSSFConfig;
    private readonly IFrameworkResultService _frameworkResult;
    private readonly ILoggerService _loggerService;
    private readonly Dictionary<string, IEmailProvider> _emailProviders;
    private readonly Dictionary<string, ISmsProvider> _smsProviders;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationManagementService"/> class.
    /// </summary>
    /// <param name="notificationRepository">Repository for notification delivery log persistence.</param>
    /// <param name="providerConfigRepository">Repository for provider configuration persistence.</param>
    /// <param name="HCLCSSFConfig">Framework configuration containing notification template settings.</param>
    /// <param name="frameworkResult">Service for constructing standardized operation results.</param>
    /// <param name="loggerInstance">Logger factory for obtaining the default logger.</param>
    /// <param name="emailProviders">Registered email provider implementations keyed by provider name.</param>
    /// <param name="smsProviders">Registered SMS provider implementations keyed by provider name.</param>
    public NotificationManagementService(
        IRepository<Notification> notificationRepository,
        IRepository<NotificationProviderConfig> providerConfigRepository,
        HCLCSSFConfig HCLCSSFConfig,
        IFrameworkResultService frameworkResult,
        ILoggerInstance loggerInstance,
        IEnumerable<IEmailProvider> emailProviders,
        IEnumerable<ISmsProvider> smsProviders)
    {
        _notificationRepository = notificationRepository;
        _providerConfigRepository = providerConfigRepository;
        _HCLCSSFConfig = HCLCSSFConfig;
        _frameworkResult = frameworkResult;
        _loggerService = loggerInstance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
        _emailProviders = emailProviders.ToDictionary(p => p.ProviderName, StringComparer.OrdinalIgnoreCase);
        _smsProviders = smsProviders.ToDictionary(p => p.ProviderName, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Retrieves paginated notification delivery logs with optional filtering by type, status,
    /// date range, and free-text search across activity, recipient, sender, and message ID fields.
    /// </summary>
    /// <param name="request">The search/filter criteria and paging parameters.</param>
    /// <returns>A paged response containing matching notification log entries.</returns>
    public virtual async Task<NotificationLogResponseModel> GetNotificationLogsAsync(NotificationSearchRequestModel request)
    {
        try
        {
            if (request.Page == null)
                _frameworkResult.Throw(ApiErrorCodes.InvalidOrNullObject);

            var response = new NotificationLogResponseModel();

            var query = await _notificationRepository.GetAllForQueryAsync();

            if (request.Type.HasValue)
                query = query.Where(n => (int)n.Type == request.Type.Value);

            if (request.Status.HasValue)
                query = query.Where(n => (int)n.Status == request.Status.Value);

            if (!string.IsNullOrWhiteSpace(request.FromDate) && DateTime.TryParse(request.FromDate, out var fromDate))
                query = query.Where(n => n.CreatedOn >= fromDate);

            if (!string.IsNullOrWhiteSpace(request.ToDate) && DateTime.TryParse(request.ToDate, out var toDate))
                query = query.Where(n => n.CreatedOn <= toDate.AddDays(1));

            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                var search = request.SearchValue.ToLower();
                query = query.Where(n =>
                    (n.Activity != null && n.Activity.ToLower().Contains(search)) ||
                    (n.Recipient != null && n.Recipient.ToLower().Contains(search)) ||
                    (n.Sender != null && n.Sender.ToLower().Contains(search)) ||
                    (n.MessageId != null && n.MessageId.ToLower().Contains(search)));
            }

            request.Page.TotalItems = query.Count();
            response.PageInfo = request.Page;

            var skip = (request.Page.CurrentPage - 1) * request.Page.ItemsPerPage;
            var notifications = query
                .OrderByDescending(n => n.CreatedOn)
                .Skip(skip)
                .Take(request.Page.ItemsPerPage)
                .Select(n => new NotificationLogModel
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    MessageId = n.MessageId,
                    Type = (int)n.Type,
                    Activity = n.Activity,
                    Status = (int)n.Status,
                    Sender = n.Sender,
                    Recipient = n.Recipient,
                    CreatedOn = n.CreatedOn
                })
                .ToList();

            response.Notifications = notifications;
            return response;
        }
        catch (Exception ex)
        {
            _loggerService.WriteToWithCaller(Log.Error, ex, "Failed to retrieve notification logs.");
            throw;
        }
    }

    /// <summary>
    /// Retrieves the configured email and SMS notification templates from the application settings.
    /// </summary>
    /// <returns>The notification template response containing email and SMS template collections.</returns>
    public virtual Task<NotificationTemplateResponseModel> GetNotificationTemplatesAsync()
    {
        var response = new NotificationTemplateResponseModel
        {
            EmailTemplates = _HCLCSSFConfig.NotificationTemplateSettings?.EmailTemplateCollection ?? new List<EmailTemplate>(),
            SmsTemplates = _HCLCSSFConfig.NotificationTemplateSettings?.SMSTemplateCollection ?? new List<SMSTemplate>()
        };

        return Task.FromResult(response);
    }

    /// <summary>
    /// Retrieves a specific notification provider configuration by ID with secrets masked.
    /// </summary>
    /// <param name="id">The unique identifier of the provider configuration.</param>
    /// <returns>The provider configuration model with sensitive fields partially redacted.</returns>
    public virtual async Task<ProviderConfigModel> GetProviderConfigAsync(Guid id)
    {
        try
        {
            var config = await _providerConfigRepository.GetAsync(id);
            if (config == null)
                _frameworkResult.Throw(ApiErrorCodes.InvalidOrNullObject);

            return MapToProviderConfigModel(config, maskSecrets: true);
        }
        catch (Exception ex)
        {
            _loggerService.WriteToWithCaller(Log.Error, ex, "Failed to retrieve provider config.");
            throw;
        }
    }

    /// <summary>
    /// Retrieves all notification provider configurations with secrets masked.
    /// </summary>
    /// <returns>A list of all provider configurations with sensitive fields redacted.</returns>
    public virtual async Task<List<ProviderConfigModel>> GetAllProviderConfigsAsync()
    {
        try
        {
            var configs = await _providerConfigRepository.GetAllAsync();
            return configs.Select(c => MapToProviderConfigModel(c, maskSecrets: true)).ToList();
        }
        catch (Exception ex)
        {
            _loggerService.WriteToWithCaller(Log.Error, ex, "Failed to retrieve provider configs.");
            throw;
        }
    }

    /// <summary>
    /// Creates or updates a notification provider configuration. When activating a provider,
    /// automatically deactivates other providers of the same channel type to ensure a single active provider.
    /// </summary>
    /// <param name="request">The save request containing provider name, channel type, settings, and active flag.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or the validation error.</returns>
    public virtual async Task<FrameworkResult> SaveProviderConfigAsync(SaveProviderConfigRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.ProviderName))
                return _frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.InvalidOrNullObject);

            if (request.Settings == null || request.Settings.Count == 0)
                return _frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.InvalidOrNullObject);

            // Validate that the provider name matches the specified channel type to prevent
            // misconfiguration (e.g., saving an SMS provider like "BrevoSMS" under the Email channel)
            var channelMismatchError = ValidateChannelProviderMatch(request.ProviderName, request.ChannelType);
            if (channelMismatchError != null)
                return _frameworkResult.ConstructFailed(ApiErrorCodes.InvalidOrNullObject, channelMismatchError);

            var configJson = JsonSerializer.Serialize(request.Settings);

            if (request.Id.HasValue && request.Id.Value != Guid.Empty)
            {
                var existing = await _providerConfigRepository.GetAsync(request.Id.Value);
                if (existing == null)
                    return _frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.InvalidOrNullObject);

                existing.ProviderName = request.ProviderName;
                existing.ChannelType = request.ChannelType;
                existing.ConfigJson = configJson;

                if (request.IsActive && !existing.IsActive)
                    await DeactivateOtherProviders(request.ChannelType, existing.Id);

                existing.IsActive = request.IsActive;
                await _providerConfigRepository.UpdateAsync(existing);
            }
            else
            {
                var newConfig = new NotificationProviderConfig
                {
                    Id = Guid.NewGuid(),
                    ProviderName = request.ProviderName,
                    ChannelType = request.ChannelType,
                    IsActive = request.IsActive,
                    ConfigJson = configJson,
                    CreatedBy = "Admin"
                };

                if (request.IsActive)
                    await DeactivateOtherProviders(request.ChannelType, newConfig.Id);

                await _providerConfigRepository.InsertAsync(newConfig);
            }

            return await _providerConfigRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _loggerService.WriteToWithCaller(Log.Error, ex, "Failed to save provider config.");
            throw;
        }
    }

    /// <summary>
    /// Sets the specified provider as the active provider for its channel type, deactivating all others.
    /// </summary>
    /// <param name="request">The request containing the provider ID to activate.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> SetActiveProviderAsync(SetActiveProviderRequest request)
    {
        try
        {
            var config = await _providerConfigRepository.GetAsync(request.Id);
            if (config == null)
                return _frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.InvalidOrNullObject);

            await DeactivateOtherProviders(config.ChannelType, config.Id);
            config.IsActive = true;
            await _providerConfigRepository.UpdateAsync(config);
            return await _providerConfigRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _loggerService.WriteToWithCaller(Log.Error, ex, "Failed to set active provider.");
            throw;
        }
    }

    /// <summary>
    /// Deletes a notification provider configuration. The active provider cannot be deleted;
    /// another provider must be activated first.
    /// </summary>
    /// <param name="request">The request containing the provider ID to delete.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> DeleteProviderConfigAsync(DeleteProviderConfigRequest request)
    {
        try
        {
            var config = await _providerConfigRepository.GetAsync(request.Id);
            if (config == null)
                return _frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.InvalidOrNullObject);

            if (config.IsActive)
                return _frameworkResult.ConstructFailed(ApiErrorCodes.InvalidOrNullObject, "Cannot delete the active provider. Set another provider as active first.");

            await _providerConfigRepository.DeleteAsync(config);
            return await _providerConfigRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _loggerService.WriteToWithCaller(Log.Error, ex, "Failed to delete provider config.");
            throw;
        }
    }

    /// <summary>
    /// Returns field definitions for supported notification provider types (email and SMS),
    /// enabling the admin UI to dynamically render provider configuration forms.
    /// </summary>
    /// <returns>Field definitions for email and SMS provider types.</returns>
    public virtual Task<ProviderFieldDefinitionsResponse> GetProviderFieldDefinitionsAsync()
    {
        var response = new ProviderFieldDefinitionsResponse
        {
            EmailProviders = NotificationProviderConstants.EmailProviderFields,
            SmsProviders = NotificationProviderConstants.SmsProviderFields
        };

        return Task.FromResult(response);
    }

    /// <summary>
    /// Sends a test notification (email or SMS) through the specified or active provider, records the
    /// delivery result, and updates the provider's last-tested timestamp and success status.
    /// Routes to email or SMS provider based on the provider's channel type.
    /// </summary>
    /// <param name="request">The test request containing recipient, provider config ID, and notification type.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating whether the test notification was sent successfully.</returns>
    public virtual async Task<FrameworkResult> SendTestNotificationAsync(SendTestNotificationRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Recipient))
                return _frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.InvalidOrNullObject);

            // Resolve the provider config: use specific provider if given, otherwise active
            NotificationProviderConfig providerConfig = null;

            if (request.ProviderConfigId.HasValue && request.ProviderConfigId.Value != Guid.Empty)
            {
                providerConfig = await _providerConfigRepository.GetAsync(request.ProviderConfigId.Value);
            }
            else
            {
                var activeConfigs = await _providerConfigRepository.GetAsync(
                    x => x.ChannelType == request.Type && x.IsActive);
                providerConfig = activeConfigs?.FirstOrDefault();
            }

            if (providerConfig == null)
                return _frameworkResult.ConstructFailed(ApiErrorCodes.InvalidOrNullObject, "No provider configured. Please add and activate a provider first.");

            // Validate that the stored provider channel matches the requested test type to catch
            // misconfigured providers (e.g., an SMS provider saved under the Email channel)
            var mismatchError = ValidateChannelProviderMatch(providerConfig.ProviderName, providerConfig.ChannelType);
            if (mismatchError != null)
            {
                _loggerService.WriteTo(Log.Error, $"Channel/provider mismatch detected: {mismatchError}");
                return _frameworkResult.ConstructFailed(ApiErrorCodes.InvalidOrNullObject, mismatchError);
            }

            // Deserialize provider settings
            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(providerConfig.ConfigJson)
                         ?? new Dictionary<string, string>();

            // Route to the correct provider type based on channel
            (ProviderSendResult sendResult, string senderAddress) = providerConfig.ChannelType == (int)NotificationTypes.SMS
                ? await SendTestSmsAsync(providerConfig, config, request.Recipient)
                : await SendTestEmailAsync(providerConfig, config, request.Recipient);

            // Update provider test status
            providerConfig.LastTestedOn = DateTime.UtcNow;
            providerConfig.LastTestSuccess = sendResult.Success;
            await _providerConfigRepository.UpdateAsync(providerConfig);
            await _providerConfigRepository.SaveChangesAsync();

            // Save notification log so the test appears in Delivery Logs
            // Only save if we have a valid user ID (required by FK constraint on Users table)
            if (request.UserId.HasValue && request.UserId.Value != Guid.Empty)
            {
                var notificationType = providerConfig.ChannelType == (int)NotificationTypes.SMS
                    ? NotificationTypes.SMS
                    : NotificationTypes.Email;

                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId.Value,
                    MessageId = sendResult.MessageId ?? $"test-{Guid.NewGuid():N}",
                    Type = notificationType,
                    Activity = "Test Notification",
                    Status = sendResult.Success ? NotificationStatus.Delivered : NotificationStatus.Failed,
                    Sender = senderAddress,
                    Recipient = request.Recipient,
                    CreatedOn = DateTime.UtcNow
                };
                await _notificationRepository.InsertAsync(notification);
                await _notificationRepository.SaveChangesAsync();
            }

            var channelLabel = providerConfig.ChannelType == (int)NotificationTypes.SMS ? "SMS" : "Email";

            if (sendResult.Success)
            {
                _loggerService.WriteTo(Log.Debug, $"Test {channelLabel} sent successfully via {providerConfig.ProviderName}. MessageId: {sendResult.MessageId}");
                return _frameworkResult.Succeeded();
            }

            _loggerService.WriteTo(Log.Error, $"Test {channelLabel} failed via {providerConfig.ProviderName}: {sendResult.ErrorMessage}");
            return _frameworkResult.ConstructFailed(ApiErrorCodes.InvalidOrNullObject, $"{channelLabel} delivery failed: {sendResult.ErrorMessage}");
        }
        catch (Exception ex)
        {
            _loggerService.WriteToWithCaller(Log.Error, ex, "Failed to send test notification.");
            throw;
        }
    }

    /// <summary>
    /// Sends a test email through the resolved email provider.
    /// </summary>
    /// <returns>A tuple of the send result and the sender address used.</returns>
    private async Task<(ProviderSendResult Result, string SenderAddress)> SendTestEmailAsync(
        NotificationProviderConfig providerConfig,
        Dictionary<string, string> config,
        string recipient)
    {
        if (!_emailProviders.TryGetValue(providerConfig.ProviderName, out var provider))
            return (new ProviderSendResult { Success = false, ErrorMessage = $"Email provider '{providerConfig.ProviderName}' is not registered." }, string.Empty);

        var fromAddress = config.GetValueOrDefault("FromAddress", "no-reply@HCL.CS.SF.com");
        var fromName = config.GetValueOrDefault("FromName", "HCL.CS.SF");

        var sentAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        var testMessage = new EmailMessage
        {
            From = fromAddress,
            FromName = fromName,
            To = recipient,
            Subject = "Test Email \u2014 HCL.CS.SF",
            HtmlBody = "<!DOCTYPE html><html><head><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'></head><body style='margin:0;padding:0;background:#020617;font-family:-apple-system,BlinkMacSystemFont,\"Segoe UI\",Roboto,\"Helvetica Neue\",Arial,sans-serif;-webkit-font-smoothing:antialiased'><div style='height:4px;background:#2563eb'></div><div style='max-width:520px;margin:0 auto;padding:40px 20px'><table role='presentation' style='width:100%;border-collapse:collapse'><tr><td style='text-align:center;padding:28px 0 32px'><div style='display:inline-block;background:#2563eb;width:44px;height:44px;border-radius:10px;text-align:center;line-height:44px;font-size:20px;font-weight:800;color:#fff'>H</div><div style='margin-top:10px;font-size:14px;font-weight:700;letter-spacing:4px;color:#94a3b8'>HCL.CS.SF</div></td></tr></table><div style='background:#ffffff;border-radius:16px;box-shadow:0 8px 32px rgba(0,0,0,0.4);overflow:hidden'><div style='padding:40px 36px'><h1 style='margin:0 0 8px;font-size:22px;font-weight:700;color:#0f172a'>Email configuration test</h1><div style='width:40px;height:3px;background:#2563eb;border-radius:2px;margin:0 0 20px'></div><p style='margin:0 0 20px;font-size:15px;line-height:1.7;color:#334155'>This is a test email sent from the <strong>HCL.CS.SF</strong> panel. If you are reading this, your email provider is configured correctly.</p><div style='background:#f0fdf4;border:1px solid #bbf7d0;border-radius:8px;padding:16px 20px;margin:0 0 16px'><p style='margin:0;font-size:14px;line-height:1.6;color:#166534'><strong>Provider:</strong> " + providerConfig.ProviderName + "<br/><strong>Status:</strong> Delivered successfully</p></div><p style='margin:0;font-size:13px;color:#64748b;text-align:center'>Sent at " + sentAt + " UTC</p></div><div style='background:#f8fafc;border-top:1px solid #e2e8f0;padding:14px 36px'><p style='margin:0;font-size:13px;color:#64748b;line-height:1.5'>This is an automated test from HCL.CS.SF. No action is required.</p></div></div><table role='presentation' style='width:100%;border-collapse:collapse'><tr><td style='text-align:center;padding:28px 0 0'><p style='margin:0 0 6px;font-size:11px;color:#475569'>Secured by HCL.CS.SF Identity Platform</p><p style='margin:0 0 6px;font-size:11px;color:#475569'>Official HCL.CS.SF communication</p><p style='margin:0;font-size:11px;color:#334155'>&copy; 2026 HCL.CS.SF. All rights reserved.</p></td></tr></table></div></body></html>"
        };

        _loggerService.WriteTo(Log.Debug, $"Sending test email via {provider.ProviderName} to {recipient}");
        var result = await provider.SendAsync(testMessage, config);
        return (result, fromAddress);
    }

    /// <summary>
    /// Sends a test SMS through the resolved SMS provider.
    /// </summary>
    /// <returns>A tuple of the send result and the sender address used.</returns>
    private async Task<(ProviderSendResult Result, string SenderAddress)> SendTestSmsAsync(
        NotificationProviderConfig providerConfig,
        Dictionary<string, string> config,
        string recipient)
    {
        if (!_smsProviders.TryGetValue(providerConfig.ProviderName, out var provider))
            return (new ProviderSendResult { Success = false, ErrorMessage = $"SMS provider '{providerConfig.ProviderName}' is not registered." }, string.Empty);

        var sender = config.GetValueOrDefault("FromNumber",
            config.GetValueOrDefault("SenderName", providerConfig.ProviderName));

        var testMessage = new SmsMessage
        {
            From = sender,
            To = recipient,
            Body = $"HCL.CS.SF test SMS via {providerConfig.ProviderName}. If you receive this, your SMS provider is configured correctly."
        };

        _loggerService.WriteTo(Log.Debug, $"Sending test SMS via {provider.ProviderName} to {recipient}");
        var result = await provider.SendAsync(testMessage, config);
        return (result, sender);
    }

    /// <summary>
    /// Validates that a provider name belongs to the correct channel type.
    /// Prevents misconfiguration such as saving an SMS provider (e.g., "BrevoSMS") under the Email channel.
    /// </summary>
    /// <param name="providerName">The provider name to validate.</param>
    /// <param name="channelType">The channel type (1 = Email, 2 = SMS).</param>
    /// <returns>An error message if there is a mismatch, or null if the combination is valid.</returns>
    private string ValidateChannelProviderMatch(string providerName, int channelType)
    {
        var isEmailChannel = channelType == (int)NotificationTypes.Email;
        var isSmsChannel = channelType == (int)NotificationTypes.SMS;

        // Check if the provider is registered as an email provider
        var isEmailProvider = _emailProviders.ContainsKey(providerName)
            || NotificationProviderConstants.EmailProviderFields.ContainsKey(providerName);

        // Check if the provider is registered as an SMS provider
        var isSmsProvider = _smsProviders.ContainsKey(providerName)
            || NotificationProviderConstants.SmsProviderFields.ContainsKey(providerName);

        if (isEmailChannel && isSmsProvider && !isEmailProvider)
            return $"Provider '{providerName}' is an SMS provider and cannot be configured under the Email channel. Please select Channel: SMS.";

        if (isSmsChannel && isEmailProvider && !isSmsProvider)
            return $"Provider '{providerName}' is an Email provider and cannot be configured under the SMS channel. Please select Channel: Email.";

        return null;
    }

    private async Task DeactivateOtherProviders(int channelType, Guid excludeId)
    {
        var activeConfigs = await _providerConfigRepository.GetAsync(
            c => c.ChannelType == channelType && c.IsActive && c.Id != excludeId);

        if (activeConfigs != null)
        {
            foreach (var config in activeConfigs)
            {
                config.IsActive = false;
                await _providerConfigRepository.UpdateAsync(config);
            }
        }
    }

    private static ProviderConfigModel MapToProviderConfigModel(NotificationProviderConfig config, bool maskSecrets)
    {
        var settings = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(config.ConfigJson))
        {
            try
            {
                settings = JsonSerializer.Deserialize<Dictionary<string, string>>(config.ConfigJson)
                    ?? new Dictionary<string, string>();
            }
            catch
            {
                settings = new Dictionary<string, string>();
            }
        }

        if (maskSecrets)
        {
            // Get field definitions to know which fields are passwords
            Dictionary<string, ProviderFieldDefinition[]> fieldDefs;
            if (config.ChannelType == (int)NotificationTypes.Email)
                fieldDefs = NotificationProviderConstants.EmailProviderFields;
            else
                fieldDefs = NotificationProviderConstants.SmsProviderFields;

            if (fieldDefs.TryGetValue(config.ProviderName, out var fields))
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
        }

        return new ProviderConfigModel
        {
            Id = config.Id,
            ProviderName = config.ProviderName,
            ChannelType = config.ChannelType,
            IsActive = config.IsActive,
            Settings = settings,
            LastTestedOn = config.LastTestedOn,
            LastTestSuccess = config.LastTestSuccess
        };
    }
}
