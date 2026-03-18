/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using MailKit.Net.Smtp;

namespace HCL.CS.SF.Infrastructure.Services.Wrapper;

/// <summary>
/// Thin wrapper around MailKit's <see cref="SmtpClient"/> to enable testability
/// and allow event-based tracking of sent messages in the email service.
/// </summary>
internal class SmtpClientWrapper : SmtpClient
{
}
