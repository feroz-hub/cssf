# Access token audience (`aud`) claim – standard and options

## What the standard says

- **RFC 7519 (JWT):** The `aud` (audience) claim identifies the **intended recipient(s)** of the token. It can be a **single string** or an **array of strings**.
- **Purpose:** Resource servers that receive the token should validate that **their own identifier** is in `aud`. If not, they must reject the token. This limits token misuse and replay across different APIs.
- **OAuth 2.0 Resource Indicators (RFC 8707):** When a client requests a token for a specific resource (e.g. via `resource` parameter or scope), the token is issued for that resource; the audience typically reflects it.

So: **`aud` should identify who the token is for**, not who issued it (that’s `iss`).

---

## Two common patterns

### 1. Single shared audience (e.g. `HCL.CS.SF.api`)

- **Idea:** One `aud` value for all access tokens (e.g. the identity/API gateway identifier).
- **Pros:** Simple; one config; all your resource servers validate the same `aud`.
- **Use when:** All APIs are part of one “platform” and trust the same audience, or you don’t need to distinguish “this token is for API A vs API B”.
- **Example:** Every token has `"aud": "HCL.CS.SF.api"`. HCL.CS.SF Admin, RentFlow, and any other app all validate `aud == "HCL.CS.SF.api"`.

### 2. Per-resource / per-API audience (e.g. `rentflow.api`)

- **Idea:** `aud` reflects the **API (resource)** the token is for. When the user requests scope for “RentFlow”, the token gets `aud` = `"rentflow.api"` (or the name of that API resource).
- **Pros:** Each API validates its own identifier; tokens issued for API A cannot be used at API B if B checks `aud`.
- **Use when:** Multiple distinct APIs (e.g. RentFlow, another product) each want to ensure tokens are only valid for them.
- **Example:** Token with scope `rentflow` → `"aud": "rentflow.api"`. RentFlow’s backend validates `aud` contains `"rentflow.api"`.

You can also combine: **multiple audiences** in one token, e.g. `"aud": ["HCL.CS.SF.api", "rentflow.api"]`, when the token is valid for more than one API. Each recipient checks that its identifier is in the array.

---

## How other identity servers do it

| Product / style | Typical behaviour |
|-----------------|--------------------|
| **IdentityServer / Duende** | When using **resource-based** or **API resources**, the issued access token’s `aud` is set from the **resource (API) identifier** the token was issued for. Optional “global” audience can be combined. |
| **Auth0** | You define **APIs** with an **API Identifier** (audience). The client requests that audience; the token gets `aud` = that identifier. |
| **Keycloak** | Audience can be set per **client** or per **resource**. Tokens can carry the requested resource/audience. |
| **Azure AD / Entra** | `aud` is the **application (client) ID** of the resource (API) the token is for, or a configured identifier for that API. |

Common theme: **when a token is issued for a specific API/resource, `aud` is set to that API’s identifier** so the API can validate it.

---

## What is “correct” for you?

- **Using the same `aud` for all clients (e.g. `HCL.CS.SF.api`) is valid and standard** if all your apps are part of one trust domain and accept that single audience.
- **If a client (e.g. RentFlow) wants its own `aud` (e.g. `rentflow.api`)** so their resource server can strictly validate “this token was issued for RentFlow”, the **standard approach** is:
  - When the token is issued **for that API** (e.g. requested scope includes the RentFlow API resource), set **`aud`** to that API’s identifier (e.g. `rentflow.api`), **or**
  - Include that identifier in an **array** of audiences (e.g. `["HCL.CS.SF.api", "rentflow.api"]`) if the token is valid for multiple APIs.

So: **same `aud` for everyone is correct when you want one shared audience; per-API or multi-audience `aud` is correct when different APIs need to see their own identifier in `aud`.**

---

## How HCL.CS.SF behaves

- **Default:** A global **API identifier** is configured (e.g. `TokenConfig.ApiIdentifier` = `"HCL.CS.SF.api"`). It is used as `aud` when the token is **not** issued for any specific API resource (e.g. only identity scopes like `openid profile email`).
- **Per-resource audience:** When the token is issued for at least one **API resource** (e.g. scope includes `rentflow`), the access token’s **`aud`** is set from the **API resource name** (the first one in the list). So:
  - If the API resource is named **`rentflow`**, tokens for that scope get `"aud": "rentflow"`.
  - If you want **`"aud": "rentflow.api"`**, create (or rename) the API resource in HCL.CS.SF so its **Name** is **`rentflow.api`** (the resource name is used as the audience).
- **Multiple API scopes:** If the token is for more than one API resource, `aud` is set to the **first** API resource name in the list. Future versions could support multiple audiences in `aud` (array).

**Summary:** Same `aud` for all when only the global identifier applies; per-API `aud` (e.g. `rentflow.api`) when the token is for an API resource — set the API resource **Name** in HCL.CS.SF to the desired audience value.
