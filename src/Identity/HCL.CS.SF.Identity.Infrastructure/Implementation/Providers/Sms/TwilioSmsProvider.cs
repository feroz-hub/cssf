/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.DomainServices.Infra;

namespace HCL.CS.SF.Infrastructure.Services.Implementation.Providers.Sms;

/// <summary>
/// SMS provider implementation that sends messages via the Twilio REST API.
/// Requires <c>AccountSid</c>, <c>AuthToken</c>, and <c>FromNumber</c> in the configuration dictionary.
/// </summary>
public class TwilioSmsProvider : ISmsProvider
{
    /// <summary>
    /// Gets the unique provider name used for factory-based resolution.
    /// </summary>
    public string ProviderName => NotificationProviderConstants.Twilio;

    /// <summary>
    /// Sends an SMS message using the Twilio API.
    /// </summary>
    /// <param name="message">The SMS message containing recipient, body, and optional sender override.</param>
    /// <param name="config">Provider configuration including AccountSid, AuthToken, and FromNumber.</param>
    /// <returns>A <see cref="ProviderSendResult"/> indicating success or failure with the Twilio message SID.</returns>
    public async Task<ProviderSendResult> SendAsync(SmsMessage message, Dictionary<string, string> config)
    {
        try
        {
            var accountSid = config["AccountSid"];
            var authToken = config["AuthToken"];
            var fromNumber = !string.IsNullOrWhiteSpace(message.From) ? message.From : config["FromNumber"];

            // Initialize the Twilio client with account credentials
            TwilioClient.Init(accountSid, authToken);

            var createOptions = new CreateMessageOptions(new PhoneNumber(message.To))
            {
                From = new PhoneNumber(fromNumber),
                Body = message.Body
            };

            // Set status callback URL from config or message-level override for delivery tracking
            if (config.TryGetValue("StatusCallbackUrl", out var statusCallbackUrl)
                && !string.IsNullOrWhiteSpace(statusCallbackUrl))
            {
                createOptions.StatusCallback = new Uri(statusCallbackUrl);
            }
            else if (!string.IsNullOrWhiteSpace(message.CallbackUrl))
            {
                createOptions.StatusCallback = new Uri(message.CallbackUrl);
            }

            var result = await MessageResource.CreateAsync(createOptions);

            return new ProviderSendResult
            {
                Success = true,
                MessageId = result.Sid,
                ErrorMessage = string.Empty
            };
        }
        catch (Exception ex)
        {
            return new ProviderSendResult
            {
                Success = false,
                MessageId = string.Empty,
                ErrorMessage = $"Twilio SMS sending failed: {ex.Message}"
            };
        }
    }
}
