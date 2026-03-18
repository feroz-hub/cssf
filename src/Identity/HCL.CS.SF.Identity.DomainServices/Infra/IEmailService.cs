/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Models.Api;

namespace HCL.CS.SF.DomainServices.Infra;

/// <summary>
/// High-level email sending service used by domain services for transactional emails
/// (e.g., email confirmation, password reset, two-factor codes). Implementations delegate
/// to the configured <see cref="IEmailProvider"/> and handle template rendering.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email notification based on the provided notification information.
    /// </summary>
    /// <param name="message">The notification details including recipient, template, and parameters.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure of the send operation.</returns>
    Task<FrameworkResult> SendEmailAsync(NotificationInfoModel message);
}
