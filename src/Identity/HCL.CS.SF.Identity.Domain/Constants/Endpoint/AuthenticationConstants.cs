/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using static HCL.CS.SF.Domain.Constants.Endpoint.OpenIdConstants;
using ClaimTypes = System.Security.Claims.ClaimTypes;

//TODO - Remove unused constants
namespace HCL.CS.SF.Domain.Constants.Endpoint;

/// <summary>
/// Core authentication constants for the OAuth 2.0 / OpenID Connect authorization server.
/// Contains protocol-level defaults, allowed values for grant types, response types, response modes,
/// prompt modes, claim mappings, and nested classes for specialized constant groupings.
/// </summary>
public static class AuthenticationConstants
{
    /// <summary>
    /// Defines the scope requirements for different OAuth response types.
    /// Determines which types of scopes (identity, resource, or both) must be present in a request.
    /// </summary>
    public enum ScopeRequirement
    {
        /// <summary>
        /// No scope requirement enforced.
        /// </summary>
        None,

        /// <summary>
        /// Only resource (API) scopes are required; identity scopes are not allowed.
        /// </summary>
        ResourceOnly,

        /// <summary>
        /// Only identity scopes (openid, profile, etc.) are required; resource scopes are not allowed.
        /// </summary>
        IdentityOnly,

        /// <summary>
        /// Both identity and resource scopes are allowed and at least an identity scope (openid) is required.
        /// </summary>
        Identity
    }

    /// <summary>
    /// The framework's internal application name identifier.
    /// </summary>
    public const string HCLCSSFName = "HCL.CS.SF";

    /// <summary>
    /// The framework's internal type identifier, matching the application name.
    /// </summary>
    public const string HCLCSSFType = HCLCSSFName;

    /// <summary>
    /// The authentication method value used when a user authenticates via an external identity provider.
    /// </summary>
    public const string ExternalAuthenticationMethod = "external";

    /// <summary>
    /// The default hash algorithm used for computing PKCE code challenges and token hashes.
    /// </summary>
    public const string DefaultHashAlgorithm = "SHA256";

    /// <summary>
    /// The default expiry duration (in seconds) for short-lived verification codes.
    /// </summary>
    public const int DefaultVerificationCodeExpiryDuration = 60;

    /// <summary>
    /// The identity provider identifier value for locally authenticated users (non-federated).
    /// </summary>
    public const string LocalIdentityProvider = "local";
    //public const string DefaultCookieAuthenticationScheme = "HCL.CS.SF.Identity";
    //public const string SignoutScheme = "HCL.CS.SF.Identity";
    //public const string ExternalCookieAuthenticationScheme = "HCL.CS.SF.External";

    /// <summary>
    /// Maximum allowed length (in characters) for query string parameter values.
    /// </summary>
    public const int QueryStringLength = 128;

    /// <summary>
    /// AES key size of 16 bytes (128 bits) for encryption operations.
    /// </summary>
    public const int KeySize16 = 16;

    /// <summary>
    /// AES key size of 24 bytes (192 bits) for encryption operations.
    /// </summary>
    public const int KeySize24 = 24;

    /// <summary>
    /// AES key size of 32 bytes (256 bits) for encryption operations.
    /// </summary>
    public const int KeySize32 = 32;

    /// <summary>
    /// Default lifetime for authentication cookies. Set to 10 hours.
    /// </summary>
    public static readonly TimeSpan DefaultCookieTimeSpan = TimeSpan.FromHours(10);

    /// <summary>
    /// Default duration for caching discovery document and JWKS responses. Set to 60 minutes.
    /// </summary>
    public static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(60);

    /// <summary>
    /// The list of PKCE code challenge methods supported by the authorization server (plain and S256).
    /// </summary>
    public static readonly List<string> CodeChallengeMethods = new()
    {
        OpenIdConstants.CodeChallengeMethods.Plain,
        OpenIdConstants.CodeChallengeMethods.Sha256
    };

    /// <summary>
    /// The list of OAuth 2.0 response types supported by the authorization endpoint.
    /// Currently only "code" (Authorization Code flow) is supported.
    /// </summary>
    public static readonly List<string> AllowedResponseTypes = new()
    {
        ResponseTypes.Code
    };

    /// <summary>
    /// Maps each allowed response type to the corresponding grant type expected at the token endpoint.
    /// </summary>
    public static readonly Dictionary<string, string> AllowedGrantTypeForResponseType = new()
    {
        { ResponseTypes.Code, GrantType.AuthorizationCode }
    };

    /// <summary>
    /// Maps each grant type to the response modes permitted for that grant type.
    /// Authorization Code flow allows "query" and "form_post" response modes.
    /// </summary>
    public static readonly Dictionary<string, IEnumerable<string>> AllowedResponseModesForGrantType = new()
    {
        { GrantType.AuthorizationCode, new[] { ResponseModes.Query, ResponseModes.FormPost } }
    };

    /// <summary>
    /// The list of response modes supported by the authorization endpoint ("form_post" and "query").
    /// </summary>
    public static readonly List<string> AllowedResponseModes = new()
    {
        ResponseModes.FormPost,
        ResponseModes.Query
    };

    /// <summary>
    /// The list of grant types that are valid at the authorize endpoint.
    /// Only Authorization Code is currently supported.
    /// </summary>
    public static readonly List<string> AllowedGrantTypesForAuthorizeEndpoint = new()
    {
        GrantType.AuthorizationCode
    };

    /// <summary>
    /// The list of OIDC prompt values supported by the authorization endpoint.
    /// Controls whether the user is prompted to re-authenticate or select an account.
    /// </summary>
    public static readonly List<string> AllowedPromptModes = new()
    {
        PromptModes.None,
        PromptModes.Login,
        PromptModes.SelectAccount
    };

    /// <summary>
    /// Maps each response type to its scope requirement, determining whether identity scopes,
    /// resource scopes, or both are needed for that response type.
    /// </summary>
    public static readonly Dictionary<string, ScopeRequirement> ResponseTypeToScope = new()
    {
        { ResponseTypes.Code, ScopeRequirement.Identity },
        { ResponseTypes.Token, ScopeRequirement.ResourceOnly },
        { ResponseTypes.IdToken, ScopeRequirement.IdentityOnly },
        { ResponseTypes.IdTokenToken, ScopeRequirement.Identity },
        { ResponseTypes.CodeIdToken, ScopeRequirement.Identity },
        { ResponseTypes.CodeToken, ScopeRequirement.Identity },
        { ResponseTypes.CodeIdTokenToken, ScopeRequirement.Identity }
    };

    /// <summary>
    /// Maps custom claim short names to their corresponding .NET <see cref="System.Security.Claims.ClaimTypes"/> URIs.
    /// Used to translate between OIDC claim names and .NET claim types during claim mapping.
    /// </summary>
    public static readonly Dictionary<string, string> StandardClaims = new()
    {
        { "username", ClaimTypes.Name },
        { "family_name", ClaimTypes.Surname },
        { "given_name", ClaimTypes.GivenName },
        { "gender", ClaimTypes.Gender },
        { "pincode", ClaimTypes.PostalCode },
        { "dateofbirth", ClaimTypes.DateOfBirth },
        { "email", ClaimTypes.Email },
        { "street ", ClaimTypes.StreetAddress }
    };

    /// <summary>
    /// Environment-level path constants stored in HttpContext.Items for cross-middleware communication.
    /// </summary>
    public static class EnvironmentPaths
    {
        /// <summary>
        /// The HttpContext items key that holds the server's base path for the current request.
        /// </summary>
        public const string HCLCSSFBasePath = "HCL.CS.SF:HCLCSSFServerBasePath";

        /// <summary>
        /// The HttpContext items key that indicates a sign-out operation has been initiated.
        /// </summary>
        public const string SignOutCalled = "HCL.CS.SF:HCLCSSFServerSignOutCalled";
    }

    /// <summary>
    /// Constants for client credential parsing types, identifying how the client authenticates
    /// at the token endpoint.
    /// </summary>
    public static class ParsedTypes
    {
        /// <summary>
        /// The client does not have a secret (public client).
        /// </summary>
        public const string NoSecret = "NoSecret";

        /// <summary>
        /// The client uses a shared secret for authentication.
        /// </summary>
        public const string SharedSecret = "SharedSecret";

        /// <summary>
        /// The client uses a JWT Bearer assertion (private_key_jwt or client_secret_jwt) for authentication.
        /// Corresponds to the IETF client assertion type URN.
        /// </summary>
        public const string JwtBearer = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";
    }

    /// <summary>
    /// Constants for client secret type identifiers, indicating the format/kind of secret stored.
    /// </summary>
    public static class SecretTypes
    {
        /// <summary>
        /// A hashed shared secret (password-like) stored in the client record.
        /// </summary>
        public const string SharedSecret = "SharedSecret";

        /// <summary>
        /// A JSON Web Key used for asymmetric client authentication.
        /// </summary>
        public const string JsonWebKey = "JWK";

        /// <summary>
        /// An X.509 certificate thumbprint used for mutual TLS or certificate-based client authentication.
        /// </summary>
        public const string X509Certificate = "X509Certificate";
    }

    /// <summary>
    /// Constants for the authorization server's user-facing UI configuration, including
    /// cookie thresholds, default route path parameters, and default route paths.
    /// </summary>
    public static class ApplicationUIConstants
    {
        // the limit after which old messages are purged
        /// <summary>
        /// The maximum number of serialized messages stored in the authentication cookie.
        /// Older messages are purged when this threshold is exceeded.
        /// </summary>
        public const int CookieMessageThreshold = 2;

        /// <summary>
        /// Default query string parameter names used by the authorization server's UI routes.
        /// </summary>
        public static class DefaultRoutePathParams
        {
            /// <summary>
            /// Query parameter name for passing error identifiers to the error page.
            /// </summary>
            public const string Error = "errorId";

            /// <summary>
            /// Query parameter name for passing the return URL to the login page.
            /// </summary>
            public const string Login = "returnUrl";

            //public const string Consent = "returnUrl";

            /// <summary>
            /// Query parameter name for passing the logout session identifier to the logout page.
            /// </summary>
            public const string Logout = "logoutId";

            /// <summary>
            /// Query parameter name for passing the end-session callback identifier.
            /// </summary>
            public const string EndSessionCallback = "endSessionId";
            //public const string Custom = "returnUrl";
            //public const string UserCode = "userCode";
        }

        /// <summary>
        /// Default URL paths for the authorization server's user-facing pages.
        /// </summary>
        public static class DefaultRoutePaths
        {
            /// <summary>
            /// Default path for the login page.
            /// </summary>
            public const string Login = "/account/login";

            /// <summary>
            /// Default path for the logout page.
            /// </summary>
            public const string Logout = "/account/logout";

            //public const string Consent = "/consent";

            /// <summary>
            /// Default path for the error display page.
            /// </summary>
            public const string Error = "/home/error";
            //public const string DeviceVerification = "/device";
        }
    }

    /// <summary>
    /// OAuth 2.0 grant type string constants as defined in the OAuth specification.
    /// These values are used in the "grant_type" parameter of token endpoint requests.
    /// </summary>
    public static class GrantType
    {
        /// <summary>
        /// The Hybrid grant type combining authorization code and implicit flows.
        /// </summary>
        public const string Hybrid = "hybrid";

        /// <summary>
        /// The Authorization Code grant type ("authorization_code") as defined in RFC 6749 Section 4.1.
        /// </summary>
        public const string AuthorizationCode = "authorization_code";

        /// <summary>
        /// The Client Credentials grant type ("client_credentials") as defined in RFC 6749 Section 4.4.
        /// </summary>
        public const string ClientCredentials = "client_credentials";

        /// <summary>
        /// The Resource Owner Password Credentials grant type ("password") as defined in RFC 6749 Section 4.3.
        /// </summary>
        public const string ResourceOwnerPassword = "password";

        /// <summary>
        /// Custom grant type for exchanging an external sign-in verification code for tokens.
        /// </summary>
        public const string UserCode = "user_code";
        //public const string DeviceFlow = "urn:ietf:params:oauth:grant-type:device_code";
    }

    /// <summary>
    /// Protocol type identifiers for client registrations.
    /// </summary>
    public static class ProtocolTypes
    {
        /// <summary>
        /// OpenID Connect protocol type identifier.
        /// </summary>
        public const string OpenIdConnect = "oidc";
        //public const string WsFederation = "wsfed";
        //public const string Saml2p = "saml2p";
    }

    /// <summary>
    /// Standard OIDC identity scope names that control which user claims are returned.
    /// </summary>
    public static class IdentityScopes
    {
        /// <summary>
        /// The "openid" scope (required for all OIDC requests). Returns the subject identifier.
        /// </summary>
        public const string OpenId = "openid";

        /// <summary>
        /// The "profile" scope. Returns standard profile claims (name, family_name, given_name, etc.).
        /// </summary>
        public const string Profile = "profile";

        /// <summary>
        /// The "email" scope. Returns the email and email_verified claims.
        /// </summary>
        public const string Email = "email";

        /// <summary>
        /// The "address" scope. Returns the address claim.
        /// </summary>
        public const string Address = "address";

        /// <summary>
        /// The "phone" scope. Returns the phone_number and phone_number_verified claims.
        /// </summary>
        public const string Phone = "phone";

        /// <summary>
        /// The "offline_access" scope. Requests a refresh token for long-lived access.
        /// </summary>
        public const string OfflineAccess = "offline_access";
    }

    /// <summary>
    /// Constants for authorization code store key names used to persist intermediate authorization state.
    /// </summary>
    public static class AuthCodeStore
    {
        /// <summary>
        /// Key name for storing the serialized authorization request object (used during code exchange).
        /// </summary>
        public const string AuthCodeRequestObjectName = "authzId";

        /// <summary>
        /// Key name for storing the return URL during the authorization flow.
        /// </summary>
        public const string ReturnUrlCode = "returnUrlId";

        /// <summary>
        /// Key name for storing the user verification code during external sign-in flows.
        /// </summary>
        public const string UserVerificationCode = "UserCode";
    }

    /// <summary>
    /// Defines which JWT claims are filtered out from access tokens when constructing
    /// the user profile response at the UserInfo endpoint.
    /// </summary>
    public class AccessTokenFilters
    {
        // filter for claims from an incoming access token (e.g. used at the user profile endpoint)
        /// <summary>
        /// Array of claim types that are excluded from the UserInfo response because they are
        /// token-level metadata (not user profile data).
        /// </summary>
        public static readonly string[] ClaimsFilter =
        {
            OpenIdConstants.ClaimTypes.AccessTokenHash,
            OpenIdConstants.ClaimTypes.Audience,
            OpenIdConstants.ClaimTypes.AuthorizedParty,
            OpenIdConstants.ClaimTypes.AuthorizationCodeHash,
            OpenIdConstants.ClaimTypes.ClientId,
            OpenIdConstants.ClaimTypes.Expiration,
            OpenIdConstants.ClaimTypes.IssuedAt,
            OpenIdConstants.ClaimTypes.Issuer,
            OpenIdConstants.ClaimTypes.JwtId,
            OpenIdConstants.ClaimTypes.Nonce,
            OpenIdConstants.ClaimTypes.NotBefore,
            OpenIdConstants.ClaimTypes.ReferenceTokenId,
            OpenIdConstants.ClaimTypes.SessionId,
            OpenIdConstants.ClaimTypes.Scope
        };
    }

    /// <summary>
    /// OID (Object Identifier) constants for elliptic curve names used in ECDSA key generation.
    /// </summary>
    public static class CurveOids
    {
        /// <summary>
        /// OID for the NIST P-256 elliptic curve (used with ES256 algorithm).
        /// </summary>
        public const string P256 = "1.2.840.10045.3.1.7";

        /// <summary>
        /// OID for the NIST P-384 elliptic curve (used with ES384 algorithm).
        /// </summary>
        public const string P384 = "1.3.132.0.34";

        /// <summary>
        /// OID for the NIST P-521 elliptic curve (used with ES512 algorithm).
        /// </summary>
        public const string P521 = "1.3.132.0.35";
    }
}
