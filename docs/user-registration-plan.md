# User registration: how users register to the server and what ways are available

This document describes **all ways** a user can be created/registered on the HCL.CS.SF server, the code paths involved, and how each method works.

---

## 1. Overview: ways to register / create users

| # | Method | Auth required? | Where used | Creates |
|---|--------|----------------|------------|--------|
| 1 | **Register User API** (self-registration or admin) | **No** (anonymous API) | Any client (e.g. Demo MVC/WPF, HCL.CS.SF-admin if UI added) | Local user, default role, optional security questions/claims; email/phone unconfirmed by default |
| 2 | **External (Google) sign-in with auto-provisioning** | No (OAuth flow) | Demo.Server only | Local user from Google identity (email, name), default role, ExternalIdentities link; no password |
| 3 | **LDAP first login** | No (LDAP bind) | When LDAP is enabled | Local user created/synced from LDAP attributes; email/phone confirmed; IdentityProvider = Ldap |
| 4 | **Admin / API: UpdateUser** | Yes (token) | HCL.CS.SF-admin (edit existing user) | N/A (updates only) |

There is **no** “Create User” (admin-only) API that is separate from Register User: the same **RegisterUser** API is used both for self-registration (no token) and for an admin creating a user (with token). The Gateway treats RegisterUser as **anonymous**, so it can be called without a token.

---

## 2. Register User API (self-registration or admin-created user)

### 2.1 Flow

1. Client sends **POST** to Gateway: `/Security/Api/User/RegisterUser` with body = `UserModel` (JSON).
2. Gateway route: `UserAccountServiceRoute.RegisterUser` → deserializes `UserModel` → calls `UserAccountService.RegisterUserAsync(userModel)`.
3. **Proxy:** `UserAccountProxyServices.RegisterUserAsync` runs `apiValidator.ValidateRequest()`. Because **RegisterUserAsync** is in `ProxyConstants.AnonymousApis`, the validator **succeeds without a token**. So the API is **callable without authentication** (self-registration).
4. **Identity:** `UserAccountService.RegisterUserAsync` (in Identity application layer):
   - Validates: `ValidateUser`, `ValidatePassword`, `ValidateUserSecurityQuestion`, `ValidateUserClaims` (all with `isFromRegister: true`).
   - Checks user does not already exist (`FindUserByUserName`).
   - `SetDefaultValuesForCreate(user)` (e.g. sets EmailConfirmed/PhoneNumberConfirmed from config, clears lockout).
   - Maps `UserModel` → `Users`, calls `userManager.CreateAsync(usersEntity, user.Password)`.
   - On success: adds to unit of work, password history, user security questions, user claims, assigns **default HCL.CS.SF role** from `SystemSettings.UserConfig.DefaultUserRole`, saves.

### 2.2 Key files

| Layer | File |
|-------|------|
| Route path | `ApiRoutePathConstants.RegisterUser` = `"/Security/Api/User/RegisterUser"` |
| Gateway route | `src/Gateway/HCL.CS.SF.Gateway/Routes/UserAccountServiceRoute.cs` → `RegisterUser` |
| Gateway proxy | `src/Gateway/HCL.CS.SF.Gateway/Proxy/UserAccountProxyServices.cs` → `RegisterUserAsync` (anonymous) |
| Service | `src/Identity/HCL.CS.SF.Identity.Application/Implementation/Api/Services/UserAccountService.cs` → `RegisterUserAsync` |
| Validators | Same file: `ValidateUser`, `ValidatePassword`, `ValidateUserSecurityQuestion`, `ValidateUserClaims` |
| Anonymous list | `src/Identity/HCL.CS.SF.Identity.Domain/Constants/ProxyConstants.cs` → `AnonymousApis` includes `"RegisterUserAsync"` |

### 2.3 Required / validated input (registration)

- **UserName:** Required; length between `UserConfig.MinUserNameLength` and `UserConfig.MaxUserNameLength`.
- **FirstName:** Required; length between `MinFirstAndLastNameLength` and `MaxFirstAndLastNameLength`.
- **LastName:** Optional; if present, same length limits.
- **Email:** Required; valid format; length ≤ 255.
- **PhoneNumber:** Optional; if present, valid format and length (min/max from config).
- **Password:** Required; no spaces; length ≤ `PasswordConfig.MaxPasswordLength`.
- **UserSecurityQuestion:** If `UserConfig.MinNoOfQuestions > 0` and identity provider is Local: at least that many questions, no duplicate question IDs, non-empty answers; answer length ≥ `MinSecurityAnswersLength` if set.
- **UserClaims:** If present, validated (e.g. type/value).
- **TwoFactorEnabled / TwoFactorType:** If 2FA enabled, type must not be None.
- **CreatedBy:** Optional; if present, length ≤ 255.
- **DateOfBirth:** If present, validated against `MinDOBYear` / `MaxDOBYear`.

### 2.4 Defaults set on create

- `EmailConfirmed` = `!RequireConfirmedEmail`, `PhoneNumberConfirmed` = `!RequireConfirmedPhoneNumber` (from `UserConfig`).
- `LockoutEnd` = null, `LockoutEnabled` = false, `AccessFailedCount` = 0, `LastPasswordChangedDate` / `LastLoginDateTime` / `LastLogoutDateTime` = null.

### 2.5 Who can call it

- **Without token:** Yes (anonymous). Typical for self-registration (e.g. Demo MVC Register page, WPF Register screen).
- **With token:** Yes. Can be used by HCL.CS.SF-admin or other back-office apps to “create” a user by calling the same API (admin would send a full `UserModel` including CreatedBy).

### 2.6 Clients that use it

- **Demo MVC:** `AccountController.Register` (POST) builds `UserModel` from `RegisterViewModel` and calls `httpService.PostAsync<FrameworkResult>(ApiRoutePathConstants.RegisterUser, user)`.
- **Demo WPF:** `RegisterUserViewModel` calls the RegisterUser API.
- **HCL.CS.SF-admin:** `lib/api/users.ts` exposes `registerUser(user)` but there is **no** “Create User” UI/action that calls it yet; only list/update/lock/unlock/roles are implemented.

---

## 3. External (Google) sign-in with auto-provisioning

### 3.1 When this creates a user

Used only in **HCL.CS.SF.Demo.Server** (demo host that uses external auth). When a user signs in with Google and:

- There is **no** existing link for that Google identity (`ExternalIdentities`), and
- There is **no** existing local user by email (and tenant), and
- **Auto-provisioning** is enabled and the email domain is allowed,

then the server **creates a new local user** and links the Google identity to it.

### 3.2 Flow

1. User starts Google sign-in → Demo.Server redirects to Google; after callback, `ExternalAuthService.CompleteGoogleCallbackAsync` runs.
2. For **non–link** flow: `ResolveOrCreateUserAsync(existingLink, provider, providerKey, payload, tenantId)`.
3. If no existing link and no existing user by email (and tenant): `CanAutoProvision(payload.Email, tenantId)` is checked (config: `AutoProvisionEnabled` + allowed domains per tenant or global).
4. If allowed: `AutoProvisionUserAsync(payload, tenantId)`:
   - Builds unique username from email (`BuildUniqueUserNameAsync`).
   - Creates `Users` with Email, FirstName, LastName from Google, `IdentityProviderType = Google`, no password (uses a generated password internally for `CreateAsync`).
   - `userManager.CreateAsync(user, CreateGeneratedPassword())`, then adds default role, tenant claim, identity provider claim.
5. Then `EnsureLinkedAsync` adds the `ExternalIdentities` row and ASP.NET Identity external login.

### 3.3 Key files

| Component | File |
|-----------|------|
| Service | `demos/HCL.CS.SF.Demo.Server/Services/ExternalAuth/ExternalAuthService.cs` |
| Methods | `CompleteGoogleCallbackAsync`, `ResolveOrCreateUserAsync`, `CanAutoProvision`, `AutoProvisionUserAsync` |
| Config | `ExternalAccountOptions` (e.g. `AutoProvisionEnabled`, `AllowedDomains`, `AllowedDomainsByTenant`) |

### 3.4 No password / no RegisterUser API

This path does **not** call `RegisterUserAsync`. It creates the user directly via `UserManager.CreateAsync` with a generated password and then adds role/claims. So it bypasses Register User validation (security questions, etc.).

---

## 4. LDAP first login (create/sync local user)

### 4.1 When this creates a user

When **LDAP is enabled** and a user authenticates with LDAP for the **first time**, the server may **create a local user** from LDAP attributes. On subsequent logins it may **sync** (update) that user.

### 4.2 Flow

1. Client calls an endpoint that uses `LdapUtil.LdapLoginAsync(username, password)` (or equivalent).
2. LDAP bind/search returns user attributes (e.g. mail, name, phone).
3. `LdapUtil.CreateLdapUser`:
   - If no local user with that username: `CreateLdapUserAsync` builds a `UserModel` from LDAP data (`AssignModel`), sets `EmailConfirmed = true`, `PhoneNumberConfirmed = true`, `IdentityProviderType = IdentityProvider.Ldap`, then calls **`userAccountService.RegisterUserAsync(userModel)`**.
   - If local user exists: `SyncLdapUserAsync` updates the existing user via `UpdateUserAsync`.

So **LDAP user creation goes through the same RegisterUserAsync** as the API, but with a server-built model (no security questions required in the same way; validation still runs).

### 4.3 Key files

| Component | File |
|-----------|------|
| LDAP util | `src/Identity/HCL.CS.SF.Identity.Application/Implementation/Api/Utils/LdapUtil.cs` |
| Methods | `LdapLoginAsync`, `CreateLdapUser`, `CreateLdapUserAsync`, `SyncLdapUserAsync`, `AssignModel` |

### 4.4 Documentation

- `src/Identity/HCL.CS.SF.Identity.Application/Implementation/Documentation/Main/LDAPIntegration.cs`: describes that LDAP bypasses “user registration and verification process” from the end-user’s perspective; a local account is created from LDAP and marked verified.

---

## 5. Summary table (registration paths)

| Path | Entry point | Auth | Creates user via | Typical use |
|------|-------------|------|-------------------|-------------|
| **Register User API** | POST `/Security/Api/User/RegisterUser` | Anonymous (no token) | `UserAccountService.RegisterUserAsync` | Self-registration, or admin creating user (if UI exists) |
| **Google auto-provision** | Demo.Server Google callback | OAuth (no HCL.CS.SF token) | `UserManager.CreateAsync` + role/claims (no RegisterUser) | First-time Google sign-in when domain allowed |
| **LDAP first login** | LDAP login (e.g. LdapUtil) | LDAP bind | `UserAccountService.RegisterUserAsync` (model built from LDAP) | First LDAP login creates local user |

---

## 6. Recommendations

1. **Self-registration:** Use **POST /Security/Api/User/RegisterUser** with a full `UserModel` (username, password, email, first/last name, optional security questions/claims). No Bearer token required.
2. **Admin “Create User” in HCL.CS.SF-admin:** Reuse the same **RegisterUser** API; add a “Create User” form that builds `UserModel` and calls `registerUser(user)` (already in `lib/api/users.ts`). Ensure required fields and validation align with server (see section 2.3).
3. **Google users:** Rely on Demo.Server’s external auth and auto-provisioning when enabled; no separate “registration” call.
4. **LDAP users:** No explicit registration; first successful LDAP login creates/syncs the local user via the existing LDAP + RegisterUser path.
