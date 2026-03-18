# HCL.CS.SF-Admin: Session, Login, and Logout Integration

## 1. How session is maintained

### 1.1 Strategy

- **NextAuth** with **JWT strategy** (no database session).
- Session data lives in an **encrypted JWT cookie** (name `next-auth.session-token` in dev, `__Secure-next-auth.session-token` in prod with HTTPS).
- The JWT holds: `accessToken`, `refreshToken`, `idToken`, `accessTokenExpires`, `roles`, `isAdmin`, etc. (see [lib/auth.ts](lib/auth.ts) `jwt` and `session` callbacks).

### 1.2 Flow (high level)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  Browser request (e.g. load /admin/clients)                                  │
└─────────────────────────────────────────────────────────────────────────────┘
                                        │
                                        ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│  NextAuth: read JWT cookie → jwt() callback runs                            │
│  • If account/user present (first sign-in): store tokens in JWT.            │
│  • Else if accessTokenExpires - 60s > now: return token (no refresh).        │
│  • Else: refreshAccessToken(token) → POST HCL.CS.SF /security/token            │
│          (grant_type=refresh_token) → update JWT with new access_token.      │
└─────────────────────────────────────────────────────────────────────────────┘
                                        │
                                        ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│  session() callback: copy token fields → session (accessToken, roles, etc.)  │
└─────────────────────────────────────────────────────────────────────────────┘
                                        │
                                        ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│  getServerSession(authOptions) returns session; API calls use                 │
│  session.accessToken in Authorization: Bearer <token>.                      │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 1.3 Where it’s implemented

| Concern | Location |
|--------|----------|
| JWT contents & refresh decision | [lib/auth.ts](lib/auth.ts) – `jwt()` callback |
| Refresh token request to HCL.CS.SF | [lib/auth.ts](lib/auth.ts) – `refreshAccessToken()` |
| Session shape exposed to app | [lib/auth.ts](lib/auth.ts) – `session()` callback |
| Token endpoint URL | [lib/oidc.ts](lib/oidc.ts) – `resolveTokenEndpoint()` (env or discovery) |
| Using session in API calls | [lib/api/client.ts](lib/api/client.ts) – `requireAccessToken()` → `auth()` → `session.accessToken` |

### 1.4 Session correctness

- **Correct:** Login stores HCL.CS.SF tokens in the JWT; refresh runs when the access token is near expiry (1 min buffer); session is available server-side via `getServerSession`; all API calls use `session.accessToken`.
- **Caveat:** If no request hits the server for longer than the access token lifetime, the next request may still send an expired token (refresh runs on that request but the first call can already be 401). UI handles 401 with “Session expired” and “Sign in again”.

### 1.5 "Sign in again" when session has expired

When an API returns 401 (e.g. tokens expired and refresh failed), list pages show an error card with **"Sign in again"** and **"Retry"**. A plain link to `/login` would not work: NextAuth still has the stale session in the cookie, so the login page sees `status === "authenticated"` and immediately redirects back to admin, which fails again with 401.

**Fix:** [components/admin/SignInAgainButton.tsx](../components/admin/SignInAgainButton.tsx) clears the session before navigating to login:

1. Call `logoutAction()` (HCL.CS.SF SignOut + revoke tokens); errors are ignored if the session is already invalid.
2. Call NextAuth `signOut({ redirect: false })` to clear the JWT cookie.
3. Set `window.location.href = "/login"` for a full page load so the login page receives a clean state with no "authenticated" session.

All modules that show a session-expired error (Clients, Users, Roles, Resources, Identity Resources, Audit) use `<SignInAgainButton />` instead of `<Link href="/login">`.

---

## 2. Login integration with the server

### 2.1 Flow

1. User submits username/password on [app/(auth)/login/page.tsx](app/(auth)/login/page.tsx).
2. Client calls `signIn("credentials", { username, password, callbackUrl, redirect: false })`.
3. NextAuth runs **CredentialsProvider** `authorize()` in [lib/auth.ts](lib/auth.ts):
   - `requestPasswordGrantTokens(userName, password)`:
     - Resolves token URL via `resolveTokenEndpoint()` (env `HCL.CS.SF_TOKEN_ENDPOINT` or discovery `token_endpoint` or `${issuer}/security/token`).
     - POSTs to HCL.CS.SF **token endpoint** with `grant_type=password`, `username`, `password`, client auth (Basic or client_id/client_secret in body), optional `scope`.
   - On success: `createAuthenticatedUser()` builds user with `accessToken`, `refreshToken`, `idToken`, `accessTokenExpires`, `roles`, `isAdmin`.
4. NextAuth `jwt()` callback stores that user in the JWT.
5. `session()` callback copies token and role data into the session object.
6. Login page redirects to `callbackUrl` (e.g. `/admin/clients`).

### 2.2 Server endpoints used

| Purpose | Endpoint (HCL.CS.SF) | Used by |
|--------|--------------------|--------|
| Token (password grant) | `POST /security/token` (or env) | `requestPasswordGrantTokens()` |
| Token (refresh) | Same | `refreshAccessToken()` |

Discovery is optional: [lib/oidc.ts](lib/oidc.ts) `getDiscoveryDocument()` from `HCL.CS.SF_METADATA_ADDRESS` or `{issuer}/.well-known/openid-configuration`.

### 2.3 Login integration correctness

- **Correct:** Login is fully integrated with the HCL.CS.SF server. Credentials are validated by HCL.CS.SF; tokens (access, refresh, id) and expiry come from the server and are stored in the JWT and session. Roles are derived from the access token and used for admin checks in [app/admin/layout.tsx](app/admin/layout.tsx).

---

## 3. Logout integration with the server

### 3.1 Current behaviour

- **Header logout** ([components/layout/Header.tsx](components/layout/Header.tsx)):
  - Calls `signOut({ redirect: false })` (NextAuth).
  - Then `router.push("/login")`.
- NextAuth clears the JWT cookie and in-memory session. The browser is sent to `/login`.

### 3.2 What is **not** done today

- **HCL.CS.SF SignOut API**  
  [lib/api/authentication.ts](lib/api/authentication.ts) exposes `signOut()` which POSTs to **`/Security/Api/Authentication/SignOut`**. This is **never called** from the UI or any logout flow. So the server is not told “this admin session is logging out”.

- **Federated logout (end-session)**  
  [app/api/auth/federated-logout-url/route.ts](app/api/auth/federated-logout-url/route.ts) builds a HCL.CS.SF **end_session** URL (with `client_id`, `post_logout_redirect_uri`, optional `id_token_hint`). This route is **never called** from the app. So we never redirect the user to HCL.CS.SF’s end_session endpoint to clear server-side OIDC session.

- **Token revocation**  
  On logout we do **not** revoke the current access or refresh token via HCL.CS.SF’s revocation endpoint. The tokens remain valid until they expire (or until revoked elsewhere, e.g. Revocation UI).

### 3.3 Logout integration correctness

- **Partially correct:** From the browser’s perspective the user is “logged out” (no session cookie, sent to login). From the server’s perspective the refresh token (and any server-side session) are still valid until expiry or explicit revocation elsewhere.
- **Gap:** Logout is **not** fully integrated with the server: no SignOut API call, no end_session redirect, no revocation of the current session’s tokens.

---

## 4. Summary table

| Area | Integrated with server? | Notes |
|------|--------------------------|--------|
| **Session storage** | N/A (JWT in cookie) | No server session store; JWT holds tokens. |
| **Session refresh** | Yes | Refresh token grant to HCL.CS.SF token endpoint. |
| **Login** | Yes | Password grant to HCL.CS.SF token endpoint; tokens and roles stored. |
| **Logout** | No | Only NextAuth signOut + redirect. No SignOut API, no end_session, no revocation. |

---

## 5. Logout improvements (implemented)

1. **HCL.CS.SF SignOut and token revocation**  
   [app/actions/logout.ts](app/actions/logout.ts) defines `logoutAction()`: it calls HCL.CS.SF `signOut()` (POST `/Security/Api/Authentication/SignOut`) and then revokes the current access and refresh tokens via the revocation endpoint. Errors are swallowed so the client always proceeds to clear the session.

2. **Federated logout**  
   When `HCL.CS.SF_ENABLE_FEDERATED_LOGOUT=true`, the Header fetches `/api/auth/federated-logout-url` (while still signed in), then after `logoutAction()` and NextAuth `signOut()`, redirects the browser to the returned end_session URL. HCL.CS.SF clears its session and redirects back to `post_logout_redirect_uri` (e.g. `/login`).

3. **Header flow**  
   [components/layout/Header.tsx](components/layout/Header.tsx) `doLogout()`: (1) fetch federated logout URL if enabled, (2) call `logoutAction()`, (3) in `finally` call NextAuth `signOut({ redirect: false })` and either `window.location.href = federatedLogoutUrl` or `router.push("/login")`. The client session is always cleared and the user is never stuck.
