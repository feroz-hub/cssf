/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.DomainServices.Infra;

/// <summary>
/// Pluggable SMS delivery provider (e.g., Twilio, AWS SNS). Each implementation
/// supplies a <see cref="ProviderName"/> for configuration look-up and handles
/// the low-level HTTP call to the SMS gateway.
/// </summary>
public interface ISmsProvider
{
    /// <summary>Gets the unique name identifying this SMS provider (e.g., "Twilio").</summary>
    string ProviderName { get; }

    /// <summary>
    /// Sends an SMS message using the provider-specific API.
    /// </summary>
    /// <param name="message">The SMS message details (from, to, body).</param>
    /// <param name="config">Provider-specific configuration key-value pairs (API keys, endpoints, etc.).</param>
    /// <returns>A <see cref="ProviderSendResult"/> indicating success or failure with a message ID.</returns>
    Task<ProviderSendResult> SendAsync(SmsMessage message, Dictionary<string, string> config);
}

/// <summary>
/// Represents an outgoing SMS message with sender, recipient, body text, and optional callback URL.
/// </summary>
public class SmsMessage
{
    /// <summary>Gets or sets the sender phone number or short code.</summary>
    public string From { get; set; }

    /// <summary>Gets or sets the recipient phone number in E.164 format.</summary>
    public string To { get; set; }

    /// <summary>Gets or sets the text body of the SMS message.</summary>
    public string Body { get; set; }

    /// <summary>Gets or sets an optional webhook URL for delivery status callbacks.</summary>
    public string CallbackUrl { get; set; }
}
