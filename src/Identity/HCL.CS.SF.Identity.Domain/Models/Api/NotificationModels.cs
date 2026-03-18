/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Configurations.Api;
using HCL.CS.SF.Domain.Constants;

namespace HCL.CS.SF.Domain.Models.Api;

/// <summary>
/// Request model for searching and filtering notification log entries.
/// Supports filtering by notification type, delivery status, date range, and free-text search.
/// </summary>
public class NotificationSearchRequestModel
{
    /// <summary>Filter by notification type (e.g., 1 = Email, 2 = SMS).</summary>
    public int? Type { get; set; }

    /// <summary>Filter by delivery status (e.g., 0 = Pending, 1 = Sent, 2 = Failed).</summary>
    public int? Status { get; set; }

    /// <summary>The start of the date range filter as a string.</summary>
    public string FromDate { get; set; }

    /// <summary>The end of the date range filter as a string.</summary>
    public string ToDate { get; set; }

    /// <summary>Free-text search value applied across notification fields.</summary>
    public string SearchValue { get; set; }

    /// <summary>Pagination parameters for the search results.</summary>
    public PagingModel Page { get; set; }
}

/// <summary>
/// Represents a single notification log entry recording a sent or attempted notification.
/// </summary>
public class NotificationLogModel
{
    /// <summary>The unique identifier of the notification log entry.</summary>
    public Guid Id { get; set; }

    /// <summary>The user identifier the notification was sent to.</summary>
    public Guid UserId { get; set; }

    /// <summary>The external message identifier from the notification provider.</summary>
    public string MessageId { get; set; }

    /// <summary>The notification type (e.g., 1 = Email, 2 = SMS).</summary>
    public int Type { get; set; }

    /// <summary>The identity activity that triggered this notification.</summary>
    public string Activity { get; set; }

    /// <summary>The delivery status of the notification (e.g., 0 = Pending, 1 = Sent, 2 = Failed).</summary>
    public int Status { get; set; }

    /// <summary>The sender address (email or phone number).</summary>
    public string Sender { get; set; }

    /// <summary>The recipient address (email or phone number).</summary>
    public string Recipient { get; set; }

    /// <summary>The UTC timestamp when the notification was created.</summary>
    public DateTime CreatedOn { get; set; }
}

/// <summary>
/// Paginated response model for notification log queries.
/// </summary>
public class NotificationLogResponseModel
{
    /// <summary>The list of notification log entries for the current page.</summary>
    public List<NotificationLogModel> Notifications { get; set; }

    /// <summary>Pagination metadata.</summary>
    public PagingModel PageInfo { get; set; }
}

/// <summary>
/// Response model returning the configured email and SMS notification templates.
/// </summary>
public class NotificationTemplateResponseModel
{
    /// <summary>The list of configured email templates.</summary>
    public List<EmailTemplate> EmailTemplates { get; set; }

    /// <summary>The list of configured SMS templates.</summary>
    public List<SMSTemplate> SmsTemplates { get; set; }
}

/// <summary>
/// Represents a notification provider configuration (e.g., SMTP server, SMS gateway).
/// </summary>
public class ProviderConfigModel
{
    /// <summary>The unique identifier of the provider configuration. Null for new configurations.</summary>
    public Guid? Id { get; set; }

    /// <summary>The name of the notification provider (e.g., "SendGrid", "Twilio").</summary>
    public string ProviderName { get; set; }

    /// <summary>The channel type (e.g., 1 = Email, 2 = SMS).</summary>
    public int ChannelType { get; set; }

    /// <summary>Indicates whether this provider is currently active and used for sending notifications.</summary>
    public bool IsActive { get; set; }

    /// <summary>Provider-specific settings (e.g., API key, host, port, credentials).</summary>
    public Dictionary<string, string> Settings { get; set; }

    /// <summary>The UTC timestamp when this provider was last tested.</summary>
    public DateTime? LastTestedOn { get; set; }

    /// <summary>Indicates whether the last connectivity test succeeded.</summary>
    public bool? LastTestSuccess { get; set; }
}

/// <summary>
/// Request model for creating or updating a notification provider configuration.
/// </summary>
public class SaveProviderConfigRequest
{
    /// <summary>The unique identifier. Null for new configurations, set for updates.</summary>
    public Guid? Id { get; set; }

    /// <summary>The provider name.</summary>
    public string ProviderName { get; set; }

    /// <summary>The channel type (e.g., 1 = Email, 2 = SMS).</summary>
    public int ChannelType { get; set; }

    /// <summary>Whether to activate this provider.</summary>
    public bool IsActive { get; set; }

    /// <summary>Provider-specific settings.</summary>
    public Dictionary<string, string> Settings { get; set; }
}

/// <summary>
/// Request model for setting a notification provider as the active provider for its channel.
/// </summary>
public class SetActiveProviderRequest
{
    /// <summary>The unique identifier of the provider configuration to activate.</summary>
    public Guid Id { get; set; }
}

/// <summary>
/// Request model for deleting a notification provider configuration.
/// </summary>
public class DeleteProviderConfigRequest
{
    /// <summary>The unique identifier of the provider configuration to delete.</summary>
    public Guid Id { get; set; }
}

/// <summary>
/// Response model returning the available notification provider types and their
/// required configuration field definitions for the admin UI.
/// </summary>
public class ProviderFieldDefinitionsResponse
{
    /// <summary>Email provider types mapped to their required configuration field definitions.</summary>
    public Dictionary<string, ProviderFieldDefinition[]> EmailProviders { get; set; }

    /// <summary>SMS provider types mapped to their required configuration field definitions.</summary>
    public Dictionary<string, ProviderFieldDefinition[]> SmsProviders { get; set; }
}

/// <summary>
/// Request model for sending a test notification to verify provider configuration.
/// </summary>
public class SendTestNotificationRequest
{
    /// <summary>The notification type to test (e.g., 1 = Email, 2 = SMS).</summary>
    public int Type { get; set; }

    /// <summary>The recipient address for the test notification.</summary>
    public string Recipient { get; set; }

    /// <summary>The provider configuration to use for the test. Null to use the active provider.</summary>
    public Guid? ProviderConfigId { get; set; }

    /// <summary>Optional user identifier for template personalization.</summary>
    public Guid? UserId { get; set; }
}
