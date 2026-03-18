# Access Token Generation and Customizing Token Fields (e.g. Capabilities)

## Where access tokens are generated

Access tokens are produced in the **Identity** project by `TokenGenerationService`:

| Step | Location | What happens |
|------|----------|----------------|
| Entry | `TokenGenerationService.ProcessTokenAsync` | Validates request, gathers claims, generates access (and optionally refresh/identity) token. |
| Claims | `GetTokenClaimsAsync` | Builds `ResultClaimsModel`: identity, role, permission, transaction, scope, audience claims. |
| Access token | `GenerateAccessTokenAsync` | Creates JWT header, builds payload via `CreateAccessTokenPayload`, signs and returns the token. |
| Payload | `CreateAccessTokenPayload` | Takes `resultClaims.AccessTokenClaims`, adds normalized claims and scope claims, wraps in `JwtPayload`. |

**Files:**

- `src/Identity/HCL.CS.SF.Identity.Application/Implementation/Endpoint/Services/TokenGenerationService.cs`
- `src/Identity/HCL.CS.SF.Identity.Domain/Models/Endpoint/ResultClaimsModel.cs`
- Claim type constants: `src/Identity/HCL.CS.SF.Identity.Domain/Constants/Endpoint/OpenIdConstants.cs` (`ClaimTypes`)

---

## What goes into the access token (fields/claims)

### 1. Normalized claims (every access token)

Added in `GetNormalizedClaimsList` and then included in `CreateAccessTokenPayload`:

| Claim | Constant | Description |
|-------|----------|-------------|
| `iss` | Issuer | Token issuer. |
| `iat` | IssuedAt | Issued-at time (Unix). |
| `nbf` | NotBefore | Not-before time (Unix). |
| `sub` | Sub | Subject: user id (or client id for client_credentials). |
| `exp` | Expiration | Expiry (Unix). |
| `jti` | JwtId | Unique token id. |
| `client_id` | ClientId | Client that received the token. |
| `idp` | IdentityProvider | Identity provider (user flows only). |
| `nonce` | Nonce | If present in request (e.g. auth code flow). |
| `auth_time` | AuthenticationTime | Auth time (user flows). |
| `sid` | SessionId | Session id (if any). |

### 2. Claims from `ResultClaimsModel.AccessTokenClaims`

`ResultClaimsModel.AccessTokenClaims` is a computed property that aggregates:

- IdentityTokenScopeClaims, RoleClaims, TransactionClaims, PermissionClaims, and **CustomAccessTokenClaims** (e.g. capabilities).

So the token gets:

- **Scope-related** (from identity/scope handling).
- **role** – from user's roles (when ApiResourceClaim/ApiScopeClaim type is `"role"`).
- **transaction** – transaction claims.
- **permission** – permissions derived from API resources/scopes and from UserClaims/RoleClaims (see below).
- **capabilities** – when configured, a dedicated claim with values from UserClaims/RoleClaims where claim type is `capabilities` (Option B, see below).

### 3. Scope claims

Scope claims are added in `CreateAccessTokenPayload`: each allowed scope is added as a `scope` claim; they are also typically represented as a single space-separated `scope` value.

### 4. Where role and permission values come from

In `GetClaimsFromApiResources`:

- **Claim types** that drive what goes into the token come from:
  - **ApiResourceClaimType** (on API resources)
  - **ApiScopeClaimType** (on API scopes)
- For each such claim type:
  - If the type is **`role`**: role names from the user's roles are added as **role** claims.
  - If the type is **`capabilities`**: values from UserClaims/RoleClaims with type `capabilities` are added as **capabilities** claims (dedicated claim, not merged into permission).
  - For any **other** type (e.g. `permission`, `locale`): values are taken from UserClaims and RoleClaims and emitted as **permission** claims.

---

## Can you customize token fields? (e.g. add a capabilities list)

**Yes**, in two ways:

### Option A – Use existing "permission" claims as capabilities

- On your **ApiResource** / **ApiScope**, set **ApiResourceClaimType** or **ApiScopeClaimType** to something like **permission**.
- Store the capability values in **UserClaims** or **RoleClaims** (e.g. claim type `permission`).
- The token will contain **permission** claims with those values. Your APIs can treat these as a capabilities list by reading the `permission` claims.

No code change required; configuration and data only.

### Option B – Add a dedicated "capabilities" claim in the token (implemented)

Option B is implemented. When you add claim type **capabilities** to an API Resource or API Scope and store capability values in UserClaims or RoleClaims with type **capabilities**, the access token will contain a separate **capabilities** claim (e.g. `"capabilities": ["read:users", "write:orders"]` in decoded form).

- **ResultClaimsModel** has **CustomAccessTokenClaims**; **capabilities** is filled there from UserClaims/RoleClaims and not converted to permission.
- **OpenIdConstants.ClaimTypes.Capabilities** = `"capabilities"`.

---

## Configuring the capabilities claim via Admin UI

To get a dedicated **capabilities** claim in the access token (e.g. `"capabilities": ["read:users", "write:orders"]`), do the following in the HCL.CS.SF Admin UI.

### 1. Add the claim type "capabilities" to an API Resource or API Scope

1. Go to **Admin → API Resources & Scopes** (or **Resources**).
2. Find the API resource that your client uses in its allowed scopes (e.g. the resource whose name is requested in the `scope` parameter at token time).
3. Click **Scopes** to expand the resource.
4. Under **Resource claim types**, type `capabilities` in the input and click **Add claim type**.  
   Alternatively, under **Scopes**, for a specific scope (e.g. `myapi.read`), add the claim type `capabilities` in that scope's **Claim types** column (input + **Add**).
5. Ensure the client that will request tokens has that resource (or scope) in its **Allowed scopes**. Otherwise the claim type is not evaluated for that client.

Result: when a token is issued and the requested scope includes this resource/scope, the token pipeline will look for claims with type **capabilities** on the user (from UserClaims and RoleClaims) and add them to the token as a separate **capabilities** claim (one JWT claim per value).

### 2. Add capability values to a role (recommended)

1. Go to **Admin → Roles & Claims**.
2. Select the role you assign to users (or create a role).
3. Under that role, in **Claim type** enter `capabilities` and in **Claim value** enter one capability per claim (e.g. `read:users`, then add another with `write:orders`).
4. Add as many capability claims as needed (each Claim type = `capabilities`, Claim value = one capability string).
5. Assign this role to the users who should receive these capabilities in their token.

Result: when those users get an access token with a scope that includes the resource/scope you configured in step 1, the token will contain a **capabilities** claim with those values (e.g. `["read:users", "write:orders"]` in decoded form).

### 3. Optional: add capabilities on the user directly

Instead of (or in addition to) role claims, you can add **User claims** with type `capabilities` and value per capability (if your admin UI exposes user claim management). Those will be merged with role capability claims into the token.

### Summary (Admin UI)

| Step | Where in Admin | Action |
|------|----------------|--------|
| 1 | **API Resources & Scopes** → expand resource | Add **claim type** `capabilities` to the resource or to a scope. |
| 2 | **Roles & Claims** → choose role | Add claims: **Claim type** = `capabilities`, **Claim value** = e.g. `read:users` (one claim per capability). |
| 3 | Assign the role to users; client has the resource/scope in allowed scopes | Request token with that scope → token will include `capabilities` with the list of values. |

---

## Summary

| Question | Answer |
|----------|--------|
| Where are access tokens generated? | `TokenGenerationService` → `GenerateAccessTokenAsync` → `CreateAccessTokenPayload` (and JWT signing). |
| Where are the fields/claims defined? | Normalized claims in `GetNormalizedClaimsList`; rest from `ResultClaimsModel.AccessTokenClaims` (roles, permissions, transaction, scopes, custom e.g. capabilities) and scope claims in `CreateAccessTokenPayload`. |
| Can you add custom token fields? | Yes: use **permission** claims as capabilities, or use the dedicated **capabilities** claim type (Option B), configured via API Resource/Scope claim types and Role (or User) claims in the Admin UI. |
