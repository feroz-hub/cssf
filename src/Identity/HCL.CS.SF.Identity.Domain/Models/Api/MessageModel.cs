/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Api;

/// <summary>
/// Represents an email notification message used for identity-related communications
/// such as email verification, password reset, and two-factor authentication codes.
/// </summary>
public class EmailMessageModel
{
    /// <summary>The unique identifier of the user this email is being sent to.</summary>
    public Guid UserId { get; set; }

    /// <summary>The sender's email address.</summary>
    public string FromAddress { get; set; }

    /// <summary>The sender's display name.</summary>
    public string FromName { get; set; }

    /// <summary>The recipient's email address.</summary>
    public string ToAddress { get; set; }

    /// <summary>The recipient's display name.</summary>
    public string ToName { get; set; }

    /// <summary>The email subject line.</summary>
    public string Subject { get; set; }

    /// <summary>The email body content (HTML or plain text).</summary>
    public string Content { get; set; }

    /// <summary>The identity activity that triggered this email (e.g., "PasswordReset", "EmailVerification").</summary>
    public string Activity { get; set; }

    /// <summary>The name of the email template to use for rendering the message body.</summary>
    public string TemplateName { get; set; }

    /// <summary>Carbon copy (CC) recipient email addresses.</summary>
    public string CC { get; set; }

    /// <summary>Template parameter substitutions (e.g., verification code, user name, reset link).</summary>
    public Dictionary<string, string> Parameters { get; set; }
}

/// <summary>
/// Represents an SMS notification message used for identity-related communications
/// such as two-factor authentication codes and phone number verification.
/// </summary>
public class SMSMessage
{
    /// <summary>The unique identifier of the user this SMS is being sent to.</summary>
    public Guid UserId { get; set; }

    /// <summary>The recipient's phone number.</summary>
    public string To { get; set; }

    /// <summary>The SMS message body content.</summary>
    public string Content { get; set; }

    /// <summary>The identity activity that triggered this SMS (e.g., "TwoFactorSMS", "PhoneVerification").</summary>
    public string Activity { get; set; }

    /// <summary>The name of the SMS template to use for rendering the message.</summary>
    public string TemplateName { get; set; }

    /// <summary>Template parameter substitutions (e.g., verification code).</summary>
    public Dictionary<string, string> Parameters { get; set; }
}
