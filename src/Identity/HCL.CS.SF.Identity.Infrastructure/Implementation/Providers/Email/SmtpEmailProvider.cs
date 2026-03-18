/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.DomainServices.Infra;

namespace HCL.CS.SF.Infrastructure.Services.Implementation.Providers.Email;

/// <summary>
/// Email provider implementation that sends emails via a standard SMTP server using MailKit.
/// Requires <c>SmtpServer</c>, <c>Port</c>, <c>UserName</c>, and <c>Password</c> in the configuration dictionary.
/// Optionally uses <c>UseSsl</c> to control the TLS mode (SSL-on-connect vs. STARTTLS).
/// </summary>
public class SmtpEmailProvider : IEmailProvider
{
    /// <summary>
    /// Gets the unique provider name used for factory-based resolution.
    /// </summary>
    public string ProviderName => NotificationProviderConstants.Smtp;

    /// <summary>
    /// Sends an email message by connecting to an SMTP server, authenticating, and transmitting the MIME message.
    /// </summary>
    /// <param name="message">The email message containing recipient, subject, HTML body, and optional CC.</param>
    /// <param name="config">Provider configuration including SmtpServer, Port, UserName, Password, and optional UseSsl/FromAddress/FromName.</param>
    /// <returns>A <see cref="ProviderSendResult"/> indicating success or failure with the MIME message ID.</returns>
    public async Task<ProviderSendResult> SendAsync(EmailMessage message, Dictionary<string, string> config)
    {
        try
        {
            // Resolve sender address: message-level override takes priority over config
            var fromAddress = !string.IsNullOrWhiteSpace(message.From)
                ? message.From
                : config.GetValueOrDefault("FromAddress", string.Empty);

            var fromName = !string.IsNullOrWhiteSpace(message.FromName)
                ? message.FromName
                : config.GetValueOrDefault("FromName", string.Empty);

            // Build the MIME message with sender, recipient, subject, and HTML body
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(fromName, fromAddress));
            mimeMessage.To.Add(MailboxAddress.Parse(message.To));
            mimeMessage.Subject = message.Subject;

            if (!string.IsNullOrWhiteSpace(message.CC))
            {
                mimeMessage.Cc.Add(MailboxAddress.Parse(message.CC));
            }

            var bodyBuilder = new BodyBuilder { HtmlBody = message.HtmlBody };
            mimeMessage.Body = bodyBuilder.ToMessageBody();

            var server = config["SmtpServer"];
            var port = int.Parse(config["Port"]);
            var userName = config["UserName"];
            var password = config["Password"];
            var useSsl = config.TryGetValue("UseSsl", out var sslValue)
                && bool.TryParse(sslValue, out var ssl) && ssl;

            // Choose SSL-on-connect (port 465) or STARTTLS (port 587) based on config
            var secureSocketOptions = useSsl
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls;

            using var client = new SmtpClient();
            await client.ConnectAsync(server, port, secureSocketOptions);
            await client.AuthenticateAsync(userName, password);
            var result = await client.SendAsync(mimeMessage);
            await client.DisconnectAsync(true);

            return new ProviderSendResult
            {
                Success = true,
                MessageId = mimeMessage.MessageId,
                ErrorMessage = string.Empty
            };
        }
        catch (Exception ex)
        {
            return new ProviderSendResult
            {
                Success = false,
                MessageId = string.Empty,
                ErrorMessage = ex.Message
            };
        }
    }
}
