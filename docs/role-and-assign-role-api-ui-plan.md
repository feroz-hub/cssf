# Role & Assign Role – API Analysis and UI/UX Plan

This document describes the **Create Role**, **Assign Role to User**, and related APIs, their full flow, and where they are (or will be) implemented in the HCL.CS.SF Admin UI.

---

## 1. Backend API Summary

### 1.1 Role APIs (Security/Api/Role/*)

| API | Method / route | Purpose | Used in UI |
|-----|----------------|---------|------------|
| CreateRole | POST /Security/Api/Role/CreateRole | Create a new role (Name, Description) | Roles page – Create Role dialog |
| UpdateRole | POST /Security/Api/Role/UpdateRole | Update role name/description | Roles page – Edit Role dialog |
| DeleteRoleById | POST /Security/Api/Role/DeleteRoleById | Delete role by Id | Roles page – Delete confirmation |
| DeleteRoleByName | POST /Security/Api/Role/DeleteRoleByName | Delete role by name | Optional (UI uses DeleteRoleById) |
| GetRoleById | POST /Security/Api/Role/GetRoleById | Get role by Id (includes RoleClaims) | Roles page – list + expand claims |
| GetRoleByName | POST /Security/Api/Role/GetRoleByName | Get role by name | Optional |
| GetAllRoles | POST /Security/Api/Role/GetAllRoles | List all roles | Roles page, User detail (dropdown) |
| AddRoleClaim | POST /Security/Api/Role/AddRoleClaim | Add one claim to a role | Roles page – Add Claim (expand) |
| AddRoleClaimList | POST /Security/Api/Role/AddRoleClaimList | Add multiple claims | Optional (UI adds one at a time) |
| GetRoleClaim | POST /Security/Api/Role/GetRoleClaim | Get claims for a role (body: RoleModel) | Roles page uses GetRoleById which returns role with RoleClaims |
| RemoveRoleClaim | POST /Security/Api/Role/RemoveRoleClaim | Remove claim by model | Optional (UI uses RemoveRoleClaimsById) |
| RemoveRoleClaimsById | POST /Security/Api/Role/RemoveRoleClaimsById | Remove claim by claim Id | Roles page – Remove claim |
| RemoveRoleClaimsList | POST /Security/Api/Role/RemoveRoleClaimsList | Remove multiple claims | Optional |

### 1.2 User–Role APIs (Security/Api/User/*)

| API | Method / route | Purpose | Used in UI |
|-----|----------------|---------|------------|
| GetUserRoles | POST /Security/Api/User/GetUserRoles | Get role names for a user (by userId) | User detail page – show assigned roles |
| AddUserRole | POST /Security/Api/User/AddUserRole | Assign one role to a user (UserId, RoleId) | User detail – “Add Role” |
| AddUserRolesList | POST /Security/Api/User/AddUserRolesList | Assign multiple roles | Optional (UI adds one at a time) |
| RemoveUserRole | POST /Security/Api/User/RemoveUserRole | Remove one role from a user | User detail – remove badge; Roles page – “Remove from role” |
| RemoveUserRoleList | POST /Security/Api/User/RemoveUserRoleList | Remove multiple roles | Optional |
| GetUsersInRole | POST /Security/Api/User/GetUsersInRole | List users in a role (by role name) | Roles page – “Users in role” section (count already used) |

---

## 2. Data Flow

### 2.1 Create / Update / Delete Role

1. User opens **Roles** page → server loads `GetAllRoles`, then per role `GetRoleById` and `GetUsersInRole(role.Name)` for count (and for “users in role” list).
2. **Create:** User clicks “Create Role” → dialog (Name, Description) → `createRoleAction` → `CreateRole` API → refresh.
3. **Edit:** User clicks “Edit” on a row → dialog pre-filled → `updateRoleAction` → `UpdateRole` API → refresh.
4. **Delete:** User clicks “Delete” → confirm → `deleteRoleAction` → `DeleteRoleById` API → refresh.

### 2.2 Role Claims (per role)

1. User expands “Claims” on a role row → table shows `role.RoleClaims` (from GetRoleById).
2. **Add claim:** User enters Claim Type + Value → “Add Claim” → `addRoleClaimAction` → `AddRoleClaim` API → refresh.
3. **Remove claim:** User clicks “Remove” on a claim → confirm → `removeRoleClaimAction` → `RemoveRoleClaimsById` API → refresh.

### 2.3 Assign Role to User (from User detail)

1. User opens **Users** → clicks a user → **User detail** page. Server loads `getUser(id)`, `listRoles()`, `getUserRoles(userId)`.
2. Assigned roles shown as badges; dropdown lists only roles not already assigned (`availableRoles`).
3. User selects a role and clicks “Add Role” → `assignUserRoleAction` → `AddUserRole` API (UserId, RoleId) → refresh.
4. User clicks × on a role badge → confirm → `removeUserRoleAction` → `RemoveUserRole` API → refresh.

### 2.4 Users in Role (from Roles page)

1. Roles page already has “Users count” per role (from `GetUsersInRole`).
2. **Enhancement:** Expandable “Users in this role” section: when expanded, show list of users (same `GetUsersInRole` data passed from server). Each row: user name (link to user detail), optional “Remove from role” → `removeUserRoleAction` (userId, roleId) → refresh.

---

## 3. UI Pages and Placement

| Page | Path | Role-related UI |
|------|------|------------------|
| Roles & Claims | `/admin/roles` | List roles; Create / Edit / Delete role; expand Claims (add/remove); **expand Users in role (list + remove from role)** |
| Users | `/admin/users` | List users; link to user detail. |
| User detail | `/admin/users/[id]` | Profile; **Role assignment** – badges + dropdown “Add Role” + remove role with confirm. |

---

## 4. Specification for UI Behaviour

### 4.1 Roles page (`/admin/roles`)

- **Table columns:** Name, Description, Claims count, Users count, Actions (Claims, Edit, Delete).
- **Create Role:** Button in header → modal: Name (required), Description → Save calls CreateRole.
- **Edit Role:** Edit opens modal with Name, Description → Save calls UpdateRole.
- **Delete Role:** Delete opens confirm dialog → Confirm calls DeleteRoleById.
- **Claims (expand):** Expand row to show claims table (Type, Value, Remove). Form to add Claim Type + Value → Add Claim calls AddRoleClaim. Remove calls RemoveRoleClaimsById.
- **Users in role (expand):** Expand row to show “Users in this role” section: table of User name (link to `/admin/users/[id]`), optional “Remove from role” button. Remove calls RemoveUserRole (userId, roleId) with confirm. Data: users array already loaded per role on the server (same as users count).

### 4.2 User detail page (`/admin/users/[id]`)

- **Role assignment card:** Title “Role assignment”.
- **Assigned roles:** Display as badges; each badge has × to remove (with confirm). Empty state: “No roles assigned.”
- **Add role:** Dropdown “Select role” (only roles not in `userRoles`), button “Add Role”. Disabled when no selection or when all roles are already assigned. Empty state in dropdown: “No additional roles” or “All roles assigned”.

---

## 5. Implementation Checklist

- [x] Roles page: Create / Edit / Delete role, Claims expand (add/remove claim).
- [x] User detail: Assign role (dropdown + Add), Remove role (badge × + confirm).
- [x] Roles page: Add “Users in this role” expand section with user list, link to user, and “Remove from role” with confirm.
- [x] User detail: Clear empty states and labels for role assignment (no roles / no additional roles).

---

## 6. File Reference (HCL.CS.SF-admin)

| Area | File |
|------|------|
| API routes | `lib/api/routes.ts` (role.*, user.getUserRoles, addUserRole, removeUserRole, getUsersInRole) |
| Role API client | `lib/api/roles.ts` (listRoles, getRole, createRole, updateRole, deleteRole, addRoleClaim, removeRoleClaim, listUsersInRole) |
| User API client | `lib/api/users.ts` (getUserRoles, addUserRole, removeUserRole) |
| Roles page (server) | `app/admin/roles/page.tsx` (loads roles + per-role getRole + listUsersInRole) |
| Roles UI | `components/modules/roles/RolesModule.tsx` |
| Role actions | `app/admin/roles/actions.ts` (createRoleAction, updateRoleAction, deleteRoleAction, addRoleClaimAction, removeRoleClaimAction) |
| User detail page | `app/admin/users/[id]/page.tsx` (loads user, listRoles, getUserRoles) |
| User detail UI | `components/modules/users/UserDetailModule.tsx` |
| User actions | `app/admin/users/actions.ts` (assignUserRoleAction, removeUserRoleAction) |

This completes the plan. Implement the remaining checklist items in the UI without changing backend APIs.
