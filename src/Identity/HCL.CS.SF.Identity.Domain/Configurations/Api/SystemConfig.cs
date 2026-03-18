/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Enums;

namespace HCL.CS.SF.Domain.Configurations.Api;

/// <summary>
/// Database connection configuration. Specifies the database provider type
/// and connection string used by the identity framework's persistence layer.
/// Bound from the "Database" section of the system configuration.
/// </summary>
public class DBConfig
{
    /// <summary>
    /// Gets or sets the database provider type (SqlServer, MySql, PostgreSQL, or SQLite).
    /// </summary>
    public DbTypes Database { get; set; }

    /// <summary>
    /// Gets or sets the database connection string used by Entity Framework Core.
    /// </summary>
    public string DBConnectionString { get; set; }
}

/// <summary>
/// Configuration settings for user login behavior, including persistence and lockout policies.
/// Bound from the "Login" section of the system configuration.
/// </summary>
public class LoginConfig
{
    /// <summary>
    /// Gets or sets whether the authentication cookie persists across browser sessions.
    /// When false, the cookie is session-only and is deleted when the browser closes.
    /// Defaults to false.
    /// </summary>
    public bool IsPersistent { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the account is locked out after exceeding the maximum number of failed login attempts.
    /// Defaults to true.
    /// </summary>
    public bool LockoutOnFailure { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the two-factor authentication client (browser/device) is remembered,
    /// so that 2FA is not required on subsequent logins from the same client.
    /// Defaults to false.
    /// </summary>
    public bool RememberClient { get; set; } = false;
}

/// <summary>
/// Configuration settings for user account validation rules, including field length constraints,
/// token expiry times, lockout policies, and two-factor authentication defaults.
/// Bound from the "User" section of the system configuration.
/// </summary>
public class UserConfig
{
    /// <summary>
    /// Gets or sets the minimum allowed username length. Defaults to 6 characters.
    /// </summary>
    public int MinUserNameLength { get; set; } = 6;

    /// <summary>
    /// Gets or sets the maximum allowed username length. Defaults to 255 characters.
    /// </summary>
    public int MaxUserNameLength { get; set; } = 255;

    /// <summary>
    /// Gets or sets the minimum allowed length for first and last name fields. Defaults to 2 characters.
    /// </summary>
    public int MinFirstAndLastNameLength { get; set; } = 2;

    /// <summary>
    /// Gets or sets the maximum allowed length for first and last name fields. Defaults to 255 characters.
    /// </summary>
    public int MaxFirstAndLastNameLength { get; set; } = 255;

    /// <summary>
    /// Gets or sets the minimum allowed phone number length. Defaults to 4 digits.
    /// </summary>
    public int MinPhoneNumberLength { get; set; } = 4;

    /// <summary>
    /// Gets or sets the maximum allowed phone number length. Defaults to 15 digits (E.164 standard).
    /// </summary>
    public int MaxPhoneNumberLength { get; set; } = 15;

    /// <summary>
    /// Gets or sets the minimum number of security questions a user must answer. Defaults to 3.
    /// </summary>
    public int MinNoOfQuestions { get; set; } = 3;

    /// <summary>
    /// Gets or sets the minimum length for security question answers. Defaults to 3 characters.
    /// </summary>
    public int MinSecurityAnswersLength { get; set; } = 3;

    /// <summary>
    /// Gets or sets the minimum age (in years) allowed for user registration based on date of birth. Defaults to 18.
    /// </summary>
    public int MinDOBYear { get; set; } = 18;

    /// <summary>
    /// Gets or sets the maximum age (in years) allowed for user registration based on date of birth. Defaults to 100.
    /// </summary>
    public int MaxDOBYear { get; set; } = 100;

    /// <summary>
    /// Gets or sets the number of failed login attempts before the account is locked. Defaults to 3.
    /// </summary>
    public int AccessFailedCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets the number of recovery codes generated for two-factor authentication backup. Defaults to 5.
    /// </summary>
    public int RequiredRecoveryCodes { get; set; } = 5;

    /// <summary>
    /// Gets or sets whether each user must have a unique email address. Defaults to true.
    /// </summary>
    public bool RequireUniqueEmail { get; set; } = true;

    /// <summary>
    /// Gets or sets whether email confirmation is required before a user can sign in. Defaults to true.
    /// </summary>
    public bool RequireConfirmedEmail { get; set; } = true;

    /// <summary>
    /// Gets or sets whether phone number confirmation is required. Defaults to true.
    /// </summary>
    public bool RequireConfirmedPhoneNumber { get; set; } = true;

    /// <summary>
    /// Gets or sets whether account lockout is enabled for newly created users. Defaults to true.
    /// </summary>
    public bool LockOutAllowedForNewUsers { get; set; } = true;

    /// <summary>
    /// Gets or sets the default lockout duration in minutes after exceeding failed login attempts. Defaults to 10 minutes.
    /// </summary>
    public int DefaultLockoutTimeSpanMin { get; set; } = 10;

    /// <summary>
    /// Gets or sets the maximum number of invalid credential attempts before lockout. Defaults to 3.
    /// </summary>
    public int MaxRetryInvalidCredentials { get; set; } = 3;

    /// <summary>
    /// Gets or sets the email verification token expiry time in minutes. Defaults to 720 minutes (12 hours).
    /// </summary>
    public int EmailTokenExpiry { get; set; } = 720;

    /// <summary>
    /// Gets or sets the one-time password (OTP) token expiry time in minutes. Defaults to 10 minutes.
    /// </summary>
    public int OTPTokenExpiry { get; set; } = 10;

    /// <summary>
    /// Gets or sets the password reset token expiry time in minutes. Defaults to 10 minutes.
    /// </summary>
    public int PasswordResetTokenExpiry { get; set; } = 10;

    /// <summary>
    /// Gets or sets the general-purpose user token expiry time in minutes. Defaults to 10 minutes.
    /// </summary>
    public int UserTokenExpiry { get; set; } = 10;

    /// <summary>
    /// Gets or sets the number of days an account remains locked before automatic unlock. Defaults to 45 days.
    /// </summary>
    public int LockAccountPeriod { get; set; } = 45;

    /// <summary>
    /// Gets or sets the default role name assigned to newly registered users. Defaults to "HCLCSSFUser".
    /// </summary>
    public string DefaultUserRole { get; set; } = "HCLCSSFUser";

    /// <summary>
    /// Gets or sets the maximum number of failed access attempts before the account is locked out.
    /// Used by ASP.NET Core Identity's lockout mechanism. Defaults to 5.
    /// </summary>
    public int MaxFailedAccessAttempts { get; set; } = 5;
}

/// <summary>
/// Configuration settings for password complexity and lifecycle policies.
/// Enforced during user registration, password change, and password reset operations.
/// Bound from the "Password" section of the system configuration.
/// </summary>
public class PasswordConfig
{
    /// <summary>
    /// Gets or sets the minimum required password length. Defaults to 8 characters.
    /// </summary>
    public int MinPasswordLength { get; set; } = 8;

    /// <summary>
    /// Gets or sets the maximum allowed password length. Defaults to 64 characters.
    /// </summary>
    public int MaxPasswordLength { get; set; } = 64;

    /// <summary>
    /// Gets or sets the minimum number of unique characters required in the password. Defaults to 8.
    /// </summary>
    public int RequiredUniqueChars { get; set; } = 8;

    /// <summary>
    /// Gets or sets whether the password must contain at least one digit (0-9). Defaults to true.
    /// </summary>
    public bool RequireDigit { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the password must contain at least one lowercase letter (a-z). Defaults to true.
    /// </summary>
    public bool RequireLowercase { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the password must contain at least one uppercase letter (A-Z). Defaults to true.
    /// </summary>
    public bool RequireUppercase { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the password must contain at least one special character. Defaults to true.
    /// </summary>
    public bool RequireSpecialChar { get; set; } = true;

    /// <summary>
    /// Gets or sets an optional regular expression pattern for custom password validation.
    /// When set, the password must match this pattern in addition to the other rules.
    /// </summary>
    public string PasswordPattern { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of previous passwords stored for reuse prevention.
    /// Users cannot set a password that matches any of the last N passwords. Defaults to 10.
    /// </summary>
    public int MaxLimitPasswordReuse { get; set; } = 10;

    /// <summary>
    /// Gets or sets the number of days before a password expires and must be changed. Defaults to 42 days.
    /// </summary>
    public int MaxPasswordExpiry { get; set; } = 42;

    /// <summary>
    /// Gets or sets the number of days before password expiration when the user receives a notification warning.
    /// Defaults to 3 days.
    /// </summary>
    public int PasswordNotificationBeforeExpiry { get; set; } = 3;
}

/// <summary>
/// SMTP email delivery configuration. Specifies the mail server connection details
/// and the notification format used for sending emails.
/// Bound from the "Email" section of the system configuration.
/// </summary>
public class EmailConfig
{
    /// <summary>
    /// Gets or sets the SMTP server hostname or IP address.
    /// </summary>
    public string SmtpServer { get; set; }

    /// <summary>
    /// Gets or sets the SMTP server port number (e.g., 25, 465, 587).
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets the SMTP authentication username.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Gets or sets the SMTP authentication password.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Gets or sets the email notification type (Token or Link) that determines how verification
    /// information is presented in emails. Defaults to Link.
    /// </summary>
    public EmailNotificationType EmailNotificationType { get; set; } = EmailNotificationType.Link;

    /// <summary>
    /// Gets or sets whether the SMTP connection uses SSL/TLS encryption.
    /// </summary>
    public bool SecureSocketOptions { get; set; }
}

/// <summary>
/// Defines an email template used for transactional notifications such as verification emails,
/// password resets, and two-factor authentication codes.
/// </summary>
public class EmailTemplate
{
    /// <summary>
    /// Gets or sets the unique template name used to look up this template (e.g., "EmailVerification").
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the email subject line.
    /// </summary>
    public string Subject { get; set; }

    /// <summary>
    /// Gets or sets the sender email address (From address).
    /// </summary>
    public string FromAddress { get; set; }

    /// <summary>
    /// Gets or sets the sender display name (From name).
    /// </summary>
    public string FromName { get; set; }

    /// <summary>
    /// Gets or sets the CC (carbon copy) recipients as a comma-separated email list.
    /// </summary>
    public string CC { get; set; }

    /// <summary>
    /// Gets or sets the email body template with placeholder tokens (e.g., {UserName}, {Token}).
    /// </summary>
    public string TemplateFormat { get; set; }
}

/// <summary>
/// SMS delivery configuration. Specifies the credentials and settings for the SMS provider
/// (e.g., Twilio) used to send OTP codes and verification messages.
/// Bound from the "SMS" section of the system configuration.
/// </summary>
public class SMSConfig
{
    /// <summary>
    /// Gets or sets the SMS provider account identifier (e.g., Twilio Account SID).
    /// </summary>
    public string SMSAccountIdentification { get; set; }

    /// <summary>
    /// Gets or sets the SMS provider account password or auth token (e.g., Twilio Auth Token).
    /// </summary>
    public string SMSAccountPassword { get; set; }

    /// <summary>
    /// Gets or sets the sender phone number or alphanumeric sender ID used as the "From" field.
    /// </summary>
    public string SMSAccountFrom { get; set; }

    /// <summary>
    /// Gets or sets the webhook URL called by the SMS provider to report delivery status updates.
    /// </summary>
    public string SMSStatusCallbackURL { get; set; }
}

/// <summary>
/// Defines an SMS template used for transactional notifications such as OTP codes
/// and phone number verification messages.
/// </summary>
public class SMSTemplate
{
    /// <summary>
    /// Gets or sets the unique template name used to look up this template (e.g., "PhoneVerification").
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the SMS body template with placeholder tokens (e.g., {Token}, {UserName}).
    /// </summary>
    public string TemplateFormat { get; set; }
}

/// <summary>
/// LDAP (Lightweight Directory Access Protocol) configuration for authenticating users
/// against an external directory service such as Active Directory.
/// Bound from the "LDAP" section of the system configuration.
/// </summary>
public class LdapConfig
{
    /// <summary>
    /// Gets or sets the LDAP server hostname or IP address.
    /// </summary>
    public string LdapHostName { get; set; }

    /// <summary>
    /// Gets or sets the LDAP domain name used for binding (e.g., "corp.example.com").
    /// </summary>
    public string LdapDomainName { get; set; }

    /// <summary>
    /// Gets or sets the LDAP server port number (typically 389 for LDAP or 636 for LDAPS).
    /// </summary>
    public int LdapPort { get; set; }

    /// <summary>
    /// Gets or sets whether the LDAP connection uses SSL/TLS (LDAPS). Defaults to false.
    /// </summary>
    public bool IsSecureConnection { get; set; }

    /// <summary>
    /// Gets or sets whether two-factor authentication is required after successful LDAP authentication.
    /// Defaults to false.
    /// </summary>
    public bool IsTwoFactorAuthenticationRequired { get; set; } = false;

    /// <summary>
    /// Gets or sets the two-factor authentication type used for LDAP users (None, Email, SMS, or AuthenticatorApp).
    /// Defaults to None.
    /// </summary>
    public virtual TwoFactorType TwoFactorType { get; set; } = TwoFactorType.None;
}

/// <summary>
/// Cryptographic utility configuration. Controls parameters for random string generation
/// used in token and secret creation.
/// Bound from the "Crypto" section of the system configuration.
/// </summary>
public class CryptoConfig
{
    /// <summary>
    /// Gets or sets the length (in characters) of cryptographically random strings generated
    /// for tokens, nonces, and other security-sensitive values. Defaults to 32.
    /// </summary>
    public int RandomStringLength { get; set; } = 32;
}
