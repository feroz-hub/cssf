/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.DomainServices.Infra;

/// <summary>
/// Pluggable email delivery provider (e.g., SendGrid, AWS SES, SMTP).
/// Each implementation supplies a <see cref="ProviderName"/> for configuration look-up
/// and handles the low-level API call to send an email.
/// </summary>
public interface IEmailProvider
{
    /// <summary>Gets the unique name identifying this email provider (e.g., "SendGrid").</summary>
    string ProviderName { get; }

    /// <summary>
    /// Sends an email message using the provider-specific API.
    /// </summary>
    /// <param name="message">The email message details.</param>
    /// <param name="config">Provider-specific configuration key-value pairs (API keys, endpoints, etc.).</param>
    /// <returns>A <see cref="ProviderSendResult"/> indicating success or failure with a message ID.</returns>
    Task<ProviderSendResult> SendAsync(EmailMessage message, Dictionary<string, string> config);
}

/// <summary>
/// Represents an outgoing email message with sender, recipient, subject, HTML body, and optional CC.
/// </summary>
public class EmailMessage
{
    /// <summary>Gets or sets the sender email address.</summary>
    public string From { get; set; }

    /// <summary>Gets or sets the display name of the sender.</summary>
    public string FromName { get; set; }

    /// <summary>Gets or sets the recipient email address.</summary>
    public string To { get; set; }

    /// <summary>Gets or sets the email subject line.</summary>
    public string Subject { get; set; }

    /// <summary>Gets or sets the HTML body content of the email.</summary>
    public string HtmlBody { get; set; }

    /// <summary>Gets or sets the CC (carbon copy) recipient email address.</summary>
    public string CC { get; set; }
}

/// <summary>
/// Represents the outcome of a provider-level send operation (email or SMS),
/// including success status, provider message ID, and any error details.
/// </summary>
public class ProviderSendResult
{
    /// <summary>Gets or sets whether the send operation succeeded.</summary>
    public bool Success { get; set; }

    /// <summary>Gets or sets the provider-assigned message identifier for tracking.</summary>
    public string MessageId { get; set; }

    /// <summary>Gets or sets the error message if the send operation failed.</summary>
    public string ErrorMessage { get; set; }
}
