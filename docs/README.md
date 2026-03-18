# HCL.CS.SF

![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-purple)
![OAuth 2.0](https://img.shields.io/badge/OAuth-2.0-green)
![OpenID Connect](https://img.shields.io/badge/OpenID-Connect-orange)

**HCL.CS.SF** is a comprehensive OAuth 2.0 and OpenID Connect (OIDC) authentication and authorization framework built on
.NET 8.0. It provides enterprise-grade identity management, user authentication, role-based access control (RBAC), and
token-based security for modern applications.

---

## 🚀 Quick Links

- 📖 **[Complete Project Overview](PROJECT_OVERVIEW.md)** - Comprehensive documentation covering architecture,
  components, and features
- ⚡ **[Quick Reference Guide](QUICK_REFERENCE.md)** - Fast lookup for configurations, enums, and key concepts
- 🎓 **[Getting Started Guide](GETTING_STARTED.md)** - Developer onboarding with diagrams, workflows, and examples

---

## 🎯 What is HCL.CS.SF?

HCL.CS.SF is a complete identity and access management solution that:

- ✅ Manages user authentication and authorization
- ✅ Implements OAuth 2.0 and OpenID Connect protocols
- ✅ Provides secure JWT token generation and validation
- ✅ Supports multiple authentication providers (Local, LDAP)
- ✅ Offers comprehensive user and role management
- ✅ Enables multi-factor authentication (MFA)
- ✅ Maintains complete audit trails and security logs
- ✅ Supports multiple databases (SQL Server, MySQL, PostgreSQL, SQLite)

---

## 🏗️ Architecture

The project follows **Clean Architecture** principles with clear separation of concerns:

```
┌─────────────────────────────────────────┐
│       Presentation Layer                │
│   (Demo Apps, Hosting, Controllers)     │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│       Application Layer                 │
│   (Service, Service.Interfaces)         │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│          Domain Layer                   │
│   (Domain, DomainServices)              │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│      Infrastructure Layer               │
│   (Infrastructure.Data, .Services)      │
└─────────────────────────────────────────┘
```

---

## ✨ Key Features

### 🔐 Security Features

- **OAuth 2.0 Flows**: Authorization Code, Authorization Code + PKCE, Client Credentials, Refresh Token
- **OpenID Connect**: Complete OIDC implementation with ID tokens
- **Multi-Factor Authentication**: Email OTP, SMS OTP, Authenticator apps
- **Password Security**: Complexity rules, history tracking, expiration policies
- **Account Protection**: Lockout mechanisms, brute force prevention

### 👥 User Management

- User registration with email/phone confirmation
- Profile management
- Password reset and recovery
- Security question-based recovery
- Soft delete support

### 🎭 Role-Based Access Control

- Create and manage roles
- Assign users to roles
- Claims-based authorization
- API scope management

### 📊 Audit & Compliance

- Complete audit trail
- Login/logout tracking
- Change tracking (Created/Modified by/on)
- Notification delivery tracking

### 🗄️ Multi-Database Support

- Microsoft SQL Server
- MySQL
- PostgreSQL
- SQLite

---

## 💻 Technology Stack

- **.NET 8.0**: Latest LTS version
- **ASP.NET Core Identity 8.0**: Foundation for identity management
- **Entity Framework Core 8.0**: ORM for database access
- **JWT**: JSON Web Tokens for secure authentication
- **OAuth 2.0 & OpenID Connect**: Industry-standard protocols

---

## 📁 Project Structure

```
HCL.CS.SF/
├── Source/                 # Core framework
│   ├── Domain/            # Business entities & models
│   ├── DomainServices/    # Repository interfaces
│   ├── Infrastructure.*   # Data access & services
│   ├── Service/           # Business logic
│   └── Hosting/           # Web hosting
├── Demo/                  # Example applications
│   ├── DemoServerApp/    # Auth server demo
│   └── DemoClientCoreMVC/ # Client app demo
└── Installer/             # Setup tools
    ├── DBMigration/       # Database migrations
    └── SeedDataCreator/   # Initial data
```

---

## 🎓 Documentation

### For New Users

1. Start with **[Quick Reference Guide](QUICK_REFERENCE.md)** for an overview
2. Read **[Getting Started Guide](GETTING_STARTED.md)** for setup instructions
3. Explore the Demo applications

### For Developers

1. Review **[Project Overview](PROJECT_OVERVIEW.md)** for architecture details
2. Study **[Getting Started Guide](GETTING_STARTED.md)** for development workflows
3. Use **[Quick Reference](QUICK_REFERENCE.md)** as a lookup guide

### For Architects

1. Read **[Project Overview](PROJECT_OVERVIEW.md)** for comprehensive architecture
2. Review the Clean Architecture implementation
3. Study security best practices and OAuth flows

---

## 🚀 Quick Start

```bash
# 1. Clone the repository
git clone <repository-url>
cd HCL.CS.SF

# 2. Restore packages
dotnet restore

# 3. Build the solution
dotnet build

# 4. Set up the database
cd Installer/DBMigration
dotnet run

# 5. Run the demo
cd Demo/DemoServerApp
dotnet run
```

For detailed setup instructions, see **[Getting Started Guide](GETTING_STARTED.md)**.

---

## 📊 Key Statistics

- **190+ C# files**: Comprehensive implementation
- **.NET 8.0**: Latest LTS framework
- **4 Database Systems**: SQL Server, MySQL, PostgreSQL, SQLite
- **2 Auth Protocols**: OAuth 2.0 & OpenID Connect
- **4 Grant Types**: Authorization Code, PKCE, Client Credentials, Refresh Token
- **3 MFA Methods**: Email, SMS, Authenticator

---

## 🔒 Security

HCL.CS.SF implements security best practices:

- ✅ Password hashing with ASP.NET Core Identity
- ✅ HTTPS enforcement
- ✅ Token expiration and rotation
- ✅ PKCE for public clients
- ✅ Account lockout protection
- ✅ Complete audit logging
- ✅ Input validation
- ✅ CSRF protection

---

## 🎯 Use Cases

- **Enterprise Single Sign-On (SSO)**: Central authentication for multiple applications
- **Microservices Authentication**: Secure microservices with centralized tokens
- **Third-Party Integration**: Enable OAuth for external applications
- **Mobile & SPA Apps**: Support for PKCE-enabled flows
- **API Gateway Security**: Token-based API protection

---

## 📚 Resources

- [OAuth 2.0 RFC 6749](https://tools.ietf.org/html/rfc6749)
- [OpenID Connect Core 1.0](https://openid.net/specs/openid-connect-core-1_0.html)
- [PKCE RFC 7636](https://tools.ietf.org/html/rfc7636)
- [JWT RFC 7519](https://tools.ietf.org/html/rfc7519)

---

## 📄 License

[Add license information here]

---

## 🤝 Contributing

[Add contribution guidelines here]

---

## 📞 Support

For questions and support, please refer to the documentation:

- **[Project Overview](PROJECT_OVERVIEW.md)**
- **[Quick Reference](QUICK_REFERENCE.md)**
- **[Getting Started](GETTING_STARTED.md)**

---

**Built with ❤️ using .NET 8.0**
