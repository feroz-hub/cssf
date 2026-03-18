# HCL.CS.SF - Quick Reference Guide

## 🎯 What is HCL.CS.SF?

**HCL.CS.SF** is an **OAuth 2.0/OpenID Connect authentication framework** for .NET 8.0 that provides enterprise-grade identity and access management.

---

## 🏗️ Architecture at a Glance

```
┌─────────────────────────────────────────────────────────────┐
│                    HCL.CS.SF FRAMEWORK                   │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌────────────┐   ┌────────────┐   ┌──────────────────┐   │
│  │   OAuth    │   │   OpenID   │   │  ASP.NET Core    │   │
│  │   2.0      │   │  Connect   │   │    Identity      │   │
│  └────────────┘   └────────────┘   └──────────────────┘   │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              Core Features                            │  │
│  │  • User Management    • Token Management             │  │
│  │  • Role Management    • MFA Support                  │  │
│  │  • Client Management  • Audit Logging                │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │         Supported Databases                           │  │
│  │  SQL Server  |  MySQL  |  PostgreSQL  |  SQLite      │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## 📦 Project Structure (Simplified)

```
HCL.CS.SF/
│
├── Source/                    # Core Framework
│   ├── Domain/               # Business entities & models
│   ├── DomainServices/       # Interfaces (contracts)
│   ├── Infrastructure.Data/  # Database & repositories
│   ├── Service/              # Business logic
│   └── Hosting/              # Web hosting
│
├── Demo/                     # Example applications
│   ├── DemoServerApp/       # Auth server example
│   └── DemoClientCoreMVC/   # Client app example
│
└── Installer/               # Setup tools
    ├── DBMigration/        # Database setup
    └── SeedDataCreator/    # Initial data
```

---

## 🔑 Key Entities

### User Management
- **Users**: User accounts with profile data
- **Roles**: User roles for RBAC
- **UserRoles**: User-to-role assignments
- **UserClaims**: Custom user claims
- **PasswordHistory**: Prevent password reuse

### OAuth/OIDC
- **Clients**: OAuth 2.0 client applications
- **SecurityTokens**: Issued tokens
- **ApiResources**: Protected APIs
- **ApiScopes**: API authorization scopes
- **IdentityResources**: User identity information

---

## 🔒 Security Features Quick List

### Authentication
✅ Username/Password  
✅ LDAP/Active Directory  
✅ Multi-Factor Authentication (Email, SMS, Authenticator)  
✅ Email/Phone Confirmation  
✅ Account Lockout  

### Authorization
✅ Role-Based Access Control (RBAC)  
✅ Claims-Based Authorization  
✅ API Scopes  
✅ Client Permissions  

### OAuth 2.0 Flows
✅ Authorization Code  
✅ Authorization Code + PKCE  
✅ Client Credentials  
✅ Refresh Token  

### Token Types
✅ Access Tokens (JWT)  
✅ Refresh Tokens  
✅ Identity Tokens (ID Token)  
✅ Authorization Codes  

### Password Security
✅ Complexity Requirements  
✅ Password History (no reuse)  
✅ Expiration Policy  
✅ Secure Reset Flow  

---

## ⚙️ Configuration Overview

```csharp
HCL.CS.SFConfig
├── SystemSettings
│   ├── DbConfig              // Database connection
│   ├── LoginConfig           // Login behavior
│   ├── UserConfig            // User requirements
│   ├── PasswordConfig        // Password policies
│   ├── EmailConfig           // Email settings
│   ├── SmsConfig             // SMS settings
│   ├── LdapConfig            // LDAP integration
│   ├── CryptoConfig          // Cryptography
│   └── LogConfig             // Logging
│
├── TokenSettings
│   ├── TokenConfig           // Token generation
│   ├── AuthenticationConfig  // Auth flow settings
│   ├── EndpointsConfig       // OAuth endpoints
│   └── TokenExpiration       // Token lifetimes
│
└── NotificationTemplateSettings
    ├── EmailTemplates        // Email templates
    └── SmsTemplates          // SMS templates
```

---

## 🎨 Common Use Cases

### 1. **Single Sign-On (SSO)**
Central authentication for multiple applications

### 2. **API Security**
Protect REST APIs with JWT tokens

### 3. **Mobile Apps**
Secure mobile authentication with PKCE

### 4. **Microservices**
Centralized auth for microservices architecture

### 5. **Third-Party Integration**
Allow external apps to authenticate users

---

## 📊 Key Enumerations

| Enum | Values |
|------|--------|
| **DbTypes** | SqlServer, MySql, PostgresSql, Sqlite |
| **TwoFactorType** | None, Email, SMS, Authenticator |
| **IdentityProvider** | Local, LDAP |
| **ApplicationType** | Web, NativeApp, SPA |
| **AccessTokenType** | JWT, Reference |

---

## 🔧 Default Configurations

### User Settings
- **Min Username Length**: 6 characters
- **Min Password Length**: 8 characters
- **Max Failed Attempts**: 5
- **Lockout Duration**: 10 minutes
- **Password Expiration**: 42 days

### Token Expiration
- **Access Token**: 3600 seconds (1 hour)
- **Refresh Token**: 18000 seconds (5 hours)
- **Identity Token**: 3600 seconds (1 hour)
- **Authorization Code**: 600 seconds (10 minutes)

### Password Requirements
✅ Minimum 8 characters  
✅ At least 1 uppercase letter  
✅ At least 1 lowercase letter  
✅ At least 1 digit  
✅ At least 1 special character  
✅ Cannot reuse last 10 passwords  

---

## 📁 Important File Locations

```
Configuration Models:
└── Source/Domain/HCL.CS.SFConfig.cs

User Entity:
└── Source/Domain/Entities/Api/Users.cs

Client Entity:
└── Source/Domain/Entities/Endpoint/Clients.cs

Database Context:
└── Source/Infrastructure.Data/ApplicationDbContext.cs

System Configuration:
└── Source/Domain/Configurations/Api/SystemConfig.cs

Enumerations:
├── Source/Domain/Enums.cs
├── Source/Domain/Enums/ApiEnums.cs
└── Source/Domain/Enums/EndpointEnums.cs
```

---

## 🚀 Technology Stack

| Component | Technology |
|-----------|------------|
| **Framework** | .NET 8.0 |
| **Language** | C# 12 |
| **Identity** | ASP.NET Core Identity 8.0 |
| **ORM** | Entity Framework Core 8.0 |
| **Tokens** | JWT (System.IdentityModel.Tokens.Jwt) |
| **Databases** | SQL Server, MySQL, PostgreSQL, SQLite |

---

## 📋 Typical Workflow

### New User Registration
1. User submits registration form
2. System validates user data
3. Password complexity check
4. Email/phone confirmation sent
5. User confirms via link/OTP
6. Account activated

### User Authentication
1. User submits credentials
2. System validates username/password
3. Check if 2FA is required
4. If yes, send OTP via email/SMS
5. User enters OTP
6. Issue access token & refresh token
7. User authenticated

### OAuth Client Authorization
1. Client redirects user to auth server
2. User logs in (if not already)
3. User consents to requested scopes
4. Auth server issues authorization code
5. Client exchanges code for tokens
6. Client uses access token for API calls

---

## 🔍 Key Concepts

### RBAC (Role-Based Access Control)
Users are assigned to roles, and roles have permissions (claims).

```
User → UserRoles → Roles → RoleClaims
```

### OAuth 2.0 Flow
```
Client App → Authorization Server → Resource Owner (User)
         ↓
    Authorization Code
         ↓
    Access Token + Refresh Token
         ↓
    Protected Resource (API)
```

### PKCE (Proof Key for Code Exchange)
Enhanced security for public clients (mobile apps, SPAs):
1. Client generates `code_verifier` (random string)
2. Client hashes it to create `code_challenge`
3. Sends `code_challenge` with auth request
4. Later sends `code_verifier` with token request
5. Server verifies they match

---

## 📚 Database Tables Overview

### Identity Tables (ASP.NET Core Identity)
- AspNetUsers
- AspNetRoles
- AspNetUserRoles
- AspNetUserClaims
- AspNetRoleClaims
- AspNetUserLogins
- AspNetUserTokens

### Custom Tables
- Clients (OAuth clients)
- SecurityTokens
- ApiResources
- ApiScopes
- IdentityResources
- AuditTrail
- PasswordHistory
- Notification
- SecurityQuestions
- UserSecurityQuestions

---

## 🛠️ Repositories & Unit of Work

### Repository Pattern
Each entity has a corresponding repository interface:
- `IUserRepository`
- `IRoleRepository`
- `IClientRepository`
- `IApiResourceRepository`
- etc.

### Unit of Work Pattern
Groups related repositories:
- `IUserManagementUnitOfWork`
- `IRoleManagementUnitOfWork`
- `IResourceUnitOfWork`
- `IClientUnitOfWork`

---

## 💡 Tips

### Security Best Practices
1. ✅ Always use HTTPS in production
2. ✅ Enable PKCE for public clients
3. ✅ Set appropriate token expiration times
4. ✅ Implement refresh token rotation
5. ✅ Enable account lockout
6. ✅ Enforce strong password policies
7. ✅ Use MFA for sensitive operations
8. ✅ Monitor audit logs regularly

### Performance Tips
1. Use appropriate database indexes
2. Implement token caching
3. Configure connection pooling
4. Use async/await properly
5. Implement pagination for large datasets

---

## 🔗 Related Standards

- **OAuth 2.0**: [RFC 6749](https://tools.ietf.org/html/rfc6749)
- **OpenID Connect**: [Core 1.0](https://openid.net/specs/openid-connect-core-1_0.html)
- **PKCE**: [RFC 7636](https://tools.ietf.org/html/rfc7636)
- **JWT**: [RFC 7519](https://tools.ietf.org/html/rfc7519)

---

## 📞 Quick Stats

- **Total Source Files**: ~190 C# files
- **Target Framework**: .NET 8.0
- **Supported Databases**: 4 (SQL Server, MySQL, PostgreSQL, SQLite)
- **Auth Protocols**: 2 (OAuth 2.0, OpenID Connect)
- **Grant Types**: 4 (Auth Code, Auth Code+PKCE, Client Credentials, Refresh Token)
- **MFA Methods**: 3 (Email, SMS, Authenticator)

---

*For detailed documentation, see [PROJECT_OVERVIEW.md](PROJECT_OVERVIEW.md)*
