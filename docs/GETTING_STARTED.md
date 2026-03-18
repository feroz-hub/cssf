# HCL.CS.SF - Developer Getting Started Guide

## 🚀 Quick Start

This guide will help you understand and work with the HCL.CS.SF framework.

---

## 📖 Table of Contents

1. [Understanding the Project](#understanding-the-project)
2. [Architecture Diagrams](#architecture-diagrams)
3. [Key Components Explained](#key-components-explained)
4. [Development Workflow](#development-workflow)
5. [Common Scenarios](#common-scenarios)
6. [Troubleshooting](#troubleshooting)

---

## 🎓 Understanding the Project

### What Problem Does It Solve?

HCL.CS.SF solves the complex problem of **authentication and authorization** in modern applications by providing:

1. **Centralized Authentication**: Single place to manage all users
2. **Token-Based Security**: Secure API access without sharing passwords
3. **Multi-App Support**: One auth server, many client applications
4. **Standards Compliance**: OAuth 2.0 and OpenID Connect protocols
5. **Enterprise Features**: LDAP, MFA, audit trails, etc.

### Real-World Example

Imagine you have:
- A web application (Customer Portal)
- A mobile app (Customer App)
- Several microservices (Order Service, Payment Service, etc.)

**Without HCL.CS.SF:**
- Each app manages its own users and passwords
- Passwords stored in multiple places
- No single logout
- Hard to implement MFA
- Difficult to audit access

**With HCL.CS.SF:**
- Centralized user management
- Users log in once (SSO)
- Apps get tokens to access APIs
- Easy to add MFA
- Complete audit trail
- Single logout works everywhere

---

## 🏛️ Architecture Diagrams

### High-Level System Architecture

```
┌──────────────────────────────────────────────────────────────────┐
│                     HCL.CS.SF Ecosystem                       │
└──────────────────────────────────────────────────────────────────┘

┌─────────────────┐         ┌──────────────────────────────────┐
│   Web Client    │────────▶│   HCL.CS.SF Server           │
│  (MVC/Blazor)   │         │   (Authorization Server)         │
└─────────────────┘         │                                  │
                            │  ┌────────────────────────────┐  │
┌─────────────────┐         │  │  User Management           │  │
│  Mobile Client  │────────▶│  │  - Registration            │  │
│  (iOS/Android)  │         │  │  - Login/Logout            │  │
└─────────────────┘         │  │  - Password Reset          │  │
                            │  │  - MFA                     │  │
┌─────────────────┐         │  └────────────────────────────┘  │
│   SPA Client    │────────▶│                                  │
│  (React/Angular)│         │  ┌────────────────────────────┐  │
└─────────────────┘         │  │  Token Management          │  │
                            │  │  - Issue Access Tokens     │  │
        ▲                   │  │  - Issue Refresh Tokens    │  │
        │                   │  │  - Validate Tokens         │  │
        │ Access Token      │  │  - Revoke Tokens           │  │
        │                   │  └────────────────────────────┘  │
        │                   │                                  │
┌───────┴──────┐            │  ┌────────────────────────────┐  │
│              │            │  │  OAuth/OIDC Endpoints      │  │
│  Protected   │            │  │  - /authorize              │  │
│  Resources   │◀───────────│  │  - /token                  │  │
│  (APIs)      │            │  │  - /userinfo               │  │
│              │            │  │  - /.well-known/openid-    │  │
└──────────────┘            │  │    configuration           │  │
                            │  └────────────────────────────┘  │
                            │                                  │
                            │  ┌────────────────────────────┐  │
                            │  │  Database                  │  │
                            │  │  - Users & Roles           │  │
                            │  │  - Clients                 │  │
                            │  │  - Tokens                  │  │
                            │  │  - Audit Logs              │  │
                            │  └────────────────────────────┘  │
                            └──────────────────────────────────┘
```

### Clean Architecture Layers

```
┌────────────────────────────────────────────────────────────┐
│                    Presentation Layer                       │
│  Controllers, Views, API Endpoints, UI Components          │
│                                                             │
│  Projects: Hosting, Demo Apps                              │
└────────────┬───────────────────────────────────────────────┘
             │ Depends on ↓
┌────────────▼───────────────────────────────────────────────┐
│                   Application Layer                         │
│  Business Logic, Use Cases, Services                       │
│                                                             │
│  Projects: Service, Service.Interfaces, ProxyService       │
└────────────┬───────────────────────────────────────────────┘
             │ Depends on ↓
┌────────────▼───────────────────────────────────────────────┐
│                     Domain Layer                            │
│  Entities, Value Objects, Domain Logic, Interfaces         │
│                                                             │
│  Projects: Domain, DomainServices, DomainValidation        │
└────────────┬───────────────────────────────────────────────┘
             │ Implemented by ↓
┌────────────▼───────────────────────────────────────────────┐
│                 Infrastructure Layer                        │
│  Data Access, External Services, Repositories              │
│                                                             │
│  Projects: Infrastructure.Data, Infrastructure.Services    │
└────────────────────────────────────────────────────────────┘

Flow of Dependencies: UI → Services → Domain ← Infrastructure
                                       ↑______________|
```

### OAuth 2.0 Authorization Code Flow (with PKCE)

```
┌──────────┐                                    ┌──────────────┐
│  Client  │                                    │    Auth      │
│   App    │                                    │   Server     │
└────┬─────┘                                    └──────┬───────┘
     │                                                  │
     │ 1. Generate code_verifier (random string)      │
     │    Hash to create code_challenge                │
     │                                                  │
     │ 2. GET /authorize?                              │
     │    client_id=abc&                               │
     │    redirect_uri=...&                            │
     │    response_type=code&                          │
     │    code_challenge=xyz&                          │
     │    code_challenge_method=S256                   │
     ├─────────────────────────────────────────────────▶
     │                                                  │
     │                              3. User Login      │
     │                                 & Consent       │
     │                                                  │
     │ 4. Redirect with Authorization Code             │
     │    https://client.com/callback?code=AUTH_CODE   │
     │◀─────────────────────────────────────────────────┤
     │                                                  │
     │ 5. POST /token                                   │
     │    grant_type=authorization_code&               │
     │    code=AUTH_CODE&                              │
     │    redirect_uri=...&                            │
     │    client_id=abc&                               │
     │    code_verifier=original_random_string         │
     ├─────────────────────────────────────────────────▶
     │                                                  │
     │              6. Verify code_verifier            │
     │                 matches code_challenge           │
     │                                                  │
     │ 7. Return Tokens                                │
     │    {                                            │
     │      "access_token": "...",                     │
     │      "refresh_token": "...",                    │
     │      "id_token": "...",                         │
     │      "token_type": "Bearer",                    │
     │      "expires_in": 3600                         │
     │    }                                            │
     │◀─────────────────────────────────────────────────┤
     │                                                  │
     │ 8. Use access_token to call APIs                │
     │                                                  │
```

### Database Entity Relationship Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        Identity Management                       │
└─────────────────────────────────────────────────────────────────┘

         ┌──────────────┐
         │    Users     │
         │──────────────│
         │ Id (PK)      │
         │ UserName     │
         │ Email        │
         │ PasswordHash │
         │ FirstName    │
         │ LastName     │
         │ DateOfBirth  │
         │ ...          │
         └──────┬───────┘
                │
                │ 1:N
                │
         ┌──────▼───────────┐
         │   UserRoles      │────────┐
         │──────────────────│        │
         │ UserId (FK)      │        │ N:1
         │ RoleId (FK)      │        │
         └──────────────────┘   ┌────▼────────┐
                                │   Roles     │
                                │─────────────│
         ┌──────────────┐       │ Id (PK)     │
         │  UserClaims  │       │ Name        │
         │──────────────│       │ Description │
         │ UserId (FK)  │       └────┬────────┘
         │ ClaimType    │            │
         │ ClaimValue   │            │ 1:N
         └──────────────┘            │
                                ┌────▼─────────┐
         ┌──────────────────┐  │ RoleClaims   │
         │ PasswordHistory  │  │──────────────│
         │──────────────────│  │ RoleId (FK)  │
         │ UserId (FK)      │  │ ClaimType    │
         │ PasswordHash     │  │ ClaimValue   │
         │ CreatedOn        │  └──────────────┘
         └──────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                      OAuth/OIDC Management                       │
└─────────────────────────────────────────────────────────────────┘

         ┌──────────────────┐
         │     Clients      │
         │──────────────────│
         │ Id (PK)          │
         │ ClientId         │
         │ ClientSecret     │
         │ ClientName       │
         │ AllowedScopes    │
         │ GrantTypes       │
         │ ...              │
         └────┬─────────────┘
              │
              │ 1:N
              │
    ┌─────────▼────────────────┐
    │ ClientRedirectUris       │
    │──────────────────────────│
    │ ClientId (FK)            │
    │ RedirectUri              │
    └──────────────────────────┘

    ┌──────────────────────────┐
    │ SecurityTokens           │
    │──────────────────────────│
    │ Id (PK)                  │
    │ Token                    │
    │ TokenType                │
    │ ClientId                 │
    │ ExpiresAt                │
    └──────────────────────────┘

         ┌─────────────────┐
         │  ApiResources   │
         │─────────────────│
         │ Id (PK)         │
         │ Name            │
         │ DisplayName     │
         └────┬────────────┘
              │
              │ 1:N
              │
         ┌────▼──────────────────┐
         │ ApiResourceClaims     │
         │───────────────────────│
         │ ApiResourceId (FK)    │
         │ Type                  │
         └───────────────────────┘

         ┌─────────────────┐
         │   ApiScopes     │
         │─────────────────│
         │ Id (PK)         │
         │ Name            │
         │ DisplayName     │
         └────┬────────────┘
              │
              │ 1:N
              │
         ┌────▼──────────────┐
         │ ApiScopeClaims    │
         │───────────────────│
         │ ApiScopeId (FK)   │
         │ Type              │
         └───────────────────┘
```

---

## 🔍 Key Components Explained

### 1. Domain Layer

**Purpose**: Defines the business rules and entities

**Key Files**:
- `Users.cs`: User entity with profile data
- `Roles.cs`: Role definitions
- `Clients.cs`: OAuth client applications
- `HCL.CS.SFConfig.cs`: Configuration model

**Example**:
```csharp
// Users entity
public class Users : IdentityUser<Guid>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public TwoFactorType TwoFactorType { get; set; }
    public DateTime LastPasswordChangedDate { get; set; }
    // ... more properties
}
```

### 2. Repository Pattern

**Purpose**: Abstract data access logic

**Pattern**:
```
Interface (DomainServices) → Implementation (Infrastructure.Data)
```

**Example**:
```csharp
// Interface in DomainServices
public interface IUserRepository : IRepository<Users>
{
    Task<Users> GetByEmailAsync(string email);
    Task<Users> GetByUsernameAsync(string username);
}

// Implementation in Infrastructure.Data
public class UserRepository : BaseRepository<Users>, IUserRepository
{
    public async Task<Users> GetByEmailAsync(string email)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }
}
```

### 3. Unit of Work Pattern

**Purpose**: Group related repositories and manage transactions

**Example**:
```csharp
public interface IUserManagementUnitOfWork
{
    IUserRepository Users { get; }
    IUserClaimRepository UserClaims { get; }
    IUserRoleRepository UserRoles { get; }
    Task<FrameworkResult> SaveChangesAsync();
}
```

### 4. Configuration System

**Purpose**: Centralized settings management

**Structure**:
```csharp
HCL.CS.SFConfig
    └── SystemSettings
        ├── DbConfig (database settings)
        ├── UserConfig (user policies)
        ├── PasswordConfig (password rules)
        ├── EmailConfig (SMTP settings)
        └── ... more configs
    
    └── TokenSettings
        ├── TokenConfig
        ├── AuthenticationConfig
        └── TokenExpiration
    
    └── NotificationTemplateSettings
        ├── EmailTemplates
        └── SmsTemplates
```

---

## 🛠️ Development Workflow

### Setting Up the Project

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd HCL.CS.SF
   ```

2. **Open the solution**
   ```bash
   # Visual Studio
   start HCL.CS.SF.sln
   
   # VS Code
   code .
   ```

3. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

4. **Build the solution**
   ```bash
   dotnet build
   ```

### Database Setup

1. **Configure connection string** in `appsettings.json`
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=HCL.CS.SF;..."
     }
   }
   ```

2. **Run migrations** using the DBMigration tool
   ```bash
   cd Installer/DBMigration
   dotnet run
   ```

3. **Seed initial data**
   ```bash
   cd Installer/SeedDataCreator
   dotnet run
   ```

### Running the Demo

1. **Start the Auth Server**
   ```bash
   cd Demo/DemoServerApp
   dotnet run
   ```
   Opens at: `https://localhost:5001`

2. **Start the Client App**
   ```bash
   cd Demo/DemoClientCoreMVC
   dotnet run
   ```
   Opens at: `https://localhost:5002`

3. **Test the flow**
   - Navigate to client app
   - Click "Login"
   - Redirected to auth server
   - Enter credentials
   - Redirected back to client
   - Authenticated!

---

## 📝 Common Scenarios

### Scenario 1: Add a New User

```csharp
// Using UserManager from ASP.NET Core Identity
var user = new Users
{
    UserName = "john.doe",
    Email = "john@example.com",
    FirstName = "John",
    LastName = "Doe",
    DateOfBirth = new DateTime(1990, 1, 1),
    TwoFactorType = TwoFactorType.Email
};

var result = await _userManager.CreateAsync(user, "Password123!");

if (result.Succeeded)
{
    // Optionally assign role
    await _userManager.AddToRoleAsync(user, "User");
    
    // Send confirmation email
    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
    // ... send email with token
}
```

### Scenario 2: Register an OAuth Client

```csharp
var client = new Clients
{
    ClientId = "mobile-app",
    ClientName = "My Mobile App",
    ClientSecret = GenerateSecureSecret(), // Use crypto service
    ApplicationType = ApplicationType.NativeApp,
    RequirePkce = true, // Always true for mobile apps
    AllowOfflineAccess = true, // Allow refresh tokens
    AllowedScopes = "openid profile email api.read",
    SupportedGrantTypes = "authorization_code refresh_token",
    SupportedResponseTypes = "code",
    AccessTokenExpiration = 3600,
    RefreshTokenExpiration = 18000,
    RedirectUris = new List<ClientRedirectUris>
    {
        new() { RedirectUri = "myapp://callback" }
    }
};

await _clientRepository.AddAsync(client);
await _unitOfWork.SaveChangesAsync();
```

### Scenario 3: Validate a JWT Token

```csharp
var tokenHandler = new JwtSecurityTokenHandler();
var validationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidIssuer = "https://auth.myapp.com",
    ValidateAudience = true,
    ValidAudience = "my-api",
    ValidateLifetime = true,
    IssuerSigningKey = GetSigningKey()
};

try
{
    var principal = tokenHandler.ValidateToken(
        token, 
        validationParameters, 
        out SecurityToken validatedToken
    );
    
    // Token is valid
    var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}
catch (SecurityTokenException)
{
    // Token is invalid
}
```

### Scenario 4: Implement Password Reset

```csharp
// Step 1: User requests password reset
var user = await _userManager.FindByEmailAsync(email);
var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

// Send email with reset link containing token
await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken);

// Step 2: User clicks link and submits new password
var result = await _userManager.ResetPasswordAsync(
    user, 
    resetToken, 
    newPassword
);

if (result.Succeeded)
{
    // Password reset successful
}
```

---

## 🐛 Troubleshooting

### Common Issues

#### 1. **Database Connection Fails**

**Symptoms**: Application crashes on startup with SQL error

**Solutions**:
- Check connection string in `appsettings.json`
- Verify database server is running
- Ensure database exists
- Check user permissions

#### 2. **Token Validation Fails**

**Symptoms**: API returns 401 Unauthorized

**Possible Causes**:
- Token expired
- Invalid signature
- Wrong audience or issuer
- Clock skew between servers

**Solutions**:
- Check token expiration time
- Verify signing key matches
- Ensure issuer URL is correct
- Synchronize server clocks

#### 3. **Login Fails with Correct Credentials**

**Symptoms**: Valid credentials rejected

**Check**:
- Is email confirmed? (`RequireConfirmedEmail`)
- Is phone confirmed? (`RequireConfirmedPhoneNumber`)
- Is account locked? (check `AccessFailedCount`)
- Is 2FA required but not provided?

#### 4. **CORS Errors in Browser**

**Symptoms**: "Access-Control-Allow-Origin" error

**Solution**:
```csharp
// In Startup.cs or Program.cs
services.AddCors(options =>
{
    options.AddPolicy("AllowClient", builder =>
    {
        builder.WithOrigins("https://client.example.com")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

app.UseCors("AllowClient");
```

---

## 📚 Learning Path

### For Beginners

1. **Start with**: 
   - Understanding OAuth 2.0 basics
   - Reading `PROJECT_OVERVIEW.md`
   - Exploring the Demo apps

2. **Then move to**:
   - Understanding the Domain entities
   - Learning about ASP.NET Core Identity
   - Reviewing the configuration options

3. **Finally**:
   - Implementing custom features
   - Extending the framework
   - Performance optimization

### For Experienced Developers

1. **Review**:
   - Architecture patterns (Clean Architecture, Repository, UoW)
   - OAuth 2.0 and OIDC flows
   - Security best practices

2. **Customize**:
   - Add custom claims
   - Implement custom grant types
   - Extend validation logic

3. **Optimize**:
   - Implement caching
   - Tune database queries
   - Monitor performance

---

## 🎓 Further Reading

### OAuth 2.0 & OIDC
- [OAuth 2.0 RFC](https://tools.ietf.org/html/rfc6749)
- [OpenID Connect Specification](https://openid.net/specs/openid-connect-core-1_0.html)
- [OAuth 2.0 Security Best Practices](https://tools.ietf.org/html/draft-ietf-oauth-security-topics)

### ASP.NET Core Identity
- [Official Documentation](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [Identity Model](https://identitymodel.readthedocs.io/)

### Clean Architecture
- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)

---

## ✅ Checklist for New Developers

- [ ] Clone the repository
- [ ] Read `PROJECT_OVERVIEW.md`
- [ ] Read `QUICK_REFERENCE.md`
- [ ] Read this Getting Started guide
- [ ] Set up development environment
- [ ] Configure database
- [ ] Run migrations
- [ ] Run demo applications
- [ ] Test OAuth flow
- [ ] Review Domain entities
- [ ] Understand configuration system
- [ ] Explore repository pattern
- [ ] Read OAuth 2.0 spec
- [ ] Read OIDC spec
- [ ] Ready to contribute!

---

*Happy Coding! 🚀*
