# API Explorer: server request format, token, and UX plan

## 1. How the server accepts requests

### 1.1 Gateway / HCL.CS.SF API (Security/Api/*)

- **Method:** POST or GET. Most operations are POST with a JSON body; some read-only endpoints accept GET (no body).
- **URL:** `{baseUrl}{path}` — e.g. `https://api.example.com/Security/Api/Client/GetClient`. Base is chosen in the explorer (Gateway/API, Installer, Demo server).
- **Headers the server expects:**
  - **`Authorization: Bearer <access_token>`** — Required for authenticated endpoints. The admin app sends the current session’s access token. If missing or invalid, server returns 401.
  - **`Content-Type: application/json`** — Set automatically for POST requests.
  - **`X-Correlation-ID: <uuid>`** — Set automatically per request for tracing.
- **Body (POST only):** JSON. Structure depends on the endpoint (e.g. `GetClient` may expect `{ "clientId": "..." }`, `RegisterClient` expects a full client model). GET requests have no body.

### 1.2 Where the token comes from

- The admin app uses **NextAuth** with a JWT that stores `accessToken` and `refreshToken` from HCL.CS.SF’s token endpoint.
- When you use the API Explorer, the **server action** (`callEndpointAction`) runs on the server and calls `requireAccessToken()`, which reads the session (from the same request’s cookie) and returns `session.accessToken`.
- That token is then set as `Authorization: Bearer <accessToken>` on the `fetch()` call to the Gateway. So **you never type or paste a token** in the explorer — it is added automatically from your current login.
- If you are not signed in or the session has expired, `requireAccessToken()` throws and the action may return an error; the explorer shows that and you can use “Sign in again”.

### 1.3 Summary table

| Item        | Who sets it        | Value / note                                      |
|------------|--------------------|----------------------------------------------------|
| URL        | Explorer           | `baseUrl` + selected `path`                       |
| Method     | Explorer           | GET or POST                                       |
| Auth       | Explorer (server)  | `Bearer <session.accessToken>` (auto from login)  |
| Content-Type | Explorer        | `application/json` for POST                       |
| X-Correlation-ID | Explorer     | New UUID per request                              |
| Body       | User (Explorer)    | JSON for POST; built from Form or Raw             |

---

## 2. UX plan: no raw JSON required (form-first)

### 2.1 Goals

- **User-friendly:** Prefer building the request from **fields** (form or key-value) instead of typing raw JSON.
- **Fits all APIs:** One generic approach that works for any endpoint (different endpoints need different JSON shapes).
- **Transparent:** Show that the session token and headers are sent so users understand how the request is made.

### 2.2 Request body: Form vs Raw

- **Form (primary):** Key-value pair editor.
  - List of rows: **Key** (text) + **Value** (text).
  - “Add field” adds a new row; each row can be removed.
  - On **Execute**, the explorer builds a JSON object from the list: `{ key1: value1, key2: value2 }`. Values are sent as strings unless we detect a number/boolean/JSON and parse (optional).
  - Empty form → body can be `{}` or empty string per server action behavior.
  - **No need to type `{}`, commas, or quotes** for simple request bodies.

- **Raw (secondary):** Optional tab or toggle that shows the current body as editable JSON (for nested objects or power users).
  - When switching **Form → Raw**, the current form key-value list is serialized to JSON and shown in the raw editor.
  - When switching **Raw → Form**, we try to parse the raw JSON and fill the key-value list (flat keys only; nested keys can be shown as `"parent.child"` or left in Raw).
  - Keeps Form and Raw in sync so switching back and forth doesn’t lose data.

- **GET:** No body section, or “No body” message. Form/Raw only apply to POST.

### 2.3 Auth and headers in the UI

- **Request panel** shows a small **“Request”** section that includes:
  - **Auth:** “Session token (Bearer) — sent automatically.” Optionally a “Copy token” that copies the current access token (for use in Postman etc.); if no session, show “Not signed in.”
  - **Headers:** Read-only list (or summary): e.g. “Authorization: Bearer ***”, “Content-Type: application/json”, “X-Correlation-ID: generated on send.” So users see that the token is added and how the request is sent.

### 2.4 Optional later improvements

- **Templates / presets:** Dropdown “Template: None | GetUserById | RegisterClient (minimal)…” that pre-fills the key-value form (or raw) with suggested keys and placeholder values for that endpoint. Can be a static map `path → { key: placeholder }`.
- **Value type:** In the form, allow “string | number | boolean | JSON” per field and encode accordingly in the built JSON.
- **OpenAPI:** If request body schemas are added to `all-apis.openapi.json`, we could generate endpoint-specific forms or templates from them.

---

## 3. Implementation checklist

- [x] Document server request format and token handling (this doc).
- [ ] Request panel: show “Auth” and “Headers” (read-only) so users see token is sent.
- [ ] Body: add **Form** mode (key-value pair list) that builds JSON; “Add field” / remove row.
- [ ] Body: add **Form / Raw** tabs (or toggle); sync Form ↔ Raw when switching.
- [ ] Execute uses body from Form (built JSON) or Raw depending on active mode.
- [ ] GET: hide or disable body form; send no body.
- [ ] (Optional) Copy token button; (optional) templates per endpoint.
