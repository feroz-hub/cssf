# HCL.CS.SF API Inventory

Generated from route sources in code (gateway handlers, endpoint registration, controller attributes, and program mappings).

## Identity Management APIs (Gateway)

These endpoints are served by `UseHCL.CS.SFApi` and currently processed as `POST` routes.

| Method | Path | Handler | Route Key |
|---|---|---|---|
| POST | `/Security/Api/ApiResource/AddApiResource` | `AddApiResource` | `AddApiResource` |
| POST | `/Security/Api/ApiResource/AddApiResourceClaim` | `AddApiResourceClaim` | `AddApiResourceClaim` |
| POST | `/Security/Api/ApiResource/AddApiScope` | `AddApiScope` | `AddApiScope` |
| POST | `/Security/Api/ApiResource/AddApiScopeClaim` | `AddApiScopeClaim` | `AddApiScopeClaim` |
| POST | `/Security/Api/ApiResource/DeleteApiResourceById` | `DeleteApiResourceById` | `DeleteApiResourceById` |
| POST | `/Security/Api/ApiResource/DeleteApiResourceByName` | `DeleteApiResourceByName` | `DeleteApiResourceByName` |
| POST | `/Security/Api/ApiResource/DeleteApiResourceClaimById` | `DeleteApiResourceClaimById` | `DeleteApiResourceClaimById` |
| POST | `/Security/Api/ApiResource/DeleteApiResourceClaimByResourceIdAsync` | `DeleteApiResourceClaimByResourceIdAsync` | `DeleteApiResourceClaimByResourceIdAsync` |
| POST | `/Security/Api/ApiResource/DeleteApiResourceClaimModel` | `DeleteApiResourceClaimModel` | `DeleteApiResourceClaimModel` |
| POST | `/Security/Api/ApiResource/DeleteApiScopeById` | `DeleteApiScopeById` | `DeleteApiScopeById` |
| POST | `/Security/Api/ApiResource/DeleteApiScopeByName` | `DeleteApiScopeByName` | `DeleteApiScopeByName` |
| POST | `/Security/Api/ApiResource/DeleteApiScopeClaimById` | `DeleteApiScopeClaimById` | `DeleteApiScopeClaimById` |
| POST | `/Security/Api/ApiResource/DeleteApiScopeClaimByScopeId` | `DeleteApiScopeClaimByScopeId` | `DeleteApiScopeClaimByScopeId` |
| POST | `/Security/Api/ApiResource/DeleteApiScopeClaimModel` | `DeleteApiScopeClaimModel` | `DeleteApiScopeClaimModel` |
| POST | `/Security/Api/ApiResource/GetAllApiResources` | `GetAllApiResources` | `GetAllApiResources` |
| POST | `/Security/Api/ApiResource/GetAllApiResourcesByScopesAsync` | `GetAllApiResourcesByScopesAsync` | `GetAllApiResourcesByScopesAsync` |
| POST | `/Security/Api/ApiResource/GetAllApiScopes` | `GetAllApiScopes` | `GetAllApiScopes` |
| POST | `/Security/Api/ApiResource/GetApiResourceById` | `GetApiResourceById` | `GetApiResourceById` |
| POST | `/Security/Api/ApiResource/GetApiResourceByName` | `GetApiResourceByName` | `GetApiResourceByName` |
| POST | `/Security/Api/ApiResource/GetApiResourceClaimsById` | `GetApiResourceClaimsById` | `GetApiResourceClaimsById` |
| POST | `/Security/Api/ApiResource/GetApiScopeById` | `GetApiScopeById` | `GetApiScopeById` |
| POST | `/Security/Api/ApiResource/GetApiScopeByName` | `GetApiScopeByName` | `GetApiScopeByName` |
| POST | `/Security/Api/ApiResource/GetApiScopeClaims` | `GetApiScopeClaims` | `GetApiScopeClaims` |
| POST | `/Security/Api/ApiResource/UpdateApiResource` | `UpdateApiResource` | `UpdateApiResource` |
| POST | `/Security/Api/ApiResource/UpdateApiScope` | `UpdateApiScope` | `UpdateApiScope` |
| POST | `/Security/Api/Audittrail/AddAuditTrail` | `AddAuditTrail` | `AddAuditTrail` |
| POST | `/Security/Api/Audittrail/AddAuditTrailModel` | `AddAuditTrailModel` | `AddAuditTrailModel` |
| POST | `/Security/Api/Audittrail/GetAuditDetails` | `GetAuditDetailsAsync` | `GetAuditDetails` |
| POST | `/Security/Api/Authentication/CountRecoveryCodesAsync` | `CountRecoveryCodesAsync` | `CountRecoveryCodesAsync` |
| POST | `/Security/Api/Authentication/GenerateRecoveryCodes` | `GenerateRecoveryCodes` | `GenerateRecoveryCodes` |
| POST | `/Security/Api/Authentication/IsUserSignedIn` | `IsUserSignedIn` | `IsUserSignedIn` |
| POST | `/Security/Api/Authentication/PasswordSignIn` | `PasswordSignIn` | `PasswordSignIn` |
| POST | `/Security/Api/Authentication/PasswordSignInByTwoFactorAuthenticatorToken` | `PasswordSignInByTwoFactorAuthenticatorToken` | `PasswordSignInByTwoFactorAuthenticatorToken` |
| POST | `/Security/Api/Authentication/ResetAuthenticatorApp` | `ResetAuthenticatorApp` | `ResetAuthenticatorApp` |
| POST | `/Security/Api/Authentication/RopValidateCredentials` | `RopValidateCredentials` | `RopValidateCredentials` |
| POST | `/Security/Api/Authentication/SetupAuthenticatorApp` | `SetupAuthenticatorApp` | `SetupAuthenticatorApp` |
| POST | `/Security/Api/Authentication/SignOut` | `SignOut` | `SignOut` |
| POST | `/Security/Api/Authentication/TwoFactorAuthenticatorAppSignIn` | `TwoFactorAuthenticatorAppSignIn` | `TwoFactorAuthenticatorAppSignIn` |
| POST | `/Security/Api/Authentication/TwoFactorEmailSignIn` | `TwoFactorEmailSignIn` | `TwoFactorEmailSignIn` |
| POST | `/Security/Api/Authentication/TwoFactorRecoveryCodeSignIn` | `TwoFactorRecoveryCodeSignIn` | `TwoFactorRecoveryCodeSignIn` |
| POST | `/Security/Api/Authentication/TwoFactorSmsSignInAsync` | `TwoFactorSmsSignInAsync` | `TwoFactorSmsSignInAsync` |
| POST | `/Security/Api/Authentication/VerifyAuthenticatorAppSetup` | `VerifyAuthenticatorAppSetup` | `VerifyAuthenticatorAppSetup` |
| POST | `/Security/Api/Client/DeleteClient` | `DeleteClient` | `DeleteClient` |
| POST | `/Security/Api/Client/GenerateClientSecret` | `GenerateClientSecret` | `GenerateClientSecret` |
| POST | `/Security/Api/Client/GetAllClient` | `GetAllClient` | `GetAllClient` |
| POST | `/Security/Api/Client/GetClient` | `GetClient` | `GetClient` |
| POST | `/Security/Api/Client/RegisterClient` | `RegisterClient` | `RegisterClient` |
| POST | `/Security/Api/Client/UpdateClient` | `UpdateClient` | `UpdateClient` |
| POST | `/Security/Api/IdentityResource/AddIdentityResource` | `AddIdentityResource` | `AddIdentityResource` |
| POST | `/Security/Api/IdentityResource/AddIdentityResourceClaim` | `AddIdentityResourceClaim` | `AddIdentityResourceClaim` |
| POST | `/Security/Api/IdentityResource/DeleteIdentityResourceById` | `DeleteIdentityResourceById` | `DeleteIdentityResourceById` |
| POST | `/Security/Api/IdentityResource/DeleteIdentityResourceByName` | `DeleteIdentityResourceByName` | `DeleteIdentityResourceByName` |
| POST | `/Security/Api/IdentityResource/DeleteIdentityResourceClaimById` | `DeleteIdentityResourceClaimByIdAsync` | `DeleteIdentityResourceClaimById` |
| POST | `/Security/Api/IdentityResource/DeleteIdentityResourceClaimByResourceIdAsync` | `DeleteIdentityResourceClaimByResourceIdAsync` | `DeleteIdentityResourceClaimByResourceId` |
| POST | `/Security/Api/IdentityResource/DeleteIdentityResourceClaimModel` | `DeleteIdentityResourceClaimModel` | `DeleteIdentityResourceClaimModel` |
| POST | `/Security/Api/IdentityResource/GetAllIdentityResources` | `GetAllIdentityResources` | `GetAllIdentityResources` |
| POST | `/Security/Api/IdentityResource/GetIdentityResourceById` | `GetIdentityResourceById` | `GetIdentityResourceById` |
| POST | `/Security/Api/IdentityResource/GetIdentityResourceByName` | `GetIdentityResourceByName` | `GetIdentityResourceByName` |
| POST | `/Security/Api/IdentityResource/GetIdentityResourceClaims` | `GetIdentityResourceClaims` | `GetIdentityResourceClaims` |
| POST | `/Security/Api/IdentityResource/UpdateIdentityResource` | `UpdateIdentityResource` | `UpdateIdentityResource` |
| POST | `/Security/Api/Role/AddRoleClaim` | `AddRoleClaim` | `AddRoleClaim` |
| POST | `/Security/Api/Role/AddRoleClaimList` | `AddRoleClaimList` | `AddRoleClaimList` |
| POST | `/Security/Api/Role/CreateRole` | `CreateRole` | `CreateRole` |
| POST | `/Security/Api/Role/DeleteRoleById` | `DeleteRoleById` | `DeleteRoleById` |
| POST | `/Security/Api/Role/DeleteRoleByName` | `DeleteRoleByName` | `DeleteRoleByName` |
| POST | `/Security/Api/Role/GetAllRoles` | `GetAllRoles` | `GetAllRoles` |
| POST | `/Security/Api/Role/GetRoleById` | `GetRoleById` | `GetRoleById` |
| POST | `/Security/Api/Role/GetRoleByName` | `GetRoleByName` | `GetRoleByName` |
| POST | `/Security/Api/Role/GetRoleClaim` | `GetRoleClaim` | `GetRoleClaim` |
| POST | `/Security/Api/Role/RemoveRoleClaim` | `RemoveRoleClaim` | `RemoveRoleClaim` |
| POST | `/Security/Api/Role/RemoveRoleClaimsById` | `RemoveRoleClaimById` | `RemoveRoleClaimsById` |
| POST | `/Security/Api/Role/RemoveRoleClaimsList` | `RemoveRoleClaimsList` | `RemoveRoleClaimsList` |
| POST | `/Security/Api/Role/UpdateRole` | `UpdateRoleAsync` | `UpdateRole` |
| POST | `/Security/Api/SecurityToken/GetActiveSecurityTokensBetweenDates` | `GetActiveSecurityTokensBetweenDates` | `GetActiveSecurityTokensBetweenDates` |
| POST | `/Security/Api/SecurityToken/GetActiveSecurityTokensByClientIds` | `GetActiveSecurityTokensByClientIds` | `GetActiveSecurityTokensByClientIds` |
| POST | `/Security/Api/SecurityToken/GetActiveSecurityTokensByUserIds` | `GetActiveSecurityTokensByUserIds` | `GetActiveSecurityTokensByUserIds` |
| POST | `/Security/Api/SecurityToken/GetAllSecurityTokensBetweenDates` | `GetAllSecurityTokensBetweenDates` | `GetAllSecurityTokensBetweenDates` |
| POST | `/Security/Api/User/AddAdminClaim` | `AddAdminClaim` | `AddAdminClaim` |
| POST | `/Security/Api/User/AddAdminClaimList` | `AddAdminClaimList` | `AddAdminClaimList` |
| POST | `/Security/Api/User/AddClaim` | `AddClaim` | `AddClaim` |
| POST | `/Security/Api/User/AddClaimList` | `AddClaimList` | `AddClaimList` |
| POST | `/Security/Api/User/AddSecurityQuestion` | `AddSecurityQuestion` | `AddSecurityQuestion` |
| POST | `/Security/Api/User/AddUserRole` | `AddUserRole` | `AddUserRole` |
| POST | `/Security/Api/User/AddUserRolesList` | `AddUserRolesList` | `AddUserRolesList` |
| POST | `/Security/Api/User/AddUserSecurityQuestion` | `AddUserSecurityQuestion` | `AddUserSecurityQuestion` |
| POST | `/Security/Api/User/AddUserSecurityQuestionList` | `AddUserSecurityQuestionList` | `AddUserSecurityQuestionList` |
| POST | `/Security/Api/User/ChangePassword` | `ChangePassword` | `ChangePassword` |
| POST | `/Security/Api/User/DeleteSecurityQuestion` | `DeleteSecurityQuestion` | `DeleteSecurityQuestion` |
| POST | `/Security/Api/User/DeleteUserById` | `DeleteUserById` | `DeleteUserById` |
| POST | `/Security/Api/User/DeleteUserByName` | `DeleteUserByName` | `DeleteUserByName` |
| POST | `/Security/Api/User/DeleteUserSecurityQuestion` | `DeleteUserSecurityQuestion` | `DeleteUserSecurityQuestion` |
| POST | `/Security/Api/User/DeleteUserSecurityQuestionList` | `DeleteUserSecurityQuestionList` | `DeleteUserSecurityQuestionList` |
| POST | `/Security/Api/User/GenerateEmailConfirmationToken` | `GenerateEmailConfirmationToken` | `GenerateEmailConfirmationToken` |
| POST | `/Security/Api/User/GenerateEmailTwoFactorToken` | `GenerateEmailTwoFactorToken` | `GenerateEmailTwoFactorToken` |
| POST | `/Security/Api/User/GeneratePasswordResetToken` | `GeneratePasswordResetToken` | `GeneratePasswordResetToken` |
| POST | `/Security/Api/User/GeneratePhoneNumberConfirmationToken` | `GeneratePhoneNumberConfirmationToken` | `GeneratePhoneNumberConfirmationToken` |
| POST | `/Security/Api/User/GenerateSmsTwoFactorToken` | `GenerateSmsTwoFactorToken` | `GenerateSmsTwoFactorToken` |
| POST | `/Security/Api/User/GenerateUserTokenAsync` | `GenerateUserTokenAsync` | `GenerateUserTokenAsync` |
| POST | `/Security/Api/User/GetAdminUserClaims` | `GetAdminUserClaims` | `GetAdminUserClaims` |
| POST | `/Security/Api/User/GetAllSecurityQuestions` | `GetAllSecurityQuestions` | `GetAllSecurityQuestions` |
| POST | `/Security/Api/User/GetAllTwoFactorType` | `GetAllTwoFactorType` | `GetAllTwoFactorType` |
| POST | `/Security/Api/User/GetAllUsers` | `GetAllUsers` | `GetAllUsers` |
| POST | `/Security/Api/User/GetClaims` | `GetClaims` | `GetClaims` |
| POST | `/Security/Api/User/GetUserByEmail` | `GetUserByEmail` | `GetUserByEmail` |
| POST | `/Security/Api/User/GetUserById` | `GetUserById` | `GetUserById` |
| POST | `/Security/Api/User/GetUserByName` | `GetUserByName` | `GetUserByName` |
| POST | `/Security/Api/User/GetUserClaims` | `GetUserClaims` | `GetUserClaims` |
| POST | `/Security/Api/User/GetUserRoleClaimsById` | `GetUserRoleClaimsById` | `GetUserRoleClaimsById` |
| POST | `/Security/Api/User/GetUserRoleClaimsByName` | `GetUserRoleClaimsByName` | `GetUserRoleClaimsByName` |
| POST | `/Security/Api/User/GetUserRoles` | `GetUserRoles` | `GetUserRoles` |
| POST | `/Security/Api/User/GetUserSecurityQuestions` | `GetUserSecurityQuestions` | `GetUserSecurityQuestions` |
| POST | `/Security/Api/User/GetUsersForClaim` | `GetUsersForClaim` | `GetUsersForClaim` |
| POST | `/Security/Api/User/GetUsersInRole` | `GetUsersInRole` | `GetUsersInRole` |
| POST | `/Security/Api/User/IsUserExistsByClaimPrincipal` | `IsUserExistsByClaimPrincipal` | `IsUserExistsByClaimPrincipal` |
| POST | `/Security/Api/User/IsUserExistsById` | `IsUserExistsById` | `IsUserExistsById` |
| POST | `/Security/Api/User/IsUserExistsByName` | `IsUserExistsByName` | `IsUserExistsByName` |
| POST | `/Security/Api/User/LockUser` | `LockUser` | `LockUser` |
| POST | `/Security/Api/User/RegisterUser` | `RegisterUser` | `RegisterUser` |
| POST | `/Security/Api/User/RemoveAdminClaim` | `RemoveAdminClaim` | `RemoveAdminClaim` |
| POST | `/Security/Api/User/RemoveAdminClaimList` | `RemoveAdminClaimList` | `RemoveAdminClaimList` |
| POST | `/Security/Api/User/RemoveClaim` | `RemoveClaim` | `RemoveClaim` |
| POST | `/Security/Api/User/RemoveClaimList` | `RemoveClaimList` | `RemoveClaimList` |
| POST | `/Security/Api/User/RemoveUserRole` | `RemoveUserRole` | `RemoveUserRole` |
| POST | `/Security/Api/User/RemoveUserRoleList` | `RemoveUserRoleList` | `RemoveUserRoleList` |
| POST | `/Security/Api/User/ReplaceClaim` | `ReplaceClaim` | `ReplaceClaim` |
| POST | `/Security/Api/User/ResetPassword` | `ResetPassword` | `ResetPassword` |
| POST | `/Security/Api/User/SetTwoFactorEnabled` | `SetTwoFactorEnabled` | `SetTwoFactorEnabled` |
| POST | `/Security/Api/User/UnLockUser` | `UnLockUser` | `UnLockUser` |
| POST | `/Security/Api/User/UnLockUserByToken` | `UnLockUserByToken` | `UnLockUserByToken` |
| POST | `/Security/Api/User/UnLockUserByuserSecurityQuestions` | `UnLockUserByuserSecurityQuestions` | `UnLockUserByuserSecurityQuestions` |
| POST | `/Security/Api/User/UpdateSecurityQuestion` | `UpdateSecurityQuestion` | `UpdateSecurityQuestion` |
| POST | `/Security/Api/User/UpdateUser` | `UpdateUser` | `UpdateUser` |
| POST | `/Security/Api/User/UpdateUserSecurityQuestion` | `UpdateUserSecurityQuestion` | `UpdateUserSecurityQuestion` |
| POST | `/Security/Api/User/UpdateUserTwoFactorType` | `UpdateUserTwoFactorType` | `UpdateUserTwoFactorType` |
| POST | `/Security/Api/User/VerifyEmailConfirmationToken` | `VerifyEmailConfirmationToken` | `VerifyEmailConfirmationToken` |
| POST | `/Security/Api/User/VerifyEmailTwoFactorToken` | `VerifyEmailTwoFactorToken` | `VerifyEmailTwoFactorToken` |
| POST | `/Security/Api/User/VerifyPhoneNumberConfirmationToken` | `VerifyPhoneNumberConfirmationToken` | `VerifyPhoneNumberConfirmationToken` |
| POST | `/Security/Api/User/VerifySmsTwoFactorToken` | `VerifySmsTwoFactorToken` | `VerifySmsTwoFactorToken` |
| POST | `/Security/Api/User/VerifyUserToken` | `VerifyUserToken` | `VerifyUserToken` |
| POST | `/Security/Api/lockuserwithenddate` | `LockUserWithEndDate` | `LockUserWithEndDatePath` |

## OAuth/OIDC Endpoints

These endpoints are served by `UseHCL.CS.SFEndpoint` through registered `SecurityEndpointModel` handlers.

| Method | Path | Handler |
|---|---|---|
| GET/POST | `/security/authorize` | `AuthorizeEndpoint` |
| GET | `/security/authorize/authorizecallback` | `AuthorizeCallBackEndpoint` |
| POST | `/security/token` | `TokenEndpoint` |
| POST | `/security/introspect` | `IntrospectionEndpoint` |
| GET/POST | `/security/endsession` | `EndSessionEndpoint` |
| GET | `/security/endsession/callback` | `EndSessionCallbackEndpoint` |
| POST | `/security/revocation` | `TokenRevocationEndpoint` |
| GET | `/.well-known/openid-configuration/jwks` | `JwksEndpoint` |
| GET/POST | `/security/userinfo` | `UserInfoEndpoint` |
| GET (expected) | `/.well-known/openid-configuration` | `DiscoveryEndpoint` |

## External Auth (Demo Server)

| Method | Path | Handler |
|---|---|---|
| GET | `/auth/external/google/start` | `ExternalAuthController.GoogleStart` |
| GET | `/auth/external/google/callback` | `ExternalAuthController.GoogleCallback` |
| POST | `/auth/external/link/google` | `ExternalAuthController.LinkGoogle` |
| POST | `/auth/external/unlink/google` | `ExternalAuthController.UnlinkGoogle` |

## Installer APIs

| Method | Path | Handler |
|---|---|---|
| GET | `/` | `SetupController.Root` |
| GET | `/setup` | `SetupController.Provider` |
| POST | `/setup/provider` | `SetupController.SaveProvider` |
| GET | `/setup/connection` | `SetupController.Connection` |
| POST | `/setup/connection` | `SetupController.SaveConnection` |
| GET | `/setup/validate` | `SetupController.Validate` |
| POST | `/setup/validate` | `SetupController.ExecuteValidation` |
| GET | `/setup/migrate` | `SetupController.Migrate` |
| POST | `/setup/migrate` | `SetupController.RunMigrations` |
| GET | `/setup/seed` | `SetupController.Seed` |
| POST | `/setup/seed` | `SetupController.Seed` |
| GET | `/complete` | `SetupController.Complete` |
| GET | `/installed` | `SetupController.Installed` |
| GET | `/error` | `SetupController.Error` |

## Resource Server Demo APIs

| Method | Path | Handler |
|---|---|---|
## Health APIs

| App | Method | Path |
|---|---|---|
| Demo Server | GET | `/health/live` |
| Demo Server | GET | `/health/ready` |
| Installer MVC | GET | `/health` |
| Resource Server Demo | GET | `/health/live` |
