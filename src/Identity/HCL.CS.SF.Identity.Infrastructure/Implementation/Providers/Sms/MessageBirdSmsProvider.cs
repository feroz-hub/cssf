/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using MessageBird;
using MessageBird.Objects;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.DomainServices.Infra;

namespace HCL.CS.SF.Infrastructure.Services.Implementation.Providers.Sms;

/// <summary>
/// SMS provider implementation that sends messages via the MessageBird API.
/// Requires <c>AccessKey</c> and <c>Originator</c> in the configuration dictionary.
/// </summary>
public class MessageBirdSmsProvider : ISmsProvider
{
    /// <summary>
    /// Gets the unique provider name used for factory-based resolution.
    /// </summary>
    public string ProviderName => NotificationProviderConstants.MessageBird;

    /// <summary>
    /// Sends an SMS message using the MessageBird API.
    /// </summary>
    /// <param name="message">The SMS message containing recipient, body, and optional sender override.</param>
    /// <param name="config">Provider configuration including AccessKey and Originator.</param>
    /// <returns>A <see cref="ProviderSendResult"/> indicating success or failure.</returns>
    public async Task<ProviderSendResult> SendAsync(SmsMessage message, Dictionary<string, string> config)
    {
        try
        {
            var accessKey = config["AccessKey"];
            var originator = !string.IsNullOrWhiteSpace(message.From) ? message.From : config["Originator"];

            var client = Client.CreateDefault(accessKey);

            // MessageBird SDK is synchronous; wrap in Task.Run to avoid blocking
            var result = await Task.Run(() =>
                client.SendMessage(originator, message.Body, new[] { long.Parse(message.To) })
            );

            return new ProviderSendResult
            {
                Success = true,
                MessageId = result.Id?.ToString() ?? string.Empty,
                ErrorMessage = string.Empty
            };
        }
        catch (Exception ex)
        {
            return new ProviderSendResult
            {
                Success = false,
                MessageId = string.Empty,
                ErrorMessage = $"MessageBird SMS sending failed: {ex.Message}"
            };
        }
    }
}
