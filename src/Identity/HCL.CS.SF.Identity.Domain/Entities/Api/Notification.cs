/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Enums;

namespace HCL.CS.SF.Domain.Entities.Api;

/// <summary>
/// Represents a notification message sent to a user (e.g., email verification, password reset, 2FA code).
/// Tracks delivery status so the system can retry or audit notification history.
/// </summary>
public class Notification : BaseEntity
{
    /// <summary>Foreign key referencing the target user.</summary>
    public Guid UserId { get; set; }

    /// <summary>Provider-assigned message identifier for delivery tracking and troubleshooting.</summary>
    public string MessageId { get; set; }

    /// <summary>The notification channel type (e.g., Email, SMS).</summary>
    public NotificationTypes Type { get; set; }

    /// <summary>Describes the security activity that triggered this notification (e.g., "PasswordReset", "EmailVerification").</summary>
    public string Activity { get; set; }

    /// <summary>Current delivery status of the notification (e.g., Pending, Sent, Failed).</summary>
    public NotificationStatus Status { get; set; }

    /// <summary>The sender address or identifier (e.g., "noreply@example.com").</summary>
    public string Sender { get; set; }

    /// <summary>The recipient address (email or phone number).</summary>
    public string Recipient { get; set; }

    /// <summary>Navigation property to the target user.</summary>
    public virtual Users User { get; set; }
}
