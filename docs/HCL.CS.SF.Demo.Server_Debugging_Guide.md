# HCL.CS.SF.Demo.Server - Debugging & Development Guide

## Project Overview

| Property | Value |
|----------|-------|
| **Project Type** | ASP.NET Core 8.0 Web Application |
| **Project Path** | `demos/HCL.CS.SF.Demo.Server/` |
| **Project File** | `HCL.CS.SF.DemoServerApp.csproj` |
| **Framework** | .NET 8.0 |
| **Runtime Identifiers** | win-x64, linux-x64 |
| **Purpose** | Demo Identity Server (OIDC/OAuth2 provider) |

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Entry Points](#entry-points)
3. [Application Flow](#application-flow)
4. [Project Structure](#project-structure)
5. [Configuration Files](#configuration-files)
6. [Environment Variables](#environment-variables)
7. [Debugging in Rider](#debugging-in-rider)
8. [Key Classes & Services](#key-classes--services)
9. [Common Debugging Scenarios](#common-debugging-scenarios)
10. [Endpoints Reference](#endpoints-reference)

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         HCL.CS.SF.Demo.Server (.NET 8)                         │
│                         OAuth2/OIDC Identity Server                         │
├─────────────────────────────────────────────────────────────────────────────┤
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌─────────────────┐ │
│  │   Program.cs │  │   Controllers│  │   HCL.CS.SF     │  │   Certificate   │ │
│  │   (Entry)    │──│   (MVC)      │──│   Services   │──│   Management    │ │
│  └──────────────┘  └──────────────┘  └──────────────┘  └─────────────────┘ │
│         │                 │                  │                              │
│         ▼                 ▼                  ▼                              │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                      HCL.CS.SF.Hosting (NuGet)                         │   │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐    │   │
│  │  │  Identity   │ │   Token     │ │    API      │ │   Proxy     │    │   │
│  │  │  Services   │ │  Services   │ │  Endpoints  │ │  Services   │    │   │
│  │  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘    │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Entry Points

### 1. Primary Entry Point: `Program.cs`

**File:** `demos/HCL.CS.SF.Demo.Server/Program.cs`

**Key Sections:**

```csharp
// Line 13 - Application Builder Setup
var builder = WebApplication.CreateBuilder(args);

// Line 15-16 - Configuration Loading
var (systemSettings, tokenSettings, notificationSettings) =
    LoadHCL.CS.SFConfiguration(builder.Environment.ContentRootPath);

// Line 26-28 - HCL.CS.SF Services Registration
builder.Services.AddHCL.CS.SF(systemSettings, tokenSettings, notificationSettings)
    .AddAsymmetricKeystore(LoadAsymmetricCertificate())
    .AddLoggerInstance(logConfig);

// Line 134 - Application Build
var app = builder.Build();

// Line 175 - Application Run
app.Run();
```

### 2. Main Entry Method Flow

```
Program.cs
    │
    ├─> LoadHCL.CS.SFConfiguration()          [Line 178-220]
    │   ├─> Load SystemSettings.json
    │   ├─> Load TokenSettings.json
    │   └─> Load NotificationTemplateSettings.json
    │
    ├─> LoadAsymmetricCertificate()        [Line 237-271]
    │   ├─> Load RSA Certificate
    │   └─> Load ECDSA Certificate
    │
    ├─> AddHCL.CS.SF()                        [HCL.CS.SFExtension.cs]
    │   └─> Configure all HCL.CS.SF services
    │
    ├─> Build Application                  [Line 134]
    │
    └─> Run Application                    [Line 175]
```

---

## Application Flow

### Startup Flow

```
1. Program.cs Execution
   │
   ├── 1.1 Create WebApplicationBuilder
   │
   ├── 1.2 Load Configurations
   │   ├── SystemSettings.json        → SystemSettings object
   │   ├── TokenSettings.json         → TokenSettings object
   │   └── NotificationTemplateSettings.json → NotificationTemplateSettings
   │
   ├── 1.3 Resolve Environment Variables
   │   ├── HCL.CS.SF_DB_CONNECTION_STRING
   │   ├── HCL.CS.SF_SIGNING_CERT_PASSWORD
   │   ├── HCL.CS.SF_RSA_SIGNING_CERT_BASE64 / PATH
   │   └── HCL.CS.SF_ECDSA_SIGNING_CERT_BASE64 / PATH
   │
   ├── 1.4 Load Signing Certificates
   │   ├── RSA Certificate (RS256)
   │   └── ECDSA Certificate (ES256)
   │
   ├── 1.5 Configure Services
   │   ├── AddHCL.CS.SF()               → Core identity services
   │   ├── AddAsymmetricKeystore()   → Signing keys
   │   ├── AddLoggerInstance()       → Logging
   │   ├── ConfigureApplicationCookie()
   │   ├── AddSession()
   │   ├── AddCors()
   │   ├── AddControllersWithViews()
   │   └── AddRateLimiter()
   │
   ├── 1.6 Configure Middleware Pipeline
   │   ├── UseDeveloperExceptionPage()  [Dev only]
   │   ├── UseHttpsRedirection()
   │   ├── UseStaticFiles()
   │   ├── UseHCL.CS.SFCorrelationId()
   │   ├── UseHCL.CS.SFRequestObservability()
   │   ├── UseSession()
   │   ├── UseRouting()
   │   ├── UseCors()
   │   ├── UseRateLimiter()
   │   ├── Security Headers Middleware
   │   ├── UseAuthentication()
   │   ├── UseAuthorization()
   │   ├── UseHCL.CS.SFEndpoint()         → OAuth/OIDC endpoints
   │   ├── UseHCL.CS.SFApi()              → Management API
   │   └── MapDefaultControllerRoute() → MVC routes
   │
   └── 1.7 Run Application
```

### Request Flow - OAuth2/OIDC

```
HTTP Request
    │
    ├─> CorrelationIdMiddleware           [X-Correlation-ID]
    ├─> RequestObservabilityMiddleware    [Logging]
    ├─> RateLimiter                       [Throttling]
    │
    ├─> Security Headers
    │   ├── X-Frame-Options: DENY
    │   ├── X-Content-Type-Options: nosniff
    │   ├── Referrer-Policy: no-referrer
    │   └── Content-Security-Policy
    │
    ├─> Authentication Middleware
    ├─> Authorization Middleware
    │
    ├─> Routing
    │   │
    │   ├── /security/authorize          → AuthorizationEndpoint
    │   ├── /security/token              → TokenEndpoint
    │   ├── /security/introspect         → IntrospectionEndpoint
    │   ├── /security/revocation         → TokenRevocationEndpoint
    │   ├── /security/endsession         → EndSessionEndpoint
    │   ├── /.well-known/openid-configuration → DiscoveryEndpoint
    │   ├── /.well-known/jwks            → JwksEndpoint
    │   ├── /account/*                   → AccountController
    │   └── /api/*                       → HCL.CS.SFApiMiddleware
    │
    └─> Response
```

### Authentication Flow

```
User Login Request
    │
    ├─> AccountController.Login()        [Line 35-91]
    │   │
    │   ├─> authenticationServices.PasswordSignInAsync()
    │   │       │
    │   │       ├─> Validate credentials
    │   │       ├─> Check 2FA requirements
    │   │       └─> Return SignInResult
    │   │
    │   ├─> If Succeeded
    │   │   └─> interactionService.ConstructUserVerificationCode()
    │   │       └─> Redirect with auth code
    │   │
    │   ├─> If RequiresTwoFactor
    │   │   └─> Redirect to 2FA page
    │   │       ├── TwoFactorEmailSignIn
    │   │       ├── TwoFactorSmsSignIn
    │   │       └── AuthenticatorSignIn
    │   │
    │   └─> If Failed
    │       └─> Show error on login page
```

---

## Project Structure

```
demos/HCL.CS.SF.Demo.Server/
│
├── Program.cs                          # Application entry point
├── HCL.CS.SF.DemoServerApp.csproj         # Project file
│
├── Controllers/
│   ├── AccountController.cs            # Login/Logout/2FA/ForgotPassword
│   └── HomeController.cs               # Home/Privacy/Error pages
│
├── Models/
│   ├── ApplicationViewModel.cs
│   ├── ErrorViewModel.cs
│   ├── LoginViewModel.cs
│   ├── LogoutViewModel.cs
│   └── TwoFactorViewModel.cs
│
├── Constants/
│   └── ApplicationConstants.cs
│
├── Configurations/                     # JSON config files
│   ├── SystemSettings.json             # Database, Email, SMS, LDAP
│   ├── TokenSettings.json              # OAuth/OIDC settings
│   └── NotificationTemplateSettings.json # Email templates
│
├── Certificates/                       # Signing certificates
│   ├── ECDSACertificate.pfx
│   └── RSACertificate.pfx
│
└── Views/                              # Razor views
    ├── Account/
    │   ├── Login.cshtml
    │   ├── Logout.cshtml
    │   ├── LoggedOut.cshtml
    │   ├── AuthenticationMethod.cshtml
    │   ├── TwoFactorEmailSignIn.cshtml
    │   ├── TwoFactorSmsSignIn.cshtml
    │   ├── AuthenticatorSignIn.cshtml
    │   ├── RecoveryCodeSignIn.cshtml
    │   ├── ForgetPassword.cshtml
    │   └── ResetPassword.cshtml
    ├── Home/
    │   ├── Index.cshtml
    │   ├── Privacy.cshtml
    │   └── Error.cshtml
    └── Shared/
        └── _Layout.cshtml
```

---

## Configuration Files

### 1. SystemSettings.json

**Purpose:** Core system configuration (Database, Email, SMS, LDAP)

**Key Sections:**
```json
{
  "SystemSettings": {
    "DBConfig": {
      "Database": 1,                    // 1=PostgreSQL, 2=SQLServer, 3=MySQL
      "DBConnectionString": "..."
    },
    "EmailConfig": {
      "SmtpServer": "...",
      "Port": 587,
      "UserName": "...",
      "Password": "...",
      "SecureSocketOptions": true
    },
    "SMSConfig": { ... },
    "LdapConfig": { ... },
    "LogConfig": { ... }
  }
}
```

### 2. TokenSettings.json

**Purpose:** OAuth2/OIDC endpoint configuration

**Key Sections:**
```json
{
  "TokenSettings": {
    "TokenConfig": {
      "IssuerUri": "https://localhost:5001",
      "ApiIdentifier": "HCL.CS.SF.api",
      "AccessTokenLifeTime": 900,        // 15 minutes
      "RefreshTokenLifeTime": 2592000,   // 30 days
      "IdentityTokenLifeTime": 300,      // 5 minutes
      "AuthorizationCodeLifeTime": 300
    },
    "UserInteractionConfig": {
      "LoginUrl": "/account/login",
      "LogoutUrl": "/account/logout",
      "ErrorUrl": "/home/error"
    }
  }
}
```

### 3. NotificationTemplateSettings.json

**Purpose:** Email/SMS notification templates

---

## Environment Variables

### Required Variables

| Variable | Purpose | Example |
|----------|---------|---------|
| `HCL.CS.SF_DB_CONNECTION_STRING` | Database connection | `Host=localhost;...` |
| `HCL.CS.SF_SIGNING_CERT_PASSWORD` | Certificate password | `YourSecurePassword` |

### Certificate Variables (choose one per cert)

| Variable | Purpose |
|----------|---------|
| `HCL.CS.SF_RSA_SIGNING_CERT_BASE64` | RSA cert as base64 |
| `HCL.CS.SF_RSA_SIGNING_CERT_PATH` | Path to RSA cert file |
| `HCL.CS.SF_RSA_SIGNING_KID` | RSA key ID |
| `HCL.CS.SF_ECDSA_SIGNING_CERT_BASE64` | ECDSA cert as base64 |
| `HCL.CS.SF_ECDSA_SIGNING_CERT_PATH` | Path to ECDSA cert file |
| `HCL.CS.SF_ECDSA_SIGNING_KID` | ECDSA key ID |

### Secret Placeholders in Config

Use `${VAR_NAME}` syntax in JSON configs for environment variable substitution:

```json
{
  "DBConnectionString": "${HCL.CS.SF_DB_CONNECTION_STRING}",
  "SmtpServer": "${SMTP_SERVER}"
}
```

---

## Debugging in Rider

### 1. Run Configuration Setup

```
Run → Edit Configurations → + → .NET

Name: HCL.CS.SF.Demo.Server
Project: HCL.CS.SF.DemoServerApp
Working Directory: /path/to/demos/HCL.CS.SF.Demo.Server
Environment Variables:
    HCL.CS.SF_DB_CONNECTION_STRING=Host=localhost;...
    HCL.CS.SF_SIGNING_CERT_PASSWORD=yourpassword
```

### 2. Rider Debug Keyboard Shortcuts

#### Windows/Linux

| Shortcut | Action | Description |
|----------|--------|-------------|
| `F5` | Start Debugging | Run with debugger attached |
| `Shift+F5` | Stop Debugging | Stop debug session |
| `Ctrl+F5` | Run Without Debug | Start without debugger |
| `F9` | Toggle Breakpoint | Set/remove breakpoint |
| `Ctrl+F8` | Toggle Breakpoint | Alternative |
| `F10` | Step Over | Execute line, don't enter methods |
| `F11` | Step Into | Enter method call |
| `Shift+F11` | Step Out | Exit current method |
| `Shift+F9` | Run to Cursor | Execute until cursor position |
| `Alt+F9` | Evaluate Expression | Evaluate expression at breakpoint |
| `Ctrl+Alt+F9` | Evaluate Expression (Advanced) | Complex expression evaluation |
| `F8` | Resume Program | Continue execution |

#### macOS

| Shortcut | Action | Description |
|----------|--------|-------------|
| `⌃D` (Control+D) | Start Debugging | Run with debugger attached |
| `⌘F2` (Cmd+F2) | Stop Debugging | Stop debug session |
| `⌃⌥R` (Ctrl+Opt+R) | Run Without Debug | Start without debugger |
| `⌘F8` (Cmd+F8) | Toggle Breakpoint | Set/remove breakpoint |
| `F8` | Step Over | Execute line, don't enter methods |
| `F7` | Step Into | Enter method call |
| `⇧F8` (Shift+F8) | Step Out | Exit current method |
| `⌥F9` (Opt+F9) | Run to Cursor | Execute until cursor position |
| `⌥F8` (Opt+F8) | Evaluate Expression | Evaluate expression at breakpoint |
| `⌥⌘F8` (Opt+Cmd+F8) | Quick Evaluate | Quick expression evaluation |
| `⌘⌥R` (Cmd+Opt+R) | Resume Program | Continue execution |
| `⇧⌘F8` (Shift+Cmd+F8) | View Breakpoints | Manage all breakpoints |

### 3. Breakpoint Types

```csharp
// Line Breakpoint - Click gutter
if (signResult.Succeeded)  // <-- Set breakpoint here

// Conditional Breakpoint - Right-click → More
if (signResult.RequiresTwoFactor)  // Condition: signResult.RequiresTwoFactor == true

// Logpoint (Tracepoint) - Right-click → More
// Action: Log "User: {model.UserName}, Result: {signResult.Succeeded}"

// Exception Breakpoint - Run → View Breakpoints → + → .NET Exception Breakpoint
// Type: HCL.CS.SF.Domain.HCL.CS.SFException
```

### 4. Watch Window Variables

```csharp
// Recommended watches for authentication debugging:
signResult                    // SignInResult object
signResult.Succeeded          // bool
signResult.RequiresTwoFactor  // bool
signResult.TwoFactorVerificationMode  // enum
signResult.UserVerificationCode       // string
interactionService            // IInteractionService
tokenSettings                 // TokenSettings
```

### 5. Debug Launch Settings

**Create `Properties/launchSettings.json`:**

```json
{
  "profiles": {
    "HCL.CS.SF.Demo.Server": {
      "commandName": "Project",
      "launchBrowser": true,
      "launchUrl": "https://localhost:5001",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "HCL.CS.SF_DB_CONNECTION_STRING": "Host=localhost;Database=HCL.CS.SF;Username=postgres;Password=postgres",
        "HCL.CS.SF_SIGNING_CERT_PASSWORD": "YourPassword"
      },
      "applicationUrl": "https://localhost:5001;http://localhost:5000"
    }
  }
}
```

---

## Key Classes & Services

### Controllers

| Class | File | Purpose |
|-------|------|---------|
| `AccountController` | `Controllers/AccountController.cs` | Authentication, 2FA, Password reset |
| `HomeController` | `Controllers/HomeController.cs` | Static pages, error handling |

### AccountController Methods

```csharp
// Login Flow
[HttpGet]  Login(string returnUrl)                    // Line 25
[HttpPost] Login(LoginViewModel, button, returnUrl)   // Line 35

// 2FA Flow
[HttpGet]  AuthenticationMethod(model, returnUrl)     // Line 95
[HttpPost] AuthenticationMethod(model, action, returnUrl) // Line 107
[HttpGet]  TwoFactorEmailSignIn(returnUrl)            // Line 142
[HttpPost] TwoFactorEmailSignIn(twoFactorModel)       // Line 152
[HttpGet]  TwoFactorSmsSignIn(returnUrl)              // Line 173
[HttpPost] TwoFactorSmsSignIn(twoFactorModel)         // Line 183
[HttpGet]  AuthenticatorSignIn(returnUrl)             // Line 204
[HttpPost] AuthenticatorSignIn(twoFactorModel)        // Line 214
[HttpGet]  RecoveryCodeSignIn(returnUrl)              // Line 236
[HttpPost] RecoveryCodeSignIn(twoFactorModel)         // Line 246

// Logout Flow
[HttpGet]  Logout(logoutId)                           // Line 266
[HttpPost] Logout(model)                              // Line 289

// Password Reset Flow
[HttpGet]  ForgetPassword(returnUrl)                  // Line 315
[HttpPost] ForgetPassword(model, button, returnUrl)   // Line 324
[HttpGet]  ResetPassword(returnUrl)                   // Line 348
[HttpPost] ResetPassword(model, returnUrl)            // Line 357
```

### Injected Services

```csharp
public class AccountController(
    IAuthenticationService authenticationServices,      // Login/logout
    IUserAccountService userAccountService,             // User management
    IInteractionService interactionService,             // OAuth flow handling
    ILoggerInstance instance)                           // Logging
```

### HCL.CS.SF Extension Services

**File:** `src/Identity/HCL.CS.SF.Identity.API/Extensions/HCL.CS.SFExtension.cs`

```csharp
// Service Registration Order (Line 81-94)
services.AddConfiguration(HCL.CS.SFConfig)
    .AddAutoMapper()
    .AddInfrastructureResources()
    .AddUtilityServices()
    .AddDistributedMemoryCache()
    .AddIdentityConfiguration()
    .AddRepository()
    .AddInfrastructureServices()
    .AddCoreServices()
    .AddWrappers()
    .AddDefaultEndpoints()
    .AddProxyServices()
    .AddProxyValidator()
    .AddProxyRoutes();
```

### HCL.CS.SFBuilder Middleware

**File:** `src/Identity/HCL.CS.SF.Identity.API/Extensions/HCL.CS.SFBuilder.cs`

```csharp
// Extension Methods
UseHCL.CS.SFSecurityHeaders()      // Security headers
UseHCL.CS.SFCorrelationId()        // X-Correlation-ID
UseHCL.CS.SFRequestObservability() // Request logging
UseHCL.CS.SFEndpoint()             // OAuth/OIDC endpoints
UseHCL.CS.SFApi()                  // Management API
MapHCL.CS.SFHealthChecks()         // Health check endpoints
```

---

## Common Debugging Scenarios

### Scenario 1: Login Failure Debugging

```csharp
// Breakpoint at AccountController.cs Line 48
var signResult = await authenticationServices.PasswordSignInAsync(model.UserName, model.Password);

// Check these values:
signResult.Succeeded              // Expected: true
signResult.RequiresTwoFactor      // Expected: false (or true for 2FA users)
signResult.IsLockedOut            // Expected: false
signResult.IsNotAllowed           // Expected: false
signResult.ErrorCode              // Check if failed
signResult.Message                // Error description
```

### Scenario 2: Token Generation Issues

```csharp
// Check certificate loading - Program.cs Line 237
LoadAsymmetricCertificate()

// Verify:
- HCL.CS.SF_RSA_SIGNING_CERT_BASE64 or PATH is set
- HCL.CS.SF_ECDSA_SIGNING_CERT_BASE64 or PATH is set  
- HCL.CS.SF_SIGNING_CERT_PASSWORD is correct
- Certificates have private keys
- Certificates are not expired
```

### Scenario 3: Database Connection Issues

```csharp
// Check at Program.cs Line 198
var dbConnectionOverride = Environment.GetEnvironmentVariable("HCL.CS.SF_DB_CONNECTION_STRING");

// Verify:
- Environment variable is set
- Connection string format is valid
- Database server is accessible
- Credentials are correct
```

### Scenario 4: CORS Issues

```csharp
// Check at Program.cs Line 50-68
options.AddPolicy("HCL.CS.SFStrictCors", policy =>
{
    // Verify allowed origins include your client app
    policy.WithOrigins("https://localhost:3000", "https://localhost:5002")
          .WithMethods("GET", "POST")
          .WithHeaders("Authorization", "Content-Type", "X-Correlation-ID");
});
```

### Scenario 5: Rate Limiting

```csharp
// Check at Program.cs Line 72-114
// Critical endpoints: /security/token, /introspect, /revocation, /account/login
// Limit: 20 requests per minute per IP+client

// Global: 120 requests per minute per IP+client
```

---

## Endpoints Reference

### OAuth2/OIDC Endpoints

| Endpoint | Method | Description | Middleware |
|----------|--------|-------------|------------|
| `/.well-known/openid-configuration` | GET | Discovery document | HCL.CS.SFEndpoint |
| `/.well-known/jwks` | GET | JSON Web Key Set | HCL.CS.SFEndpoint |
| `/security/authorize` | GET | Authorization endpoint | HCL.CS.SFEndpoint |
| `/security/token` | POST | Token endpoint | HCL.CS.SFEndpoint |
| `/security/introspect` | POST | Token introspection | HCL.CS.SFEndpoint |
| `/security/revocation` | POST | Token revocation | HCL.CS.SFEndpoint |
| `/security/endsession` | GET/POST | End session (logout) | HCL.CS.SFEndpoint |
| `/security/userinfo` | GET | User info endpoint | HCL.CS.SFEndpoint |

### MVC Endpoints

| Endpoint | Controller | Action | Purpose |
|----------|------------|--------|---------|
| `/` | Home | Index | Home page |
| `/account/login` | Account | Login | User login |
| `/account/logout` | Account | Logout | User logout |
| `/account/authenticationmethod` | Account | AuthenticationMethod | 2FA method selection |
| `/account/twofactoremailsignin` | Account | TwoFactorEmailSignIn | Email 2FA |
| `/account/twofactorsmssignin` | Account | TwoFactorSmsSignIn | SMS 2FA |
| `/account/authenticatorsignin` | Account | AuthenticatorSignIn | TOTP 2FA |
| `/account/recoverycodesignin` | Account | RecoveryCodeSignIn | Recovery codes |
| `/account/forgetpassword` | Account | ForgetPassword | Password reset request |
| `/account/resetpassword` | Account | ResetPassword | Password reset |

### Health Check Endpoints

| Endpoint | Purpose |
|----------|---------|
| `/health/live` | Liveness probe |
| `/health/ready` | Readiness probe (db + cache) |

### API Endpoints

| Endpoint | Purpose |
|----------|---------|
| `/api/*` | Management API (via HCL.CS.SFApiMiddleware) |

---

## Logging & Diagnostics

### Log Location

```
// File logging (configured in Program.cs Line 22-25)
{AppDomain.CurrentDomain.BaseDirectory}/Authentication.txt

// Example: 
// /bin/Debug/net8.0/Authentication.txt
```

### Log Format

```json
{
  "Timestamp": "2024-01-15T10:30:00.000Z",
  "Level": "Debug",
  "Message": "Login request for user: john.doe",
  "InstanceName": "Authentication",
  "CorrelationId": "..."
}
```

### Enabling Debug Logging

```csharp
// Program.cs - IdentityModelEventSource
IdentityModelEventSource.ShowPII = true;  // Enable for debugging (DON'T in production)

// Logging configuration
builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Logging.AddJsonConsole();
```

---

## Troubleshooting Guide

### Issue: Application Won't Start

**Check:**
1. Environment variables are set
2. Configuration files exist in `Configurations/`
3. Certificate files exist or env vars are set
4. Database is accessible

### Issue: Login Page Shows Error

**Check:**
1. User exists in database
2. Password is correct
3. Account is not locked
4. Account is confirmed (if required)

### Issue: Tokens Not Generated

**Check:**
1. Certificates are loaded: Check `LoadAsymmetricCertificate()`
2. Certificates have private keys
3. Certificate password is correct
4. Certificates are not expired

### Issue: Client Can't Connect

**Check:**
1. CORS policy allows client origin
2. Client is registered in database
3. Redirect URIs match exactly
4. HTTPS is configured properly

---

## Quick Start Commands

```bash
# Navigate to project
cd demos/HCL.CS.SF.Demo.Server

# Set environment variables (Linux/Mac)
export HCL.CS.SF_DB_CONNECTION_STRING="Host=localhost;Database=HCL.CS.SF;Username=postgres;Password=postgres"
export HCL.CS.SF_SIGNING_CERT_PASSWORD="YourPassword"

# Set environment variables (Windows PowerShell)
$env:HCL.CS.SF_DB_CONNECTION_STRING="Host=localhost;Database=HCL.CS.SF;Username=postgres;Password=postgres"
$env:HCL.CS.SF_SIGNING_CERT_PASSWORD="YourPassword"

# Run with dotnet CLI
dotnet run

# Run with specific URL
dotnet run --urls "https://localhost:5001"

# Build
dotnet build

# Watch mode (auto-restart on changes)
dotnet watch run
```
