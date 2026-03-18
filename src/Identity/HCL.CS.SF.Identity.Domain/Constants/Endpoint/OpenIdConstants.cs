/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Constants.Endpoint;

/// <summary>
/// OpenID Connect and OAuth 2.0 protocol constants. Contains all standard claim types,
/// endpoint route paths, error codes, grant types, response types, response modes,
/// token types, algorithm identifiers, and request parameter names as defined in the
/// OAuth 2.0 (RFC 6749), OIDC Core, and related specifications.
/// </summary>
public static class OpenIdConstants
{
    /// <summary>
    /// Standard and custom JWT claim type constants used in access tokens, identity tokens,
    /// and user profile responses. Includes both OIDC-defined claims and framework-specific claims.
    /// </summary>
    public static class ClaimTypes
    {
        /// <summary>
        /// The user's display name claim (maps to System.Security.Claims.ClaimTypes.Name).
        /// </summary>
        public const string Name = System.Security.Claims.ClaimTypes.Name;

        /// <summary>
        /// The user's family/last name claim (maps to System.Security.Claims.ClaimTypes.Surname).
        /// </summary>
        public const string FamilyName = System.Security.Claims.ClaimTypes.Surname;

        /// <summary>
        /// The user's given/first name claim (maps to System.Security.Claims.ClaimTypes.GivenName).
        /// </summary>
        public const string GivenName = System.Security.Claims.ClaimTypes.GivenName;

        /// <summary>
        /// The user's gender claim (maps to System.Security.Claims.ClaimTypes.Gender).
        /// </summary>
        public const string Gender = System.Security.Claims.ClaimTypes.Gender;

        /// <summary>
        /// The user's postal/pin code claim (maps to System.Security.Claims.ClaimTypes.PostalCode).
        /// </summary>
        public const string PinCode = System.Security.Claims.ClaimTypes.PostalCode;

        /// <summary>
        /// The user's date of birth claim (maps to System.Security.Claims.ClaimTypes.DateOfBirth).
        /// </summary>
        public const string DateOfBirth = System.Security.Claims.ClaimTypes.DateOfBirth;

        /// <summary>
        /// The user's email address claim (maps to System.Security.Claims.ClaimTypes.Email).
        /// </summary>
        public const string Email = System.Security.Claims.ClaimTypes.Email;

        /// <summary>
        /// The user's street address claim (maps to System.Security.Claims.ClaimTypes.StreetAddress).
        /// </summary>
        public const string Street = System.Security.Claims.ClaimTypes.StreetAddress;

        /// <summary>
        /// The subject identifier claim ("sub") - the unique user identifier in OIDC.
        /// </summary>
        public const string Sub = "sub";

        /// <summary>
        /// The username claim used internally by the framework.
        /// </summary>
        public const string UserName = "username";

        /// <summary>
        /// The OIDC preferred username claim displayed to the end user.
        /// </summary>
        public const string PreferredUserName = "preferred_username";

        /// <summary>
        /// Custom claim indicating whether the user's email address has been confirmed.
        /// </summary>
        public const string EmailConfirmed = "email_confirmed";

        /// <summary>
        /// Standard OIDC claim indicating whether the user's email address has been verified.
        /// </summary>
        public const string EmailVerified = "email_verified";

        /// <summary>
        /// The user's phone number claim.
        /// </summary>
        public const string PhoneNumber = "phone_number";

        /// <summary>
        /// Custom claim indicating whether the user's phone number has been confirmed.
        /// </summary>
        public const string PhoneNumberConfirmed = "phone_number_confirmed";

        /// <summary>
        /// Standard OIDC claim indicating whether the user's phone number has been verified.
        /// </summary>
        public const string PhoneNumberVerified = "phone_number_verified";

        /// <summary>
        /// The user's address claim (JSON object containing street, city, etc.).
        /// </summary>
        public const string Address = "address";

        /// <summary>
        /// The user's middle name claim.
        /// </summary>
        public const string MiddleName = "middlename";

        /// <summary>
        /// The user's nickname claim.
        /// </summary>
        public const string NickName = "nickname";

        /// <summary>
        /// The user's birthdate claim (date string).
        /// </summary>
        public const string Birthdate = "birthdate";

        /// <summary>
        /// The user's city claim (custom, non-standard).
        /// </summary>
        public const string City = "city";

        /// <summary>
        /// The user's last name claim (custom, non-standard).
        /// </summary>
        public const string LastName = "lastname";

        /// <summary>
        /// The user's first name claim (custom, non-standard).
        /// </summary>
        public const string FirstName = "firstname";

        /// <summary>
        /// The authentication methods references ("amr") claim indicating how the user was authenticated.
        /// </summary>
        public const string AuthenticationMethod = "amr";

        /// <summary>
        /// The session identifier ("sid") claim linking the token to an authentication session.
        /// </summary>
        public const string SessionId = "sid";

        /// <summary>
        /// The authentication time ("auth_time") claim indicating when the user last authenticated.
        /// </summary>
        public const string AuthenticationTime = "auth_time";

        /// <summary>
        /// The authorized party ("azp") claim identifying the client to which the token was issued.
        /// </summary>
        public const string AuthorizedParty = "azp";

        /// <summary>
        /// The access token hash ("at_hash") claim used to bind an ID token to an access token.
        /// </summary>
        public const string AccessTokenHash = "at_hash";

        /// <summary>
        /// The authorization code hash ("c_hash") claim used to bind an ID token to an authorization code.
        /// </summary>
        public const string AuthorizationCodeHash = "c_hash";

        /// <summary>
        /// The nonce claim used to associate a client session with an ID token and mitigate replay attacks.
        /// </summary>
        public const string Nonce = "nonce";

        /// <summary>
        /// The JWT ID ("jti") claim providing a unique identifier for the token.
        /// </summary>
        public const string JwtId = "jti";

        /// <summary>
        /// The client identifier ("client_id") claim identifying the OAuth client that requested the token.
        /// </summary>
        public const string ClientId = "client_id";

        /// <summary>
        /// The scope claim listing the scopes granted in the access token.
        /// </summary>
        public const string Scope = "scope";

        /// <summary>
        /// The actor ("act") claim used in token exchange scenarios to represent delegation.
        /// </summary>
        public const string Actor = "act";

        /// <summary>
        /// A generic identifier claim used internally.
        /// </summary>
        public const string Id = "id";

        /// <summary>
        /// The identity provider ("idp") claim indicating which provider authenticated the user.
        /// </summary>
        public const string IdentityProvider = "idp";

        /// <summary>
        /// Custom claim type for the user's role (used in role-based access control).
        /// </summary>
        public const string UserRole = "userrole";

        /// <summary>
        /// Standard role claim type for RBAC (role-based access control).
        /// </summary>
        public const string Role = "role";

        /// <summary>
        /// Claim type for reference token identifiers (used when tokens are stored server-side).
        /// </summary>
        public const string ReferenceTokenId = "reference_token_id";

        /// <summary>
        /// The "issued at" ("iat") claim indicating when the token was created (Unix timestamp).
        /// </summary>
        public const string IssuedAt = "iat";

        /// <summary>
        /// The "expiration" ("exp") claim indicating when the token expires (Unix timestamp).
        /// </summary>
        public const string Expiration = "exp";

        /// <summary>
        /// The "not before" ("nbf") claim indicating the earliest time the token is valid (Unix timestamp).
        /// </summary>
        public const string NotBefore = "nbf";

        /// <summary>
        /// The "audience" ("aud") claim identifying the intended recipient(s) of the token.
        /// </summary>
        public const string Audience = "aud";

        /// <summary>
        /// The "issuer" ("iss") claim identifying the authorization server that issued the token.
        /// </summary>
        public const string Issuer = "iss";

        /// <summary>
        /// Custom claim type for fine-grained API permissions (e.g., "HCL.CS.SF.user.read").
        /// </summary>
        public const string Permission = "permission";

        /// <summary>
        /// Custom claim type for transaction identifiers in token exchange flows.
        /// </summary>
        public const string Transaction = "transaction";

        /// <summary>
        /// The "events" claim used in logout tokens to indicate the event type (e.g., back-channel logout).
        /// </summary>
        public const string Events = "events";

        /// <summary>Custom claim type for a dedicated capabilities list in the access token (e.g. "capabilities": ["read:users", "write:orders"]).</summary>
        public const string Capabilities = "capabilities";
    }

    /// <summary>
    /// OAuth 2.0 client authentication method string constants used at the token endpoint.
    /// </summary>
    public static class AuthenticationMethods
    {
        /// <summary>
        /// HTTP Basic authentication using client_id and client_secret in the Authorization header.
        /// </summary>
        public const string ClientSecretBasic = "client_secret_basic";

        /// <summary>
        /// POST body authentication with client_id and client_secret as form parameters.
        /// </summary>
        public const string ClientSecretPost = "client_secret_post";

        /// <summary>
        /// JWT-based authentication where the client signs a JWT assertion with its client secret.
        /// </summary>
        public const string ClientSecretJwt = "client_secret_jwt";
    }

    /// <summary>
    /// URL path constants for OAuth 2.0 / OIDC server endpoints.
    /// All paths are prefixed with the "security" path segment.
    /// </summary>
    public static class EndpointRoutePaths
    {
        /// <summary>
        /// The common path prefix for all OAuth/OIDC endpoint routes.
        /// </summary>
        public const string ServerPathPrefix = "security";

        /// <summary>
        /// The authorization endpoint path where clients initiate the Authorization Code flow.
        /// </summary>
        public const string Authorize = ServerPathPrefix + "/authorize";

        /// <summary>
        /// The authorization callback endpoint path used after user authentication to resume the flow.
        /// </summary>
        public const string AuthorizeCallback = Authorize + "/authorizecallback";

        /// <summary>
        /// The OpenID Connect Discovery document endpoint path (/.well-known/openid-configuration).
        /// </summary>
        public const string DiscoveryConfiguration = "/.well-known/openid-configuration";

        /// <summary>
        /// The JWKS (JSON Web Key Set) endpoint path for exposing public signing keys.
        /// </summary>
        public const string JWKSWebKeys = DiscoveryConfiguration + "/jwks";

        /// <summary>
        /// The token endpoint path where clients exchange grants for tokens.
        /// </summary>
        public const string Token = ServerPathPrefix + "/token";

        /// <summary>
        /// The token revocation endpoint path (RFC 7009).
        /// </summary>
        public const string Revocation = ServerPathPrefix + "/revocation";

        /// <summary>
        /// The UserInfo endpoint path that returns claims about the authenticated user.
        /// </summary>
        public const string UserInfo = ServerPathPrefix + "/userinfo";

        /// <summary>
        /// The token introspection endpoint path (RFC 7662).
        /// </summary>
        public const string Introspection = ServerPathPrefix + "/introspect";

        /// <summary>
        /// The end-session (logout) endpoint path for RP-Initiated Logout.
        /// </summary>
        public const string EndSession = ServerPathPrefix + "/endsession";

        /// <summary>
        /// The end-session callback endpoint path invoked after the user confirms logout.
        /// </summary>
        public const string EndSessionCallback = EndSession + "/callback";

        /// <summary>
        /// The device authorization endpoint path (RFC 8628) for device code flow.
        /// </summary>
        public const string DeviceAuthorization = ServerPathPrefix + "/deviceauthorization";

        /// <summary>
        /// The dynamic client registration endpoint path.
        /// </summary>
        public const string Register = ServerPathPrefix + "/register";

        /// <summary>
        /// The subset of endpoint paths that require CORS headers for cross-origin browser requests.
        /// </summary>
        public static readonly string[] CorsPaths =
        {
            DiscoveryConfiguration,
            JWKSWebKeys,
            Token,
            UserInfo,
            Revocation
        };
    }

    /// <summary>
    /// Human-readable endpoint name constants used in logging and error messages.
    /// </summary>
    public static class EndpointsName
    {
        /// <summary>
        /// Display name for the Authorization endpoint.
        /// </summary>
        public const string Authorize = "Authorize";

        /// <summary>
        /// Display name for the Token endpoint.
        /// </summary>
        public const string Token = "Token";

        /// <summary>
        /// Display name for the Introspection endpoint.
        /// </summary>
        public const string Introspection = "Introspection";

        /// <summary>
        /// Display name for the Revocation endpoint.
        /// </summary>
        public const string Revocation = "Revocation";

        /// <summary>
        /// Display name for the UserInfo endpoint.
        /// </summary>
        public const string UserInfo = "Userinfo";

        /// <summary>
        /// Display name for the Discovery endpoint.
        /// </summary>
        public const string Discovery = "Discovery";

        /// <summary>
        /// Display name for the End Session endpoint.
        /// </summary>
        public const string EndSession = "Endsession";
    }

    /// <summary>
    /// Parameter names for the OIDC end-session (logout) request as defined in the OIDC RP-Initiated Logout spec.
    /// </summary>
    public static class EndSessionRequest
    {
        /// <summary>
        /// The previously issued ID token passed as a hint about the user's identity.
        /// </summary>
        public const string IdTokenHint = "id_token_hint";

        /// <summary>
        /// The URI to which the user is redirected after logout.
        /// </summary>
        public const string PostLogoutRedirectUri = "post_logout_redirect_uri";

        /// <summary>
        /// An opaque value passed through to the post-logout redirect URI.
        /// </summary>
        public const string State = "state";

        /// <summary>
        /// The session identifier used for session-based logout.
        /// </summary>
        public const string Sid = "sid";

        /// <summary>
        /// The issuer identifier of the authorization server.
        /// </summary>
        public const string Issuer = "iss";
    }

    /// <summary>
    /// Parameter names for the token revocation endpoint request (RFC 7009).
    /// </summary>
    public static class RevocationRequest
    {
        /// <summary>
        /// The token to be revoked.
        /// </summary>
        public const string Token = "token";

        /// <summary>
        /// A hint about the type of token being revoked (e.g., "access_token" or "refresh_token").
        /// </summary>
        public const string TokenTypeHint = "token_type_hint";
    }

    /// <summary>
    /// Standard OAuth 2.0 and OIDC error code constants returned in error responses.
    /// These values are used in the "error" field of error responses from the authorization
    /// and token endpoints, as defined in RFC 6749 and the OIDC Core specification.
    /// </summary>
    public static class Errors
    {
        /// <summary>
        /// The request is missing a required parameter, includes an invalid parameter value,
        /// or is otherwise malformed.
        /// </summary>
        public const string InvalidRequest = "invalid_request";

        /// <summary>
        /// Client authentication failed (e.g., unknown client, no client authentication included,
        /// or unsupported authentication method).
        /// </summary>
        public const string InvalidClient = "invalid_client";

        /// <summary>
        /// The provided authorization grant or refresh token is invalid, expired, or revoked.
        /// </summary>
        public const string InvalidGrant = "invalid_grant";

        /// <summary>
        /// The authenticated client is not authorized to use this grant type.
        /// </summary>
        public const string UnauthorizedClient = "unauthorized_client";

        /// <summary>
        /// The authorization grant type is not supported by the authorization server.
        /// </summary>
        public const string UnsupportedGrantType = "unsupported_grant_type";

        /// <summary>
        /// The authorization server does not support the requested response type.
        /// </summary>
        public const string UnsupportedResponseType = "unsupported_response_type";

        /// <summary>
        /// The requested signing or encryption algorithm is not supported.
        /// </summary>
        public const string UnsupportedAlgorithm = "unsupported_algorithm";

        /// <summary>
        /// The authorization server does not support the revocation of the presented token type.
        /// </summary>
        public const string UnsupportedTokenType = "unsupported_token_type";

        /// <summary>
        /// The requested scope is invalid, unknown, or exceeds the scope granted by the resource owner.
        /// </summary>
        public const string InvalidScope = "invalid_scope";

        /// <summary>
        /// The authorization request is still pending (device code flow).
        /// </summary>
        public const string AuthorizationPending = "authorization_pending";

        /// <summary>
        /// The resource owner or authorization server denied the request.
        /// </summary>
        public const string AccessDenied = "access_denied";

        /// <summary>
        /// The client is polling too frequently (device code flow rate limiting).
        /// </summary>
        public const string SlowDown = "slow_down";

        /// <summary>
        /// The token has expired and is no longer valid.
        /// </summary>
        public const string ExpiredToken = "token_expired";

        /// <summary>
        /// The target resource or audience is invalid.
        /// </summary>
        public const string InvalidTarget = "invalid_target";

        /// <summary>
        /// The requested endpoint is invalid or not available.
        /// </summary>
        public const string InvalidEndpoint = "invalid_endpoint";

        /// <summary>
        /// The access token provided is expired, revoked, malformed, or invalid.
        /// </summary>
        public const string InvalidToken = "invalid_token";

        /// <summary>
        /// The authorization server does not support the use of the request_uri parameter.
        /// </summary>
        public const string RequestUriNotSupported = "request_uri_not_supported";

        /// <summary>
        /// The authorization server does not support the use of the request parameter.
        /// </summary>
        public const string RequestNotSupported = "request_not_supported";

        /// <summary>
        /// The request object (JWT) is invalid or could not be validated.
        /// </summary>
        public const string InvalidRequestObject = "invalid_request_object";

        /// <summary>
        /// The request_uri in the authorization request could not be fetched or is invalid.
        /// </summary>
        public const string InvalidRequestUri = "invalid_request_uri";

        /// <summary>
        /// The authorization server requires user consent which was not provided.
        /// </summary>
        public const string ConsentRequired = "consent_required";

        /// <summary>
        /// The user must select an account (multiple sessions exist).
        /// </summary>
        public const string AccountSelectionRequired = "account_selection_required";

        /// <summary>
        /// Dynamic client registration is not supported by this authorization server.
        /// </summary>
        public const string RegistrationNotSupported = "registration_not_supported";

        /// <summary>
        /// The authorization server requires the user to authenticate (prompt=none was requested
        /// but no active session exists).
        /// </summary>
        public const string LoginRequired = "login_required";

        /// <summary>
        /// The authorization server is currently unable to handle the request due to temporary overload
        /// or maintenance.
        /// </summary>
        public const string TemporarilyUnavailable = "temporarily_unavailable";

        /// <summary>
        /// The authorization server encountered an unexpected condition that prevented it
        /// from fulfilling the request.
        /// </summary>
        public const string ServerError = "server_error";

        /// <summary>
        /// The authorization server requires interaction with the user that was not possible
        /// (e.g., prompt=none with an expired session).
        /// </summary>
        public const string InteractionRequired = "interaction_required";

        /// <summary>
        /// The request requires higher privileges (scopes) than provided by the access token.
        /// </summary>
        public const string InsufficientScope = "insufficient_scope";

        /// <summary>
        /// The request or token format is invalid.
        /// </summary>
        public const string InvalidFormat = "invalid_format";
    }

    /// <summary>
    /// Maps OAuth 2.0 / OIDC error codes to their corresponding HTTP status codes.
    /// Used to set the appropriate HTTP response status when returning error responses.
    /// </summary>
    public static class HTTPStatusCodes
    {
        /// <summary>HTTP 200 - Successful request.</summary>
        public const int success = 200;
        /// <summary>HTTP 400 - The request is invalid or malformed.</summary>
        public const int invalid_request = 400;
        /// <summary>HTTP 401 - The provided token is invalid or expired.</summary>
        public const int invalid_token = 401;
        /// <summary>HTTP 400 - The client is not authorized for the requested operation.</summary>
        public const int unauthorized_client = 400;
        /// <summary>HTTP 400 - Access to the resource was denied.</summary>
        public const int access_denied = 400;
        /// <summary>HTTP 400 - The requested response type is not supported.</summary>
        public const int unsupported_response_type = 400;
        /// <summary>HTTP 400 - The requested scope is invalid.</summary>
        public const int invalid_scope = 400;
        /// <summary>HTTP 400 - An internal server error occurred during request processing.</summary>
        public const int server_error = 400;
        /// <summary>HTTP 400 - The server is temporarily unavailable.</summary>
        public const int temporarily_unavailable = 400;
        /// <summary>HTTP 400 - Generic bad request.</summary>
        public const int bad_request = 400;
        /// <summary>HTTP 401 - Client authentication failed.</summary>
        public const int invalid_client = 401;
        /// <summary>HTTP 401 - The request requires authentication.</summary>
        public const int unauthorized = 401;
        /// <summary>HTTP 400 - The authorization grant is invalid.</summary>
        public const int invalid_grant = 400;
        /// <summary>HTTP 401 - The token has expired.</summary>
        public const int token_expired = 401;
        /// <summary>HTTP 401 - The redirect URI callback is invalid.</summary>
        public const int invalid_callback = 401;
        /// <summary>HTTP 401 - The client secret provided is invalid.</summary>
        public const int invalid_client_secret = 401;
        /// <summary>HTTP 400 - The requested grant type is not supported.</summary>
        public const int unsupported_grant_type = 400;
        /// <summary>HTTP 400 - The presented token type is not supported for revocation.</summary>
        public const int unsupported_token_type = 400;
        /// <summary>HTTP 403 - The access token does not have sufficient scopes.</summary>
        public const int insufficient_Scope = 403;
        /// <summary>HTTP 404 - The requested resource was not found.</summary>
        public const int not_found = 404;

        /// <summary>
        /// Resolves an OAuth 2.0 error code string to its corresponding HTTP status code.
        /// Returns 400 (Bad Request) for unrecognized error codes.
        /// </summary>
        /// <param name="openIdErrorCode">The OAuth/OIDC error code string.</param>
        /// <returns>The HTTP status code corresponding to the error.</returns>
        public static int GetHttpStatusCode(string openIdErrorCode)
        {
            var errorStatusCode = openIdErrorCode switch
            {
                "invalid_request" => invalid_request,
                "invalid_token" => invalid_token,
                "unauthorized_client" => unauthorized_client,
                "access_denied" => access_denied,
                "unsupported_response_type" => unsupported_response_type,
                "invalid_scope" => invalid_scope,
                "server_error" => server_error,
                "temporarily_unavailable" => temporarily_unavailable,
                "invalid_client" => invalid_client,
                "invalid_grant" => invalid_grant,
                "token_expired" => token_expired,
                "invalid_callback" => invalid_callback,
                "invalid_client_secret" => invalid_client_secret,
                "unsupported_grant_type" => unsupported_grant_type,
                "unsupported_token_type" => unsupported_token_type,
                "insufficient_scope" => insufficient_Scope,
                "sucess" => success,
                _ => invalid_request
            };
            return errorStatusCode;
        }
    }

    /// <summary>
    /// OAuth 2.0 grant type string constants as used in the "grant_type" token request parameter.
    /// </summary>
    public static class GrantTypes
    {
        /// <summary>
        /// Resource Owner Password Credentials grant type.
        /// </summary>
        public const string Password = "password";

        /// <summary>
        /// Authorization Code grant type.
        /// </summary>
        public const string AuthorizationCode = "authorization_code";

        /// <summary>
        /// Client Credentials grant type for machine-to-machine authentication.
        /// </summary>
        public const string ClientCredentials = "client_credentials";

        /// <summary>
        /// Refresh Token grant type for obtaining new access tokens using a refresh token.
        /// </summary>
        public const string RefreshToken = "refresh_token";

        /// <summary>Exchange external sign-in verification code (e.g. after Google) for tokens.</summary>
        public const string UserCode = "user_code";
        //public const string Saml2Bearer = "urn:ietf:params:oauth:grant-type:saml2-bearer";
        //public const string JwtBearer = "urn:ietf:params:oauth:grant-type:jwt-bearer";
        //public const string TokenExchange = "urn:ietf:params:oauth:grant-type:token-exchange";
    }

    /// <summary>
    /// Token response field name constants returned in the JSON body of token endpoint responses.
    /// </summary>
    public static class TokenResponseType
    {
        /// <summary>
        /// The "access_token" response field containing the issued access token.
        /// </summary>
        public const string AccessToken = "access_token";

        /// <summary>
        /// The "expires_in" response field indicating the token lifetime in seconds.
        /// </summary>
        public const string ExpiresIn = "expires_in";

        /// <summary>
        /// The "token_type" response field (typically "Bearer").
        /// </summary>
        public const string TokenType = "token_type";

        /// <summary>
        /// The "refresh_token" response field containing the issued refresh token.
        /// </summary>
        public const string RefreshToken = "refresh_token";

        /// <summary>
        /// The "id_token" response field containing the issued identity token.
        /// </summary>
        public const string IdentityToken = "id_token";

        /// <summary>
        /// The "error" response field containing the error code when the request fails.
        /// </summary>
        public const string Error = "error";

        /// <summary>
        /// The "error_description" response field containing a human-readable error description.
        /// </summary>
        public const string ErrorDescription = "error_description";

        /// <summary>
        /// The standard Bearer token type value.
        /// </summary>
        public const string BearerTokenType = "Bearer";

        /// <summary>
        /// The "issued_token_type" response field used in token exchange responses.
        /// </summary>
        public const string IssuedTokenType = "issued_token_type";

        /// <summary>
        /// The "scope" response field listing the scopes associated with the issued token.
        /// </summary>
        public const string Scope = "scope";
    }

    /// <summary>
    /// OAuth 2.0 response mode constants that control how the authorization response is delivered to the client.
    /// </summary>
    public static class ResponseModes
    {
        /// <summary>
        /// The authorization response parameters are sent via an HTML form POST to the redirect URI.
        /// </summary>
        public const string FormPost = "form_post";

        /// <summary>
        /// The authorization response parameters are appended to the redirect URI as query string parameters.
        /// </summary>
        public const string Query = "query";

        /// <summary>
        /// The authorization response parameters are appended to the redirect URI as a URI fragment.
        /// </summary>
        public const string Fragment = "fragment";
    }

    /// <summary>
    /// OAuth 2.0 response type constants used in the "response_type" authorization request parameter.
    /// </summary>
    public static class ResponseTypes
    {
        /// <summary>
        /// Authorization Code response type - returns an authorization code.
        /// </summary>
        public const string Code = "code";

        /// <summary>
        /// Implicit grant access token response type.
        /// </summary>
        public const string Token = "token";

        /// <summary>
        /// Implicit grant ID token response type.
        /// </summary>
        public const string IdToken = "id_token";

        /// <summary>
        /// Hybrid response type returning both an ID token and an access token.
        /// </summary>
        public const string IdTokenToken = "id_token token";

        /// <summary>
        /// Hybrid response type returning both an authorization code and an ID token.
        /// </summary>
        public const string CodeIdToken = "code id_token";

        /// <summary>
        /// Hybrid response type returning both an authorization code and an access token.
        /// </summary>
        public const string CodeToken = "code token";

        /// <summary>
        /// Hybrid response type returning an authorization code, ID token, and access token.
        /// </summary>
        public const string CodeIdTokenToken = "code id_token token";
    }

    /// <summary>
    /// Token type identifier constants used in token storage, introspection, and revocation.
    /// </summary>
    public static class TokenType
    {
        /// <summary>
        /// Access token type identifier.
        /// </summary>
        public const string AccessToken = "access_token";

        /// <summary>
        /// Identity token (id_token) type identifier.
        /// </summary>
        public const string IdentityToken = "id_token";

        /// <summary>
        /// Refresh token type identifier.
        /// </summary>
        public const string RefreshToken = "refresh_token";

        /// <summary>
        /// Authorization code type identifier.
        /// </summary>
        public const string AuthorizationCode = "authorization_code";

        /// <summary>
        /// Verification code type identifier (used for external sign-in flows).
        /// </summary>
        public const string VerificationCode = "verification_code";

        /// <summary>
        /// Initial access token type identifier (used for dynamic client registration).
        /// </summary>
        public const string InitialAccessToken = "initial_access_token";

        /// <summary>
        /// Request parameter type identifier (used for JWT-secured authorization requests).
        /// </summary>
        public const string RequestParameter = "request_parameter";

        /// <summary>
        /// Logout token type identifier (used for back-channel logout).
        /// </summary>
        public const string LogoutToken = "logout_token";
    }

    /// <summary>
    /// Client assertion type URN constants for JWT-based client authentication.
    /// </summary>
    public static class ClientAssertionTypes
    {
        /// <summary>
        /// The JWT Bearer client assertion type URN as defined in RFC 7523.
        /// </summary>
        public const string JwtBearer = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";
    }

    /// <summary>
    /// PKCE (Proof Key for Code Exchange) code challenge method constants as defined in RFC 7636.
    /// </summary>
    public static class CodeChallengeMethods
    {
        /// <summary>
        /// Plain code challenge method - the code_verifier is sent as-is (not recommended for production).
        /// </summary>
        public const string Plain = "plain";

        /// <summary>
        /// S256 code challenge method - the code_verifier is hashed with SHA-256 and Base64url-encoded.
        /// This is the recommended method.
        /// </summary>
        public const string Sha256 = "S256";
    }

    /// <summary>
    /// Authentication method reference ("amr") claim values indicating how the user was authenticated.
    /// </summary>
    public static class UserAuthenticationMethods
    {
        /// <summary>
        /// The user authenticated with a password.
        /// </summary>
        public const string Password = "pwd";

        /// <summary>
        /// The user completed multi-factor authentication.
        /// </summary>
        public const string MultiFactorAuthentication = "mfa";

        /// <summary>
        /// The user authenticated with a one-time password (OTP).
        /// </summary>
        public const string OneTimePassword = "otp";
    }

    /// <summary>
    /// Parameter names for the OAuth 2.0 authorization request as defined in RFC 6749 and OIDC Core.
    /// </summary>
    public static class AuthorizeRequest
    {
        /// <summary>
        /// The requested scope(s) for the authorization.
        /// </summary>
        public const string Scope = "scope";

        /// <summary>
        /// The PKCE code challenge method (e.g., "S256" or "plain").
        /// </summary>
        public const string CodeChallengeMethod = "code_challenge_method";

        /// <summary>
        /// The PKCE code challenge derived from the code verifier.
        /// </summary>
        public const string CodeChallenge = "code_challenge";

        //public const string AcrValues = "acr_values";
        //public const string LoginHint = "login_hint";
        //public const string IdTokenHint = "id_token_hint";
        //public const string UiLocales = "ui_locales";

        /// <summary>
        /// The maximum authentication age (in seconds). If the user's session is older, re-authentication is required.
        /// </summary>
        public const string MaxAge = "max_age";

        /// <summary>
        /// The prompt parameter controlling whether the user is prompted for re-authentication or account selection.
        /// </summary>
        public const string Prompt = "prompt";

        //public const string Display = "display";

        /// <summary>
        /// A random value used to associate the client session with the ID token (replay attack mitigation).
        /// </summary>
        public const string Nonce = "nonce";

        /// <summary>
        /// The response mode specifying how the authorization response is delivered (query, fragment, form_post).
        /// </summary>
        public const string ResponseMode = "response_mode";

        /// <summary>
        /// An opaque value maintained by the client to prevent CSRF and preserve state across redirects.
        /// </summary>
        public const string State = "state";

        /// <summary>
        /// The URI to which the authorization server redirects the user after authorization.
        /// </summary>
        public const string RedirectUri = "redirect_uri";

        /// <summary>
        /// The unique identifier of the client making the authorization request.
        /// </summary>
        public const string ClientId = "client_id";

        /// <summary>
        /// The type of response expected from the authorization endpoint (e.g., "code").
        /// </summary>
        public const string ResponseType = "response_type";

        /// <summary>
        /// A JWT-encoded authorization request object (JAR - RFC 9101).
        /// </summary>
        public const string Request = "request";

        /// <summary>
        /// A URI pointing to a JWT-encoded authorization request object.
        /// </summary>
        public const string RequestUri = "request_uri";

        /// <summary>
        /// The intended audience for the authorization request.
        /// </summary>
        public const string Audience = "aud";
    }

    /// <summary>
    /// Parameter names for the token introspection request (RFC 7662).
    /// </summary>
    public static class IntrospectionRequest
    {
        /// <summary>
        /// The token to be introspected.
        /// </summary>
        public const string Token = "token";

        /// <summary>
        /// The scopes associated with the token being introspected.
        /// </summary>
        public const string Scope = "scope";

        /// <summary>
        /// A hint about the type of the token being introspected.
        /// </summary>
        public const string TokenHintType = "token_hint_type";
    }

    /// <summary>
    /// Token type hint values used in introspection and revocation requests.
    /// </summary>
    public static class TokenHintTypes
    {
        /// <summary>
        /// Hint indicating the token is an access token.
        /// </summary>
        public const string AccessToken = "access_token";

        /// <summary>
        /// Hint indicating the token is a refresh token.
        /// </summary>
        public const string RefreshToken = "refresh_token";
    }

    /// <summary>
    /// Parameter names for the OAuth 2.0 token endpoint request.
    /// </summary>
    public static class TokenRequest
    {
        /// <summary>
        /// The grant type being used (e.g., "authorization_code", "client_credentials").
        /// </summary>
        public const string GrantType = "grant_type";

        /// <summary>
        /// The redirect URI that was used in the authorization request (must match for code exchange).
        /// </summary>
        public const string RedirectUri = "redirect_uri";

        /// <summary>
        /// The client identifier.
        /// </summary>
        public const string ClientId = "client_id";

        /// <summary>
        /// The client secret for confidential client authentication.
        /// </summary>
        public const string ClientSecret = "client_secret";

        /// <summary>
        /// A JWT assertion used for private_key_jwt or client_secret_jwt authentication.
        /// </summary>
        public const string ClientAssertion = "client_assertion";

        /// <summary>
        /// The type of client assertion (e.g., "urn:ietf:params:oauth:client-assertion-type:jwt-bearer").
        /// </summary>
        public const string ClientAssertionType = "client_assertion_type";

        /// <summary>
        /// A SAML or JWT assertion used in assertion-based grant types.
        /// </summary>
        public const string Assertion = "assertion";

        /// <summary>
        /// The authorization code received from the authorization endpoint.
        /// </summary>
        public const string Code = "code";

        /// <summary>
        /// The refresh token to exchange for a new access token.
        /// </summary>
        public const string RefreshToken = "refresh_token";

        /// <summary>
        /// The requested scope(s) for the token.
        /// </summary>
        public const string Scope = "scope";

        /// <summary>
        /// The resource owner's username (Resource Owner Password Credentials grant).
        /// </summary>
        public const string UserName = "username";

        /// <summary>
        /// The resource owner's user identifier (used in internal token operations).
        /// </summary>
        public const string UserId = "userid";

        /// <summary>
        /// The resource owner's password (Resource Owner Password Credentials grant).
        /// </summary>
        public const string Password = "password";

        /// <summary>
        /// The PKCE code verifier that proves possession of the code challenge.
        /// </summary>
        public const string CodeVerifier = "code_verifier";

        /// <summary>
        /// The token type parameter used in token exchange flows.
        /// </summary>
        public const string TokenType = "token_type";

        /// <summary>
        /// The algorithm parameter for key-related token operations.
        /// </summary>
        public const string Algorithm = "alg";

        /// <summary>
        /// The key parameter used in key-related token operations.
        /// </summary>
        public const string Key = "key";

        /// <summary>
        /// The token parameter for token introspection or revocation within token requests.
        /// </summary>
        public const string Token = "token";

        /// <summary>Verification code from external sign-in redirect (e.g. UserCode query param).</summary>
        public const string UserCode = "user_code";

        //public const string DeviceCode = "device_code";

        // token exchange
        /// <summary>
        /// The target resource URI in token exchange requests.
        /// </summary>
        public const string Resource = "resource";

        /// <summary>
        /// The target audience in token exchange requests.
        /// </summary>
        public const string Audience = "audience";
        //public const string RequestedTokenType = "requested_token_type";
        //public const string SubjectToken = "subject_token";
        //public const string SubjectTokenType = "subject_token_type";
        //public const string ActorToken = "actor_token";
        //public const string ActorTokenType = "actor_token_type";
    }

    /// <summary>
    /// OIDC prompt parameter values that control the authorization server's user interaction behavior.
    /// </summary>
    public static class PromptModes
    {
        /// <summary>
        /// Do not display any authentication or consent UI. If the user is not already authenticated,
        /// an error is returned.
        /// </summary>
        public const string None = "none";

        /// <summary>
        /// Force the user to re-authenticate, even if they have an active session.
        /// </summary>
        public const string Login = "login";

        //public const string Consent = "consent";

        /// <summary>
        /// Prompt the user to select an account if multiple sessions exist.
        /// </summary>
        public const string SelectAccount = "select_account";
    }

    /// <summary>
    /// Authentication scheme constants used in HTTP Authorization headers.
    /// </summary>
    public static class AuthenticationSchemes
    {
        /// <summary>
        /// The Bearer token authentication scheme used in the HTTP Authorization header.
        /// </summary>
        public const string AuthorizationHeaderBearer = "Bearer";
        //public const string FormPostBearer = "access_token";
        //public const string QueryStringBearer = "access_token";
        //public const string AuthorizationHeaderPop = "PoP";
        //public const string FormPostPop = "pop_access_token";
        //public const string QueryStringPop = "pop_access_token";
    }

    /// <summary>
    /// Query string parameter name constants used in server-side UI route redirects.
    /// </summary>
    public static class RoutePathParameters
    {
        /// <summary>
        /// Parameter name for error identifiers in error page redirects.
        /// </summary>
        public const string Error = "errorId";

        /// <summary>
        /// Parameter name for the return URL in login page redirects.
        /// </summary>
        public const string Login = "returnUrl";

        /// <summary>
        /// Parameter name for the return URL in consent page redirects.
        /// </summary>
        public const string Consent = "returnUrl";

        /// <summary>
        /// Parameter name for the logout session identifier in logout page redirects.
        /// </summary>
        public const string Logout = "logoutId";

        /// <summary>
        /// Parameter name for the end-session callback identifier.
        /// </summary>
        public const string EndSessionCallback = "endSessionId";

        /// <summary>
        /// Parameter name for the return URL in custom page redirects.
        /// </summary>
        public const string Custom = "returnUrl";

        /// <summary>
        /// Parameter name for the user verification code in device/external auth flows.
        /// </summary>
        public const string UserCode = "userCode";
    }

    /// <summary>
    /// JWS (JSON Web Signature) algorithm identifier constants as defined in RFC 7518.
    /// Used for JWT token signing and key generation.
    /// </summary>
    public static class Algorithms
    {
        /// <summary>ECDSA using P-256 curve and SHA-256 hash.</summary>
        public const string EcdsaSha256 = "ES256";

        /// <summary>ECDSA using P-384 curve and SHA-384 hash.</summary>
        public const string EcdsaSha384 = "ES384";

        /// <summary>ECDSA using P-521 curve and SHA-512 hash.</summary>
        public const string EcdsaSha512 = "ES512";

        /// <summary>HMAC using SHA-384 (symmetric).</summary>
        public const string HmacSha384 = "HS384";

        /// <summary>HMAC using SHA-512 (symmetric).</summary>
        public const string HmacSha512 = "HS512";

        /// <summary>HMAC using SHA-256 (symmetric).</summary>
        public const string HmacSha256 = "HS256";

        /// <summary>RSASSA-PKCS1-v1_5 using SHA-256 (asymmetric).</summary>
        public const string RsaSha256 = "RS256";

        /// <summary>RSASSA-PKCS1-v1_5 using SHA-384 (asymmetric).</summary>
        public const string RsaSha384 = "RS384";

        /// <summary>RSASSA-PKCS1-v1_5 using SHA-512 (asymmetric).</summary>
        public const string RsaSha512 = "RS512";

        /// <summary>RSASSA-PSS using SHA-256 and MGF1 with SHA-256 (asymmetric).</summary>
        public const string RsaSsaPssSha256 = "PS256";

        /// <summary>RSASSA-PSS using SHA-384 and MGF1 with SHA-384 (asymmetric).</summary>
        public const string RsaSsaPssSha384 = "PS384";

        /// <summary>RSASSA-PSS using SHA-512 and MGF1 with SHA-512 (asymmetric).</summary>
        public const string RsaSsaPssSha512 = "PS512";
    }

    /// <summary>
    /// Constants for OIDC back-channel logout token event types and identifiers.
    /// </summary>
    public static class LogoutTokenEvents
    {
        /// <summary>
        /// The back-channel logout event URI included in the "events" claim of logout tokens.
        /// </summary>
        public const string BackChannelLogoutUri = "http://schemas.openid.net/event/backchannel-logout";

        /// <summary>
        /// The token type identifier for logout tokens.
        /// </summary>
        public const string LogoutToken = "logout_token";
    }

    //public static class ResponseTypes
    //{
    //    public const string AuthorizationCode = "code";
    //    public const string IdentityToken = "id_token";
    //    public const string AccessToken = "token";
    //}
}
