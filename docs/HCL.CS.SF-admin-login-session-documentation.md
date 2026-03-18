# HCL.CS.SF-Admin: Full Analysis — Login, Session, and Session Expiry

This document describes the HCL.CS.SF-Admin application: its structure, how login works (including with the Demo Server), how the session is maintained, and how session expiry is handled, with concrete implementation references.

---

## 1. HCL.CS.SF-Admin Overview

### 1.1 What It Is

**HCL.CS.SF-Admin** is a Next.js administrative console for the HCL.CS.SF identity platform. It allows administrators to manage users, roles, clients, API resources, identity resources, security tokens, and audit logs. It authenticates against HCL.CS.SF's token endpoint (typically served by **HCL.CS.SF.Demo.Server** or the Identity API) and uses the returned tokens for all API calls.

### 1.2 Tech Stack and Structure

| Layer | Technology | Location |
|-------|------------|----------|
| Framework | Next.js (App Router) | `HCL.CS.SF-admin/` |
| Auth | NextAuth.js (JWT strategy) | `lib/auth.ts`, `app/api/auth/[...nextauth]/route.ts` |
| Session | Encrypted JWT cookie | NextAuth default (`next-auth.session-token` / `__Secure-next-auth.session-token`) |
| Backend API | HCL.CS.SF Identity API / Demo Server | `HCL.CS.SF_ISSUER`, `HCL.CS.SF_API_BASE_URL`, `HCL.CS.SF_DEMO_SERVER_BASE_URL` |
| Types | Extended Session/JWT | `types/next-auth.d.ts` |

**Key directories:**

- **`app/(auth)/login/`** — Login page and forgot-password.
- **`app/admin/`** — All admin routes (dashboard, users, roles, clients, resources, operations, audit). Protected by middleware and layout.
- **`app/api/auth/`** — NextAuth route handler and federated-logout URL.
- **`lib/`** — Auth config (`auth.ts`), env (`env.ts`), OIDC discovery (`oidc.ts`), API client and routes.
- **`components/`** — UI: layout (Header, Sidebar), admin modules, shared UI.
- **`middleware.ts`** — Protects `/admin/*`, redirects unauthenticated or non-admin to `/login`.

### 1.3 Environment Configuration

Authentication and session behaviour depend on these variables (see `HCL.CS.SF-admin/.env.example`):

| Variable | Purpose |
|----------|---------|
| `NEXTAUTH_URL` | Public URL of the admin app (e.g. `https://admin.localhost:3001`). |
| `NEXTAUTH_SECRET` | Secret for signing/encrypting the JWT session cookie. **Required.** |
| `HCL.CS.SF_ISSUER` | HCL.CS.SF identity issuer base URL (e.g. `https://localhost:5001`). |
| `HCL.CS.SF_CLIENT_ID` / `HCL.CS.SF_CLIENT_SECRET` | OAuth client credentials for the admin client in HCL.CS.SF. **Required.** |
| `HCL.CS.SF_API_BASE_URL` | Base URL for `/Security/Api/*` calls (defaults to issuer). |
| `HCL.CS.SF_DEMO_SERVER_BASE_URL` | Demo Server base URL for health check and Google sign-in redirect. Defaults to issuer. |
| `HCL.CS.SF_SCOPES` | Space-separated scopes requested at login. |
| `HCL.CS.SF_TOKEN_ENDPOINT` / `HCL.CS.SF_REVOCATION_ENDPOINT` | Optional; otherwise from OIDC discovery. |
| `HCL.CS.SF_POST_LOGOUT_REDIRECT_URI` | Where to redirect after federated end-session. |
| `HCL.CS.SF_ENABLE_FEDERATED_LOGOUT` | If `true`, logout redirects to HCL.CS.SF end_session. |
| `NEXT_PUBLIC_GOOGLE_LOGIN_ENABLED` | If `true`, shows "Sign in with Google" (requires Demo Server + user_code grant). |

---

## 2. Login for Demo Server — Implementation

Login can happen in three ways: **username/password** (resource owner password grant), **user code** (one-time code, e.g. after Google sign-in on Demo Server), and **Google** (redirect to Demo Server, which redirects back with a user code).

### 2.1 Login Page Entry Point

**File:** `HCL.CS.SF-admin/app/(auth)/login/page.tsx`

- Renders the login form (username, password), "Sign in with Google" button, and "Forgot password?" link.
- Uses `useSession()` to detect authenticated state and redirect to `callbackUrl` when already signed in.
- Handles `callbackUrl`, `error`, and `reason=admin_required` from the URL.
- **UserCode in URL:** If the URL has `UserCode` (e.g. after Google callback from Demo Server), the page automatically calls `signIn("credentials", { code: userCode, callbackUrl, redirect: false })` once (guarded by `userCodeAttempted` ref).

### 2.2 Username/Password Login (Password Grant)

**Flow:**

1. User enters username and password and clicks "Sign in".
2. `startLogin()` calls `signIn("credentials", { username, password, callbackUrl, redirect: false })`.
3. NextAuth invokes the **CredentialsProvider** `authorize()` in `lib/auth.ts` with `credentials.username` and `credentials.password`.
4. **`requestPasswordGrantTokens(userName, password)`** in `lib/auth.ts`:
   - Resolves token endpoint via `resolveTokenEndpoint()` (`lib/oidc.ts`): `HCL.CS.SF_TOKEN_ENDPOINT` or discovery `token_endpoint` or `${issuer}/security/token`.
   - POSTs to the token endpoint with `grant_type=password`, `username`, `password`, client authentication (Basic or `client_id`/`client_secret` in body), and optional `scope`.
   - Tries several combinations (scope vs no scope, client secret in body vs Basic) until one succeeds or `invalid_grant` (then stops).
5. On success, **`createAuthenticatedUser()`** builds an object with `accessToken`, `refreshToken`, `idToken`, `accessTokenExpires` (from `expires_in`), `roles` (from token), `isAdmin` (from roles).
6. NextAuth **`jwt()`** callback stores this in the JWT; **`session()`** callback copies it into the session.
7. Login page redirects to `callbackUrl` (e.g. `/admin/clients`).

**Relevant code:**

- `lib/auth.ts`: `authorize()`, `requestPasswordGrantTokens()`, `executePasswordGrantAttempt()`, `createAuthenticatedUser()`.

### 2.3 User-Code Login (e.g. After Google on Demo Server)

Used when the user returns from Demo Server with a one-time verification code in the query string (e.g. `/login?UserCode=...`).

**Flow:**

1. Demo Server (e.g. after Google callback) creates a verification code and redirects to `returnUrl` with that code. The URL is built by **`InteractionService.ConstructUserVerificationCode(returnUrl, verificationCode)`** in the Identity layer, which appends the code as the `UserCode` query parameter (see `AuthenticationConstants.AuthCodeStore.UserVerificationCode`).
2. Admin login page reads `UserCode` from `searchParams` and, when `status === "unauthenticated"`, calls `signIn("credentials", { code: userCode, callbackUrl, redirect: false })` once.
3. **CredentialsProvider** `authorize()` receives `credentials.code`. In `lib/auth.ts`:
   - **`requestUserCodeGrantTokens(userCode)`** POSTs to the same token endpoint with `grant_type=user_code`, `user_code=<code>`, `client_id`, `client_secret`, and optional `scope`.
   - HCL.CS.SF's token endpoint must support the `user_code` grant (e.g. via `UserCodeFlowSpecification` and `TokenRequestValidator` handling `OpenIdConstants.GrantTypes.UserCode`).
4. On success, the same `createAuthenticatedUser()` and JWT/session flow run; user is redirected to `callbackUrl`.

**Relevant code:**

- `lib/auth.ts`: `authorize()` (branch `credentials?.code`), `requestUserCodeGrantTokens()`.
- Demo Server: `ExternalAuthService.CompleteGoogleCallbackAsync()` → `ConstructUserVerificationCode(returnUrl, verificationCode)`; `AccountController` for password login also uses `ConstructUserVerificationCode`.
- Identity: `UserCodeFlowSpecification`, `TokenRequestValidator` (user_code branch), `InteractionService.ConstructUserVerificationCode()`.

### 2.4 "Sign in with Google" (Redirect to Demo Server)

**Flow:**

1. User clicks "Sign in with Google" on the login page.
2. The button sets `window.location.href` to:
   - `{demoServerBaseUrl}/auth/external/google/start?returnUrl={encodeURIComponent(origin + '/login')}`
   - So after Google sign-in, Demo Server redirects back to the admin app's `/login` with optional query params (Demo Server adds `UserCode` via `ConstructUserVerificationCode`).
3. Demo Server handles `/auth/external/google/start` (ExternalAuthController), challenges with Google, then on callback (`/auth/external/google/callback`) completes sign-in, creates a verification code, and redirects to `returnUrl?UserCode=...`.
4. Admin login page loads with `UserCode` in the URL and runs the **user-code** flow above, exchanging the code for tokens and establishing the session.

**Relevant code:**

- `app/(auth)/login/page.tsx`: Google button `onClick` using `env.demoServerBaseUrl` and `returnUrl = origin + '/login'`.
- Demo Server: `ExternalAuthController.GoogleStart`, `ExternalAuthController.GoogleCallback`, `ExternalAuthService.CompleteGoogleCallbackAsync()` → `ConstructUserVerificationCode(returnUrl, verificationCode)`.

### 2.5 Admin Role Requirement

- **Middleware** (`middleware.ts`): For every request to `/admin/:path*`, reads the JWT via `getToken()`. If there is no token, redirects to `/login?callbackUrl=...`. If the token's roles do not include an "admin" role (`isAdminRole(roles)`), redirects to `/login?reason=admin_required&callbackUrl=...`.
- **Admin layout** (`app/admin/layout.tsx`): Calls `auth()` (getServerSession). If no session, redirects to `/login`. If session has no admin role (`hasAdminRole(session.roles)`), renders a 403 "Access denied" page.
- Login page: If `reason=admin_required`, shows a message that the session does not have administrator access; if already authenticated with that reason, it calls `signOut({ redirect: false })` so the user can sign in with an admin account.

---

## 3. How Session Is Maintained — Implementation

### 3.1 Strategy and Storage

- **Strategy:** NextAuth with **JWT strategy** (no database session).
- **Storage:** Session data is stored in an **encrypted JWT cookie** (name `next-auth.session-token` in dev, `__Secure-next-auth.session-token` in production with HTTPS).
- **JWT contents:** Populated by the **`jwt()`** callback in `lib/auth.ts` and exposed to the app via the **`session()`** callback. Extended types are in `types/next-auth.d.ts`.

### 3.2 What Is Stored in the Session/JWT

From **`lib/auth.ts`** and **`types/next-auth.d.ts`**:

| Field | Description |
|-------|-------------|
| `accessToken` | HCL.CS.SF access token (Bearer) for API calls. |
| `refreshToken` | Used to refresh the access token when it is near expiry. |
| `idToken` | Optional; used for federated logout `id_token_hint`. |
| `accessTokenExpires` | Timestamp (ms) when the access token expires. |
| `scopes` | Granted scope string. |
| `roles` | Array of role strings from the token. |
| `isAdmin` | Derived from roles (true if any role contains "admin"). |
| `error` | Set when refresh fails (e.g. `RefreshAccessTokenError`). |
| Standard NextAuth | `user` (id, name, email), etc. |

### 3.3 When the JWT Is Updated

**`jwt()` callback** in `lib/auth.ts`:

1. **On sign-in:** When `account?.provider === "credentials"` and `user` is present, the full authenticated user (tokens, expiry, roles, isAdmin) is written into the token.
2. **On subsequent requests:**
   - If `accessToken` exists and `accessTokenExpires` is set and **`Date.now() < accessTokenExpires - 60_000`** (1-minute buffer), the existing token is returned (no refresh).
   - Otherwise **`refreshAccessToken(token)`** is called: POST to the token endpoint with `grant_type=refresh_token` and `refresh_token`. On success, the JWT is updated with the new `access_token`, `expires_in`, and optionally new `refresh_token`/`id_token`; roles are re-derived from the new access token. On failure, `token.error` is set (e.g. `RefreshAccessTokenError`).

So the session is "maintained" by:
- Storing HCL.CS.SF tokens in the JWT cookie.
- Refreshing the access token automatically when it is within 1 minute of expiry, using the refresh token.

### 3.4 Where Session Is Used

| Use | Implementation |
|-----|----------------|
| **Protecting routes** | `middleware.ts`: `getToken()` for `/admin/*`; redirect if no token or not admin. |
| **Server components / layout** | `auth()` in `app/admin/layout.tsx` and root `app/layout.tsx` (passes session to `Providers`). |
| **API calls** | `requireAccessToken()` in `lib/api/client.ts`: calls `auth()` and returns `session.accessToken`; used by `HCL.CS.SFPostWithSession`, `HCL.CS.SFGetWithSession` and all server-side API modules. |
| **Client components** | `useSession()` from `next-auth/react` (e.g. Header, login page). |
| **Session provider** | `components/providers.tsx`: `SessionProvider` wraps the app with the server-fetched session. |

### 3.5 Token Endpoint Resolution

**`lib/oidc.ts`:**

- `resolveTokenEndpoint()`: Uses `HCL.CS.SF_TOKEN_ENDPOINT` if set; otherwise OIDC discovery from `HCL.CS.SF_METADATA_ADDRESS` or `{issuer}/.well-known/openid-configuration`; fallback `{issuer}/security/token`.
- Same base is used for password, user_code, and refresh_token grants.

---

## 4. Session Expiry Handling — Implementation

### 4.1 Proactive Refresh (Before Expiry)

- **Where:** `lib/auth.ts`, **`jwt()`** callback.
- **Logic:** On each request that runs the callback, if the access token exists but **`Date.now() >= accessTokenExpires - 60_000`**, the code calls **`refreshAccessToken(token)`**.
- **Effect:** As long as the user keeps making requests, the access token is refreshed before it expires. If the app is idle longer than the access token lifetime, the next request may still send an expired token once; the next refresh attempt can then succeed or fail (see below).

### 4.2 When Refresh Fails

- **`refreshAccessToken()`** in `lib/auth.ts`: If the refresh request fails (e.g. 4xx, no `access_token`, or refresh token revoked/expired), the callback returns the token with `error` set (e.g. `RefreshAccessTokenError` or the server's `error_description`).
- **`session()`** callback copies `token.error` into **`session.error`**.
- The session remains "authenticated" from NextAuth's perspective (JWT still present), but the access token may be missing or stale. The next API call that uses `requireAccessToken()` will either use a bad token or, if the token was cleared, can throw (see below). In practice, **API 401** is the main trigger for the "session expired" UX.

### 4.3 API 401 (Unauthorized)

- **`lib/api/client.ts`:**
  - **`requireAccessToken()`** throws **`HCL.CS.SFApiError`** with status `401` if there is no session or no `session.accessToken`.
  - All HCL.CS.SF API calls (e.g. `HCL.CS.SFPost`, `HCL.CS.SFGet`) that get a 401 from the server throw `HCL.CS.SFApiError` with `statusCode: 401`.
- **`apiErrorMessage(error)`:** For `HCL.CS.SFApiError` with status 401, returns **"Session expired. Please sign in again."**
- **`getLoadErrorInfo(error)`:** Returns `{ message, isUnauthorized }`; `isUnauthorized` is true when the error is a 401 `HCL.CS.SFApiError` (via **`isUnauthorizedError()`**).

So "session expiry" is surfaced when:
- The server returns 401, or
- The session has no access token (e.g. after a failed refresh).

### 4.4 UI When Session Is Expired or Invalid

- **Admin dashboard** (`app/admin/page.tsx`): Loads data with `listUsers`, `listRoles`, `listClientNames`, `searchAudit`. On catch, uses `getLoadErrorInfo(error)`. If there is an error, shows an error card with `loadErrorMessage` and, if `isUnauthorized`, a **`<SignInAgainButton />`** and a "Retry" link.
- **List modules** (e.g. Clients, Users, Roles, Resources, Identity Resources, Audit): When their data fetch throws, they show an error state and **`<SignInAgainButton />`** so the user can clear the session and sign in again.

### 4.5 SignInAgainButton (Clearing Stale Session)

A plain link to `/login` would not work when the session is "expired" but the JWT cookie is still present: NextAuth would still report `status === "authenticated"` and the login page would redirect back to admin, which would hit 401 again.

**`components/admin/SignInAgainButton.tsx`:**

1. Calls **`logoutAction()`** (server action): HCL.CS.SF SignOut API and revoke access + refresh tokens. Errors are ignored so the client always proceeds.
2. Calls **`signOut({ redirect: false })`** to clear the NextAuth JWT cookie.
3. Sets **`window.location.href = "/login"`** for a full reload so the login page sees a clean, unauthenticated state.

So "session expiry" is handled by:
- Detecting 401 (or missing token) from API calls.
- Showing "Session expired. Please sign in again." and **SignInAgainButton**.
- SignInAgainButton forcing a full logout (HCL.CS.SF + NextAuth) and a full navigation to `/login`.

### 4.6 Logout and Revocation

- **`app/actions/logout.ts` — `logoutAction()`:**
  - Gets session via `auth()`.
  - If session has tokens: calls HCL.CS.SF SignOut API (`signOut()`), then revokes access token and, if present, refresh token via **`lib/api/revocation.ts`** (`revokeToken`).
  - All errors are caught so the client can always clear the session.
- **Header logout** (`components/layout/Header.tsx` — `doLogout()`):
  - Optionally fetches federated logout URL from `/api/auth/federated-logout-url`.
  - Calls `logoutAction()`.
  - In `finally`, calls `signOut({ redirect: false })` and then either redirects to the federated end_session URL or `router.push('/login')`.

So both "Sign in again" (expired session) and normal "Logout" ensure the HCL.CS.SF session and tokens are revoked when possible, and the NextAuth cookie is always cleared.

---

## 5. Summary Table (Implementation Reference)

| Topic | Implemented in |
|-------|----------------|
| **Login (password)** | `lib/auth.ts` — `authorize()`, `requestPasswordGrantTokens()`, `createAuthenticatedUser()`; `app/(auth)/login/page.tsx` — `startLogin()`. |
| **Login (user code)** | `lib/auth.ts` — `authorize()` (code branch), `requestUserCodeGrantTokens()`; login page — effect with `UserCode` from URL. |
| **Login (Google)** | Login page — redirect to Demo Server `/auth/external/google/start?returnUrl=...`; Demo Server callback redirects to `/login?UserCode=...`. |
| **Token endpoint** | `lib/oidc.ts` — `resolveTokenEndpoint()`; discovery or env. |
| **Session storage** | NextAuth JWT cookie; `lib/auth.ts` — `jwt()` and `session()` callbacks; `types/next-auth.d.ts`. |
| **Session refresh** | `lib/auth.ts` — `jwt()` (expiry check), `refreshAccessToken()`. |
| **Using session in API** | `lib/api/client.ts` — `requireAccessToken()` → `auth()` → `session.accessToken`. |
| **Route protection** | `middleware.ts` — token and admin role; `app/admin/layout.tsx` — `auth()`, `hasAdminRole()`. |
| **Session expiry (refresh)** | `lib/auth.ts` — 1-minute buffer, `refreshAccessToken()`; on failure, `session.error` set. |
| **Session expiry (401)** | `lib/api/client.ts` — 401 → `HCL.CS.SFApiError`, `apiErrorMessage()`, `getLoadErrorInfo()`, `isUnauthorizedError()`. |
| **Session expiry (UI)** | `app/admin/page.tsx`, list modules — error card + **`SignInAgainButton`** when `isUnauthorized`. |
| **Sign in again** | `components/admin/SignInAgainButton.tsx` — `logoutAction()`, `signOut()`, `window.location.href = '/login'`. |
| **Logout** | `app/actions/logout.ts` — `logoutAction()`; `components/layout/Header.tsx` — `doLogout()`; optional federated logout via `app/api/auth/federated-logout-url/route.ts`. |

---

## 6. Related Documentation

- **Session and auth integration (including logout):** `HCL.CS.SF-admin/docs/session-and-auth-integration.md`
- **Google login plan:** `docs/auth/google-login-HCL.CS.SF-admin-plan.md`
- **Demo Server debugging:** `docs/HCL.CS.SF.Demo.Server_Debugging_Guide.md`

This document and the above together give a full picture of HCL.CS.SF-Admin authentication, Demo Server login (including Google), session maintenance, and session expiry handling with implementation pointers.
