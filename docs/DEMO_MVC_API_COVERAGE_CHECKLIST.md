# Demo MVC API Coverage Checklist

**Generated:** 2026-03-02 08:28 UTC

## Scope

- Source of truth for routed APIs: `ApiRoutePathConstants.ApiRouteModels` in `src/Identity/HCL.CS.SF.Identity.Domain/Constants/ApiRoutePathConstants.cs` (commented entries excluded).
- Coverage rule: API considered integrated when `ApiRoutePathConstants.<RouteConstant>` is referenced in `demos/HCL.CS.SF.Demo.Client.Mvc/**/*.cs`.
- Note: This report measures explicit integration paths, not manual calls made through the generic API test page.

## Summary

| Metric | Count |
|---|---:|
| Routed APIs (non-comment) | 139 |
| Referenced in Demo MVC | 59 |
| Not Referenced in Demo MVC | 80 |
| Coverage | 42.45% |

## Missing By Area

| Area | Missing Count |
|---|---:|
| ApiResource | 6 |
| ApiScope | 6 |
| IdentityResource | 4 |
| Role | 7 |
| UserAuth | 46 |
| Client | 1 |
| Audit | 3 |
| SecurityToken | 4 |
| Other | 3 |

### Missing: ApiResource

- [ ] `DeleteApiResourceByName`
- [ ] `DeleteApiResourceClaimByResourceIdAsync`
- [ ] `DeleteApiResourceClaimModel`
- [ ] `GetAllApiResourcesByScopesAsync`
- [ ] `GetApiResourceByName`
- [ ] `GetApiResourceClaimsById`

### Missing: ApiScope

- [ ] `DeleteApiScopeByName`
- [ ] `DeleteApiScopeClaimByScopeId`
- [ ] `DeleteApiScopeClaimModel`
- [ ] `GetAllApiScopes`
- [ ] `GetApiScopeByName`
- [ ] `GetApiScopeClaims`

### Missing: IdentityResource

- [ ] `DeleteIdentityResourceByName`
- [ ] `DeleteIdentityResourceClaimModel`
- [ ] `GetIdentityResourceByName`
- [ ] `GetIdentityResourceClaims`

### Missing: Role

- [ ] `AddRoleClaimList`
- [ ] `DeleteRoleByName`
- [ ] `GetRoleByName`
- [ ] `GetRoleClaim`
- [ ] `GetUsersInRole`
- [ ] `RemoveRoleClaim`
- [ ] `RemoveRoleClaimsList`

### Missing: UserAuth

- [ ] `AddAdminClaimList`
- [ ] `AddClaim`
- [ ] `AddClaimList`
- [ ] `AddUserRole`
- [ ] `AddUserSecurityQuestion`
- [ ] `AddUserSecurityQuestionList`
- [ ] `DeleteUserById`
- [ ] `DeleteUserByName`
- [ ] `DeleteUserSecurityQuestion`
- [ ] `DeleteUserSecurityQuestionList`
- [ ] `GenerateEmailTwoFactorToken`
- [ ] `GenerateSmsTwoFactorToken`
- [ ] `GenerateUserTokenAsync`
- [ ] `GetAllTwoFactorType`
- [ ] `GetClaims`
- [ ] `GetUserByEmail`
- [ ] `GetUserClaims`
- [ ] `GetUserRoleClaimsById`
- [ ] `GetUserRoleClaimsByName`
- [ ] `GetUserSecurityQuestions`
- [ ] `GetUsersForClaim`
- [ ] `IsUserExistsByClaimPrincipal`
- [ ] `IsUserExistsById`
- [ ] `IsUserExistsByName`
- [ ] `IsUserSignedIn`
- [ ] `LockUserWithEndDatePath`
- [ ] `PasswordSignIn`
- [ ] `PasswordSignInByTwoFactorAuthenticatorToken`
- [ ] `RemoveAdminClaimList`
- [ ] `RemoveClaim`
- [ ] `RemoveClaimList`
- [ ] `RemoveUserRole`
- [ ] `ReplaceClaim`
- [ ] `RopValidateCredentials`
- [ ] `SetTwoFactorEnabled`
- [ ] `SignOut`
- [ ] `TwoFactorAuthenticatorAppSignIn`
- [ ] `TwoFactorEmailSignIn`
- [ ] `TwoFactorRecoveryCodeSignIn`
- [ ] `TwoFactorSmsSignInAsync`
- [ ] `UnLockUserByToken`
- [ ] `UnLockUserByuserSecurityQuestions`
- [ ] `UpdateUserSecurityQuestion`
- [ ] `VerifyEmailTwoFactorToken`
- [ ] `VerifySmsTwoFactorToken`
- [ ] `VerifyUserToken`

### Missing: Client

- [ ] `GenerateClientSecret`

### Missing: Audit

- [ ] `AddAuditTrail`
- [ ] `AddAuditTrailModel`
- [ ] `GetAuditDetails`

### Missing: SecurityToken

- [ ] `GetActiveSecurityTokensBetweenDates`
- [ ] `GetActiveSecurityTokensByClientIds`
- [ ] `GetActiveSecurityTokensByUserIds`
- [ ] `GetAllSecurityTokensBetweenDates`

### Missing: Other

- [ ] `AddSecurityQuestion`
- [ ] `DeleteSecurityQuestion`
- [ ] `UpdateSecurityQuestion`

## Covered Route Constants

- [x] `AddAdminClaim`
- [x] `AddApiResource`
- [x] `AddApiResourceClaim`
- [x] `AddApiScope`
- [x] `AddApiScopeClaim`
- [x] `AddIdentityResource`
- [x] `AddIdentityResourceClaim`
- [x] `AddRoleClaim`
- [x] `AddUserRolesList`
- [x] `ChangePassword`
- [x] `CountRecoveryCodesAsync`
- [x] `CreateRole`
- [x] `DeleteApiResourceById`
- [x] `DeleteApiResourceClaimById`
- [x] `DeleteApiScopeById`
- [x] `DeleteApiScopeClaimById`
- [x] `DeleteClient`
- [x] `DeleteIdentityResourceById`
- [x] `DeleteIdentityResourceClaimById`
- [x] `DeleteRoleById`
- [x] `GenerateEmailConfirmationToken`
- [x] `GeneratePasswordResetToken`
- [x] `GeneratePhoneNumberConfirmationToken`
- [x] `GenerateRecoveryCodes`
- [x] `GetAdminUserClaims`
- [x] `GetAllApiResources`
- [x] `GetAllClient`
- [x] `GetAllIdentityResources`
- [x] `GetAllRoles`
- [x] `GetAllSecurityQuestions`
- [x] `GetAllUsers`
- [x] `GetApiResourceById`
- [x] `GetApiScopeById`
- [x] `GetClient`
- [x] `GetIdentityResourceById`
- [x] `GetRoleById`
- [x] `GetUserById`
- [x] `GetUserByName`
- [x] `GetUserRoles`
- [x] `LockUser`
- [x] `RegisterClient`
- [x] `RegisterUser`
- [x] `RemoveAdminClaim`
- [x] `RemoveRoleClaimsById`
- [x] `RemoveUserRoleList`
- [x] `ResetAuthenticatorApp`
- [x] `ResetPassword`
- [x] `SetupAuthenticatorApp`
- [x] `UnLockUser`
- [x] `UpdateApiResource`
- [x] `UpdateApiScope`
- [x] `UpdateClient`
- [x] `UpdateIdentityResource`
- [x] `UpdateRole`
- [x] `UpdateUser`
- [x] `UpdateUserTwoFactorType`
- [x] `VerifyAuthenticatorAppSetup`
- [x] `VerifyEmailConfirmationToken`
- [x] `VerifyPhoneNumberConfirmationToken`

## All Missing Route Constants

- [ ] `AddAdminClaimList`
- [ ] `AddAuditTrail`
- [ ] `AddAuditTrailModel`
- [ ] `AddClaim`
- [ ] `AddClaimList`
- [ ] `AddRoleClaimList`
- [ ] `AddSecurityQuestion`
- [ ] `AddUserRole`
- [ ] `AddUserSecurityQuestion`
- [ ] `AddUserSecurityQuestionList`
- [ ] `DeleteApiResourceByName`
- [ ] `DeleteApiResourceClaimByResourceIdAsync`
- [ ] `DeleteApiResourceClaimModel`
- [ ] `DeleteApiScopeByName`
- [ ] `DeleteApiScopeClaimByScopeId`
- [ ] `DeleteApiScopeClaimModel`
- [ ] `DeleteIdentityResourceByName`
- [ ] `DeleteIdentityResourceClaimModel`
- [ ] `DeleteRoleByName`
- [ ] `DeleteSecurityQuestion`
- [ ] `DeleteUserById`
- [ ] `DeleteUserByName`
- [ ] `DeleteUserSecurityQuestion`
- [ ] `DeleteUserSecurityQuestionList`
- [ ] `GenerateClientSecret`
- [ ] `GenerateEmailTwoFactorToken`
- [ ] `GenerateSmsTwoFactorToken`
- [ ] `GenerateUserTokenAsync`
- [ ] `GetActiveSecurityTokensBetweenDates`
- [ ] `GetActiveSecurityTokensByClientIds`
- [ ] `GetActiveSecurityTokensByUserIds`
- [ ] `GetAllApiResourcesByScopesAsync`
- [ ] `GetAllApiScopes`
- [ ] `GetAllSecurityTokensBetweenDates`
- [ ] `GetAllTwoFactorType`
- [ ] `GetApiResourceByName`
- [ ] `GetApiResourceClaimsById`
- [ ] `GetApiScopeByName`
- [ ] `GetApiScopeClaims`
- [ ] `GetAuditDetails`
- [ ] `GetClaims`
- [ ] `GetIdentityResourceByName`
- [ ] `GetIdentityResourceClaims`
- [ ] `GetRoleByName`
- [ ] `GetRoleClaim`
- [ ] `GetUserByEmail`
- [ ] `GetUserClaims`
- [ ] `GetUserRoleClaimsById`
- [ ] `GetUserRoleClaimsByName`
- [ ] `GetUserSecurityQuestions`
- [ ] `GetUsersForClaim`
- [ ] `GetUsersInRole`
- [ ] `IsUserExistsByClaimPrincipal`
- [ ] `IsUserExistsById`
- [ ] `IsUserExistsByName`
- [ ] `IsUserSignedIn`
- [ ] `LockUserWithEndDatePath`
- [ ] `PasswordSignIn`
- [ ] `PasswordSignInByTwoFactorAuthenticatorToken`
- [ ] `RemoveAdminClaimList`
- [ ] `RemoveClaim`
- [ ] `RemoveClaimList`
- [ ] `RemoveRoleClaim`
- [ ] `RemoveRoleClaimsList`
- [ ] `RemoveUserRole`
- [ ] `ReplaceClaim`
- [ ] `RopValidateCredentials`
- [ ] `SetTwoFactorEnabled`
- [ ] `SignOut`
- [ ] `TwoFactorAuthenticatorAppSignIn`
- [ ] `TwoFactorEmailSignIn`
- [ ] `TwoFactorRecoveryCodeSignIn`
- [ ] `TwoFactorSmsSignInAsync`
- [ ] `UnLockUserByToken`
- [ ] `UnLockUserByuserSecurityQuestions`
- [ ] `UpdateSecurityQuestion`
- [ ] `UpdateUserSecurityQuestion`
- [ ] `VerifyEmailTwoFactorToken`
- [ ] `VerifySmsTwoFactorToken`
- [ ] `VerifyUserToken`
