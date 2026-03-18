/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace IntegrationTests.ApiDomainModel;

public static class ApiErrorCodes
{
    // Identity Resources
    public const string IdentityResourceIsNull = "IDENTITYRESOURCE_IS_NULL";
    public const string IdentityResourceIdInvalid = "IDENTITYRESOURCE_ID_INVALID";
    public const string IdentityResourceNameInvalid = "IDENTITYRESOURCE_NAME_INVALID ";
    public const string InvalidOrNullClaims = "INVALID_OR_NULLCLAIMS";
    public const string IdentityResourceNameRequired = "IDENTITYRESOURCE_NAME_REQUIRED";
    public const string IdentityResourceNameTooLong = "IDENTITYRESOURCE_NAME_TOO_LONG";
    public const string IdentityDisplayNameTooLong = "IDENTITYRESOURCE_DISPLAY_NAME_TOO_LONG";
    public const string IdentityResourceAlreadyExists = "IDENTITYRESOURCE_ALREADY_EXISTS";

    // Identity ClaimsINVALID_CERTIFICATE
    public const string IdentityResourceClaimIdInvalid = "IDENTITYRESOURCE_CLAIM_ID_INVALID";
    public const string IdentityResourceClaimAlreadyExists = "IDENTITYRESOURCE_CLAIM_ALREADY_EXISTS";
    public const string IdentityResourceClaimTypeRequired = "IDENTITYRESOURCE_CLAIM_TYPE_REQUIRED";
    public const string IdentityClaimIsNull = "IDENTITYCLAIM_IS_NULL";
    public const string IdentityResourceClaimTypeTooLong = "IDENTITYRESOURCE_CLAIM_TYPE_TOO_LONG";
    public const string IdentityResourceClaimCreatedByTooLong = "IDENTITYRESOURCE_CLAIM_CREATEDBY_TOO_LONG";

    public const string IdentityResourceClaimCreatedbyOrModifiedByTooLong =
        "IDENTITYRESOURCE_CLAIM_CREATEDBY_MODIFIEDBY_TOO_LONG";

    // Common
    public const string NoRecordsFound = "NO_RECORDS_FOUND";
    public const string ConcurrencyFailure = "CONCURRENCY_FAILURE";
    public const string InvalidOrNullObject = "INVALID_OR_NULL_OBJECT";
    public const string UnauthorizedAccess = "UNAUTHORIZED_ACCESS";
    public const string AccessDenied = "ACCESS_DENIED";
    public const string NotFound = "NOT_FOUND";

    // Api Resources
    public const string ApiResourceIsNull = "APIRESOURCE_IS_NULL";
    public const string ApiResourceNameInvalid = "APIRESOURCE_NAME_INVALID";
    public const string ApiResourceIdInvalid = "APIRESOURCE_ID_INVALID";
    public const string ApiResourceAlreadyExists = "APIRESOURCE_ALREADY_EXISTS";
    public const string ApiResourceNameRequired = "APIRESOURCE_NAME_REQUIRED";
    public const string ApiResourceNameTooLong = "APIRESOURCE_NAME_TOO_LONG";
    public const string ApiResourceDisplayNameTooLong = "APIRESOURCE_DISPLAY_NAME_TOO_LONG";

    // Api Resources Claims
    public const string ApiResourceClaimIdInvalid = "APIRESOURCE_CLAIM_ID_INVALID";
    public const string ApiResourceClaimIsNull = "APIRESOURCE_CLAIM_IS_NULL";
    public const string ApiResourceClaimTypeRequired = "APIRESOURCE_CLAIM_TYPE_REQUIRED";
    public const string ApiResourceClaimValueRequired = "APIRESOURCE_CLAIM_VALUE_REQUIRED";
    public const string ApiResourceClaimTypeTooLong = "APIRESOURCE_CLAIM_TYPE_TOO_LONG";
    public const string ApiResourceClaimValueTooLong = "APIRESOURCE_CLAIM_VALUE_TOO_LONG";
    public const string ApiResourceClaimAlreadyExists = "APIRESOURCE_CLAIM_ALREADY_EXISTS";
    public const string InvalidApiResourceClaimTypeOrResourceId = "INVALID_APIRESOURCE_CLAIM_TYPE_OR_RESOURCEID";

    public const string InvalidIdentityResourceClaimTypeOrResourceId =
        "INVALID_IDENTITYRESOURCE_CLAIM_TYPE_OR_RESOURCEID";

    public const string ApiResourceClaimIdIsInvalid = "APIRESOURCE_CLAIM_ID_INVALID";
    public const string ApiResourceClaimCreatedByTooLong = "APIRESOURCE_CLAIM_CREATEDBY_TOO_LONG";

    public const string ApiResourceClaimCreatedbyOrModifiedByTooLong =
        "APIRESOURCE_CLAIM_CREATEDBY_MODIFIEDBY_TOO_LONG";

    // Api Scopes
    public const string ApiScopeIsNull = "APISCOPE_IS_NULL";
    public const string ApiScopeIdInvalid = "APISCOPE_ID_INVALID";
    public const string ApiScopeNameInvalid = "APISCOPE_NAME_INVALID ";
    public const string ApiScopeNameRequired = "APISCOPE_NAME_REQUIRED";

    public const string ApiScopeNameOrDisplayNameOrCreatedbyTooLong =
        "APISCOPE_NAME_OR_DISPLAYNAME_OR_CREATEDBY_TOO_LONG";

    public const string ApiScopeModifiedbyTooLong = "APISCOPE_MODIFIEDBY_TOO_LONG";
    public const string ApiScopeAlreadyExists = "APISCOPE_ALREADY_EXISTS";
    public const string InvalidApiScopeNameOrResourceId = "INVALID_APISCOPE_NAME_OR_RESOURCEID";
    public const string ApiScopeNameTooLong = "APISCOPE_NAME_TOO_LONG";
    public const string ApiScopeDisplayNameTooLong = "APISCOPE_DISPLAYNAME_TOO_LONG";

    // Api Scopes claims
    public const string ApiScopeClaimIdInvalid = "APISCOPE_CLAIM_ID_INVALID";
    public const string ApiScopeClaimIsNull = "APISCOPE_CLAIM_IS_NULL";
    public const string ApiScopeClaimTypeRequired = "APISCOPE_CLAIM_TYPE_REQUIRED";
    public const string ApiScopeClaimTypeOrCreatedbyTooLong = "APISCOPE_CLAIMTYPE_CREATEDBY_TOO_LONG";
    public const string ApiScopeClaimModifiedbyTooLong = "APISCOPE_CLAIM_MODIFIEDBY_TOO_LONG";
    public const string ApiScopeClaimNotFoundForType = "APISCOPE_CLAIM_NOT_FOUND_FOR_TYPE";
    public const string ApiScopeClaimsAlreadyExists = "APISCOPE_CLAIMS_ALREADY_EXISTS";
    public const string InvalidApiScopeClaimTypeOrScopeId = "INVALID_APISCOPE_CLAIM_TYPE_OR_SCOPEID";
    public const string ApiScopeClaimTypeTooLong = "APISCOPE_CLAIM_TYPE_TOO_LONG";
    public const string ApiScopeClaimValueRequired = "APISCOPE_CLAIM_VALUE_REQUIRED";
    public const string ApiScopeClaimValueTooLong = "APISCOPE_CLAIM_VALUE_TOO_LONG";

    // ROP
    public const string Invalid_Rop_Validation_Model = "INVALID_ROP_VALIDATION_MODEL";

    // LDAP
    public const string ConnectionStringInvalid = "CONNECTION_STRING_INVALID";
    public const string LDAPConnectionFailed = "LDAP_CONNECTION_FAILED";
    public const string RestrictedApiForLdapUser = "RESTRICTED_API_FOR_LDAP_USER";

    // Audit
    public const string AuditModelIsNull = "AUDIT_MODEL_IS_NULL";
    public const string InvalidAuditActionType = "INVALID_AUDIT_ACTION_TYPE";
    public const string AuditTableNameTooLong = "AUDIT_TABLENAME_TOO_LONG";
    public const string FromDateGreaterThanToDate = "FROMDATE_GREATER_TODATE";

    // UserClaims
    public const string UserClaimUserIdRequired = "USERCLAIM_USERID_REQUIRED";
    public const string UserClaimTypeRequired = "USERCLAIM_CLAIMTYPE_REQUIRED";
    public const string UserClaimValueRequired = "USERCLAIM_CLAIMVALUE_REQUIRED";
    public const string UserClaimsAlreadyExists = "USER_CLAIMS_EXISTS";
    public const string UserClaimCreatedbyTooLong = "USERCLAIM_CREATEDBY_TOO_LONG";
    public const string UserClaimModifiedbyTooLong = "USERCLAIM_MODIFIEDBY_TOO_LONG";
    public const string InvalidUserClaims = "INVALID_USER_CLAIMS";
    public const string InvalidFromAddress = "INVALID_FROM_ADDRESS";
    public const string InvalidToAddress = "INVALID_TO_ADDRESS";
    public const string InvalidSubject = "INVALID_SUBJECT";
    public const string InvalidTemplateName = "INVALID_TEMPLATENAME";
    public const string NoContentSpecifiedForEmail = "NO_CONTENT_FOR_EMAIL";
    public const string SmtpServerNotConfiguredForEmail = "SMTPSERVER_NOT_CONFIGURED";
    public const string PortNotConfiguredForEmail = "PORT_NOT_CONFIGURED";
    public const string UserNameNotConfiguredForEmail = "USERNAME_MISSING_FOR_EMAIL";
    public const string PasswordNotConfiguredForEmail = "PASSWORD_MISSING_FOR_EMAIL";
    public const string ActivityTooLong = "ACTIVITY_TOO_LONG";

    public const string ToNumberNotConfiguredForSMS = "TONUMBER_MISSING_FOR_SMS";
    public const string FromNumberNotConfiguredForSMS = "FROMNUMBER_MISSING_FOR_SMS";
    public const string NoContentSpecifiedForSMS = "NO_CONTENT_FOR_SMS";
    public const string UserNameNotConfiguredForSMS = "USERNAME_MISSING_FOR_SMS";
    public const string PasswordNotConfiguredForSMS = "PASSWORD_MISSING_FOR_SMS";
    public const string SMSCallbackURLMissing = "SMS_CALLBACK_URL_MISSING";

    // Password
    public const string PasswordPatternNotMatched = "PASSWORD_PATTERN_NOT_MATCHED";
    public const string PasswordRequiredLowerCase = "PASSWORD_REQUIRED_LOWERCASE";
    public const string PasswordRequiredUpperCase = "PASSWORD_REQUIRED_UPPERCASE";
    public const string InvalidPasswordLength = "INVALID_PASSWORD_LENGTH";
    public const string PasswordTooLong = "PASSWORD_TOO_LONG";
    public const string PasswordRequiredNumericValue = "PASSWORD_REQUIRED_NUMERIC";
    public const string PasswordRequiredSpecialCharacters = "PASSWORD_REQUIRED_SPECIALCHAR";
    public const string PasswordContainsSpace = "PASSWORD_CONTAINS_SPACE";
    public const string InvalidCurrentPassword = "INVALID_CURRENTPASSWORD";
    public const string InvalidNewPassword = "INVALID_NEWPASSWORD";
    public const string PasswordMismatch = "PASSWORD_MISMATCH";
    public const string CurrentAndNewPasswordAreSame = "PASSWORD_SAME";
    public const string PasswordFromPreviousList = "PASSWORD_ALREADY_USED";
    public const string PasswordAboutToExpire = "PASSWORD_ABOUT_TO_EXPIRE";

    public const string FetchingAuthenticationUserFailed = "FETCHING_AUTHENTICATION_USER_FAILED";

    public const string AuthenticatorAppVerificationFailed = "AUTHENTICATOR_APP_VERIFICATION_FAILED";
    public const string AuthenticatorResetFailed = "AUTHENTICATOR_APP_RESET_FAILED";
    public const string RecoveryCode2FANotEnabled = "RECOVERYCODE_2FA_NOT_ENABLED";

    public const string InvalidUserId = "INVALID_USERID";
    public const string InvalidClaimsPrincipal = "INVALID_CLAIMS_PRINCIPAL";
    public const string UserModelIsNull = "USER_MODEL_IS_NULL";
    public const string InvalidLockoutEndDate = "INVALID_LOCKOUT_ENDDATE";
    public const string UserAlreadyExists = "USER_EXISTS";
    public const string TemplateDoesNotExists = "TEMPLATE_NOT_EXISTS";
    public const string InvalidTwoFactorType = "INVALID_TWOFACTOR_TYPE";
    public const string InvalidTwoFactorTypeId = "INVALID_TWOFACTOR_TYPE_ID";
    public const string TwoFactorNotEnabledForUser = "TWOFACTOR_NOT_ENABLED";
    public const string InvalidUsername = "INVALID_USERNAME";
    public const string UsernameRequired = "USERNAME_REQUIRED";
    public const string InvalidUserOrPassword = "INVALID_USER_OR_PASSWORD";
    public const string InvalidLengthForUsername = "INVALID_USERNAME_LENGTH";
    public const string FirstnameRequired = "FIRSTNAME_REQUIRED";
    public const string InvalidLengthForFirstName = "INVALID_FIRST_NAME_LENGTH";
    public const string InvalidLengthForLastName = "INVALID_LAST_NAME_LENGTH";
    public const string InvalidEmailFormat = "INVALID_EMAIL_FORMAT";
    public const string InvalidLengthForPhoneNumber = "INVALID_PHONENUMBER_LENGTH";
    public const string LastnameRequired = "LASTNAME_REQUIRED";
    public const string PhonenumberRequired = "PHONENUMBER_REQUIRED";
    public const string InvalidPhonenumber = "INVALID_PHONE_NUMBER";

    // Added new error code for DOB required
    public const string DOBIsRequired = "DOB_REQUIRED";
    public const string InvalidDob = "INVALID_DOB";

    public const string UserIsAlreadyDeleted = "USER_DELETED";
    public const string DefaultPasswordNeedsToChange = "DEFAULT_PASSWORD_NEEDS_TO_CHANGE";

    public const string InvalidClaims = "INVALID_CLAIMS";
    public const string InvalidClaimType = "INVALID_CLAIM_TYPE";
    public const string InvalidClaimValue = "INVALID_CLAIM_VALUE";

    public const string InvalidTwoFactorTokenProvided = "INVALID_TWOFACTOR_TOKEN_PROVIDED";
    public const string InvalidTwoFactorTokenGenerated = "INVALID_TWOFACTOR_TOKEN_GENERATED";
    public const string InvalidEmailConfirmationTokenGenerated = "INVALID_EMAILCONFIRMATION_TOKEN_GENERATED";
    public const string InvalidEmailConfirmationTokenProvided = "INVALID_EMAILCONFIRMATION_TOKEN_PROVIDED";
    public const string InvalidPhoneConfirmationTokenGenerated = "INVALID_PHONECONFIRMATION_TOKEN_GENERATED";
    public const string InvalidPhoneConfirmationTokenProvided = "INVALID_PHONECONFIRMATION_TOKEN_PROVIDED";
    public const string InvalidUserTokenGenerated = "INVALID_USER_TOKEN_GENERATED";
    public const string InvalidUserTokenProvided = "INVALID_USER_TOKEN_PROVIDED";
    public const string InvalidResetTokenProvided = "INVALID_RESET_TOKEN_PROVIDED";
    public const string InvalidResetTokenGenerated = "INVALID_RESET_TOKEN_GENERATED";
    public const string InvalidUserToken = "INVALID_USER_TOKEN";
    public const string InvalidPurpose = "INVALID_PURPOSE";

    //Security questions related
    public const string InvalidSecurityQuestionModel = "INVALID_SECURITYQUESTION_MODEL";
    public const string InvalidSecurityQuestion = "INVALID_SECURITYQUESTION";
    public const string SecurityQuestionAlreadyExists = "SECURITYQUESTION_EXISTS";
    public const string SecurityQuestionNotExists = "SECURITYQUESTION_NOT_EXISTS";
    public const string SecurityQuestionUserAssociated = "SECURITY_QUESTION_USER_ASSOCIATED";
    public const string SecurityQuestionTooLong = "SECURITYQUESTION_TOO_LONG";

    //User Security questions related
    public const string SecurityQuestionAlreadyExistsForUser = "USER_SECURITYQUESTION_EXISTS";
    public const string SecurityQuestionNotExistsForUser = "USER_SECURITYQUESTION_NOT_EXISTS";
    public const string InvalidUserSecurityQuestionModel = "INVALID_USER_SECURITYQUESTION_MODEL";
    public const string AnswerDoesNotMatchForSecurityQuestion = "SECURITYQUESTION_ANSWER_MISMATCH";
    public const string InvalidUserIdForSecurityQuestion = "INVALID_USERID_FOR_SECURITYQUESTION";
    public const string InvalidLengthForUserSecurityAnswer = "INVALID_USER_SECURITYANSWER_LENGTH";
    public const string UserSecurityAnswerIsRequired = "USER_SECURITYANSWER_REQUIRED";
    public const string SecurityQuestionRequired = "SECURITYQUESTION_REQUIRED";
    public const string InvalidSecurityQuestionCount = "INVALID_SECURITYQUESTION_COUNT";
    public const string InvalidSecurityAnswerLength = "INVALID_SECURITYANSWER_LENGTH";
    public const string InvalidSecurityAnswer = "INVALID_SECURITY_ANSWER";
    public const string InvalidSecurityQuestionId = "INVALID_SECURITYQUESTION_ID";
    public const string UniqueSecurityQuestion = "UNIQUE_SECURITYQUESTION";
    public const string UserSecurityQuestionUserIdRequired = "USER_SECURITYQUESTION_USERID_REQUIRED";
    public const string UserSecurityQuestionSecurityIdRequired = "USER_SECURITYQUESTION_SECURITY_ID_REQUIRED";
    public const string SecurityQuestionCreatedbyTooLong = "SECURITYQUESTION_CREATEDBY_TOO_LONG";
    public const string SecurityQuestionModifiedbyTooLong = "SECURITYQUESTION_MODIFIEDBY_TOO_LONG";
    public const string SecurityQuestionAnswerTooLong = "SECURITYQUESTION_ANSWER_TOO_LONG";

    //Lock Unlock user account

    public const string InactiveInSystemUserSecurityQuestion = "INACTIVE_IN_SYSTEM_USER_SECURITYQUESTION";

    public const string PasswordRequired = "PASSWORD_REQUIRED.";
    public const string EmailRequired = "EMAIL_REQUIRED";
    public const string EmailLengthTooLong = "EMAIL_LENGTH_TOO_LONG";
    public const string UserAccountLocked = "USER_ACCOUNT_LOCKED";
    public const string LoginNotAllowedAtThisTime = "LOGIN_RESTRICTED_TIME";
    public const string LoginNotAllowedAtThisWorkStation = "LOGIN_RESTRICTED_WORKSTATION";
    public const string PasswordExpired = "PASSWORD_EXPIRED";

    public const string InvalidLDAPUserName = "INVALID_LDAP_USERNAME";
    public const string InvalidLDAPUserNameOrPassword = "INVALID_LDAP_USERNAME_OR_PASSWORD";
    public const string InvalidLDAPHostname = "INVALID_LDAP_HOSTNAME";
    public const string InvalidLDAPPort = "INVALID_LDAP_PORT";
    public const string LoginFailedMismatchRemainingCount = "LOGIN_REMAINING_ATTEMPTS";
    public const string DuplicateLDAPUserFound = "DUPLICATE_LDAP_USER_FOUND";
    public const string InvalidLDAPConfiguration = "INVALID_LDAP_CONFIGURATION";

    // TODO: This message needs to be corrected based on different context. Pre-sign in check related error message is given in resx. But in
    // all the invoked places, the context varies.
    public const string LoginNotAllowed = "LOGIN_NOT_ALLOWED";

    public const string NoDefaultAuthenticateSchemeFound = "NO_AUTHENTICATION_SCHEME_FOUND";
    public const string NoAuthenticationHandlerConfigured = "NO_AUTHENTICATION_HANDLER_CONFIGURED";

    // Roles related
    public const string RoleIdRequired = "ROLE_ID_REQUIRED";
    public const string RoleNameRequired = "ROLE_NAME_REQUIRED";
    public const string RoleAlreadyExists = "ROLE_ALREADY_EXISTS";
    public const string RoleIsNull = "ROLE_IS_NULL";
    public const string UserRoleAlreadyExists = "USER_ROLE_ALREADY_EXISTS";
    public const string UserRoleNotExists = "USER_ROLE_NOT_EXISTS";
    public const string InvalidRoleId = "INVALID_ROLEID";
    public const string InvalidRoleName = "ROLE_NAME_INVALID";
    public const string RoleNameTooLong = "ROLE_NAME_TOO_LONG";
    public const string DuplicateUserRoleInput = "DUPLICATE_USER_ROLE_INPUT";

    // UserRoles related
    public const string UserRoleIsNull = "USERROLE_IS_NULL";
    public const string RoleHasUsers = "ROLE_HAS_USERS";
    public const string ValidFromShouldLessThanValidTo = "VALIDFROM_SHOULD_LESS_THAN_VALIDTO";

    // General - User Related
    public const string UserIdRequired = "USER_ID_REQUIRED";

    public const string
        UserNameMissing = "USERNAME_MISSING"; // Added for conditions where username field is empty or not given

    // RoleClaims related
    public const string InvalidRoleClaim = "INVALID_ROLE_CLAIM";
    public const string RoleClaimExists = "ROLECLAIM_ALREADY_EXISTS";
    public const string RoleClaimNotExists = "ROLECLAIM_NOT_EXISTS";
    public const string RoleClaimTypeRequired = "ROLECLAIM_CLAIMTYPE_REQUIRED";
    public const string RoleClaimValueRequired = "ROLECLAIM_CLAIMVALUE_REQUIRED";
    public const string InvalidRoleClaimValueOrClaimType = "INVALID_ROLE_CLAIMVALUE_OR_CLAIMTYPE";
    public const string RoleClaimCreatedByTooLong = "ROLE_CLAIM_CREATEDBY_TOO_LONG";
    public const string RoleClaimModifiedByTooLong = "ROLE_CLAIM_MODIFIEDBY_TOO_LONG";
    public const string InvalidRoleClaimRoleId = "INVALID_ROLECLAIM_ROLEID";
    public const string InvalidRoleClaimId = "INVALID_ROLECLAIM_ID";

    public const string LoggerConfigurationIsNull = "LOGGER_CONFIGURATION_IS_NULL";

    public const string NoChangesWritten = "NO_CHANGES_WRITTEN";
    public const string InvalidLogFilePath = "INVALID_FILEPATH_LOG";

    public const string LoginSuccessful = "LOGIN_SUCCESS";
    public const string LoginFailed = "LOGIN_FAILED";
    public const string CreatedByTooLong = "CREATEDBY_TOO_LONG";
    public const string ModifiedByTooLong = "MODIFIEDBY_TOO_LONG";

    public const string InvalidEmailConfiguration = "INVALID_EMAIL_CONFIGURATION";
    public const string InvalidSmsConfiguration = "INVALID_SMS_CONFIGURATION";
}
