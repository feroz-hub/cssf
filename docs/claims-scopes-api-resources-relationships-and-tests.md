# Claims, Scopes, API Resources, API Resource Claims, API Scope Claims, Role Claims — Relationships and How to Check Active/Exists

## 1. Entity relationship overview

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│ API RESOURCE (ApiResources)                                                       │
│ - Id, Name, DisplayName, Description, Enabled, IsDeleted, ...                      │
└─────────────────────────────────────────────────────────────────────────────────┘
    │
    │ 1:N
    ├──────────────────────────────────┬─────────────────────────────────────────────┐
    ▼                                  ▼                                             │
┌─────────────────────────┐   ┌─────────────────────────────────────────────────────┐
│ API RESOURCE CLAIMS     │   │ API SCOPES (ApiScopes)                              │
│ (ApiResourceClaims)     │   │ - Id, ApiResourceId, Name, DisplayName, ...         │
│ - Id, ApiResourceId    │   └─────────────────────────────────────────────────────┘
│ - Type (claim type      │            │                                            │
│   e.g. role,            │            │ 1:N                                        │
│   permission,           │            ▼                                            │
│   capabilities)         │   ┌─────────────────────────────────────────────────────┐
└─────────────────────────┘   │ API SCOPE CLAIMS (ApiScopeClaims)                   │
                              │ - Id, ApiScopeId, Type (claim type)                  │
                              └─────────────────────────────────────────────────────┘
```

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│ ROLES (Roles)                                                                     │
│ - Id, Name, Description, IsDeleted, ...                                          │
└─────────────────────────────────────────────────────────────────────────────────┘
    │
    │ 1:N
    ▼
┌─────────────────────────────────────────────────────────────────────────────────┐
│ ROLE CLAIMS (RoleClaims)                                                          │
│ - Id, RoleId, ClaimType, ClaimValue, IsDeleted, ...                               │
│   (ClaimType + ClaimValue = e.g. "capabilities" + "read:users")                   │
└─────────────────────────────────────────────────────────────────────────────────┘
```

**How they link at token time**

- **Client** has **AllowedScopes** (list of scope names, e.g. resource name or scope name like `ClientApi.Read`).
- When a **token** is requested with a **scope** parameter:
  - The requested scope is matched to **ApiResources** (by resource Name) and **ApiScopes** (by scope Name).
  - For each matched resource/scope, **ApiResourceClaimType** and **ApiScopeClaimType** (the **Type** on ApiResourceClaims and ApiScopeClaims) define which claim types go into the token (e.g. `role`, `permission`, `capabilities`).
  - **Role claims** (and **user claims**) are read for the current user; where **ClaimType** matches the resource/scope claim **Type**, those **ClaimValue**s are added to the token (as role, permission, or capabilities claims).

So: **API Resource** → **API Resource Claims** (claim types) and **API Scopes** → **API Scope Claims** (claim types) define *what* claim types to emit; **Role Claims** (and User Claims) supply the *values* for those types.

---

## 2. Relationships in detail

| Parent | Child | Link | Cardinality |
|--------|--------|------|-------------|
| **ApiResources** | **ApiResourceClaims** | `ApiResourceClaims.ApiResourceId` = `ApiResources.Id` | 1:N |
| **ApiResources** | **ApiScopes** | `ApiScopes.ApiResourceId` = `ApiResources.Id` | 1:N |
| **ApiScopes** | **ApiScopeClaims** | `ApiScopeClaims.ApiScopeId` = `ApiScopes.Id` | 1:N |
| **Roles** | **RoleClaims** | `RoleClaims.RoleId` = `Roles.Id` | 1:N |
| **Users** | **UserRoles** | `UserRoles.UserId` = `Users.Id` | 1:N (users get roles) |
| **UserRoles** | **Roles** | `UserRoles.RoleId` = `Roles.Id` | N:1 |

**Token pipeline link**

- **GetAllApiResourcesByScopesAsync(requestedScopes)** returns **ApiResourcesByScopesModel** rows that join ApiResources, ApiResourceClaims, ApiScopes, ApiScopeClaims. That gives, per requested scope, the **ApiResourceClaimType** and **ApiScopeClaimType** (the `Type` on ApiResourceClaims and ApiScopeClaims).
- **TokenGenerationService.GetClaimsFromApiResources** uses those types and the user’s **RoleClaims** (and UserClaims) to build **role**, **permission**, and **capabilities** claims for the access token.

---

## 3. How to check if something exists or is active

### 3.1 Soft delete (IsDeleted)

All of these entities use **soft delete** via **IsDeleted** (on BaseEntity or the entity):

- **ApiResources**, **ApiScopes**, **ApiResourceClaims**, **ApiScopeClaims**, **Roles**, **RoleClaims**

The persistence layer applies a **global query filter**: `IsDeleted == false`. So:

- **Exists and active** (for reads): any `Get*` method only returns rows where `IsDeleted == false`.
- **Delete** operations set `IsDeleted = true` (soft delete). After that, `Get*` will not return that entity (e.g. Get by Id returns null / no records).

**How to check “exists” or “is active”:**

- **API Resource**: `GetApiResourceAsync(apiResourceId)` or `GetApiResourceAsync(apiResourceName)` — if the service returns a non-null model (or does not throw “no records”), the resource exists and is active. If it returns null or throws with `ApiErrorCodes.NoRecordsFound` / `ApiResourceIdInvalid` / `ApiResourceNameInvalid`, it does not exist or is deleted.
- **API Scope**: `GetApiScopeAsync(apiScopeId)` or `GetApiScopeAsync(apiScopeName)` — same idea; null or invalid/not-found error means not active or not found.
- **API Resource Claims**: `GetApiResourceClaimsAsync(apiResourceId)` — returns list of active claims for that resource; null or empty when none or resource missing/inactive.
- **API Scope Claims**: `GetApiScopeClaimsAsync(apiScopeId)` — returns list of active claims for that scope; null or empty when none or scope missing/inactive.
- **Role**: `GetRoleAsync(roleId)` or `GetRoleAsync(roleName)` — non-null means role exists and is active.
- **Role claims**: Get role by id/name and check `RoleClaims` (or use GetRoleClaimAsync). Only non-deleted role claims are returned by the query filter.

### 3.2 API Resource “Enabled” (business active)

- **ApiResources** has an **Enabled** flag (bool). This is separate from **IsDeleted**.
- **Exists**: still determined by Get* (soft delete). **Active for use**: you can treat a resource as “on” only when it exists and `Enabled == true`; when `Enabled == false`, the resource exists but you may treat it as disabled for authorization/token issuance.

So:

- **Exists**: Get API resource by Id or name; non-null and no “not found” error.
- **Active (soft delete)**: same as exists, because Get* already filters `IsDeleted == false`.
- **Active (business)**: `resource.Enabled == true`.

### 3.3 Summary: how to check

| Entity | Check exists / active (soft delete) | Check “enabled” (if applicable) |
|--------|-------------------------------------|----------------------------------|
| **API Resource** | `GetApiResourceAsync(id)` or `GetApiResourceAsync(name)` → non-null | `ApiResourcesModel.Enabled == true` |
| **API Scope** | `GetApiScopeAsync(id)` or `GetApiScopeAsync(name)` → non-null | N/A (no Enabled flag) |
| **API Resource Claims** | `GetApiResourceClaimsAsync(apiResourceId)` → list (may be empty); resource must exist first | N/A |
| **API Scope Claims** | `GetApiScopeClaimsAsync(apiScopeId)` → list (may be empty); scope must exist first | N/A |
| **Role** | `GetRoleAsync(id)` or `GetRoleAsync(roleName)` → non-null | N/A |
| **Role Claims** | Get role then inspect `RoleClaims`, or use role-claim API; only non-deleted claims returned | N/A |

---

## 4. Test coverage overview

The following test areas exist or are recommended:

### 4.1 API Resources

- Add (success, null claims/scopes, CreatedBy null → Anonymous).
- Update (success, clear child claims, add child claims).
- Delete by Id and by Name (resource and child scopes/claims no longer returned).
- Get by Id and by Name (success).
- **Exists/active**: Get by invalid Id or invalid Name returns failure / null (see existing error tests).
- **Enabled**: Update resource with `Enabled = false` / `true` and assert.
- Error cases: duplicate name, null/empty name, name/display name too long, update after delete (no active record), delete invalid Id/Name, concurrency.

### 4.2 API Scopes

- Add (success, null ApiScopeClaims, CreatedBy null → Anonymous).
- Update (success).
- Delete by Id and by Name (scope and its scope claims no longer returned).
- Get by Id and by Name (success).
- **Exists/active**: Get by invalid Id or Name returns failure / null.
- Error cases: empty name, name too long, duplicate scope name, update after delete, delete invalid Id/Name, concurrency.

### 4.3 API Resource Claims

- Add (success); Get by resource Id returns the new claim type.
- Delete by Id (all claims for resource), by claim model (specific Type + ResourceId).
- **Exists**: Delete with invalid resource Id or non-matching Type/ResourceId returns error.
- Error cases: duplicate (same ResourceId + Type), Type too long, invalid resource Id for delete.

### 4.4 API Scope Claims

- Add (success); Get by scope Id returns the new claim type.
- Delete by claim model, by scope Id (all claims for scope).
- **Exists**: Delete with invalid scope Id or non-matching Type/ScopeId returns error.
- Error cases: duplicate (same ScopeId + Type), Type too long, invalid scope Id for delete.

### 4.5 Role and Role Claims

- Create role (success, with and without RoleClaims).
- Add role claim(s) (single and list).
- Update role (including role claims).
- Remove role claim(s) (by model, by list).
- Delete role by name (success).
- Get role by Id and by name; GetRoleClaimAsync (success).
- **Exists**: Remove role claim with non-existent RoleId or non-matching claim → error (RoleClaimNotExists).
- Error cases: duplicate role name, duplicate role claim (same RoleId + ClaimType + ClaimValue), role name too long, remove claim when no record.

---

## 5. Where tests live

- **API Resources, Scopes, API Resource Claims, API Scope Claims**: `tests/HCL.CS.SF.IntegrationTests/Api/ApiResourceServiceTests.cs`
- **Roles and Role Claims**: `tests/HCL.CS.SF.IntegrationTests/Api/RoleServiceTests.cs`
- **Relationship and existence**: `tests/HCL.CS.SF.IntegrationTests/Api/ClaimsScopesApiResourcesRelationshipTests.cs`

### 5.1 Test cases in ClaimsScopesApiResourcesRelationshipTests.cs

| Trait | Test method | What it verifies |
|-------|-------------|------------------|
| Relationship | GetApiResource_ById_IncludesApiResourceClaimsAndApiScopes | Get resource returns non-null ApiResourceClaims and ApiScopes |
| Relationship | GetApiResource_ById_EachScopeIncludesApiScopeClaims | Each scope has ApiScopeClaims collection |
| Relationship | GetApiResourceClaims_ByResourceId_ReturnsOnlyClaimsForThatResource | Claim list only contains claims for the given resource Id |
| Relationship | GetApiScopeClaims_ByScopeId_ReturnsOnlyClaimsForThatScope | Claim list only contains claims for the given scope Id |
| Relationship | GetRole_ByName_IncludesRoleClaims | Get role returns RoleClaims |
| Relationship | GetRoleClaims_ReturnsClaimsForRoleOnly | GetRoleClaimAsync returns claims for the role |
| Relationship | GetAllApiResourcesByScopesAsync_WithValidScope_ReturnsMatchingResourceAndClaimTypes | Requested scopes resolve to ApiResourcesByScopes with claim types |
| Exists | GetApiResource_ByInvalidId_ReturnsNullOrThrows | Get by non-existent Guid returns null |
| Exists | GetApiResource_ByInvalidName_ReturnsNullOrThrows | Get by non-existent name returns null |
| Exists | GetApiResource_AfterDelete_ReturnsNull | After Delete, Get by Id returns null |
| Exists | GetApiScope_ByInvalidId_ReturnsNull | Get scope by non-existent Id returns null |
| Exists | GetApiScope_AfterDelete_ReturnsNull | After scope delete, Get returns null |
| Exists | GetApiResourceClaims_ByInvalidResourceId_ReturnsNull | Get claims for non-existent resource Id returns null |
| Exists | GetApiScopeClaims_ByInvalidScopeId_ReturnsEmptyOrNull | Get claims for non-existent scope Id returns null/empty |
| Exists | GetRole_ByInvalidId_ReturnsNull | Get role by non-existent Id returns null |
| Exists | GetRole_ByInvalidName_ReturnsNull | Get role by non-existent name returns null |
| Active | ApiResource_Enabled_CanBeUpdatedAndRead | ApiResourcesModel.Enabled can be set and persisted |

---

## 6. Quick reference: service methods for “exists” / “active”

| To check | Service method | “Exists / active” when |
|----------|----------------|-------------------------|
| API Resource | `IApiResourceService.GetApiResourceAsync(Guid)` or `GetApiResourceAsync(string)` | Returns non-null model |
| API Scope | `IApiResourceService.GetApiScopeAsync(Guid)` or `GetApiScopeAsync(string)` | Returns non-null model |
| API Resource Claims | `IApiResourceService.GetApiResourceClaimsAsync(apiResourceId)` | Resource exists; list may be empty |
| API Scope Claims | `IApiResourceService.GetApiScopeClaimsAsync(apiScopeId)` | Scope exists; list may be empty |
| Role | `IRoleService.GetRoleAsync(Guid)` or `GetRoleAsync(string)` | Returns non-null model |
| Role Claims | Get role then `RoleModel.RoleClaims`, or role-claim APIs | Only non-deleted claims are returned |

After a **Delete** call, the corresponding **Get** will return null / no records (or error code) because the entity is soft-deleted (`IsDeleted = true`) and filtered out by the global query filter.
