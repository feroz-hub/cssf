/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Text.Json;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Configurations.Api;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.DomainServices.Wrappers;
using HCL.CS.SF.Infrastructure.Services.Extension;
using HCL.CS.SF.Infrastructure.Services.Implementation.Providers;

namespace HCL.CS.SF.Infrastructure.Services.Implementation;

/// <summary>
/// Orchestrates SMS sending by resolving notification templates, selecting the appropriate
/// SMS provider (DB-configured or fallback Twilio), and persisting delivery status to the database.
/// </summary>
internal class SmsService : ISmsService
{
    /// <summary>
    /// User manager for resolving user details during template placeholder replacement.
    /// </summary>
    private readonly UserManagerWrapper<Users> csUserManager;

    /// <summary>
    /// Service for constructing standardized success/failure result objects.
    /// </summary>
    private readonly IFrameworkResultService frameworkResultService;

    /// <summary>
    /// HTTP context accessor used for reading Twilio status callback form data.
    /// </summary>
    private readonly IHttpContextAccessor httpContextAccessor;

    /// <summary>
    /// Logger service for diagnostic and error logging.
    /// </summary>
    private readonly ILoggerService loggerService;

    /// <summary>
    /// AutoMapper instance for mapping between notification models and entities.
    /// </summary>
    private readonly IMapper mapper;

    /// <summary>
    /// Twilio SMS configuration from appsettings, used as the fallback when no DB-configured provider is active.
    /// </summary>
    private readonly SMSConfig smsConfig;

    /// <summary>
    /// Repository for persisting and updating SMS notification records.
    /// </summary>
    private readonly IRepository<Notification> smsRepository;

    /// <summary>
    /// Repository for querying active notification provider configurations from the database.
    /// </summary>
    private readonly IRepository<NotificationProviderConfig> providerConfigRepository;

    /// <summary>
    /// Factory that resolves the correct SMS provider implementation by name.
    /// </summary>
    private readonly NotificationProviderFactory providerFactory;

    /// <summary>
    /// Notification template settings containing SMS template definitions.
    /// </summary>
    private readonly NotificationTemplateSettings templateSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmsService"/> class with all required dependencies.
    /// </summary>
    /// <param name="userManager">User manager for resolving user data during template rendering.</param>
    /// <param name="configSettings">Application-wide configuration including SMS and template settings.</param>
    /// <param name="loggerInstance">Logger instance registry for obtaining the default logger.</param>
    /// <param name="smsRepository">Repository for SMS notification persistence.</param>
    /// <param name="providerConfigRepository">Repository for querying active provider configurations.</param>
    /// <param name="providerFactory">Factory for resolving SMS providers by name.</param>
    /// <param name="mapper">AutoMapper instance for model-to-entity mapping.</param>
    /// <param name="frameworkResultService">Service for building standardized result objects.</param>
    /// <param name="httpContextAccessor">HTTP context accessor for reading Twilio callback form data.</param>
    public SmsService(
        UserManagerWrapper<Users> userManager,
        HCLCSSFConfig configSettings,
        ILoggerInstance loggerInstance,
        IRepository<Notification> smsRepository,
        IRepository<NotificationProviderConfig> providerConfigRepository,
        NotificationProviderFactory providerFactory,
        IMapper mapper,
        IFrameworkResultService frameworkResultService,
        IHttpContextAccessor httpContextAccessor)
    {
        this.smsRepository = smsRepository;
        this.providerConfigRepository = providerConfigRepository;
        this.providerFactory = providerFactory;
        this.mapper = mapper;
        this.httpContextAccessor = httpContextAccessor;
        templateSettings = configSettings.NotificationTemplateSettings;
        loggerService = loggerInstance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
        smsConfig = configSettings.SystemSettings.SMSConfig;
        this.frameworkResultService = frameworkResultService;
        csUserManager = userManager;
    }

    /// <summary>
    /// Sends an SMS notification by resolving the template, validating the message,
    /// and dispatching via an active DB-configured provider or the fallback Twilio configuration.
    /// </summary>
    /// <param name="notificationModel">The notification model containing template name, user ID, and parameters.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or the specific validation/delivery failure.</returns>
    public async Task<FrameworkResult> SendSmsAsync(NotificationInfoModel notificationModel)
    {
        if (notificationModel == null)
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidOrNullObject);

        if (string.IsNullOrWhiteSpace(notificationModel.TemplateName))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidTemplateName);

        try
        {
            var message = mapper.Map<NotificationInfoModel, SMSMessage>(notificationModel);
            string errorCode;
            (message, errorCode) = await ResolveSmsTemplate(message);
            if (!string.IsNullOrWhiteSpace(errorCode)) return frameworkResultService.Failed<FrameworkResult>(errorCode);

            var result = ValidateSmsMessage(message);
            if (result.Status == ResultStatus.Failed) return result;

            // Check for active DB-configured provider
            var activeProviderConfigs = await providerConfigRepository.GetAsync(
                x => x.ChannelType == 2 && x.IsActive);

            if (activeProviderConfigs != null && activeProviderConfigs.Any())
            {
                var providerConfig = activeProviderConfigs[0];
                await SendSmsViaProvider(message, providerConfig);
            }
            else
            {
                // Fallback to Twilio config from appsettings
                result = ValidateSmsConfiguration();
                if (result.Status == ResultStatus.Failed) return result;

                var accountSid = smsConfig.SMSAccountIdentification;
                var authToken = smsConfig.SMSAccountPassword;

                TwilioClient.Init(accountSid, authToken);
                var sendToNumber = message.To;
                var messageResource = await MessageResource.CreateAsync(
                    new PhoneNumber(sendToNumber),
                    from: new PhoneNumber(smsConfig.SMSAccountFrom),
                    body: message.Content);

                await SaveSmsNotification(message, messageResource);
            }

            loggerService.WriteTo(Log.Debug, "SMS sent");
            return frameworkResultService.Succeeded();
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "SMS delivery failed");
            throw;
        }
    }

    /// <summary>
    /// Sends an SMS through the specified DB-configured provider, persists the notification record,
    /// and updates its status based on the provider's response.
    /// </summary>
    /// <param name="message">The resolved SMS message model with template content applied.</param>
    /// <param name="providerConfig">The active provider configuration from the database.</param>
    /// <exception cref="InvalidOperationException">Thrown when the provider reports a delivery failure.</exception>
    private async Task SendSmsViaProvider(SMSMessage message, NotificationProviderConfig providerConfig)
    {
        var config = JsonSerializer.Deserialize<Dictionary<string, string>>(providerConfig.ConfigJson)
                     ?? new Dictionary<string, string>();

        var smsMessage = new SmsMessage
        {
            From = config.GetValueOrDefault("FromNumber") ?? config.GetValueOrDefault("SenderName") ?? config.GetValueOrDefault("Originator"),
            To = message.To,
            Body = message.Content,
            CallbackUrl = config.GetValueOrDefault("StatusCallbackUrl")
        };

        // Save notification record
        var notification = mapper.Map<SMSMessage, Notification>(message);
        notification.MessageId = Guid.NewGuid().ToString();
        notification.Type = NotificationTypes.SMS;
        notification.Status = NotificationStatus.Initiated;
        notification.Sender = smsMessage.From;
        await smsRepository.InsertAsync(notification);
        await smsRepository.SaveChangesAsync();

        var provider = providerFactory.GetSmsProvider(providerConfig.ProviderName);
        loggerService.WriteTo(Log.Debug, $"Sending SMS via {provider.ProviderName}");

        var sendResult = await provider.SendAsync(smsMessage, config);

        if (sendResult.Success)
        {
            notification.MessageId = sendResult.MessageId ?? notification.MessageId;
            notification.Status = NotificationStatus.Delivered;
        }
        else
        {
            notification.Status = NotificationStatus.Failed;
            loggerService.WriteTo(Log.Error, $"Provider {provider.ProviderName} failed: {sendResult.ErrorMessage}");
        }

        await smsRepository.UpdateAsync(notification);
        await smsRepository.SaveChangesAsync();

        if (!sendResult.Success)
            throw new InvalidOperationException($"SMS delivery failed via {provider.ProviderName}: {sendResult.ErrorMessage}");
    }

    // TODO: Testing is pending, need deployment environment to get call back from Twilio.

    /// <summary>
    /// Updates the delivery status of an SMS notification based on a Twilio status callback.
    /// Reads <c>MessageSid</c> and <c>MessageStatus</c> from the HTTP request form data.
    /// </summary>
    /// <returns>A <see cref="FrameworkResult"/> indicating whether the status update succeeded.</returns>
    public async Task<FrameworkResult> UpdateSmsStatusAsync()
    {
        try
        {
            var request = httpContextAccessor.HttpContext.Request;
            if (request != null)
            {
                var smsSid = request.Form["MessageSid"];
                var messageStatus = request.Form["MessageStatus"];
                var notification = await smsRepository.GetAsync(x => x.MessageId == smsSid);
                if (notification != null && notification.Any())
                {
                    var message = notification[0];
                    // TODO - At testing need to find the message status format to update.
                    // message.Status = messageStatus;
                    await smsRepository.UpdateAsync(message);
                    return await smsRepository.SaveChangesAsync();
                }
            }

            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidOrNullObject);
        }
        catch (Exception ex)
        {
            loggerService.WriteTo(Log.Error, "SMS status update failed", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Resolves the SMS template by name, populates placeholders with user data and custom parameters,
    /// and applies the resolved content to the message model.
    /// </summary>
    /// <param name="message">The SMS message model containing the template name and user ID.</param>
    /// <returns>A tuple of the updated message model and an error code string (empty on success).</returns>
    private async Task<(SMSMessage, string)> ResolveSmsTemplate(SMSMessage message)
    {
        var smsTemplate = templateSettings.SMSTemplateCollection.Find(x => x.Name == message.TemplateName);
        if (smsTemplate != null)
        {
            var templateFormat = smsTemplate.TemplateFormat.Clone().ToString();

            var users = await csUserManager.FindByIdAsync(message.UserId.ToString());
            if (users != null)
                templateFormat = !string.IsNullOrWhiteSpace(templateFormat)
                    ? templateFormat.UpdateNotificationTemplatePlaceholder(users, message.Parameters)
                    : null;
            else
                return (message, ApiErrorCodes.InvalidUserId);

            message.Content = templateFormat;
            return (message, string.Empty);
        }

        return (message, ApiErrorCodes.TemplateDoesNotExists);
    }

    /// <summary>
    /// Persists a new SMS notification record using the Twilio message resource details.
    /// </summary>
    /// <param name="message">The SMS message model to map into a notification entity.</param>
    /// <param name="messageResource">The Twilio message resource containing the SID and status.</param>
    private async Task SaveSmsNotification(SMSMessage message, MessageResource messageResource)
    {
        try
        {
            var notification = mapper.Map<SMSMessage, Notification>(message);
            notification.MessageId = messageResource.Sid;
            notification.Type = NotificationTypes.SMS;
            notification.Status = GetStatus(messageResource);
            notification.Sender = smsConfig.SMSAccountFrom;
            await smsRepository.InsertAsync(notification);
            await smsRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            loggerService.WriteTo(Log.Error, "SMS delivery failed", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Validates the resolved SMS message for required fields (to number, content)
    /// and enforces field length constraints.
    /// </summary>
    /// <param name="message">The SMS message model to validate.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating validation success or the specific failure.</returns>
    private FrameworkResult ValidateSmsMessage(SMSMessage message)
    {
        loggerService.WriteTo(Log.Debug, "Validating SMS message");
        if (string.IsNullOrWhiteSpace(message.To))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.ToNumberNotConfiguredForSMS);

        if (string.IsNullOrWhiteSpace(message.Content))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.NoContentSpecifiedForSMS);

        if (!string.IsNullOrWhiteSpace(message.Activity) && message.Activity.Length > Constants.ColumnLength255)
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.ActivityTooLong);

        return frameworkResultService.Succeeded();
    }

    /// <summary>
    /// Validates the fallback Twilio SMS configuration from appsettings for required fields
    /// (account SID, auth token, from number, callback URL).
    /// </summary>
    /// <returns>A <see cref="FrameworkResult"/> indicating validation success or the specific failure.</returns>
    private FrameworkResult ValidateSmsConfiguration()
    {
        loggerService.WriteTo(Log.Debug, "Validating SMS configuration");
        if (string.IsNullOrWhiteSpace(smsConfig.SMSAccountIdentification))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.UserNameNotConfiguredForSMS);

        if (string.IsNullOrWhiteSpace(smsConfig.SMSAccountPassword))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.PasswordNotConfiguredForSMS);

        if (string.IsNullOrWhiteSpace(smsConfig.SMSAccountFrom))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.FromNumberNotConfiguredForSMS);

        if (string.IsNullOrWhiteSpace(smsConfig.SMSStatusCallbackURL))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.SMSCallbackURLMissing);

        return frameworkResultService.Succeeded();
    }

    /// <summary>
    /// Converts a Twilio <see cref="MessageResource"/> status to the internal <see cref="NotificationStatus"/> enum.
    /// </summary>
    /// <param name="messageResource">The Twilio message resource whose status is to be mapped.</param>
    /// <returns>The corresponding <see cref="NotificationStatus"/> value.</returns>
    private NotificationStatus GetStatus(MessageResource messageResource)
    {
        return (NotificationStatus)Enum.Parse(typeof(NotificationStatus), messageResource.Status.ToString());
    }
}
