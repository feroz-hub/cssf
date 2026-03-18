/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Text.Json;
using AutoMapper;
using MailKit;
using MailKit.Security;
using MimeKit;
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
using HCL.CS.SF.Infrastructure.Services.Wrapper;

namespace HCL.CS.SF.Infrastructure.Services.Implementation;

/// <summary>
/// Orchestrates email sending by resolving notification templates, selecting the appropriate
/// email provider (DB-configured or fallback SMTP), and persisting delivery status to the database.
/// </summary>
internal class EmailService : IEmailService
{
    /// <summary>
    /// User manager for resolving user details during template placeholder replacement.
    /// </summary>
    private readonly UserManagerWrapper<Users> csUserManager;

    /// <summary>
    /// SMTP configuration from appsettings, used as the fallback when no DB-configured provider is active.
    /// </summary>
    private readonly EmailConfig emailConfig;

    /// <summary>
    /// Repository for persisting and updating email notification records.
    /// </summary>
    private readonly IRepository<Notification> emailRepository;

    /// <summary>
    /// Repository for querying active notification provider configurations from the database.
    /// </summary>
    private readonly IRepository<NotificationProviderConfig> providerConfigRepository;

    /// <summary>
    /// Factory that resolves the correct email provider implementation by name.
    /// </summary>
    private readonly NotificationProviderFactory providerFactory;

    /// <summary>
    /// Service for constructing standardized success/failure result objects.
    /// </summary>
    private readonly IFrameworkResultService frameworkResultService;

    /// <summary>
    /// Logger service for diagnostic and error logging.
    /// </summary>
    private readonly ILoggerService loggerService;

    /// <summary>
    /// AutoMapper instance for mapping between notification models and entities.
    /// </summary>
    private readonly IMapper mapper;

    /// <summary>
    /// Notification template settings containing email template definitions.
    /// </summary>
    private readonly NotificationTemplateSettings templateSettings;

    /// <summary>
    /// Tracks the current notification entity across send/save operations.
    /// </summary>
    private Notification notification;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailService"/> class with all required dependencies.
    /// </summary>
    /// <param name="userManager">User manager for resolving user data during template rendering.</param>
    /// <param name="configSettings">Application-wide configuration including email and template settings.</param>
    /// <param name="loggerInstance">Logger instance registry for obtaining the default logger.</param>
    /// <param name="emailRepository">Repository for email notification persistence.</param>
    /// <param name="providerConfigRepository">Repository for querying active provider configurations.</param>
    /// <param name="providerFactory">Factory for resolving email providers by name.</param>
    /// <param name="mapper">AutoMapper instance for model-to-entity mapping.</param>
    /// <param name="frameworkResultService">Service for building standardized result objects.</param>
    public EmailService(
        UserManagerWrapper<Users> userManager,
        HCLCSSFConfig configSettings,
        ILoggerInstance loggerInstance,
        IRepository<Notification> emailRepository,
        IRepository<NotificationProviderConfig> providerConfigRepository,
        NotificationProviderFactory providerFactory,
        IMapper mapper,
        IFrameworkResultService frameworkResultService)
    {
        this.emailRepository = emailRepository;
        this.providerConfigRepository = providerConfigRepository;
        this.providerFactory = providerFactory;
        this.mapper = mapper;

        loggerService = loggerInstance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
        templateSettings = configSettings.NotificationTemplateSettings;
        emailConfig = configSettings.SystemSettings.EmailConfig;
        this.frameworkResultService = frameworkResultService;
        csUserManager = userManager;
    }

    /// <summary>
    /// Sends an email notification by resolving the template, validating the message,
    /// and dispatching via an active DB-configured provider or the fallback SMTP configuration.
    /// </summary>
    /// <param name="notificationModel">The notification model containing template name, user ID, and parameters.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or the specific validation/delivery failure.</returns>
    public async Task<FrameworkResult> SendEmailAsync(NotificationInfoModel notificationModel)
    {
        if (notificationModel == null)
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidOrNullObject);

        if (string.IsNullOrWhiteSpace(notificationModel.TemplateName))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidTemplateName);

        try
        {
            var message = mapper.Map<NotificationInfoModel, EmailMessageModel>(notificationModel);
            string errorCode;
            (message, errorCode) = await ResolveEmailTemplate(message);
            if (!string.IsNullOrWhiteSpace(errorCode)) return frameworkResultService.Failed<FrameworkResult>(errorCode);

            var result = ValidateIncomingMessage(message);
            if (result.Status == ResultStatus.Failed) return result;

            // Check for active DB-configured provider
            var activeProviderConfigs = await providerConfigRepository.GetAsync(
                x => x.ChannelType == 1 && x.IsActive);

            if (activeProviderConfigs != null && activeProviderConfigs.Any())
            {
                var providerConfig = activeProviderConfigs[0];
                await SendViaProvider(message, providerConfig);
            }
            else
            {
                // Fallback to SMTP config from appsettings
                result = ValidateEmailConfiguration();
                if (result.Status == ResultStatus.Failed) return result;

                var mailMessage = CreateEmailMessage(message);
                await SendEmailAsync(mailMessage, message);
            }

            return frameworkResultService.Succeeded();
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Email delivery failed");
            throw;
        }
    }

    /// <summary>
    /// Sends an email through the specified DB-configured provider, persists the notification record,
    /// and updates its status based on the provider's response.
    /// </summary>
    /// <param name="message">The resolved email message model with template content applied.</param>
    /// <param name="providerConfig">The active provider configuration from the database.</param>
    /// <exception cref="InvalidOperationException">Thrown when the provider reports a delivery failure.</exception>
    private async Task SendViaProvider(EmailMessageModel message, NotificationProviderConfig providerConfig)
    {
        var config = JsonSerializer.Deserialize<Dictionary<string, string>>(providerConfig.ConfigJson)
                     ?? new Dictionary<string, string>();

        var emailMessage = new EmailMessage
        {
            From = message.FromAddress,
            FromName = message.FromName,
            To = message.ToAddress,
            Subject = message.Subject,
            HtmlBody = message.Content,
            CC = message.CC
        };

        await SaveEmailNotification(message, Guid.NewGuid().ToString());

        var provider = providerFactory.GetEmailProvider(providerConfig.ProviderName);
        loggerService.WriteTo(Log.Debug, $"Sending email via {provider.ProviderName}");

        var sendResult = await provider.SendAsync(emailMessage, config);

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

        await emailRepository.UpdateAsync(notification);
        await emailRepository.SaveChangesAsync();

        if (!sendResult.Success)
            throw new InvalidOperationException($"Email delivery failed via {provider.ProviderName}: {sendResult.ErrorMessage}");
    }

    /// <summary>
    /// Resolves the email template by name, populates placeholders with user data and custom parameters,
    /// and applies the resolved content, subject, and sender details to the message model.
    /// </summary>
    /// <param name="message">The email message model containing the template name and user ID.</param>
    /// <returns>A tuple of the updated message model and an error code string (empty on success).</returns>
    private async Task<(EmailMessageModel, string)> ResolveEmailTemplate(EmailMessageModel message)
    {
        var emailTemplate = templateSettings.EmailTemplateCollection.Find(x => x.Name == message.TemplateName);
        var temlateFormat = emailTemplate.TemplateFormat;
        if (emailTemplate != null)
        {
            var users = await csUserManager.FindByIdAsync(message.UserId.ToString());
            if (users != null)
                temlateFormat = !string.IsNullOrWhiteSpace(temlateFormat)
                    ? temlateFormat.UpdateNotificationTemplatePlaceholder(users, message.Parameters)
                    : null;
            else
                return (message, ApiErrorCodes.InvalidUserId);

            message.Content = temlateFormat;
            message.Subject = emailTemplate.Subject;
            message.FromAddress = emailTemplate.FromAddress;
            message.FromName = emailTemplate.FromName;
            message.CC = emailTemplate.CC;
            return (message, string.Empty);
        }

        return (message, ApiErrorCodes.TemplateDoesNotExists);
    }

    /// <summary>
    /// Constructs a <see cref="MimeMessage"/> from the resolved email message model for SMTP delivery.
    /// </summary>
    /// <param name="message">The email message model with fully resolved content.</param>
    /// <returns>A configured <see cref="MimeMessage"/> ready for SMTP transmission.</returns>
    private MimeMessage CreateEmailMessage(EmailMessageModel message)
    {
        loggerService.WriteTo(Log.Debug, "Constructing Email message");

        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress(message.FromName, message.FromAddress));
        emailMessage.To.Add(new MailboxAddress(message.ToName, message.ToAddress));
        emailMessage.Subject = message.Subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = message.Content };
        emailMessage.Body = bodyBuilder.ToMessageBody();
        return emailMessage;
    }

    /// <summary>
    /// Sends an email via the fallback SMTP configuration using MailKit, handling connection,
    /// authentication, delivery, and notification status updates.
    /// </summary>
    /// <param name="mailMessage">The MIME message to send.</param>
    /// <param name="message">The email message model used for notification record persistence.</param>
    private async Task SendEmailAsync(MimeMessage mailMessage, EmailMessageModel message)
    {
        using var smtpClient = new SmtpClientWrapper();
        try
        {
            await SaveEmailNotification(message, mailMessage.MessageId);

            smtpClient.MessageSent += OnMessageSent;
            loggerService.WriteTo(Log.Debug, "Sending email");

            if (emailConfig.SecureSocketOptions)
                await smtpClient.ConnectAsync(emailConfig.SmtpServer, emailConfig.Port, true);
            else
                await smtpClient.ConnectAsync(emailConfig.SmtpServer, emailConfig.Port, SecureSocketOptions.StartTls);

            smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");
            if (emailConfig.UserName != null && emailConfig.Password != null)
                try
                {
                    await smtpClient.AuthenticateAsync(emailConfig.UserName, emailConfig.Password);
                }
                catch (Exception ex)
                {
                    loggerService.WriteTo(Log.Error, "Email Authentication Failure:" + ex.Message);
                }

            await smtpClient.SendAsync(mailMessage);
        }
        catch (Exception ex)
        {
            notification.Status = NotificationStatus.Failed;
            await emailRepository.UpdateAsync(notification);
            await emailRepository.SaveChangesAsync();
            loggerService.WriteToWithCaller(Log.Error, ex, "Email delivery failed");
            throw;
        }
        finally
        {
            await smtpClient.DisconnectAsync(true);
            smtpClient.Dispose();
            loggerService.WriteTo(Log.Debug, "Email client disconnected");
        }
    }

    /// <summary>
    /// Event handler invoked by MailKit when a message is successfully sent.
    /// Updates the notification status to <see cref="NotificationStatus.Delivered"/>.
    /// </summary>
    private void OnMessageSent(object sender, MessageSentEventArgs e)
    {
        notification.Status = NotificationStatus.Delivered;
        emailRepository.UpdateAsync(notification);
        emailRepository.SaveChangesAsync();
    }

    /// <summary>
    /// Persists a new email notification record with an initial status of <see cref="NotificationStatus.Initiated"/>.
    /// </summary>
    /// <param name="message">The email message model to map into a notification entity.</param>
    /// <param name="messageId">The unique message identifier (MIME ID or provider-assigned ID).</param>
    private async Task SaveEmailNotification(EmailMessageModel message, string messageId)
    {
        try
        {
            notification = mapper.Map<EmailMessageModel, Notification>(message);
            notification.MessageId = messageId;
            notification.Type = NotificationTypes.Email;
            notification.Status = NotificationStatus.Initiated;
            await emailRepository.InsertAsync(notification);
            await emailRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while saving email notification");
            throw;
        }
    }

    /// <summary>
    /// Validates the resolved email message for required fields (to address, content, from address, subject)
    /// and enforces field length constraints.
    /// </summary>
    /// <param name="message">The email message model to validate.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating validation success or the specific failure.</returns>
    private FrameworkResult ValidateIncomingMessage(EmailMessageModel message)
    {
        loggerService.WriteTo(Log.Debug, "Validating Email message");
        if (string.IsNullOrWhiteSpace(message.ToAddress))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidToAddress);

        if (string.IsNullOrWhiteSpace(message.Content))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.NoContentSpecifiedForEmail);

        if (string.IsNullOrWhiteSpace(message.FromAddress))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidFromAddress);

        if (string.IsNullOrWhiteSpace(message.Subject))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidSubject);

        if (!string.IsNullOrWhiteSpace(message.Activity) && message.Activity.Length > Constants.ColumnLength255)
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.ActivityTooLong);

        return frameworkResultService.Succeeded();
    }

    /// <summary>
    /// Validates the fallback SMTP configuration from appsettings for required fields
    /// (server, port, username, password).
    /// </summary>
    /// <returns>A <see cref="FrameworkResult"/> indicating validation success or the specific failure.</returns>
    private FrameworkResult ValidateEmailConfiguration()
    {
        loggerService.WriteTo(Log.Debug, "Validating Email configuration");
        if (string.IsNullOrWhiteSpace(emailConfig.SmtpServer))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.SmtpServerNotConfiguredForEmail);

        if (emailConfig.Port <= 0)
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.PortNotConfiguredForEmail);

        if (string.IsNullOrWhiteSpace(emailConfig.UserName))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.UserNameNotConfiguredForEmail);

        if (string.IsNullOrWhiteSpace(emailConfig.Password))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.PasswordNotConfiguredForEmail);

        return frameworkResultService.Succeeded();
    }
}
