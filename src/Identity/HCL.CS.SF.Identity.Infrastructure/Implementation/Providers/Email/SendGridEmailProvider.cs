/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using SendGrid;
using SendGrid.Helpers.Mail;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.DomainServices.Infra;

namespace HCL.CS.SF.Infrastructure.Services.Implementation.Providers.Email;

/// <summary>
/// Email provider implementation that sends transactional emails via the SendGrid API.
/// Requires <c>ApiKey</c> in the configuration dictionary. Optionally uses <c>FromAddress</c> and <c>FromName</c>.
/// </summary>
public class SendGridEmailProvider : IEmailProvider
{
    /// <summary>
    /// Gets the unique provider name used for factory-based resolution.
    /// </summary>
    public string ProviderName => NotificationProviderConstants.SendGrid;

    /// <summary>
    /// Sends an email message using the SendGrid API.
    /// </summary>
    /// <param name="message">The email message containing recipient, subject, HTML body, and optional CC.</param>
    /// <param name="config">Provider configuration including ApiKey, and optional FromAddress/FromName.</param>
    /// <returns>A <see cref="ProviderSendResult"/> indicating success or failure with the SendGrid message ID.</returns>
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

            var client = new SendGridClient(config["ApiKey"]);

            var from = new EmailAddress(fromAddress, fromName);
            var to = new EmailAddress(message.To);
            var msg = MailHelper.CreateSingleEmail(from, to, message.Subject, null, message.HtmlBody);

            if (!string.IsNullOrWhiteSpace(message.CC))
            {
                msg.AddCc(new EmailAddress(message.CC));
            }

            var response = await client.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                // SendGrid returns the message ID in the X-Message-Id response header
                var messageId = response.Headers.TryGetValues("X-Message-Id", out var values)
                    ? values.FirstOrDefault() ?? string.Empty
                    : string.Empty;

                return new ProviderSendResult
                {
                    Success = true,
                    MessageId = messageId,
                    ErrorMessage = string.Empty
                };
            }

            var errorBody = await response.Body.ReadAsStringAsync();
            return new ProviderSendResult
            {
                Success = false,
                MessageId = string.Empty,
                ErrorMessage = $"SendGrid returned {response.StatusCode}: {errorBody}"
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
