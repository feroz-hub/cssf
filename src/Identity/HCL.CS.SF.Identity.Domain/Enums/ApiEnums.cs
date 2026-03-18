/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Enums;

/// <summary>
/// Defines the available query filter options for retrieving audit trail records.
/// These options control which criteria are used to filter audit log entries.
/// </summary>
public enum QueryOption
{
    /// <summary>
    /// Filter audit records by a specific date only.
    /// </summary>
    Date = 1,

    /// <summary>
    /// Filter audit records by the actor (ChangedBy) and a specific date.
    /// </summary>
    ChangeByAndDate = 2,

    /// <summary>
    /// Filter audit records by the actor (ChangedBy) within a date range (from/to).
    /// </summary>
    ChangeByAndBetweenDates = 3,

    /// <summary>
    /// Filter audit records by the actor (ChangedBy), action type (e.g., Create/Update/Delete), and a date range.
    /// </summary>
    ChangeBywithActionAndBetweenDates = 4,

    /// <summary>
    /// No filter applied; returns all available audit records.
    /// </summary>
    None = 5
}

/// <summary>
/// Represents the delivery status of a notification (email or SMS).
/// Tracks the lifecycle of a notification from initiation through delivery or failure.
/// </summary>
public enum NotificationStatus
{
    /// <summary>
    /// The notification has been created and initiated but not yet sent.
    /// </summary>
    Initiated = 1,

    /// <summary>
    /// The notification was successfully delivered to the recipient.
    /// </summary>
    Delivered = 2,

    /// <summary>
    /// The notification delivery failed permanently.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// The notification delivery is delayed by the provider.
    /// </summary>
    Delayed = 4,

    /// <summary>
    /// The notification was relayed through an intermediate provider or gateway.
    /// </summary>
    Relayed = 5,

    /// <summary>
    /// The notification was expanded (e.g., a group message was split into individual messages).
    /// </summary>
    Expanded = 6,

    /// <summary>
    /// The notification is queued at the provider and waiting to be processed.
    /// </summary>
    queued = 7,

    /// <summary>
    /// The notification is currently being sent by the provider.
    /// </summary>
    Sending = 8,

    /// <summary>
    /// The notification has been sent by the provider but delivery is not yet confirmed.
    /// </summary>
    Sent = 9,

    /// <summary>
    /// The notification could not be delivered to the recipient (e.g., invalid phone number or mailbox full).
    /// </summary>
    Undelivered = 10,

    /// <summary>
    /// The notification is being received by the provider (inbound message scenario).
    /// </summary>
    Receiving = 11,

    /// <summary>
    /// The notification was received by the provider (inbound message scenario).
    /// </summary>
    Received = 12,

    /// <summary>
    /// The notification was accepted by the provider for processing.
    /// </summary>
    Accepted = 13,

    /// <summary>
    /// The notification has been scheduled for future delivery.
    /// </summary>
    Scheduled = 14,

    /// <summary>
    /// The notification has been read/opened by the recipient (when read-tracking is available).
    /// </summary>
    Read = 15,

    /// <summary>
    /// The notification was only partially delivered (e.g., some recipients received it while others did not).
    /// </summary>
    Partially = 16
}

/// <summary>
/// Defines the supported notification channel types used for delivering messages to users
/// (e.g., verification codes, two-factor authentication tokens, password reset links).
/// </summary>
public enum NotificationTypes
{
    /// <summary>
    /// Send notifications via email (SMTP, SendGrid, or other configured email provider).
    /// </summary>
    Email = 1,

    /// <summary>
    /// Send notifications via SMS (Twilio, Vonage, or other configured SMS provider).
    /// </summary>
    SMS = 2
}

/// <summary>
/// Defines the format used for email-based notifications such as account verification
/// or password reset messages.
/// </summary>
public enum EmailNotificationType
{
    /// <summary>
    /// The email contains a short-lived token (e.g., OTP) that the user must enter manually.
    /// </summary>
    Token = 1,

    /// <summary>
    /// The email contains a clickable verification or action link with an embedded token.
    /// </summary>
    Link = 2
}

/// <summary>
/// Identifies the identity provider used to authenticate a user.
/// Determines where user credentials are validated during sign-in.
/// </summary>
public enum IdentityProvider
{
    /// <summary>
    /// The user is authenticated against the local identity store (ASP.NET Core Identity database).
    /// </summary>
    Local = 1,

    /// <summary>
    /// The user is authenticated against an external LDAP/Active Directory server.
    /// </summary>
    Ldap = 2,

    /// <summary>
    /// The user is authenticated via Google OpenID Connect (external OAuth/OIDC provider).
    /// </summary>
    Google = 3
}

/// <summary>
/// Defines the type of audit trail operation recorded when a tracked entity is modified.
/// Used by the change-tracking infrastructure to categorize database mutations.
/// </summary>
public enum AuditType
{
    /// <summary>
    /// No audit action; the entity was not modified.
    /// </summary>
    None = 0,

    /// <summary>
    /// A new entity record was created (INSERT operation).
    /// </summary>
    Create = 1,

    /// <summary>
    /// An existing entity record was modified (UPDATE operation).
    /// </summary>
    Update = 2,

    /// <summary>
    /// An existing entity record was removed (DELETE operation).
    /// </summary>
    Delete = 3
}

/// <summary>
/// Defines the supported two-factor authentication (2FA) methods available for user accounts.
/// Used during sign-in when two-factor authentication is enabled for a user.
/// </summary>
public enum TwoFactorType
{
    /// <summary>
    /// Two-factor authentication is not enabled for the user.
    /// </summary>
    None = 0,

    /// <summary>
    /// Two-factor code is delivered via email to the user's confirmed email address.
    /// </summary>
    Email = 1,

    /// <summary>
    /// Two-factor code is delivered via SMS to the user's confirmed phone number.
    /// </summary>
    Sms = 2,

    /// <summary>
    /// Two-factor code is generated by a TOTP authenticator application (e.g., Google Authenticator, Microsoft Authenticator).
    /// </summary>
    AuthenticatorApp = 3
}

/// <summary>
/// Defines the filter options for querying persisted security tokens (access tokens, refresh tokens).
/// Used by the token revocation and introspection administration features.
/// </summary>
public enum SecurityTokenOption
{
    /// <summary>
    /// Filter security tokens by the OAuth client (client_id) that requested them.
    /// </summary>
    Client = 1,

    /// <summary>
    /// Filter security tokens by the user (subject) to whom they were issued.
    /// </summary>
    User = 2,

    /// <summary>
    /// Filter security tokens issued within a specified date range.
    /// </summary>
    BetweenDates = 3,

    /// <summary>
    /// Return all security tokens without any filter applied.
    /// </summary>
    All = 4,

    /// <summary>
    /// No filter specified; no tokens are returned.
    /// </summary>
    None = 5
}
