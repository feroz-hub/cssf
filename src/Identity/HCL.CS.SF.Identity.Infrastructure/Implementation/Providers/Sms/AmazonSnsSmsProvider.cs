/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.DomainServices.Infra;

namespace HCL.CS.SF.Infrastructure.Services.Implementation.Providers.Sms;

/// <summary>
/// SMS provider implementation that sends messages via Amazon Simple Notification Service (SNS).
/// Requires <c>AccessKeyId</c>, <c>SecretAccessKey</c>, and <c>Region</c> in the configuration dictionary.
/// Optionally supports <c>SenderId</c> for branded sender identification.
/// </summary>
public class AmazonSnsSmsProvider : ISmsProvider
{
    /// <summary>
    /// Gets the unique provider name used for factory-based resolution.
    /// </summary>
    public string ProviderName => NotificationProviderConstants.AmazonSns;

    /// <summary>
    /// Sends an SMS message using AWS SNS Publish API.
    /// </summary>
    /// <param name="message">The SMS message containing recipient phone number and body text.</param>
    /// <param name="config">Provider configuration including AccessKeyId, SecretAccessKey, Region, and optional SenderId.</param>
    /// <returns>A <see cref="ProviderSendResult"/> indicating success or failure with the SNS message ID.</returns>
    public async Task<ProviderSendResult> SendAsync(SmsMessage message, Dictionary<string, string> config)
    {
        try
        {
            var accessKeyId = config["AccessKeyId"];
            var secretAccessKey = config["SecretAccessKey"];
            var region = config["Region"];

            // Create an SNS client scoped to the configured AWS region
            using var client = new AmazonSimpleNotificationServiceClient(
                accessKeyId,
                secretAccessKey,
                RegionEndpoint.GetBySystemName(region)
            );

            var publishRequest = new PublishRequest
            {
                PhoneNumber = message.To,
                Message = message.Body
            };

            // Set optional sender ID attribute for regions that support alphanumeric sender IDs
            if (config.TryGetValue("SenderId", out var senderId) && !string.IsNullOrWhiteSpace(senderId))
            {
                publishRequest.MessageAttributes["AWS.SNS.SMS.SenderID"] = new MessageAttributeValue
                {
                    DataType = "String",
                    StringValue = senderId
                };
            }

            // Mark as transactional to ensure high-priority delivery
            publishRequest.MessageAttributes["AWS.SNS.SMS.SMSType"] = new MessageAttributeValue
            {
                DataType = "String",
                StringValue = "Transactional"
            };

            var response = await client.PublishAsync(publishRequest);

            return new ProviderSendResult
            {
                Success = true,
                MessageId = response.MessageId,
                ErrorMessage = string.Empty
            };
        }
        catch (Exception ex)
        {
            return new ProviderSendResult
            {
                Success = false,
                MessageId = string.Empty,
                ErrorMessage = $"Amazon SNS SMS sending failed: {ex.Message}"
            };
        }
    }
}
