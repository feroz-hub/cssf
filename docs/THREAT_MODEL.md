# Threat Model

## Primary assets

- User credentials and tokens.
- Client secrets and signing keys.
- Audit records.

## Key threat categories

- Credential theft and replay.
- Token forgery and key compromise.
- Privilege escalation via weak authorization checks.
- Sensitive data leakage in logs or error responses.

## Controls baseline

- Strong hashing and key storage controls.
- HTTPS-only deployment for all public endpoints.
- Least-privilege service permissions and DB credentials.
- Centralized logging with tamper-evident retention.
