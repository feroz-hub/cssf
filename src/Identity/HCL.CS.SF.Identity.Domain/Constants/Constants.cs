/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Constants;

/// <summary>
/// General-purpose constants used across the identity framework, including default provider names,
/// table names for audit tracking, database column length limits, and lists of auditable tables and ignored columns.
/// </summary>
public class Constants
{
    /// <summary>
    /// The default token provider name used by ASP.NET Core Identity for generating tokens.
    /// </summary>
    public const string DefaultProvider = "Default";

    /// <summary>
    /// The token provider name used for email-based token generation (e.g., email confirmation tokens).
    /// </summary>
    public const string DefaultEmailProvider = "Email";

    /// <summary>
    /// The token provider name used for phone-based token generation (e.g., SMS verification codes).
    /// </summary>
    public const string DefaultPhoneProvider = "Phone";

    /// <summary>
    /// The token provider name used for TOTP authenticator app setup and verification.
    /// </summary>
    public const string DefaultAuthenticatorProvider = "Authenticator";

    // public const string DefaultDbSchema = "HCL.CS.SF";

    /// <summary>
    /// The audit trail column name used to record which user created a record.
    /// </summary>
    public const string CreatedBy = "CreatedBy";

    /// <summary>
    /// The database table name for the Roles entity, used in audit trail tracking.
    /// </summary>
    public const string RoleTable = "Roles";

    /// <summary>
    /// The database table name for the Users entity, used in audit trail tracking.
    /// </summary>
    public const string UserTable = "Users";

    /// <summary>
    /// Standard database column length limit of 255 characters, used for names, identifiers, and short text fields.
    /// </summary>
    public const int ColumnLength255 = 255;

    /// <summary>
    /// Extended database column length limit of 2048 characters, used for URIs, state parameters, and longer text fields.
    /// </summary>
    public const int ColumnLength2048 = 2048;

    /// <summary>
    /// The list of database table names that are tracked by the audit trail system.
    /// Only changes to entities in these tables generate audit log entries.
    /// </summary>
    public static readonly List<string> AllowedAuditTables = new()
    {
        "Roles",
        "RoleClaims",
        "Users",
        "UserClaims",
        "UserRoles",
        "UserSecurityQuestions"
    };

    /// <summary>
    /// The list of database column names excluded from audit trail change tracking.
    /// These columns are either auto-generated, contain sensitive data (e.g., PasswordHash),
    /// or change too frequently to be useful in audit logs.
    /// </summary>
    public static readonly List<string> IgnoredAuditColumns = new()
    {
        "Id",
        "NormalizedName",
        "ConcurrencyStamp",
        "NormalizedUserName",
        "NormalizedEmail",
        "PasswordHash",
        "SecurityStamp",
        "DateOfBirth",
        "RowVersion"
    };

    // public const int CryptographyDerivedKeyLength = 32;
    // public const int CryptographyScryptCost = 262144;
    // public const int CryptographyScryptBlocksize = 8;
    // public const int CryptographyScryptParallel = 1;
    // public const int CryptographyPasswordBcryptCost = 13;
    // public const string CryptographySaltPreformat = "$2a$";
    // public const string CryptographySaltPreformat2 = "$";

    // public const string DatabaseConnectionStringMissing = "Database connection string not configured";
    // public const string EmailAlreadyInUse = "EmailAlreadyInUse";
    // public const string InvalidCredentials = "InvalidCredentials";
    // public const string AuditErrorDbInsert = "Error during inserting to the database";
    // public const string AuditErrorDbGet = "Error Occured when getting data from database";
    // public const string AuditInsertSuccess = "Audit Trail Successfully Inserted";
    // public const string AuditTableErrorCount = "No Audit Records Found";
    // public const string AuditTableUnknownError = "UnKnown Error. Contact Administrator";
    // public const string AuditTableDateRangeError = "From date should not greater than to Date";
    // public const string AuditNullError = "Value cannot be null.";
}

/// <summary>
/// Constants for internal result dictionary keys used to pass context between
/// the authorization and token endpoint processing pipelines.
/// </summary>
public class ResultCustomConstants
{
    /// <summary>
    /// Dictionary key used to store and retrieve the validated authorization code request object
    /// during the authorization code exchange at the token endpoint.
    /// </summary>
    public const string AuthorizeCodeRequestKey = "AuthorizeCodeRequestKey";

    /// <summary>
    /// Dictionary key used to store and retrieve the validated token request object
    /// during token endpoint processing.
    /// </summary>
    public const string TokenRequestKey = "TokenRequestKey";
}

/// <summary>
/// Constants for encryption key identifiers used by the data protection and token encryption services.
/// </summary>
public class EncryptionKeyConstants
{
    /// <summary>
    /// The default encryption key purpose string used for general-purpose data protection operations.
    /// </summary>
    public const string DefaultEncryptionKey = "CSHCL.CS.SF";

    /// <summary>
    /// The encryption key purpose string used for encrypting/decrypting OAuth request parameters
    /// (e.g., authorization code payloads stored in the data store).
    /// </summary>
    public const string RequestParameterEncryptedKey = "RPSecurityKey";

    /// <summary>
    /// The encryption key purpose string used for encrypting/decrypting verification tokens
    /// (e.g., email confirmation tokens, password reset tokens).
    /// </summary>
    public const string VerificationEncryptedKey = "VerificationSecurityKey";
}

/// <summary>
/// Constants for structured logging category keys used to identify log sources.
/// </summary>
public class LoggerKeyConstants
{
    /// <summary>
    /// The default logger category key for the identity framework's structured logging output.
    /// </summary>
    public const string DefaultLoggerKey = "CSHCL.CS.SF";
}

/// <summary>
/// Constants for notification template names used to look up email and SMS templates
/// when sending transactional messages (verification codes, password resets, 2FA tokens, etc.).
/// </summary>
public class NotificationConstants
{
    /// <summary>
    /// The fallback template name used when no specific template is configured.
    /// </summary>
    public const string DefaultTemplate = "DefaultTemplate";

    /// <summary>
    /// Template name for generic email verification notifications.
    /// </summary>
    public const string EmailVerification = "EmailVerification";

    /// <summary>
    /// Template name for phone number verification notifications.
    /// </summary>
    public const string PhoneNumberVerification = "PhoneVerification";

    /// <summary>
    /// Template name for generating and sending two-factor authentication tokens.
    /// </summary>
    public const string GenerateTwoFactorToken = "GenerateTwoFactorToken";

    /// <summary>
    /// Template name for email verification using a clickable link.
    /// </summary>
    public const string EmailVerificationUsingLink = "EmailVerificationUsingLink";

    /// <summary>
    /// Template name for email verification using a manually entered token/OTP.
    /// </summary>
    public const string EmailVerificationUsingToken = "EmailVerificationUsingToken";

    /// <summary>
    /// Template name for phone number verification using a token/OTP sent via SMS.
    /// </summary>
    public const string PhoneNumberVerificationToken = "PhoneNumberVerificationToken";

    /// <summary>
    /// Template name for password reset notifications containing a reset token.
    /// </summary>
    public const string ResetPasswordUsingToken = "ResetPasswordUsingToken";
}

/// <summary>
/// Constants for permission suffixes used to construct fine-grained API permission claim values.
/// Combined with a resource prefix (e.g., "HCL.CS.SF.user") to form full permission strings.
/// </summary>
public class PermissionConstants
{
    /// <summary>
    /// Read permission suffix. Grants read-only access to the associated resource.
    /// </summary>
    public const string Read = ".read";

    /// <summary>
    /// Write permission suffix. Grants create and update access to the associated resource.
    /// </summary>
    public const string Write = ".write";

    /// <summary>
    /// Delete permission suffix. Grants delete access to the associated resource.
    /// </summary>
    public const string Delete = ".delete";

    /// <summary>
    /// Manage permission suffix. Grants full administrative access (read, write, delete) to the associated resource.
    /// </summary>
    public const string Manage = ".manage";
}
