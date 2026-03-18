/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.DomainServices.Infra;

namespace HCL.CS.SF.Infrastructure.Services.Implementation.Providers.Email;

/// <summary>
/// Email provider implementation that sends transactional emails via Amazon Simple Email Service (SES).
/// Requires <c>AccessKeyId</c>, <c>SecretAccessKey</c>, and <c>Region</c> in the configuration dictionary.
/// </summary>
public class AmazonSesEmailProvider : IEmailProvider
{
    /// <summary>
    /// Gets the unique provider name used for factory-based resolution.
    /// </summary>
    public string ProviderName => NotificationProviderConstants.AmazonSes;

    /// <summary>
    /// Sends an email message using the AWS SES SendEmail API.
    /// </summary>
    /// <param name="message">The email message containing recipient, subject, HTML body, and optional CC.</param>
    /// <param name="config">Provider configuration including AccessKeyId, SecretAccessKey, Region, and optional FromAddress/FromName.</param>
    /// <returns>A <see cref="ProviderSendResult"/> indicating success or failure with the SES message ID.</returns>
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

            // Format as "Name <address>" when a display name is provided
            var source = !string.IsNullOrWhiteSpace(fromName)
                ? $"{fromName} <{fromAddress}>"
                : fromAddress;

            var region = RegionEndpoint.GetBySystemName(config["Region"]);

            // Create an SES client scoped to the configured AWS region
            using var client = new AmazonSimpleEmailServiceClient(
                config["AccessKeyId"],
                config["SecretAccessKey"],
                region);

            var sendRequest = new SendEmailRequest
            {
                Source = source,
                Destination = new Destination
                {
                    ToAddresses = new List<string> { message.To }
                },
                Message = new Message
                {
                    Subject = new Content(message.Subject),
                    Body = new Body
                    {
                        Html = new Content(message.HtmlBody)
                    }
                }
            };

            if (!string.IsNullOrWhiteSpace(message.CC))
            {
                sendRequest.Destination.CcAddresses = new List<string> { message.CC };
            }

            var response = await client.SendEmailAsync(sendRequest);

            return new ProviderSendResult
            {
                Success = true,
                MessageId = response.MessageId ?? string.Empty,
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
