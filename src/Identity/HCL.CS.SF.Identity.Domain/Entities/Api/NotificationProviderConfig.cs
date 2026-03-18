/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Entities.Api;

/// <summary>
/// Stores configuration for a notification delivery provider (e.g., SMTP server, SMS gateway, push service).
/// The identity server uses these configurations to send security-related notifications
/// such as email verification codes, password reset links, and 2FA one-time passwords.
/// </summary>
public class NotificationProviderConfig : BaseEntity
{
    /// <summary>Display name of the notification provider (e.g., "SendGrid", "Twilio").</summary>
    public string ProviderName { get; set; }

    /// <summary>Numeric identifier for the delivery channel (e.g., 1=Email, 2=SMS, 3=Push).</summary>
    public int ChannelType { get; set; }

    /// <summary>Whether this provider is currently active and should be used for sending notifications.</summary>
    public bool IsActive { get; set; }

    /// <summary>JSON-serialized provider configuration (API keys, endpoints, sender addresses, etc.).</summary>
    public string ConfigJson { get; set; }

    /// <summary>UTC timestamp of the last connectivity test against this provider.</summary>
    public DateTime? LastTestedOn { get; set; }

    /// <summary>Result of the last provider connectivity test; null if never tested.</summary>
    public bool? LastTestSuccess { get; set; }
}
