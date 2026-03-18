/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using PostmarkDotNet;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.DomainServices.Infra;

namespace HCL.CS.SF.Infrastructure.Services.Implementation.Providers.Email;

/// <summary>
/// Email provider implementation that sends transactional emails via the Postmark API.
/// Requires <c>ServerToken</c> in the configuration dictionary. Optionally uses <c>FromAddress</c> and <c>FromName</c>.
/// </summary>
public class PostmarkEmailProvider : IEmailProvider
{
    /// <summary>
    /// Gets the unique provider name used for factory-based resolution.
    /// </summary>
    public string ProviderName => NotificationProviderConstants.Postmark;

    /// <summary>
    /// Sends an email message using the Postmark API.
    /// </summary>
    /// <param name="message">The email message containing recipient, subject, HTML body, and optional CC.</param>
    /// <param name="config">Provider configuration including ServerToken, and optional FromAddress/FromName.</param>
    /// <returns>A <see cref="ProviderSendResult"/> indicating success or failure with the Postmark message ID.</returns>
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

            var from = !string.IsNullOrWhiteSpace(fromName)
                ? $"{fromName} <{fromAddress}>"
                : fromAddress;

            var client = new PostmarkClient(config["ServerToken"]);

            var postmarkMessage = new PostmarkMessage
            {
                From = from,
                To = message.To,
                Subject = message.Subject,
                HtmlBody = message.HtmlBody
            };

            if (!string.IsNullOrWhiteSpace(message.CC))
            {
                postmarkMessage.Cc = message.CC;
            }

            var response = await client.SendMessageAsync(postmarkMessage);

            // Postmark uses a status enum to indicate success or failure
            if (response.Status == PostmarkStatus.Success)
            {
                return new ProviderSendResult
                {
                    Success = true,
                    MessageId = response.MessageID.ToString(),
                    ErrorMessage = string.Empty
                };
            }

            return new ProviderSendResult
            {
                Success = false,
                MessageId = string.Empty,
                ErrorMessage = $"Postmark error: {response.Message}"
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
