/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Vonage;
using Vonage.Messaging;
using Vonage.Request;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.DomainServices.Infra;

namespace HCL.CS.SF.Infrastructure.Services.Implementation.Providers.Sms;

/// <summary>
/// SMS provider implementation that sends messages via the Vonage (formerly Nexmo) SMS API.
/// Requires <c>ApiKey</c>, <c>ApiSecret</c>, and <c>FromNumber</c> in the configuration dictionary.
/// </summary>
public class VonageSmsProvider : ISmsProvider
{
    /// <summary>
    /// Gets the unique provider name used for factory-based resolution.
    /// </summary>
    public string ProviderName => NotificationProviderConstants.Vonage;

    /// <summary>
    /// Sends an SMS message using the Vonage SMS API.
    /// </summary>
    /// <param name="message">The SMS message containing recipient, body, and optional sender override.</param>
    /// <param name="config">Provider configuration including ApiKey, ApiSecret, and FromNumber.</param>
    /// <returns>A <see cref="ProviderSendResult"/> indicating success or failure with the Vonage message ID.</returns>
    public async Task<ProviderSendResult> SendAsync(SmsMessage message, Dictionary<string, string> config)
    {
        try
        {
            var apiKey = config["ApiKey"];
            var apiSecret = config["ApiSecret"];
            var fromNumber = !string.IsNullOrWhiteSpace(message.From) ? message.From : config["FromNumber"];

            // Authenticate with Vonage using API key and secret
            var credentials = Credentials.FromApiKeyAndSecret(apiKey, apiSecret);
            var client = new VonageClient(credentials);

            var response = await client.SmsClient.SendAnSmsAsync(new SendSmsRequest
            {
                From = fromNumber,
                To = message.To,
                Text = message.Body
            });

            var firstMessage = response.Messages?.FirstOrDefault();

            // Vonage uses status "0" to indicate successful delivery
            if (firstMessage?.Status == "0")
            {
                return new ProviderSendResult
                {
                    Success = true,
                    MessageId = firstMessage.MessageId,
                    ErrorMessage = string.Empty
                };
            }

            return new ProviderSendResult
            {
                Success = false,
                MessageId = string.Empty,
                ErrorMessage = $"Vonage SMS failed: {firstMessage?.ErrorText ?? "Unknown error"}"
            };
        }
        catch (Exception ex)
        {
            return new ProviderSendResult
            {
                Success = false,
                MessageId = string.Empty,
                ErrorMessage = $"Vonage SMS sending failed: {ex.Message}"
            };
        }
    }
}
