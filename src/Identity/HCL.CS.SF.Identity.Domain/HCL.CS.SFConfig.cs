/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Configurations.Api;
using HCL.CS.SF.Domain.Configurations.Endpoint;

namespace HCL.CS.SF.Domain;

public class HCLCSSFConfig
{
    public SystemSettings SystemSettings { get; set; } = new();

    public NotificationTemplateSettings NotificationTemplateSettings { get; set; } = new();

    public TokenSettings TokenSettings { get; set; } = new();
}

public class SystemSettings
{
    public DBConfig DBConfig { get; set; } = new();

    public LoginConfig LoginConfig { get; set; } = new();

    public UserConfig UserConfig { get; set; } = new();

    public PasswordConfig PasswordConfig { get; set; } = new();

    public EmailConfig EmailConfig { get; set; } = new();

    public SMSConfig SMSConfig { get; set; } = new();

    public LdapConfig LdapConfig { get; set; } = new();

    public CryptoConfig CryptoConfig { get; set; } = new();

    public LogConfig LogConfig { get; set; } = new();
}

public class NotificationTemplateSettings
{
    public List<EmailTemplate> EmailTemplateCollection { get; set; } = new();

    public List<SMSTemplate> SMSTemplateCollection { get; set; } = new();
}

public class TokenSettings
{
    public TokenConfig TokenConfig { get; set; } = new();

    public AuthenticationConfig AuthenticationConfig { get; set; } = new();

    public InputLengthRestrictionsConfig InputLengthRestrictionsConfig { get; set; } = new();

    public UserInteractionConfig UserInteractionConfig { get; set; } = new();

    public EndpointsConfig EndpointsConfig { get; set; } = new();

    public TokenExpiration TokenExpiration { get; set; } = new();
}
