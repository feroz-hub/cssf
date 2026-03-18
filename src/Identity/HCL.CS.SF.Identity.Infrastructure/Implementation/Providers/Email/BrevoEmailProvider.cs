/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.DomainServices.Infra;

namespace HCL.CS.SF.Infrastructure.Services.Implementation.Providers.Email;

/// <summary>
/// Email provider implementation that sends transactional emails via the Brevo (formerly Sendinblue) API.
/// Requires <c>ApiKey</c> in the configuration dictionary. Optionally uses <c>FromAddress</c> and <c>FromName</c>.
/// </summary>
public class BrevoEmailProvider : IEmailProvider
{
    /// <summary>
    /// Gets the unique provider name used for factory-based resolution.
    /// </summary>
    public string ProviderName => NotificationProviderConstants.Brevo;

    /// <summary>
    /// Sends an email message using the Brevo Transactional Email API.
    /// </summary>
    /// <param name="message">The email message containing recipient, subject, HTML body, and optional CC.</param>
    /// <param name="config">Provider configuration including ApiKey, and optional FromAddress/FromName.</param>
    /// <returns>A <see cref="ProviderSendResult"/> indicating success or failure with the Brevo message ID.</returns>
    public async Task<ProviderSendResult> SendAsync(EmailMessage message, Dictionary<string, string> config)
    {
        try
        {
            // Resolve sender address: message-level override takes priority over config
            var fromAddress = !string.IsNullOrWhiteSpace(message.From)
                ? message.From
                : config.GetValueOrDefault("FromAddress", string.Empty);

            var fromName = !string.IsNullOrWhiteSpace(message.FromName)
                ? message.FromName
                : config.GetValueOrDefault("FromName", string.Empty);

            // Configure the Brevo SDK with the API key
            var configuration = new Configuration();
            configuration.ApiKey.Add("api-key", config["ApiKey"]);

            var apiInstance = new TransactionalEmailsApi(configuration);

            var sendSmtpEmail = new SendSmtpEmail
            {
                Sender = new SendSmtpEmailSender(fromName, fromAddress),
                To = new List<SendSmtpEmailTo> { new(message.To) },
                Subject = message.Subject,
                HtmlContent = message.HtmlBody
            };

            if (!string.IsNullOrWhiteSpace(message.CC))
            {
                sendSmtpEmail.Cc = new List<SendSmtpEmailCc> { new(message.CC) };
            }

            var result = await apiInstance.SendTransacEmailAsync(sendSmtpEmail);

            return new ProviderSendResult
            {
                Success = true,
                MessageId = result.MessageId ?? string.Empty,
                ErrorMessage = string.Empty
            };
        }
        catch (Exception ex)
        {
            return new ProviderSendResult
            {
                Success = false,
                MessageId = string.Empty,
                ErrorMessage = ex.Message
            };
        }
    }
}
