# "Authorize Code Challenge is invalid" – authorization_code + PKCE

When exchanging an **authorization code** for tokens (token endpoint), you may get:

- **Error (OAuth):** `invalid_request` or `invalid_grant`
- **Error message:** `Authorize Code Challenge is invalid.`

This comes from HCL.CS.SF’s **PKCE** (Proof Key for Code Exchange) validation for the authorization_code flow.

## What the server checks

At the **token** request, HCL.CS.SF:

1. Loads the authorization code (created at the **authorize** step) and its stored `code_challenge` and `code_challenge_method`.
2. Runs PKCE validation (see `ProofKeyParametersSpecification` and `AuthorizeRequestSpecification`), which includes:
   - **Code challenge present** – stored `code_challenge` must not be null/empty.
   - **Code challenge format** – `code_challenge` must be **base64url**: only characters `A–Z`, `a–z`, `0–9`, `_`, `-` (no `+`, `/`, or `=`).
   - **Code challenge method** – only `S256` is supported.
   - **Code verifier** – you must send `code_verifier` in the token request.
   - **Code verifier vs challenge** – `BASE64URL(SHA256(code_verifier))` must equal the stored `code_challenge`.

The message **"Authorize Code Challenge is invalid"** is returned when the **code challenge format** check fails: the value stored with the authorization code is empty or contains a character that is not allowed (e.g. `+`, `/`, or `=`). In practice, the same flow can also fail earlier (e.g. “Code Challenge for the client is required”) or when the verifier doesn’t match the challenge (different error message).

## Common causes and fixes

### 1. **Wrong encoding of `code_challenge` at authorize**

- **Requirement:** `code_challenge` must be **base64url**-encoded (RFC 7636).
  - Take `SHA256(code_verifier)` and encode the result in **base64url**.
  - Base64url: use `-` instead of `+`, `_` instead of `/`, and **omit** padding (`=`).
- **Wrong:** Using standard base64 (with `+`, `/`, and `=`) or including padding in `code_challenge`.
- **Fix:** Generate the challenge exactly as:
  - `code_challenge = BASE64URL(SHA256(code_verifier))`  
  - No `+`, `/`, or `=` in the string sent as `code_challenge`.

### 2. **`code_verifier` doesn’t match the authorize request**

- The **same** `code_verifier` must be used:
  - When generating `code_challenge` for the **authorize** request.
  - As the `code_verifier` parameter in the **token** request.
- **Fix:** Keep the same `code_verifier` (e.g. in session or state) from the authorize step and send it once at the token exchange. Don’t generate a new random value for the token request.

### 3. **Missing or wrong parameters**

- **Authorize request:** Must include `code_challenge` and `code_challenge_method=S256` when the client uses PKCE (or when HCL.CS.SF requires PKCE for the client).
- **Token request:** Must include `code_verifier` (and `grant_type=authorization_code`, `code`, `redirect_uri` as required).
- **Fix:** Ensure both requests include the correct parameters; use `code_challenge_method=S256` and a base64url `code_challenge`.

### 4. **Length and character set**

- **code_verifier:** Length 43–128 characters; only `[A-Za-z0-9\-._~]` (see RFC 7636).
- **code_challenge:** After base64url encoding, length must be within HCL.CS.SF’s limits (e.g. 43–128).
- **Fix:** Generate a 43–128 character verifier from the allowed set; then compute `code_challenge = BASE64URL(SHA256(code_verifier))`.

### 5. **Stale or corrupted authorization code**

- If the stored `code_challenge` was truncated or corrupted (e.g. in storage or serialization), format validation can fail.
- **Fix:** Run a **new** login flow: new authorize request (with correct PKCE) → new authorization code → one token exchange with the matching `code_verifier`. Don’t reuse an old code from before a client/config change.

## Checklist for authorization_code + PKCE

- [ ] **Authorize request**
  - Send `code_challenge` = base64url(SHA256(code_verifier)), **no** `+`, `/`, or `=`.
  - Send `code_challenge_method=S256`.
  - Use a new, random `code_verifier` (43–128 chars, allowed set) and store it (e.g. session/cookie) for the token step.
- [ ] **Token request**
  - Send the **same** `code_verifier` used to build `code_challenge`.
  - Send `grant_type=authorization_code`, `code`, `redirect_uri` (and client auth) as required.
- [ ] **Client**
  - If using a library, ensure it uses **S256** and **base64url** for the challenge (not plain base64).
  - Ensure the same verifier is reused for the token call and not regenerated.

## References in code

- **Message:** `ValidationMessages.resx` → `INVALID_CODE_CHALLENGE` → "Authorize Code Challenge is invalid."
- **PKCE validation (token):** `ProofKeyParametersSpecification.cs` (e.g. `CheckCodeChallengeFormat`, `CheckCodeVerifierAgainstCodeChallenge`).
- **PKCE validation (authorize):** `AuthorizeRequestSpecification.cs` → `ValidatePkce`.
- **Allowed format:** `code_challenge` must match regex `^[A-Za-z0-9_-]+$` (base64url, no padding).
