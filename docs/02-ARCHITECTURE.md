# HCL.CS.SF Architecture

**Document ID:** HCL.CS.SF-DOC-02-ARCHITECTURE  
**Version:** 1.0.0  
**Classification:** Internal Use  
**Last Updated:** 2026-03-01  

---

## Table of Contents

1. [C4 Context Diagram](#1-c4-context-diagram)
2. [C4 Container Diagram](#2-c4-container-diagram)
3. [Identity Service Architecture](#3-identity-service-architecture)
4. [Gateway Architecture](#4-gateway-architecture)
5. [Installer Architecture](#5-installer-architecture)
6. [Communication Patterns](#6-communication-patterns)
7. [Deployment Shapes](#7-deployment-shapes)

---

## 1. C4 Context Diagram

### 1.1 System Context

```mermaid
C4Context
    title HCL.CS.SF Identity Platform - System Context
    
    Person(user, "End User", "Person accessing client applications")
    Person(admin, "System Administrator", "Manages identity platform")
    
    System_Boundary(HCL.CS.SF_boundary, "HCL.CS.SF Identity Platform") {
        System(HCL.CS.SF, "HCL.CS.SF IdP", "OAuth 2.0 / OpenID Connect Identity Provider")
    }
    
    System_Ext(web_app, "Web Application", "Customer portal, admin dashboard")
    System_Ext(spa, "Single Page Application", "React, Angular, Vue apps")
    System_Ext(mobile, "Mobile Application", "iOS, Android apps")
    System_Ext(api, "API / Resource Server", "Protected REST APIs")
    System_Ext(ldap, "LDAP / Active Directory", "Enterprise directory service")
    System_Ext(smtp, "SMTP Server", "Email notification service")
    
    Rel(user, web_app, "Uses", "HTTPS")
    Rel(user, spa, "Uses", "HTTPS")
    Rel(user, mobile, "Uses", "HTTPS")
    
    Rel(web_app, HCL.CS.SF, "Authenticates users, requests tokens", "OAuth 2.0 / OIDC")
    Rel(spa, HCL.CS.SF, "Authenticates users (PKCE)", "OAuth 2.0 / OIDC")
    Rel(mobile, HCL.CS.SF, "Authenticates users (PKCE)", "OAuth 2.0 / OIDC")
    
    Rel(api, HCL.CS.SF, "Validates access tokens", "JWKS / Introspection")
    
    Rel(HCL.CS.SF, ldap, "Authenticates users", "LDAP/LDAPS")
    Rel(HCL.CS.SF, smtp, "Sends notifications", "SMTP")
    
    Rel(admin, HCL.CS.SF, "Manages system", "Admin API")
    
    UpdateLayoutConfig($c4ShapeInRow="3", $c4BoundaryInRow="1")
```

### 1.2 External Actors

| Actor | Description | Integration Protocol |
|-------|-------------|---------------------|
| End User | Person authenticating to applications | Browser/HTTP |
| System Administrator | Person managing identity configuration | HTTPS/Admin API |
| Web Application | Server-rendered applications | OAuth 2.0 Authorization Code |
| SPA | Browser-based JavaScript applications | OAuth 2.0 + PKCE |
| Mobile App | Native mobile applications | OAuth 2.0 + PKCE |
| API / Resource Server | Protected resource APIs | Token validation |
| LDAP/AD | Enterprise directory | LDAP/LDAPS |
| SMTP Server | Email delivery | SMTP/SMTPS |

---

## 2. C4 Container Diagram

### 2.1 Container Overview

```mermaid
C4Container
    title HCL.CS.SF Identity Platform - Container Diagram
    
    Person(user, "End User", "Person accessing applications")
    
    Container_Boundary(gateway_boundary, "Gateway Layer") {
        Container(gateway, "API Gateway", "ASP.NET Core", "Reverse proxy, routing, security headers, observability")
    }
    
    Container_Boundary(identity_boundary, "Identity Service") {
        Container(api_layer, "API Layer", "ASP.NET Core", "HTTP endpoints, routing, middleware")
        Container(endpoint_layer, "Endpoint Layer", "C#", "OAuth/OIDC endpoint implementations")
        Container(service_layer, "Application Services", "C#", "Business logic, validation, orchestration")
        Container(domain_layer, "Domain Layer", "C#", "Entities, value objects, domain logic")
    }
    
    Container_Boundary(infra_boundary, "Infrastructure") {
        Container(persistence, "Persistence", "EF Core", "Data access, repositories, migrations")
        Container(infrastructure, "Infrastructure Services", "C#", "Email, SMS, logging, key storage")
    }
    
    ContainerDb(database, "Database", "SQL Server/MySQL/PostgreSQL/SQLite", "User data, tokens, audit logs")
    ContainerDb(cache, "Distributed Cache", "Redis/In-Memory", "Session storage, temporary data")
    
    Container(installer, "Installer MVC", "ASP.NET Core MVC", "Database provisioning and seeding")
    
    Rel(user, gateway, "Makes requests to", "HTTPS")
    Rel(gateway, api_layer, "Routes to", "HTTP")
    
    Rel(api_layer, endpoint_layer, "Invokes", "In-process")
    Rel(endpoint_layer, service_layer, "Uses", "In-process")
    Rel(service_layer, domain_layer, "Uses", "In-process")
    
    Rel(service_layer, persistence, "Persists via", "In-process")
    Rel(persistence, database, "Reads/Writes", "ADO.NET/EF Core")
    
    Rel(service_layer, infrastructure, "Uses", "In-process")
    Rel(infrastructure, cache, "Caches to", "Redis protocol")
    
    Rel(installer, database, "Provisions", "SQL")
```

### 2.2 Container Responsibilities

| Container | Technology | Primary Responsibility | Source Location |
|-----------|------------|----------------------|-----------------|
| **API Gateway** | ASP.NET Core | Request routing, security headers, correlation IDs, observability | `/src/Gateway/HCL.CS.SF.Gateway/` |
| **Identity API** | ASP.NET Core | HTTP hosting, endpoint mapping, middleware pipeline | `/src/Identity/HCL.CS.SF.Identity.API/` |
| **Endpoint Layer** | C# / .NET 8 | OAuth/OIDC protocol implementations | `/src/Identity/HCL.CS.SF.Identity.Application/Implementation/Endpoint/` |
| **Application Services** | C# / .NET 8 | Business logic, validation, specifications | `/src/Identity/HCL.CS.SF.Identity.Application/Implementation/Api/` |
| **Domain Layer** | C# / .NET 8 | Entities, models, constants, enums | `/src/Identity/HCL.CS.SF.Identity.Domain/` |
| **Persistence** | EF Core 8 | Data access, repositories, mappings | `/src/Identity/HCL.CS.SF.Identity.Persistence/` |
| **Infrastructure** | C# / .NET 8 | External service integrations | `/src/Identity/HCL.CS.SF.Identity.Infrastructure/` |
| **Installer** | ASP.NET Core MVC | Database setup, migrations, seeding | `/installer/HCL.CS.SF.Installer.Mvc/` |

---

## 3. Identity Service Architecture

### 3.1 Layered Architecture

```mermaid
flowchart TB
    subgraph "Presentation Layer"
        API[Identity API<br/>HCL.CS.SF.Identity.API]
    end
    
    subgraph "Application Layer"
        ENDPOINTS[Endpoint Implementations<br/>Authorize, Token, UserInfo, etc.]
        SERVICES[Application Services<br/>UserAccount, Role, Client, etc.]
        SPEC[Specifications<br/>Validation Rules]
    end
    
    subgraph "Domain Layer"
        ENTITIES[Domain Entities<br/>Users, Clients, Tokens]
        MODELS[Domain Models<br/>Request/Response DTOs]
        CONSTANTS[Constants<br/>OpenIdConstants]
    end
    
    subgraph "Domain Services Layer"
        REPO_INTERFACES[Repository Interfaces<br/>IUserRepository, etc.]
        UOW_INTERFACES[Unit of Work Interfaces]
        INFRA_INTERFACES[Infrastructure Interfaces<br/>IEmailService, etc.]
    end
    
    subgraph "Infrastructure Layer"
        REPO_IMPL[Repository Implementations]
        UOW_IMPL[Unit of Work Implementations]
        INFRA_IMPL[Infrastructure Implementations<br/>EmailService, SmsService]
        EF[Entity Framework<br/>DbContext, Migrations]
    end
    
    subgraph "Data Stores"
        DB[(Database)]
        CACHE[(Cache)]
    end
    
    API --> ENDPOINTS
    ENDPOINTS --> SERVICES
    ENDPOINTS --> SPEC
    SERVICES --> ENTITIES
    SERVICES --> MODELS
    SERVICES --> REPO_INTERFACES
    SERVICES --> INFRA_INTERFACES
    
    REPO_INTERFACES --> REPO_IMPL
    INFRA_INTERFACES --> INFRA_IMPL
    
    REPO_IMPL --> EF
    EF --> DB
    INFRA_IMPL --> CACHE
```

### 3.2 Layer Details

#### 3.2.1 API Layer (`HCL.CS.SF.Identity.API`)

**Source:** `/src/Identity/HCL.CS.SF.Identity.API/`

| Component | Purpose | Key Files |
|-----------|---------|-----------|
| Program.cs | Application entry point, DI configuration | `Program.cs` |
| Extensions | Custom middleware registration, HCL.CS.SF builder | `Extensions/HCL.CS.SFBuilder.cs`, `Extensions/HCL.CS.SFExtension.cs` |
| Health Checks | Dependency health verification | `Health/DatabaseDependencyHealthCheck.cs`, `Health/CacheDependencyHealthCheck.cs` |

#### 3.2.2 Application Layer (`HCL.CS.SF.Identity.Application`)

**Source:** `/src/Identity/HCL.CS.SF.Identity.Application/`

| Sub-Component | Purpose | Key Files |
|---------------|---------|-----------|
| **Endpoints** | OAuth/OIDC protocol handlers | `Implementation/Endpoint/*Endpoint.cs` |
| **Services** | API business logic | `Implementation/Api/Services/*Service.cs` |
| **Specifications** | Validation rule definitions | `Implementation/Endpoint/Specifications/`, `Implementation/Api/Specifications/` |
| **Validators** | Request validation | `Implementation/Endpoint/Validators/`, `Implementation/Api/Validators/` |
| **Results** | HTTP result generation | `Implementation/Endpoint/Results/*Result.cs` |

#### 3.2.3 Domain Layer (`HCL.CS.SF.Identity.Domain`)

**Source:** `/src/Identity/HCL.CS.SF.Identity.Domain/`

| Sub-Component | Purpose | Key Files |
|---------------|---------|-----------|
| **Entities** | Database entity definitions | `Entities/Api/*.cs`, `Entities/Endpoint/*.cs` |
| **Models** | Request/response DTOs | `Models/Api/*.cs`, `Models/Endpoint/*.cs` |
| **Configurations** | System settings | `Configurations/Api/`, `Configurations/Endpoint/` |
| **Constants** | Application constants | `Constants/`, `Constants/Endpoint/OpenIdConstants.cs` |
| **Enums** | Enumeration types | `Enums/`, `Enums/ApiEnums.cs`, `Enums/EndpointEnums.cs` |

#### 3.2.4 Domain Services Layer (`HCL.CS.SF.Identity.DomainServices`)

**Source:** `/src/Identity/HCL.CS.SF.Identity.DomainServices/`

| Sub-Component | Purpose | Key Files |
|---------------|---------|-----------|
| **Repository Interfaces** | Data access contracts | `Repository/Api/*.cs`, `Repository/Endpoint/*.cs` |
| **Unit of Work** | Transaction boundaries | `UnitOfWork/Api/*.cs`, `UnitOfWork/Endpoint/*.cs` |
| **Infrastructure Interfaces** | External service contracts | `Infra/*.cs` |

#### 3.2.5 Infrastructure Layer

**Source:** `/src/Identity/HCL.CS.SF.Identity.Infrastructure*/`

| Sub-Component | Purpose | Key Files |
|---------------|---------|-----------|
| **Services** | External integrations | `Implementation/EmailService.cs`, `Implementation/SmsService.cs` |
| **Resources** | Localization, key storage | `KeyStore.cs`, `ResourceStringHandler.cs` |

#### 3.2.6 Persistence Layer (`HCL.CS.SF.Identity.Persistence`)

**Source:** `/src/Identity/HCL.CS.SF.Identity.Persistence/`

| Sub-Component | Purpose | Key Files |
|---------------|---------|-----------|
| **DbContext** | EF Core database context | `ApplicationDbContext.cs` |
| **Repositories** | Repository implementations | `Repository/Api/*.cs` |
| **Unit of Work** | Transaction implementation | `UnitOfWork/Api/*.cs` |
| **Mappers** | Entity framework mappings | `Mapper/Api/*.cs`, `Mapper/Endpoint/*.cs` |

### 3.3 Clean Architecture Dependency Rules

```mermaid
flowchart LR
    subgraph "Dependency Direction"
        direction TB
        API[API Layer] --> APP[Application Layer]
        APP --> DOM[Domain Layer]
        DOM --> DOM_SERV[Domain Services]
        DOM_SERV --> INFRA[Infrastructure]
        INFRA --> DB[(Database)]
    end
    
    style API fill:#e1f5fe
    style APP fill:#fff3e0
    style DOM fill:#e8f5e9
    style DOM_SERV fill:#fce4ec
    style INFRA fill:#f3e5f5
```

**Key Principles:**
1. **Domain Layer** has no external dependencies
2. **Application Layer** depends only on Domain and Domain Services
3. **Infrastructure** implements interfaces defined in Domain Services
4. **API Layer** wires everything together via Dependency Injection

---

## 4. Gateway Architecture

### 4.1 Gateway Component Diagram

```mermaid
flowchart LR
    subgraph "Client Requests"
        CLIENT[Client Application]
    end
    
    subgraph "Gateway (HCL.CS.SF.Gateway)"
        direction TB
        CORR[CorrelationIdMiddleware<br/>X-Correlation-ID propagation]
        SEC[SecurityHeadersMiddleware<br/>HSTS, CSP, X-Frame-Options]
        OBS[RequestObservabilityMiddleware<br/>Metrics, logging]
        API[HCL.CS.SFApiMiddleware<br/>Request processing]
        ENDPOINT[HCL.CS.SFEndpointMiddleware<br/>Endpoint routing]
        
        CORR --> SEC
        SEC --> OBS
        OBS --> API
        API --> ENDPOINT
    end
    
    subgraph "Proxy Services"
        direction TB
        AUTH_PROXY[AuthenticationProxyService]
        USER_PROXY[UserAccountProxyService]
        CLIENT_PROXY[ClientProxyService]
        ROLE_PROXY[RoleProxyService]
        RESOURCE_PROXY[ApiResourceProxyService]
        TOKEN_PROXY[SecurityTokenProxyService]
        AUDIT_PROXY[AuditTrailProxyService]
    end
    
    subgraph "Identity Service"
        IDENTITY[Identity API]
    end
    
    CLIENT --> CORR
    ENDPOINT --> AUTH_PROXY
    ENDPOINT --> USER_PROXY
    ENDPOINT --> CLIENT_PROXY
    ENDPOINT --> ROLE_PROXY
    ENDPOINT --> RESOURCE_PROXY
    ENDPOINT --> TOKEN_PROXY
    ENDPOINT --> AUDIT_PROXY
    
    AUTH_PROXY --> IDENTITY
    USER_PROXY --> IDENTITY
    CLIENT_PROXY --> IDENTITY
    ROLE_PROXY --> IDENTITY
    RESOURCE_PROXY --> IDENTITY
    TOKEN_PROXY --> IDENTITY
    AUDIT_PROXY --> IDENTITY
```

### 4.2 Gateway Middleware Pipeline

**Source:** `/src/Gateway/HCL.CS.SF.Gateway/Hosting/`

| Middleware | Purpose | Order |
|------------|---------|-------|
| **CorrelationIdMiddleware** | Generate/propagate correlation IDs | 1 (First) |
| **SecurityHeadersMiddleware** | Add security response headers | 2 |
| **RequestObservabilityMiddleware** | Metrics collection, request logging | 3 |
| **HCL.CS.SFApiMiddleware** | API request processing | 4 |
| **HCL.CS.SFEndpointMiddleware** | Endpoint-specific routing | 5 (Last) |

### 4.3 Proxy Services

**Source:** `/src/Gateway/HCL.CS.SF.Gateway/Proxy/`

| Service | Responsibility | Routes |
|---------|---------------|--------|
| `AuthenticationProxyService` | Authentication flows | `/api/auth/*` |
| `UserAccountProxyService` | User management | `/api/users/*` |
| `ClientProxyService` | OAuth client management | `/api/clients/*` |
| `RoleProxyService` | Role management | `/api/roles/*` |
| `ApiResourceProxyService` | API resource management | `/api/resources/*` |
| `SecurityTokenProxyService` | Token operations | `/api/tokens/*` |
| `AuditTrailProxyService` | Audit log queries | `/api/audit/*` |
| `IdentityResourceProxyService` | Identity resource management | `/api/identity-resources/*` |

### 4.4 Security Headers Applied

**Source:** `/src/Gateway/HCL.CS.SF.Gateway/Hosting/SecurityHeadersMiddleware.cs`

| Header | Value | Purpose |
|--------|-------|---------|
| X-Frame-Options | DENY | Clickjacking protection |
| X-Content-Type-Options | nosniff | MIME sniffing protection |
| Referrer-Policy | strict-origin-when-cross-origin | Referrer control |
| Strict-Transport-Security | max-age=31536000; includeSubDomains | HSTS enforcement |
| Permissions-Policy | camera=(), microphone=(), geolocation=(), payment=() | Feature policy |
| Server | (removed) | Information disclosure reduction |

---

## 5. Installer Architecture

### 5.1 Installer Workflow

```mermaid
flowchart TB
    subgraph "Installation Workflow"
        START([Start]) --> CHECK{Installation<br/>Completed?}
        CHECK -->|Yes| REDIRECT[Redirect to<br/>Completion Page]
        CHECK -->|No| PROVIDER[Select Database<br/>Provider]
        
        PROVIDER --> SQLSERVER[SQL Server]
        PROVIDER --> MYSQL[MySQL]
        PROVIDER --> POSTGRES[PostgreSQL]
        PROVIDER --> SQLITE[SQLite]
        
        SQLSERVER --> CONNECTION[Configure<br/>Connection String]
        MYSQL --> CONNECTION
        POSTGRES --> CONNECTION
        SQLITE --> CONNECTION
        
        CONNECTION --> VALIDATE{Validate<br/>Connection}
        VALIDATE -->|Failed| CONNECTION
        VALIDATE -->|Success| MIGRATE[Execute<br/>Migrations]
        
        MIGRATE --> SEED[Seed<br/>Master Data]
        SEED --> ADMIN[Create<br/>Admin User]
        ADMIN --> CLIENT[Create<br/>Initial Client]
        CLIENT --> COMPLETE[Mark<br/>Complete]
        COMPLETE --> END([End])
        REDIRECT --> END
    end
```

### 5.2 Installer Components

**Source:** `/installer/HCL.CS.SF.Installer.Mvc/`

| Component | Purpose | Key Files |
|-----------|---------|-----------|
| **SetupController** | MVC controller for installation wizard | `Controllers/SetupController.cs` |
| **InstallerService** | Orchestrates installation workflow | `Application/Services/InstallerService.cs` |
| **DatabaseProvisioner** | Provider-specific provisioning | `Infrastructure/Services/DatabaseProvisioning/*Provisioner.cs` |
| **DatabaseMigrationService** | EF Core migration execution | `Infrastructure/Services/DatabaseMigrationService.cs` |
| **SeedDataService** | Master data seeding | `Infrastructure/Services/SeedDataService.cs` |
| **SetupRedirectMiddleware** | Blocks access until installed | `Middleware/SetupRedirectMiddleware.cs` |
| **InstallationGateService** | Prevents re-installation | `Infrastructure/Services/InstallationGateService.cs` |

### 5.3 Provider Provisioners

| Provider | Provisioner Class | Responsibilities |
|----------|------------------|------------------|
| SQL Server | `SqlServerProvisioner` | Create database, validate connection |
| MySQL | `MySqlProvisioner` | Create database, validate connection |
| PostgreSQL | `PostgreSqlProvisioner` | Create database, validate connection |
| SQLite | `SqliteProvisioner` | Ensure directory exists, validate path |

---

## 6. Communication Patterns

### 6.1 Gateway to Identity Service

```mermaid
sequenceDiagram
    participant Client
    participant Gateway
    participant Identity
    participant Database
    
    Client->>Gateway: HTTP Request<br/>+ X-Correlation-ID
    Gateway->>Gateway: Generate/Validate Correlation ID
    Gateway->>Gateway: Add Security Headers
    Gateway->>Gateway: Start Metrics Timer
    
    Gateway->>Identity: Proxy Request
    Identity->>Identity: Validate Token (if required)
    Identity->>Database: Query/Update Data
    Database-->>Identity: Results
    Identity->>Identity: Generate Response
    Identity-->>Gateway: HTTP Response
    
    Gateway->>Gateway: Record Metrics<br/>(duration, status)
    Gateway->>Gateway: Add Correlation ID to Response
    Gateway-->>Client: HTTP Response<br/>+ X-Correlation-ID
```

### 6.2 OAuth Authorization Code Flow

```mermaid
sequenceDiagram
    participant User
    participant Client
    participant Gateway
    participant Identity
    participant Database
    
    User->>Client: Click Login
    Client->>Client: Generate PKCE<br/>code_verifier + code_challenge
    Client->>Gateway: GET /security/authorize<br/>?client_id=...&code_challenge=...
    Gateway->>Identity: Forward Request
    Identity->>Identity: Validate Client + PKCE
    Identity->>Database: Store Authorization Code<br/>(with code_challenge)
    Database-->>Identity: Code Stored
    Identity-->>Gateway: Redirect to Login
    Gateway-->>Client: Redirect to Login
    
    Client->>User: Display Login Page
    User->>Client: Enter Credentials
    Client->>Gateway: POST Credentials
    Gateway->>Identity: Forward
    Identity->>Database: Validate User
    Database-->>Identity: User Valid
    Identity->>Database: Consume Authorization Code
    Database-->>Identity: Code Details
    Identity-->>Gateway: Redirect with Code
    Gateway-->>Client: Redirect to callback<br/>?code=AUTH_CODE
    
    Client->>Gateway: POST /security/token<br/>grant_type=authorization_code<br/>&code=...&code_verifier=...
    Gateway->>Identity: Forward
    Identity->>Identity: Validate Code + PKCE
    Identity->>Database: Consume Code<br/>Store Tokens
    Database-->>Identity: Success
    Identity-->>Gateway: Tokens<br/>(access_token, id_token, refresh_token)
    Gateway-->>Client: Token Response
```

### 6.3 Inter-Layer Communication

| Pattern | Usage | Example |
|---------|-------|---------|
| **Dependency Injection** | Cross-layer service resolution | `IServiceCollection` registration |
| **Repository Pattern** | Data access abstraction | `IUserRepository` → `UserRepository` |
| **Unit of Work** | Transaction boundaries | `IUserManagementUnitOfWork.SaveChangesAsync()` |
| **Specification Pattern** | Validation rules | `AuthorizeRequestSpecification` |
| **Result Pattern** | Operation outcomes | `FrameworkResult<T>` |

---

## 7. Deployment Shapes

### 7.1 Local Development

```mermaid
flowchart TB
    subgraph "Local Development Environment"
        DEMO[Demo Server<br/>https://localhost:5001]
        CLIENT[Demo MVC Client<br/>https://localhost:5003]
        RESOURCE[Resource API<br/>https://localhost:5002]
        INSTALLER[Installer MVC<br/>https://localhost:7039]
        
        SQLITE[(SQLite<br/>.data/HCL.CS.SF_identity.db)]
    end
    
    DEMO --> SQLITE
    INSTALLER --> SQLITE
    CLIENT --> DEMO
    RESOURCE --> DEMO
```

**Deployment Command:**
```bash
./scripts/run-local-demo.sh --client-id "..." --client-secret "..."
```

### 7.2 Docker Compose

```mermaid
flowchart TB
    subgraph "Docker Compose Stack"
        GATEWAY[Gateway Container<br/>Port 8080]
        IDENTITY[Identity API Container<br/>Port 8080]
        ADMIN[Admin API Container<br/>Port 8080]
        
        POSTGRES[(PostgreSQL<br/>Port 5432)]
        REDIS[(Redis Cache)]
    end
    
    GATEWAY --> IDENTITY
    GATEWAY --> ADMIN
    IDENTITY --> POSTGRES
    IDENTITY --> REDIS
    ADMIN --> POSTGRES
```

**Configuration:**
- Compose file: `/docker/docker-compose.yml`
- Identity Dockerfile: `/docker/identity-api.Dockerfile`
- Admin Dockerfile: `/docker/admin-api.Dockerfile`
- Gateway Dockerfile: `/docker/gateway.Dockerfile`

### 7.3 Kubernetes

```mermaid
flowchart TB
    subgraph "Kubernetes Cluster"
        subgraph "Ingress Layer"
            INGRESS[Nginx Ingress<br/>HCL.CS.SF.example.com]
        end
        
        subgraph "Service Layer"
            SVC_ID[Identity Service<br/>ClusterIP: 8080]
        end
        
        subgraph "Pod Layer"
            ID_POD1[Identity Pod 1]
            ID_POD2[Identity Pod 2]
        end
        
        subgraph "Config"
            CM[ConfigMap<br/>HCL.CS.SF-config]
        end
        
        subgraph "Data Stores"
            EXT_DB[(External Database)]
            EXT_CACHE[(External Cache)]
        end
    end
    
    INGRESS --> SVC_ID
    SVC_ID --> ID_POD1
    SVC_ID --> ID_POD2
    
    ID_POD1 --> CM
    ID_POD2 --> CM
    
    ID_POD1 --> EXT_DB
    ID_POD1 --> EXT_CACHE
    ID_POD2 --> EXT_DB
    ID_POD2 --> EXT_CACHE
```

**Manifests:**
- ConfigMap: `/k8s/configmap.yaml`
- Deployment: `/k8s/identity-deployment.yaml`
- Service: `/k8s/identity-service.yaml`
- Ingress: `/k8s/ingress.yaml`

**Deployment Commands:**
```bash
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/identity-deployment.yaml
kubectl apply -f k8s/identity-service.yaml
kubectl apply -f k8s/ingress.yaml
```

### 7.4 Deployment Comparison

| Aspect | Local | Docker Compose | Kubernetes |
|--------|-------|----------------|------------|
| **Use Case** | Development | Testing / Small prod | Production |
| **Database** | SQLite (default) | Containerized PostgreSQL | External managed DB |
| **Cache** | In-memory | Containerized Redis | External managed Redis |
| **Scaling** | Single instance | Single instance | Horizontal pod autoscaling |
| **SSL** | Development certs | Development certs | Ingress TLS termination |
| **Monitoring** | Console logs | Container logs | Prometheus/Grafana integration |

---

## 8. Key Architectural Decisions

### 8.1 ADR Summary

| Decision | Rationale | Trade-off |
|----------|-----------|-----------|
| **Clean Architecture** | Separation of concerns, testability | More boilerplate, learning curve |
| **Custom OAuth Implementation** | Full control over security, no external dependencies | Development effort, maintenance burden |
| **EF Core with Multiple Providers** | Flexibility for different deployment scenarios | Provider-specific code needed |
| **Gateway as Library** | Can be embedded or standalone | Less separation than separate process |
| **PKCE Mandatory** | Security best practice for all clients | Breaks legacy clients without PKCE |
| **Implicit Flow Disabled** | Security (token exposure in URL) | Requires PKCE implementation in clients |

### 8.2 Source File Inventory

This architecture is grounded in the following source files:

| Layer | Path |
|-------|------|
| API | `/src/Identity/HCL.CS.SF.Identity.API/Program.cs` |
| Application | `/src/Identity/HCL.CS.SF.Identity.Application/Implementation/` |
| Domain | `/src/Identity/HCL.CS.SF.Identity.Domain/Entities/`, `/src/Identity/HCL.CS.SF.Identity.Domain/Models/` |
| Domain Services | `/src/Identity/HCL.CS.SF.Identity.DomainServices/` |
| Infrastructure | `/src/Identity/HCL.CS.SF.Identity.Infrastructure/`, `/src/Identity/HCL.CS.SF.Identity.Infrastructure.Resources/` |
| Persistence | `/src/Identity/HCL.CS.SF.Identity.Persistence/` |
| Gateway | `/src/Gateway/HCL.CS.SF.Gateway/` |
| Installer | `/installer/HCL.CS.SF.Installer.Mvc/` |

---

## Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2026-03-01 | Enterprise Documentation Team | Initial release |
