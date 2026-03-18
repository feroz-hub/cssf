# HCL.CS.SF Identity Server - Full Runtime Execution Manual

**Document Version:** 2.0  
**Generated:** 2026-02-26  
**Project:** HCL.CS.SF Identity Server (OAuth 2.0 / OpenID Connect)  

---

# TABLE OF CONTENTS

1. [Phase 0: Dotnet Secret Setup Procedure](#phase-0--dotnet-secret-setup-procedure-execute-first)
2. [Phase 1: Pre-Start Analysis](#phase-1-pre-start-analysis)
3. [Phase 2: Server Startup Execution Trace](#phase-2-server-startup-execution-trace)
4. [Phase 3: Client Startup Trace](#phase-3-client-startup-trace)
5. [Phase 4: Full Login Flow Trace](#phase-4-full-login-flow-trace)
6. [Phase 5: Refresh Flow Trace](#phase-5-refresh-flow-trace)
7. [Phase 6: Logout Flow Trace](#phase-6-logout-flow-trace)
8. [Phase 7: Configuration Dependency Map](#phase-7-configuration-dependency-map)
9. [Phase 8: Failure Matrix](#phase-8-failure-matrix)
10. [Phase 9: Production Hardening Checklist](#phase-9-production-hardening-checklist)
11. [Phase 10: Startup Order Summary](#phase-10-startup-order-summary)

---

# 🔎 PHASE 0 — DOTNET SECRET SETUP PROCEDURE (Execute First)

Before any runtime execution, initialize user secrets for all projects:

## Identity Server (HCL.CS.SF.Hosting)

```bash
cd src/Identity/HCL.CS.SF.Identity.API
dotnet user-secrets init
dotnet user-secrets set "HCL.CS.SF_DB_CONNECTION_STRING" "Host=localhost;Port=5432;Database=HCL.CS.SF_identity;Username=HCL.CS.SF_user;Password=<PROD_PASSWORD>"
dotnet user-secrets set "HCL.CS.SF_SIGNING_CERT_PASSWORD" "<CERT_PASSWORD>"
dotnet user-secrets set "HCL.CS.SF_SMTP_USERNAME" "<SMTP_USER>"
dotnet user-secrets set "HCL.CS.SF_SMTP_PASSWORD" "<SMTP_PASSWORD>"
dotnet user-secrets set "HCL.CS.SF_SMS_ACCOUNT_ID" "<SMS_ACCOUNT_ID>"
dotnet user-secrets set "HCL.CS.SF_SMS_ACCOUNT_PASSWORD" "<SMS_PASSWORD>"
dotnet user-secrets set "HCL.CS.SF_SMS_ACCOUNT_FROM" "<SMS_FROM_NUMBER>"
```

## Gateway Service (HCL.CS.SF.ProxyService)

```bash
cd src/Gateway/HCL.CS.SF.Gateway
dotnet user-secrets init
dotnet user-secrets set "HCL.CS.SF_DB_CONNECTION_STRING" "Host=localhost;Port=5432;Database=HCL.CS.SF_identity;Username=HCL.CS.SF_user;Password=<PROD_PASSWORD>"
```

## Demo Server (if applicable)

```bash
cd demos/HCL.CS.SF.Demo.Server
dotnet user-secrets init
dotnet user-secrets set "HCL.CS.SF_DB_CONNECTION_STRING" "Data Source=HCL.CS.SF.db"
dotnet user-secrets set "HCL.CS.SF_RSA_SIGNING_CERT_PATH" "./certificates/HCL.CS.SF_rsa.pfx"
dotnet user-secrets set "HCL.CS.SF_ECDSA_SIGNING_CERT_PATH" "./certificates/HCL.CS.SF_ecdsa.pfx"
dotnet user-secrets set "HCL.CS.SF_SIGNING_CERT_PASSWORD" "<CERT_PASSWORD>"
```

## Demo Client MVC (if applicable)

```bash
cd demos/HCL.CS.SF.Demo.Client.Mvc
dotnet user-secrets init
dotnet user-secrets set "OAuth__ClientSecret" "<CLIENT_SECRET>"
dotnet user-secrets set "OAuth__ClientId" "<CLIENT_ID>"
dotnet user-secrets set "OAuth__Authority" "https://localhost:5001"
```

## Verify Secrets Loaded

```bash
dotnet user-secrets list
```

## Environment-Specific Secret Sources

| Environment | Secret Source | Configuration |
|-------------|---------------|---------------|
| Development | `secrets.json` (auto-generated) | `%APPDATA%/Microsoft/UserSecrets/<UserSecretsId>/secrets.json` |
| Staging | Azure Key Vault | `AddAzureKeyVault()` in Program.cs |
| Production | Container orchestration | Kubernetes Secrets / Docker Secrets |

---

# PHASE 1: PRE-START ANALYSIS

## A. Required File Structure

### Server Components

| File/Directory | Purpose | Status |
|----------------|---------|--------|
| `src/Identity/HCL.CS.SF.Identity.API/Program.cs` | Application entry point | **CRITICAL** |
| `src/Identity/HCL.CS.SF.Identity.Infrastructure.Resources/Settings/SystemSettings.json` | System configuration | **CRITICAL** |
| `src/Identity/HCL.CS.SF.Identity.Infrastructure.Resources/Settings/TokenSettings.json` | OAuth/OIDC endpoint settings | **CRITICAL** |
| `src/Identity/HCL.CS.SF.Identity.Infrastructure.Resources/Settings/NotificationTemplateSettings.json` | Email/SMS templates | **CRITICAL** |
| `demos/HCL.CS.SF.Demo.Server/Program.cs` | Demo host entry point | **CRITICAL** |
| `demos/HCL.CS.SF.Demo.Server/appsettings.json` | ASP.NET Core configuration | **CRITICAL** |
| Certificates (RSA/ECDSA .pfx files) | Token signing keys | **CRITICAL** |

### Client Components (Demo MVC Client)

| File/Directory | Purpose | Status |
|----------------|---------|--------|
| `demos/HCL.CS.SF.Demo.Client.Mvc/Program.cs` | Client application entry | **CRITICAL** |
| `demos/HCL.CS.SF.Demo.Client.Mvc/appsettings.json` | OAuth client configuration | **CRITICAL** |

### Library Components (Source)

| File | Purpose |
|------|---------|
| `src/Identity/HCL.CS.SF.Identity.API/Extensions/HCL.CS.SFExtension.cs` | Main DI registration |
| `src/Identity/HCL.CS.SF.Identity.API/Extensions/HCL.CS.SFBuilder.cs` | Middleware registration |
| `src/Identity/HCL.CS.SF.Identity.Application/Extension/ServiceExtension.cs` | Core services registration |
| `src/Identity/HCL.CS.SF.Identity.Persistence/Extension/InfrastructureDataExtension.cs` | EF Core & Identity registration |
| `src/Identity/HCL.CS.SF.Identity.Infrastructure.Resources/Extension/InfrastructureResourceExtension.cs` | Keystore & resources |
| `src/Gateway/HCL.CS.SF.Gateway/Extension/ServiceExtension.cs` | Proxy services registration |
| `src/Gateway/HCL.CS.SF.Gateway/Hosting/HCL.CS.SFEndpointMiddleware.cs` | OAuth endpoint middleware |
| `src/Gateway/HCL.CS.SF.Gateway/Hosting/HCL.CS.SFApiMiddleware.cs` | API proxy middleware |

---

## B. Required Environment Variables

### Database Configuration

| Variable | Purpose | Read Location |
|----------|---------|---------------|
| `HCL.CS.SF_DB_CONNECTION_STRING` | Database connection string | `SystemSettings.json` `${HCL.CS.SF_DB_CONNECTION_STRING}` |

### Certificate Configuration

| Variable | Purpose | Read Location |
|----------|---------|---------------|
| `HCL.CS.SF_RSA_SIGNING_CERT_BASE64` | RSA certificate (Base64) | `Program.cs` `LoadCertificateFromEnvironment()` |
| `HCL.CS.SF_RSA_SIGNING_CERT_PATH` | RSA certificate file path | `Program.cs` `LoadCertificateFromEnvironment()` |
| `HCL.CS.SF_ECDSA_SIGNING_CERT_BASE64` | ECDSA certificate (Base64) | `Program.cs` `LoadCertificateFromEnvironment()` |
| `HCL.CS.SF_ECDSA_SIGNING_CERT_PATH` | ECDSA certificate file path | `Program.cs` `LoadCertificateFromEnvironment()` |
| `HCL.CS.SF_SIGNING_CERT_PASSWORD` | Certificate password | `Program.cs` `LoadAsymmetricCertificate()` |
| `HCL.CS.SF_RSA_SIGNING_KID` | RSA Key ID | `Program.cs` (optional, defaults to "HCL.CS.SF-rsa-current") |
| `HCL.CS.SF_ECDSA_SIGNING_KID` | ECDSA Key ID | `Program.cs` (optional, defaults to "HCL.CS.SF-ecdsa-current") |

### Email Configuration

| Variable | Purpose | Read Location |
|----------|---------|---------------|
| `HCL.CS.SF_SMTP_USERNAME` | SMTP username | `SystemSettings.json` `${HCL.CS.SF_SMTP_USERNAME}` |
| `HCL.CS.SF_SMTP_PASSWORD` | SMTP password | `SystemSettings.json` `${HCL.CS.SF_SMTP_PASSWORD}` |

### SMS Configuration

| Variable | Purpose | Read Location |
|----------|---------|---------------|
| `HCL.CS.SF_SMS_ACCOUNT_ID` | SMS provider account ID | `SystemSettings.json` `${HCL.CS.SF_SMS_ACCOUNT_ID}` |
| `HCL.CS.SF_SMS_ACCOUNT_PASSWORD` | SMS provider password | `SystemSettings.json` `${HCL.CS.SF_SMS_ACCOUNT_PASSWORD}` |
| `HCL.CS.SF_SMS_ACCOUNT_FROM` | SMS sender number | `SystemSettings.json` `${HCL.CS.SF_SMS_ACCOUNT_FROM}` |

### ASP.NET Core Environment

| Variable | Purpose | Default |
|----------|---------|---------|
| `ASPNETCORE_ENVIRONMENT` | Environment (Development/Production) | Production |
| `ASPNETCORE_URLS` | Binding URLs | `https://localhost:5001` |

### Client Application (OAuth)

| Variable | Purpose | Read Location |
|----------|---------|---------------|
| `OAuth__ClientSecret` | Client application secret | `appsettings.json` override |
| `OAuth__ClientId` | Client application ID | `appsettings.json` override |
| `OAuth__Authority` | Identity server URL | `appsettings.json` override |

---

## C. Database Preparation

### Required Tables (Entity Framework Migrations)

#### Identity Tables (ASP.NET Core Identity)
| Table | Purpose | Entity Class |
|-------|---------|--------------|
| `AspNetUsers` | User accounts | `Users` |
| `AspNetRoles` | Role definitions | `Roles` |
| `AspNetUserRoles` | User-role mappings | `UserRoles` |
| `AspNetUserClaims` | User claims | `UserClaims` |
| `AspNetRoleClaims` | Role claims | `RoleClaims` |
| `AspNetUserLogins` | External login providers | `UserLogins` |
| `AspNetUserTokens` | User tokens | `UserTokens` |

#### OAuth/OIDC Tables
| Table | Purpose | Entity Class |
|-------|---------|--------------|
| `HCL.CS.SF_Clients` | OAuth client registrations | `Clients` |
| `HCL.CS.SF_ClientRedirectUris` | Allowed redirect URIs | `ClientRedirectUris` |
| `HCL.CS.SF_ClientPostLogoutRedirectUris` | Post-logout redirect URIs | `ClientPostLogoutRedirectUris` |
| `HCL.CS.SF_SecurityTokens` | Authorization codes, refresh tokens, access tokens | `SecurityTokens` |

#### Resource Management Tables
| Table | Purpose | Entity Class |
|-------|---------|--------------|
| `HCL.CS.SF_ApiResources` | API resource definitions | `ApiResources` |
| `HCL.CS.SF_ApiResourceClaims` | API resource claims | `ApiResourceClaims` |
| `HCL.CS.SF_ApiScopes` | API scopes | `ApiScopes` |
| `HCL.CS.SF_ApiScopeClaims` | API scope claims | `ApiScopeClaims` |
| `HCL.CS.SF_IdentityResources` | Identity resources (openid, profile, email) | `IdentityResources` |
| `HCL.CS.SF_IdentityClaims` | Identity resource claims | `IdentityClaims` |

#### Additional Tables
| Table | Purpose | Entity Class |
|-------|---------|--------------|
| `HCL.CS.SF_AuditTrail` | Audit logging | `AuditTrail` |
| `HCL.CS.SF_Notification` | Notifications | `Notification` |
| `HCL.CS.SF_SecurityQuestions` | Security questions | `SecurityQuestions` |
| `HCL.CS.SF_UserSecurityQuestions` | User security answers | `UserSecurityQuestions` |
| `HCL.CS.SF_PasswordHistory` | Password history | `PasswordHistory` |

### Migration Process

1. **EF Core Migration Command:**
   ```bash
   dotnet ef migrations add InitialCreate --project src/Identity/HCL.CS.SF.Identity.Persistence
   dotnet ef database update --project demos/HCL.CS.SF.Demo.Server
   ```

2. **Database Initialization on Startup:**
   - `ApplicationDbContext` is registered in `InfrastructureDataExtension.AddIdentityConfiguration()`
   - Database provider determined by `SystemSettings.DBConfig.Database`:
     - `1` = SQL Server
     - `2` = MySQL
     - `3` = PostgreSQL
     - `4` = SQLite

3. **What Happens if DB Missing:**
   - Startup VALIDATION occurs in `HCL.CS.SFExtension.ValidateDatabaseConfiguration()`
   - Connection is tested during `AddHCL.CS.SF()` call
   - **FAILURE:** `AggregateException` thrown with message "Database connection string is not configured" or connection error
   - Application will NOT start without valid database connection

### Clients Table Requirements

Required fields for a functional OAuth client:

| Field | Required | Description |
|-------|----------|-------------|
| `ClientId` | YES | Unique client identifier (string, max 128) |
| `ClientSecret` | Conditional | Required if `RequireClientSecret=true` |
| `ClientName` | YES | Display name |
| `AllowedScopes` | YES | Space-separated allowed scopes |
| `SupportedGrantTypes` | YES | e.g., "authorization_code", "client_credentials" |
| `SupportedResponseTypes` | YES | e.g., "code", "token" |
| `RequirePkce` | Recommended | true for public clients |
| `RequireClientSecret` | YES | true for confidential clients |
| `AllowedSigningAlgorithm` | YES | RS256, ES256, PS256, HS256, etc. |
| `AccessTokenExpiration` | YES | Seconds until access token expires |
| `RefreshTokenExpiration` | YES | Seconds until refresh token expires |
| `IdentityTokenExpiration` | YES | Seconds until ID token expires |

---

# PHASE 2: SERVER STARTUP EXECUTION TRACE

## Command: `dotnet run` (or `dotnet demos/HCL.CS.SF.Demo.Server/bin/Debug/net8.0/HCL.CS.SF.Demo.Server.dll`)

### Step-by-Step Execution Trace

| Step | File | Method | What Executes | Dependencies | Failure Condition |
|------|------|--------|---------------|--------------|-------------------|
| 1 | `Program.cs` | `Main()` | Entry point invoked | OS runtime | Process termination |
| 2 | `Program.cs` | `WebApplication.CreateBuilder()` | ASP.NET Core host initialization | `ASPNETCORE_ENVIRONMENT` | Environment variable missing |
| 3 | `Program.cs` | `LoadConfiguration()` | Load SystemSettings.json, TokenSettings.json, NotificationTemplateSettings.json | Files must exist | `FileNotFoundException` |
| 4 | `Program.cs` | `LoadAsymmetricCertificate()` | Load RSA/ECDSA certificates from env vars or files | `HCL.CS.SF_RSA_SIGNING_CERT_PATH`, `HCL.CS.SF_SIGNING_CERT_PASSWORD` | `CryptographicException` if cert invalid |
| 5 | `HCL.CS.SFExtension.cs` | `AddHCL.CS.SF()` | Main DI registration entry point | Configuration objects | Validation failures |
| 6 | `HCL.CS.SFExtension.cs` | `ValidateConfiguration()` | Validate DB, LDAP, Email, SMS, Token configs | All config sections | `AggregateException` with error list |
| 7 | `HCL.CS.SFExtension.cs` | `ValidateDatabaseConfiguration()` | Test database connection | `HCL.CS.SF_DB_CONNECTION_STRING` | `SqlException`, `NpgsqlException`, etc. |
| 8 | `InfrastructureDataExtension.cs` | `AddIdentityConfiguration()` | Register EF Core DbContext | Database provider | Provider not supported |
| 9 | `InfrastructureDataExtension.cs` | `AddIdentityServices()` | Configure ASP.NET Core Identity | DbContext | Identity options invalid |
| 10 | `ServiceExtension.cs` | `AddCoreServices()` | Register OAuth endpoint services | TokenSettings | Endpoint config missing |
| 11 | `ServiceExtension.cs` | `AddDefaultEndpoints()` | Register OAuth endpoints (Authorize, Token, etc.) | Core services | Endpoint path conflicts |
| 12 | `HCL.CS.SFExtension.cs` | `AddAsymmetricKeystore()` | Register signing certificates | Certificate loaded in step 4 | Keystore null or empty |
| 13 | `Program.cs` | `Build()` | Build ServiceProvider | All DI registrations | Circular dependency |
| 14 | `Program.cs` | `UseHCL.CS.SFSecurityHeaders()` | Add security headers middleware | `HCL.CS.SFBuilder.cs` | None |
| 15 | `Program.cs` | `UseAuthentication()` | Add authentication middleware | Identity services | None |
| 16 | `Program.cs` | `UseHCL.CS.SFEndpoint()` | Add OAuth endpoint middleware | `HCL.CS.SFEndpointMiddleware` | Endpoint service missing |
| 17 | `Program.cs` | `UseHCL.CS.SFApi()` | Add API proxy middleware | `HCL.CS.SFApiMiddleware` | API gateway service missing |
| 18 | `Program.cs` | `Run()` | Start Kestrel, bind to ports | `ASPNETCORE_URLS` | Port already in use |

### Certificate Loading Process

```
Program.cs LoadAsymmetricCertificate()
├── Check HCL.CS.SF_RSA_SIGNING_CERT_BASE64 env var
│   └── If set: Decode Base64 → Load PFX → Validate private key
├── Check HCL.CS.SF_RSA_SIGNING_CERT_PATH env var
│   └── If set: Load from path → Validate password → Validate private key
├── Check HCL.CS.SF_ECDSA_SIGNING_CERT_BASE64 env var
│   └── If set: Decode Base64 → Load PFX → Validate private key
├── Check HCL.CS.SF_ECDSA_SIGNING_CERT_PATH env var
│   └── If set: Load from path → Validate password → Validate private key
└── Return List<AsymmetricKeyInfoModel>
    └── KeyStore.cs validates algorithm compatibility
```

### Middleware Execution Order

```
HTTP Request
├── UseHCL.CS.SFSecurityHeaders()          [FIRST - adds HSTS, X-Frame-Options, CSP]
├── UseAuthentication()                  [Validates JWT/cookies]
├── UseHCL.CS.SFEndpoint()                  [OAuth/OIDC endpoints - Authorize, Token, etc.]
│   ├── HCL.CS.SFEndpointMiddleware.InvokeAsync()
│   ├── Find matching endpoint by path
│   └── Call IEndpoint.ProcessAsync()
├── UseHCL.CS.SFApi()                       [Management API endpoints]
│   └── HCL.CS.SFApiMiddleware.InvokeAsync()
└── 404 if no endpoint matched
```

### Endpoint Registration Order

```
ServiceExtension.AddDefaultEndpoints()
├── TokenEndpoint        → POST /security/token
├── AuthorizeEndpoint    → GET/POST /security/authorize
├── AuthorizeCallBackEndpoint → /security/authorize/authorizecallback
├── IntrospectionEndpoint → POST /security/introspect
├── DiscoveryEndpoint    → GET /.well-known/openid-configuration
├── EndSessionEndpoint   → GET/POST /security/endsession
├── EndSessionCallbackEndpoint → /security/endsession/callback
├── TokenRevocationEndpoint → POST /security/revocation
├── JwksEndpoint         → GET /.well-known/openid-configuration/jwks
└── UserInfoEndpoint     → GET/POST /security/userinfo
```

### Port Binding Sequence

```
Kestrel Startup
├── Read ASPNETCORE_URLS (default: https://localhost:5001)
├── Bind to configured URLs
├── Initialize HTTPS certificate (dev cert or configured cert)
├── Start listening
└── Log: "Now listening on: https://localhost:5001"
```

---

# PHASE 3: CLIENT STARTUP TRACE

## Command: `dotnet run` on Demo.Client.Mvc

### Step-by-Step Execution

| Step | File | Method | What Executes | Dependencies |
|------|------|--------|---------------|--------------|
| 1 | `Program.cs` | `Main()` | Entry point | OS runtime |
| 2 | `Program.cs` | `WebApplication.CreateBuilder()` | Host initialization | Environment |
| 3 | `Program.cs` | `builder.Services.Configure<OAuthClientOptions>()` | Bind OAuth config from appsettings.json | Configuration section |
| 4 | `Program.cs` | `AddAuthentication()` | Add Cookie authentication scheme | None |
| 5 | `Program.cs` | `AddOpenIdConnect()` | Configure OIDC middleware | OAuthClientOptions |
| 6 | `Program.cs` | `AddSingleton<TokenService>()` | Register TokenService for API calls | HttpClient |
| 7 | `Program.cs` | `Build()` | Build ServiceProvider | All registrations |
| 8 | `Program.cs` | `UseAuthentication()` | Enable auth middleware | Auth services |
| 9 | `Program.cs` | `UseAuthorization()` | Enable authorization | Auth middleware |
| 10 | `Program.cs` | `MapControllers()` | Register MVC routes | Controllers |
| 11 | `Program.cs` | `Run()` | Start listening | Port available |

### OpenIdConnect Configuration Details

```csharp
options.Authority = OAuthClientOptions.Authority;           // IdP URL
options.ClientId = OAuthClientOptions.ClientId;             // Client identifier
options.ClientSecret = OAuthClientOptions.ClientSecret;     // Client secret
options.CallbackPath = OAuthClientOptions.CallbackPath;     // /signin-oidc
options.ResponseType = OpenIdConnectResponseType.Code;      // Authorization code flow
options.UsePkce = true;                                     // PKCE enabled
options.Scope.Add("offline_access");                        // Refresh tokens
options.SaveTokens = true;                                  // Store tokens in cookie
```

### Authority Usage
- **Discovery Document:** `{Authority}/.well-known/openid-configuration`
- **Authorize Endpoint:** `{Authority}/security/authorize`
- **Token Endpoint:** `{Authority}/security/token`
- **JWKS Endpoint:** `{Authority}/.well-known/openid-configuration/jwks`

---

# PHASE 4: FULL LOGIN FLOW TRACE

## Authorization Code Flow with PKCE

### Step 1: User Clicks Login

| Aspect | Detail |
|--------|--------|
| **File** | `AccountController.cs` (client MVC) |
| **Method** | `Login()` action |
| **Action** | Returns `Challenge()` result |
| **What Happens** | ASP.NET Core OIDC middleware intercepts Challenge |

### Step 2: OIDC Middleware Builds Authorization URL

| Aspect | Detail |
|--------|--------|
| **File** | ASP.NET Core `OpenIdConnectHandler` |
| **Method** | `HandleChallengeAsync()` |
| **Generates** | PKCE `code_verifier` (random 128 char) |
| **Computes** | `code_challenge` = Base64(SHA256(code_verifier)) |
| **Generates** | `state` (CSRF protection) |
| **Generates** | `nonce` (replay protection) |

### Step 3: Redirect to HCL.CS.SF Authorize Endpoint

| Aspect | Detail |
|--------|--------|
| **URL** | `GET /security/authorize` |
| **Query Params** | `client_id`, `redirect_uri`, `response_type=code`, `scope`, `code_challenge`, `code_challenge_method=S256`, `state`, `nonce` |
| **File** | `HCL.CS.SFEndpointMiddleware.cs` |
| **Method** | `InvokeAsync()` → `Find()` → `AuthorizeEndpoint` |

### Step 4: Authorize Endpoint Processing

| Step | File | Method | Action | Security Check |
|------|------|--------|--------|----------------|
| 4.1 | `AuthorizeEndpoint.cs` | `ProcessAsync()` | Parse request (GET/POST) | Content-Type validation |
| 4.2 | `AuthorizationCodeBase.cs` | `ProcessAuthorizeRequestAsync()` | Validate request parameters | Parameter presence |
| 4.3 | `AuthorizeRequestSpecification.cs` | `IsSatisfiedBy()` | Validate client_id | Client exists in DB |
| 4.4 | `AuthorizeRequestSpecification.cs` | Validate redirect_uri | Check against ClientRedirectUris table | Redirect URI whitelist |
| 4.5 | `AuthorizeRequestSpecification.cs` | Validate PKCE parameters | code_challenge length 43-128 | PKCE required |
| 4.6 | `AuthorizeRequestSpecification.cs` | Validate response_type | Must be "code" or supported | Response type allowed |
| 4.7 | `ResourceScopeValidator.cs` | Validate scopes | Check against AllowedScopes | Scope validation |

### Step 5: User Authentication

| Step | File | Method | Action |
|------|------|--------|--------|
| 5.1 | `AuthorizationService.cs` | `AuthenticateAsync()` | Check if user already signed in |
| 5.2 | `SignInManagerWrapper.cs` | `PasswordSignInAsync()` | Validate credentials |
| 5.3 | `Argon2PasswordHasherWrapper.cs` | `VerifyHashedPassword()` | Verify password hash |
| 5.4 | `UserStoreWrapper.cs` | `UpdateAsync()` | Update security stamp if needed |

### Step 6: Authorization Code Generation

| Step | File | Method | Action |
|------|------|--------|--------|
| 6.1 | `AuthorizationService.cs` | `CreateAuthorizationCodeAsync()` | Generate random code |
| 6.2 | `SecurityTokenRepository.cs` | `InsertAsync()` | Store code in SecurityTokens table |
| 6.3 | `AuthorizationService.cs` | Build authorization response | Include code, state |

### Step 7: Redirect Back to Client

| Aspect | Detail |
|--------|--------|
| **URL** | `{redirect_uri}?code={authorization_code}&state={state}` |
| **Method** | HTTP 302 Redirect |

### Step 8: Client Token Exchange

| Step | File | Method | Action |
|------|------|--------|--------|
| 8.1 | ASP.NET Core OIDC Handler | `HandleRemoteAuthenticateAsync()` | Receive callback |
| 8.2 | OIDC Handler | Exchange code for tokens | POST to `/security/token` |
| 8.3 | `TokenEndpoint.cs` | `ProcessAsync()` | Handle token request |

### Step 9: Token Endpoint Processing

| Step | File | Method | Action | Security Check |
|------|------|--------|--------|----------------|
| 9.1 | `ClientSecretValidator.cs` | `ValidateClientSecretAsync()` | Authenticate client | client_secret valid |
| 9.2 | `TokenRequestValidator.cs` | `ValidateTokenRequestAsync()` | Validate request | grant_type supported |
| 9.3 | `TokenRequestValidator.cs` | Validate authorization code | Check SecurityTokens table | Code not expired |
| 9.4 | `TokenRequestValidator.cs` | Validate PKCE | code_verifier matches challenge | PKCE verification |
| 9.5 | `TokenGenerationService.cs` | `ProcessTokenAsync()` | Generate tokens | All validations passed |

### Step 10: JWT Token Generation

| Step | File | Method | Action |
|------|------|--------|--------|
| 10.1 | `TokenGenerationService.cs` | `GenerateAccessTokenAsync()` | Create JWT header + payload |
| 10.2 | `CertificateExtension.cs` | `GetAsymmetricSigningCredentials()` | Get signing key from keystore |
| 10.3 | `TokenGenerationService.cs` | `GenerateRefreshTokenAsync()` | Generate refresh token |
| 10.4 | `SecurityTokenRepository.cs` | `InsertAsync()` | Store refresh token (hashed) |
| 10.5 | `TokenGenerationService.cs` | `GenerateIdentityTokenAsync()` | Create ID token if openid scope |

### Step 11: Token Response

```json
{
  "access_token": "eyJhbGciOiJSUzI1Ni...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "refresh_token": "def50200...",
  "id_token": "eyJhbGciOiJSUzI1Ni...",
  "scope": "openid profile email"
}
```

### Step 12: Client Cookie Creation

| Aspect | Detail |
|--------|--------|
| **File** | ASP.NET Core `CookieAuthenticationHandler` |
| **Action** | Create authentication cookie with claims |
| **Tokens** | Stored in cookie properties (SaveTokens=true) |

---

# PHASE 5: REFRESH FLOW TRACE

## Token Refresh Sequence

| Step | File | Method | Action | DB Table Accessed |
|------|------|--------|--------|-------------------|
| 1 | Client Application | `TokenService.RefreshTokenAsync()` | Detect expired access token | N/A |
| 2 | Client | POST to `/security/token` | Send refresh_token grant | N/A |
| 3 | `TokenEndpoint.cs` | `ProcessAsync()` | Receive refresh request | N/A |
| 4 | `ClientSecretValidator.cs` | `ValidateClientSecretAsync()` | Authenticate client | Clients |
| 5 | `TokenRequestValidator.cs` | `ValidateTokenRequestAsync()` | Validate grant_type | N/A |
| 6 | `TokenGenerationService.cs` | `RenewRefreshTokenAsync()` | Process refresh | N/A |
| 7 | `TokenGenerationService.cs` | `ValidateRefreshTokenAsync()` | Validate refresh token | SecurityTokens |
| 8 | `TokenGenerationService.cs` | Check hash | SHA256(refresh_token) match | SecurityTokens.Key |
| 9 | `TokenGenerationService.cs` | Check expiration | CreationTime + ExpiresAt > Now | SecurityTokens |
| 10 | `TokenGenerationService.cs` | Check consumption | ConsumedAt == null | SecurityTokens |
| 11 | `TokenGenerationService.cs` | Check reuse detection | TokenReuseDetected == false | SecurityTokens |
| 12 | `TokenGenerationService.cs` | Mark old token consumed | Update ConsumedAt, ConsumedTime | SecurityTokens |
| 13 | `TokenGenerationService.cs` | Generate new access token | Create new JWT | N/A |
| 14 | `TokenGenerationService.cs` | Generate new refresh token | Create new refresh token | SecurityTokens |
| 15 | `SecurityTokenRepository.cs` | `InsertAsync()` | Store new refresh token | SecurityTokens |
| 16 | `TokenGenerationService.cs` | Return tokens | New access_token + refresh_token | N/A |

### Refresh Token Rotation

```
Refresh Token Lifecycle:
├── Initial token issued (CreationTime, ExpiresAt set)
├── Token used for refresh
│   ├── Old token: ConsumedAt = Now, ConsumedTime = Now
│   └── New token: Created, stored with new Key hash
├── If old token used again (reuse detection)
│   └── All tokens for user+client marked TokenReuseDetected = true
└── Security: Prevents replay attacks
```

---

# PHASE 6: LOGOUT FLOW TRACE

## Front-Channel Logout

| Step | File | Method | Action | Security |
|------|------|--------|--------|----------|
| 1 | Client Controller | `Logout()` action | User clicks logout | N/A |
| 2 | `EndSessionEndpoint.cs` | `ProcessAsync()` | Handle end session request | Validate id_token_hint |
| 3 | `EndSessionRequestValidator.cs` | `ValidateEndSessionRequestAsync()` | Validate logout request | post_logout_redirect_uri check |
| 4 | `SessionManagementService.cs` | `RemoveUserSessionAsync()` | Clear server session | SecurityTokens cleanup |
| 5 | `TokenGenerationService.cs` | `RemoveUserTokensAsync()` | Delete user tokens | Delete from SecurityTokens |
| 6 | Client OIDC Handler | `SignOutAsync()` | Clear local cookie | Cookie deleted |
| 7 | `EndSessionEndpoint.cs` | Redirect to post_logout_redirect_uri | Complete logout flow | State parameter preserved |

## Back-Channel Logout (if configured)

| Step | File | Method | Action |
|------|------|--------|--------|
| 1 | `BackChannelLogoutService.cs` | `SendLogoutNotificationAsync()` | Triggered during logout |
| 2 | `TokenGenerationService.cs` | `GenerateBackChannelLogoutTokenAsync()` | Create logout token JWT |
| 3 | `BackChannelLogoutService.cs` | POST to client backchannel logout URI | Send logout_token |
| 4 | Client | Validate logout token | Verify JWT signature |
| 5 | Client | Clear session | Terminate user session |

---

# PHASE 7: CONFIGURATION DEPENDENCY MAP

```
Program.cs
├── SystemSettings.json [CRITICAL]
│   ├── DBConfig [CRITICAL]
│   │   ├── Database (SqlServer/MySql/PostgreSQL/SQLite)
│   │   └── DBConnectionString → HCL.CS.SF_DB_CONNECTION_STRING
│   ├── UserConfig [CRITICAL]
│   │   ├── MinUserNameLength, MaxUserNameLength
│   │   ├── RequireUniqueEmail
│   │   ├── RequireConfirmedEmail
│   │   ├── DefaultLockoutTimeSpanMin
│   │   └── MaxFailedAccessAttempts
│   ├── PasswordConfig [CRITICAL]
│   │   ├── MinPasswordLength (default: 8)
│   │   ├── RequireDigit, RequireLowercase, RequireUppercase, RequireSpecialChar
│   │   └── MaxLimitPasswordReuse
│   ├── EmailConfig [OPTIONAL]
│   │   ├── SmtpServer, Port
│   │   ├── UserName → HCL.CS.SF_SMTP_USERNAME
│   │   └── Password → HCL.CS.SF_SMTP_PASSWORD
│   ├── SMSConfig [OPTIONAL]
│   │   ├── SMSAccountIdentification → HCL.CS.SF_SMS_ACCOUNT_ID
│   │   ├── SMSAccountPassword → HCL.CS.SF_SMS_ACCOUNT_PASSWORD
│   │   └── SMSAccountFrom → HCL.CS.SF_SMS_ACCOUNT_FROM
│   ├── LDAPConfig [OPTIONAL]
│   │   ├── LdapHostName, LDAPPort
│   │   └── LdapDomainName
│   └── LogConfig [OPTIONAL]
│       ├── WriteLogTo (File/DataBase)
│       └── LogFileConfig.FilePath
│
├── TokenSettings.json [CRITICAL]
│   ├── TokenConfig [CRITICAL]
│   │   ├── IssuerUri (e.g., https://localhost:5001)
│   │   ├── CachingLifetime
│   │   └── ShowKeySet
│   ├── TokenExpiration [CRITICAL]
│   │   ├── MinAccessTokenExpiration, MaxAccessTokenExpiration
│   │   ├── MinRefreshTokenExpiration, MaxRefreshTokenExpiration
│   │   └── MinAuthorizationCodeExpiration, MaxAuthorizationCodeExpiration
│   ├── UserInteractionConfig [CRITICAL]
│   │   ├── LoginUrl (/account/login)
│   │   ├── LogoutUrl (/account/logout)
│   │   └── ErrorUrl (/home/error)
│   ├── EndpointsConfig [CRITICAL]
│   │   ├── EnableAuthorizeEndpoint
│   │   ├── EnableTokenEndpoint
│   │   ├── EnableUserInfoEndpoint
│   │   └── EnableEndSessionEndpoint
│   └── InputLengthRestrictionsConfig [PRODUCTION-ONLY]
│       ├── ClientId, ClientSecret, Scope
│       └── RedirectUri, CodeChallenge, CodeVerifier
│
├── Certificates [CRITICAL for Production]
│   ├── RSA .pfx file → HCL.CS.SF_RSA_SIGNING_CERT_PATH
│   ├── ECDSA .pfx file → HCL.CS.SF_ECDSA_SIGNING_CERT_PATH
│   └── Password → HCL.CS.SF_SIGNING_CERT_PASSWORD
│
└── NotificationTemplateSettings.json [CRITICAL]
    ├── EmailTemplateCollection
    │   ├── DefaultTemplate
    │   ├── EmailVerificationUsingLink
    │   ├── EmailVerificationUsingToken
    │   └── ResetPasswordUsingToken
    └── SMSTemplateCollection
        ├── PhoneNumberVerificationToken
        └── ResetPasswordUsingToken
```

---

# PHASE 8: FAILURE MATRIX

| Failure | Error Message | Thrown At | Fix |
|---------|---------------|-----------|-----|
| Invalid client_id | `invalid_client` | `AuthorizeEndpoint.ProcessAsync()` → `AuthorizeRequestSpecification` | Check `Clients` table for valid ClientId |
| Redirect URI mismatch | `invalid_request` | `AuthorizeRequestSpecification.IsSatisfiedBy()` | Update `ClientRedirectUris` table |
| Missing certificate | `CryptographicException: Invalid certificate / No certificate found` | `LoadAsymmetricCertificate()` | Place .pfx file at configured path, set password secret |
| Invalid client secret | `invalid_client` | `ClientSecretValidator.ValidateClientSecretAsync()` | Sync secret in DB and client configuration |
| Expired authorization code | `invalid_grant` | `TokenRequestValidator.ValidateTokenRequestAsync()` | Reduce `AuthorizationCodeExpiration` in Client config |
| PKCE mismatch | `invalid_grant` | `ProofKeyParametersSpecification.IsSatisfiedBy()` | Ensure `code_challenge_method` is S256, verifier matches challenge |
| Expired refresh token | `invalid_grant` | `TokenGenerationService.ValidateRefreshTokenAsync()` | Check `SecurityTokens.ExpiresAt`, re-authenticate user |
| DB connection failure | `NpgsqlException` / `SqlException` / `MySqlException` | `ValidateDatabaseConfiguration()` | Verify connection string secret, check DB server |
| Wrong issuer | `invalid_token` | JWT validation | Match `TokenConfig.IssuerUri` with Authority URL |
| Token reuse detected | `invalid_grant` | `TokenGenerationService.ValidateRefreshTokenAsync()` | Token was already used, force full re-authentication |
| Invalid scope | `invalid_scope` | `ResourceScopeValidator.ValidateAsync()` | Add scope to `Clients.AllowedScopes` |
| Unsupported grant type | `unsupported_grant_type` | `TokenRequestValidator.ValidateTokenRequestAsync()` | Add grant type to `Clients.SupportedGrantTypes` |
| Invalid certificate password | `CryptographicException` | `X509Certificate2` constructor | Verify `HCL.CS.SF_SIGNING_CERT_PASSWORD` secret |
| Expired certificate | `Certificate Expired` | `KeyStore.CheckCertificateValidity()` | Renew signing certificate |
| Algorithm mismatch | `Certificate and Algorithm type mismatch` | `KeyStore.VerifyCertificate()` | Ensure certificate key type matches configured algorithm |
| LDAP connection failed | `LdapException` | `ValidateLdapConfiguration()` | Check LDAP host, port, domain configuration |
| SMTP connection failed | `SmtpException` | `ValidateEmailConfiguration()` | Verify SMTP server, credentials |

---

# PHASE 9: PRODUCTION HARDENING CHECKLIST

## Security Configuration

- [ ] **HTTPS Enforcement**
  - [ ] HSTS enabled (`UseHsts()`)
  - [ ] HTTPS redirect enabled (`UseHttpsRedirection()`)
  - [ ] Development certificate NOT used in production

- [ ] **Certificate Management**
  - [ ] RSA signing certificate deployed (min 2048-bit)
  - [ ] Certificate expiration monitoring (alert at 30 days)
  - [ ] Certificate rotation procedure documented
  - [ ] Private key stored in HSM or secure vault

- [ ] **Token Security**
  - [ ] Access token expiration ≤ 15 minutes (900 seconds)
  - [ ] Refresh token rotation enabled
  - [ ] Refresh token reuse detection enabled
  - [ ] PKCE required for all public clients

- [ ] **Database Security**
  - [ ] Connection string uses encrypted credentials
  - [ ] Database user has minimal required permissions
  - [ ] Row-level security enabled (if supported)
  - [ ] Audit logging enabled for sensitive operations

## Infrastructure

- [ ] **Rate Limiting**
  - [ ] `/security/token` endpoint rate limited
  - [ ] `/security/authorize` endpoint rate limited
  - [ ] Failed login attempt lockout configured

- [ ] **Health Monitoring**
  - [ ] Health check endpoint (`/health`) implemented
  - [ ] Database connectivity check in health endpoint
  - [ ] Certificate validity check in health endpoint

- [ ] **Logging & Observability**
  - [ ] Structured logging (Serilog) configured
  - [ ] Log sink to centralized system (Seq/ELK/Splunk)
  - [ ] PII redaction in logs
  - [ ] Audit table writes verified
  - [ ] Correlation IDs for request tracing

- [ ] **Secrets Management**
  - [ ] No secrets in appsettings.json
  - [ ] Azure Key Vault / AWS Secrets Manager integration
  - [ ] Secret rotation policy (90-day recommended)
  - [ ] dotnet user-secrets NOT used in production

## Container/Deployment

- [ ] **Container Security**
  - [ ] Read-only root filesystem
  - [ ] Non-root user execution
  - [ ] Resource limits (CPU/memory)
  - [ ] Image scanning for vulnerabilities

- [ ] **Network Security**
  - [ ] Network policies (ingress/egress) configured
  - [ ] Database NOT exposed to public internet
  - [ ] Internal service communication encrypted (mTLS)

---

# PHASE 10: STARTUP ORDER SUMMARY

## Correct Startup Sequence

```
┌─────────────────────────────────────────────────────────────────┐
│  STEP 1: DATABASE                                                │
│  ├── PostgreSQL/MySQL/SQL Server running                        │
│  ├── Migrations applied (or schema created)                     │
│  ├── Connection string validated                                │
│  └── Required seed data present                                 │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│  STEP 2: IDENTITY SERVER (HCL.CS.SF)                               │
│  ├── Certificates loaded from secure storage                    │
│  ├── Keystore initialized with signing keys                     │
│  ├── OAuth endpoints registered                                 │
│  ├── Discovery endpoint available                               │
│  └── Listening on configured ports                              │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│  STEP 3: CLIENT APPLICATION                                     │
│  ├── OIDC discovery successful (/.well-known/openid-configuration)
│  ├── Callback URL reachable by browser                          │
│  └── Cookie authentication configured                           │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│  STEP 4: RESOURCE SERVER (API)                                  │
│  ├── JWT validation configured                                  │
│  ├── Authority pointing to Identity Server                      │
│  └── JWKS endpoint accessible                                   │
└─────────────────────────────────────────────────────────────────┘
```

## Why Order Matters

| Dependency | Reason |
|------------|--------|
| Database before Identity Server | EF Core migrations run on startup; Identity tables must exist |
| Identity Server before Client | Discovery document must be available before OIDC middleware initializes |
| Identity Server before Resource Server | JWKS endpoint must be accessible for JWT signature validation |
| Client Callback URL reachable | Identity Server validates redirect_uri against configured URLs |

## Startup Validation Commands

```bash
# 1. Verify database connectivity
psql "${HCL.CS.SF_DB_CONNECTION_STRING}" -c "SELECT 1;"

# 2. Verify Identity Server discovery
curl -s https://localhost:5001/.well-known/openid-configuration | jq .

# 3. Verify JWKS endpoint
curl -s https://localhost:5001/.well-known/openid-configuration/jwks | jq .

# 4. Verify client callback URL is reachable
curl -I https://client.example.com/signin-oidc

# 5. Verify certificate validity
openssl pkcs12 -in certificates/HCL.CS.SF_rsa.pfx -nodes -passin pass:${HCL.CS.SF_SIGNING_CERT_PASSWORD} | openssl x509 -noout -dates
```

---

# APPENDIX A: BEFORE-START CHECKLIST

## Pre-Deployment Verification

- [ ] All required environment variables set
- [ ] Database server accessible from application server
- [ ] Certificate files present with correct permissions
- [ ] Certificate not expired (check `NotAfter` date)
- [ ] SMTP server accessible (if email notifications required)
- [ ] LDAP server accessible (if LDAP authentication enabled)
- [ ] Log directory writable (if file logging enabled)
- [ ] Required ports not in use by other processes
- [ ] Firewall rules allow inbound HTTPS traffic
- [ ] SSL/TLS certificate for HTTPS binding valid

## Configuration Validation

```bash
# Run configuration validation
dotnet run --project src/Identity/HCL.CS.SF.Identity.API -- validate-config

# Expected output: "Configuration validation passed" or detailed error list
```

---

# APPENDIX B: AFTER-START VERIFICATION CHECKLIST

## Smoke Tests

- [ ] **Discovery Endpoint**
  ```bash
  curl https://localhost:5001/.well-known/openid-configuration
  ```
  Expected: JSON with `authorization_endpoint`, `token_endpoint`, etc.

- [ ] **JWKS Endpoint**
  ```bash
  curl https://localhost:5001/.well-known/openid-configuration/jwks
  ```
  Expected: JSON with RSA/ECDSA public keys

- [ ] **Health Check**
  ```bash
  curl https://localhost:5001/health
  ```
  Expected: HTTP 200 with healthy status

- [ ] **Token Endpoint (Client Credentials)**
  ```bash
  curl -X POST https://localhost:5001/security/token \
    -H "Content-Type: application/x-www-form-urlencoded" \
    -d "grant_type=client_credentials" \
    -d "client_id=${CLIENT_ID}" \
    -d "client_secret=${CLIENT_SECRET}" \
    -d "scope=openid"
  ```
  Expected: JSON with `access_token`

- [ ] **Authorize Endpoint (Browser)**
  Open in browser:
  ```
  https://localhost:5001/security/authorize?client_id=${CLIENT_ID}&response_type=code&scope=openid&redirect_uri=${REDIRECT_URI}&state=test&code_challenge=${CHALLENGE}&code_challenge_method=S256
  ```
  Expected: Login page or redirect with authorization code

## Log Verification

- [ ] No `ERROR` or `FATAL` log entries
- [ ] Database connection logged as successful
- [ ] Certificate loading logged as successful
- [ ] Endpoints registered logged
- [ ] Server started listening logged

---

*End of Runtime Execution Manual*
