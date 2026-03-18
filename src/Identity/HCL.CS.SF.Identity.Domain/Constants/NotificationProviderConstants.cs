/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Constants;

/// <summary>
/// Constants and field definitions for supported notification providers (email and SMS).
/// Each provider has a unique string identifier and a set of required/optional configuration fields
/// that the admin UI uses to render dynamic configuration forms.
/// </summary>
public static class NotificationProviderConstants
{
    // ──── Email Provider Identifiers ────

    /// <summary>
    /// SMTP email provider identifier. Uses direct SMTP server connection for email delivery.
    /// </summary>
    public const string Smtp = "SMTP";

    /// <summary>
    /// SendGrid email provider identifier. Uses the SendGrid API for email delivery.
    /// </summary>
    public const string SendGrid = "SendGrid";

    /// <summary>
    /// Brevo (formerly Sendinblue) email provider identifier. Uses the Brevo API for email delivery.
    /// </summary>
    public const string Brevo = "Brevo";

    /// <summary>
    /// Resend email provider identifier. Uses the Resend API for email delivery.
    /// </summary>
    public const string Resend = "Resend";

    /// <summary>
    /// Amazon Simple Email Service (SES) provider identifier. Uses AWS SES for email delivery.
    /// </summary>
    public const string AmazonSes = "AmazonSES";

    /// <summary>
    /// Mailgun email provider identifier. Uses the Mailgun API for email delivery.
    /// </summary>
    public const string Mailgun = "Mailgun";

    /// <summary>
    /// Postmark email provider identifier. Uses the Postmark API for email delivery.
    /// </summary>
    public const string Postmark = "Postmark";

    // ──── SMS Provider Identifiers ────

    /// <summary>
    /// Twilio SMS provider identifier. Uses the Twilio API for SMS delivery.
    /// </summary>
    public const string Twilio = "Twilio";

    /// <summary>
    /// Brevo SMS provider identifier. Uses the Brevo SMS API for message delivery.
    /// </summary>
    public const string BrevoSms = "BrevoSMS";

    /// <summary>
    /// Vonage (formerly Nexmo) SMS provider identifier. Uses the Vonage API for SMS delivery.
    /// </summary>
    public const string Vonage = "Vonage";

    /// <summary>
    /// Amazon Simple Notification Service (SNS) provider identifier. Uses AWS SNS for SMS delivery.
    /// </summary>
    public const string AmazonSns = "AmazonSNS";

    /// <summary>
    /// MessageBird SMS provider identifier. Uses the MessageBird API for SMS delivery.
    /// </summary>
    public const string MessageBird = "MessageBird";

    /// <summary>
    /// Plivo SMS provider identifier. Uses the Plivo API for SMS delivery.
    /// </summary>
    public const string Plivo = "Plivo";

    /// <summary>
    /// Maps each email provider identifier to its required and optional configuration fields.
    /// Used by the admin UI to dynamically render provider-specific configuration forms.
    /// </summary>
    public static readonly Dictionary<string, ProviderFieldDefinition[]> EmailProviderFields = new()
    {
        [Smtp] = new[]
        {
            new ProviderFieldDefinition("SmtpServer", "SMTP Server", "text", true),
            new ProviderFieldDefinition("Port", "Port", "number", true),
            new ProviderFieldDefinition("UserName", "Username", "text", true),
            new ProviderFieldDefinition("Password", "Password", "password", true),
            new ProviderFieldDefinition("UseSsl", "Use SSL", "boolean", false),
            new ProviderFieldDefinition("FromAddress", "Default From Address", "email", true),
            new ProviderFieldDefinition("FromName", "Default From Name", "text", false)
        },
        [SendGrid] = new[]
        {
            new ProviderFieldDefinition("ApiKey", "API Key", "password", true),
            new ProviderFieldDefinition("FromAddress", "From Address", "email", true),
            new ProviderFieldDefinition("FromName", "From Name", "text", false)
        },
        [Brevo] = new[]
        {
            new ProviderFieldDefinition("ApiKey", "API Key", "password", true),
            new ProviderFieldDefinition("FromAddress", "From Address", "email", true),
            new ProviderFieldDefinition("FromName", "From Name", "text", false)
        },
        [Resend] = new[]
        {
            new ProviderFieldDefinition("ApiKey", "API Key", "password", true),
            new ProviderFieldDefinition("FromAddress", "From Address", "email", true),
            new ProviderFieldDefinition("FromName", "From Name", "text", false)
        },
        [AmazonSes] = new[]
        {
            new ProviderFieldDefinition("AccessKeyId", "Access Key ID", "text", true),
            new ProviderFieldDefinition("SecretAccessKey", "Secret Access Key", "password", true),
            new ProviderFieldDefinition("Region", "AWS Region", "text", true),
            new ProviderFieldDefinition("FromAddress", "From Address", "email", true),
            new ProviderFieldDefinition("FromName", "From Name", "text", false)
        },
        [Mailgun] = new[]
        {
            new ProviderFieldDefinition("ApiKey", "API Key", "password", true),
            new ProviderFieldDefinition("Domain", "Mailgun Domain", "text", true),
            new ProviderFieldDefinition("FromAddress", "From Address", "email", true),
            new ProviderFieldDefinition("FromName", "From Name", "text", false),
            new ProviderFieldDefinition("Region", "Region (US/EU)", "text", false)
        },
        [Postmark] = new[]
        {
            new ProviderFieldDefinition("ServerToken", "Server Token", "password", true),
            new ProviderFieldDefinition("FromAddress", "From Address", "email", true),
            new ProviderFieldDefinition("FromName", "From Name", "text", false)
        }
    };

    /// <summary>
    /// Maps each SMS provider identifier to its required and optional configuration fields.
    /// Used by the admin UI to dynamically render provider-specific configuration forms.
    /// </summary>
    public static readonly Dictionary<string, ProviderFieldDefinition[]> SmsProviderFields = new()
    {
        [Twilio] = new[]
        {
            new ProviderFieldDefinition("AccountSid", "Account SID", "text", true),
            new ProviderFieldDefinition("AuthToken", "Auth Token", "password", true),
            new ProviderFieldDefinition("FromNumber", "From Phone Number", "text", true),
            new ProviderFieldDefinition("StatusCallbackUrl", "Status Callback URL", "text", false)
        },
        [BrevoSms] = new[]
        {
            new ProviderFieldDefinition("ApiKey", "API Key", "password", true),
            new ProviderFieldDefinition("SenderName", "Sender Name", "text", true)
        },
        [Vonage] = new[]
        {
            new ProviderFieldDefinition("ApiKey", "API Key", "text", true),
            new ProviderFieldDefinition("ApiSecret", "API Secret", "password", true),
            new ProviderFieldDefinition("FromNumber", "From Number/Name", "text", true)
        },
        [AmazonSns] = new[]
        {
            new ProviderFieldDefinition("AccessKeyId", "Access Key ID", "text", true),
            new ProviderFieldDefinition("SecretAccessKey", "Secret Access Key", "password", true),
            new ProviderFieldDefinition("Region", "AWS Region", "text", true),
            new ProviderFieldDefinition("SenderId", "Sender ID", "text", false)
        },
        [MessageBird] = new[]
        {
            new ProviderFieldDefinition("AccessKey", "Access Key", "password", true),
            new ProviderFieldDefinition("Originator", "Originator", "text", true)
        },
        [Plivo] = new[]
        {
            new ProviderFieldDefinition("AuthId", "Auth ID", "text", true),
            new ProviderFieldDefinition("AuthToken", "Auth Token", "password", true),
            new ProviderFieldDefinition("FromNumber", "From Number", "text", true)
        }
    };
}

/// <summary>
/// Describes a single configuration field for a notification or external auth provider.
/// Used to dynamically build admin UI forms for provider-specific settings.
/// </summary>
public class ProviderFieldDefinition
{
    /// <summary>
    /// Gets or sets the configuration key name (e.g., "ApiKey", "SmtpServer").
    /// Used as the dictionary key when storing the provider configuration.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Gets or sets the human-readable label displayed in the admin UI form.
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// Gets or sets the HTML input type for the field (e.g., "text", "password", "number", "email", "boolean", "textarea").
    /// </summary>
    public string InputType { get; set; }

    /// <summary>
    /// Gets or sets whether this field is required for the provider to function correctly.
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// Initializes a new <see cref="ProviderFieldDefinition"/> with the specified key, label, input type, and required flag.
    /// </summary>
    /// <param name="key">The configuration key name.</param>
    /// <param name="label">The human-readable label for the admin UI.</param>
    /// <param name="inputType">The HTML input type (e.g., "text", "password").</param>
    /// <param name="required">Whether the field is required.</param>
    public ProviderFieldDefinition(string key, string label, string inputType, bool required)
    {
        Key = key;
        Label = label;
        InputType = inputType;
        Required = required;
    }
}
