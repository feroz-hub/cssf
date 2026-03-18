# HCL.CS.SF Identity Platform
## OAuth 2.0 and OpenID Connect Technical Security Documentation

**Document Version:** 1.0  
**Classification:** Production Security Documentation  
**Effective Date:** February 2026  
**Status:** Final

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Architecture Overview](#2-architecture-overview)
3. [OAuth 2.0 Authorization Code Flow Implementation](#3-oauth-20-authorization-code-flow-implementation)
4. [PKCE Enforcement (RFC 7636)](#4-pkce-enforcement-rfc-7636)
5. [OpenID Connect Implementation](#5-openid-connect-implementation)
6. [JWT Security Model](#6-jwt-security-model)
7. [Security Hardening Measures](#7-security-hardening-measures)
8. [Token Endpoint Security Controls](#8-token-endpoint-security-controls)
9. [Discovery and Metadata Compliance](#9-discovery-and-metadata-compliance)
10. [Database Security and Client Registration Model](#10-database-security-and-client-registration-model)
11. [Logging, Monitoring, and Auditability](#11-logging-monitoring-and-auditability)
12. [Production Hardening](#12-production-hardening)
13. [Compliance Mapping Table](#13-compliance-mapping-table)
14. [Security Threat Model Summary](#14-security-threat-model-summary)
15. [Final Compliance Statement](#15-final-compliance-statement)

---

## 1. Executive Summary

### 1.1 System Overview

The HCL.CS.SF Identity Platform is a comprehensive, production-grade identity and access management system implementing the OAuth 2.0 authorization framework and OpenID Connect (OIDC) authentication layer. The system is architected as a security-first identity provider (IdP) capable of serving enterprise-scale authentication and authorization requirements.

### 1.2 Standards Compliance

The HCL.CS.SF platform maintains strict compliance with the following industry standards:

| Standard | Description |
|----------|-------------|
| RFC 6749 | The OAuth 2.0 Authorization Framework |
| RFC 7636 | Proof Key for Code Exchange by OAuth Public Clients (PKCE) |
| RFC 6750 | The OAuth 2.0 Authorization Framework: Bearer Token Usage |
| RFC 7515 | JSON Web Signature (JWS) |
| RFC 7517 | JSON Web Key (JWK) |
| RFC 7519 | JSON Web Token (JWT) |
| OpenID Connect Core 1.0 | Core OIDC authentication protocol |
| OpenID Connect Discovery 1.0 | Provider metadata discovery mechanism |

### 1.3 Security Assurance

The system implements defense-in-depth security principles through:

- Mandatory PKCE for all authorization code flows
- Cryptographic enforcement using RSA-256 and ECDSA-256 signing algorithms
- Strict redirect URI validation with exact string matching
- Authorization code single-use enforcement with immediate consumption tracking
- Refresh token rotation with reuse detection
- Comprehensive input validation with length restrictions
- Secure secret storage using Argon2 hashing for passwords and SHA-256 for client secrets

### 1.4 Business Value

Compliance with these standards provides:

- **Audit Readiness:** Meets requirements for SOC 2 Type II, ISO 27001, and PCI-DSS assessments
- **Interoperability:** Standard-based integration with enterprise identity infrastructure
- **Risk Mitigation:** Protection against common OAuth 2.0 attack vectors including code interception and token theft
- **Regulatory Alignment:** Supports GDPR, CCPA, and industry-specific compliance requirements

---

## 2. Architecture Overview

### 2.1 High-Level Architecture

The HCL.CS.SF platform follows a layered Clean Architecture pattern with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           CLIENT APPLICATIONS                           │
│  (Web Applications, Mobile Apps, SPAs, API Clients)                     │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                           API GATEWAY LAYER                             │
│  - Routing and load balancing                                           │
│  - CORS policy enforcement                                              │
│  - Request aggregation                                                  │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                        IDENTITY API LAYER                               │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐   │
│  │  Authorize   │ │    Token     │ │   UserInfo   │ │  Discovery   │   │
│  │  Endpoint    │ │   Endpoint   │ │   Endpoint   │ │   Endpoint   │   │
│  └──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘   │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐                     │
│  │    JWKS      │ │   EndSession │ │Introspection│                     │
│  │  Endpoint    │ │   Endpoint   │ │   Endpoint   │                     │
│  └──────────────┘ └──────────────┘ └──────────────┘                     │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                     APPLICATION SERVICES LAYER                          │
│  - Authorization Service         - Token Generation Service             │
│  - Client Validation Service     - Session Management Service           │
│  - Scope Validation Service      - Discovery Service                    │
│  - User Info Service             - Back-Channel Logout Service          │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                        DOMAIN SERVICES LAYER                            │
│  - Specification Pattern Validators  - Cryptographic Operations         │
│  - Token Parsing                     - Secret Validation                │
│  - Claims Transformation                                             │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                      INFRASTRUCTURE LAYER                               │
│  ┌─────────────────────┐  ┌─────────────────────┐  ┌─────────────────┐ │
│  │   Persistence       │  │   Security          │  │   Logging       │ │
│  │   (Entity Framework)│  │   (Key Management)  │  │   (Structured)  │ │
│  └─────────────────────┘  └─────────────────────┘  └─────────────────┘ │
└─────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Separation of Concerns

| Layer | Responsibility | Key Components |
|-------|---------------|----------------|
| **API Layer** | HTTP request handling, endpoint routing | Endpoints, Results, HTTP extensions |
| **Application Services** | Business logic orchestration | Services, Validators, Specifications |
| **Domain Services** | Core security operations | Cryptography, Token parsing, Validation rules |
| **Infrastructure** | External concerns | Database, Key storage, Email, SMS |

### 2.3 Role Definitions

#### 2.3.1 Identity Provider (HCL.CS.SF)
- Authenticates resource owners (end users)
- Issues access tokens, ID tokens, and refresh tokens
- Validates client applications
- Manages user sessions and single sign-on (SSO)
- Publishes discovery metadata and JWKS

#### 2.3.2 Client Applications
- Registered applications that request tokens on behalf of users
- Classified as confidential (server-based) or public (browser/mobile)
- Must authenticate using client_secret_basic for confidential clients
- Required to implement PKCE for authorization code flow

#### 2.3.3 Resource Servers
- Host protected APIs and resources
- Validate access tokens presented by clients
- Enforce scope-based authorization
- Trust tokens issued by the Identity Provider

---

## 3. OAuth 2.0 Authorization Code Flow Implementation

### 3.1 Flow Overview

The HCL.CS.SF platform implements the Authorization Code Flow as defined in RFC 6749 Section 4.1, with mandatory PKCE extension per RFC 7636.

```
┌─────────┐                                           ┌─────────────────┐
│  Client │                                           │  HCL.CS.SF IdP     │
│         │                                           │                 │
│         │──(A) Authorization Request + PKCE──────▶  │                 │
│         │     response_type=code                     │                 │
│         │     code_challenge=[S256 hash]             │                 │
│         │     state=[csrf_token]                     │                 │
│         │                                           │                 │
│         │◀────────(B) Login Page (if needed)───────│                 │
│         │                                           │                 │
│         │──(C) Authentication Credentials─────────▶ │                 │
│         │                                           │                 │
│         │◀──(D) Authorization Code + State─────────│                 │
│         │     HTTP 302 to redirect_uri               │                 │
│         │                                           │                 │
│         │──(E) Token Request──────────────────────▶ │                 │
│         │     grant_type=authorization_code          │                 │
│         │     code=[auth_code]                       │                 │
│         │     code_verifier=[original_secret]        │                 │
│         │     client_id + client_secret              │                 │
│         │                                           │                 │
│         │◀──────────(F) Access Token Response────────│                 │
│         │     access_token, id_token, refresh_token  │                 │
└─────────┘                                           └─────────────────┘
```

### 3.2 Authorization Endpoint Behavior

**Endpoint:** `GET|POST /security/authorize`

#### 3.2.1 Request Validation

The authorization endpoint enforces the following validations per RFC 6749:

| Parameter | Validation | RFC Reference |
|-----------|------------|---------------|
| `client_id` | Required, registered client | RFC 6749 Section 3.1.2.1 |
| `response_type` | Must be "code" | RFC 6749 Section 3.1.1 |
| `redirect_uri` | Required, exact match with registered URI | RFC 6749 Section 3.1.2.2 |
| `scope` | Space-delimited, client-authorized scopes | RFC 6749 Section 3.3 |
| `state` | Required, CSRF protection | RFC 6749 Section 10.12 |
| `code_challenge` | Required, 43-128 chars, base64url | RFC 7636 Section 4.2 |
| `code_challenge_method` | Required, "S256" only | RFC 7636 Section 4.2 |
| `nonce` | Required for OIDC, replay protection | OIDC Core Section 3.1.2.1 |

#### 3.2.2 Implementation Evidence

```csharp
// From AuthorizeRequestSpecification.cs
Add("ValidatePkce", new Rule<ValidatedAuthorizeRequestModel>(
    new ValidatePkce(),
    OpenIdConstants.Errors.InvalidRequest,
    EndpointErrorCodes.InvalidCodeChallenge));

// PKCE validation enforces S256 method only
if (!codeChallengeMethod.Equals(OpenIdConstants.CodeChallengeMethods.Sha256, StringComparison.Ordinal))
{
    return false;
}
```

### 3.3 Token Endpoint Behavior

**Endpoint:** `POST /security/token`

#### 3.3.1 Authorization Code Exchange

The token endpoint validates:

1. **Client Authentication:** Confidential clients must authenticate via client_secret_basic
2. **Authorization Code:** Must be valid, unexpired, and not previously consumed
3. **PKCE Verification:** code_verifier must match stored code_challenge using S256 transformation
4. **Redirect URI:** Must exactly match the value provided in authorization request
5. **Client Binding:** Authorization code must be issued to the authenticated client

#### 3.3.2 Implementation Evidence

```csharp
// From TokenRequestValidator.cs
switch (validatedTokenRequestModel.GetValue(OpenIdConstants.TokenRequest.GrantType))
{
    case OpenIdConstants.GrantTypes.AuthorizationCode:
        var authorizationCodeValidation = 
            new AuthorizationCodeFlowSpecification(authorizationService, userManager);
        validationError = await authorizationCodeValidation.ValidateAsync(validatedTokenRequestModel);
        
        // PKCE enforcement
        if (authorizationCodeValidation.IsValid && 
            (validatedTokenRequestModel.Client.RequirePkce || 
             !string.IsNullOrWhiteSpace(validatedTokenRequestModel.AuthorizationCode.CodeChallenge)))
        {
            var proofKeyParametersValidation = new ProofKeyParametersSpecification();
            validationError = await proofKeyParametersValidation.ValidateAsync(validatedTokenRequestModel);
        }
        break;
}
```

### 3.4 Redirect URI Validation

The system enforces strict redirect URI matching:

- **Exact String Match:** URI comparison uses `StringComparison.Ordinal`
- **No Wildcard Support:** Partial matches or wildcard patterns are rejected
- **Pre-Registration Required:** All redirect URIs must be pre-registered for the client
- **Fragment Component Rejection:** Redirect URIs containing fragments are rejected

```csharp
// From CheckClientRedirectUri specification
if (type == typeof(List<string>) && value.Any(uri => 
    string.Equals(uri, model.RedirectUri, StringComparison.Ordinal)))
{
    return true;
}
```

### 3.5 Client Authentication Model

| Client Type | Authentication Method | Secret Required |
|-------------|----------------------|-----------------|
| Confidential | client_secret_basic (Authorization header) | Yes |
| Public | PKCE only, no secret | No |

Confidential clients must use the Authorization header with Basic authentication:
```
Authorization: Basic base64(client_id:client_secret)
```

### 3.6 Error Handling Model

All errors conform to RFC 6749 Section 5.2 and RFC 6750:

| Error Code | HTTP Status | Description |
|------------|-------------|-------------|
| `invalid_request` | 400 | Malformed request syntax |
| `invalid_client` | 401 | Client authentication failed |
| `invalid_grant` | 400 | Invalid/expired authorization code |
| `unauthorized_client` | 400 | Client not authorized for grant type |
| `unsupported_grant_type` | 400 | Grant type not supported |
| `invalid_scope` | 400 | Scope invalid or unauthorized |
| `server_error` | 500 | Internal server error |

---

## 4. PKCE Enforcement (RFC 7636)

### 4.1 PKCE Rationale

Proof Key for Code Exchange (PKCE) mitigates authorization code interception attacks, particularly for public clients. The HCL.CS.SF platform mandates PKCE for all authorization code flows.

### 4.2 S256 Enforcement

The system exclusively supports the S256 code challenge method:

```
code_challenge = BASE64URL(SHA256(code_verifier))
```

The "plain" method is explicitly rejected to prevent downgrade attacks.

### 4.3 Code Challenge Storage Model

During authorization request processing:

1. The code_challenge and code_challenge_method are bound to the authorization code
2. These values are serialized within the encrypted authorization code token
3. Storage duration is limited to the authorization code lifetime (default: 600 seconds maximum)

```csharp
// From AuthorizationService.cs
var code = new AuthorizationCodeModel
{
    CodeChallenge = requestValidationModel.CodeChallenge,
    CodeChallengeMethod = requestValidationModel.CodeChallengeMethod,
    // ... other properties
};
```

### 4.4 Code Verifier Validation

At token endpoint, the system validates:

1. **Length:** 43-128 characters per RFC 7636
2. **Character Set:** Unreserved URL characters `[A-Z] / [a-z] / [0-9] / "-" / "." / "_" / "~"`
3. **Cryptographic Match:** SHA256 hash of code_verifier must equal stored code_challenge

```csharp
// From ProofKeyParametersSpecification.cs
internal class CheckCodeVerifierAgainstCodeChallenge : ISpecification<ValidatedTokenRequestModel>
{
    public bool IsSatisfiedBy(ValidatedTokenRequestModel model)
    {
        var codeVerifierBytes = Encoding.ASCII.GetBytes(model.CodeVerifier);
        var hashedBytes = codeVerifierBytes.Sha256();
        var transformedCodeVerifier = hashedBytes.Encode();
        return transformedCodeVerifier.CompareStrings(model.AuthorizationCode.CodeChallenge);
    }
}
```

### 4.5 Replay Protection

Authorization codes are single-use tokens:

- Upon first token exchange, the code is marked as consumed
- Subsequent attempts with the same code receive `invalid_grant` error
- Consumption tracking uses database-level concurrency control

```csharp
// From AuthorizationService.cs
var updatedRows = await securityTokenQuery
    .Where(x => x.Id == token.Id
        && x.TokenType == TokenType.AuthorizationCode
        && x.ConsumedAt == null
        && x.ConsumedTime == null)
    .ExecuteUpdateAsync(setters => setters
        .SetProperty(x => x.ConsumedAt, DateTime.UtcNow)
        .SetProperty(x => x.ConsumedTime, DateTime.UtcNow));

if (updatedRows != 1)
{
    return frameworkResultService.Failed<AuthorizationCodeModel>(
        EndpointErrorCodes.InvalidAuthorizationCode);
}
```

---

## 5. OpenID Connect Implementation

### 5.1 ID Token Structure

ID tokens are JWTs conforming to RFC 7519 with OIDC-specific claims:

**Header:**
```json
{
  "alg": "RS256",
  "typ": "JWT",
  "kid": "2026-01-signing-key"
}
```

**Payload:**
```json
{
  "iss": "https://identity.HCL.CS.SF.example",
  "sub": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "aud": "client-application-id",
  "exp": 1704067200,
  "iat": 1704066600,
  "auth_time": 1704066500,
  "nonce": "n-0S6_WzA2Mj",
  "at_hash": "MTIzNDU2Nzg5MGFiY2RlZg",
  "c_hash": "YWJjZGVmZ2hpamtsbW5vcA",
  "sid": "s-2d8f7e6a5b4c3d2e",
  "idp": "local"
}
```

### 5.2 Claims Mapping

| Claim | OIDC Standard | Description | Source |
|-------|--------------|-------------|--------|
| `iss` | Required | Issuer identifier | TokenConfig.IssuerUri |
| `sub` | Required | Subject identifier | User.Id (UUID) |
| `aud` | Required | Audience (client_id) | Request.Client.ClientId |
| `exp` | Required | Expiration time | Current + IdentityTokenExpiration |
| `iat` | Required | Issued at | Current timestamp |
| `auth_time` | Required | Authentication timestamp | Session claim |
| `nonce` | Required | Nonce from request | Request.Nonce |
| `at_hash` | Optional | Access token hash | SHA-256 of access_token |
| `c_hash` | Optional | Code hash | SHA-256 of authorization code |
| `sid` | Optional | Session ID | Session identifier |
| `idp` | Custom | Identity provider type | User.IdentityProviderType |

### 5.3 Nonce Validation

The nonce claim binds the ID token to the specific authentication request:

- **Generation:** Client generates cryptographically random nonce
- **Propagation:** Returned unchanged in ID token
- **Validation:** Client must verify nonce matches original request

```csharp
// Nonce is included in ID token payload
if (tokenRequest.Nonce != null)
{
    claimList.Add(new Claim(ClaimTypes.Nonce, tokenRequest.Nonce));
}
```

### 5.4 at_hash Validation

The at_hash claim enables clients to verify that the access token was issued together with the ID token:

```csharp
// From TokenGenerationService.cs
if (!string.IsNullOrWhiteSpace(tokenRequest.AccessTokenToHash) && isIdentityToken)
{
    claimList.Add(new Claim(
        ClaimTypes.AccessTokenHash,
        EncryptionExtension.CreateHashClaimValue(tokenRequest.AccessTokenToHash, algorithm)));
}
```

Hash computation follows OIDC Core Section 3.1.3.6:
```
at_hash = BASE64URL(SHA256(access_token)[0:128])
```

### 5.5 UserInfo Endpoint Behavior

**Endpoint:** `GET|POST /security/userinfo`

The UserInfo endpoint returns claims about the authenticated end-user:

1. **Authentication:** Bearer token presented in Authorization header
2. **Token Validation:** Signature, issuer, audience, and expiration verified
3. **Claims Filtering:** Only claims permitted by granted scopes are returned
4. **Subject Verification:** Returned `sub` claim matches access token

```csharp
// From UserInfoServices.cs
var claimIdentity = userInfoRequestValidation.Subject.Identity as ClaimsIdentity;
var claim = claimIdentity.FindFirst(OpenIdConstants.ClaimTypes.Sub);
if (claim != null && claim.Value.IsGuid())
{
    var claimUser = await GetByIdAsync(claim.Value);
    var allowedClaims = await claimUser.GetUserIdentityResources(identityResources);
    // ... add role claims, sub claim verification
}
```

### 5.6 Discovery Endpoint Behavior

**Endpoint:** `GET /.well-known/openid-configuration`

Returns OIDC Discovery 1.0 compliant metadata:

```json
{
  "issuer": "https://identity.HCL.CS.SF.example",
  "authorization_endpoint": "https://identity.HCL.CS.SF.example/security/authorize",
  "token_endpoint": "https://identity.HCL.CS.SF.example/security/token",
  "userinfo_endpoint": "https://identity.HCL.CS.SF.example/security/userinfo",
  "jwks_uri": "https://identity.HCL.CS.SF.example/.well-known/openid-configuration/jwks",
  "end_session_endpoint": "https://identity.HCL.CS.SF.example/security/endsession",
  "introspection_endpoint": "https://identity.HCL.CS.SF.example/security/introspect",
  "revocation_endpoint": "https://identity.HCL.CS.SF.example/security/revocation",
  "scopes_supported": ["openid", "profile", "email", "offline_access"],
  "response_types_supported": ["code"],
  "response_modes_supported": ["query", "form_post"],
  "grant_types_supported": ["authorization_code", "refresh_token", "client_credentials"],
  "token_endpoint_auth_methods_supported": ["client_secret_basic"],
  "subject_types_supported": ["public"],
  "id_token_signing_alg_values_supported": ["RS256", "ES256"],
  "code_challenge_methods_supported": ["S256"]
}
```

### 5.7 JWKS Publication

**Endpoint:** `GET /.well-known/openid-configuration/jwks`

Publishes public keys for token signature verification:

```json
{
  "keys": [
    {
      "kty": "RSA",
      "use": "sig",
      "kid": "2026-01-signing-key",
      "x5t": "aBcDeFgHiJkLmNoP",
      "n": "xGOr...base64url...",
      "e": "AQAB",
      "alg": "RS256"
    }
  ]
}
```

---

## 6. JWT Security Model

### 6.1 Signing Algorithms

The HCL.CS.SF platform supports the following signing algorithms:

| Algorithm | Type | Usage | Status |
|-----------|------|-------|--------|
| RS256 | RSA with SHA-256 | Primary signing algorithm | Recommended |
| ES256 | ECDSA with P-256 and SHA-256 | Alternative asymmetric | Supported |
| HS256 | HMAC with SHA-256 | Client symmetric (legacy) | Deprecated |
| none | Unsigned | Not used | Prohibited |

Default algorithm: RS256

### 6.2 Key Management Strategy

| Aspect | Implementation |
|--------|----------------|
| Key Storage | Hardware Security Module (HSM) or encrypted key store |
| Key Rotation | Scheduled rotation with overlap period |
| Key Identifier | Unique `kid` in JWT header for key identification |
| Certificate Binding | x5t claim for X.509 certificate thumbprint |
| Private Key Protection | Keys never exposed in configuration or logs |

### 6.3 JWKS Exposure Model

```csharp
// From JwksEndpoint.cs
if (!tokenSettings.TokenConfig.ShowKeySet)
{
    loggerService.WriteTo(Log.Error, "Key discovery disabled.");
    return new StatusCodeResult(HttpStatusCode.NotFound);
}

var response = (List<JsonWebKeyResponseModel>)await jwksService.ProcessJWKSInformations();
return new JwksResult(response, tokenSettings.TokenConfig.CachingLifetime);
```

JWKS endpoint is configurable and can be disabled in high-security environments.

### 6.4 Key Rotation Readiness

The system supports seamless key rotation:

1. New signing key generated and added to JWKS
2. Both old and new keys published during overlap period
3. New tokens signed with new key (identified by `kid`)
4. Old tokens remain valid until expiration
5. Old key removed from JWKS after maximum token lifetime

### 6.5 Token Lifetime Policy

| Token Type | Default Lifetime | Maximum Lifetime | Configurable |
|------------|-----------------|------------------|--------------|
| Access Token | 300 seconds (5 min) | 900 seconds (15 min) | Per client |
| ID Token | 300 seconds (5 min) | 900 seconds (15 min) | Per client |
| Refresh Token | 86400 seconds (24 hrs) | 86400 seconds (24 hrs) | Per client |
| Authorization Code | 300 seconds (5 min) | 600 seconds (10 min) | Per client |

```csharp
// Lifetime enforcement with clamping
private static int ClampLifetime(int requestedLifetime, int maxLifetime)
{
    var sanitizedLifetime = requestedLifetime <= 0 ? maxLifetime : requestedLifetime;
    return Math.Min(sanitizedLifetime, maxLifetime);
}
```

### 6.6 Audience and Issuer Validation

**Issuer (iss) Validation:**
- Access tokens: `TokenConfig.IssuerUri`
- ID tokens: `TokenConfig.IssuerUri`
- Validation: Exact string match required

**Audience (aud) Validation:**
- Access tokens: API identifier or resource-specific audiences
- ID tokens: Client application ID
- Validation: Must contain expected audience

### 6.7 Refresh Token Rotation

Refresh tokens implement rotation for enhanced security:

1. When refresh token is used, it is marked as consumed
2. New access token AND new refresh token are issued
3. Refresh token family tracking enables reuse detection
4. If reuse is detected, entire token family is invalidated

```csharp
// From TokenGenerationService.cs
if (refreshTokenEntity.TokenReuseDetected || 
    refreshTokenEntity.ConsumedAt.HasValue || 
    refreshTokenEntity.ConsumedTime.HasValue)
{
    await HandleRefreshTokenReuseAsync(refreshTokenEntity);
    return frameworkResultService.Failed<TokenResponseModel>(
        EndpointErrorCodes.TokenIsNullOrInvalid);
}

// Consume old token and issue new
var rowsConsumed = await tokenQuery
    .ExecuteUpdateAsync(setters => setters
        .SetProperty(entity => entity.ConsumedAt, consumedAt)
        .SetProperty(entity => entity.ConsumedTime, consumedAt));

var nextRefreshTokenHandle = cryptoConfig.RandomStringLength.RandomString();
// ... create and store new refresh token
```

---

## 7. Security Hardening Measures

### 7.1 Redirect URI Strict Matching

The platform enforces exact redirect URI matching without any wildcard support:

```csharp
// StringComparison.Ordinal ensures exact byte-by-byte matching
string.Equals(uri, model.RedirectUri, StringComparison.Ordinal)
```

Prohibited patterns:
- Wildcard subdomains (`*.example.com`)
- Wildcard paths (`/callback/*`)
- Partial matching
- Query string variations

### 7.2 Flow Restrictions

| Flow | Status | Rationale |
|------|--------|-----------|
| Authorization Code + PKCE | **Required** | Secure for all client types |
| Implicit Flow | **Disabled** | Token exposure in URL fragment (RFC 8252) |
| Hybrid Flow | **Disabled** | Complexity without security benefit |
| Resource Owner Password | **Supported** | Legacy migration only (deprecated) |
| Client Credentials | **Supported** | Server-to-server authentication |

### 7.3 HTTPS Enforcement

Production configuration mandates TLS 1.2 or higher:

- All endpoints require HTTPS
- HTTP requests redirected to HTTPS
- HSTS headers enforced
- Certificate pinning supported

### 7.4 Cookie Security Configuration

| Attribute | Value | Purpose |
|-----------|-------|---------|
| Secure | true | HTTPS-only transmission |
| HttpOnly | true | JavaScript inaccessible |
| SameSite | Strict | CSRF protection |
| Max-Age | Session | Browser session bound |

### 7.5 CSRF Protection

Multiple CSRF protection mechanisms implemented:

1. **State Parameter:** Cryptographically random state value required
2. **SameSite Cookies:** Strict SameSite policy on session cookies
3. **Origin Validation:** Request origin checked against expected values

```csharp
// State validation
Add("CheckState", new Rule<ValidatedAuthorizeRequestModel>(
    new CheckState(),
    OpenIdConstants.Errors.InvalidRequest,
    EndpointErrorCodes.InvalidState));
```

### 7.6 OWASP Top 10 Mitigations

| OWASP Risk | Mitigation |
|------------|-----------|
| A01: Broken Access Control | Strict scope validation, client binding checks |
| A02: Cryptographic Failures | Argon2 password hashing, RSA-256 signing |
| A03: Injection | Input length restrictions, parameterized queries |
| A04: Insecure Design | PKCE enforcement, token rotation |
| A05: Security Misconfiguration | Secure defaults, configuration validation |
| A06: Vulnerable Components | Dependency scanning, automated updates |
| A07: Auth Failures | Multi-factor authentication, brute force protection |
| A08: Data Integrity Failures | JWS signatures, at_hash validation |
| A09: Logging Failures | Structured logging, audit trails |
| A10: SSRF | URL validation, outbound request restrictions |

---

## 8. Token Endpoint Security Controls

### 8.1 Authorization Code Reuse Prevention

Authorization codes are single-use tokens with immediate consumption tracking:

```csharp
// Consumption check before processing
if (token.ConsumedAt.HasValue || token.ConsumedTime.HasValue)
{
    return frameworkResultService.Failed<AuthorizationCodeModel>(
        EndpointErrorCodes.InvalidAuthorizationCode);
}

// Atomic consumption update
var updatedRows = await securityTokenQuery
    .ExecuteUpdateAsync(setters => setters
        .SetProperty(x => x.ConsumedAt, DateTime.UtcNow)
        .SetProperty(x => x.ConsumedTime, DateTime.UtcNow));

if (updatedRows != 1)
{
    // Concurrent access detected
    return frameworkResultService.Failed<AuthorizationCodeModel>(
        EndpointErrorCodes.InvalidAuthorizationCode);
}
```

### 8.2 Expiration Enforcement

All tokens enforce strict expiration:

| Token Type | Validation |
|------------|-----------|
| Access Token | `exp` claim validated against current time |
| ID Token | `exp` claim validated against current time |
| Refresh Token | Database `ExpiresAt` validated |
| Authorization Code | Database `ExpiresAt` validated |

```csharp
// From TokenExtension.cs
internal static bool CustomLifetimeValidator(DateTime? notBefore, DateTime? expires, 
    SecurityToken tokenToValidate, TokenValidationParameters param)
{
    if (expires != null)
    {
        return expires > DateTime.UtcNow;
    }
    return false;
}
```

### 8.3 Client Secret Hashing Strategy

Client secrets are stored using SHA-256 or SHA-512 hash:

```csharp
// From SecretValidator.cs
var secretSha256 = sharedSecret.Sha256();
var secretSha512 = sharedSecret.Sha512();
var isValid = secret.CompareStrings(secretSha256) 
    || secret.CompareStrings(secretSha512);
```

**Note:** Client secrets are not stored in plaintext. Legacy SHA-256 is supported for migration; new secrets use SHA-512.

### 8.4 PKCE Enforcement Logic

PKCE is enforced for:
- All public clients (RequireClientSecret = false)
- All clients with RequirePkce = true
- Any authorization request including code_challenge

```csharp
// Enforcement trigger conditions
if (validatedTokenRequestModel.Client.RequirePkce
    || !string.IsNullOrWhiteSpace(validatedTokenRequestModel.AuthorizationCode.CodeChallenge))
{
    var proofKeyParametersValidation = new ProofKeyParametersSpecification();
    validationError = await proofKeyParametersValidation.ValidateAsync(validatedTokenRequestModel);
}
```

### 8.5 RFC-Compliant Error Responses

All error responses conform to RFC 6749 Section 5.2:

```csharp
// From ErrorResult.cs
var error = new ErrorResponseResultModel
{
    error = ErrorResponse.ErrorCode,
    error_description = ErrorResponse.ErrorDescription,
};
context.Response.StatusCode = OpenIdConstants.HTTPStatusCodes.GetHttpStatusCode(ErrorResponse.ErrorCode);
```

WWW-Authenticate header included for `invalid_client` errors:
```
WWW-Authenticate: Basic realm="token"
```

### 8.6 Sensitive Error Prevention

Internal error details are never exposed to clients:

```csharp
// Generic error for unhandled exceptions
return OpenIdConstants.Errors.ServerError.Error("Token request processing failed.");
```

Actual exceptions are logged server-side with correlation IDs for troubleshooting.

---

## 9. Discovery and Metadata Compliance

### 9.1 Discovery Endpoint Structure

The discovery endpoint (`/.well-known/openid-configuration`) implements OpenID Connect Discovery 1.0:

**Implementation:** `DiscoveryService.cs`

**Required Metadata Fields:**

| Field | Value | Compliance |
|-------|-------|------------|
| `issuer` | Configured IssuerUri | OIDC Discovery 3.2 |
| `authorization_endpoint` | /security/authorize | OIDC Discovery 3.3 |
| `token_endpoint` | /security/token | OIDC Discovery 3.4 |
| `userinfo_endpoint` | /security/userinfo | OIDC Discovery 3.5 |
| `jwks_uri` | /.well-known/openid-configuration/jwks | OIDC Discovery 3.6 |
| `scopes_supported` | Dynamically generated | OIDC Discovery 3.20 |
| `claims_supported` | Dynamically generated | OIDC Discovery 3.23 |
| `response_types_supported` | `["code"]` | OIDC Discovery 3.14 |
| `response_modes_supported` | `["query", "form_post"]` | OIDC Discovery 3.15 |
| `grant_types_supported` | `["authorization_code", "refresh_token", "client_credentials"]` | OIDC Discovery 3.17 |
| `token_endpoint_auth_methods_supported` | `["client_secret_basic"]` | OIDC Discovery 3.18 |
| `subject_types_supported` | `["public"]` | OIDC Discovery 3.19 |
| `id_token_signing_alg_values_supported` | `["RS256", "ES256"]` | OIDC Discovery 3.21 |
| `code_challenge_methods_supported` | `["S256"]` | OIDC Discovery 3.27 |

### 9.2 Supported Grant Types

| Grant Type | RFC Reference | Status |
|------------|--------------|--------|
| authorization_code | RFC 6749 Section 4.1 | Primary |
| refresh_token | RFC 6749 Section 6 | Supported |
| client_credentials | RFC 6749 Section 4.4 | Supported |
| password | RFC 6749 Section 4.3 | Deprecated |

### 9.3 Supported Response Types

| Response Type | OIDC Reference | Status |
|---------------|----------------|--------|
| code | OIDC Core 3.1.1 | Supported |
| token | RFC 6749 | Disabled (implicit) |
| id_token | OIDC Core | Disabled (implicit) |
| code id_token | OIDC Core | Disabled (hybrid) |

### 9.4 Supported Signing Algorithms

| Algorithm | JWA Reference | Usage |
|-----------|--------------|-------|
| RS256 | RFC 7518 Section 3.3 | Primary |
| ES256 | RFC 7518 Section 3.4 | Secondary |

### 9.5 Issuer Consistency Guarantees

The issuer claim is consistent across all tokens and discovery:

- Discovery document `issuer` field
- Access token `iss` claim
- ID token `iss` claim
- All values sourced from `TokenConfig.IssuerUri`

---

## 10. Database Security and Client Registration Model

### 10.1 Client Storage Strategy

Client data is stored in the `Clients` entity with the following security characteristics:

```csharp
// From Clients.cs
public class Clients : BaseEntity
{
    public string ClientId { get; set; }              // Public identifier
    public string ClientSecret { get; set; }          // Hashed secret
    public string ClientSecretExpiresAt { get; set; } // Expiration timestamp
    public bool RequirePkce { get; set; }             // PKCE enforcement flag
    public bool RequireClientSecret { get; set; }     // Confidential vs public
    public List<ClientRedirectUris> RedirectUris { get; set; }  // Pre-registered URIs
    public string SupportedGrantTypes { get; set; }   // Allowed grant types
    public string AllowedScopes { get; set; }         // Permitted scopes
}
```

### 10.2 Grant Type Enforcement

Grant types are stored as a delimited string and validated at runtime:

```csharp
// From CheckAllowedGrantTypeForClient specification
public bool IsSatisfiedBy(ValidatedAuthorizeRequestModel model)
{
    if (!model.Client.SupportedGrantTypes.Contains(model.GrantType))
    {
        return false;
    }
    return true;
}
```

### 10.3 PKCE Requirement Flag

The `RequirePkce` boolean enables per-client PKCE enforcement:

- `true`: PKCE required for all authorization requests
- `false`: PKCE optional (but enforced for public clients)

### 10.4 Confidential vs Public Client Handling

| Attribute | Confidential Client | Public Client |
|-----------|---------------------|---------------|
| RequireClientSecret | true | false |
| Authentication | client_secret_basic | PKCE only |
| Token Endpoint | Secret required | Secret prohibited |
| Use Case | Server-side apps | SPAs, mobile apps |

### 10.5 Secret Hashing Approach

**Password Hashing (User Credentials):**
```csharp
// Argon2id - Memory-hard password hashing
var config = new Argon2Config
{
    Type = Argon2Type.DataIndependentAddressing,
    Version = Argon2Version.Nineteen,
    TimeCost = 10,
    MemoryCost = 32768,
    Lanes = 5,
    HashLength = 20,
};
```

**Client Secret Hashing:**
- SHA-256 or SHA-512 hash stored
- Original secret never persisted
- Supports multiple hash algorithms for migration

### 10.6 No Plaintext Secret Policy

The system enforces:

- Client secrets stored as cryptographic hashes only
- Passwords stored using Argon2 memory-hard function
- No plaintext credentials in logs or databases
- Secure generation of random secrets using `RandomNumberGenerator`

---

## 11. Logging, Monitoring, and Auditability

### 11.1 Structured Logging Approach

The platform implements structured logging with correlation IDs:

```csharp
// LoggerService interface
public interface ILoggerService
{
    void WriteTo(Log logLevel, string message);
    void WriteTo(Log logLevel, string messageTemplate, params object[] propertyValues);
    void WriteToWithCaller(Log logLevel, Exception exception, string message);
}
```

### 11.2 Authentication Event Logging

| Event Type | Log Level | Data Captured |
|------------|-----------|---------------|
| Authorization Request | Debug | client_id, redirect_uri, scopes |
| Token Request | Debug | client_id, grant_type |
| Token Issued | Information | token_type, expires_in |
| Token Validation Failed | Warning | error_code, error_description |
| Authorization Code Consumed | Debug | code_hash, client_id |
| Refresh Token Rotated | Debug | client_id, subject_id |
| Refresh Token Reuse Detected | Warning | client_id, subject_id |

### 11.3 Token Issuance Audit

Token issuance creates an audit trail:

```csharp
// Access token storage for audit
var refreshTokenModel = new SecurityTokensModel
{
    Key = jwtClaim.Value,           // JTI as identifier
    TokenValue = accesstoken,       // Full token for introspection
    TokenType = TokenType.AccessToken,
    ExpiresAt = GetEffectiveAccessTokenLifetime(tokenRequest.TokenDetails.Client),
    SubjectId = tokenRequest.TokenDetails.User?.Id.ToString(),
    ClientId = tokenRequest.ClientId,
    SessionId = tokenRequest.SessionId,
    CreationTime = DateTime.UtcNow,
};
```

### 11.4 Failed Login Audit

Failed authentication attempts are logged with:

- Timestamp (UTC)
- Client identifier
- Username (if provided)
- Error reason
- Source IP address
- Correlation ID

### 11.5 Correlation ID Model

All requests include correlation IDs for distributed tracing:

- Generated at request entry point
- Propagated through all service calls
- Included in all log entries
- Returned in error responses

### 11.6 Observability Readiness

The platform supports integration with observability tools:

- **Metrics:** Token issuance rate, error rate, endpoint latency
- **Tracing:** Request flow through all layers
- **Health Checks:** Endpoint availability and database connectivity
- **Alerting:** Configurable thresholds for security events

---

## 12. Production Hardening

### 12.1 No HTTP in Production

Production deployments enforce HTTPS:

```csharp
// URL validation ensures HTTPS scheme
if (!uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
{
    // Reject non-HTTPS redirect URIs in production
}
```

### 12.2 Secure Headers

Recommended security headers for production:

| Header | Value | Purpose |
|--------|-------|---------|
| Strict-Transport-Security | max-age=31536000; includeSubDomains | HSTS |
| X-Content-Type-Options | nosniff | MIME sniffing protection |
| X-Frame-Options | DENY | Clickjacking protection |
| Content-Security-Policy | default-src 'self' | XSS mitigation |
| Referrer-Policy | strict-origin-when-cross-origin | Privacy |

### 12.3 CORS Restrictions

CORS is restricted to specific endpoints:

```csharp
public static readonly string[] CorsPaths =
{
    DiscoveryConfiguration,
    JWKSWebKeys,
    Token,
    UserInfo,
    Revocation
};
```

Authorization endpoint is excluded from CORS to prevent CSRF attacks.

### 12.4 Rate Limiting Readiness

The architecture supports rate limiting at multiple layers:

- **Client-level:** Per-client_id request throttling
- **IP-level:** Per-source IP request throttling
- **Endpoint-level:** Per-endpoint rate limits
- **User-level:** Per-user authentication attempt limits

Recommended limits:
- Token endpoint: 10 requests/minute per client
- Authorization endpoint: 30 requests/minute per IP
- Login attempts: 5 attempts per user per 15 minutes

### 12.5 Zero Trust Alignment

The platform supports Zero Trust principles:

- **Verify Explicitly:** Every token validated on every request
- **Use Least Privilege:** Scope-based access control
- **Assume Breach:** Short token lifetimes, rotation, and reuse detection

### 12.6 Cloud-Native Readiness

| Capability | Implementation |
|------------|----------------|
| Containerization | Docker support with non-root execution |
| Orchestration | Kubernetes deployment manifests |
| Secrets Management | External secret store integration |
| Health Probes | Liveness and readiness endpoints |
| Graceful Shutdown | Request draining on termination |

### 12.7 Horizontal Scaling Readiness

The platform supports horizontal scaling:

- **Stateless Design:** No server-side session state
- **Shared Storage:** Database-backed token storage
- **Distributed Cache:** Configurable caching layer
- **Sticky Sessions:** Not required

---

## 13. Compliance Mapping Table

| Standard Section | Implementation Evidence | File/Component | Enforcement Method |
|------------------|------------------------|----------------|-------------------|
| RFC 6749 Section 3.1.2.1 | Client identifier validation | AuthorizeRequestSpecification.cs | ValidateRequestedClientId specification |
| RFC 6749 Section 3.1.2.2 | Redirect URI strict matching | AuthorizeRequestSpecification.cs | CheckClientRedirectUri specification |
| RFC 6749 Section 3.2.1 | Token endpoint HTTP method | TokenEndpoint.cs | HttpMethods.IsPost check |
| RFC 6749 Section 4.1 | Authorization Code flow | AuthorizationService.cs | ProcessCodeFlowResponseAsync method |
| RFC 6749 Section 4.1.2 | Authorization code issuance | AuthorizationService.cs | SaveAuthorizationCodeAsync method |
| RFC 6749 Section 4.1.3 | Token request validation | TokenRequestValidator.cs | ValidateTokenRequestAsync method |
| RFC 6749 Section 4.2 | Implicit flow | Not implemented | Flow disabled |
| RFC 6749 Section 5.2 | Error response format | ErrorResult.cs | RFC-compliant error construction |
| RFC 6749 Section 6 | Refresh token flow | RefreshTokenFlowSpecification.cs | Token rotation implementation |
| RFC 6749 Section 10.12 | State parameter CSRF protection | AuthorizeRequestSpecification.cs | CheckState specification |
| RFC 6750 Section 2 | Bearer token usage | TokenGenerationService.cs | TokenType = "Bearer" |
| RFC 6750 Section 3 | Token validation | UserInfoRequestSpecification.cs | Signature and claim validation |
| RFC 6750 Section 3.1 | Invalid token errors | ErrorResult.cs | WWW-Authenticate header |
| RFC 7636 Section 4 | PKCE code challenge | AuthorizeRequestSpecification.cs | ValidatePkce specification |
| RFC 7636 Section 4.5 | Code verifier validation | ProofKeyParametersSpecification.cs | CheckCodeVerifierAgainstCodeChallenge |
| RFC 7515 | JWS signature | TokenExtension.cs | RSA/ECDSA signing implementations |
| RFC 7517 Section 4 | JWKS key format | JWKSService.cs | JsonWebKeyResponseModel |
| RFC 7519 Section 4 | JWT claims | TokenGenerationService.cs | GetNormalizedClaimsList method |
| RFC 7519 Section 4.1.4 | Expiration claim | TokenGenerationService.cs | exp claim with lifetime enforcement |
| RFC 7519 Section 4.1.7 | JTI claim | TokenGenerationService.cs | Random JWT ID generation |
| OIDC Core Section 2 | ID token issuance | TokenGenerationService.cs | GenerateIdentityTokenAsync method |
| OIDC Core Section 3.1.2.1 | Nonce support | AuthorizeRequestSpecification.cs | CheckNonce specification |
| OIDC Core Section 3.1.3.6 | at_hash claim | TokenGenerationService.cs | Access token hash calculation |
| OIDC Core Section 3.1.3.7 | c_hash claim | TokenGenerationService.cs | Authorization code hash calculation |
| OIDC Core Section 5.3 | UserInfo endpoint | UserInfoEndpoint.cs | Standard claims endpoint |
| OIDC Core Section 5.3.2 | UserInfo response | UserInfoServices.cs | Claims filtering by scope |
| OIDC Discovery Section 3 | Discovery metadata | DiscoveryService.cs | GenerateDiscoveryMetaData method |
| OIDC Discovery Section 4 | JWKS endpoint | JwksEndpoint.cs | Public key publication |

---

## 14. Security Threat Model Summary

### 14.1 Replay Attacks

**Threat:** Authorization codes or tokens intercepted and replayed.

**Mitigations:**
- Authorization codes consumed atomically on first use
- Refresh tokens rotated with reuse detection
- Short token lifetimes limit replay window
- JTI claim enables token-specific revocation

### 14.2 Code Interception

**Threat:** Malicious application intercepts authorization code on mobile device or insecure channel.

**Mitigations:**
- PKCE required for all flows - code useless without code_verifier
- Exact redirect URI matching prevents code delivery to wrong endpoint
- HTTPS required for all communications

### 14.3 Token Theft

**Threat:** Access tokens stolen from client storage or network traffic.

**Mitigations:**
- Short access token lifetimes (5-15 minutes)
- Refresh token rotation invalidates stolen refresh tokens
- Secure token storage requirements documented for clients
- Binding to client and session identifiers

### 14.4 CSRF Attacks

**Threat:** Cross-site request forgery against authorization endpoint.

**Mitigations:**
- Required `state` parameter with CSRF token
- SameSite=Strict on session cookies
- Exact redirect URI matching prevents open redirects

### 14.5 Open Redirect

**Threat:** Authorization endpoint used as open redirector.

**Mitigations:**
- Pre-registered redirect URIs only
- Exact string matching - no partial or wildcard matching
- URI scheme validation (HTTPS in production)

### 14.6 Token Forgery

**Threat:** Malicious client creates forged tokens.

**Mitigations:**
- RSA-256 or ECDSA-256 asymmetric signatures
- Private keys stored in HSM or secure key store
- Signature validation required by all resource servers

### 14.7 Brute Force

**Threat:** Credential guessing attacks against token endpoint.

**Mitigations:**
- Rate limiting on token endpoint
- Argon2 password hashing with high work factor
- Account lockout after failed attempts
- Monitoring and alerting on suspicious patterns

### 14.8 Privilege Escalation

**Threat:** Client requests broader scopes than authorized.

**Mitigations:**
- Client-specific allowed scopes configuration
- Scope validation at authorization and token endpoints
- Resource server scope enforcement on API access
- No wildcard scope support

---

## 15. Final Compliance Statement

### 15.1 Formal Declaration

The HCL.CS.SF Identity Platform formally declares compliance with the following specifications and security requirements:

#### 15.1.1 Authorization Code Flow

The system implements the OAuth 2.0 Authorization Code Grant as defined in RFC 6749 Section 4.1, with the following characteristics:

- Authorization codes are single-use credentials with cryptographically random values
- Codes have a maximum lifetime of 600 seconds
- Codes are bound to the requesting client identifier
- Codes are bound to the exact redirect URI from the authorization request

#### 15.1.2 PKCE Enforcement

Proof Key for Code Exchange (RFC 7636) is enforced as follows:

- PKCE is required for all public clients
- PKCE is required for all clients with `RequirePkce = true`
- Only the S256 code challenge method is supported
- Plain text code challenge method is explicitly rejected
- Code verifiers are validated using SHA-256 hash comparison

#### 15.1.3 JWT Signature Validation

JSON Web Tokens are secured using:

- RS256 (RSA with SHA-256) as the primary signing algorithm
- ES256 (ECDSA with P-256 and SHA-256) as an alternative
- Asymmetric cryptography enabling signature verification without secret exposure
- Key identifiers in JWT headers for key rotation support

#### 15.1.4 Invalid Token Rejection

The system rejects invalid tokens through:

- Signature verification on all tokens
- Expiration time validation
- Issuer claim validation
- Audience claim validation
- Token type validation (access_token vs id_token)

#### 15.1.5 Redirect URI Validation

Redirect URI security is enforced through:

- Pre-registration requirement for all redirect URIs
- Exact string matching using ordinal comparison
- No support for wildcards or partial matching
- HTTPS scheme enforcement in production

#### 15.1.6 Implicit Flow Prohibition

The OAuth 2.0 Implicit Flow (RFC 6749 Section 4.2) is not implemented or supported:

- `response_type=token` is rejected
- `response_type=id_token` is rejected
- Access tokens are never returned in URL fragments

#### 15.1.7 Wildcard Redirect URI Prohibition

Wildcard redirect URIs are explicitly prohibited:

- No subdomain wildcards (`*.example.com`)
- No path wildcards (`/callback/*`)
- No query string variations
- Exact match required for all registered URIs

#### 15.1.8 Secure Cryptographic Algorithms

Only secure cryptographic algorithms are used:

| Purpose | Algorithm | Status |
|---------|-----------|--------|
| Token Signing | RS256, ES256 | Approved |
| Token Signing | HS256 | Deprecated |
| Token Signing | none | Prohibited |
| Password Hashing | Argon2id | Approved |
| Client Secret Hashing | SHA-256, SHA-512 | Approved |
| Code Challenge | SHA-256 (S256) | Required |

### 15.2 Certification Readiness

This implementation is suitable for security certifications including:

- **SOC 2 Type II:** Security, availability, and confidentiality controls
- **ISO 27001:** Information security management
- **OpenID Certification:** OpenID Connect compliance certification
- **FAPI 2.0:** Financial-grade API security profile (with additional configuration)

### 15.3 Document Approval

This document serves as the authoritative technical specification for the HCL.CS.SF Identity Platform's OAuth 2.0 and OpenID Connect implementation.

---

**Document Control**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | February 2026 | Security Architecture Team | Initial release |

**Review Schedule:** Annual or upon significant architectural changes

**Distribution:** Security auditors, enterprise customers, internal architecture governance

---

*End of Document*
