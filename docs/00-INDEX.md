# HCL.CS.SF Enterprise Documentation Index

**Document ID:** HCL.CS.SF-DOC-00-INDEX  
**Version:** 1.0.0  
**Classification:** Internal Use  
**Last Updated:** 2026-03-01  

---

## 1. Documentation Map

This repository contains enterprise-grade documentation for the HCL.CS.SF Identity Platform (Legacy Implementation). The documentation is organized as a numbered series for easy navigation and reference.

| Doc ID | Document | Purpose | Primary Audience |
|--------|----------|---------|------------------|
| 00 | **INDEX** (this doc) | Documentation navigation and versioning | All readers |
| 01 | [SYSTEM-OVERVIEW](./01-SYSTEM-OVERVIEW.md) | What HCL.CS.SF is, system boundaries, supported flows | Architects, Stakeholders |
| 02 | [ARCHITECTURE](./02-ARCHITECTURE.md) | C4 diagrams, component architecture, deployment shapes | Architects, Developers |
| 03 | [SECURITY-ARCHITECTURE](./03-SECURITY-ARCHITECTURE.md) | Auth flows, JWT lifecycle, OWASP mapping, threat model | Security Engineers, Auditors |
| 04 | [API-REFERENCE](./04-API-REFERENCE.md) | Endpoint specifications, request/response contracts | API Consumers, Integrators |
| 05 | [DATABASE](./05-DATABASE.md) | Schema design, entity relationships, migrations | DBAs, Developers |
| 06 | [OBSERVABILITY](./06-OBSERVABILITY.md) | Correlation IDs, metrics, health checks, redaction | DevOps, SREs |
| 07 | [DEPLOYMENT](./07-DEPLOYMENT.md) | Local, Docker, K8s deployment procedures | DevOps, Platform Engineers |
| 08 | [RUNBOOKS](./08-RUNBOOKS.md) | Operational tasks, incident response, troubleshooting | Operators, On-Call Engineers |

---

## 2. Audience Map

### 2.1 Security Auditors
**Start here:** [03-SECURITY-ARCHITECTURE](./03-SECURITY-ARCHITECTURE.md)

Key sections:
- Section 4: OAuth/OIDC Flow Security
- Section 6: JWT Security Model
- Section 8: OWASP Top 10 Mitigations
- Section 9: Threat Model Summary

Supporting documents:
- [THREAT_MODEL.md](./THREAT_MODEL.md) - High-level threat categories
- [OAUTH_OIDC_SECURITY_WHITEPAPER.md](./OAUTH_OIDC_SECURITY_WHITEPAPER.md) - Detailed security controls

### 2.2 Solution Architects
**Start here:** [02-ARCHITECTURE](./02-ARCHITECTURE.md)

Key sections:
- Section 2: C4 Context and Container Diagrams
- Section 3: Identity Service Layer Architecture
- Section 6: Deployment Shapes

Supporting documents:
- [ARCHITECTURE.md](./ARCHITECTURE.md) - Runtime topology summary
- [ARCHITECTURE_MAP.md](./ARCHITECTURE_MAP.md) - Layered boundaries

### 2.3 API Integrators
**Start here:** [04-API-REFERENCE](./04-API-REFERENCE.md)

Key sections:
- Section 2: Discovery Endpoint
- Section 3: Authorize Endpoint
- Section 4: Token Endpoint
- Section 10: Example curl Requests

Supporting documents:
- [HCL.CS.SF_USAGE_INTEGRATION_MANUAL.md](./HCL.CS.SF_USAGE_INTEGRATION_MANUAL.md) - Integration patterns
- [API_COLLECTION.json](./API_COLLECTION.json) - Postman/HTTP client collection

### 2.4 DevOps / Platform Engineers
**Start here:** [07-DEPLOYMENT](./07-DEPLOYMENT.md)

Key sections:
- Section 3: Docker Compose Deployment
- Section 4: Kubernetes Deployment
- Section 5: Environment Configuration

Supporting documents:
- [DEPLOYMENT.md](./DEPLOYMENT.md) - Quick deployment summary
- [RAILWAY_DEPLOYMENT.md](./RAILWAY_DEPLOYMENT.md) - Railway service-by-service deployment guide
- [RUNBOOK.md](./RUNBOOK.md) - Production runbook

### 2.5 Database Administrators
**Start here:** [05-DATABASE](./05-DATABASE.md)

Key sections:
- Section 2: Supported Database Engines
- Section 4: Key Tables and Relationships
- Section 6: Migration Strategy

### 2.6 Site Reliability Engineers
**Start here:** [06-OBSERVABILITY](./06-OBSERVABILITY.md) → [08-RUNBOOKS](./08-RUNBOOKS.md)

Key sections:
- Section 2: Correlation ID Middleware
- Section 3: Metrics and Instrumentation
- Section 5: Health Check Endpoints
- Runbook Section 3: Incident Response Checklist

---

## 3. Versioning Rules

### 3.1 Document Versioning

Each document follows semantic versioning (MAJOR.MINOR.PATCH):

| Version Change | When to Apply |
|----------------|---------------|
| **MAJOR** | Breaking changes to documented behavior; new security requirements; removed features |
| **MINOR** | New features added; expanded sections; additional examples |
| **PATCH** | Typos, clarifications, formatting fixes; non-behavioral corrections |

### 3.2 Source Code Correlation

| Document | Primary Source Files |
|----------|---------------------|
| 01-SYSTEM-OVERVIEW | `/src/Identity/*`, `/src/Gateway/*`, `/installer/*` |
| 02-ARCHITECTURE | `/src/Identity/*/`, `/src/Gateway/HCL.CS.SF.Gateway/` |
| 03-SECURITY-ARCHITECTURE | `/src/Identity/HCL.CS.SF.Identity.Application/Implementation/Endpoint/` |
| 04-API-REFERENCE | `/src/Identity/HCL.CS.SF.Identity.Application/Implementation/Endpoint/*Endpoint.cs` |
| 05-DATABASE | `/src/Identity/HCL.CS.SF.Identity.Domain/Entities/`, `/src/Identity/HCL.CS.SF.Identity.Persistence/Mapper/` |
| 06-OBSERVABILITY | `/src/Gateway/HCL.CS.SF.Gateway/Hosting/`, `/src/Identity/HCL.CS.SF.Identity.API/Health/` |
| 07-DEPLOYMENT | `/docker/`, `/k8s/`, `/scripts/` |
| 08-RUNBOOKS | `/RUNBOOK.md`, `/scripts/` |

### 3.3 Change Log Format

Each document includes a Version History section at the bottom:

```markdown
## Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2026-03-01 | Documentation Team | Initial release |
```

---

## 4. Cross-Reference Guide

### 4.1 Security Documentation Chain

```
THREAT_MODEL.md
    ↓
OAUTH_OIDC_SECURITY_WHITEPAPER.md (detailed controls)
    ↓
03-SECURITY-ARCHITECTURE.md (implementation mapping)
    ↓
04-API-REFERENCE.md (endpoint security details)
```

### 4.2 Operational Documentation Chain

```
DEPLOYMENT.md (quick start)
    ↓
07-DEPLOYMENT.md (comprehensive)
    ↓
RUNBOOK.md (health/metrics overview)
    ↓
06-OBSERVABILITY.md (detailed instrumentation)
    ↓
08-RUNBOOKS.md (procedures and incidents)
```

### 4.3 Developer Documentation Chain

```
PROJECT_OVERVIEW.md (high-level)
    ↓
GETTING_STARTED.md (tutorial)
    ↓
01-SYSTEM-OVERVIEW.md (system boundaries)
    ↓
02-ARCHITECTURE.md (component design)
    ↓
05-DATABASE.md (data model)
```

---

## 5. Document Conventions

### 5.1 Terminology

| Term | Definition | Source |
|------|------------|--------|
| **Identity Service** | The core OAuth/OIDC token issuance and user management service | `/src/Identity/HCL.CS.SF.Identity.API/` |
| **Gateway** | Reverse proxy with routing, middleware, observability | `/src/Gateway/HCL.CS.SF.Gateway/` |
| **Installer** | MVC application for database provisioning and seeding | `/installer/HCL.CS.SF.Installer.Mvc/` |
| **Demo Clients** | Sample applications demonstrating integration | `/demos/` |
| **Endpoint** | OAuth/OIDC protocol endpoint (authorize, token, etc.) | `/src/Identity/HCL.CS.SF.Identity.Application/Implementation/Endpoint/` |

### 5.2 Diagram Notation

All diagrams use **Mermaid** syntax for version control compatibility:
- `C4Context` / `C4Container` - System and container diagrams
- `erDiagram` - Entity relationship diagrams
- `sequenceDiagram` - Flow and interaction diagrams
- `flowchart` - Process and decision flows

### 5.3 Code References

Code snippets reference actual file paths in the repository:
```
Source: /src/Identity/HCL.CS.SF.Identity.Domain/Entities/Api/Users.cs
```

---

## 6. Compliance Mapping

### 6.1 SOC 2 Type II

| SOC 2 Trust Service Criteria | Relevant Documents |
|------------------------------|-------------------|
| Security (CC6.1-CC6.7) | 03-SECURITY-ARCHITECTURE.md, 08-RUNBOOKS.md |
| Availability (A1.2) | 07-DEPLOYMENT.md, 06-OBSERVABILITY.md |
| Processing Integrity (PI1.3) | 04-API-REFERENCE.md, 05-DATABASE.md |
| Confidentiality (C1.1) | 03-SECURITY-ARCHITECTURE.md |

### 6.2 ISO 27001

| ISO 27001 Control | Relevant Documents |
|-------------------|-------------------|
| A.9.4 (System access control) | 03-SECURITY-ARCHITECTURE.md |
| A.12.3 (Backup) | 05-DATABASE.md |
| A.12.4 (Logging) | 06-OBSERVABILITY.md |
| A.16.1 (Incident management) | 08-RUNBOOKS.md |

---

## 7. External References

| Standard | Reference |
|----------|-----------|
| OAuth 2.0 | [RFC 6749](https://tools.ietf.org/html/rfc6749) |
| OpenID Connect Core | [OIDC Core 1.0](https://openid.net/specs/openid-connect-core-1_0.html) |
| PKCE | [RFC 7636](https://tools.ietf.org/html/rfc7636) |
| JWT | [RFC 7519](https://tools.ietf.org/html/rfc7519) |
| JWKS | [RFC 7517](https://tools.ietf.org/html/rfc7517) |
| Token Introspection | [RFC 7662](https://tools.ietf.org/html/rfc7662) |
| Token Revocation | [RFC 7009](https://tools.ietf.org/html/rfc7009) |

---

## 8. Contributing to Documentation

### 8.1 Update Triggers

Update documentation when:
1. New endpoints are added or modified
2. Database schema changes (entities, mappings, migrations)
3. Security controls are added or changed
4. Deployment procedures are modified
5. New environment variables or configuration options are introduced

### 8.2 Review Checklist

Before committing documentation changes:
- [ ] All code references point to existing files
- [ ] Mermaid diagrams render correctly
- [ ] Version history updated
- [ ] Cross-references validated
- [ ] No speculative features documented

---

## Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2026-03-01 | Enterprise Documentation Team | Initial documentation pack release |
