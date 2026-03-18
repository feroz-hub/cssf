# Google Login Integration in HCL.CS.SF-Admin – Plan

## 1. Current state

### 1.1 Google login in HCL.CS.SF (Demo Server)

- **Endpoints** (see `docs/auth/google-signin.md`):
  - `GET /auth/external/google/start?returnUrl=...&tenantId=...` – starts Google OIDC, redirects to Google.
  - `GET /auth/external/google/callback` – after Google callback, resolves/creates user, signs in with cookie, then redirects to `returnUrl` with a **verification code** in the query: `returnUrl` becomes `returnUrl?UserCode=<code>`.
- **Verification code**: Issued by `AuthorizationService.SaveVerificationCodeAsync(user.UserName)` (Identity/Endpoint). The code is stored in `SecurityTokens` (Identity DB) and is one-time (consumed in authorize callback). The value in the URL is the stored token value (encrypted username).
- **Demo Server** hosts the full HCL.CS.SF stack (`AddHCL.CS.SF`), so the same Identity API that issues the verification code also exposes the token endpoint.

### 1.2 HCL.CS.SF-admin login today

- NextAuth with **Credentials** provider only.
- User submits username/password → admin calls Identity **token endpoint** with `grant_type=password` → receives access/refresh tokens → session holds those tokens.
- No Google option in the admin UI.

### 1.3 Gap

- After “Sign in with Google” on the Demo Server, the user is redirected to `returnUrl` with `UserCode=...`. The admin app could use that code to obtain HCL.CS.SF tokens only if the **token endpoint** accepts a grant that exchanges this code for tokens. Currently the token endpoint supports only: `authorization_code`, `password`, `client_credentials`, `refresh_token`. There is no “exchange verification code for tokens” grant.

---

## 2. Design

### 2.1 Option A – New grant type in Identity (chosen)

- Add a new grant type, e.g. **`user_code`** (or `external_verification_code`), to the Identity token endpoint.
- Request parameters: `grant_type=user_code`, `user_code=<code>`, `client_id`, `client_secret` (or client auth), optional `scope`.
- Server:
  1. Validates client and grant type (admin client must be configured to allow this grant).
  2. Validates `user_code` via existing `AuthorizationService.ValidateVerificationCodeAsync(code)`.
  3. Gets username from the validated token (decrypted `TokenValue`).
  4. Deletes the security token (one-time use).
  5. Loads user by username, builds `Subject` (ClaimsPrincipal) and scope model (same as password flow).
  6. Generates access/refresh/id tokens as in password grant.
- **Pros**: Reuses existing verification code storage and validation; no new endpoints; admin stays a normal OAuth client.  
- **Cons**: Requires Identity (and optionally Gateway) and client configuration changes.

### 2.2 Option B – Demo Server proxy endpoint

- Demo Server exposes e.g. `POST /auth/external/exchange` with `{ "code": "..." }` and admin client credentials.
- Demo Server validates code (same Identity service), then would need to obtain HCL.CS.SF tokens for that user without the user’s password. That would require Identity to support an “issue token for user” API or a similar grant. So Option B still leads to an Identity-side change.

### 2.3 Option C – No backend change

- Admin could redirect to Demo Server login page and open it in the same tab. After Google sign-in, the user lands on the Demo Server (cookie session) instead of the admin. No way to get tokens into the admin app without a new grant or proxy.

**Conclusion:** Implement Option A (new `user_code` grant) and wire the admin to it.

---

## 3. Implementation plan

### 3.1 Identity API (token endpoint)

1. **Constants**
   - Add `OpenIdConstants.GrantTypes.UserCode` (e.g. `"user_code"`).
   - Add `OpenIdConstants.TokenRequest.UserCode` (e.g. `"user_code"`).
2. **Token request validator**
   - In `TokenRequestValidator`, add a branch for `grant_type == UserCode`: validate `user_code` parameter and run a new specification (e.g. `UserCodeFlowSpecification`).
3. **UserCodeFlowSpecification**
   - Check client is allowed grant type `user_code`.
   - Require and validate `user_code` (length, etc.).
   - Call `IAuthorizationService.ValidateVerificationCodeAsync(userCode)`.
   - If valid, get username from the returned token’s decrypted `TokenValue`, then delete the token (one-time use).
   - Find user by username; if not found or not allowed, return error.
   - Set `model.UserName` and `model.Subject` (ClaimsPrincipal for that user).
   - Run scope validation (reuse ROPC-style scope rules) and set `TokenDetails` / `AllowedScopesParserModel`.
4. **Discovery**
   - Include `user_code` in `grant_types_supported` if desired (optional).
5. **Client configuration**
   - Admin client must have `user_code` in its allowed grant types. In HCL.CS.SF, edit the admin client (e.g. in Clients UI) and add `user_code` to Supported Grant Types (alongside `password`, `refresh_token`, etc.). Alternatively update the client record in the database (e.g. `SupportedGrantTypes` column).

### 3.2 Demo Server

1. **Redirect host allowlist**
   - Ensure `Authentication:Google:AllowedRedirectHosts` includes the admin app origin (e.g. `https://admin.localhost:3001` or production admin URL) so that `returnUrl` for “Sign in with Google” can point to the admin.
2. **No new endpoints** – existing `/auth/external/google/start` and callback already append `UserCode` to `returnUrl`.

### 3.3 HCL.CS.SF-admin (Next.js)

1. **Environment / config**
   - Use existing `HCL.CS.SF_DEMO_SERVER_BASE_URL` (or issuer) for the Google start URL.
   - Optional: `NEXT_PUBLIC_GOOGLE_LOGIN_ENABLED=true` to show/hide the Google button (can also derive from same URL).
2. **Login page**
   - Add a “Sign in with Google” button/link.
   - On click: redirect to `{demoServerBaseUrl}/auth/external/google/start?returnUrl={encodeURIComponent(NEXTAUTH_URL + '/login')}` (so after Google, Demo Server redirects to `/login?UserCode=...`).
3. **Login page (when returning with code)**
   - On load, if query has `UserCode` (and optionally `from=google`): call `signIn("credentials", { code: userCode, redirect: true, callbackUrl })` so that the credentials provider can exchange the code.
4. **Credentials provider**
   - In `authorize`, if `credentials.code` is present (and non-empty):
     - Call Identity token endpoint with `grant_type=user_code`, `user_code=credentials.code`, client_id, client_secret, scope.
     - If response contains `access_token`, build the same session shape as password grant (createAuthenticatedUser from token response) and return it.
     - If error, return null and surface error (e.g. via error query param).
   - Otherwise, keep existing username/password behaviour.
5. **Error handling**
   - If code is expired or invalid, show a clear message and optionally clear the `UserCode` query param.

### 3.4 Gateway (optional)

- If the admin calls the token endpoint via the Gateway, ensure the Gateway forwards the token route and allows the new grant type for the admin client. No Gateway code change is strictly required if the admin talks to the Identity server’s token endpoint directly; if it goes through the Gateway, the same grant type must be allowed there.

---

## 4. Security notes

- **One-time use**: The verification code must be deleted after a successful token exchange (in the UserCode flow specification).
- **Client auth**: Only confidential clients (with client_secret) should be allowed the `user_code` grant; validate client as for other grants.
- **Redirect**: `returnUrl` must be restricted to allowed hosts (Demo Server already has `AllowedRedirectHosts`).
- **HTTPS**: Use HTTPS in production for admin and Demo Server.

---

## 5. Files to add/change (summary)

| Area        | Action |
|------------|--------|
| Identity   | Add `GrantTypes.UserCode`, `TokenRequest.UserCode`; add `UserCodeFlowSpecification`; in `TokenRequestValidator` handle `user_code`; optionally discovery. |
| Demo Server| Config: add admin origin to `AllowedRedirectHosts`. |
| HCL.CS.SF-admin | Login: Google button, handle `UserCode` query and call `signIn` with code; Credentials provider: handle `credentials.code` and call token endpoint with `user_code` grant. |

---

## 6. Testing

1. Enable Google in Demo Server (ClientId, ClientSecret, `Enabled=true`).
2. Add admin origin to `AllowedRedirectHosts`.
3. Configure admin client in Identity to allow grant type `user_code`.
4. In admin: click “Sign in with Google” → complete Google → land on `/login?UserCode=...` → automatic sign-in and redirect to `/admin`.
5. Verify code is one-time: repeating the same link or code should fail with invalid_grant.
