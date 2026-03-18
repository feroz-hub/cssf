/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.ErrorCodes;

/// <summary>
/// Error code constants returned by the OAuth 2.0 / OIDC endpoint layer (authorization, token,
/// introspection, revocation, and client management endpoints). Each constant is a machine-readable
/// error identifier used in error responses and paired with localized messages from resource files.
/// </summary>
public static class EndpointErrorCodes
{
    // ──── Common Errors ────

    /// <summary>
    /// Returned when a query operation finds no matching records.
    /// </summary>
    public const string NoRecordsFound = "NO_RECORDS_FOUND";

    // ──── Client Registration and Management Errors ────

    /// <summary>
    /// Returned when the client name is required but not provided during registration.
    /// </summary>
    public const string ClientNameIsRequired = "CLIENT_NAME_REQUIRED";

    /// <summary>
    /// Returned when the client secret has expired and can no longer be used for authentication.
    /// </summary>
    public const string ClientSecretExpired = "CLIENT_SECRET_EXPIRED";

    /// <summary>
    /// Returned when the client secret provided does not match the stored (hashed) secret.
    /// </summary>
    public const string ClientSecretInvalid = "CLIENT_SECRET_INVALID";

    /// <summary>
    /// Returned when attempting to register a client with a name that already exists.
    /// </summary>
    public const string ClientNameAlreadyExists = "CLIENT_NAME_ALREADY_EXISTS";

    /// <summary>
    /// Returned when the client is found but is marked as inactive/disabled.
    /// </summary>
    public const string InactiveClient = "INACTIVE_CLIENT";

    /// <summary>
    /// Returned when the specified client_id does not exist in the client registry.
    /// </summary>
    public const string ClientDoesNotExist = "CLIENT_DOES_NOT_EXIST";

    /// <summary>
    /// Returned when the client secret expiration date is required but not provided.
    /// </summary>
    public const string ClientSecretExpiresAtRequired = "CLIENTSECRET_EXPIRES_AT_REQUIRED";

    /// <summary>
    /// Returned when the client object passed to a create/update operation is invalid.
    /// </summary>
    public const string InvalidClientObject = "INVALID_CLIENT_OBJECT";

    /// <summary>
    /// Returned when the client query returns no matching records.
    /// </summary>
    public const string NoClientRecordsFound = "NO_CLIENT_RECORDS_FOUND";

    /// <summary>
    /// Returned when the client_id is missing from a client secret generation request.
    /// </summary>
    public const string ClientIdMissingInSecret = "CLIENTID_MISSING_IN_SECRET";

    /// <summary>
    /// Returned when the client_id is missing from the token or authorization request.
    /// </summary>
    public const string ClientIdMissingInRequest = "CLIENTID_MISSING_IN_REQUEST";

    /// <summary>
    /// Returned when the client_id exceeds the maximum allowed length.
    /// </summary>
    public const string ClientIdTooLong = "CLIENT_ID_TOO_LONG";

    /// <summary>
    /// Returned when the client name exceeds the maximum allowed length.
    /// </summary>
    public const string ClientNameTooLong = "CLIENT_NAME_TOO_LONG";

    /// <summary>
    /// Returned when the client is not authorized to use the requested grant type.
    /// </summary>
    public const string ClientNotAuthorizedForGrantType = "CLIENT_NOTAUTHORIZED_GRANT_TYPE";

    /// <summary>
    /// Returned when the client binding data (e.g., certificate thumbprint) is invalid.
    /// </summary>
    public const string InvalidClientBinding = "INVALID_CLIENT_BINDING";

    /// <summary>
    /// Returned when the client does not have a secret configured (required for confidential clients).
    /// </summary>
    public const string ClientSecretNotAvailable = "CLIENT_SECRET_NOTAVAILABLE";

    /// <summary>
    /// Returned when the allowed_scopes list is required but not provided during client registration.
    /// </summary>
    public const string AllowedScopesIsRequired = "ALLOWED_SCOPES_REQUIRED";

    /// <summary>
    /// Returned when the supported grant types list is required but not provided during client registration.
    /// </summary>
    public const string SupportedGrantTypesIsRequired = "SUPPORTED_GRANT_TYPES_REQUIRED";

    /// <summary>
    /// Returned when the supported response types list is required but not provided during client registration.
    /// </summary>
    public const string SupportedResponseTypesIsRequired = "SUPPORTED_RESPONSE_TYPES_REQUIRED";

    /// <summary>
    /// Returned when the client_id is required but not provided.
    /// </summary>
    public const string ClientIdIsRequired = "CLIENT_ID_REQUIRED";

    /// <summary>
    /// Returned when the client_secret is required but not provided.
    /// </summary>
    public const string ClientSecretIsRequired = "CLIENT_SECRET_REQUIRED";

    /// <summary>
    /// Returned when the configured access token expiration is outside the allowed min/max range.
    /// </summary>
    public const string InvalidAccessTokenExpireRange = "INVALID_ACCESS_TOKEN_EXPIRE_RANGE";

    /// <summary>
    /// Returned when the configured identity token expiration is outside the allowed min/max range.
    /// </summary>
    public const string InvalidIdentityTokenExpireRange = "INVALID_IDENTITY_TOKEN_EXPIRE_RANGE";

    /// <summary>
    /// Returned when the configured refresh token expiration is outside the allowed min/max range.
    /// </summary>
    public const string InvalidRefreshTokenExpireRange = "INVALID_REFRESH_TOKEN_EXPIRE_RANGE";

    /// <summary>
    /// Returned when the configured logout token expiration is outside the allowed min/max range.
    /// </summary>
    public const string InvalidLogoutTokenExpireRange = "INVALID_LOGOUT_TOKEN_EXPIRE_RANGE";

    /// <summary>
    /// Returned when the configured authorization code expiration is outside the allowed min/max range.
    /// </summary>
    public const string InvalidAuthorizationCodeExpireRange = "INVALID_AUTHORIZATION_CODE_EXPIRE_RANGE";

    /// <summary>
    /// Returned when the signing algorithm specified for the client is not a valid SigningAlgorithm enum value.
    /// </summary>
    public const string SigningAlgorithmIsInvalid = "SIGNING_ALGORITHM_IS_INVALID";

    /// <summary>
    /// Returned when the ModifiedBy field exceeds the maximum allowed length.
    /// </summary>
    public const string ModifiedByTooLong = "MODIFIEDBY_TOO_LONG";

    // ──── User Authentication Errors (at endpoint level) ────

    /// <summary>
    /// Returned when the username is missing from the Resource Owner Password Credentials request.
    /// </summary>
    public const string UserNameMissing = "USERNAME_MISSING";

    /// <summary>
    /// Returned when the username exceeds the maximum allowed length at the endpoint level.
    /// </summary>
    public const string UserNameTooLong = "USERNAME_TOO_LONG";

    /// <summary>
    /// Returned when the password exceeds the maximum allowed length at the endpoint level.
    /// </summary>
    public const string PasswordTooLong = "PASSWORD_TOO_LONG";

    /// <summary>
    /// Returned when user authentication fails during the Resource Owner Password flow.
    /// </summary>
    public const string UserAuthenticationFailed = "USER_AUTHENTICATION_FAILED";

    /// <summary>
    /// Returned when the subject identifier in the token does not match the expected value.
    /// </summary>
    public const string InvalidSubjectId = "INVALID_SUBJECT_ID";

    /// <summary>
    /// Returned when the user claims are invalid or could not be retrieved.
    /// </summary>
    public const string InvalidUserClaims = "INVALID_USER_CLAIMS";

    /// <summary>
    /// Returned when the user associated with the token no longer exists in the database.
    /// </summary>
    public const string UserDoesNotExist = "USER_DOESNOT_EXIST";

    /// <summary>
    /// Returned when the user has no role assignments and roles are required for token issuance.
    /// </summary>
    public const string NoUserRoleMapped = "NO_USERROLE_MAPPED";

    /// <summary>
    /// Returned when the subject claim value in the token does not match the authenticated user.
    /// </summary>
    public const string SubjectClaimValueMismatch = "SUBJECT_CLAIM_VALUE_MISMATCH";

    /// <summary>
    /// Returned when two-factor authentication is enabled and requires the user to complete a 2FA step.
    /// </summary>
    public const string TwoFactorEnabled = "TWO_FACTOR_ENABLED";

    /// <summary>
    /// Returned when the user account is invalid (e.g., deleted, disabled, or not found).
    /// </summary>
    public const string InvalidUser = "INVALID_USER";

    /// <summary>
    /// Returned when the user's role has no claims mapped (and role claims are required for token generation).
    /// </summary>
    public const string NoUserRoleClaimMapped = "NO_USERROLE_CLAIM_MAPPED";

    // ──── URI Validation Errors ────

    /// <summary>
    /// Returned when the logo_uri provided during client registration is not a valid URI.
    /// </summary>
    public const string InvalidLogoUri = "INVALID_LOGO_URI";

    /// <summary>
    /// Returned when the client_uri provided during client registration is not a valid URI.
    /// </summary>
    public const string InvalidClientUri = "INVALID_CLIENT_URI";

    /// <summary>
    /// Returned when the policy_uri provided during client registration is not a valid URI.
    /// </summary>
    public const string InvalidPolicyUri = "INVALID_POLICY_URI";

    /// <summary>
    /// Returned when the redirect_uri is not a valid URI format.
    /// </summary>
    public const string InvalidRedirectUri = "INVALID_REDIRECT_URI";

    /// <summary>
    /// Returned when the redirect_uri is valid but not registered for the client.
    /// </summary>
    public const string RedirectUriNotRegistered = "REDIRECT_URI_NOT_REGISTERED";

    /// <summary>
    /// Returned when the redirect_uri exceeds the maximum allowed length.
    /// </summary>
    public const string RedirectUriTooLong = "REDIRECT_URI_TOO_LONG";

    /// <summary>
    /// Returned when the redirect_uri is mandatory for the requested grant type but not provided.
    /// </summary>
    public const string RedirectURIIsMandatory = "REDIRECT_URI_IS_MANDATORY";

    /// <summary>
    /// Returned when the post_logout_redirect_uri is not a valid URI format.
    /// </summary>
    public const string InvalidPostLogoutRedirectUri = "INVALID_POSTLOGOUT_REDIRECT_URI";

    /// <summary>
    /// Returned when the post_logout_redirect_uri exceeds the maximum allowed length.
    /// </summary>
    public const string PostRedirectUriTooLong = "POST_REDIRECT_URI_TOO_LONG";

    /// <summary>
    /// Returned when the redirect_uri is missing from the authorization or token request.
    /// </summary>
    public const string RedirectUriMissing = "REDIRECT_URI_MISSING";

    /// <summary>
    /// Returned when the tos_uri (terms of service) is not a valid URI format.
    /// </summary>
    public const string InvalidTermsOfService = "INVALID_TERMSOFSERVICE_URI";

    /// <summary>
    /// Returned when the CreatedBy field of a redirect URI exceeds the maximum length.
    /// </summary>
    public const string RedirectUriCreatedByTooLong = "REDIRECTURI_CREATEDBY_TOO_LONG";

    /// <summary>
    /// Returned when the CreatedBy field of a post-logout redirect URI exceeds the maximum length.
    /// </summary>
    public const string PostRedirectUriCreatedByTooLong = "POSTREDIRECTURI_CREATEDBY_TOO_LONG";

    /// <summary>
    /// Returned when the ModifiedBy field of a redirect URI exceeds the maximum length.
    /// </summary>
    public const string RedirectUriModifiedByTooLong = "REDIRECTURI_MODIFIEDBY_TOO_LONG";

    /// <summary>
    /// Returned when the ModifiedBy field of a post-logout redirect URI exceeds the maximum length.
    /// </summary>
    public const string PostRedirectUriModifiedByTooLong = "POSTREDIRECTURI_MODIFIEDBY_TOO_LONG";

    /// <summary>
    /// Returned when the client ID associated with a redirect URI is invalid.
    /// </summary>
    public const string InvalidRedirectUriClientId = "INVALID_REDIRECTURI_CLIENTID";

    /// <summary>
    /// Returned when the client ID associated with a post-logout redirect URI is invalid.
    /// </summary>
    public const string InvalidPostRedirectUriClientId = "INVALID_POST_REDIRECTURI_CLIENTID";

    /// <summary>
    /// Returned when the client's redirect URI is marked as inactive.
    /// </summary>
    public const string InactiveClientRedirectUri = "INACTIVE_CLIENT_REDIRECTURI";

    /// <summary>
    /// Returned when the client's post-logout redirect URI is marked as inactive.
    /// </summary>
    public const string InactiveClientPostLogoutRedirectUri = "INACTIVE_CLIENT_POSTLOGOUTREDIRECTURI";

    // ──── Token Validation Errors ────

    /// <summary>
    /// Returned when a required token is missing from the request.
    /// </summary>
    public const string TokenMissing = "TOKEN_MISSING";

    /// <summary>
    /// Returned when the token length exceeds the maximum allowed value.
    /// </summary>
    public const string Invalid_Token_Length = "INVALID_TOKEN_LENGTH";

    /// <summary>
    /// Returned when the token format is invalid (e.g., not a valid JWT).
    /// </summary>
    public const string InvalidTokenFormat = "INVALID_TOKEN_FORMAT";

    /// <summary>
    /// Returned when the client is not configured to receive access tokens (missing grant type).
    /// </summary>
    public const string ClientNotConfiguredToReceiveAccessToken = "CLIENT_NOT_CONFIGURED_FOR_ACCESSTOKEN";

    /// <summary>
    /// Returned when token introspection finds the token to be invalid or unrecognized.
    /// </summary>
    public const string InvalidTokenInIntrospection = "INVALID_TOKEN_IN_INTROSPECTION";

    /// <summary>
    /// Returned when the token has expired.
    /// </summary>
    public const string TokenExpired = "TOKEN_EXPIRED";

    /// <summary>
    /// Returned when the token has been revoked and is no longer valid.
    /// </summary>
    public const string TokenRevoked = "TOKEN_REVOKED";

    /// <summary>
    /// Returned when the token_type_hint value is not a recognized token type.
    /// </summary>
    public const string InvalidTokenHintType = "INVALID_TOKEN_HINT_TYPE";

    /// <summary>
    /// Returned when the token type is not supported for the requested operation.
    /// </summary>
    public const string UnsupportedTokenType = "UNSUPPORTED_TOKEN_TYPE";

    /// <summary>
    /// Returned when the specified token could not be found in the token store.
    /// </summary>
    public const string NoTokenFound = "NO_TOKEN_FOUND";

    /// <summary>
    /// Returned when the client is not authorized to request refresh tokens (offline_access scope not allowed).
    /// </summary>
    public const string RefreshTokenRequestNotAllowed = "REFRESH_TOKEN_REQUEST_NOT_ALLOWED";

    /// <summary>
    /// Returned when the refresh_token parameter is missing from a refresh token grant request.
    /// </summary>
    public const string RefreshTokenMissing = "REFRESH_TOKEN_MISSING";

    /// <summary>
    /// Returned when the refresh token value exceeds the maximum allowed length.
    /// </summary>
    public const string RefreshTokenTooLong = "REFRESH_TOKEN_TOO_LONG";

    /// <summary>
    /// Returned when refresh token validation fails (expired, revoked, or invalid).
    /// </summary>
    public const string RefreshTokenValidationFailed = "REFRESH_TOKEN_VALIDATION_FAILED";

    /// <summary>
    /// Returned when the token exceeds the maximum allowed overall length.
    /// </summary>
    public const string TokenMaxLengthExceeded = "TOKEN_MAX_LENGTH_EXCEEDED";

    /// <summary>
    /// Returned when the refresh token does not exist in the token store.
    /// </summary>
    public const string RefreshTokenDoesNotExist = "REFRESHTOKEN_DOESNOT_EXIST";

    /// <summary>
    /// Returned when the token is null, empty, or otherwise invalid.
    /// </summary>
    public const string TokenIsNullOrInvalid = "TOKEN_NULL_OR_INVALID";

    /// <summary>
    /// Returned when the system fails to persist a refresh token to the token store.
    /// </summary>
    public const string FailedToStoreRefreshToken = "FAILED_TO_STORE_REFRESHTOKEN";

    /// <summary>
    /// Returned when the system fails to persist the return URL during the authorization flow.
    /// </summary>
    public const string FailedToStoreReturnUrl = "FAILED_TO_STORE_RETURNURL";

    /// <summary>
    /// Returned when back-channel logout requires a session ID but none is available.
    /// </summary>
    public const string BackChannelRequiredSessionId = "BACK_CHANNEL_REQUIRED_SESSIONID";

    // ──── Authorization Request Validation Errors ────

    /// <summary>
    /// Returned when the response_type parameter is missing from the authorization request.
    /// </summary>
    public const string ResponseTypeMissing = "RESPONSE_TYPE_MISSING";

    /// <summary>
    /// Returned when the response_mode parameter value is not supported.
    /// </summary>
    public const string InvalidResponseMode = "INVALID_RESPONSE_MODE";

    /// <summary>
    /// Returned when the grant type is not valid for the requested endpoint.
    /// </summary>
    public const string InvalidGrantTypeForEndpoint = "INVALID_GRANTTYPE_FOR_ENDPOINT";

    /// <summary>
    /// Returned when the grant type is not registered/allowed for the client.
    /// </summary>
    public const string InvalidGrantTypeForClient = "INVALID_GRANTTYPE_FOR_CLIENT";

    /// <summary>
    /// Returned when the grant_type parameter is missing from the token request.
    /// </summary>
    public const string GrantTypeIsMissing = "GRANT_TYPE_MISSING";

    /// <summary>
    /// Returned when the grant_type parameter exceeds the maximum allowed length.
    /// </summary>
    public const string GrantTypeTooLong = "GRANT_TYPE_TOO_LONG";

    // ──── Scope Validation Errors ────

    /// <summary>
    /// Returned when the scope parameter is missing from the request.
    /// </summary>
    public const string ScopeMissing = "SCOPE_MISSING";

    /// <summary>
    /// Returned when the scope parameter exceeds the maximum allowed length.
    /// </summary>
    public const string ScopeTooLong = "SCOPE_TOO_LONG";

    /// <summary>
    /// Returned when a requested scope is invalid or not registered on the server.
    /// </summary>
    public const string InvalidScopeOrNotAllowed = "INVALIDSCOPE_OR_NOT_ALLOWED";

    /// <summary>
    /// Returned when the response type requires the "openid" scope but it is not included.
    /// </summary>
    public const string ResponseTypeRequiresOpenIdScope = "REQUIRES_OPEN_ID_SCOPE";

    /// <summary>
    /// Returned when the requested scope is not in the client's allowed_scopes list.
    /// </summary>
    public const string RequestedScopeNotAllowedForClient = "REQUEST_SCOPE_NOT_ALLOWED";

    /// <summary>
    /// Returned when the "openid" scope is required for the request but not included.
    /// </summary>
    public const string OpenIdScopeMissing = "OPEN_ID_SCOPE_MISSING";

    /// <summary>
    /// Returned when the request must include identity scopes but none are present.
    /// </summary>
    public const string MustIncludeIdentityScopes = "MUST_INCLUDE_IDENTITY_SCOPES";

    /// <summary>
    /// Returned when the request must not include resource scopes for the given response type.
    /// </summary>
    public const string MustNotIncludeResourceScopes = "MUST_NOT_INCLUDE_RESOURCE_SCOPES";

    /// <summary>
    /// Returned when the request must not include the offline_access scope for the given grant type.
    /// </summary>
    public const string MustNotIncludeOfflineAccessScope = "MUST_NOT_INCLUDE_OFFLINE_ACCESS_SCOPE";

    /// <summary>
    /// Returned when the request must include resource (API) scopes but none are present.
    /// </summary>
    public const string MustIncludeResourceScopes = "MUST_INCLUDE_RESOURCE_SCOPES";

    /// <summary>
    /// Returned when the "openid" scope is not allowed for the client or request context.
    /// </summary>
    public const string OpenIdScopeNotAllowed = "OPEN_ID_SCOPE_NOT_ALLOWED";

    /// <summary>
    /// Returned when the claims associated with the requested scopes are invalid.
    /// </summary>
    public const string InvalidScopeClaims = "INVALID_SCOPE_CLAIMS";

    // ──── PKCE (Proof Key for Code Exchange) Errors ────

    /// <summary>
    /// Returned when the code_challenge parameter is invalid (wrong length or format).
    /// </summary>
    public const string InvalidCodeChallenge = "INVALID_CODE_CHALLENGE";

    /// <summary>
    /// Returned when the client requires PKCE but no code_challenge was provided.
    /// </summary>
    public const string ClientMissingCodeChallenge = "CLIENT_MISSING_CODE_CHALLENGE";

    /// <summary>
    /// Returned when a code_challenge is provided but the code_challenge_method is missing.
    /// </summary>
    public const string ClientMissingCodeChallengeMethod = "CLIENT_MISSING_CODE_CHALLENGE_METHOD";

    /// <summary>
    /// Returned when the code_verifier is missing from the token request (required for PKCE).
    /// </summary>
    public const string CodeVerifierMissing = "CODE_VERIFIER_MISSING";

    /// <summary>
    /// Returned when the code_verifier is shorter than the minimum required length (43 chars per RFC 7636).
    /// </summary>
    public const string CodeVerifierTooShort = "CODE_VERIFIER_TOO_SHORT";

    /// <summary>
    /// Returned when the code_verifier exceeds the maximum allowed length (128 chars per RFC 7636).
    /// </summary>
    public const string CodeVerifierTooLong = "CODE_VERIFIER_TOO_LONG";

    /// <summary>
    /// Returned when the code_challenge_method is not a supported value (only "plain" and "S256" are supported).
    /// </summary>
    public const string UnsupportedCodeChallengeMethod = "UNSUPPORTED_CODE_CHALLENGE_METHOD";

    // ──── Authorization Request Parameter Errors ────

    /// <summary>
    /// Returned when a JWT request URI is provided but the server does not support request_uri.
    /// </summary>
    public const string JwtRequestUriNotSupported = "JWT_REQUEST_URI_NOT_SUPPORTED";

    /// <summary>
    /// Returned when the nonce parameter is invalid (wrong length or format).
    /// </summary>
    public const string InvalidNonce = "INVALID_NONCE";

    /// <summary>
    /// Returned when the prompt parameter value is not a supported prompt mode.
    /// </summary>
    public const string InvalidPrompt = "INVALID_PROMPT";

    /// <summary>
    /// Returned when the max_age parameter is not a valid positive integer.
    /// </summary>
    public const string InvalidMaxAge = "INVALID_MAX_AGE";

    /// <summary>
    /// Returned when the user is not authenticated and the authorize endpoint requires authentication.
    /// </summary>
    public const string UnauthenticatedUser = "UNAUTHENTICATED_USER";

    /// <summary>
    /// Returned when the identity token (id_token_hint) is invalid or cannot be validated.
    /// </summary>
    public const string InvalidIdentityToken = "INVALID_IDENTITY_TOKEN";

    /// <summary>
    /// Returned when the state parameter is invalid or missing.
    /// </summary>
    public const string InvalidState = "INVALID_STATE";

    // ──── Authorization Code Errors ────

    /// <summary>
    /// Returned when the authorization code is missing from the token request.
    /// </summary>
    public const string AuthorizationCodeMissing = "AUTH_CODE_MISSING";

    /// <summary>
    /// Returned when the authorization code exceeds the maximum allowed length.
    /// </summary>
    public const string AuthorizationCodeTooLong = "AUTH_CODE_TOO_LONG";

    /// <summary>
    /// Returned when the authorization code is invalid, already used, or does not match the client.
    /// </summary>
    public const string InvalidAuthorizationCode = "INVALID_AUTH_CODE";

    /// <summary>
    /// Returned when the authorization code has expired (codes are short-lived by design).
    /// </summary>
    public const string AuthorizationCodeExpired = "AUTH_CODE_EXPIRED";

    /// <summary>
    /// Returned when the scopes in the token request do not match the scopes from the authorization code.
    /// </summary>
    public const string AuthorizationCodeScopeError = "AUTH_CODE_SCOPE_ERROR";

    /// <summary>
    /// Returned when the code_verifier does not match the code_challenge from the authorization request.
    /// </summary>
    public const string InvalidCodeVerifier = "INVALID_CODE_VERIFIER";

    // ──── General Endpoint Errors ────

    /// <summary>
    /// Returned when a required argument is null (general programming error).
    /// </summary>
    public const string ArgumentNullError = "ARGUMENT_NULL_ERROR";

    /// <summary>
    /// Returned when the request is invalid (generic validation failure at the endpoint level).
    /// </summary>
    public const string InvalidRequest = "INVALID_REQUEST";

    /// <summary>
    /// Returned when the security token ID is invalid or does not exist.
    /// </summary>
    public const string InvalidSecurityTokenId = "INVALID_SECURITY_TOKEN_ID";

    /// <summary>
    /// Returned when the client secret provided at the endpoint is invalid.
    /// </summary>
    public const string InvalidClientSecret = "INVALID_CLIENT_SECRET";

    /// <summary>
    /// Returned when the token revocation request is malformed or missing required parameters.
    /// </summary>
    public const string InvalidRevocationRequest = "INVALID_REVOCATIONREQUEST";

    /// <summary>
    /// Returned when an invalid or unsupported operation is attempted.
    /// </summary>
    public const string InvalidOperation = "INVALID_OPERATION";

    /// <summary>
    /// Returned when the client_id in the request exceeds the configured maximum length.
    /// </summary>
    public const string ClientIdMaxLengthExceeded = "CLIENTID_MAX_LENGTH_EXCEEDED";

    /// <summary>
    /// Returned when the client_secret in the request exceeds the configured maximum length.
    /// </summary>
    public const string ClientSecretMaxLengthExceeded = "CLIENTSECRET_MAX_LENGTH_EXCEEDED";

    /// <summary>
    /// Returned when the HTTP Authorization header content is malformed or cannot be parsed.
    /// </summary>
    public const string HeaderContentIsNotProper = "HEADER_CONTENT_NOTPROPER";

    /// <summary>
    /// Returned when the HTTP request itself is invalid (e.g., wrong method, missing content type).
    /// </summary>
    public const string InvalidHttpRequest = "INVALID_HTTP_REQUEST";

    /// <summary>
    /// Returned when prompt=none is requested but the user must log in (conflicting requirements).
    /// </summary>
    public const string LoginRequiredAndPromptNone = "LOGINREQUIRED_AND_PROMPT_NONE";
}
