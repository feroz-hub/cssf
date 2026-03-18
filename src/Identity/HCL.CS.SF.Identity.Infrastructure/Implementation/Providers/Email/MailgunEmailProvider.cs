/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using RestSharp;
using RestSharp.Authenticators;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.DomainServices.Infra;

namespace HCL.CS.SF.Infrastructure.Services.Implementation.Providers.Email;

/// <summary>
/// Email provider implementation that sends transactional emails via the Mailgun REST API.
/// Requires <c>ApiKey</c> and <c>Domain</c> in the configuration dictionary.
/// Supports US and EU regions via the optional <c>Region</c> config key.
/// </summary>
public class MailgunEmailProvider : IEmailProvider
{
    /// <summary>
    /// Gets the unique provider name used for factory-based resolution.
    /// </summary>
    public string ProviderName => NotificationProviderConstants.Mailgun;

    /// <summary>
    /// Sends an email message using the Mailgun messages API endpoint.
    /// </summary>
    /// <param name="message">The email message containing recipient, subject, HTML body, and optional CC.</param>
    /// <param name="config">Provider configuration including ApiKey, Domain, and optional Region (US/EU) and FromAddress/FromName.</param>
    /// <returns>A <see cref="ProviderSendResult"/> indicating success or failure with the Mailgun message ID.</returns>
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

            var domain = config["Domain"];
            var region = config.GetValueOrDefault("Region", "US");

            // Select the correct Mailgun API base URL based on region (EU has a separate endpoint)
            var baseUrl = string.Equals(region, "EU", StringComparison.OrdinalIgnoreCase)
                ? "https://api.eu.mailgun.net/v3"
                : "https://api.mailgun.net/v3";

            // Authenticate using HTTP Basic Auth with "api" as the username and the API key as password
            var options = new RestClientOptions($"{baseUrl}/{domain}")
            {
                Authenticator = new HttpBasicAuthenticator("api", config["ApiKey"])
            };

            using var client = new RestClient(options);

            var request = new RestRequest("messages", Method.Post);
            request.AddParameter("from", from);
            request.AddParameter("to", message.To);
            request.AddParameter("subject", message.Subject);
            request.AddParameter("html", message.HtmlBody);

            if (!string.IsNullOrWhiteSpace(message.CC))
            {
                request.AddParameter("cc", message.CC);
            }

            var response = await client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                // Extract message ID from the JSON response body
                var messageId = string.Empty;
                if (!string.IsNullOrWhiteSpace(response.Content))
                {
                    var doc = System.Text.Json.JsonDocument.Parse(response.Content);
                    if (doc.RootElement.TryGetProperty("id", out var idElement))
                    {
                        messageId = idElement.GetString() ?? string.Empty;
                    }
                }

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
                ErrorMessage = $"Mailgun returned {response.StatusCode}: {response.Content}"
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
