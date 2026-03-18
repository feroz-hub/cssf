# RentFlow integration with HCL.CS.SF

HCL.CS.SF is configured so the **RentFlow** app receives a **capabilities** claim in the access token based on the user’s role. Each RentFlow role has a fixed list of capabilities that are added to the token when the user requests the `rentflow` scope.

## Roles and capabilities


| Role                  | Description                         | Capabilities (in access token)                                                                                                                                                                                                                                                                                                                                                               |
| --------------------- | ----------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **rentflow_owner**    | Full tenant and property management | health:read, tenant:read, tenant:write, tenant:users:read, tenant:users:invite, tenant:users:accept, tenant:users:manage, property:create, property:read, property:floor:add, property:room:add, property:bed:add, property:spaces:read, property:spaces:manage, occupancy:read, occupancy:bed:read, occupancy:bed:assign, occupancy:bed:unassign, resident:create, resident:read, meal:read |
| **rentflow_manager**  | Property and occupancy management   | health:read, tenant:read, tenant:users:read, tenant:users:invite, property:read, property:floor:add, property:room:add, property:bed:add, property:spaces:read, property:spaces:manage, occupancy:read, occupancy:bed:read, occupancy:bed:assign, occupancy:bed:unassign, resident:create, resident:read, meal:read                                                                          |
| **rentflow_resident** | Limited tenant actions              | health:read, tenant:users:accept, meal:skip, roomchat:read, roomchat:send                                                                                                                                                                                                                                                                                                                    |


## New installations (HCL.CS.SF seed)

If you run the **HCL.CS.SF installer** and seed a **fresh** database, the following are created automatically:

1. **API resource** `rentflow` with:
  - Resource claim type **capabilities** and **role**
  - Scope **rentflow** with claim types **capabilities** and **role**
2. **Roles**: `rentflow_owner`, `rentflow_manager`, `rentflow_resident`
3. **Role claims** for each role: `ClaimType = "capabilities"`, `ClaimValue =` each capability string (e.g. `health:read`, `tenant:read`, …)

No extra steps are required for a new install.

## Existing installations (HCL.CS.SF already seeded)

If the database was seeded **before** RentFlow support was added, do one of the following.

### Option A – Run a one-time migration/script

Use your usual migration or SQL/script approach to:

1. Insert the **rentflow** API resource and its **ApiResourceClaims** (Type = `capabilities`, Type = `role`) and **ApiScopes** (scope name `rentflow`) with **ApiScopeClaims** (Type = `capabilities`, Type = `role`), if not already present.
2. Insert the three **Roles**: `rentflow_owner`, `rentflow_manager`, `rentflow_resident`.
3. Insert **RoleClaims** for each role with `ClaimType = "capabilities"` and `ClaimValue` = each capability from the table above (see seed: `HCL.CS.SFMasterDataSeed.CreateRoleClaims_RentFlowOwner()`, `CreateRoleClaims_RentFlowManager()`, `CreateRoleClaims_RentFlowResident()`).

### Option B – Admin UI

See **[Add RentFlow via Admin UI](#add-rentflow-via-admin-ui)** below for step-by-step instructions.

## Add RentFlow via Admin UI

Follow these steps in **HCL.CS.SF Admin** to add RentFlow (resource, scope, roles, capabilities, and client) without running the seed again.

**Where to find it in the sidebar:** **Security** → **Resources & Scopes**; **Identity** → **Roles & Claims**, **Users**; **Security** → **Clients**.

---

### Step 1: Create the RentFlow API resource and scope

1. In the sidebar, go to **API Resources & Scopes** (or **Resources**).
2. Click **Create Resource**.
3. Fill in:
  - **Name:** `rentflow`
  - **Display Name:** `RentFlow`
  - **Description:** e.g. `RentFlow app – capabilities in access token by role`
  - **Enabled:** checked
4. Click **Save**.
5. Find the new **rentflow** resource in the table and click **Scopes** to expand it.
6. Under **Resource claim types**:
  - In the input, type `**capabilities`** and click **Add claim type**.
  - Type `**role`** and click **Add claim type**.
7. Under **Scopes**, click **Create Scope**.
8. Create one scope:
  - **Name:** `rentflow`
  - **Display Name:** `RentFlow`
  - **Description:** e.g. `RentFlow API access with capabilities claim`
  - Save.
9. In the same expanded row, in the **Claim types** column for the **rentflow** scope:
  - In the small input, type `**capabilities`** and click **Add**.
  - Type `**role`** and click **Add**.

---

### Step 2: Create the three RentFlow roles

1. In the sidebar, go to **Roles & Claims**.
2. Create each role (click **Create role** or equivalent, then Save):
  - **rentflow_owner** – Description: e.g. `RentFlow owner – full tenant and property management`
  - **rentflow_manager** – Description: e.g. `RentFlow manager – property and occupancy management`
  - **rentflow_resident** – Description: e.g. `RentFlow resident – limited tenant actions`

---

### Step 3: Add capabilities to each role

For each role, expand it (e.g. click the role row or **Claims**) and add claims with **Claim type** = `**capabilities`** and **Claim value** = one capability per claim (use **Add Claim** for each value).

**rentflow_owner** – add these 21 claims (each with Claim type `capabilities` and value as below):

```
health:read
tenant:read
tenant:write
tenant:users:read
tenant:users:invite
tenant:users:accept
tenant:users:manage
property:create
property:read
property:floor:add
property:room:add
property:bed:add
property:spaces:read
property:spaces:manage
occupancy:read
occupancy:bed:read
occupancy:bed:assign
occupancy:bed:unassign
resident:create
resident:read
meal:read
```

**rentflow_manager** – add these 17 claims (Claim type `capabilities`):

```
health:read
tenant:read
tenant:users:read
tenant:users:invite
property:read
property:floor:add
property:room:add
property:bed:add
property:spaces:read
property:spaces:manage
occupancy:read
occupancy:bed:read
occupancy:bed:assign
occupancy:bed:unassign
resident:create
resident:read
meal:read
```

**rentflow_resident** – add these 5 claims (Claim type `capabilities`):

```
health:read
tenant:users:accept
meal:skip
roomchat:read
roomchat:send
```

**Note:** Role claims with **Claim type** = `capabilities` are allowed to have any **Claim value** (e.g. `health:read`). Other claim types (e.g. `permission`) still require the value to be an existing API scope name. If you previously saw "Scope does not exist/active in Api resource or scope master", that validation has been relaxed for `capabilities` claims.

---

### Step 4: Give the RentFlow client the rentflow scope

1. In the sidebar, go to **Clients**.
2. Open the client used by the RentFlow app (or create a new one).
3. In **Allowed scopes**, ensure `**rentflow`** is selected (e.g. under the RentFlow / API resource group). Keep any other scopes the app needs (e.g. `openid`, `profile`, `email`, `offline_access`).
4. Save the client.

---

## How to create a HCL.CS.SF client for the RentFlow control-plane BFF

Use this when integrating the **control-plane BFF** with HCL.CS.SF. Register the following in HCL.CS.SF (via HCL.CS.SF Admin) and configure the control-plane env to match.

### 1. Redirect URI (login callback)

Register this in HCL.CS.SF as the OAuth/OIDC **redirect URI** for each environment:


| Environment     | Redirect URI                                      |
| --------------- | ------------------------------------------------- |
| Local (default) | `https://localhost:3000/auth/callback`            |
| Custom          | `{NEXT_PUBLIC_RENTFLOW_APP_ORIGIN}/auth/callback` |


Use your app’s origin + `/auth/callback`, e.g.:

- **Dev:** `https://localhost:3000/auth/callback`
- **Staging:** `https://your-staging-domain.example/auth/callback`
- **Prod:** `https://your-production-domain.example/auth/callback`

### 2. Post-logout redirect URI

Register this in HCL.CS.SF as the **post-logout redirect URI**:


| Environment     | Post-logout URI                      |
| --------------- | ------------------------------------ |
| Local (default) | `https://localhost:3000/`            |
| Custom          | `{NEXT_PUBLIC_RENTFLOW_APP_ORIGIN}/` |


### 3. Client configuration summary (for HCL.CS.SF)

When creating or editing the OAuth client in HCL.CS.SF Admin, use:


| Setting                         | Value                                                                                                                                                                                                                                                   |
| ------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Grant type**                  | **Authorization Code** + **PKCE** (S256). In HCL.CS.SF Admin, select **authorization_code** (and **refresh_token** if the BFF uses refresh tokens). Ensure PKCE is required (HCL.CS.SF Admin creates clients with **Require PKCE** by default for this flow). |
| **Redirect URI(s)**             | `https://<your-app-origin>/auth/callback` — add one per environment (local, staging, prod).                                                                                                                                                             |
| **Post-logout redirect URI(s)** | `https://<your-app-origin>/` — add one per environment.                                                                                                                                                                                                 |
| **Client ID**                   | `rentflow.bff.local` (or the value you set in control-plane as `RENTFLOW_BFF_CLIENT_ID`)                                                                                                                                                                |
| **Client secret**               | Set in control-plane as `RENTFLOW_BFF_CLIENT_SECRET` (generate or copy from HCL.CS.SF after creating the client).                                                                                                                                          |
| **Scopes**                      | `openid profile email rentflow.api` (or set `RENTFLOW_BFF_SCOPE` in control-plane). For RentFlow capabilities, include `**rentflow`** so the token includes **sub**, **capabilities**, and optionally **tenant_id**.                                    |


The token must include at least **sub** and **capabilities** (and optionally **tenant_id**).

### 4. Creating the client in HCL.CS.SF Admin

1. In the sidebar, go to **Security** → **Clients** (or **Clients**).
2. Click **Create client** (or **Register client**).
3. Fill in:
  - **Client ID:** `rentflow.bff.local` (or your chosen ID; must match `RENTFLOW_BFF_CLIENT_ID` in control-plane).
  - **Client name:** e.g. `RentFlow Control-Plane BFF`.
  - **Application type:** **Web** (or as appropriate for a BFF).
  - **Allowed grant types:** check **authorization_code** and **refresh_token** (if the BFF uses refresh tokens).
  - **Redirect URIs:** add one line per environment, e.g.:
    - `https://localhost:3000/auth/callback`
    - Add staging/prod origins as needed, e.g. `https://your-staging-domain.example/auth/callback`.
  - **Post-logout redirect URIs:** add one line per environment, e.g.:
    - `https://localhost:3000/`
    - Add staging/prod origins as needed, e.g. `https://your-staging-domain.example/`.
  - **Allowed scopes:** select **openid**, **profile**, **email**, and **rentflow** (and **rentflow.api** if you use that scope). Include **offline_access** if using refresh tokens.
  - Leave **Require PKCE** enabled (S256). Do not allow public clients unless you intend a no-secret flow.
  - Set **Client secret** (generate or set your own) and store the same value in control-plane as `RENTFLOW_BFF_CLIENT_SECRET`.
4. Save the client. Copy the client secret if newly generated and set it in control-plane.
5. Ensure the **rentflow** API resource and scope exist (see [Step 1: Create the RentFlow API resource and scope](#step-1-create-the-rentflow-api-resource-and-scope)) so that the **rentflow** scope and **capabilities** claim are issued.

### 5. Control-plane env (to match HCL.CS.SF)

In `apps/control-plane/.env.local` (or your environment), either rely on defaults or set:

```bash
# Redirect URI — only if different from {APP_ORIGIN}/auth/callback
# AUTH_HCL.CS.SF_REDIRECT_URI=https://localhost:3000/auth/callback

# Post-logout — only if different from {APP_ORIGIN}/
# AUTH_HCL.CS.SF_POST_LOGOUT_REDIRECT_URI=https://localhost:3000/
```

Rule: **client redirect URI in HCL.CS.SF** = `<APP_ORIGIN>/auth/callback`, and **post-logout URI in HCL.CS.SF** = `<APP_ORIGIN>/`. Register those exact URLs in HCL.CS.SF for each environment you use.

---

### Step 5: Assign users to RentFlow roles

1. Go to **Users** and open a user.
2. Assign one of **rentflow_owner**, **rentflow_manager**, or **rentflow_resident** (depending on how that user should use RentFlow).
3. When that user gets a token with scope `**rentflow`**, the access token will contain **role** and **capabilities** for that role.

---

## RentFlow client configuration

- **Allowed scopes** for the RentFlow client must include `**rentflow`** so that token issuance evaluates the rentflow resource and adds the **capabilities** (and **role**) claims.
- When requesting tokens (e.g. authorization code or password grant), include `**rentflow`** in the `scope` parameter.

---

## Authorization code flow – required request parameters (for RentFlow frontend)

Use this section when implementing the **authorization code + PKCE** flow in the RentFlow frontend. Base URL is your HCL.CS.SF authority (e.g. `https://your-HCL.CS.SF-host`). Paths are relative to that base.

### Step 1: Authorize request (redirect the user to login)

**Method:** `GET`  
**URL:** `{authority}/security/authorize`

**Query parameters (sent in the URL when redirecting the user to HCL.CS.SF):**

| Parameter               | Required | Description |
| ----------------------- | -------- | ----------- |
| `client_id`             | **Yes**  | OAuth client ID (e.g. `rentflow.bff.local`). |
| `redirect_uri`          | **Yes**  | Callback URL where HCL.CS.SF will send the user after login. Must **exactly** match a redirect URI registered for the client (e.g. `https://your-app/auth/callback`). |
| `response_type`         | **Yes**  | Use `code` for authorization code flow. |
| `scope`                 | **Yes**  | Space-separated scopes. Must include `openid` for OIDC; include `rentflow` for capabilities in the token. Example: `openid profile email rentflow`. Add `offline_access` if you need refresh tokens. |
| `state`                 | **Yes**  | Opaque value you generate and store (e.g. in session). HCL.CS.SF returns it in the callback so you can prevent CSRF. |
| `nonce`                 | **Yes**  | Required when `scope` includes `openid`. Random string you generate; it is echoed in the id_token. Generate per request (e.g. random string or UUID). |
| `code_challenge`        | **Yes**  | PKCE challenge. Must be **base64url**(**SHA256**(`code_verifier`)). No `+`, `/`, or `=` padding. HCL.CS.SF requires PKCE for authorization_code (S256 only). |
| `code_challenge_method` | **Yes**  | Use `S256` (HCL.CS.SF only supports S256). |

**Example authorize URL (split for readability):**

```
GET {authority}/security/authorize?client_id=rentflow.bff.local
  &redirect_uri=https://your-app.example/auth/callback
  &response_type=code
  &scope=openid%20profile%20email%20rentflow
  &state=your-random-state
  &nonce=your-random-nonce
  &code_challenge=BASE64URL_SHA256_OF_CODE_VERIFIER
  &code_challenge_method=S256
```

- Generate a random **code_verifier** (43–128 chars, charset `[A-Za-z0-9\-._~]`). Store it (e.g. in session or secure cookie) for the token step.
- Compute **code_challenge** = base64url(SHA256(code_verifier)). Do **not** use standard base64 (use `-` and `_`, no padding).

After the user logs in and consents, HCL.CS.SF redirects to your `redirect_uri` with query parameters: `code` (the authorization code) and `state` (your state). Extract the `code` and use it in Step 2.

---

### Step 2: Token request (exchange code for tokens)

**Method:** `POST`  
**URL:** `{authority}/security/token`  
**Content-Type:** `application/x-www-form-urlencoded`

**Request body (form fields):**

| Parameter       | Required | Description |
| --------------- | -------- | ----------- |
| `grant_type`    | **Yes**  | `authorization_code` |
| `code`          | **Yes**  | The authorization code from the callback URL (`?code=...`). |
| `redirect_uri`  | **Yes**  | **Same** value you used in the authorize request. Must match exactly. |
| `code_verifier` | **Yes**  | The same **code_verifier** you used to generate `code_challenge` in Step 1. |

**Client authentication:** Send the client credentials using **HTTP Basic** (recommended) or in the body.

- **Option A – Basic:**  
  `Authorization: Basic base64(client_id:client_secret)`
- **Option B – Body:**  
  Add to the form: `client_id` and `client_secret`.

**Example (conceptual):**

```
POST {authority}/security/token
Content-Type: application/x-www-form-urlencoded
Authorization: Basic <base64(client_id:client_secret)>

grant_type=authorization_code
&code=THE_CODE_FROM_CALLBACK
&redirect_uri=https://your-app.example/auth/callback
&code_verifier=THE_SAME_VERIFIER_USED_IN_STEP_1
```

**Response:** JSON with `access_token`, `token_type` (e.g. `Bearer`), `expires_in`, and optionally `refresh_token`, `id_token`, `scope`. The access token will contain the **rentflow** scope and, when the user has a RentFlow role, the **capabilities** and **role** claims.

---

### Summary checklist for frontend

1. **Before redirecting to HCL.CS.SF:** Generate and store `state`, `nonce` (if using openid), and `code_verifier`; compute `code_challenge` = base64url(SHA256(code_verifier)).
2. **Authorize request:** GET with `client_id`, `redirect_uri`, `response_type=code`, `scope` (include `openid` and `rentflow`), `state`, `nonce` (if openid), `code_challenge`, `code_challenge_method=S256`.
3. **Callback:** Read `code` and `state` from the URL; validate `state`; exchange `code` with the same `redirect_uri` and `code_verifier`.
4. **Token request:** POST with `grant_type=authorization_code`, `code`, `redirect_uri`, `code_verifier`, and client credentials (Basic or body).

---

## Local testing: client_credentials flow

To test RentFlow token generation locally with the **client_credentials** grant (machine-to-machine, no user):

### 1. Ensure the RentFlow scope exists

- **API resource** `rentflow` with **scope** `rentflow` must exist.
- **New installs:** created by seed (see [New installations](#new-installations-HCL.CS.SF-seed)).
- **Existing installs:** add via [Step 1: Create the RentFlow API resource and scope](#step-1-create-the-rentflow-api-resource-and-scope) in the Admin UI, or run your migration/script.

### 2. Create or edit a client for client_credentials

In **HCL.CS.SF Admin** → **Security** → **Clients**:

1. Create a new client (or pick an existing one for testing).
2. Set:
   - **Client ID:** e.g. `rentflow.m2m.local`
   - **Client secret:** set or generate and save it for the request.
   - **Allowed grant types:** enable **client_credentials**.
   - **Allowed scopes:** include **rentflow** (and any other API scopes you need). Do **not** add identity scopes like `openid` or `profile` for client_credentials.
3. Save. Copy the client secret if you generated it.

### 3. Request a token locally

Use your HCL.CS.SF **token endpoint** (e.g. `https://localhost:<port>/security/token` or the URL from your discovery document). Replace `<TOKEN_ENDPOINT>`, `<CLIENT_ID>`, and `<CLIENT_SECRET>`.

**Using curl:**

```bash
curl -X POST "<TOKEN_ENDPOINT>" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -u "<CLIENT_ID>:<CLIENT_SECRET>" \
  -d "grant_type=client_credentials" \
  -d "scope=rentflow"
```

**Using PowerShell (Windows):**

```powershell
$base64 = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("<CLIENT_ID>:<CLIENT_SECRET>"))
Invoke-RestMethod -Method Post -Uri "<TOKEN_ENDPOINT>" `
  -Headers @{ "Authorization" = "Basic $base64"; "Content-Type" = "application/x-www-form-urlencoded" } `
  -Body "grant_type=client_credentials&scope=rentflow"
```

A successful response includes `access_token`, `expires_in`, and `token_type` (e.g. `Bearer`). The access token will contain the **rentflow** scope (and any scope claims configured for that scope). Decode the JWT to confirm `scope` (or `scopes`) includes `rentflow`.

**Note:** With **client_credentials** there is no user, so the token does **not** include user-based **role** or **capabilities** claims. Those are added only for user-involved flows (e.g. authorization code, password) when the user has a RentFlow role and requests the `rentflow` scope. For machine-to-machine testing you are only verifying that the **rentflow** scope is issued and the token is valid.

### 4. Optional: use HCL.CS.SF Admin Operations

If **Operations** → **Endpoints** (or similar) in HCL.CS.SF Admin exposes a token tester:

1. Choose **Client credentials** flow.
2. Enter the same client ID and secret.
3. Set **Scope** to `rentflow` and send the request to confirm the token is returned.

## Token shape

When a user has one of the RentFlow roles and the token is requested with scope `rentflow`, the access token will contain:

- **role**: e.g. `rentflow_owner`, `rentflow_manager`, or `rentflow_resident`
- **capabilities**: array of strings, e.g. `["health:read", "tenant:read", "tenant:write", ...]` (exact list depends on the role; see table above)

RentFlow can authorize API calls by checking the **capabilities** claim in the access token.

## Assigning users to RentFlow roles

In HCL.CS.SF Admin:

1. Go to **Users**, open the user, and assign the appropriate role: **rentflow_owner**, **rentflow_manager**, or **rentflow_resident**.
2. That user’s next token request with scope **rentflow** will include the **capabilities** list for that role.

## Seed reference (code)

- **API resource**: `installer/HCL.CS.SF.Installer.Mvc/Infrastructure/Seeding/HCL.CS.SFMasterDataSeed.cs` (RentFlow entry in `GetApiResourceEntityMaster()`).
- **Roles**: same file, `CreateRolesMaster()` (rentflow_owner, rentflow_manager, rentflow_resident).
- **Role claims**: same file, `CreateRoleClaims_RentFlowOwner()`, `CreateRoleClaims_RentFlowManager()`, `CreateRoleClaims_RentFlowResident()`.
- **Wiring in seed**: `installer/HCL.CS.SF.Installer.Mvc/Infrastructure/Services/SeedDataService.cs` (RentFlow role claims added after HCL.CS.SFUser claims).

