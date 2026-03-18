# HCL.CS.SF - Comprehensive Project Overview

## 📋 Table of Contents
1. [Introduction](#introduction)
2. [Project Architecture](#project-architecture)
3. [Core Components](#core-components)
4. [Database Entities](#database-entities)
5. [Configuration System](#configuration-system)
6. [Security Features](#security-features)
7. [Technology Stack](#technology-stack)
8. [Project Structure](#project-structure)
9. [Key Features](#key-features)
10. [Use Cases](#use-cases)

---

## 🎯 Introduction

**HCL.CS.SF** is a comprehensive **OAuth 2.0 and OpenID Connect (OIDC) authentication and authorization HCL.CS.SF** built on .NET 8.0. It provides enterprise-grade identity management, user authentication, role-based access control (RBAC), and token-based security for modern applications.

### Purpose
The framework serves as a complete identity and access management solution that:
- Manages user authentication and authorization
- Implements OAuth 2.0 and OpenID Connect protocols
- Provides secure token generation and validation
- Supports multiple authentication providers (Local, LDAP)
- Offers comprehensive user and role management
- Enables multi-factor authentication (MFA)
- Maintains audit trails and security logs

---

## 🏗️ Project Architecture

The project follows **Clean Architecture** principles with clear separation of concerns:

```
┌─────────────────────────────────────────────────┐
│              Presentation Layer                  │
│  (Demo Apps, Hosting, Controllers)              │
└───────────────┬─────────────────────────────────┘
                │
┌───────────────▼─────────────────────────────────┐
│            Application Layer                     │
│  (Service, Service.Interfaces, ProxyService)    │
└───────────────┬─────────────────────────────────┘
                │
┌───────────────▼─────────────────────────────────┐
│              Domain Layer                        │
│  (Domain, DomainServices, DomainValidation)     │
└───────────────┬─────────────────────────────────┘
                │
┌───────────────▼─────────────────────────────────┐
│          Infrastructure Layer                    │
│  (Infrastructure.Data, Infrastructure.Services) │
└─────────────────────────────────────────────────┘
```

### Architectural Layers

1. **Domain Layer** (`Source/Domain`)
   - Contains core business entities
   - Defines domain models and configurations
   - Houses enums and constants
   - Platform-agnostic business logic

2. **Domain Services** (`Source/DomainServices`)
   - Repository interfaces (IRepository pattern)
   - Unit of Work interfaces
   - Infrastructure service contracts
   - Database context abstraction

3. **Infrastructure Layer**
   - **Infrastructure.Data**: Entity Framework Core implementations, repositories, database context
   - **Infrastructure.Services**: Email, SMS, logging services
   - **Infrastructure.Resources**: Resource strings and localization

4. **Application Layer**
   - **Service**: Business logic implementation
   - **Service.Interfaces**: Service contracts
   - **ProxyService**: API proxy services

5. **Presentation Layer**
   - **Hosting**: ASP.NET Core hosting configuration
   - **Demo Apps**: Sample implementations (Client & Server)

---

## 🧩 Core Components

### 1. **Domain Entities**

#### API Entities (Identity Management)
Located in `Source/Domain/Entities/Api/`:

- **Users**: Core user entity extending ASP.NET Core Identity's `IdentityUser<Guid>`
  - Properties: FirstName, LastName, DateOfBirth, TwoFactorType, LastPasswordChangedDate
  - Authentication tracking: LastLoginDateTime, LastLogoutDateTime
  - Identity provider type (Local/LDAP)
  - Soft delete support

- **Roles**: Role entity extending `IdentityRole<Guid>`
  - Role description
  - Audit trail (CreatedOn, ModifiedOn, CreatedBy, ModifiedBy)

- **UserClaims**: Custom claims associated with users
- **RoleClaims**: Claims associated with roles
- **UserRoles**: Many-to-many relationship between users and roles
- **UserTokens**: Token management for users
- **UserLogins**: External login provider data
- **PasswordHistory**: Track password history for reuse prevention
- **UserSecurityQuestions**: Security questions for account recovery
- **AuditTrail**: Comprehensive audit logging

#### Endpoint Entities (OAuth/OIDC)
Located in `Source/Domain/Entities/Endpoint/`:

- **Clients**: OAuth 2.0 client applications
  - Client credentials (ClientId, ClientSecret)
  - Token expiration settings (AccessToken, RefreshToken, IdentityToken)
  - Grant types and response types
  - PKCE configuration
  - Redirect URIs and logout URIs
  - Application type (Web, SPA, Native)

- **ClientRedirectUris**: Allowed redirect URIs for clients
- **ClientPostLogoutRedirectUris**: Post-logout redirect URIs
- **SecurityTokens**: Token storage and management

#### Resource Entities
- **ApiResources**: API resource definitions
- **ApiScopes**: API scopes for authorization
- **ApiResourceClaims** & **ApiScopeClaims**: Claims for APIs
- **IdentityResources**: Identity information resources
- **IdentityClaims**: Claims for identity resources

### 2. **Configuration System**

The framework uses a hierarchical configuration model (`HCL.CS.SFConfig`):

#### System Settings
- **DbConfig**: Multi-database support (SQL Server, MySQL, PostgreSQL, SQLite)
- **LoginConfig**: Session persistence, lockout settings
- **UserConfig**: 
  - Username/password requirements
  - Email/phone validation
  - Account lockout policies
  - Token expiration settings
  - Default roles
- **PasswordConfig**:
  - Length requirements (8-64 characters)
  - Complexity rules (uppercase, lowercase, digits, special chars)
  - Password history (prevent reuse of last 10 passwords)
  - Expiration policies (42 days default)
- **EmailConfig**: SMTP configuration
- **SmsConfig**: SMS provider settings
- **LdapConfig**: LDAP/Active Directory integration
- **CryptoConfig**: Cryptographic settings
- **LogConfig**: Logging configuration

#### Token Settings
- **TokenConfig**: Token generation and validation
- **AuthenticationConfig**: Authentication flow settings
- **InputLengthRestrictionsConfig**: Input validation
- **UserInteractionConfig**: UI interaction settings
- **EndpointsConfig**: OAuth/OIDC endpoint configuration
- **TokenExpiration**: Token lifetime management

#### Notification Templates
- **Email Templates**: Customizable email notifications
- **SMS Templates**: SMS message templates

---

## 🗄️ Database Entities

The framework uses **ASP.NET Core Identity** as its foundation with extensive customizations:

### Database Context
`ApplicationDbContext` extends `IdentityDbContext` and includes:
- All Identity tables (Users, Roles, UserRoles, UserClaims, RoleClaims)
- Custom tables (Clients, SecurityTokens, ApiResources, etc.)
- Support for soft deletes
- Audit trail tracking

### Entity Relationships
```
Users (1) ──── (N) UserRoles ──── (N) Roles
  │                                     │
  ├─── (N) UserClaims               (N) RoleClaims
  ├─── (N) UserLogins
  ├─── (N) UserTokens
  ├─── (N) PasswordHistory
  └─── (N) UserSecurityQuestions

Clients (1) ──── (N) ClientRedirectUris
         └────── (N) ClientPostLogoutRedirectUris

ApiResources (1) ──── (N) ApiResourceClaims
ApiScopes (1) ──────── (N) ApiScopeClaims
IdentityResources (1) ─ (N) IdentityClaims
```

---

## 🔒 Security Features

### 1. **Authentication Methods**
- **Local Authentication**: Username/password with hashing
- **LDAP/Active Directory Integration**: Enterprise authentication
- **Multi-Factor Authentication (MFA)**:
  - Email-based OTP
  - SMS-based OTP
  - Authenticator apps
  - None (can be disabled)

### 2. **Authorization**
- **Role-Based Access Control (RBAC)**
- **Claims-Based Authorization**
- **API Scopes and Resources**
- **Client-specific permissions**

### 3. **OAuth 2.0 & OpenID Connect**
- **Grant Types**:
  - Authorization Code
  - Authorization Code with PKCE
  - Client Credentials
  - Refresh Token
- **Token Types**:
  - Access Tokens (JWT)
  - Refresh Tokens
  - Identity Tokens (ID Token)
  - Authorization Codes
- **PKCE Support**: Protection against authorization code interception
- **Token Management**:
  - Configurable expiration times
  - Token revocation
  - Refresh token rotation

### 4. **Password Security**
- **Complexity Requirements**: Uppercase, lowercase, digits, special characters
- **Password History**: Prevent reuse of last N passwords (default: 10)
- **Password Expiration**: Automatic expiration after 42 days
- **Password Reset**: Secure token-based reset flow
- **Breach Detection**: Can be integrated

### 5. **Account Security**
- **Account Lockout**: After failed login attempts (default: 5 attempts)
- **Lockout Duration**: Configurable (default: 10 minutes)
- **Email Confirmation**: Required for new accounts
- **Phone Number Confirmation**: Optional/Required
- **Security Questions**: Account recovery mechanism

### 6. **Audit & Compliance**
- **Audit Trail**: Complete logging of user actions
- **Login Tracking**: Login/logout timestamps
- **Change Tracking**: CreatedBy, ModifiedBy, CreatedOn, ModifiedOn
- **Notification Logging**: Email/SMS delivery status

---

## 💻 Technology Stack

### Core Technologies
- **.NET 8.0**: Latest LTS version of .NET
- **ASP.NET Core Identity**: Foundation for identity management
- **Entity Framework Core 8.0**: ORM for database access
- **C# 12**: Latest C# language features

### Key NuGet Packages
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (8.0.4)
- `Microsoft.IdentityModel.Tokens` (7.5.1)
- `System.IdentityModel.Tokens.Jwt` (7.5.1)
- `Newtonsoft.Json` (13.0.1)

### Database Support
- Microsoft SQL Server
- MySQL
- PostgreSQL
- SQLite

### Authentication & Security
- OAuth 2.0
- OpenID Connect (OIDC)
- JWT (JSON Web Tokens)
- PKCE (Proof Key for Code Exchange)

---

## 📁 Project Structure

```
HCL.CS.SF/
├── Source/                          # Core framework source code
│   ├── Domain/                      # Domain entities and models
│   │   ├── Entities/
│   │   │   ├── Api/                # Identity entities (Users, Roles, etc.)
│   │   │   └── Endpoint/           # OAuth/OIDC entities (Clients, Tokens)
│   │   ├── Configurations/         # Configuration classes
│   │   ├── Constants/              # Application constants
│   │   ├── Enums/                  # Enumerations
│   │   └── Models/                 # Domain models
│   ├── DomainServices/             # Repository and service interfaces
│   │   ├── Repository/             # IRepository interfaces
│   │   ├── UnitOfWork/             # IUnitOfWork interfaces
│   │   └── Infra/                  # Infrastructure service interfaces
│   ├── DomainValidation/           # Domain validation logic
│   ├── Infrastructure.Data/        # Data access implementation
│   │   ├── Repository/             # Repository implementations
│   │   ├── UnitOfWork/             # Unit of Work implementations
│   │   └── ApplicationDbContext.cs # EF Core DbContext
│   ├── Infrastructure.Services/    # Service implementations
│   ├── Infrastructure.Resources/   # Resource strings
│   ├── Service/                    # Business logic services
│   ├── Service.Interfaces/         # Service contracts
│   ├── ProxyService/               # API proxy services
│   └── Hosting/                    # ASP.NET Core hosting
│
├── Demo/                           # Demonstration applications
│   ├── DemoServerApp/             # Server-side demo (OAuth Provider)
│   └── DemoClientCoreMVC/         # Client-side demo (OAuth Client)
│
├── Installer/                      # Installation utilities
│   ├── DBMigration/               # Database migration tool
│   └── SeedDataCreator/           # Seed data generation
│
├── Integration_Test/              # Integration tests
│   ├── IntegrationTest/           # Test project
│   └── TestApp.Helper/            # Test helpers
│
└── HCL.CS.SF.sln              # Solution file
```

---

## ✨ Key Features

### 1. **User Management**
- User registration with email/phone confirmation
- User profile management
- Password management (change, reset, history)
- Account lockout and recovery
- Security question-based recovery
- Soft delete support

### 2. **Role Management**
- Create, update, delete roles
- Assign roles to users
- Role-based permissions
- Role claims management

### 3. **OAuth 2.0 Client Management**
- Register OAuth 2.0 clients
- Configure client credentials
- Manage redirect URIs
- Set token expiration policies
- Configure grant types and response types

### 4. **Token Management**
- Generate access tokens (JWT)
- Issue refresh tokens
- Create identity tokens
- Token validation and verification
- Token revocation

### 5. **API Resource Management**
- Define protected API resources
- Configure API scopes
- Assign claims to resources

### 6. **Identity Resource Management**
- Standard OpenID Connect scopes (openid, profile, email)
- Custom identity resources
- Claims mapping

### 7. **Notification System**
- Email notifications (registration, password reset, etc.)
- SMS notifications (OTP, alerts)
- Customizable templates
- Delivery status tracking

### 8. **Logging & Auditing**
- Application logging (file or database)
- Audit trail for user actions
- Security event logging
- Configurable log levels

### 9. **Multi-Database Support**
- SQL Server
- MySQL
- PostgreSQL
- SQLite

---

## 🎯 Use Cases

### 1. **Enterprise Single Sign-On (SSO)**
Organizations can use HCL.CS.SF as a central authentication provider for multiple internal applications.

### 2. **Microservices Authentication**
Secure microservices architecture with centralized token issuance and validation.

### 3. **Third-Party Application Integration**
Enable third-party applications to authenticate users via OAuth 2.0.

### 4. **Mobile & SPA Applications**
Support for mobile apps and Single Page Applications with PKCE-enabled authorization code flow.

### 5. **API Gateway Security**
Protect API gateways with token-based authentication and scope-based authorization.

### 6. **B2B & B2C Platforms**
Support both business-to-business and business-to-consumer authentication scenarios.

---

## 🔧 Configuration Example

```csharp
var securityConfig = new HCL.CS.SFConfig
{
    SystemSettings = new SystemSettings
    {
        DbConfig = new DbConfig
        {
            Database = DbTypes.SqlServer,
            DbConnectionString = "Server=...;Database=HCL.CS.SF;..."
        },
        UserConfig = new UserConfig
        {
            RequireUniqueEmail = true,
            RequireConfirmedEmail = true,
            MaxFailedAccessAttempts = 5,
            DefaultLockoutTimeSpanMinutes = 10
        },
        PasswordConfig = new PasswordConfig
        {
            MinPasswordLength = 8,
            RequireDigit = true,
            RequireUppercase = true,
            RequireLowercase = true,
            RequireSpecialCharacters = true,
            MaxLimitPasswordReuse = 10
        }
    },
    TokenSettings = new TokenSettings
    {
        TokenExpiration = new TokenExpiration
        {
            AccessTokenExpiration = 3600,      // 1 hour
            RefreshTokenExpiration = 18000,    // 5 hours
            IdentityTokenExpiration = 3600     // 1 hour
        }
    }
};
```

---

## 📊 Entity Enumerations

### Key Enums

1. **TwoFactorType**: None, Email, SMS, Authenticator
2. **IdentityProvider**: Local, LDAP
3. **ApplicationType**: Web, NativeApp, SPA
4. **AccessTokenType**: JWT, Reference
5. **EmailNotificationType**: Link, OTP
6. **DbTypes**: SqlServer, MySql, PostgresSql, Sqlite
7. **NotificationStatus**: Initiated, Delivered, Failed, Sent, etc.

---

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK
- Database (SQL Server, MySQL, PostgreSQL, or SQLite)
- SMTP server (for email notifications)
- SMS provider (optional, for SMS notifications)

### Installation Steps
1. Clone the repository
2. Configure `HCL.CS.SFConfig` settings
3. Run database migrations using the DBMigration tool
4. Seed initial data using SeedDataCreator
5. Configure client applications
6. Start the authentication server

---

## 🔐 Security Considerations

### Best Practices Implemented
- ✅ Password hashing with ASP.NET Core Identity
- ✅ HTTPS enforcement
- ✅ Token expiration and rotation
- ✅ PKCE for public clients
- ✅ Account lockout protection
- ✅ Audit logging
- ✅ Input validation
- ✅ CSRF protection
- ✅ SQL injection prevention (via EF Core)

---

## 📝 Summary

**HCL.CS.SF** is a production-ready, enterprise-grade authentication and authorization framework that provides:

- 🔒 **Comprehensive Security**: OAuth 2.0, OpenID Connect, JWT, MFA
- 👥 **User & Role Management**: Complete RBAC implementation
- 🎯 **Flexible Configuration**: Highly configurable to meet various requirements
- 💾 **Multi-Database Support**: Works with major database systems
- 📧 **Notification System**: Email and SMS notifications
- 📊 **Audit & Compliance**: Complete audit trail and logging
- 🏗️ **Clean Architecture**: Maintainable and testable codebase
- 🔌 **Extensible**: Easy to extend and customize

The framework follows industry best practices and standards, making it suitable for both small applications and large-scale enterprise systems.

---

## 📚 Additional Resources

- **OAuth 2.0 Specification**: [RFC 6749](https://tools.ietf.org/html/rfc6749)
- **OpenID Connect**: [OpenID Connect Core 1.0](https://openid.net/specs/openid-connect-core-1_0.html)
- **PKCE**: [RFC 7636](https://tools.ietf.org/html/rfc7636)
- **JWT**: [RFC 7519](https://tools.ietf.org/html/rfc7519)

---

*This documentation provides a comprehensive overview of the HCL.CS.SF framework. For specific implementation details, please refer to the source code and inline documentation.*
