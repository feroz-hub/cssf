/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.DomainServices.Infra;

namespace HCL.CS.SF.Infrastructure.Services.Implementation.Providers.Email;

/// <summary>
/// Email provider implementation that sends transactional emails via the Resend REST API.
/// Requires <c>ApiKey</c> in the configuration dictionary. Optionally uses <c>FromAddress</c> and <c>FromName</c>.
/// </summary>
public class ResendEmailProvider : IEmailProvider
{
    /// <summary>
    /// Shared HTTP client instance for Resend API calls (reused to leverage connection pooling).
    /// </summary>
    private static readonly HttpClient HttpClient = new();

    /// <summary>
    /// Gets the unique provider name used for factory-based resolution.
    /// </summary>
    public string ProviderName => NotificationProviderConstants.Resend;

    /// <summary>
    /// Sends an email message using the Resend API via HTTP POST.
    /// </summary>
    /// <param name="message">The email message containing recipient, subject, HTML body, and optional CC.</param>
    /// <param name="config">Provider configuration including ApiKey, and optional FromAddress/FromName.</param>
    /// <returns>A <see cref="ProviderSendResult"/> indicating success or failure with the Resend message ID.</returns>
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
            var from = !string.IsNullOrWhiteSpace(fromName)
                ? $"{fromName} <{fromAddress}>"
                : fromAddress;

            // Build the JSON payload for the Resend API
            var payload = new Dictionary<string, object>
            {
                ["from"] = from,
                ["to"] = new[] { message.To },
                ["subject"] = message.Subject,
                ["html"] = message.HtmlBody
            };

            if (!string.IsNullOrWhiteSpace(message.CC))
            {
                payload["cc"] = new[] { message.CC };
            }

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config["ApiKey"]);

            var response = await HttpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // Extract the message ID from the JSON response
                var responseDoc = JsonDocument.Parse(responseBody);
                var messageId = responseDoc.RootElement.TryGetProperty("id", out var idElement)
                    ? idElement.GetString() ?? string.Empty
                    : string.Empty;

                return new ProviderSendResult
                {
                    Success = true,
                    MessageId = messageId,
                    ErrorMessage = string.Empty
                };
            }

            return new ProviderSendResult
            {
                Success = false,
                MessageId = string.Empty,
                ErrorMessage = $"Resend returned {response.StatusCode}: {responseBody}"
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
