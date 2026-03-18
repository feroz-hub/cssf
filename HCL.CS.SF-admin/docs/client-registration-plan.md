# Client registration: server flow, required fields, validation, and HCL.CS.SF-admin alignment

## 1. How a client is registered

### 1.1 Flow (high level)

```
HCL.CS.SF-admin (Create Client)  →  POST /Security/Api/Client/RegisterClient  (body: ClientsModel)
       ↓
Gateway (ClientServiceRoute.RegisterClient)  →  ClientServices.RegisterClientAsync(clientModel)
       ↓
ClientService.RegisterClientAsync (Identity)
  • Validate: ClientModelSpecification (CrudMode.Add)
  • If invalid → throw (validation error code)
  • If valid:
    - Overwrite ClientId = random, ClientSecret = SHA256(random), ClientIdIssuedAt = UtcNow, ClientSecretExpiresAt = UtcNow + config
    - Map to entity, Insert, SaveChanges
    - Return registered client (with plain ClientSecret for one-time display)
```

### 1.2 Key files (server)

| Step | Location |
|------|----------|
| API route | [Gateway/Routes/ClientServiceRoute.cs](src/Gateway/HCL.CS.SF.Gateway/Routes/ClientServiceRoute.cs) – `RegisterClient` |
| Route path | [ApiRoutePathConstants.RegisterClient](src/Identity/HCL.CS.SF.Identity.Domain/Constants/ApiRoutePathConstants.cs) = `"/Security/Api/Client/RegisterClient"` |
| Service | [ClientService.RegisterClientAsync](src/Identity/HCL.CS.SF.Identity.Application/Implementation/Api/Services/ClientService.cs) |
| Validation | [ClientModelSpecification](src/Identity/HCL.CS.SF.Identity.Application/Implementation/Api/Specifications/ClientModelSpecification.cs) (CrudMode.Add) |
| Model | [ClientsModel](src/Identity/HCL.CS.SF.Identity.Domain/Models/Endpoint/ClientsModel.cs) |

### 1.3 What the server overwrites on create

In `RegisterClientAsync` the server **ignores** client-supplied values for:

- `ClientId` → set to new random string
- `ClientSecret` → set to SHA256 of new random; plain secret returned in response
- `ClientIdIssuedAt` → `DateTime.UtcNow`
- `ClientSecretExpiresAt` → `DateTime.UtcNow.AddDays(tokenConfig.ClientSecretExpirationInDays)`
- `RequireClientSecret` → set to `true`
- `IsFirstPartyApp` → set to `true`

So the admin can send placeholders for `ClientId`, `ClientSecret`, and the two dates; they are overwritten.

---

## 2. Required fields and validation (ClientModelSpecification, Add mode)

All rules below run for **Add** (create). Rules are in [ClientModelSpecification.cs](src/Identity/HCL.CS.SF.Identity.Application/Implementation/Api/Specifications/ClientModelSpecification.cs).

### 2.1 Required / validated fields (create)

| Field / rule | Requirement | Error code (server) |
|--------------|-------------|----------------------|
| **ClientName** | Not null | ClientNameIsRequired |
| **ClientName** | Length ≤ 255 | ClientNameTooLong |
| **CreatedBy** | Length ≤ 255 (if present) | CreatedByTooLong |
| **TermsOfServiceUri** | Non-empty and valid URI | InvalidTermsOfService |
| **LogoUri** | Non-empty and valid URI | InvalidLogoUri |
| **ClientUri** | Non-empty and valid URI | InvalidClientUri |
| **PolicyUri** | Non-empty and valid URI | InvalidPolicyUri |
| **AllowedScopes** | Not null, count ≥ 1 | AllowedScopesIsRequired |
| **AllowedScopes** | Every scope must exist (identity + API resources + API scopes + offline_access) | InvalidScopeOrNotAllowed |
| **SupportedGrantTypes** | Not null, count ≥ 1 | SupportedGrantTypesIsRequired |
| **SupportedGrantTypes** | At least one supported grant type; allowed values: authorization_code, refresh_token, client_credentials, password, user_code | InvalidGrantTypeForClient |
| **SupportedResponseTypes** | Not null | SupportedResponseTypesIsRequired |
| **SupportedResponseTypes** | `["code"]` only when `authorization_code` is enabled; otherwise `[]` | ResponseTypeMissing |
| **RedirectUris** (when authorization_code grant) | At least one redirect URI | RedirectURIIsMandatory |
| **RedirectUris** | Each URI valid; each length ≤ 2048; each CreatedBy ≤ 255 | InvalidRedirectUri, RedirectUriTooLong, RedirectUriCreatedByTooLong |
| **PostLogoutRedirectUris** | If any: each valid URI; each length ≤ 2048; each CreatedBy ≤ 255 | InvalidPostLogoutRedirectUri, PostRedirectUriTooLong, PostRedirectUriCreatedByTooLong |
| **AccessTokenExpiration** | In range [MinAccessTokenExpiration, MaxAccessTokenExpiration] (default 60–900) | InvalidAccessTokenExpireRange |
| **IdentityTokenExpiration** | In range [MinIdentityTokenExpiration, MaxIdentityTokenExpiration] (default 60–900) | InvalidIdentityTokenExpireRange |
| **RefreshTokenExpiration** | In range [MinRefreshTokenExpiration, MaxRefreshTokenExpiration] (default 300–86400) | InvalidRefreshTokenExpireRange |
| **LogoutTokenExpiration** | In range [MinLogoutTokenExpiration, MaxLogoutTokenExpiration] (default 1800–86400) | InvalidLogoutTokenExpireRange |
| **AuthorizationCodeExpiration** | In range [MinAuthorizationCodeExpiration, MaxAuthorizationCodeExpiration] (default 60–600) | InvalidAuthorizationCodeExpireRange |
| **AllowedSigningAlgorithm** | Allowed value (e.g. RS256) | SigningAlgorithmIsInvalid |
| **PKCE** | If public client (no secret), RequirePkce must be true | InvalidCodeChallenge |
| **Client name uniqueness** (Add only) | No existing client with same ClientName | ClientNameAlreadyExists |

### 2.2 Server default token expiration ranges (TokenConfig)

From [TokenConfig.cs](src/Identity/HCL.CS.SF.Identity.Domain/Configurations/Endpoint/TokenConfig.cs) (defaults; can be overridden by config):

| Token type | Min (default) | Max (default) |
|------------|----------------|----------------|
| Access | 60 | 900 |
| Identity | 60 | 900 |
| Refresh | 300 | 86400 |
| Authorization code | 60 | 600 |
| Logout | 1800 | 86400 |

### 2.3 URI validation (server)

- **IsValidUri**: For single-string fields (ClientUri, LogoUri, TermsOfServiceUri, PolicyUri). Returns **false** if value is null or whitespace; otherwise checks `IsValidUrl()`. So these must be **non-empty and valid URLs**.
- **IsValidUris**: For redirect/post-logout lists. If the list is non-empty, each entry must be a valid URI; lengths and CreatedBy are checked as above.

---

## 3. HCL.CS.SF-admin: create-client payload and validation

### 3.1 Where it happens

- **Form and submit:** [components/modules/clients/ClientsModule.tsx](HCL.CS.SF-admin/components/modules/clients/ClientsModule.tsx) – Create dialog, `onSubmitCreate` → `createClientAction(toPayload(form))`.
- **Action and model:** [app/admin/clients/actions.ts](HCL.CS.SF-admin/app/admin/clients/actions.ts) – `formSchema.safeParse(input)`, then `buildCreateModel(parsed.data, actor)`.
- **API:** [lib/api/clients.ts](HCL.CS.SF-admin/lib/api/clients.ts) – `createClient(model)` → POST `ApiRoutes.client.registerClient` with body `ClientsModel`.

### 3.2 Form schema (actions.ts)

| Field | Admin validation | Sent to server |
|-------|-------------------|----------------|
| name | min 2 chars | ClientName |
| clientId | optional (ignored on create) | (overwritten) |
| type | enum "1"–"4" | ApplicationType |
| allowedGrantTypes | array, min 1 | SupportedGrantTypes |
| redirectUris | array, min 1; each valid URL; trimmed | RedirectUris |
| postLogoutUris | array; each valid URL if present; trimmed | PostLogoutRedirectUris |
| allowedScopes | array, min 1 | AllowedScopes |
| accessTokenLifetime | coerce number, 300–86400 | AccessTokenExpiration |
| refreshTokenLifetime | coerce number, 300–2592000 | RefreshTokenExpiration |
| logoUri, clientUri, termsOfServiceUri, policyUri | optional; empty → default `https://localhost:3000/...` | LogoUri, ClientUri, TermsOfServiceUri, PolicyUri |

### 3.3 buildCreateModel (actions.ts) – fixed values sent

- **IdentityTokenExpiration:** 3600  
- **LogoutTokenExpiration:** 1800  
- **AuthorizationCodeExpiration:** 600  
- **SupportedResponseTypes:** `["code"]` only when `authorization_code` is selected; otherwise `[]`  
- **AllowedSigningAlgorithm:** `"RS256"`  
- **RequirePkce:** true, **RequireClientSecret:** true, **IsFirstPartyApp:** true  
- **CreatedBy:** actor (name or email or `"HCL.CS.SF-admin"`)  
- **RedirectUris / PostLogoutRedirectUris:** each with `CreatedBy: actor`, `CreatedOn: now`, `Id` and `ClientId` zero GUID

So the admin **does** send all required fields and fixed values expected by the server for create.

---

## 4. Alignment and gaps

### 4.1 Aligned

- **ClientName:** Required, length; admin requires name min 2, server 255 max – aligned.
- **URIs (Client, Logo, Terms, Policy):** Server requires non-empty valid URI; admin defaults empty to `https://localhost:3000/...` – aligned.
- **AllowedScopes / SupportedGrantTypes / SupportedResponseTypes:** Admin sends non-empty grant types; response types are conditional: `["code"]` for auth-code clients and `[]` for non-auth-code clients – aligned.
- **Redirect URIs:** Admin requires at least one valid URL only for auth-code clients; CreatedBy set – aligned.
- **Post-logout URIs:** Optional on server if list empty; admin sends trimmed list – aligned.
- **Scopes must exist:** Admin uses `availableScopes` from `listApiResources()` so only server-known scopes can be chosen – aligned.
- **Client name uniqueness:** Server checks on create; admin does not pre-check (user sees server error if duplicate) – acceptable.
- **Token expiration:** Admin sends Identity/Logout/AuthorizationCode in server default ranges; Access and Refresh are user-editable within admin limits (300–86400 and 300–2592000).

### 4.2 Potential mismatch: token expiration ranges

- **Access token:** Server default range is **60–900** seconds. Admin allows **300–86400**. If the server keeps default config and the user picks e.g. 3600, the server returns **InvalidAccessTokenExpireRange**.
- **Refresh token:** Server default max is **86400**; admin max is **2592000** – so values above 86400 can be rejected if server is unchanged.
- **Authorization code:** Server default max **600**; admin sends fixed **600** – aligned.

**Recommendation:** Either align admin limits with server defaults (e.g. access 60–900, refresh 300–86400) or document that server config (TokenExpiration) may need to be increased if users set higher values in the admin. Optional: expose server token config via API and derive admin min/max from it.

### 4.3 Summary table

| Server requirement | Admin send / check |
|--------------------|--------------------|
| ClientName required, ≤255 | name required, min 2; not explicitly capped at 255 |
| CreatedBy ≤255 | actor (name/email/fallback) |
| ClientUri, LogoUri, Terms, Policy non-empty valid URI | defaults when empty; isValidUrl |
| AllowedScopes non-empty, scopes exist on server | min 1; choices from listApiResources |
| SupportedGrantTypes non-empty | min 1 |
| SupportedResponseTypes conditional on authorization_code | `["code"]` for auth-code clients, `[]` otherwise |
| RedirectUris ≥1 when authorization_code | enforced only for auth-code clients |
| Redirect/PostLogout URI length ≤2048, CreatedBy ≤255 | no explicit 2048/255 in UI; server will reject if exceeded |
| Access/Identity/Refresh/Logout/AuthCode in server ranges | fixed or user input; access 300–86400 can exceed server default 60–900 |
| AllowedSigningAlgorithm | fixed RS256 |
| ClientName unique (Add) | no client-side check; server error shown |

---

## 5. Recommended checks in HCL.CS.SF-admin (implemented)

1. **ClientName max length:** Implemented. Form schema has `name.max(255)`; Client Name input has `maxLength={255}`; client-side `validateForm` checks name length.
2. **Redirect/PostLogout URI length:** Implemented. Schema refines redirect and post-logout URI arrays so each entry is ≤ 2048 characters. `validateForm` checks parseList(redirectUrisText) and parseList(postLogoutUrisText) for length. Helper text on the form shows the limit.
3. **CreatedBy (actor) length:** Implemented. Before `buildCreateModel` / `mergeClientModel`, create and update actions check `actor.length <= 255` and return a clear error if exceeded.
4. **Token expiration bounds:** Implemented. Access token lifetime restricted to 60–900 seconds; refresh token lifetime to 300–86400 seconds in schema and in `validateForm`. Number inputs use `min`/`max`; helper text shows the allowed range.
5. **Scope source:** Unchanged; `availableScopes` from `listApiResources()` so only server-known scopes are sent.
