# Railway Deployment

This guide deploys HCL.CS.SF to a single Railway project with these services:

- `postgres`: Railway PostgreSQL
- `redis`: Railway Redis
- `HCL.CS.SF-server`: the identity server
- `HCL.CS.SF-installer`: the first-time setup UI
- `HCL.CS.SF-admin`: the Next.js admin app

Important:

- Railway terminates TLS at the edge. The containers should listen on internal HTTP, but the public URLs are still `https://`.
- `HCL.CS.SF-server` already runs the PostgreSQL bootstrap SQL and every `scripts/migrations/*_postgresql.sql` file during startup.
- Use the installer only for the first baseline setup and seed. After that, redeploying `HCL.CS.SF-server` is enough for later SQL migrations.
- Token signing certificates are not the same as HTTPS certificates. You must keep the signing certificates stable across restarts.
- Railway reference variables use the syntax `${{service-name.VARIABLE_NAME}}`. The examples below assume your service names are exactly `postgres`, `redis`, `HCL.CS.SF-server`, `HCL.CS.SF-installer`, and `HCL.CS.SF-admin`.
- HCL.CS.SF now uses `AutoMapper 16.1.1`. If you have an AutoMapper license key, set `HCL.CS.SF_AUTOMAPPER_LICENSE_KEY` on the server and installer. If you leave it unset, deployment still works but AutoMapper will log a license warning at startup.

## 1. Before You Start

Create these secrets locally:

```bash
export HCL.CS.SF_SIGNING_CERT_PASSWORD="$(openssl rand -base64 48 | tr -d '\n')"
export NEXTAUTH_SECRET="$(openssl rand -base64 32 | tr -d '\n')"
```

Generate persistent token-signing certificates for HCL.CS.SF:

```bash
openssl req -x509 -nodes -newkey rsa:4096 \
  -keyout HCL.CS.SF-rsa.key \
  -out HCL.CS.SF-rsa.crt \
  -days 825 \
  -subj "/CN=HCL.CS.SF Token RSA"

openssl pkcs12 -export \
  -out HCL.CS.SF-rsa.pfx \
  -inkey HCL.CS.SF-rsa.key \
  -in HCL.CS.SF-rsa.crt \
  -password "pass:${HCL.CS.SF_SIGNING_CERT_PASSWORD}"

openssl ecparam -genkey -name prime256v1 -noout -out HCL.CS.SF-ecdsa.key

openssl req -new -x509 \
  -key HCL.CS.SF-ecdsa.key \
  -out HCL.CS.SF-ecdsa.crt \
  -days 825 \
  -subj "/CN=HCL.CS.SF Token ECDSA"

openssl pkcs12 -export \
  -out HCL.CS.SF-ecdsa.pfx \
  -inkey HCL.CS.SF-ecdsa.key \
  -in HCL.CS.SF-ecdsa.crt \
  -password "pass:${HCL.CS.SF_SIGNING_CERT_PASSWORD}"
```

Convert both `.pfx` files to one-line Base64 strings for Railway secrets:

```bash
base64 < HCL.CS.SF-rsa.pfx | tr -d '\n'
base64 < HCL.CS.SF-ecdsa.pfx | tr -d '\n'
```

## 2. Create The Railway Services

Create one Railway project, then add:

- PostgreSQL service
- Redis service
- `HCL.CS.SF-server` service from this repo
- `HCL.CS.SF-installer` service from this repo
- `HCL.CS.SF-admin` service from this repo

Recommended service settings:

| Service | Root Directory | Dockerfile | Public Domain | Health Check |
| --- | --- | --- | --- | --- |
| `HCL.CS.SF-server` | `.` | `docker/identity-api.Dockerfile` | Yes | `/health/ready` |
| `HCL.CS.SF-installer` | `.` | `docker/installer.Dockerfile` | Yes | `/health` |
| `HCL.CS.SF-admin` | `HCL.CS.SF-admin` | `Dockerfile` | Yes | `/api/health` |

Recommended volumes:

- Add a volume to `HCL.CS.SF-server` mounted at `/data`
- Add a volume to `HCL.CS.SF-installer` mounted at `/data`

The server volume keeps ASP.NET Core data-protection keys stable. The installer volume keeps its installation lock file stable after the first successful setup.

For `HCL.CS.SF-server` and `HCL.CS.SF-installer`, set `RAILWAY_DOCKERFILE_PATH` to the Dockerfile path shown above. Railway uses that configuration variable to select a non-root Dockerfile.

## 3. Configure `HCL.CS.SF-server`

Set these variables on the `HCL.CS.SF-server` service.

Required:

| Variable | Value |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `RAILWAY_DOCKERFILE_PATH` | `docker/identity-api.Dockerfile` |
| `SystemSettings__DBConfig__Database` | `PostgreSQL` |
| `HCL.CS.SF_DB_CONNECTION_STRING` | `Host=${{postgres.PGHOST}};Port=${{postgres.PGPORT}};Database=${{postgres.PGDATABASE}};Username=${{postgres.PGUSER}};Password=${{postgres.PGPASSWORD}};SSL Mode=Require;` |
| `HCL.CS.SF_REDIS_CONNECTION_STRING` | `${{redis.REDIS_URL}}` |
| `HCL.CS.SF_REDIS_INSTANCE_NAME` | `HCL.CS.SF:` |
| `TokenSettings__TokenConfig__IssuerUri` | `https://${{RAILWAY_PUBLIC_DOMAIN}}` |
| `Security__Cors__AllowedOrigins__0` | `https://${{HCL.CS.SF-admin.RAILWAY_PUBLIC_DOMAIN}}` |
| `Security__Cors__AllowedOrigins__1` | `https://${{RAILWAY_PUBLIC_DOMAIN}}` |
| `HCL.CS.SF_TRUST_PROXY_HEADERS` | `true` |
| `HCL.CS.SF_DATA_PROTECTION_KEYS_PATH` | `/data/keys` |
| `HCL.CS.SF_SIGNING_CERT_PASSWORD` | the secret you generated above |
| `HCL.CS.SF_RSA_SIGNING_CERT_BASE64` | Base64 of `HCL.CS.SF-rsa.pfx` |
| `HCL.CS.SF_ECDSA_SIGNING_CERT_BASE64` | Base64 of `HCL.CS.SF-ecdsa.pfx` |

Recommended:

| Variable | Value |
| --- | --- |
| `HCL.CS.SF_RSA_SIGNING_KID` | `HCL.CS.SF-rsa-current` |
| `HCL.CS.SF_ECDSA_SIGNING_KID` | `HCL.CS.SF-ecdsa-current` |

Optional, if you have an AutoMapper license key:

| Variable | Value |
| --- | --- |
| `HCL.CS.SF_AUTOMAPPER_LICENSE_KEY` | your AutoMapper license key |

Optional, only if you enable Google login in HCL.CS.SF:

| Variable | Value |
| --- | --- |
| `Authentication__Google__Enabled` | `true` |
| `Authentication__Google__ClientId` | your Google OAuth client id |
| `Authentication__Google__ClientSecret` | your Google OAuth client secret |
| `Authentication__Google__AllowedRedirectHosts__0` | admin hostname only, for example `HCL.CS.SF-admin.up.railway.app` |

Notes:

- `TokenSettings__TokenConfig__IssuerUri` must exactly match the public URL users and clients use for HCL.CS.SF.
- Do not use `${{postgres.DATABASE_URL}}` for `HCL.CS.SF_DB_CONNECTION_STRING`. HCL.CS.SF startup bootstrap expects the Npgsql keyword format shown above.
- Use a custom domain for production if possible. If you switch domains later, update the issuer and any seeded client URLs together.
- If SMTP or SMS is required, also set `HCL.CS.SF_SMTP_*` and `HCL.CS.SF_SMS_*` variables from `demos/HCL.CS.SF.Demo.Server/Configurations/SystemSettings.json`.

## 4. Configure `HCL.CS.SF-installer`

Set these variables on the `HCL.CS.SF-installer` service.

| Variable | Value |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `RAILWAY_DOCKERFILE_PATH` | `docker/installer.Dockerfile` |
| `HCL.CS.SF_TRUST_PROXY_HEADERS` | `true` |
| `InstallerLock__MarkerFilePath` | `/data/installer.lock.json` |
| `HCL.CS.SF_INSTALLER_DATA_PROTECTION_KEYS_PATH` | `/data/keys` |

Optional:

| Variable | Value |
| --- | --- |
| `HCL.CS.SF_AUTOMAPPER_LICENSE_KEY` | your AutoMapper license key |

Notes:

- The installer does not need direct DB variables ahead of time. You provide the connection string in the installer UI.
- Keep the installer public only while you need first-time setup. After installation, remove its public domain or restrict access.

## 5. Configure `HCL.CS.SF-admin`

Set these variables on the `HCL.CS.SF-admin` service.

Required:

| Variable | Value |
| --- | --- |
| `NEXTAUTH_URL` | `https://${{RAILWAY_PUBLIC_DOMAIN}}` |
| `NEXTAUTH_SECRET` | the secret you generated above |
| `HCL.CS.SF_ISSUER` | `https://${{HCL.CS.SF-server.RAILWAY_PUBLIC_DOMAIN}}` |
| `HCL.CS.SF_API_BASE_URL` | `https://${{HCL.CS.SF-server.RAILWAY_PUBLIC_DOMAIN}}` |
| `HCL.CS.SF_DEMO_SERVER_BASE_URL` | `https://${{HCL.CS.SF-server.RAILWAY_PUBLIC_DOMAIN}}` |
| `HCL.CS.SF_INSTALLER_BASE_URL` | `https://${{HCL.CS.SF-installer.RAILWAY_PUBLIC_DOMAIN}}` |
| `HCL.CS.SF_POST_LOGOUT_REDIRECT_URI` | `https://${{RAILWAY_PUBLIC_DOMAIN}}/login` |
| `HCL.CS.SF_CLIENT_ID` | client id created in the installer seed step |
| `HCL.CS.SF_CLIENT_SECRET` | client secret created in the installer seed step |

Recommended:

| Variable | Value |
| --- | --- |
| `HCL.CS.SF_SCOPES` | `openid profile email offline_access phone HCL.CS.SF.apiresource HCL.CS.SF.client HCL.CS.SF.user HCL.CS.SF.role HCL.CS.SF.identityresource HCL.CS.SF.adminuser HCL.CS.SF.securitytoken` |
| `HCL.CS.SF_ENABLE_FEDERATED_LOGOUT` | `false` for the first deployment |
| `HCL.CS.SF_ALLOW_INSECURE_TLS` | `false` |
| `NEXT_PUBLIC_GOOGLE_LOGIN_ENABLED` | `false` unless you explicitly configure Google flow support |

Optional:

| Variable | Value |
| --- | --- |
| `HCL.CS.SF_METADATA_ADDRESS` | `https://${{HCL.CS.SF-server.RAILWAY_PUBLIC_DOMAIN}}/.well-known/openid-configuration` |
| `HCL.CS.SF_TOKEN_ENDPOINT` | `https://${{HCL.CS.SF-server.RAILWAY_PUBLIC_DOMAIN}}/security/token` |
| `HCL.CS.SF_REVOCATION_ENDPOINT` | `https://${{HCL.CS.SF-server.RAILWAY_PUBLIC_DOMAIN}}/security/revocation` |

Notes:

- `HCL.CS.SF-admin` no longer needs Railway build arguments. Set these values as runtime service variables only.

## 6. First-Time Deployment Order

1. Deploy `postgres` and `redis`.
2. Deploy `HCL.CS.SF-server` with its production variables.
3. Deploy `HCL.CS.SF-installer`.
4. Open the installer public URL.
5. In the installer, choose `PostgreSql`.
6. Use the Railway PostgreSQL internal values in the connection string:

```text
Host=<PGHOST>;Port=<PGPORT>;Database=<PGDATABASE>;Username=<PGUSER>;Password=<PGPASSWORD>;SSL Mode=Require;
```

7. Validate the connection.
8. Run migrations.
9. Seed the baseline data.
10. Copy the generated `Client Id` and `Client Secret`.
11. Set those two values on `HCL.CS.SF-admin`.
12. Deploy `HCL.CS.SF-admin`.

If the installer complains about PostgreSQL TLS validation, append this once:

```text
Trust Server Certificate=true;
```

## 7. Recommended Installer Seed Values For `HCL.CS.SF-admin`

Use these values in the installer seed screen.

Client:

- Client name: `HCL.CS.SF Admin`
- Client URI: `https://<your-HCL.CS.SF-admin-domain>`
- Grant types: enable `Authorization Code`, `Refresh Token`, and `Resource Owner Password`
- Response types: keep `code`
- Use default scopes: `true`
- Redirect URI: `https://<your-HCL.CS.SF-admin-domain>/api/auth/callback/HCL.CS.SF`
- Post logout redirect URI: `https://<your-HCL.CS.SF-admin-domain>/login`

Admin user:

- Create your first HCL.CS.SF administrator account here

Notes:

- The current `HCL.CS.SF-admin` app signs users in with password grant and refresh token. That is why `Resource Owner Password` and `Refresh Token` must be enabled.
- If you later enable Google login inside `HCL.CS.SF-admin`, you will also need a client that supports the `user_code` grant. The installer seed UI does not create that grant today, so keep `NEXT_PUBLIC_GOOGLE_LOGIN_ENABLED=false` until you add it separately.
- Seeded client secrets currently expire after 100 days. Plan to rotate or replace that client secret before it expires.
- If you switch from Railway public domains to custom domains, update the service variables above to the custom URLs before you re-seed or ask users to sign in.

## 8. Verification

After deployment, verify these URLs:

- `https://<your-HCL.CS.SF-server-domain>/.well-known/openid-configuration`
- `https://<your-HCL.CS.SF-server-domain>/health/ready`
- `https://<your-HCL.CS.SF-installer-domain>/health`
- `https://<your-HCL.CS.SF-admin-domain>/api/health`

Expected outcome:

- discovery returns the public issuer URL
- installer validates PostgreSQL and completes seed once
- admin app loads and can sign in with the seeded client and admin user

## 9. After The First Install

After the first successful installer run:

- keep `HCL.CS.SF-server` deployed
- keep `postgres` and `redis` deployed
- keep `HCL.CS.SF-admin` deployed
- remove the installer public domain or restrict access

For later code or schema changes:

- redeploy `HCL.CS.SF-server`
- its startup entrypoint will apply the PostgreSQL bootstrap script and all available PostgreSQL migration scripts again
- do not rerun seed unless you intentionally want a brand-new empty database
