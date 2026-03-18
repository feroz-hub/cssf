/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Configurations.Api;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.DomainServices.Infra;

namespace HCL.CS.SF.Service.Implementation.Api.Utils;

/// <summary>
/// Utility class for sending identity-related notifications (email and SMS) for operations
/// such as two-factor authentication, email verification, phone verification, and password reset.
/// Routes notifications to the appropriate channel based on the user's two-factor type
/// and resolves notification templates from configuration.
/// </summary>

internal class NotificationUtil
{
    private readonly EmailConfig config;
    private readonly IEmailService emailSender;
    private readonly IFrameworkResultService frameworkResult;
    private readonly ISmsService smsSender;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationUtil"/> class.
    /// </summary>
    internal NotificationUtil(
        IEmailService emailSender,
        ISmsService smsSender,
        IFrameworkResultService frameworkResult,
        EmailConfig config)
    {
        this.emailSender = emailSender;
        this.smsSender = smsSender;
        this.frameworkResult = frameworkResult;
        this.config = config;
    }

    /// <summary>
    /// Sends a two-factor authentication notification via the user's configured 2FA channel (email or SMS).
    /// </summary>
    /// <param name="user">The user entity containing contact information and 2FA preferences.</param>
    /// <param name="purpose">The notification purpose (e.g., email verification, 2FA token).</param>
    /// <param name="token">The security token or verification code to include in the notification.</param>
    /// <returns>The result of the notification send operation.</returns>

    internal async Task<FrameworkResult> TwoFactorNotificationAsync(Users user, string purpose, string token)
    {
        if (user.TwoFactorType == TwoFactorType.Email) return await SendEmailAsync(user, purpose, token);

        if (user.TwoFactorType == TwoFactorType.Sms) return await SendSmsAsync(user, purpose, token);

        return null;
    }

    /// <summary>
    /// Sends an email notification using the configured email service and template system.
    /// </summary>
    /// <param name="user">The user entity with the destination email address.</param>
    /// <param name="purpose">The notification purpose used to resolve the email template.</param>
    /// <param name="token">The security token to embed in the email template.</param>
    /// <returns>The result of the email send operation.</returns>

    internal async Task<FrameworkResult> SendEmailAsync(Users user, string purpose, string token)
    {
        if (!GlobalConfiguration.IsEmailConfigurationValid)
            return frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.InvalidEmailConfiguration);

        var emailParameter = new Dictionary<string, string>();
        emailParameter.Add("{TOKEN}", token);

        var templateName = GetTemplateName(purpose);
        var notification = new NotificationInfoModel
        {
            UserId = user.Id,
            TemplateName = templateName,
            Activity = purpose,
            ToAddress = user.Email,
            Parameters = emailParameter
        };
        return await emailSender.SendEmailAsync(notification);
    }

    /// <summary>
    /// Sends an SMS notification using the configured SMS service and template system.
    /// </summary>
    /// <param name="user">The user entity with the destination phone number.</param>
    /// <param name="purpose">The notification purpose used to resolve the SMS template.</param>
    /// <param name="token">The security token to embed in the SMS template.</param>
    /// <returns>The result of the SMS send operation.</returns>

    internal async Task<FrameworkResult> SendSmsAsync(Users user, string purpose, string token)
    {
        if (!GlobalConfiguration.IsSmsConfigurationValid)
            return frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.InvalidSmsConfiguration);

        var smsParameter = new Dictionary<string, string>();
        smsParameter.Add("{TOKEN}", token);

        var templateName = GetTemplateName(purpose);
        var smsMessage = new NotificationInfoModel
        {
            UserId = user.Id,
            Activity = purpose,
            ToAddress = user.PhoneNumber,
            TemplateName = templateName,
            Parameters = smsParameter
        };
        return await smsSender.SendSmsAsync(smsMessage);
    }

    /// <summary>
    /// Resolves the notification template name based on the purpose and configured notification type
    /// (e.g., link-based vs. token-based email verification).
    /// </summary>

    private string GetTemplateName(string purpose)
    {
        var templateName = string.Empty;
        switch (purpose)
        {
            case NotificationConstants.EmailVerification:
                if (config.EmailNotificationType == EmailNotificationType.Link)
                    templateName = NotificationConstants.EmailVerificationUsingLink;
                else if (config.EmailNotificationType == EmailNotificationType.Token)
                    templateName = NotificationConstants.EmailVerificationUsingToken;

                break;
            case NotificationConstants.PhoneNumberVerification:
                templateName = NotificationConstants.PhoneNumberVerificationToken;
                break;
            case NotificationConstants.GenerateTwoFactorToken:
                templateName = NotificationConstants.GenerateTwoFactorToken;
                break;
            case NotificationConstants.ResetPasswordUsingToken:
                templateName = NotificationConstants.ResetPasswordUsingToken;
                break;
            default:
                templateName = purpose;
                break;
        }

        return templateName;
    }
}
