# HCL.CS.SF API Reference

**Document ID:** HCL.CS.SF-DOC-04-API-REFERENCE  
**Version:** 1.0.0  
**Classification:** Internal Use  
**Last Updated:** 2026-03-01  

---

## Table of Contents

1. [Discovery Endpoint](#1-discovery-endpoint)
2. [Authorize Endpoint](#2-authorize-endpoint)
3. [Token Endpoint](#3-token-endpoint)
4. [JWKS Endpoint](#4-jwks-endpoint)
5. [Introspection Endpoint](#5-introspection-endpoint)
6. [Token Revocation Endpoint](#6-token-revocation-endpoint)
7. [UserInfo Endpoint](#7-userinfo-endpoint)
8. [EndSession Endpoint](#8-endsession-endpoint)
9. [Error Response Contract](#9-error-response-contract)
10. [Example Requests](#10-example-requests)

---

## 1. Discovery Endpoint

Returns OpenID Connect Discovery 1.0 compliant metadata.

### 1.1 Endpoint Specification

| Attribute | Value |
|-----------|-------|
| **Endpoint** | `/.well-known/openid-configuration` |
| **Methods** | GET |
| **Authentication** | None (public) |
| **Idempotency** | Yes (safe, read-only) |

**Source:** `/src/Identity/HCL.CS.SF.Identity.Application/Implementation/Endpoint/DiscoveryEndpoint.cs`

### 1.2 Request Parameters

None required. Optional `BaseUrl` derived from request context.

### 1.3 Response Payload

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

### 1.4 Response Model

**Source:** `/src/Identity/HCL.CS.SF.Identity.Domain/Models/Endpoint/Response/DiscoveryResult.cs`

| Field | Type | Description |
|-------|------|-------------|
| `issuer` | string | The issuer identifier URL |
| `authorization_endpoint` | string | URL of the authorization endpoint |
| `token_endpoint` | string | URL of the token endpoint |
| `userinfo_endpoint` | string | URL of the UserInfo endpoint |
| `jwks_uri` | string | URL of the JWKS endpoint |
| `end_session_endpoint` | string | URL of the end session endpoint |
| `introspection_endpoint` | string | URL of the introspection endpoint |
| `revocation_endpoint` | string | URL of the revocation endpoint |
| `scopes_supported` | string[] | List of supported scopes |
| `response_types_supported` | string[] | List of supported response types |
| `grant_types_supported` | string[] | List of supported grant types |
| `token_endpoint_auth_methods_supported` | string[] | Supported client auth methods |
| `id_token_signing_alg_values_supported` | string[] | Supported signing algorithms |
| `code_challenge_methods_supported` | string[] | Supported PKCE methods |

### 1.5 HTTP Status Codes

| Status | Condition |
|--------|-----------|
| 200 OK | Successful discovery |

---

## 2. Authorize Endpoint

Initiates the OAuth 2.0 Authorization Code flow with PKCE.

### 2.1 Endpoint Specification

| Attribute | Value |
|-----------|-------|
| **Endpoint** | `/security/authorize` |
| **Methods** | GET, POST |
| **Authentication** | None (triggers authentication) |
| **Content-Type** | `application/x-www-form-urlencoded` (POST) |
| **Idempotency** | No (creates authorization code) |

**Source:** `/src/Identity/HCL.CS.SF.Identity.Application/Implementation/Endpoint/AuthorizeEndpoint.cs`

### 2.2 Request Parameters

| Parameter | Required | Type | Description | Constraints |
|-----------|----------|------|-------------|-------------|
| `client_id` | Yes | string | Registered client identifier | Must be valid client |
| `response_type` | Yes | string | Must be `code` | Only `code` supported |
| `redirect_uri` | Yes | string | Pre-registered redirect URI | Exact match required |
| `scope` | Yes | string | Space-delimited scopes | Must be client-allowed |
| `state` | Yes | string | CSRF protection token | Min 16 chars recommended |
| `code_challenge` | Yes | string | PKCE code challenge | 43-128 chars, base64url |
| `code_challenge_method` | Yes | string | Must be `S256` | Only S256 supported |
| `nonce` | Recommended | string | Replay protection | Min 16 chars recommended |
| `response_mode` | No | string | Response delivery method | `query` or `form_post` |
| `prompt` | No | string | Authentication prompt | `login`, `none`, `select_account` |

### 2.3 Parameter Validation

**Source:** `/src/Identity/HCL.CS.SF.Identity.Application/Implementation/Endpoint/Specifications/AuthorizeRequestSpecification.cs`

| Validation Rule | Error Code | HTTP Status |
|-----------------|------------|-------------|
| Client exists | `invalid_client` | 400 |
| Response type supported | `unsupported_response_type` | 400 |
| Redirect URI registered | `invalid_request` | 400 |
| PKCE parameters present | `invalid_request` | 400 |
| Code challenge method is S256 | `invalid_request` | 400 |
| Scopes allowed for client | `invalid_scope` | 400 |

### 2.4 Success Response

**HTTP 302 Redirect** to `redirect_uri`:

```
Location: https://client.example.com/callback?code=AUTH_CODE&state=STATE_VALUE
```

| Parameter | Description |
|-----------|-------------|
| `code` | Authorization code (single-use, time-limited) |
| `state` | Echoed state parameter |

### 2.5 Error Response

**HTTP 302 Redirect** to `redirect_uri` (if valid):

```
Location: https://client.example.com/callback?error=invalid_request&error_description=...&state=STATE_VALUE
```

| Error Code | Description |
|------------|-------------|
| `invalid_request` | Malformed or missing parameters |
| `invalid_client` | Unknown or disabled client |
| `invalid_scope` | Requested scope not allowed |
| `unsupported_response_type` | Response type not supported |
| `server_error` | Internal server error |
| `access_denied` | User denied authorization |

---

## 3. Token Endpoint

Exchanges authorization codes or refresh tokens for access tokens.

### 3.1 Endpoint Specification

| Attribute | Value |
|-----------|-------|
| **Endpoint** | `/security/token` |
| **Methods** | POST |
| **Authentication** | Client authentication required (Basic Auth) |
| **Content-Type** | `application/x-www-form-urlencoded` |
| **Idempotency** | No (creates new tokens) |

**Source:** `/src/Identity/HCL.CS.SF.Identity.Application/Implementation/Endpoint/TokenEndpoint.cs`

### 3.2 Request Headers

| Header | Required | Description |
|--------|----------|-------------|
| `Authorization` | Yes (confidential clients) | `Basic base64(client_id:client_secret)` |
| `Content-Type` | Yes | `application/x-www-form-urlencoded` |

### 3.3 Request Parameters

#### Authorization Code Exchange

| Parameter | Required | Description |
|-----------|----------|-------------|
| `grant_type` | Yes | `authorization_code` |
| `code` | Yes | Authorization code from authorize endpoint |
| `redirect_uri` | Yes | Must match authorize request |
| `code_verifier` | Yes | PKCE code verifier |
| `client_id` | Conditional | Required if not in Authorization header |

#### Refresh Token Exchange

| Parameter | Required | Description |
|-----------|----------|-------------|
| `grant_type` | Yes | `refresh_token` |
| `refresh_token` | Yes | Refresh token |
| `scope` | No | Requested scopes (subset of original) |
| `client_id` | Conditional | Required if not in Authorization header |

#### Client Credentials

| Parameter | Required | Description |
|-----------|----------|-------------|
| `grant_type` | Yes | `client_credentials` |
| `scope` | No | Requested scopes |
| `client_id` | Conditional | Required if not in Authorization header |

### 3.4 Success Response

**HTTP 200 OK**

```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "refresh_token": "def50200a8c5e3b4...",
  "id_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "scope": "openid profile email"
}
```

| Field | Type | Description |
|-------|------|-------------|
| `access_token` | string | JWT access token |
| `token_type` | string | Always `Bearer` |
| `expires_in` | integer | Token lifetime in seconds |
| `refresh_token` | string | Refresh token (if `offline_access` scope) |
| `id_token` | string | OIDC ID token (if `openid` scope) |
| `scope` | string | Granted scopes |

**Source:** `/src/Identity/HCL.CS.SF.Identity.Domain/Models/Endpoint/Response/TokenResponseModel.cs`

### 3.5 Error Response

**HTTP 400 Bad Request** (or per error code)

```json
{
  "error": "invalid_grant",
  "error_description": "The authorization code has expired or is invalid."
}
```

| Error Code | HTTP Status | Description |
|------------|-------------|-------------|
| `invalid_request` | 400 | Malformed request |
| `invalid_client` | 401 | Client authentication failed |
| `invalid_grant` | 400 | Invalid/expired code or refresh token |
| `unauthorized_client` | 400 | Client not authorized for grant type |
| `unsupported_grant_type` | 400 | Grant type not supported |
| `invalid_scope` | 400 | Invalid or unauthorized scope |
| `server_error` | 500 | Internal server error |

### 3.6 Client Authentication

Confidential clients must authenticate using HTTP Basic Authentication:

```
Authorization: Basic base64(client_id:client_secret)
```

Example:
```
Authorization: Basic bXktY2xpZW50LWlkOm15LWNsaWVudC1zZWNyZXQ=
```

---

## 4. JWKS Endpoint

Returns JSON Web Key Set for token signature verification.

### 4.1 Endpoint Specification

| Attribute | Value |
|-----------|-------|
| **Endpoint** | `/.well-known/openid-configuration/jwks` |
| **Methods** | GET |
| **Authentication** | None (public, if enabled) |
| **Idempotency** | Yes |

**Source:** `/src/Identity/HCL.CS.SF.Identity.Application/Implementation/Endpoint/JwksEndpoint.cs`

### 4.2 Response Payload

```json
{
  "keys": [
    {
      "kty": "RSA",
      "use": "sig",
      "kid": "2026-01-signing-key",
      "x5t": "aBcDeFgHiJkLmNoP",
      "n": "xGOrWEZVBb1prf7a7fI7BpoR8Y3X1H-Q3g6vQ8Vm8c2...",
      "e": "AQAB",
      "alg": "RS256"
    },
    {
      "kty": "EC",
      "use": "sig",
      "kid": "2026-02-ecdsa-key",
      "crv": "P-256",
      "x": "MKBCTNIcKUSDii11ySs3526iDZ8AiTo7Tu6KPAqv7D4",
      "y": "4Etl6SRW2YiLUrN5vfvVHuhp7x8PxltmWWlbbM4IFyM",
      "alg": "ES256"
    }
  ]
}
```

### 4.3 Response Model

**Source:** `/src/Identity/HCL.CS.SF.Identity.Domain/Models/Endpoint/Response/JsonWebKeyResponseModel.cs`

| Field | Type | Description |
|-------|------|-------------|
| `kty` | string | Key type (RSA, EC) |
| `use` | string | Key use (sig for signing) |
| `kid` | string | Key ID for key identification |
| `alg` | string | Algorithm (RS256, ES256) |
| `n`, `e` | string | RSA modulus and exponent |
| `crv`, `x`, `y` | string | EC curve and coordinates |

### 4.4 Configuration

JWKS endpoint can be disabled via configuration:

```json
{
  "TokenSettings": {
    "TokenConfig": {
      "ShowKeySet": false
    }
  }
}
```

When disabled, returns HTTP 404.

---

## 5. Introspection Endpoint

Validates tokens and returns token metadata.

### 5.1 Endpoint Specification

| Attribute | Value |
|-----------|-------|
| **Endpoint** | `/security/introspect` |
| **Methods** | POST |
| **Authentication** | Client authentication required |
| **Content-Type** | `application/x-www-form-urlencoded` |
| **Idempotency** | Yes (read-only) |

**Source:** `/src/Identity/HCL.CS.SF.Identity.Application/Implementation/Endpoint/IntrospectionEndpoint.cs`

### 5.2 Request Parameters

| Parameter | Required | Description |
|-----------|----------|-------------|
| `token` | Yes | Token to introspect |
| `token_type_hint` | No | `access_token` or `refresh_token` |
| `scope` | No | Scope to verify (space-delimited) |

### 5.3 Request Headers

| Header | Required | Description |
|--------|----------|-------------|
| `Authorization` | Yes | `Basic base64(client_id:client_secret)` |
| `Content-Type` | Yes | `application/x-www-form-urlencoded` |

### 5.4 Success Response (Active Token)

**HTTP 200 OK**

```json
{
  "active": true,
  "scope": "openid profile email",
  "client_id": "my-client-id",
  "token_type": "Bearer",
  "exp": 1704067200,
  "iat": 1704063600,
  "sub": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "aud": "my-client-id",
  "iss": "https://identity.HCL.CS.SF.example"
}
```

### 5.5 Success Response (Inactive Token)

**HTTP 200 OK**

```json
{
  "active": false
}
```

### 5.6 Response Model

**Source:** `/src/Identity/HCL.CS.SF.Identity.Domain/Models/Endpoint/Response/IntrospectionResponseModel.cs`

| Field | Type | Description |
|-------|------|-------------|
| `active` | boolean | Token validity status |
| `scope` | string | Granted scopes |
| `client_id` | string | Token client |
| `token_type` | string | Token type |
| `exp` | integer | Expiration timestamp |
| `iat` | integer | Issued at timestamp |
| `sub` | string | Subject (user ID) |
| `aud` | string | Audience |
| `iss` | string | Issuer |

---

## 6. Token Revocation Endpoint

Revokes access or refresh tokens.

### 6.1 Endpoint Specification

| Attribute | Value |
|-----------|-------|
| **Endpoint** | `/security/revocation` |
| **Methods** | POST |
| **Authentication** | Client authentication required |
| **Content-Type** | `application/x-www-form-urlencoded` |
| **Idempotency** | Yes (idempotent operation) |

**Source:** `/src/Identity/HCL.CS.SF.Identity.Application/Implementation/Endpoint/TokenRevocationEndpoint.cs`

### 6.2 Request Parameters

| Parameter | Required | Description |
|-----------|----------|-------------|
| `token` | Yes | Token to revoke |
| `token_type_hint` | No | `access_token` or `refresh_token` |

### 6.3 Request Headers

| Header | Required | Description |
|--------|----------|-------------|
| `Authorization` | Yes | `Basic base64(client_id:client_secret)` |
| `Content-Type` | Yes | `application/x-www-form-urlencoded` |

### 6.4 Success Response

**HTTP 200 OK**

Empty response body (per RFC 7009).

**Note:** Returns 200 OK even if token does not exist or is already revoked (token enumeration prevention).

### 6.5 Error Response

```json
{
  "error": "invalid_client",
  "error_description": "Client authentication failed."
}
```

---

## 7. UserInfo Endpoint

Returns claims about the authenticated user.

### 7.1 Endpoint Specification

| Attribute | Value |
|-----------|-------|
| **Endpoint** | `/security/userinfo` |
| **Methods** | GET, POST |
| **Authentication** | Bearer token required |
| **Content-Type** | N/A (GET) or `application/x-www-form-urlencoded` (POST) |
| **Idempotency** | Yes |

**Source:** `/src/Identity/HCL.CS.SF.Identity.Application/Implementation/Endpoint/UserInfoEndpoint.cs`

### 7.2 Request Headers

| Header | Required | Description |
|--------|----------|-------------|
| `Authorization` | Yes | `Bearer {access_token}` |

### 7.3 Success Response

**HTTP 200 OK**

```json
{
  "sub": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "preferred_username": "john.doe",
  "given_name": "John",
  "family_name": "Doe",
  "email": "john.doe@example.com",
  "email_verified": true,
  "role": ["User", "Admin"]
}
```

### 7.4 Claims Returned

| Claim | Scope Required | Description |
|-------|----------------|-------------|
| `sub` | `openid` | Subject identifier (user ID) |
| `preferred_username` | `profile` | Username |
| `given_name` | `profile` | First name |
| `family_name` | `profile` | Last name |
| `email` | `email` | Email address |
| `email_verified` | `email` | Email verification status |
| `role` | `profile` | User roles |

### 7.5 Error Response

| Error Code | HTTP Status | Description |
|------------|-------------|-------------|
| `invalid_token` | 401 | Missing or invalid access token |
| `insufficient_scope` | 403 | Token lacks required scope |

---

## 8. EndSession Endpoint

Initiates logout and ends the user session.

### 8.1 Endpoint Specification

| Attribute | Value |
|-----------|-------|
| **Endpoint** | `/security/endsession` |
| **Methods** | GET |
| **Authentication** | None (user session based) |
| **Idempotency** | No (modifies session state) |

**Source:** `/src/Identity/HCL.CS.SF.Identity.Application/Implementation/Endpoint/EndSessionEndpoint.cs`

### 8.2 Request Parameters

| Parameter | Required | Description |
|-----------|----------|-------------|
| `id_token_hint` | Recommended | ID token for client identification |
| `post_logout_redirect_uri` | Conditional | Redirect after logout (must be pre-registered) |
| `state` | No | State to return after logout |
| `sid` | No | Session ID hint |

### 8.3 Success Response

**HTTP 302 Redirect** (if `post_logout_redirect_uri` provided and valid):

```
Location: https://client.example.com/logged-out?state=STATE_VALUE
```

**HTTP 200 OK** with logout confirmation page (if no redirect URI).

### 8.4 Error Response

If `post_logout_redirect_uri` is invalid, error displayed on logout page (not redirected).

---

## 9. Error Response Contract

### 9.1 Standard Error Format

**Source:** `/src/Identity/HCL.CS.SF.Identity.Domain/Models/Endpoint/Response/ErrorResponseModel.cs`

```json
{
  "error": "invalid_request",
  "error_description": "The request is missing a required parameter."
}
```

| Field | Type | Description |
|-------|------|-------------|
| `error` | string | Error code (machine-readable) |
| `error_description` | string | Human-readable description |
| `error_uri` | string | Optional URI with more information |
| `state` | string | Echoed state parameter (if provided) |

### 9.2 Error Codes Reference

**Source:** `/src/Identity/HCL.CS.SF.Identity.Domain/Constants/Endpoint/OpenIdConstants.cs`

| Error Code | Description | Typical HTTP Status |
|------------|-------------|---------------------|
| `invalid_request` | Malformed or missing parameters | 400 |
| `invalid_client` | Client authentication failed | 401 |
| `invalid_grant` | Invalid/expired authorization grant | 400 |
| `unauthorized_client` | Client not authorized | 400 |
| `unsupported_grant_type` | Grant type not supported | 400 |
| `unsupported_response_type` | Response type not supported | 400 |
| `invalid_scope` | Invalid or unauthorized scope | 400 |
| `invalid_token` | Token is invalid or expired | 401 |
| `insufficient_scope` | Token lacks required scope | 403 |
| `access_denied` | User denied authorization | 400 |
| `server_error` | Internal server error | 500 |
| `temporarily_unavailable` | Service temporarily unavailable | 503 |

### 9.3 HTTP Status Code Mapping

**Source:** `/src/Identity/HCL.CS.SF.Identity.Domain/Constants/Endpoint/OpenIdConstants.cs` - `HTTPStatusCodes`

```csharp
var errorStatusCode = openIdErrorCode switch
{
    "invalid_request" => 400,
    "invalid_client" => 401,
    "invalid_grant" => 400,
    "unauthorized_client" => 400,
    "unsupported_grant_type" => 400,
    "unsupported_response_type" => 400,
    "invalid_scope" => 400,
    "invalid_token" => 401,
    "insufficient_scope" => 403,
    "access_denied" => 400,
    "server_error" => 500,
    "temporarily_unavailable" => 503,
    _ => 400
};
```

---

## 10. Example Requests

### 10.1 Discovery Request

```bash
curl -X GET \
  https://identity.HCL.CS.SF.example/.well-known/openid-configuration
```

### 10.2 Authorization Request

```bash
# Step 1: Generate PKCE parameters
CODE_VERIFIER=$(openssl rand -base64 32 | tr -d '=+/')
CODE_CHALLENGE=$(echo -n "$CODE_VERIFIER" | openssl dgst -sha256 -binary | openssl base64 | tr -d '=+/')
STATE=$(openssl rand -hex 16)
NONCE=$(openssl rand -hex 16)

# Step 2: Redirect user to authorize endpoint
# (Browser redirect)
https://identity.HCL.CS.SF.example/security/authorize?\
  client_id=my-client-id&\
  response_type=code&\
  redirect_uri=https%3A%2F%2Fclient.example.com%2Fcallback&\
  scope=openid%20profile%20email&\
  state=$STATE&\
  nonce=$NONCE&\
  code_challenge=$CODE_CHALLENGE&\
  code_challenge_method=S256
```

### 10.3 Token Exchange (Authorization Code)

```bash
curl -X POST \
  https://identity.HCL.CS.SF.example/security/token \
  -H "Authorization: Basic $(echo -n 'my-client-id:my-client-secret' | base64)" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=authorization_code" \
  -d "code=AUTH_CODE_FROM_CALLBACK" \
  -d "redirect_uri=https://client.example.com/callback" \
  -d "code_verifier=$CODE_VERIFIER"
```

### 10.4 Token Exchange (Refresh Token)

```bash
curl -X POST \
  https://identity.HCL.CS.SF.example/security/token \
  -H "Authorization: Basic $(echo -n 'my-client-id:my-client-secret' | base64)" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=refresh_token" \
  -d "refresh_token=def50200a8c5e3b4..." \
  -d "scope=openid profile"
```

### 10.5 Client Credentials

```bash
curl -X POST \
  https://identity.HCL.CS.SF.example/security/token \
  -H "Authorization: Basic $(echo -n 'my-client-id:my-client-secret' | base64)" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials" \
  -d "scope=api:read api:write"
```

### 10.6 Token Introspection

```bash
curl -X POST \
  https://identity.HCL.CS.SF.example/security/introspect \
  -H "Authorization: Basic $(echo -n 'my-client-id:my-client-secret' | base64)" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "token=eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -d "token_type_hint=access_token"
```

### 10.7 Token Revocation

```bash
curl -X POST \
  https://identity.HCL.CS.SF.example/security/revocation \
  -H "Authorization: Basic $(echo -n 'my-client-id:my-client-secret' | base64)" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "token=def50200a8c5e3b4..." \
  -d "token_type_hint=refresh_token"
```

### 10.8 UserInfo Request

```bash
curl -X GET \
  https://identity.HCL.CS.SF.example/security/userinfo \
  -H "Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### 10.9 JWKS Request

```bash
curl -X GET \
  https://identity.HCL.CS.SF.example/.well-known/openid-configuration/jwks
```

### 10.10 End Session Request

```bash
# Browser redirect
curl -X GET \
  "https://identity.HCL.CS.SF.example/security/endsession?id_token_hint=ID_TOKEN&post_logout_redirect_uri=https%3A%2F%2Fclient.example.com%2Flogged-out"
```

---

## 11. Audit and Logging Behavior

### 11.1 Logged Events

| Endpoint | Events Logged | Correlation ID |
|----------|---------------|----------------|
| Discovery | Request received, response sent | Yes |
| Authorize | Request validation, code issued, errors | Yes |
| Token | Token issued, grant type, client ID | Yes |
| Introspection | Token introspected, result | Yes |
| Revocation | Token revoked | Yes |
| UserInfo | User info requested, claims returned | Yes |
| EndSession | Logout initiated/completed | Yes |

### 11.2 Sensitive Data Redaction

Per `LogRedactionHelper.cs`, the following fields are redacted in logs:
- Passwords, secrets, tokens
- Authorization headers
- Cookies
- API keys
- Email addresses
- Phone numbers

---

## Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2026-03-01 | Enterprise Documentation Team | Initial release |
