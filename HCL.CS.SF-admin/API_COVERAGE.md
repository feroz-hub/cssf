# HCL.CS.SF Admin API Coverage

This admin UI calls HCL.CS.SF only through REST endpoints. It does not write to the database directly.

## Clients
- `POST /Security/Api/Client/GetAllClient`
- `POST /Security/Api/Client/GetClient`
- `POST /Security/Api/Client/RegisterClient`
- `POST /Security/Api/Client/UpdateClient`
- `POST /Security/Api/Client/DeleteClient`
- `POST /Security/Api/Client/GenerateClientSecret`

## API Resources and Scopes
- `POST /Security/Api/ApiResource/GetAllApiResources`
- `POST /Security/Api/ApiResource/GetApiResourceById`
- `POST /Security/Api/ApiResource/AddApiResource`
- `POST /Security/Api/ApiResource/UpdateApiResource`
- `POST /Security/Api/ApiResource/DeleteApiResourceById`
- `POST /Security/Api/ApiResource/GetApiScopeById`
- `POST /Security/Api/ApiResource/AddApiScope`
- `POST /Security/Api/ApiResource/UpdateApiScope`
- `POST /Security/Api/ApiResource/DeleteApiScopeById`

## Roles and Claims
- `POST /Security/Api/Role/GetAllRoles`
- `POST /Security/Api/Role/GetRoleById`
- `POST /Security/Api/Role/CreateRole`
- `POST /Security/Api/Role/UpdateRole`
- `POST /Security/Api/Role/DeleteRoleById`
- `POST /Security/Api/Role/AddRoleClaim`
- `POST /Security/Api/Role/RemoveRoleClaimsById`
- `POST /Security/Api/User/GetUsersInRole`

## Users
- `POST /Security/Api/User/GetAllUsers`
- `POST /Security/Api/User/GetUserById`
- `POST /Security/Api/User/UpdateUser`
- `POST /Security/Api/User/LockUser`
- `POST /Security/Api/User/UnLockUser`
- `POST /Security/Api/User/GetUserRoles`
- `POST /Security/Api/User/AddUserRole`
- `POST /Security/Api/User/RemoveUserRole`
- `POST /Security/Api/User/GetAdminUserClaims`
- `POST /Security/Api/User/AddAdminClaim`
- `POST /Security/Api/User/RemoveAdminClaim`

## Token and Session Revocation
- `POST /Security/Api/SecurityToken/GetActiveSecurityTokensByUserIds`
- `POST /Security/Api/SecurityToken/GetActiveSecurityTokensByClientIds`
- `POST /Security/Api/SecurityToken/GetActiveSecurityTokensBetweenDates`
- `POST /Security/Api/SecurityToken/GetAllSecurityTokensBetweenDates`
- `POST /security/revocation` (OAuth revocation endpoint)

## Audit
- `POST /Security/Api/Audittrail/GetAuditDetails`

## Known Backend Limits (UI handles these as best effort)
- No dedicated endpoint for historical client secret list or secret revoke-by-id. Current flow supports rotation and one-time display of generated secret.
- No dedicated user session API with `ip_address` and `user_agent`. Session page uses active security tokens as approximation.
- Token list payload does not always include explicit `issued_at`/`expires_at`/`jti`; UI displays what the current API returns.
- Audit payload shape may vary by event. Subject/client/IP/result/correlation are extracted from event JSON when present.
