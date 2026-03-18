# HCL.CS.SF Operational Runbooks

**Document ID:** HCL.CS.SF-DOC-08-RUNBOOKS  
**Version:** 1.0.0  
**Classification:** Internal Use  
**Last Updated:** 2026-03-01  

---

## Table of Contents

1. [Common Operational Tasks](#1-common-operational-tasks)
2. [Incident Response Checklist](#2-incident-response-checklist)
3. [Safe Logging Rules](#3-safe-logging-rules)
4. [Emergency Procedures](#4-emergency-procedures)

---

## 1. Common Operational Tasks

### 1.1 Rotate Signing Keys

**Frequency:** Every 180 days or on suspected compromise

**Procedure:**

```bash
# 1. Generate new signing key
openssl genrsa -out new-signing-key.pem 2048

# 2. Extract public key for JWKS
openssl rsa -in new-signing-key.pem -pubout -out new-signing-key.pub

# 3. Add new key to JWKS with unique kid
# Edit: /src/Identity/HCL.CS.SF.Identity.Application/Implementation/Endpoint/Services/JWKSService.cs

# 4. Deploy with new key active
# New tokens will use new key
# Old tokens remain valid until expiry

# 5. After maximum token lifetime (default 24h), remove old key
```

**Verification:**
```bash
# Verify JWKS endpoint shows new key
curl https://identity.HCL.CS.SF.example/.well-known/openid-configuration/jwks | jq '.keys[].kid'

# Verify tokens use new key (check JWT header)
echo "<access_token>" | cut -d. -f1 | base64 -d | jq '.kid'
```

**Rollback:**
- Keep old key in JWKS for overlap period
- If issues detected, switch `TokenConfig:DefaultSigningKey` back to old key

### 1.2 Database Migration

**Pre-requisites:**
- Database backup completed
- Maintenance window scheduled (for breaking changes)
- Migration scripts reviewed

**Procedure:**

```bash
# 1. Create backup
# PostgreSQL
pg_dump -U HCL.CS.SF -F c HCL.CS.SFIdentity > backup-$(date +%Y%m%d).dump

# SQL Server
sqlcmd -S localhost -Q "BACKUP DATABASE [HCL.CS.SFIdentity] TO DISK = 'backup.bak'"

# 2. Review pending migrations
dotnet ef migrations list \
  --project src/Identity/HCL.CS.SF.Identity.Persistence \
  --startup-project src/Identity/HCL.CS.SF.Identity.API

# 3. Generate SQL script (optional)
dotnet ef migrations script \
  --project src/Identity/HCL.CS.SF.Identity.Persistence \
  --startup-project src/Identity/HCL.CS.SF.Identity.API \
  --idempotent \
  -o migration-script.sql

# 4. Apply migrations
dotnet ef database update \
  --project src/Identity/HCL.CS.SF.Identity.Persistence \
  --startup-project src/Identity/HCL.CS.SF.Identity.API

# 5. Verify migration
# Check __EFMigrationsHistory table for latest entry
```

**Verification:**
```sql
-- Verify migration applied
SELECT MigrationId, ProductVersion 
FROM "__EFMigrationsHistory" 
ORDER BY MigrationId DESC 
LIMIT 1;
```

**Rollback:**
```bash
# If migration fails, revert to previous
dotnet ef database update <PreviousMigrationName> \
  --project src/Identity/HCL.CS.SF.Identity.Persistence \
  --startup-project src/Identity/HCL.CS.SF.Identity.API

# Restore from backup if needed
pg_restore -U HCL.CS.SF -d HCL.CS.SFIdentity --clean backup.dump
```

### 1.3 Seed Initial Data

**Use Case:** New deployment, adding standard configuration

**Procedure:**

```bash
# 1. Run installer for initial seed
dotnet run --project installer/HCL.CS.SF.Installer.Mvc \
  --launch-profile "https"

# 2. Navigate to installer UI
open https://localhost:7039

# 3. Complete setup wizard:
#    - Select database provider
#    - Configure connection
#    - Run migrations
#    - Create admin user
#    - Create initial OAuth client
```

**Manual Seed (if needed):**
```sql
-- Insert admin user (use application to properly hash password)
-- Insert standard API resources
-- Insert standard identity resources
-- Insert initial OAuth client
```

### 1.4 Reset Admin Password

**Procedure:**

```bash
# 1. Generate password reset token
# Use Admin API or database script

# 2. If direct database access:
# Update PasswordHash with new Argon2 hash
# Requires custom script - recommend using Admin API instead

# 3. Alternative: Use installer to create new admin account
# Then disable old account via database
```

**SQL (Emergency Only):**
```sql
-- ⚠️ WARNING: Direct DB modification bypasses validation
-- Update requires Argon2 hash - use application layer when possible
UPDATE "Users" 
SET "RequiresDefaultPasswordChange" = true 
WHERE "UserName" = 'admin';
```

### 1.5 Diagnose Auth Failures

**Investigation Steps:**

1. **Identify correlation ID**
   ```bash
   # Get correlation ID from client or logs
   grep "invalid_client" /var/log/HCL.CS.SF/*.log | grep "correlationId"
   ```

2. **Trace request flow**
   ```bash
   # Search logs for correlation ID
   grep "<correlation-id>" /var/log/HCL.CS.SF/*.log
   ```

3. **Check specific error codes**
   | Error Code | Common Causes | Resolution |
   |------------|---------------|------------|
   | `invalid_client` | Wrong client_id/secret | Verify credentials in client configuration |
   | `invalid_grant` | Expired/used code | Check token lifetime, single-use enforcement |
   | `invalid_request` | Missing parameters | Verify all required OAuth parameters present |
   | `invalid_scope` | Unauthorized scope | Check client.AllowedScopes configuration |

4. **Verify client configuration**
   ```sql
   SELECT "ClientId", "AllowedScopes", "SupportedGrantTypes", "RequirePkce"
   FROM "Clients"
   WHERE "ClientId" = '<client-id>';
   ```

### 1.6 Clean Up Expired Tokens

**Frequency:** Daily (automated)

**Procedure:**

```sql
-- PostgreSQL
DELETE FROM "SecurityTokens" 
WHERE "ConsumedAt" IS NOT NULL 
   OR TO_TIMESTAMP("ExpiresAt") < NOW() - INTERVAL '7 days';

-- SQL Server
DELETE FROM SecurityTokens 
WHERE ConsumedAt IS NOT NULL 
   OR DATEADD(SECOND, ExpiresAt, '1970-01-01') < DATEADD(DAY, -7, GETUTCDATE());

-- MySQL
DELETE FROM SecurityTokens 
WHERE ConsumedAt IS NOT NULL 
   OR FROM_UNIXTIME(ExpiresAt) < DATE_SUB(NOW(), INTERVAL 7 DAY);
```

**Verification:**
```sql
-- Count remaining expired tokens (should be 0)
SELECT COUNT(*) 
FROM "SecurityTokens" 
WHERE "ConsumedAt" IS NOT NULL 
   OR TO_TIMESTAMP("ExpiresAt") < NOW() - INTERVAL '7 days';
```

---

## 2. Incident Response Checklist

### 2.1 Token Issues

**Symptoms:**
- Clients unable to authenticate
- Token validation failures
- High rate of 401 responses

**Response Steps:**

```
□ 1. Check JWKS endpoint availability
   curl https://identity.HCL.CS.SF.example/.well-known/openid-configuration/jwks

□ 2. Verify signing key configuration
   - Check key files exist and are readable
   - Verify kid in JWT header matches JWKS

□ 3. Check token lifetimes
   - Verify exp claims are reasonable
   - Check server clock synchronization

□ 4. Review token validation errors in logs
   grep "invalid_token" /var/log/HCL.CS.SF/*.log | tail -100

□ 5. If key compromise suspected:
   - Rotate signing keys immediately
   - Invalidate all existing tokens
   - Notify affected clients

□ 6. Monitor error rate after fix
```

### 2.2 JWKS Mismatch

**Symptoms:**
- Resource servers report invalid signatures
- JWKS endpoint returns 404 or empty keys

**Response Steps:**

```
□ 1. Check JWKS endpoint response
   curl -v https://identity.HCL.CS.SF.example/.well-known/openid-configuration/jwks

□ 2. Verify ShowKeySet configuration
   - If false, JWKS disabled intentionally
   - Enable if resource servers need key discovery

□ 3. Verify signing keys exist in key store
   ls -la /app/keys/

□ 4. Check key format
   openssl rsa -in signing-key.pem -check -noout

□ 5. If keys missing:
   - Restore from backup
   - Or regenerate new keys (requires client reconfiguration)

□ 6. Verify JWKS cache expiration on resource servers
   - Default cache: 24 hours
   - May need to restart resource servers
```

### 2.3 Database Down

**Symptoms:**
- Health check reports database unhealthy
- All API requests failing
- Error: "Database connection failed"

**Response Steps:**

```
□ 1. Verify database server is running
   systemctl status postgresql  # Linux
   # Or check cloud console for managed DB

□ 2. Check network connectivity
   telnet db-server 5432
   # Or: nc -zv db-server 5432

□ 3. Verify connection string
   - Check credentials haven't expired
   - Verify hostname/IP is correct

□ 4. Check database logs
   tail -f /var/log/postgresql/*.log

□ 5. Check connection pool exhaustion
   SELECT count(*) FROM pg_stat_activity;

□ 6. If database corrupted:
   - Restore from latest backup
   - Apply transaction logs if available

□ 7. Verify application recovers
   curl https://identity.HCL.CS.SF.example/health/ready
```

### 2.4 Gateway 5xx Errors

**Symptoms:**
- High rate of 500/502/503 errors
- Gateway logs show timeouts

**Response Steps:**

```
□ 1. Check gateway health
   curl https://gateway.HCL.CS.SF.example/health/live

□ 2. Check upstream (Identity) health
   curl https://identity.HCL.CS.SF.example/health/ready

□ 3. Check resource utilization
   - CPU: top, htop
   - Memory: free -h
   - Disk: df -h

□ 4. Check for memory leaks
   - Monitor memory growth over time
   - Restart pods if memory pressure

□ 5. Scale horizontally if needed
   kubectl scale deployment HCL.CS.SF-identity --replicas=5

□ 6. Check for deadlock/thread pool exhaustion
   - Review thread dump if available
   - Restart services if unresponsive

□ 7. Enable detailed logging temporarily
   # Set LogLevel to Debug for investigation
```

### 2.5 Security Incident - Suspected Breach

**Symptoms:**
- Unusual token activity
- Failed authentication spikes from unknown IPs
- Suspicious audit log entries

**Response Steps:**

```
□ 1. Preserve evidence
   - Capture current logs
   - Snapshot database state
   - Do not restart services yet

□ 2. Identify affected scope
   - Which clients/users affected?
   - What data may be compromised?

□ 3. Immediate containment
   - Rotate all signing keys
   - Invalidate all active sessions
   - Force password reset for affected accounts

□ 4. Block suspicious IPs (if applicable)
   # At firewall/load balancer level

□ 5. Notify stakeholders
   - Security team
   - Compliance team
   - Affected customers (if required)

□ 6. Post-incident
   - Full security review
   - Update threat model
   - Implement additional controls
```

---

## 3. Safe Logging Rules

### 3.1 Prohibited in Logs

| Category | Examples | Why |
|----------|----------|-----|
| **Credentials** | Passwords, API keys, client secrets | Authentication bypass risk |
| **Tokens** | Access tokens, refresh tokens | Session hijacking risk |
| **PII** | Email addresses, phone numbers, SSN | Privacy violation, GDPR |
| **Financial** | Credit card numbers, bank accounts | PCI-DSS violation |
| **Full Request Bodies** | POST data with sensitive fields | Data leakage |

### 3.2 Allowed in Logs

| Category | Examples | Notes |
|----------|----------|-------|
| **User IDs** | GUIDs (not email format) | Use `LogRedactionHelper.GetSafeUserId()` |
| **Correlation IDs** | UUID format | Essential for tracing |
| **Client IDs** | Public identifiers | No security risk |
| **IP Addresses** | Client IPs | For security monitoring |
| **Timestamps** | Request times | Operational data |
| **Status Codes** | HTTP 200, 401, 500 | No sensitive data |

### 3.3 Redaction Helpers

**Source:** `/src/Gateway/HCL.CS.SF.Gateway/Hosting/LogRedactionHelper.cs`

```csharp
// Always use these helpers for user-facing values
LogRedactionHelper.GetSafeUserId(userId);      // Validates format
LogRedactionHelper.GetSafeTenantId(tenantId);  // Always redacts
LogRedactionHelper.RedactByFieldName(name, value); // Field-based redaction
```

### 3.4 Log Review Checklist

When reviewing logs for security:

- [ ] No passwords or secrets visible
- [ ] No full JWT tokens (truncation OK)
- [ ] No email addresses in user identifiers
- [ ] No phone numbers in plain text
- [ ] Correlation IDs present for all requests
- [ ] Failed authentication attempts logged
- [ ] Token issuance events logged
- [ ] Admin actions in audit trail

### 3.5 Log Retention

| Log Type | Retention | Storage |
|----------|-----------|---------|
| Application logs | 30 days | Hot storage |
| Security audit logs | 7 years | Cold storage (compliance) |
| Access logs | 90 days | Warm storage |
| Error logs | 1 year | Cold storage |

---

## 4. Emergency Procedures

### 4.1 Emergency Key Rotation

**When:** Suspected key compromise

```bash
#!/bin/bash
# emergency-key-rotation.sh

# 1. Generate emergency key
openssl genrsa -out /app/keys/emergency-key.pem 2048

# 2. Update configuration to use emergency key
kubectl set env deployment/HCL.CS.SF-identity \
  TokenSettings__TokenConfig__SigningKeyPath=/app/keys/emergency-key.pem \
  -n HCL.CS.SF

# 3. Restart deployment
kubectl rollout restart deployment/HCL.CS.SF-identity -n HCL.CS.SF

# 4. Verify new key in JWKS
curl https://identity.HCL.CS.SF.example/.well-known/openid-configuration/jwks | jq

# 5. Notify clients to refresh keys
# (JWKS has 24h cache by default)
```

### 4.2 Database Emergency Restore

```bash
#!/bin/bash
# emergency-db-restore.sh

# 1. Stop application
kubectl scale deployment HCL.CS.SF-identity --replicas=0 -n HCL.CS.SF

# 2. Drop and recreate database (PostgreSQL)
psql -U postgres -c "DROP DATABASE IF EXISTS HCL.CS.SFIdentity;"
psql -U postgres -c "CREATE DATABASE HCL.CS.SFIdentity;"

# 3. Restore from backup
pg_restore -U HCL.CS.SF -d HCL.CS.SFIdentity latest-backup.dump

# 4. Verify restore
psql -U HCL.CS.SF -d HCL.CS.SFIdentity -c "SELECT COUNT(*) FROM \"Users\";"

# 5. Run any pending migrations
dotnet ef database update \
  --project src/Identity/HCL.CS.SF.Identity.Persistence \
  --startup-project src/Identity/HCL.CS.SF.Identity.API

# 6. Restart application
kubectl scale deployment HCL.CS.SF-identity --replicas=3 -n HCL.CS.SF
```

### 4.3 Service Degradation Mode

When partial functionality is acceptable:

```json
// appsettings.Emergency.json
{
  "TokenSettings": {
    "TokenConfig": {
      "ShowKeySet": false,          // Disable JWKS to reduce load
      "CachingLifetime": 86400      // Increase cache duration
    },
    "TokenExpiration": {
      "AccessTokenExpiration": 60,   // Shorter tokens = less storage
      "RefreshTokenExpiration": 3600
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"  // Reduce log volume
    }
  }
}
```

### 4.4 Complete Service Shutdown

**When:** Critical security incident, data corruption

```bash
# Kubernetes
kubectl scale deployment HCL.CS.SF-identity --replicas=0 -n HCL.CS.SF
kubectl scale deployment HCL.CS.SF-gateway --replicas=0 -n HCL.CS.SF

# Docker Compose
docker-compose down

# Verify shutdown
kubectl get pods -n HCL.CS.SF
# Should show: No resources found
```

**Recovery:**
```bash
# After incident resolved
kubectl scale deployment HCL.CS.SF-identity --replicas=3 -n HCL.CS.SF
kubectl rollout status deployment/HCL.CS.SF-identity -n HCL.CS.SF
```

### 4.5 Communication Templates

**Internal Incident Notification:**
```
Subject: [INCIDENT] HCL.CS.SF Identity Service - <Brief Description>

Severity: [P1/P2/P3]
Start Time: <ISO timestamp>
Affected Service: HCL.CS.SF Identity Provider
Impact: <Description of user impact>

Current Status: <Investigating/Identified/Monitoring/Resolved>

Actions Taken:
- <Step 1>
- <Step 2>

Next Update: <Time>
Incident Commander: <Name>
```

**External Customer Notification (if required):**
```
Subject: Service Notification - HCL.CS.SF Identity

We are investigating issues with our identity service that may affect 
authentication for some users.

Impact: Users may experience difficulty logging in.

Workaround: Clear browser cookies and retry.

We will provide updates every 30 minutes.

Incident ID: <ID>
```

---

## 5. Contact Information

| Role | Contact | Escalation |
|------|---------|------------|
| On-Call Engineer | PagerDuty/Slack | +1 hour no response |
| Security Team | security@company.com | Immediate for P1 |
| Engineering Lead | eng-lead@company.com | P2 and above |
| Product Manager | pm@company.com | Customer-impacting |

---

## Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2026-03-01 | Enterprise Documentation Team | Initial release |
