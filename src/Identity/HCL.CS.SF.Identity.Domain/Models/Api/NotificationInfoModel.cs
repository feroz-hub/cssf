/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Api;

/// <summary>
/// Lightweight notification request model used to trigger email or SMS notifications
/// for identity-related activities (e.g., verification, password reset).
/// </summary>
public class NotificationInfoModel
{
    /// <summary>The unique identifier of the user the notification is for.</summary>
    public Guid UserId { get; set; }

    /// <summary>The recipient's email address or phone number.</summary>
    public string ToAddress { get; set; }

    /// <summary>The identity activity that triggered this notification (e.g., "EmailVerification").</summary>
    public string Activity { get; set; }

    /// <summary>The name of the notification template to use.</summary>
    public string TemplateName { get; set; }

    /// <summary>Template parameter substitutions (e.g., verification code, user name).</summary>
    public Dictionary<string, string> Parameters { get; set; }
}
