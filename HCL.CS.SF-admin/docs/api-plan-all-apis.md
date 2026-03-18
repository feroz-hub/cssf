# API plan: flow, specification, validation, and HCL.CS.SF-admin alignment

This document covers **all APIs** used by HCL.CS.SF-admin: server flow, validation/specifications, required fields, and admin alignment (including gaps). The Client API is documented in detail in [client-registration-plan.md](./client-registration-plan.md); here we reference it and document all other APIs in the same style.

**Server constants:** `ColumnLength255 = 255`, `ColumnLength2048 = 2048` ([Constants.cs](../../src/Identity/HCL.CS.SF.Identity.Domain/Constants/Constants.cs)).

---

## 1. Client API

Fully documented in [client-registration-plan.md](./client-registration-plan.md).

- **Flow:** Admin → `POST /Security/Api/Client/RegisterClient` or UpdateClient → Gateway → `ClientService` → `ClientModelSpecification`.
- **Validation:** ClientName required ≤255; Redirect/PostLogout URIs ≤2048 each; token ranges (e.g. access 60–900, refresh 300–86400); CreatedBy/ModifiedBy ≤255.
- **Admin:** Aligned after gaps fix (name max 255, URI max 2048, token ranges, actor length).

---

## 2. Role API

### 2.1 Flow

- **Create:** Admin → `POST /Security/Api/Role/CreateRole` → Gateway `RoleServiceRoute` → `RoleService.CreateRoleAsync` → `RoleModelSpecification(CrudMode.Add)`.
- **Update:** `POST /Security/Api/Role/UpdateRole` → `RoleModelSpecification(CrudMode.Update)`.
- **Add role claim:** `POST /Security/Api/Role/AddRoleClaim` → `RoleClaimModelSpecification`.
- **Remove role claim:** `POST /Security/Api/Role/RemoveRoleClaim` (by claim id).

**Key files:** [RoleService.cs](../../src/Identity/HCL.CS.SF.Identity.Application/Implementation/Api/Services/RoleService.cs), [RoleModelSpecification.cs](../../src/Identity/HCL.CS.SF.Identity.Application/Implementation/Api/Specifications/RoleModelSpecification.cs), [RoleClaimModelSpecification.cs](../../src/Identity/HCL.CS.SF.Identity.Application/Implementation/Api/Specifications/RoleClaimModelSpecification.cs).

### 2.2 Specification and required fields

**Role (Create/Update):**

| Field / rule | Requirement | Error code |
|--------------|-------------|------------|
| **Name** | Not null | InvalidRoleName |
| **Name** | Length ≤ 255 | RoleNameTooLong |
| **CreatedBy** | Length ≤ 255 | CreatedByTooLong |
| **RoleClaims** (if any) | ClaimType and ClaimValue non-empty | InvalidRoleClaimValueOrClaimType |
| **RoleClaims** (if any) | CreatedBy per claim ≤ 255 | RoleClaimCreatedByTooLong |
| **RoleClaims** (if any) | ClaimValue must be an existing ApiScope name (no duplicates) | InvalidScopeClaims |
| **Update only: Id** | Valid identifier | InvalidRoleId |
| **Update only: ModifiedBy** | Length ≤ 255 | ModifiedByTooLong |
| **Update only: RoleClaims** | RoleId valid on each claim; ModifiedBy ≤ 255 | InvalidRoleClaimRoleId, RoleClaimModifiedByTooLong |

**Role claim (AddRoleClaim):**

| Field / rule | Requirement | Error code |
|--------------|-------------|------------|
| **RoleId** | Valid identifier | InvalidRoleId |
| **ClaimType** | Not null | RoleClaimTypeRequired |
| **ClaimValue** | Not null | RoleClaimValueRequired |
| **ClaimValue** | Must be an existing ApiScope name | InvalidScopeClaims |
| **CreatedBy** | Length ≤ 255 | CreatedByTooLong |

### 2.3 HCL.CS.SF-admin alignment

- **Actions:** [roles/actions.ts](../app/admin/roles/actions.ts) — createRoleAction, updateRoleAction, addRoleClaimAction, removeRoleClaimAction.
- **Schema:** `roleSchema`: name `min(2)`, description default `""`; no max length. `claimSchema`: roleId, claimType, claimValue `min(1)`.
- **Actor:** `actorName(session)` → CreatedBy/ModifiedBy (unbounded in UI; server rejects if > 255).

**Gaps:**

1. Role **name** has no `max(255)`; server returns RoleNameTooLong if > 255.
2. **CreatedBy / ModifiedBy** not capped at 255 in admin (actor can be long email/name).
3. Role **claim** claimType/claimValue no max length (server does not enforce 255 on these in RoleClaimModelSpecification but DB may; safe to add .max(255)).

**Recommendations:** Add name `.max(255)`; validate/truncate actor to 255 before sending; add claimType/claimValue `.max(255)`.

---

## 3. ApiResource (API Resource) API

### 3.1 Flow

- **Add:** Admin → `POST /Security/Api/ApiResource/AddApiResource` → Gateway → `ApiResourceService.AddApiResourceAsync` → `ApiResourceModelSpecification(CrudMode.Add)`.
- **Update:** `POST /Security/Api/ApiResource/UpdateApiResource` → `ApiResourceModelSpecification(CrudMode.Update)`.

**Key files:** [ApiResourceService.cs](../../src/Identity/HCL.CS.SF.Identity.Application/Implementation/Api/Services/ApiResourceService.cs), [ApiResourceModelSpecification.cs](../../src/Identity/HCL.CS.SF.Identity.Application/Implementation/Api/Specifications/ApiResourceModelSpecification.cs).

### 3.2 Specification and required fields

**ApiResource (Add/Update):**

| Field / rule | Requirement | Error code |
|--------------|-------------|------------|
| **Name** | Not null | ApiResourceNameRequired |
| **Name** | Length ≤ 255 | ApiResourceNameTooLong |
| **DisplayName** | Length ≤ 255 | ApiResourceDisplayNameTooLong |
| **CreatedBy** | Length ≤ 255 | CreatedByTooLong |
| **Update: Id** | Valid identifier | ApiResourceIdInvalid |
| **Update: ModifiedBy** | Length ≤ 255 | ModifiedByTooLong |
| **Add: duplicate name** | No existing resource with same Name | ApiResourceAlreadyExists |

Nested (when sent): ApiResourceClaims (Type required, ≤255; CreatedBy ≤255); ApiScopes (Name required, Name/DisplayName/CreatedBy ≤255); ApiScopeClaims (Type ≤255; CreatedBy/ModifiedBy ≤255). Admin currently creates resources without nested claims/scopes in the same payload; scope/claim operations are separate.

### 3.3 HCL.CS.SF-admin alignment

- **Actions:** [resources/actions.ts](../app/admin/resources/actions.ts) — createResourceAction, updateResourceAction, createScopeAction, updateScopeAction, etc.
- **Schema:** `resourceSchema`: name `min(2)`, displayName `min(2)`, description `min(1)`; no max length.

**Gaps:**

1. **Name** and **displayName** no `max(255)`.
2. **CreatedBy / ModifiedBy** not limited to 255.

**Recommendations:** Add `.max(255)` for name and displayName; ensure actor ≤ 255.

---

## 4. ApiScope API

### 4.1 Flow

- **Add:** Admin → `POST /Security/Api/ApiResource/AddApiScope` → `ApiResourceService.AddApiScopeAsync` → `ApiScopeModelSpecification(CrudMode.Add)`.
- **Update:** `POST /Security/Api/ApiResource/UpdateApiScope` → `ApiScopeModelSpecification(CrudMode.Update)`.

**Key files:** [ApiResourceService.cs](../../src/Identity/HCL.CS.SF.Identity.Application/Implementation/Api/Services/ApiResourceService.cs), [ApiScopeModelSpecification.cs](../../src/Identity/HCL.CS.SF.Identity.Application/Implementation/Api/Specifications/ApiScopeModelSpecification.cs).

### 4.2 Specification and required fields

**ApiScope (Add):**

| Field / rule | Requirement | Error code |
|--------------|-------------|------------|
| **ApiResourceId** | Valid identifier | ApiResourceIdInvalid |
| **Name** | Not null | ApiScopeNameRequired |
| **Name** | Length ≤ 255 | ApiScopeNameTooLong |
| **DisplayName** | Length ≤ 255 | ApiScopeDisplayNameTooLong |
| **CreatedBy** | Length ≤ 255 | CreatedByTooLong |
| **Duplicate name** | No existing scope with same Name | ApiScopeAlreadyExists |
| **Scope claims (if any)** | Type required, Type/CreatedBy ≤ 255 | ApiScopeClaimTypeRequired, ApiScopeClaimTypeOrCreatedbyTooLong |

**ApiScope (Update):**

| Field / rule | Requirement | Error code |
|--------------|-------------|------------|
| **Id** | Valid identifier | ApiScopeIdInvalid |
| **ModifiedBy** | Length ≤ 255 | ApiScopeModifiedbyTooLong |
| **Name** | Must match existing active scope (identity check) | ApiScopeNameInvalid |

### 4.3 HCL.CS.SF-admin alignment

- **Schema:** `scopeSchema`: resourceId, name `min(2)`, displayName `min(2)`; no max length.

**Gaps:**

1. **Name** and **displayName** no `max(255)`.
2. **CreatedBy / ModifiedBy** not limited to 255.

**Recommendations:** Add `.max(255)` for name and displayName; ensure actor ≤ 255.

---

## 5. IdentityResource API

### 5.1 Flow

- **Add:** Admin → `POST /Security/Api/IdentityResource/AddIdentityResource` → Gateway → `IdentityResourceService.AddIdentityResourceAsync` → `IdentityResourceModelSpecification(CrudMode.Add)`.
- **Update:** `POST /Security/Api/IdentityResource/UpdateIdentityResource` → `IdentityResourceModelSpecification(CrudMode.Update)`.

**Key files:** [IdentityResourceService.cs](../../src/Identity/HCL.CS.SF.Identity.Application/Implementation/Api/Services/IdentityResourceService.cs), [IdentityResourceModelSpecification.cs](../../src/Identity/HCL.CS.SF.Identity.Application/Implementation/Api/Specifications/IdentityResourceModelSpecification.cs).

### 5.2 Specification and required fields

**IdentityResource (Add/Update):**

| Field / rule | Requirement | Error code |
|--------------|-------------|------------|
| **Name** | Not null | IdentityResourceNameRequired |
| **Name** | Length ≤ 255 | IdentityResourceNameTooLong |
| **DisplayName** | Length ≤ 255 | IdentityDisplayNameTooLong |
| **CreatedBy** | Length ≤ 255 | CreatedByTooLong |
| **Update: Id** | Valid identifier | IdentityResourceIdInvalid |
| **Update: ModifiedBy** | Length ≤ 255 | ModifiedByTooLong |
| **Add: IdentityClaims (if any)** | Type required, Type/CreatedBy ≤ 255 | IdentityResourceClaimTypeRequired, IdentityResourceClaimTypeTooLong, IdentityResourceClaimCreatedByTooLong |
| **Add: duplicate name** | No existing identity resource with same Name | IdentityResourceAlreadyExists |

### 5.3 HCL.CS.SF-admin alignment

- **Actions:** [identity-resources/actions.ts](../app/admin/identity-resources/actions.ts) — createIdentityResourceAction, updateIdentityResourceAction, addIdentityClaimAction.
- **Schema:** `resourceSchema`: name `min(2)`, displayName `min(1)`; no max length. `claimSchema`: identityResourceId, type `min(1)`.

**Gaps:**

1. **Name** and **displayName** no `max(255)`.
2. **CreatedBy / ModifiedBy** not limited to 255.
3. Identity **claim type** no `max(255)`; server enforces IdentityResourceClaimTypeTooLong.

**Recommendations:** Add `.max(255)` for name, displayName, and claim type; ensure actor ≤ 255.

---

## 6. IdentityClaim API (add claim)

### 6.1 Flow

- **Add:** Admin → `POST /Security/Api/IdentityResource/AddIdentityResourceClaim` → `IdentityResourceService.AddIdentityResourceClaimAsync` → `IdentityClaimModelSpecification(CrudMode.Add)`.

**Key files:** [IdentityResourceService.cs](../../src/Identity/HCL.CS.SF.Identity.Application/Implementation/Api/Services/IdentityResourceService.cs), [IdentityClaimModelSpecification.cs](../../src/Identity/HCL.CS.SF.Identity.Application/Implementation/Api/Specifications/IdentityClaimModelSpecification.cs).

### 6.2 Specification and required fields

| Field / rule | Requirement | Error code |
|--------------|-------------|------------|
| **IdentityResourceId** | Valid identifier | IdentityResourceIdInvalid |
| **Type** | Not null | IdentityResourceClaimTypeRequired |
| **Type** | Length ≤ 255 | IdentityResourceClaimTypeTooLong |
| **CreatedBy** | Length ≤ 255 | CreatedByTooLong |
| **Duplicate** | No existing claim with same IdentityResourceId + Type | IdentityResourceClaimAlreadyExists |

### 6.3 HCL.CS.SF-admin alignment

- **Schema:** `claimSchema`: type `min(1)`; no max length; actor as CreatedBy.

**Gaps:**

1. **Type** no `max(255)`.
2. **CreatedBy** not limited to 255.

**Recommendations:** Add `.max(255)` for type; ensure actor ≤ 255.

---

## 7. User API (profile update, lock/unlock, roles, revocation)

### 7.1 Flow

- **Update user:** Admin → User API Update → `UserAccountService.UpdateUserAsync` (inline checks, no separate *ModelSpecification).
- **Lock/Unlock:** LockUser, UnlockUser.
- **Roles:** AddUserRole, RemoveUserRole (UserRoleModelSpecification).
- **Claims:** AddClaim, etc. (UserClaimModelSpecification: UserId, ClaimType, ClaimValue, CreatedBy ≤255).

**Key files:** [UserAccountService.cs](../../src/Identity/HCL.CS.SF.Identity.Application/Implementation/Api/Services/UserAccountService.cs) (e.g. Email length 255, CreatedBy/ModifiedBy 255), [UserClaimModelSpecification.cs](../../src/Identity/HCL.CS.SF.Identity.Application/Implementation/Api/Specifications/UserClaimModelSpecification.cs), [UserRoleService.cs](../../src/Identity/HCL.CS.SF.Identity.Application/Implementation/Api/Services/UserRoleService.cs).

### 7.2 Validation (from service)

- **UpdateUser:** Email length ≤ 255; CreatedBy/ModifiedBy ≤ 255; security question answer ≤ 255; claim CreatedBy/ModifiedBy ≤ 255.
- **UserClaim:** UserId, ClaimType, ClaimValue required; CreatedBy/ModifiedBy ≤ 255.
- **UserRole:** User and Role must exist (UserRoleModelSpecification).

### 7.3 HCL.CS.SF-admin alignment

- **Actions:** [users/actions.ts](../app/admin/users/actions.ts) — updateUserProfileAction, lockUserAction, unlockUserAction, addUserRoleAction, removeUserRoleAction, revokeTokenAction.
- **Schema:** updateSchema: firstName min(1), email email(); no explicit max lengths.

**Gaps:**

1. **Email** not capped at 255 (server enforces).
2. **ModifiedBy** (actor) not capped at 255.

**Recommendations:** Add email `.max(255)`; ensure actor ≤ 255 when sending ModifiedBy.

---

## 8. Audit API

### 8.1 Flow

- **Search:** Admin → `POST /Security/Api/Audittrail/...` (search) → `AuditTrailService`. Service validates TableName/CreatedBy ≤ 255 when filtering/storing; search request is read-only from admin.

**Key files:** [AuditTrailService.cs](../../src/Identity/HCL.CS.SF.Identity.Application/Implementation/Api/Services/AuditTrailService.cs).

### 8.2 HCL.CS.SF-admin alignment

- **Actions:** [audit/actions.ts](../app/admin/audit/actions.ts) — searchAuditAction with actionType, actor, searchValue, fromDate, toDate, page, itemsPerPage.
- **Schema:** actionType 0–3, itemsPerPage 1–200; no server model specification for search request.

**Gaps:** None critical; search parameters are bounded and appropriate.

---

## 9. Revocation (SecurityToken) API

### 9.1 Flow

- **Revoke token:** Admin → `POST /Security/Api/SecurityToken/RevokeToken` (or equivalent) with token + token_type_hint.
- **Bulk revoke:** By user (subject) or by client.

**Key files:** [SecurityTokenServiceRoute](../../src/Gateway), token revocation endpoint (TokenRevocationRequestSpecification for OAuth revoke request format).

### 9.2 HCL.CS.SF-admin alignment

- **Actions:** [revocation/actions.ts](../app/admin/revocation/actions.ts) — revokeTokenAction (token + tokenTypeHint), bulkRevokeByUserAction, bulkRevokeByClientAction, searchRevocationAction.
- **Schema:** revokeSchema: token min(1), tokenTypeHint enum; bulk schemas: subject or clientId min(1).

**Gaps:** None; payload matches server expectations.

---

## 10. Summary: gaps and implementation status

| API | Admin gap | Status / recommendation |
|-----|-----------|---------------------------|
| **Client** | Name 255, URIs 2048, token ranges, actor 255 | Done (see client-registration-plan.md) |
| **Role** | Name max 255; actor 255; claimType/claimValue max 255 | Done: schema max 255, `getActor()` truncates |
| **ApiResource** | Name, displayName max 255; actor 255 | Done: schema max 255, `getActor()` truncates |
| **ApiScope** | Name, displayName max 255; actor 255 | Done: schema max 255, `getActor()` truncates |
| **IdentityResource** | Name, displayName max 255; actor 255 | Done: schema max 255, `getActor()` truncates |
| **IdentityClaim** | Type max 255; actor 255 | Done: schema max 255, `getActor()` truncates |
| **RoleClaim** | claimType, claimValue max 255; CreatedBy 255 | Done: schema max 255, `getActor()` truncates |
| **User** | Email max 255; ModifiedBy (actor) 255 | Done: email max 255, `getActor()` truncates |
| **Audit** | — | Aligned |
| **Revocation** | — | Aligned |

**Shared helpers:** `lib/constants.ts` exports `MAX_LENGTH_255`; `lib/actor.ts` exports `getActor(session)` which returns the current user name/email truncated to 255 characters so CreatedBy/ModifiedBy never exceed server limit. All write actions in roles, resources, identity-resources, and users now use these and schema `.max(MAX_LENGTH_255)` where the server enforces 255.
