/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Plivo;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.DomainServices.Infra;

namespace HCL.CS.SF.Infrastructure.Services.Implementation.Providers.Sms;

/// <summary>
/// SMS provider implementation that sends messages via the Plivo API.
/// Requires <c>AuthId</c>, <c>AuthToken</c>, and <c>FromNumber</c> in the configuration dictionary.
/// </summary>
public class PlivoSmsProvider : ISmsProvider
{
    /// <summary>
    /// Gets the unique provider name used for factory-based resolution.
    /// </summary>
    public string ProviderName => NotificationProviderConstants.Plivo;

    /// <summary>
    /// Sends an SMS message using the Plivo API.
    /// </summary>
    /// <param name="message">The SMS message containing recipient, body, and optional callback URL.</param>
    /// <param name="config">Provider configuration including AuthId, AuthToken, and FromNumber.</param>
    /// <returns>A <see cref="ProviderSendResult"/> indicating success or failure with the Plivo message UUID.</returns>
    public async Task<ProviderSendResult> SendAsync(SmsMessage message, Dictionary<string, string> config)
    {
        try
        {
            var authId = config["AuthId"];
            var authToken = config["AuthToken"];
            var fromNumber = !string.IsNullOrWhiteSpace(message.From) ? message.From : config["FromNumber"];

            var client = new PlivoApi(authId, authToken);

            // Plivo SDK is synchronous; wrap in Task.Run to avoid blocking
            var response = await Task.Run(() =>
                client.Message.Create(
                    src: fromNumber,
                    dst: new List<string> { message.To },
                    text: message.Body,
                    url: !string.IsNullOrWhiteSpace(message.CallbackUrl) ? message.CallbackUrl : null
                )
            );

            return new ProviderSendResult
            {
                Success = true,
                MessageId = response.MessageUuid?.FirstOrDefault() ?? string.Empty,
                ErrorMessage = string.Empty
            };
        }
        catch (Exception ex)
        {
            return new ProviderSendResult
            {
                Success = false,
                MessageId = string.Empty,
                ErrorMessage = $"Plivo SMS sending failed: {ex.Message}"
            };
        }
    }
}
