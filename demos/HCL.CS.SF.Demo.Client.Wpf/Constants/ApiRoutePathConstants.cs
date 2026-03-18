/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.DemoClientWpfApp.Constants
{
    public static class ApiRoutePathConstants
    {
        public const string BasePath = "/Security/Api";

        public const string UserApi = BasePath + "/User";

        public const string ClientApi = BasePath + "/Client";

        public const string RoleApi = BasePath + "/Role";

        public const string ApiResourceApi = BasePath + "/ApiResource";

        public const string IdentityApi = BasePath + "/IdentityResource";

        public const string AuditTrailApi = BasePath + "/Audittrail";

        public const string AuthenticationAPi = BasePath + "/Authentication";

        public const string SecurityTokenApi = BasePath + "/SecurityToken";

        public const string AddApiResource = ApiResourceApi + "/AddApiResource";

        public const string UpdateApiResource = ApiResourceApi + "/UpdateApiResource";

        public const string DeleteApiResourceById = ApiResourceApi + "/DeleteApiResourceById";

        public const string DeleteApiResourceByName = ApiResourceApi + "/DeleteApiResourceByName";

        public const string GetApiResourceById = ApiResourceApi + "/GetApiResourceById";

        public const string GetApiResourceByName = ApiResourceApi + "/GetApiResourceByName";

        public const string GetAllApiResources = ApiResourceApi + "/GetAllApiResources";

        public const string GetAllApiResourcesByScopesAsync = ApiResourceApi + "/GetAllApiResourcesByScopesAsync";

        public const string AddApiResourceClaim = ApiResourceApi + "/AddApiResourceClaim";

        public const string DeleteApiResourceClaimById = ApiResourceApi + "/DeleteApiResourceClaimById";

        public const string DeleteApiResourceClaimByResourceIdAsync = ApiResourceApi + "/DeleteApiResourceClaimByResourceIdAsync";

        public const string DeleteApiResourceClaimModel = ApiResourceApi + "/DeleteApiResourceClaimModel";

        public const string GetApiResourceClaimsById = ApiResourceApi + "/GetApiResourceClaimsById";

        public const string AddApiScope = ApiResourceApi + "/AddApiScope";

        public const string UpdateApiScope = ApiResourceApi + "/UpdateApiScope";

        public const string DeleteApiScopeById = ApiResourceApi + "/DeleteApiScopeById";

        public const string DeleteApiScopeByName = ApiResourceApi + "/DeleteApiScopeByName";

        public const string GetApiScopeById = ApiResourceApi + "/GetApiScopeById";

        public const string GetApiScopeByName = ApiResourceApi + "/GetApiScopeByName";

        public const string GetAllApiScopes = ApiResourceApi + "/GetAllApiScopes";

        public const string AddApiScopeClaim = ApiResourceApi + "/AddApiScopeClaim";

        public const string DeleteApiScopeClaimByScopeId = ApiResourceApi + "/DeleteApiScopeClaimByScopeId";

        public const string DeleteApiScopeClaimById = ApiResourceApi + "/DeleteApiScopeClaimById";

        public const string DeleteApiScopeClaimModel = ApiResourceApi + "/DeleteApiScopeClaimModel";

        public const string GetApiScopeClaims = ApiResourceApi + "/GetApiScopeClaims";

        public const string AddIdentityResource = IdentityApi + "/AddIdentityResource";

        public const string UpdateIdentityResource = IdentityApi + "/UpdateIdentityResource";

        public const string DeleteIdentityResourceById = IdentityApi + "/DeleteIdentityResourceById";

        public const string DeleteIdentityResourceByName = IdentityApi + "/DeleteIdentityResourceByName";

        public const string GetIdentityResourceById = IdentityApi + "/GetIdentityResourceById";

        public const string GetIdentityResourceByName = IdentityApi + "/GetIdentityResourceByName";

        public const string GetAllIdentityResources = IdentityApi + "/GetAllIdentityResources";

        public const string AddIdentityResourceClaim = IdentityApi + "/AddIdentityResourceClaim";

        public const string DeleteIdentityResourceClaimById = IdentityApi + "/DeleteIdentityResourceClaimById";

        public const string DeleteIdentityResourceClaimByResourceId = IdentityApi + "/DeleteIdentityResourceClaimByResourceIdAsync";

        public const string DeleteIdentityResourceClaimModel = IdentityApi + "/DeleteIdentityResourceClaimModel";

        public const string GetIdentityResourceClaims = IdentityApi + "/GetIdentityResourceClaims";

        public const string CreateRole = RoleApi + "/CreateRole";

        public const string UpdateRole = RoleApi + "/UpdateRole";

        public const string DeleteRoleById = RoleApi + "/DeleteRoleById";

        public const string DeleteRoleByName = RoleApi + "/DeleteRoleByName";

        public const string GetRoleById = RoleApi + "/GetRoleById";

        public const string GetRoleByName = RoleApi + "/GetRoleByName";

        public const string GetAllRoles = RoleApi + "/GetAllRoles";

        public const string AddRoleClaim = RoleApi + "/AddRoleClaim";

        public const string AddRoleClaimList = RoleApi + "/AddRoleClaimList";

        public const string RemoveRoleClaim = RoleApi + "/RemoveRoleClaim";

        public const string RemoveRoleClaimsList = RoleApi + "/RemoveRoleClaimsList";

        public const string RemoveRoleClaimsById = RoleApi + "/RemoveRoleClaimsById";

        public const string GetRoleClaim = RoleApi + "/GetRoleClaim";

        public const string AddAdminClaim = UserApi + "/AddAdminClaim";

        public const string AddAdminClaimList = UserApi + "/AddAdminClaimList";

        public const string AddClaim = UserApi + "/AddClaim";

        public const string AddClaimList = UserApi + "/AddClaimList";

        public const string AddSecurityQuestion = UserApi + "/AddSecurityQuestion";

        public const string AddUserRole = UserApi + "/AddUserRole";

        public const string AddUserRolesList = UserApi + "/AddUserRolesList";

        public const string AddUserSecurityQuestion = UserApi + "/AddUserSecurityQuestion";

        public const string AddUserSecurityQuestionList = UserApi + "/AddUserSecurityQuestionList";

        public const string ChangePassword = UserApi + "/ChangePassword";

        public const string DeleteSecurityQuestion = UserApi + "/DeleteSecurityQuestion";

        public const string DeleteUserByName = UserApi + "/DeleteUserByName";

        public const string GetAllUsers = UserApi + "/GetAllUsers";

        public const string DeleteUserById = UserApi + "/DeleteUserById";

        public const string DeleteUserSecurityQuestion = UserApi + "/DeleteUserSecurityQuestion";

        public const string DeleteUserSecurityQuestionList = UserApi + "/DeleteUserSecurityQuestionList";

        public const string GenerateEmailConfirmationToken = UserApi + "/GenerateEmailConfirmationToken";

        public const string GeneratePhoneNumberConfirmationToken = UserApi + "/GeneratePhoneNumberConfirmationToken";

        public const string GenerateEmailTwoFactorToken = UserApi + "/GenerateEmailTwoFactorToken";

        public const string GeneratePasswordResetToken = UserApi + "/GeneratePasswordResetToken";

        public const string GenerateSmsTwoFactorToken = UserApi + "/GenerateSmsTwoFactorToken";

        public const string GenerateUserTokenAsync = UserApi + "/GenerateUserTokenAsync";

        public const string GetAllSecurityQuestions = UserApi + "/GetAllSecurityQuestions";

        public const string GetAllTwoFactorType = UserApi + "/GetAllTwoFactorType";

        public const string GetClaims = UserApi + "/GetClaims";

        public const string GetUserByEmail = UserApi + "/GetUserByEmail";

        public const string GetUserById = UserApi + "/GetUserById";

        public const string GetUserByName = UserApi + "/GetUserByName";

        public const string GetUserClaims = UserApi + "/GetUserClaims";

        public const string GetAdminUserClaims = UserApi + "/GetAdminUserClaims";

        public const string GetUserRoleClaimsById = UserApi + "/GetUserRoleClaimsById";

        public const string GetUserRoleClaimsByName = UserApi + "/GetUserRoleClaimsByName";

        public const string GetUserRoles = UserApi + "/GetUserRoles";

        public const string GetUserSecurityQuestions = UserApi + "/GetUserSecurityQuestions";

        public const string GetUsersForClaim = UserApi + "/GetUsersForClaim";

        public const string GetUsersInRole = UserApi + "/GetUsersInRole";

        public const string IsUserExistsByClaimPrincipal = UserApi + "/IsUserExistsByClaimPrincipal";

        public const string IsUserExistsById = UserApi + "/IsUserExistsById";

        public const string IsUserExistsByName = UserApi + "/IsUserExistsByName";

        public const string LockUser = UserApi + "/LockUser";

        public const string LockUserByTime = UserApi + "/LockUserByTime";

        public const string RegisterUser = UserApi + "/RegisterUser";

        public const string RemoveAdminClaim = UserApi + "/RemoveAdminClaim";

        public const string RemoveAdminClaimList = UserApi + "/RemoveAdminClaimList";

        public const string RemoveClaim = UserApi + "/RemoveClaim";

        public const string RemoveClaimList = UserApi + "/RemoveClaimList";

        public const string RemoveUserRole = UserApi + "/RemoveUserRole";

        public const string RemoveUserRoleList = UserApi + "/RemoveUserRoleList";

        public const string ReplaceClaim = UserApi + "/ReplaceClaim";

        public const string ResetPassword = UserApi + "/ResetPassword";

        public const string SetTwoFactorEnabled = UserApi + "/SetTwoFactorEnabled";

        public const string UnLockUser = UserApi + "/UnLockUser";

        public const string UnLockUserByToken = UserApi + "/UnLockUserByToken";

        public const string UnLockUserByuserSecurityQuestions = UserApi + "/UnLockUserByuserSecurityQuestions";

        public const string UpdateSecurityQuestion = UserApi + "/UpdateSecurityQuestion";

        public const string UpdateUser = UserApi + "/UpdateUser";

        public const string UpdateUserSecurityQuestion = UserApi + "/UpdateUserSecurityQuestion";

        public const string UpdateUserTwoFactorType = UserApi + "/UpdateUserTwoFactorType";

        public const string VerifyEmailConfirmationToken = UserApi + "/VerifyEmailConfirmationToken";

        public const string VerifyEmailTwoFactorToken = UserApi + "/VerifyEmailTwoFactorToken";

        public const string VerifyPhoneNumberConfirmationToken = UserApi + "/VerifyPhoneNumberConfirmationToken";

        public const string VerifySmsTwoFactorToken = UserApi + "/VerifySmsTwoFactorToken";

        public const string VerifyUserToken = UserApi + "/VerifyUserToken";

        public const string LockUserPath = BasePath + "/lockuser";

        public const string LockUserWithEndDatePath = BasePath + "/lockuserwithenddate";

        public const string AddAuditTrail = AuditTrailApi + "/AddAuditTrail";

        public const string AddAuditTrailModel = AuditTrailApi + "/AddAuditTrailModel";

        public const string GetAuditDetails = AuditTrailApi + "/GetAuditDetails";

        //public const string GetAuditDetailsByCreatedOn = AuditTrailApi + "/GetAuditDetailsByCreatedOn";

        //public const string GetAuditDetailsByFromDate = AuditTrailApi + "/GetAuditDetailsByFromDate";

        //public const string GetAuditDetailsByActionType = AuditTrailApi + "/GetAuditDetailsByActionType";

        public const string GenerateRecoveryCodes = AuthenticationAPi + "/GenerateRecoveryCodes";

        public const string IsUserSignedIn = AuthenticationAPi + "/IsUserSignedIn";

        public const string PasswordSignIn = AuthenticationAPi + "/PasswordSignIn";

        public const string PasswordSignInByTwoFactorAuthenticatorToken = AuthenticationAPi + "/PasswordSignInByTwoFactorAuthenticatorToken";

        public const string ResetAuthenticatorApp = AuthenticationAPi + "/ResetAuthenticatorApp";

        public const string RopValidateCredentials = AuthenticationAPi + "/RopValidateCredentials";

        public const string SetupAuthenticatorApp = AuthenticationAPi + "/SetupAuthenticatorApp";

        public const string SignOut = AuthenticationAPi + "/SignOut";

        public const string TwoFactorAuthenticatorAppSignIn = AuthenticationAPi + "/TwoFactorAuthenticatorAppSignIn";

        public const string TwoFactorEmailSignIn = AuthenticationAPi + "/TwoFactorEmailSignIn";

        public const string TwoFactorRecoveryCodeSignIn = AuthenticationAPi + "/TwoFactorRecoveryCodeSignIn";

        public const string TwoFactorSmsSignInAsync = AuthenticationAPi + "/TwoFactorSmsSignInAsync";

        public const string VerifyAuthenticatorAppSetup = AuthenticationAPi + "/VerifyAuthenticatorAppSetup";

        public const string CountRecoveryCodesAsync = AuthenticationAPi + "/CountRecoveryCodesAsync";

        public const string DeleteClient = ClientApi + "/DeleteClient";

        public const string GenerateClientSecret = ClientApi + "/GenerateClientSecret";

        public const string GetAllClient = ClientApi + "/GetAllClient";

        public const string GetClient = ClientApi + "/GetClient";

        public const string RegisterClient = ClientApi + "/RegisterClient";

        public const string UpdateClient = ClientApi + "/UpdateClient";

        public const string GetActiveSecurityTokensByClientIds = SecurityTokenApi + "/GetActiveSecurityTokensByClientIds";

        public const string GetActiveSecurityTokensByUserIds = SecurityTokenApi + "/GetActiveSecurityTokensByUserIds";

        public const string GetActiveSecurityTokensBetweenDates = SecurityTokenApi + "/GetActiveSecurityTokensBetweenDates";

        public const string GetAllSecurityTokensBetweenDates = SecurityTokenApi + "/GetAllSecurityTokensBetweenDates";

    }
}


