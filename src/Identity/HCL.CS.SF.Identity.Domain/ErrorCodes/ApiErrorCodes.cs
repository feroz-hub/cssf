/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.ErrorCodes;

/// <summary>
/// Error code constants returned by the Admin API layer (user management, resource management,
/// role management, notification, and authentication services). Each constant is a machine-readable
/// error identifier that is paired with a localized error message from the resource files.
/// </summary>
public static class ApiErrorCodes
{
    // ──── Identity Resource Errors ────

    /// <summary>
    /// Returned when the identity resource object passed to a create/update operation is null.
    /// </summary>
    public const string IdentityResourceIsNull = "IDENTITYRESOURCE_IS_NULL";

    /// <summary>
    /// Returned when the identity resource ID is invalid or does not exist in the database.
    /// </summary>
    public const string IdentityResourceIdInvalid = "IDENTITYRESOURCE_ID_INVALID";

    /// <summary>
    /// Returned when the identity resource name is invalid (e.g., contains illegal characters).
    /// </summary>
    public const string IdentityResourceNameInvalid = "IDENTITYRESOURCE_NAME_INVALID ";

    /// <summary>
    /// Returned when claims associated with an identity resource are null or invalid.
    /// </summary>
    public const string InvalidOrNullClaims = "INVALID_OR_NULLCLAIMS";

    /// <summary>
    /// Returned when the identity resource name is missing from the request.
    /// </summary>
    public const string IdentityResourceNameRequired = "IDENTITYRESOURCE_NAME_REQUIRED";

    /// <summary>
    /// Returned when the identity resource name exceeds the maximum allowed length.
    /// </summary>
    public const string IdentityResourceNameTooLong = "IDENTITYRESOURCE_NAME_TOO_LONG";

    /// <summary>
    /// Returned when the identity resource display name exceeds the maximum allowed length.
    /// </summary>
    public const string IdentityDisplayNameTooLong = "IDENTITYRESOURCE_DISPLAY_NAME_TOO_LONG";

    /// <summary>
    /// Returned when attempting to create an identity resource with a name that already exists.
    /// </summary>
    public const string IdentityResourceAlreadyExists = "IDENTITYRESOURCE_ALREADY_EXISTS";

    // ──── Identity Resource Claim Errors ────

    /// <summary>
    /// Returned when the identity resource claim ID is invalid or does not exist.
    /// </summary>
    public const string IdentityResourceClaimIdInvalid = "IDENTITYRESOURCE_CLAIM_ID_INVALID";

    /// <summary>
    /// Returned when attempting to add an identity resource claim that already exists for the resource.
    /// </summary>
    public const string IdentityResourceClaimAlreadyExists = "IDENTITYRESOURCE_CLAIM_ALREADY_EXISTS";

    /// <summary>
    /// Returned when the identity resource claim type is missing from the request.
    /// </summary>
    public const string IdentityResourceClaimTypeRequired = "IDENTITYRESOURCE_CLAIM_TYPE_REQUIRED";

    /// <summary>
    /// Returned when the identity claim object passed to a create operation is null.
    /// </summary>
    public const string IdentityClaimIsNull = "IDENTITYCLAIM_IS_NULL";

    /// <summary>
    /// Returned when the identity resource claim type exceeds the maximum allowed length.
    /// </summary>
    public const string IdentityResourceClaimTypeTooLong = "IDENTITYRESOURCE_CLAIM_TYPE_TOO_LONG";

    /// <summary>
    /// Returned when the CreatedBy field of an identity resource claim exceeds the maximum length.
    /// </summary>
    public const string IdentityResourceClaimCreatedByTooLong = "IDENTITYRESOURCE_CLAIM_CREATEDBY_TOO_LONG";

    /// <summary>
    /// Returned when the CreatedBy or ModifiedBy fields of an identity resource claim exceed the maximum length.
    /// </summary>
    public const string IdentityResourceClaimCreatedbyOrModifiedByTooLong =
        "IDENTITYRESOURCE_CLAIM_CREATEDBY_MODIFIEDBY_TOO_LONG";

    // ──── Common Errors ────

    /// <summary>
    /// Returned when a query operation finds no matching records in the database.
    /// </summary>
    public const string NoRecordsFound = "NO_RECORDS_FOUND";

    /// <summary>
    /// Returned when a database concurrency conflict occurs (another process modified the record).
    /// </summary>
    public const string ConcurrencyFailure = "CONCURRENCY_FAILURE";

    /// <summary>
    /// Returned when a required input object is null or contains invalid data.
    /// </summary>
    public const string InvalidOrNullObject = "INVALID_OR_NULL_OBJECT";

    /// <summary>
    /// Returned when the caller does not have a valid access token or the token is missing.
    /// </summary>
    public const string UnauthorizedAccess = "UNAUTHORIZED_ACCESS";

    /// <summary>
    /// Returned when the caller's access token does not contain the required permissions.
    /// </summary>
    public const string AccessDenied = "ACCESS_DENIED";

    /// <summary>
    /// Returned when the requested resource does not exist.
    /// </summary>
    public const string NotFound = "NOT_FOUND";

    // ──── API Resource Errors ────

    /// <summary>
    /// Returned when the API resource object passed to a create/update operation is null.
    /// </summary>
    public const string ApiResourceIsNull = "APIRESOURCE_IS_NULL";

    /// <summary>
    /// Returned when the API resource name is invalid or contains illegal characters.
    /// </summary>
    public const string ApiResourceNameInvalid = "APIRESOURCE_NAME_INVALID";

    /// <summary>
    /// Returned when the API resource ID is invalid or does not exist in the database.
    /// </summary>
    public const string ApiResourceIdInvalid = "APIRESOURCE_ID_INVALID";

    /// <summary>
    /// Returned when attempting to create an API resource with a name that already exists.
    /// </summary>
    public const string ApiResourceAlreadyExists = "APIRESOURCE_ALREADY_EXISTS";

    /// <summary>
    /// Returned when the API resource name is missing from the request.
    /// </summary>
    public const string ApiResourceNameRequired = "APIRESOURCE_NAME_REQUIRED";

    /// <summary>
    /// Returned when the API resource name exceeds the maximum allowed length.
    /// </summary>
    public const string ApiResourceNameTooLong = "APIRESOURCE_NAME_TOO_LONG";

    /// <summary>
    /// Returned when the API resource display name exceeds the maximum allowed length.
    /// </summary>
    public const string ApiResourceDisplayNameTooLong = "APIRESOURCE_DISPLAY_NAME_TOO_LONG";

    // ──── API Resource Claim Errors ────

    /// <summary>
    /// Returned when the API resource claim ID is invalid or does not exist.
    /// </summary>
    public const string ApiResourceClaimIdInvalid = "APIRESOURCE_CLAIM_ID_INVALID";

    /// <summary>
    /// Returned when the API resource claim object passed to a create operation is null.
    /// </summary>
    public const string ApiResourceClaimIsNull = "APIRESOURCE_CLAIM_IS_NULL";

    /// <summary>
    /// Returned when the API resource claim type is missing from the request.
    /// </summary>
    public const string ApiResourceClaimTypeRequired = "APIRESOURCE_CLAIM_TYPE_REQUIRED";

    /// <summary>
    /// Returned when the API resource claim value is missing from the request.
    /// </summary>
    public const string ApiResourceClaimValueRequired = "APIRESOURCE_CLAIM_VALUE_REQUIRED";

    /// <summary>
    /// Returned when the API resource claim type exceeds the maximum allowed length.
    /// </summary>
    public const string ApiResourceClaimTypeTooLong = "APIRESOURCE_CLAIM_TYPE_TOO_LONG";

    /// <summary>
    /// Returned when the API resource claim value exceeds the maximum allowed length.
    /// </summary>
    public const string ApiResourceClaimValueTooLong = "APIRESOURCE_CLAIM_VALUE_TOO_LONG";

    /// <summary>
    /// Returned when attempting to add an API resource claim that already exists for the resource.
    /// </summary>
    public const string ApiResourceClaimAlreadyExists = "APIRESOURCE_CLAIM_ALREADY_EXISTS";

    /// <summary>
    /// Returned when the API resource claim type or resource ID combination is invalid.
    /// </summary>
    public const string InvalidApiResourceClaimTypeOrResourceId = "INVALID_APIRESOURCE_CLAIM_TYPE_OR_RESOURCEID";

    /// <summary>
    /// Returned when the identity resource claim type or resource ID combination is invalid.
    /// </summary>
    public const string InvalidIdentityResourceClaimTypeOrResourceId =
        "INVALID_IDENTITYRESOURCE_CLAIM_TYPE_OR_RESOURCEID";

    /// <summary>
    /// Returned when the API resource claim ID is invalid (duplicate constant for backward compatibility).
    /// </summary>
    public const string ApiResourceClaimIdIsInvalid = "APIRESOURCE_CLAIM_ID_INVALID";

    /// <summary>
    /// Returned when the CreatedBy field of an API resource claim exceeds the maximum length.
    /// </summary>
    public const string ApiResourceClaimCreatedByTooLong = "APIRESOURCE_CLAIM_CREATEDBY_TOO_LONG";

    /// <summary>
    /// Returned when the CreatedBy or ModifiedBy fields of an API resource claim exceed the maximum length.
    /// </summary>
    public const string ApiResourceClaimCreatedbyOrModifiedByTooLong =
        "APIRESOURCE_CLAIM_CREATEDBY_MODIFIEDBY_TOO_LONG";

    // ──── API Scope Errors ────

    /// <summary>
    /// Returned when the API scope object passed to a create/update operation is null.
    /// </summary>
    public const string ApiScopeIsNull = "APISCOPE_IS_NULL";

    /// <summary>
    /// Returned when the API scope ID is invalid or does not exist in the database.
    /// </summary>
    public const string ApiScopeIdInvalid = "APISCOPE_ID_INVALID";

    /// <summary>
    /// Returned when the API scope name is invalid or contains illegal characters.
    /// </summary>
    public const string ApiScopeNameInvalid = "APISCOPE_NAME_INVALID ";

    /// <summary>
    /// Returned when the API scope name is missing from the request.
    /// </summary>
    public const string ApiScopeNameRequired = "APISCOPE_NAME_REQUIRED";

    /// <summary>
    /// Returned when the API scope name, display name, or CreatedBy field exceeds the maximum length.
    /// </summary>
    public const string ApiScopeNameOrDisplayNameOrCreatedbyTooLong =
        "APISCOPE_NAME_OR_DISPLAYNAME_OR_CREATEDBY_TOO_LONG";

    /// <summary>
    /// Returned when the API scope ModifiedBy field exceeds the maximum allowed length.
    /// </summary>
    public const string ApiScopeModifiedbyTooLong = "APISCOPE_MODIFIEDBY_TOO_LONG";

    /// <summary>
    /// Returned when attempting to create an API scope with a name that already exists.
    /// </summary>
    public const string ApiScopeAlreadyExists = "APISCOPE_ALREADY_EXISTS";

    /// <summary>
    /// Returned when the API scope name or resource ID combination is invalid.
    /// </summary>
    public const string InvalidApiScopeNameOrResourceId = "INVALID_APISCOPE_NAME_OR_RESOURCEID";

    /// <summary>
    /// Returned when the API scope name exceeds the maximum allowed length.
    /// </summary>
    public const string ApiScopeNameTooLong = "APISCOPE_NAME_TOO_LONG";

    /// <summary>
    /// Returned when the API scope display name exceeds the maximum allowed length.
    /// </summary>
    public const string ApiScopeDisplayNameTooLong = "APISCOPE_DISPLAYNAME_TOO_LONG";

    // ──── API Scope Claim Errors ────

    /// <summary>
    /// Returned when the API scope claim ID is invalid or does not exist.
    /// </summary>
    public const string ApiScopeClaimIdInvalid = "APISCOPE_CLAIM_ID_INVALID";

    /// <summary>
    /// Returned when the API scope claim object passed to a create operation is null.
    /// </summary>
    public const string ApiScopeClaimIsNull = "APISCOPE_CLAIM_IS_NULL";

    /// <summary>
    /// Returned when the API scope claim type is missing from the request.
    /// </summary>
    public const string ApiScopeClaimTypeRequired = "APISCOPE_CLAIM_TYPE_REQUIRED";

    /// <summary>
    /// Returned when the API scope claim type or CreatedBy field exceeds the maximum length.
    /// </summary>
    public const string ApiScopeClaimTypeOrCreatedbyTooLong = "APISCOPE_CLAIMTYPE_CREATEDBY_TOO_LONG";

    /// <summary>
    /// Returned when the API scope claim ModifiedBy field exceeds the maximum allowed length.
    /// </summary>
    public const string ApiScopeClaimModifiedbyTooLong = "APISCOPE_CLAIM_MODIFIEDBY_TOO_LONG";

    /// <summary>
    /// Returned when no API scope claim was found for the specified claim type.
    /// </summary>
    public const string ApiScopeClaimNotFoundForType = "APISCOPE_CLAIM_NOT_FOUND_FOR_TYPE";

    /// <summary>
    /// Returned when attempting to add an API scope claim that already exists for the scope.
    /// </summary>
    public const string ApiScopeClaimsAlreadyExists = "APISCOPE_CLAIMS_ALREADY_EXISTS";

    /// <summary>
    /// Returned when the API scope claim type or scope ID combination is invalid.
    /// </summary>
    public const string InvalidApiScopeClaimTypeOrScopeId = "INVALID_APISCOPE_CLAIM_TYPE_OR_SCOPEID";

    /// <summary>
    /// Returned when the API scope claim type exceeds the maximum allowed length.
    /// </summary>
    public const string ApiScopeClaimTypeTooLong = "APISCOPE_CLAIM_TYPE_TOO_LONG";

    /// <summary>
    /// Returned when the API scope claim value is missing from the request.
    /// </summary>
    public const string ApiScopeClaimValueRequired = "APISCOPE_CLAIM_VALUE_REQUIRED";

    /// <summary>
    /// Returned when the API scope claim value exceeds the maximum allowed length.
    /// </summary>
    public const string ApiScopeClaimValueTooLong = "APISCOPE_CLAIM_VALUE_TOO_LONG";

    // ──── Resource Owner Password (ROP) Errors ────

    /// <summary>
    /// Returned when the ROP credential validation model is null or missing required fields.
    /// </summary>
    public const string Invalid_Rop_Validation_Model = "INVALID_ROP_VALIDATION_MODEL";

    // ──── LDAP Errors ────

    /// <summary>
    /// Returned when the LDAP connection string is invalid or missing.
    /// </summary>
    public const string ConnectionStringInvalid = "CONNECTION_STRING_INVALID";

    /// <summary>
    /// Returned when the LDAP server connection attempt fails (network, authentication, or TLS error).
    /// </summary>
    public const string LDAPConnectionFailed = "LDAP_CONNECTION_FAILED";

    /// <summary>
    /// Returned when an LDAP-authenticated user attempts to call an API restricted to local users only.
    /// </summary>
    public const string RestrictedApiForLdapUser = "RESTRICTED_API_FOR_LDAP_USER";

    // ──── Audit Trail Errors ────

    /// <summary>
    /// Returned when the audit trail model passed to the add operation is null.
    /// </summary>
    public const string AuditModelIsNull = "AUDIT_MODEL_IS_NULL";

    /// <summary>
    /// Returned when the specified audit action type is not a valid AuditType enum value.
    /// </summary>
    public const string InvalidAuditActionType = "INVALID_AUDIT_ACTION_TYPE";

    /// <summary>
    /// Returned when the audit table name exceeds the maximum allowed length.
    /// </summary>
    public const string AuditTableNameTooLong = "AUDIT_TABLENAME_TOO_LONG";

    /// <summary>
    /// Returned when the "from" date is greater than the "to" date in a date-range audit query.
    /// </summary>
    public const string FromDateGreaterThanToDate = "FROMDATE_GREATER_TODATE";

    // ──── User Claim Errors ────

    /// <summary>
    /// Returned when the user ID is missing from a user claim operation request.
    /// </summary>
    public const string UserClaimUserIdRequired = "USERCLAIM_USERID_REQUIRED";

    /// <summary>
    /// Returned when the claim type is missing from a user claim operation request.
    /// </summary>
    public const string UserClaimTypeRequired = "USERCLAIM_CLAIMTYPE_REQUIRED";

    /// <summary>
    /// Returned when the claim value is missing from a user claim operation request.
    /// </summary>
    public const string UserClaimValueRequired = "USERCLAIM_CLAIMVALUE_REQUIRED";

    /// <summary>
    /// Returned when attempting to add a user claim that already exists for the user.
    /// </summary>
    public const string UserClaimsAlreadyExists = "USER_CLAIMS_EXISTS";

    /// <summary>
    /// Returned when the CreatedBy field of a user claim exceeds the maximum length.
    /// </summary>
    public const string UserClaimCreatedbyTooLong = "USERCLAIM_CREATEDBY_TOO_LONG";

    /// <summary>
    /// Returned when the ModifiedBy field of a user claim exceeds the maximum length.
    /// </summary>
    public const string UserClaimModifiedbyTooLong = "USERCLAIM_MODIFIEDBY_TOO_LONG";

    /// <summary>
    /// Returned when the user claims data is invalid or malformed.
    /// </summary>
    public const string InvalidUserClaims = "INVALID_USER_CLAIMS";

    // ──── Email/SMS Notification Errors ────

    /// <summary>
    /// Returned when the sender (From) email address is invalid or missing.
    /// </summary>
    public const string InvalidFromAddress = "INVALID_FROM_ADDRESS";

    /// <summary>
    /// Returned when the recipient (To) email address is invalid or missing.
    /// </summary>
    public const string InvalidToAddress = "INVALID_TO_ADDRESS";

    /// <summary>
    /// Returned when the email subject is invalid or missing.
    /// </summary>
    public const string InvalidSubject = "INVALID_SUBJECT";

    /// <summary>
    /// Returned when the notification template name is invalid or does not match any configured template.
    /// </summary>
    public const string InvalidTemplateName = "INVALID_TEMPLATENAME";

    /// <summary>
    /// Returned when the email body content is empty or not specified.
    /// </summary>
    public const string NoContentSpecifiedForEmail = "NO_CONTENT_FOR_EMAIL";

    /// <summary>
    /// Returned when the SMTP server is not configured in the email settings.
    /// </summary>
    public const string SmtpServerNotConfiguredForEmail = "SMTPSERVER_NOT_CONFIGURED";

    /// <summary>
    /// Returned when the SMTP port is not configured in the email settings.
    /// </summary>
    public const string PortNotConfiguredForEmail = "PORT_NOT_CONFIGURED";

    /// <summary>
    /// Returned when the SMTP username is missing from the email configuration.
    /// </summary>
    public const string UserNameNotConfiguredForEmail = "USERNAME_MISSING_FOR_EMAIL";

    /// <summary>
    /// Returned when the SMTP password is missing from the email configuration.
    /// </summary>
    public const string PasswordNotConfiguredForEmail = "PASSWORD_MISSING_FOR_EMAIL";

    /// <summary>
    /// Returned when the audit activity description exceeds the maximum allowed length.
    /// </summary>
    public const string ActivityTooLong = "ACTIVITY_TOO_LONG";

    /// <summary>
    /// Returned when the recipient phone number is missing from an SMS notification request.
    /// </summary>
    public const string ToNumberNotConfiguredForSMS = "TONUMBER_MISSING_FOR_SMS";

    /// <summary>
    /// Returned when the sender phone number is missing from the SMS configuration.
    /// </summary>
    public const string FromNumberNotConfiguredForSMS = "FROMNUMBER_MISSING_FOR_SMS";

    /// <summary>
    /// Returned when the SMS body content is empty or not specified.
    /// </summary>
    public const string NoContentSpecifiedForSMS = "NO_CONTENT_FOR_SMS";

    /// <summary>
    /// Returned when the SMS provider username is missing from the configuration.
    /// </summary>
    public const string UserNameNotConfiguredForSMS = "USERNAME_MISSING_FOR_SMS";

    /// <summary>
    /// Returned when the SMS provider password is missing from the configuration.
    /// </summary>
    public const string PasswordNotConfiguredForSMS = "PASSWORD_MISSING_FOR_SMS";

    /// <summary>
    /// Returned when the SMS delivery status callback URL is missing from the configuration.
    /// </summary>
    public const string SMSCallbackURLMissing = "SMS_CALLBACK_URL_MISSING";

    // ──── Password Validation Errors ────

    /// <summary>
    /// Returned when the password does not match the configured regex pattern.
    /// </summary>
    public const string PasswordPatternNotMatched = "PASSWORD_PATTERN_NOT_MATCHED";

    /// <summary>
    /// Returned when the password does not contain at least one lowercase letter.
    /// </summary>
    public const string PasswordRequiredLowerCase = "PASSWORD_REQUIRED_LOWERCASE";

    /// <summary>
    /// Returned when the password does not contain at least one uppercase letter.
    /// </summary>
    public const string PasswordRequiredUpperCase = "PASSWORD_REQUIRED_UPPERCASE";

    /// <summary>
    /// Returned when the password is shorter than the minimum required length.
    /// </summary>
    public const string InvalidPasswordLength = "INVALID_PASSWORD_LENGTH";

    /// <summary>
    /// Returned when the password exceeds the maximum allowed length.
    /// </summary>
    public const string PasswordTooLong = "PASSWORD_TOO_LONG";

    /// <summary>
    /// Returned when the password does not contain at least one numeric digit.
    /// </summary>
    public const string PasswordRequiredNumericValue = "PASSWORD_REQUIRED_NUMERIC";

    /// <summary>
    /// Returned when the password does not contain at least one special character.
    /// </summary>
    public const string PasswordRequiredSpecialCharacters = "PASSWORD_REQUIRED_SPECIALCHAR";

    /// <summary>
    /// Returned when the password contains whitespace characters, which are not allowed.
    /// </summary>
    public const string PasswordContainsSpace = "PASSWORD_CONTAINS_SPACE";

    /// <summary>
    /// Returned when the current password provided during a password change is incorrect.
    /// </summary>
    public const string InvalidCurrentPassword = "INVALID_CURRENTPASSWORD";

    /// <summary>
    /// Returned when the new password provided during a password change/reset is invalid.
    /// </summary>
    public const string InvalidNewPassword = "INVALID_NEWPASSWORD";

    /// <summary>
    /// Returned when the password and confirmation password do not match.
    /// </summary>
    public const string PasswordMismatch = "PASSWORD_MISMATCH";

    /// <summary>
    /// Returned when the new password is the same as the current password during a password change.
    /// </summary>
    public const string CurrentAndNewPasswordAreSame = "PASSWORD_SAME";

    /// <summary>
    /// Returned when the new password matches one of the previously used passwords (reuse prevention).
    /// </summary>
    public const string PasswordFromPreviousList = "PASSWORD_ALREADY_USED";

    /// <summary>
    /// Returned as a warning when the user's password is about to expire within the notification window.
    /// </summary>
    public const string PasswordAboutToExpire = "PASSWORD_ABOUT_TO_EXPIRE";

    // ──── Authentication Errors ────

    /// <summary>
    /// Returned when the system fails to retrieve the authenticated user from the sign-in session.
    /// </summary>
    public const string FetchingAuthenticationUserFailed = "FETCHING_AUTHENTICATION_USER_FAILED";

    /// <summary>
    /// Returned when TOTP authenticator app verification fails (wrong code or expired code).
    /// </summary>
    public const string AuthenticatorAppVerificationFailed = "AUTHENTICATOR_APP_VERIFICATION_FAILED";

    /// <summary>
    /// Returned when the authenticator app reset operation fails.
    /// </summary>
    public const string AuthenticatorResetFailed = "AUTHENTICATOR_APP_RESET_FAILED";

    /// <summary>
    /// Returned when recovery code generation is attempted but 2FA is not enabled for the user.
    /// </summary>
    public const string RecoveryCode2FANotEnabled = "RECOVERYCODE_2FA_NOT_ENABLED";

    // ──── User Management Errors ────

    /// <summary>
    /// Returned when the provided user ID is invalid (empty GUID or non-existent).
    /// </summary>
    public const string InvalidUserId = "INVALID_USERID";

    /// <summary>
    /// Returned when the claims principal object is null or does not contain valid identity information.
    /// </summary>
    public const string InvalidClaimsPrincipal = "INVALID_CLAIMS_PRINCIPAL";

    /// <summary>
    /// Returned when the user model passed to a create/update operation is null.
    /// </summary>
    public const string UserModelIsNull = "USER_MODEL_IS_NULL";

    /// <summary>
    /// Returned when the lockout end date is invalid (e.g., in the past).
    /// </summary>
    public const string InvalidLockoutEndDate = "INVALID_LOCKOUT_ENDDATE";

    /// <summary>
    /// Returned when attempting to register a user with a username or email that already exists.
    /// </summary>
    public const string UserAlreadyExists = "USER_EXISTS";

    /// <summary>
    /// Returned when the specified notification template does not exist in the configuration.
    /// </summary>
    public const string TemplateDoesNotExists = "TEMPLATE_NOT_EXISTS";

    /// <summary>
    /// Returned when the specified two-factor authentication type is not valid.
    /// </summary>
    public const string InvalidTwoFactorType = "INVALID_TWOFACTOR_TYPE";

    /// <summary>
    /// Returned when the specified two-factor type ID is not valid.
    /// </summary>
    public const string InvalidTwoFactorTypeId = "INVALID_TWOFACTOR_TYPE_ID";

    /// <summary>
    /// Returned when a 2FA operation is attempted but 2FA is not enabled for the user.
    /// </summary>
    public const string TwoFactorNotEnabledForUser = "TWOFACTOR_NOT_ENABLED";

    /// <summary>
    /// Returned when the username contains invalid characters or format.
    /// </summary>
    public const string InvalidUsername = "INVALID_USERNAME";

    /// <summary>
    /// Returned when the username field is required but not provided.
    /// </summary>
    public const string UsernameRequired = "USERNAME_REQUIRED";

    /// <summary>
    /// Returned when the username or password is incorrect during sign-in.
    /// </summary>
    public const string InvalidUserOrPassword = "INVALID_USER_OR_PASSWORD";

    /// <summary>
    /// Returned when the username length is outside the configured min/max bounds.
    /// </summary>
    public const string InvalidLengthForUsername = "INVALID_USERNAME_LENGTH";

    /// <summary>
    /// Returned when the first name field is required but not provided.
    /// </summary>
    public const string FirstnameRequired = "FIRSTNAME_REQUIRED";

    /// <summary>
    /// Returned when the first name length is outside the configured min/max bounds.
    /// </summary>
    public const string InvalidLengthForFirstName = "INVALID_FIRST_NAME_LENGTH";

    /// <summary>
    /// Returned when the last name length is outside the configured min/max bounds.
    /// </summary>
    public const string InvalidLengthForLastName = "INVALID_LAST_NAME_LENGTH";

    /// <summary>
    /// Returned when the email address format is invalid (not a valid email).
    /// </summary>
    public const string InvalidEmailFormat = "INVALID_EMAIL_FORMAT";

    /// <summary>
    /// Returned when the phone number length is outside the configured min/max bounds.
    /// </summary>
    public const string InvalidLengthForPhoneNumber = "INVALID_PHONENUMBER_LENGTH";

    /// <summary>
    /// Returned when the last name field is required but not provided.
    /// </summary>
    public const string LastnameRequired = "LASTNAME_REQUIRED";

    /// <summary>
    /// Returned when the phone number field is required but not provided.
    /// </summary>
    public const string PhonenumberRequired = "PHONENUMBER_REQUIRED";

    /// <summary>
    /// Returned when the phone number format is invalid.
    /// </summary>
    public const string InvalidPhonenumber = "INVALID_PHONE_NUMBER";

    // Added new error code for DOB required
    /// <summary>
    /// Returned when the date of birth field is required but not provided.
    /// </summary>
    public const string DOBIsRequired = "DOB_REQUIRED";

    /// <summary>
    /// Returned when the date of birth is outside the configured min/max age bounds.
    /// </summary>
    public const string InvalidDob = "INVALID_DOB";

    /// <summary>
    /// Returned when attempting to operate on a user that has been soft-deleted.
    /// </summary>
    public const string UserIsAlreadyDeleted = "USER_DELETED";

    /// <summary>
    /// Returned when a user with a default/temporary password attempts to access a protected resource
    /// without first changing the password.
    /// </summary>
    public const string DefaultPasswordNeedsToChange = "DEFAULT_PASSWORD_NEEDS_TO_CHANGE";

    /// <summary>
    /// Returned when the claims data is invalid or malformed.
    /// </summary>
    public const string InvalidClaims = "INVALID_CLAIMS";

    /// <summary>
    /// Returned when the claim type value is invalid or empty.
    /// </summary>
    public const string InvalidClaimType = "INVALID_CLAIM_TYPE";

    /// <summary>
    /// Returned when the claim value is invalid or empty.
    /// </summary>
    public const string InvalidClaimValue = "INVALID_CLAIM_VALUE";

    // ──── Token Verification Errors ────

    /// <summary>
    /// Returned when the two-factor token provided by the user is invalid or expired.
    /// </summary>
    public const string InvalidTwoFactorTokenProvided = "INVALID_TWOFACTOR_TOKEN_PROVIDED";

    /// <summary>
    /// Returned when the system fails to generate a two-factor token.
    /// </summary>
    public const string InvalidTwoFactorTokenGenerated = "INVALID_TWOFACTOR_TOKEN_GENERATED";

    /// <summary>
    /// Returned when the system fails to generate an email confirmation token.
    /// </summary>
    public const string InvalidEmailConfirmationTokenGenerated = "INVALID_EMAILCONFIRMATION_TOKEN_GENERATED";

    /// <summary>
    /// Returned when the email confirmation token provided by the user is invalid or expired.
    /// </summary>
    public const string InvalidEmailConfirmationTokenProvided = "INVALID_EMAILCONFIRMATION_TOKEN_PROVIDED";

    /// <summary>
    /// Returned when the system fails to generate a phone number confirmation token.
    /// </summary>
    public const string InvalidPhoneConfirmationTokenGenerated = "INVALID_PHONECONFIRMATION_TOKEN_GENERATED";

    /// <summary>
    /// Returned when the phone number confirmation token provided by the user is invalid or expired.
    /// </summary>
    public const string InvalidPhoneConfirmationTokenProvided = "INVALID_PHONECONFIRMATION_TOKEN_PROVIDED";

    /// <summary>
    /// Returned when the system fails to generate a general-purpose user token.
    /// </summary>
    public const string InvalidUserTokenGenerated = "INVALID_USER_TOKEN_GENERATED";

    /// <summary>
    /// Returned when the general-purpose user token provided is invalid or expired.
    /// </summary>
    public const string InvalidUserTokenProvided = "INVALID_USER_TOKEN_PROVIDED";

    /// <summary>
    /// Returned when the password reset token provided by the user is invalid or expired.
    /// </summary>
    public const string InvalidResetTokenProvided = "INVALID_RESET_TOKEN_PROVIDED";

    /// <summary>
    /// Returned when the system fails to generate a password reset token.
    /// </summary>
    public const string InvalidResetTokenGenerated = "INVALID_RESET_TOKEN_GENERATED";

    /// <summary>
    /// Returned when a user token is invalid (generic token validation failure).
    /// </summary>
    public const string InvalidUserToken = "INVALID_USER_TOKEN";

    /// <summary>
    /// Returned when the token purpose string is invalid or missing.
    /// </summary>
    public const string InvalidPurpose = "INVALID_PURPOSE";

    // ──── Security Question Errors ────

    /// <summary>
    /// Returned when the security question model is null or missing required fields.
    /// </summary>
    public const string InvalidSecurityQuestionModel = "INVALID_SECURITYQUESTION_MODEL";

    /// <summary>
    /// Returned when the security question text is invalid or empty.
    /// </summary>
    public const string InvalidSecurityQuestion = "INVALID_SECURITYQUESTION";

    /// <summary>
    /// Returned when attempting to add a security question that already exists.
    /// </summary>
    public const string SecurityQuestionAlreadyExists = "SECURITYQUESTION_EXISTS";

    /// <summary>
    /// Returned when the specified security question does not exist in the database.
    /// </summary>
    public const string SecurityQuestionNotExists = "SECURITYQUESTION_NOT_EXISTS";

    /// <summary>
    /// Returned when attempting to delete a security question that is associated with user accounts.
    /// </summary>
    public const string SecurityQuestionUserAssociated = "SECURITY_QUESTION_USER_ASSOCIATED";

    /// <summary>
    /// Returned when the security question text exceeds the maximum allowed length.
    /// </summary>
    public const string SecurityQuestionTooLong = "SECURITYQUESTION_TOO_LONG";

    // ──── User Security Question Errors ────

    /// <summary>
    /// Returned when the security question is already associated with the user.
    /// </summary>
    public const string SecurityQuestionAlreadyExistsForUser = "USER_SECURITYQUESTION_EXISTS";

    /// <summary>
    /// Returned when the specified security question is not associated with the user.
    /// </summary>
    public const string SecurityQuestionNotExistsForUser = "USER_SECURITYQUESTION_NOT_EXISTS";

    /// <summary>
    /// Returned when the user security question model is null or missing required fields.
    /// </summary>
    public const string InvalidUserSecurityQuestionModel = "INVALID_USER_SECURITYQUESTION_MODEL";

    /// <summary>
    /// Returned when the user's answer does not match the stored answer for the security question.
    /// </summary>
    public const string AnswerDoesNotMatchForSecurityQuestion = "SECURITYQUESTION_ANSWER_MISMATCH";

    /// <summary>
    /// Returned when the user ID is invalid in a security question operation.
    /// </summary>
    public const string InvalidUserIdForSecurityQuestion = "INVALID_USERID_FOR_SECURITYQUESTION";

    /// <summary>
    /// Returned when the security answer length is outside the configured bounds.
    /// </summary>
    public const string InvalidLengthForUserSecurityAnswer = "INVALID_USER_SECURITYANSWER_LENGTH";

    /// <summary>
    /// Returned when the security answer field is required but not provided.
    /// </summary>
    public const string UserSecurityAnswerIsRequired = "USER_SECURITYANSWER_REQUIRED";

    /// <summary>
    /// Returned when the security question field is required but not selected.
    /// </summary>
    public const string SecurityQuestionRequired = "SECURITYQUESTION_REQUIRED";

    /// <summary>
    /// Returned when the number of security questions provided is less than the required minimum.
    /// </summary>
    public const string InvalidSecurityQuestionCount = "INVALID_SECURITYQUESTION_COUNT";

    /// <summary>
    /// Returned when the security answer length is outside the allowed bounds.
    /// </summary>
    public const string InvalidSecurityAnswerLength = "INVALID_SECURITYANSWER_LENGTH";

    /// <summary>
    /// Returned when the security answer is invalid or contains prohibited characters.
    /// </summary>
    public const string InvalidSecurityAnswer = "INVALID_SECURITY_ANSWER";

    /// <summary>
    /// Returned when the security question ID is invalid or does not exist.
    /// </summary>
    public const string InvalidSecurityQuestionId = "INVALID_SECURITYQUESTION_ID";

    /// <summary>
    /// Returned when duplicate security questions are provided (each question must be unique).
    /// </summary>
    public const string UniqueSecurityQuestion = "UNIQUE_SECURITYQUESTION";

    /// <summary>
    /// Returned when the user ID is missing from a user security question operation.
    /// </summary>
    public const string UserSecurityQuestionUserIdRequired = "USER_SECURITYQUESTION_USERID_REQUIRED";

    /// <summary>
    /// Returned when the security question ID is missing from a user security question operation.
    /// </summary>
    public const string UserSecurityQuestionSecurityIdRequired = "USER_SECURITYQUESTION_SECURITY_ID_REQUIRED";

    /// <summary>
    /// Returned when the CreatedBy field of a security question exceeds the maximum length.
    /// </summary>
    public const string SecurityQuestionCreatedbyTooLong = "SECURITYQUESTION_CREATEDBY_TOO_LONG";

    /// <summary>
    /// Returned when the ModifiedBy field of a security question exceeds the maximum length.
    /// </summary>
    public const string SecurityQuestionModifiedbyTooLong = "SECURITYQUESTION_MODIFIEDBY_TOO_LONG";

    /// <summary>
    /// Returned when the security question answer exceeds the maximum allowed length.
    /// </summary>
    public const string SecurityQuestionAnswerTooLong = "SECURITYQUESTION_ANSWER_TOO_LONG";

    // ──── Account Lock/Unlock Errors ────

    /// <summary>
    /// Returned when a user security question operation is attempted on an inactive/deactivated account.
    /// </summary>
    public const string InactiveInSystemUserSecurityQuestion = "INACTIVE_IN_SYSTEM_USER_SECURITYQUESTION";

    /// <summary>
    /// Returned when the password field is required but not provided.
    /// </summary>
    public const string PasswordRequired = "PASSWORD_REQUIRED.";

    /// <summary>
    /// Returned when the email field is required but not provided.
    /// </summary>
    public const string EmailRequired = "EMAIL_REQUIRED";

    /// <summary>
    /// Returned when the email address exceeds the maximum allowed length.
    /// </summary>
    public const string EmailLengthTooLong = "EMAIL_LENGTH_TOO_LONG";

    /// <summary>
    /// Returned when the user account is locked due to too many failed login attempts.
    /// </summary>
    public const string UserAccountLocked = "USER_ACCOUNT_LOCKED";

    /// <summary>
    /// Returned when login is not allowed at the current time (time-based access restrictions).
    /// </summary>
    public const string LoginNotAllowedAtThisTime = "LOGIN_RESTRICTED_TIME";

    /// <summary>
    /// Returned when login is not allowed from the current workstation (location-based access restrictions).
    /// </summary>
    public const string LoginNotAllowedAtThisWorkStation = "LOGIN_RESTRICTED_WORKSTATION";

    /// <summary>
    /// Returned when the user's password has expired and must be changed before login.
    /// </summary>
    public const string PasswordExpired = "PASSWORD_EXPIRED";

    // ──── LDAP User Errors ────

    /// <summary>
    /// Returned when the LDAP username is invalid or empty.
    /// </summary>
    public const string InvalidLDAPUserName = "INVALID_LDAP_USERNAME";

    /// <summary>
    /// Returned when the LDAP username or password is incorrect.
    /// </summary>
    public const string InvalidLDAPUserNameOrPassword = "INVALID_LDAP_USERNAME_OR_PASSWORD";

    /// <summary>
    /// Returned when the LDAP hostname is invalid or unreachable.
    /// </summary>
    public const string InvalidLDAPHostname = "INVALID_LDAP_HOSTNAME";

    /// <summary>
    /// Returned when the LDAP port number is invalid or out of range.
    /// </summary>
    public const string InvalidLDAPPort = "INVALID_LDAP_PORT";

    /// <summary>
    /// Returned during a failed login attempt, indicating the number of remaining attempts before lockout.
    /// </summary>
    public const string LoginFailedMismatchRemainingCount = "LOGIN_REMAINING_ATTEMPTS";

    /// <summary>
    /// Returned when multiple users with the same credentials are found in the LDAP directory.
    /// </summary>
    public const string DuplicateLDAPUserFound = "DUPLICATE_LDAP_USER_FOUND";

    /// <summary>
    /// Returned when the LDAP configuration is incomplete or invalid.
    /// </summary>
    public const string InvalidLDAPConfiguration = "INVALID_LDAP_CONFIGURATION";

    // TODO: This message needs to be corrected based on different context. Pre-sign in check related error message is given in resx. But in
    // all the invoked places, the context varies.
    /// <summary>
    /// Returned when a pre-sign-in check fails (e.g., email not confirmed, account disabled).
    /// </summary>
    public const string LoginNotAllowed = "LOGIN_NOT_ALLOWED";

    /// <summary>
    /// Returned when no default authentication scheme is found in the application configuration.
    /// </summary>
    public const string NoDefaultAuthenticateSchemeFound = "NO_AUTHENTICATION_SCHEME_FOUND";

    /// <summary>
    /// Returned when no authentication handler is configured for the requested scheme.
    /// </summary>
    public const string NoAuthenticationHandlerConfigured = "NO_AUTHENTICATION_HANDLER_CONFIGURED";

    // ──── Role Errors ────

    /// <summary>
    /// Returned when the role ID is required but not provided.
    /// </summary>
    public const string RoleIdRequired = "ROLE_ID_REQUIRED";

    /// <summary>
    /// Returned when the role name is required but not provided.
    /// </summary>
    public const string RoleNameRequired = "ROLE_NAME_REQUIRED";

    /// <summary>
    /// Returned when attempting to create a role with a name that already exists.
    /// </summary>
    public const string RoleAlreadyExists = "ROLE_ALREADY_EXISTS";

    /// <summary>
    /// Returned when the role object passed to a create/update operation is null.
    /// </summary>
    public const string RoleIsNull = "ROLE_IS_NULL";

    /// <summary>
    /// Returned when attempting to assign a role that is already assigned to the user.
    /// </summary>
    public const string UserRoleAlreadyExists = "USER_ROLE_ALREADY_EXISTS";

    /// <summary>
    /// Returned when attempting to remove a role assignment that does not exist for the user.
    /// </summary>
    public const string UserRoleNotExists = "USER_ROLE_NOT_EXISTS";

    /// <summary>
    /// Returned when the provided role ID is invalid or does not exist.
    /// </summary>
    public const string InvalidRoleId = "INVALID_ROLEID";

    /// <summary>
    /// Returned when the role name contains invalid characters or format.
    /// </summary>
    public const string InvalidRoleName = "ROLE_NAME_INVALID";

    /// <summary>
    /// Returned when the role name exceeds the maximum allowed length.
    /// </summary>
    public const string RoleNameTooLong = "ROLE_NAME_TOO_LONG";

    /// <summary>
    /// Returned when duplicate role names are provided in a batch user-role assignment request.
    /// </summary>
    public const string DuplicateUserRoleInput = "DUPLICATE_USER_ROLE_INPUT";

    // ──── User Role Errors ────

    /// <summary>
    /// Returned when the user role assignment object is null.
    /// </summary>
    public const string UserRoleIsNull = "USERROLE_IS_NULL";

    /// <summary>
    /// Returned when attempting to delete a role that still has users assigned to it.
    /// </summary>
    public const string RoleHasUsers = "ROLE_HAS_USERS";

    /// <summary>
    /// Returned when the ValidFrom date is greater than or equal to the ValidTo date in a role assignment.
    /// </summary>
    public const string ValidFromShouldLessThanValidTo = "VALIDFROM_SHOULD_LESS_THAN_VALIDTO";

    // ──── General User Errors ────

    /// <summary>
    /// Returned when the user ID is required but not provided in the request.
    /// </summary>
    public const string UserIdRequired = "USER_ID_REQUIRED";

    /// <summary>
    /// Returned when the username field is empty or not provided in the request.
    /// </summary>
    public const string
        UserNameMissing = "USERNAME_MISSING"; // Added for conditions where username field is empty or not given

    // ──── Role Claim Errors ────

    /// <summary>
    /// Returned when the role claim data is invalid or malformed.
    /// </summary>
    public const string InvalidRoleClaim = "INVALID_ROLE_CLAIM";

    /// <summary>
    /// Returned when the role claim already exists for the specified role.
    /// </summary>
    public const string RoleClaimExists = "ROLECLAIM_ALREADY_EXISTS";

    /// <summary>
    /// Returned when the role claim does not exist for the specified role.
    /// </summary>
    public const string RoleClaimNotExists = "ROLECLAIM_NOT_EXISTS";

    /// <summary>
    /// Returned when the role claim type is required but not provided.
    /// </summary>
    public const string RoleClaimTypeRequired = "ROLECLAIM_CLAIMTYPE_REQUIRED";

    /// <summary>
    /// Returned when the role claim value is required but not provided.
    /// </summary>
    public const string RoleClaimValueRequired = "ROLECLAIM_CLAIMVALUE_REQUIRED";

    /// <summary>
    /// Returned when the role claim value or claim type combination is invalid.
    /// </summary>
    public const string InvalidRoleClaimValueOrClaimType = "INVALID_ROLE_CLAIMVALUE_OR_CLAIMTYPE";

    /// <summary>
    /// Returned when the CreatedBy field of a role claim exceeds the maximum length.
    /// </summary>
    public const string RoleClaimCreatedByTooLong = "ROLE_CLAIM_CREATEDBY_TOO_LONG";

    /// <summary>
    /// Returned when the ModifiedBy field of a role claim exceeds the maximum length.
    /// </summary>
    public const string RoleClaimModifiedByTooLong = "ROLE_CLAIM_MODIFIEDBY_TOO_LONG";

    /// <summary>
    /// Returned when the role ID in a role claim operation is invalid.
    /// </summary>
    public const string InvalidRoleClaimRoleId = "INVALID_ROLECLAIM_ROLEID";

    /// <summary>
    /// Returned when the role claim ID is invalid or does not exist.
    /// </summary>
    public const string InvalidRoleClaimId = "INVALID_ROLECLAIM_ID";

    // ──── Miscellaneous Errors ────

    /// <summary>
    /// Returned when the logger configuration is null or incomplete.
    /// </summary>
    public const string LoggerConfigurationIsNull = "LOGGER_CONFIGURATION_IS_NULL";

    /// <summary>
    /// Returned when a database SaveChanges operation writes zero rows (no data was modified).
    /// </summary>
    public const string NoChangesWritten = "NO_CHANGES_WRITTEN";

    /// <summary>
    /// Returned when the log file path is invalid or inaccessible.
    /// </summary>
    public const string InvalidLogFilePath = "INVALID_FILEPATH_LOG";

    /// <summary>
    /// Returned (as an informational code) when user login succeeds.
    /// </summary>
    public const string LoginSuccessful = "LOGIN_SUCCESS";

    /// <summary>
    /// Returned when user login fails due to invalid credentials or other authentication errors.
    /// </summary>
    public const string LoginFailed = "LOGIN_FAILED";

    /// <summary>
    /// Returned when the CreatedBy audit field exceeds the maximum allowed length.
    /// </summary>
    public const string CreatedByTooLong = "CREATEDBY_TOO_LONG";

    /// <summary>
    /// Returned when the ModifiedBy audit field exceeds the maximum allowed length.
    /// </summary>
    public const string ModifiedByTooLong = "MODIFIEDBY_TOO_LONG";

    /// <summary>
    /// Returned when the email provider configuration is invalid or incomplete.
    /// </summary>
    public const string InvalidEmailConfiguration = "INVALID_EMAIL_CONFIGURATION";

    /// <summary>
    /// Returned when the SMS provider configuration is invalid or incomplete.
    /// </summary>
    public const string InvalidSmsConfiguration = "INVALID_SMS_CONFIGURATION";
}
