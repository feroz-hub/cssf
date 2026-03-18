/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Constants;

/// <summary>
/// Constants for API request parameter names used in query strings and request bodies.
/// These standardize the parameter naming convention across all Admin API endpoints,
/// ensuring consistent serialization/deserialization between the API gateway and the identity services.
/// </summary>
public static class ApiRouteParameterConstants
{
    /// <summary>
    /// Parameter name for the user's unique identifier (GUID).
    /// </summary>
    public const string UserId = "user_id";

    /// <summary>
    /// Parameter name for the user's current password (used in password change operations).
    /// </summary>
    public const string CurrentPassword = "current_password";

    /// <summary>
    /// Parameter name for the user's new password (used in password change and reset operations).
    /// </summary>
    public const string NewPassword = "new_password";

    /// <summary>
    /// Parameter name for the password reset token received via email or SMS.
    /// </summary>
    public const string PasswordResetToken = "password_reset_token";

    /// <summary>
    /// Parameter name for a general-purpose user token (e.g., custom verification tokens).
    /// </summary>
    public const string UserToken = "user_token";

    /// <summary>
    /// Parameter name for the token purpose string that identifies what the user token is used for.
    /// </summary>
    public const string TokenPurpose = "token_purpose";

    /// <summary>
    /// Parameter name for the two-factor authentication type identifier (Email, SMS, or AuthenticatorApp).
    /// </summary>
    public const string TwoFactorType = "two_factor_type";

    /// <summary>
    /// Parameter name for the email-based two-factor authentication token/OTP.
    /// </summary>
    public const string EmailToken = "email_token";

    /// <summary>
    /// Parameter name for the SMS-based two-factor authentication token/OTP.
    /// </summary>
    public const string SmsToken = "sms_token";

    /// <summary>
    /// Parameter name for the enabled/disabled flag (e.g., enabling or disabling 2FA for a user).
    /// </summary>
    public const string Enabled = "enabled";

    /// <summary>
    /// Parameter name for the lockout end date when locking a user account with a specific expiry.
    /// </summary>
    public const string EndDate = "end_date";

    /// <summary>
    /// Parameter name for filtering records by creation date.
    /// </summary>
    public const string CreatedDate = "created_date";

    /// <summary>
    /// Parameter name for filtering records by the user who created them.
    /// </summary>
    public const string CreatedBy = "created_by";

    /// <summary>
    /// Parameter name for the start date in date-range audit trail queries.
    /// </summary>
    public const string FromDate = "from_date";

    /// <summary>
    /// Parameter name for the end date in date-range audit trail queries.
    /// </summary>
    public const string ToDate = "to_date";

    /// <summary>
    /// Parameter name for the audit action type filter (Create, Update, Delete).
    /// </summary>
    public const string ActionType = "action_type";

    /// <summary>
    /// Parameter name for the TOTP authenticator app verification token.
    /// </summary>
    public const string TwoFactorAuthenticatorToken = "two_factor_authenticator_token";

    /// <summary>
    /// Parameter name for the application name used in authenticator app setup (displayed in the TOTP app).
    /// </summary>
    public const string ApplicationName = "application_name";

    /// <summary>
    /// Parameter name for the username used in authentication and user lookup operations.
    /// </summary>
    public const string UserName = "user_name";

    /// <summary>
    /// Parameter name for the password used in authentication operations.
    /// </summary>
    public const string Password = "password";

    /// <summary>
    /// Parameter name for the claim type string (e.g., "role", "email") in claim management operations.
    /// </summary>
    public const string ClaimType = "claim_type";

    /// <summary>
    /// Parameter name for the claim value string in claim management operations.
    /// </summary>
    public const string ClaimValue = "claim_value";

    /// <summary>
    /// Parameter name for a list of user security question-answer pairs.
    /// </summary>
    public const string ListOfUserSecurityQuestions = "user_security_questions_list";

    /// <summary>
    /// Parameter name for the notification channel type (Email or SMS).
    /// </summary>
    public const string NotificationType = "notification_type";

    /// <summary>
    /// Parameter name for the notification template object used when sending test or actual notifications.
    /// </summary>
    public const string NotificationTemplate = "notification_template";

    /// <summary>
    /// Parameter name for a list of client identifiers (used in bulk security token queries).
    /// </summary>
    public const string ClientsList = "clients_list";

    /// <summary>
    /// Parameter name for a list of user identifiers (used in bulk security token queries).
    /// </summary>
    public const string UserList = "user_list";

    /// <summary>
    /// Parameter name for the pagination model (page number, page size) in paginated API queries.
    /// </summary>
    public const string PagingModel = "paging_model";
}
