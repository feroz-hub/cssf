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

namespace HCL.CS.SF.Infrastructure.Services.Implementation.Providers.Sms;

/// <summary>
/// SMS provider implementation that sends transactional SMS via the Brevo (formerly Sendinblue) API.
/// Requires <c>ApiKey</c> and <c>SenderName</c> in the configuration dictionary.
/// </summary>
public class BrevoSmsProvider : ISmsProvider
{
    /// <summary>
    /// Gets the unique provider name used for factory-based resolution.
    /// </summary>
    public string ProviderName => NotificationProviderConstants.BrevoSms;

    /// <summary>
    /// Sends an SMS message using the Brevo Transactional SMS API.
    /// </summary>
    /// <param name="message">The SMS message containing recipient, body, and optional callback URL.</param>
    /// <param name="config">Provider configuration including ApiKey and SenderName.</param>
    /// <returns>A <see cref="ProviderSendResult"/> indicating success or failure with the Brevo message ID.</returns>
    public async Task<ProviderSendResult> SendAsync(SmsMessage message, Dictionary<string, string> config)
    {
        try
        {
            var apiKey = config["ApiKey"];
            var senderName = !string.IsNullOrWhiteSpace(message.From) ? message.From : config["SenderName"];

            // Configure the Brevo SDK with the API key
            var configuration = new Configuration();
            configuration.AddApiKey("api-key", apiKey);

            var apiInstance = new TransactionalSMSApi(configuration);

            var sendTransacSms = new SendTransacSms(
                sender: senderName,
                recipient: message.To,
                content: message.Body
            );

            // Optionally attach a webhook URL for delivery status callbacks
            if (!string.IsNullOrWhiteSpace(message.CallbackUrl))
            {
                sendTransacSms.WebUrl = message.CallbackUrl;
            }

            var result = await apiInstance.SendTransacSmsAsync(sendTransacSms);

            return new ProviderSendResult
            {
                Success = true,
                MessageId = result.MessageId.ToString(),
                ErrorMessage = string.Empty
            };
        }
        catch (Exception ex)
        {
            return new ProviderSendResult
            {
                Success = false,
                MessageId = string.Empty,
                ErrorMessage = $"Brevo SMS sending failed: {ex.Message}"
            };
        }
    }
}
