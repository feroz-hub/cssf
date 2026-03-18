/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Models.Api;

namespace HCL.CS.SF.Domain.Constants;

/// <summary>
/// Defines all Admin API route path constants and the master list of API route models
/// that map each API operation to its URL path and required permissions.
/// Used by the API gateway to route requests and enforce permission-based access control.
/// </summary>
public static class ApiRoutePathConstants
{
    /// <summary>
    /// The base URL path prefix for all Admin API endpoints.
    /// </summary>
    public const string BasePath = "/Security/Api";

    /// <summary>
    /// Base path for all User management API endpoints.
    /// </summary>
    public const string UserApi = BasePath + "/User";

    /// <summary>
    /// Base path for all OAuth Client management API endpoints.
    /// </summary>
    public const string ClientApi = BasePath + "/Client";

    /// <summary>
    /// Base path for all Role management API endpoints.
    /// </summary>
    public const string RoleApi = BasePath + "/Role";

    /// <summary>
    /// Base path for all API Resource and API Scope management endpoints.
    /// </summary>
    public const string ApiResourceApi = BasePath + "/ApiResource";

    /// <summary>
    /// Base path for all Identity Resource management endpoints.
    /// </summary>
    public const string IdentityApi = BasePath + "/IdentityResource";

    /// <summary>
    /// Base path for all Audit Trail endpoints.
    /// </summary>
    public const string AuditTrailApi = BasePath + "/Audittrail";

    /// <summary>
    /// Base path for all Authentication endpoints (sign-in, sign-out, 2FA, authenticator app).
    /// </summary>
    public const string AuthenticationAPi = BasePath + "/Authentication";

    /// <summary>
    /// Base path for all Security Token query and management endpoints.
    /// </summary>
    public const string SecurityTokenApi = BasePath + "/SecurityToken";

    /// <summary>
    /// Base path for all Notification management endpoints (email/SMS templates, provider config, logs).
    /// </summary>
    public const string NotificationApi = BasePath + "/Notification";

    public const string GetNotificationLogs = NotificationApi + "/GetNotificationLogs";

    public const string GetNotificationTemplates = NotificationApi + "/GetNotificationTemplates";

    public const string GetProviderConfig = NotificationApi + "/GetProviderConfig";

    public const string GetAllProviderConfigs = NotificationApi + "/GetAllProviderConfigs";

    public const string SaveProviderConfig = NotificationApi + "/SaveProviderConfig";

    public const string SetActiveProvider = NotificationApi + "/SetActiveProvider";

    public const string DeleteProviderConfig = NotificationApi + "/DeleteProviderConfig";

    public const string GetProviderFieldDefinitions = NotificationApi + "/GetProviderFieldDefinitions";

    public const string SendTestNotification = NotificationApi + "/SendTestNotification";

    /// <summary>
    /// Base path for all External Authentication provider management endpoints (e.g., Google OIDC).
    /// </summary>
    public const string ExternalAuthApi = BasePath + "/ExternalAuth";

    public const string GetAllExternalAuthProviders = ExternalAuthApi + "/GetAllExternalAuthProviders";

    public const string GetExternalAuthProvider = ExternalAuthApi + "/GetExternalAuthProvider";

    public const string SaveExternalAuthProvider = ExternalAuthApi + "/SaveExternalAuthProvider";

    public const string DeleteExternalAuthProvider = ExternalAuthApi + "/DeleteExternalAuthProvider";

    public const string TestExternalAuthProvider = ExternalAuthApi + "/TestExternalAuthProvider";

    public const string GetExternalAuthFieldDefinitions = ExternalAuthApi + "/GetExternalAuthFieldDefinitions";

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

    public const string DeleteApiResourceClaimByResourceIdAsync =
        ApiResourceApi + "/DeleteApiResourceClaimByResourceIdAsync";

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

    public const string DeleteIdentityResourceClaimByResourceId =
        IdentityApi + "/DeleteIdentityResourceClaimByResourceIdAsync";

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

    public const string PasswordSignInByTwoFactorAuthenticatorToken =
        AuthenticationAPi + "/PasswordSignInByTwoFactorAuthenticatorToken";

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

    /// <summary>
    /// The master registry of all API route models. Each entry maps an API operation name to its
    /// URL path and the list of permission claims required to invoke it. The API gateway uses this
    /// list to enforce authorization before forwarding requests to the identity service.
    /// Operations listed with <see cref="ApiPermissionConstants.Anonymous"/> do not require authentication.
    /// </summary>
    public static readonly List<ApiRouteModel> ApiRouteModels = new()
    {
        // Api Resources
        new ApiRouteModel
        {
            Name = "AddApiResourceAsync", Path = AddApiResource,
            Permissions = new List<string>
                { ApiPermissionConstants.ApiResourceWrite, ApiPermissionConstants.ApiResourceManage }
        },
        new ApiRouteModel
        {
            Name = "UpdateApiResourceAsync", Path = UpdateApiResource,
            Permissions = new List<string>
                { ApiPermissionConstants.ApiResourceWrite, ApiPermissionConstants.ApiResourceManage }
        },
        new ApiRouteModel
        {
            Name = "DeleteApiResourceAsync", Path = DeleteApiResourceById,
            Permissions = new List<string>
                { ApiPermissionConstants.ApiResourceDelete, ApiPermissionConstants.ApiResourceManage }
        },
        new ApiRouteModel
        {
            Name = "DeleteApiResourceAsync", Path = DeleteApiResourceByName,
            Permissions = new List<string>
                { ApiPermissionConstants.ApiResourceDelete, ApiPermissionConstants.ApiResourceManage }
        },
        new ApiRouteModel
        {
            Name = "GetApiResourceAsync", Path = GetApiResourceById,
            Permissions = new List<string>
                { ApiPermissionConstants.ApiResourceRead, ApiPermissionConstants.ApiResourceManage }
        },
        new ApiRouteModel
        {
            Name = "GetApiResourceAsync", Path = GetApiResourceByName,
            Permissions = new List<string>
                { ApiPermissionConstants.ApiResourceRead, ApiPermissionConstants.ApiResourceManage }
        },
        new ApiRouteModel
        {
            Name = "GetAllApiResourcesAsync", Path = GetAllApiResources,
            Permissions = new List<string>
                { ApiPermissionConstants.ApiResourceRead, ApiPermissionConstants.ApiResourceManage }
        },
        new ApiRouteModel
        {
            Name = "GetAllApiResourcesByScopesAsync", Path = GetAllApiResourcesByScopesAsync,
            Permissions = new List<string>
                { ApiPermissionConstants.ApiResourceRead, ApiPermissionConstants.ApiResourceManage }
        },

        // Api Resource Claim
        new ApiRouteModel
        {
            Name = "AddApiResourceClaimAsync", Path = AddApiResourceClaim,
            Permissions = new List<string>
                { ApiPermissionConstants.ApiResourceWrite, ApiPermissionConstants.ApiResourceManage }
        },
        new ApiRouteModel
        {
            Name = "DeleteApiResourceClaimByIdAsync", Path = DeleteApiResourceClaimById,
            Permissions = new List<string>
                { ApiPermissionConstants.ApiResourceDelete, ApiPermissionConstants.ApiResourceManage }
        },
        new ApiRouteModel
        {
            Name = "DeleteApiResourceClaimAsync", Path = DeleteApiResourceClaimByResourceIdAsync,
            Permissions = new List<string>
                { ApiPermissionConstants.ApiResourceDelete, ApiPermissionConstants.ApiResourceManage }
        },
        new ApiRouteModel
        {
            Name = "DeleteApiResourceClaimAsync", Path = DeleteApiResourceClaimModel,
            Permissions = new List<string>
                { ApiPermissionConstants.ApiResourceDelete, ApiPermissionConstants.ApiResourceManage }
        },
        new ApiRouteModel
        {
            Name = "GetApiResourceClaimsAsync", Path = GetApiResourceClaimsById,
            Permissions = new List<string>
                { ApiPermissionConstants.ApiResourceRead, ApiPermissionConstants.ApiResourceManage }
        },

        // Api Scope
        new ApiRouteModel
        {
            Name = "AddApiScopeAsync", Path = AddApiScope,
            Permissions = new List<string>
                { ApiPermissionConstants.ApiResourceWrite, ApiPermissionConstants.ApiResourceManage }
        },
        new ApiRouteModel
        {
            Name = "UpdateApiScopeAsync", Path = UpdateApiScope,
            Permissions = new List<string>
                { ApiPermissionConstants.ApiResourceWrite, ApiPermissionConstants.ApiResourceManage }
        },
        new ApiRouteModel
        {
            Name = "DeleteApiScopeAsync", Path = DeleteApiScopeById,
            Permissions = new List<string>
                { ApiPermissionConstants.ApiResourceDelete, ApiPermissionConstants.ApiResourceManage }
        },
        new ApiRouteModel
        {
            Name = "DeleteApiScopeAsync", Path = DeleteApiScopeByName,
            Permissions = new List<string>
                { ApiPermissionConstants.ApiResourceDelete, ApiPermissionConstants.ApiResourceManage }
        },
        new ApiRouteModel
        {
            Name = "GetApiScopeAsync", Path = GetApiScopeById,
            Permissions = new List<string>
                { ApiPermissionConstants.ApiResourceRead, ApiPermissionConstants.ApiResourceManage }
        },
        new ApiRouteModel
        {
            Name = "GetApiScopeAsync", Path = GetApiScopeByName,
            Permissions = new List<string>
                { ApiPermissionConstants.ApiResourceRead, ApiPermissionConstants.ApiResourceManage }
        },
        new ApiRouteModel
        {
            Name = "GetAllApiScopesAsync", Path = GetAllApiScopes,
            Permissions = new List<string>
                { ApiPermissionConstants.ApiResourceRead, ApiPermissionConstants.ApiResourceManage }
        },
        new ApiRouteModel
        {
            Name = "AddApiScopeClaimAsync", Path = AddApiScopeClaim,
            Permissions = new List<string>
                { ApiPermissionConstants.ApiResourceWrite, ApiPermissionConstants.ApiResourceManage }
        },
        new ApiRouteModel
        {
            Name = "DeleteApiScopeClaimAsync", Path = DeleteApiScopeClaimByScopeId,
            Permissions = new List<string>
                { ApiPermissionConstants.ApiResourceDelete, ApiPermissionConstants.ApiResourceManage }
        },
        new ApiRouteModel
        {
            Name = "DeleteApiScopeClaimByIdAsync", Path = DeleteApiScopeClaimById,
            Permissions = new List<string>
                { ApiPermissionConstants.ApiResourceDelete, ApiPermissionConstants.ApiResourceManage }
        },
        new ApiRouteModel
        {
            Name = "DeleteApiScopeClaimAsync", Path = DeleteApiScopeClaimModel,
            Permissions = new List<string>
                { ApiPermissionConstants.ApiResourceDelete, ApiPermissionConstants.ApiResourceManage }
        },
        new ApiRouteModel
        {
            Name = "GetApiScopeClaimsAsync", Path = GetApiScopeClaims,
            Permissions = new List<string>
                { ApiPermissionConstants.ApiResourceRead, ApiPermissionConstants.ApiResourceManage }
        },

        // User account service
        new ApiRouteModel
        {
            Name = "LockUserAsync", Path = LockUser,
            Permissions = new List<string> { ApiPermissionConstants.AdminWrite, ApiPermissionConstants.AdminManage }
        },
        new ApiRouteModel
        {
            Name = "LockUserAsync", Path = LockUserWithEndDatePath,
            Permissions = new List<string> { ApiPermissionConstants.AdminWrite, ApiPermissionConstants.AdminManage }
        },
        new ApiRouteModel
        {
            Name = "AddAdminClaimAsync", Path = AddAdminClaim,
            Permissions = new List<string> { ApiPermissionConstants.AdminWrite, ApiPermissionConstants.AdminManage }
        },
        new ApiRouteModel
        {
            Name = "AddAdminClaimAsync", Path = AddAdminClaimList,
            Permissions = new List<string> { ApiPermissionConstants.AdminWrite, ApiPermissionConstants.AdminManage }
        },
        new ApiRouteModel
        {
            Name = "AddClaimAsync", Path = AddClaim,
            Permissions = new List<string> { ApiPermissionConstants.UserWrite, ApiPermissionConstants.AdminWrite }
        },
        new ApiRouteModel
        {
            Name = "AddClaimAsync", Path = AddClaimList,
            Permissions = new List<string> { ApiPermissionConstants.UserWrite, ApiPermissionConstants.AdminWrite }
        },
        new ApiRouteModel
        {
            Name = "AddSecurityQuestionAsync", Path = AddSecurityQuestion,
            Permissions = new List<string> { ApiPermissionConstants.AdminWrite, ApiPermissionConstants.AdminManage }
        },
        new ApiRouteModel
        {
            Name = "AddUserRoleAsync", Path = AddUserRole,
            Permissions = new List<string> { ApiPermissionConstants.AdminWrite, ApiPermissionConstants.AdminManage }
        },
        new ApiRouteModel
        {
            Name = "AddUserRolesAsync", Path = AddUserRolesList,
            Permissions = new List<string> { ApiPermissionConstants.AdminWrite, ApiPermissionConstants.AdminManage }
        },
        new ApiRouteModel
        {
            Name = "AddUserSecurityQuestionAsync", Path = AddUserSecurityQuestion,
            Permissions = new List<string> { ApiPermissionConstants.UserWrite, ApiPermissionConstants.AdminWrite }
        },
        new ApiRouteModel
        {
            Name = "AddUserSecurityQuestionAsync", Path = AddUserSecurityQuestionList,
            Permissions = new List<string> { ApiPermissionConstants.UserWrite, ApiPermissionConstants.AdminWrite }
        },
        new ApiRouteModel
        {
            Name = "ChangePasswordAsync", Path = ChangePassword,
            Permissions = new List<string> { ApiPermissionConstants.UserWrite, ApiPermissionConstants.AdminWrite }
        },
        new ApiRouteModel
        {
            Name = "DeleteSecurityQuestionAsync", Path = DeleteSecurityQuestion,
            Permissions = new List<string> { ApiPermissionConstants.AdminDelete, ApiPermissionConstants.AdminManage }
        },
        new ApiRouteModel
        {
            Name = "DeleteUserAsync", Path = DeleteUserById,
            Permissions = new List<string> { ApiPermissionConstants.AdminDelete, ApiPermissionConstants.AdminManage }
        },
        new ApiRouteModel
        {
            Name = "DeleteUserAsync", Path = DeleteUserByName,
            Permissions = new List<string> { ApiPermissionConstants.AdminDelete, ApiPermissionConstants.AdminManage }
        },
        new ApiRouteModel
        {
            Name = "DeleteUserSecurityQuestionAsync", Path = DeleteUserSecurityQuestion,
            Permissions = new List<string> { ApiPermissionConstants.UserDelete, ApiPermissionConstants.AdminDelete }
        },
        new ApiRouteModel
        {
            Name = "DeleteUserSecurityQuestionAsync", Path = DeleteUserSecurityQuestionList,
            Permissions = new List<string> { ApiPermissionConstants.UserDelete, ApiPermissionConstants.AdminDelete }
        },
        new ApiRouteModel
        {
            Name = "GenerateEmailConfirmationTokenAsync", Path = GenerateEmailConfirmationToken,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "GeneratePhoneNumberConfirmationTokenAsync", Path = GeneratePhoneNumberConfirmationToken,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "GenerateEmailTwoFactorTokenAsync", Path = GenerateEmailTwoFactorToken,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "GeneratePasswordResetTokenAsync", Path = GeneratePasswordResetToken,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "GenerateSmsTwoFactorTokenAsync", Path = GenerateSmsTwoFactorToken,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "GenerateUserTokenAsync", Path = GenerateUserTokenAsync,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "GetAllSecurityQuestionsAsync", Path = GetAllSecurityQuestions,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "GetAllTwoFactorTypeAsync", Path = GetAllTwoFactorType,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "GetClaimsAsync", Path = GetClaims,
            Permissions = new List<string> { ApiPermissionConstants.UserRead, ApiPermissionConstants.AdminRead }
        },
        new ApiRouteModel
        {
            Name = "GetUserByEmailAsync", Path = GetUserByEmail,
            Permissions = new List<string> { ApiPermissionConstants.UserRead, ApiPermissionConstants.AdminRead }
        },
        new ApiRouteModel
        {
            Name = "GetUserByIdAsync", Path = GetUserById,
            Permissions = new List<string> { ApiPermissionConstants.UserRead, ApiPermissionConstants.AdminRead }
        },
        new ApiRouteModel
        {
            Name = "GetUserByNameAsync", Path = GetUserByName,
            Permissions = new List<string> { ApiPermissionConstants.UserRead, ApiPermissionConstants.AdminRead }
        },
        new ApiRouteModel
        {
            Name = "GetAllUsersAsync", Path = GetAllUsers,
            Permissions = new List<string> { ApiPermissionConstants.AdminRead, ApiPermissionConstants.AdminManage }
        },
        new ApiRouteModel
        {
            Name = "GetUserClaimsAsync", Path = GetUserClaims,
            Permissions = new List<string> { ApiPermissionConstants.UserRead, ApiPermissionConstants.AdminRead }
        },
        new ApiRouteModel
        {
            Name = "GetAdminUserClaimsAsync", Path = GetAdminUserClaims,
            Permissions = new List<string> { ApiPermissionConstants.AdminRead, ApiPermissionConstants.AdminManage }
        },
        new ApiRouteModel
        {
            Name = "GetUserRoleClaimsByIdAsync", Path = GetUserRoleClaimsById,
            Permissions = new List<string> { ApiPermissionConstants.UserRead, ApiPermissionConstants.AdminRead }
        },
        new ApiRouteModel
        {
            Name = "GetUserRoleClaimsByNameAsync", Path = GetUserRoleClaimsByName,
            Permissions = new List<string> { ApiPermissionConstants.UserRead, ApiPermissionConstants.AdminRead }
        },
        new ApiRouteModel
        {
            Name = "GetUserRolesAsync", Path = GetUserRoles,
            Permissions = new List<string> { ApiPermissionConstants.UserRead, ApiPermissionConstants.AdminRead }
        },
        new ApiRouteModel
        {
            Name = "GetUserSecurityQuestionsAsync", Path = GetUserSecurityQuestions,
            Permissions = new List<string> { ApiPermissionConstants.UserRead, ApiPermissionConstants.AdminRead }
        },
        new ApiRouteModel
        {
            Name = "GetUsersForClaimAsync", Path = GetUsersForClaim,
            Permissions = new List<string> { ApiPermissionConstants.UserRead, ApiPermissionConstants.AdminRead }
        },
        new ApiRouteModel
        {
            Name = "GetUsersInRoleAsync", Path = GetUsersInRole,
            Permissions = new List<string> { ApiPermissionConstants.UserRead, ApiPermissionConstants.AdminRead }
        },
        new ApiRouteModel
        {
            Name = "IsUserExistsAsync", Path = IsUserExistsByClaimPrincipal,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "IsUserExistsAsync", Path = IsUserExistsById,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "IsUserExistsAsync", Path = IsUserExistsByName,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "RegisterUserAsync", Path = RegisterUser,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "RemoveAdminClaimAsync", Path = RemoveAdminClaim,
            Permissions = new List<string> { ApiPermissionConstants.AdminDelete, ApiPermissionConstants.AdminManage }
        },
        new ApiRouteModel
        {
            Name = "RemoveAdminClaimAsync", Path = RemoveAdminClaimList,
            Permissions = new List<string> { ApiPermissionConstants.AdminDelete, ApiPermissionConstants.AdminManage }
        },
        new ApiRouteModel
        {
            Name = "RemoveClaimAsync", Path = RemoveClaim,
            Permissions = new List<string> { ApiPermissionConstants.UserDelete, ApiPermissionConstants.AdminDelete }
        },
        new ApiRouteModel
        {
            Name = "RemoveClaimAsync", Path = RemoveClaimList,
            Permissions = new List<string> { ApiPermissionConstants.UserDelete, ApiPermissionConstants.AdminDelete }
        },
        new ApiRouteModel
        {
            Name = "RemoveUserRoleAsync", Path = RemoveUserRole,
            Permissions = new List<string> { ApiPermissionConstants.AdminDelete, ApiPermissionConstants.AdminManage }
        },
        new ApiRouteModel
        {
            Name = "RemoveUserRolesAsync", Path = RemoveUserRoleList,
            Permissions = new List<string> { ApiPermissionConstants.AdminDelete, ApiPermissionConstants.AdminManage }
        },
        new ApiRouteModel
        {
            Name = "ReplaceClaimAsync", Path = ReplaceClaim,
            Permissions = new List<string> { ApiPermissionConstants.UserWrite, ApiPermissionConstants.AdminWrite }
        },
        new ApiRouteModel
        {
            Name = "ResetPasswordAsync", Path = ResetPassword,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "SetTwoFactorEnabledAsync", Path = SetTwoFactorEnabled,
            Permissions = new List<string> { ApiPermissionConstants.UserWrite, ApiPermissionConstants.AdminWrite }
        },
        new ApiRouteModel
        {
            Name = "UnLockUserAsync", Path = UnLockUser,
            Permissions = new List<string> { ApiPermissionConstants.AdminWrite, ApiPermissionConstants.AdminManage }
        },
        new ApiRouteModel
        {
            Name = "UnLockUserAsync", Path = UnLockUserByToken,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "UnlockUserAsync", Path = UnLockUserByuserSecurityQuestions,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "UpdateSecurityQuestionAsync", Path = UpdateSecurityQuestion,
            Permissions = new List<string> { ApiPermissionConstants.AdminWrite, ApiPermissionConstants.AdminManage }
        },
        new ApiRouteModel
        {
            Name = "UpdateUserAsync", Path = UpdateUser,
            Permissions = new List<string> { ApiPermissionConstants.UserWrite, ApiPermissionConstants.AdminWrite }
        },
        new ApiRouteModel
        {
            Name = "UpdateUserSecurityQuestionAsync", Path = UpdateUserSecurityQuestion,
            Permissions = new List<string> { ApiPermissionConstants.UserWrite, ApiPermissionConstants.AdminWrite }
        },
        new ApiRouteModel
        {
            Name = "UpdateUserTwoFactorTypeAsync", Path = UpdateUserTwoFactorType,
            Permissions = new List<string> { ApiPermissionConstants.UserWrite, ApiPermissionConstants.AdminWrite }
        },
        new ApiRouteModel
        {
            Name = "VerifyEmailConfirmationTokenAsync", Path = VerifyEmailConfirmationToken,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "VerifyEmailTwoFactorTokenAsync", Path = VerifyEmailTwoFactorToken,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "VerifyPhoneNumberConfirmationTokenAsync", Path = VerifyPhoneNumberConfirmationToken,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "VerifySmsTwoFactorTokenAsync", Path = VerifySmsTwoFactorToken,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "VerifyUserTokenAsync", Path = VerifyUserToken,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },

        // Identity Resources
        new ApiRouteModel
        {
            Name = "AddIdentityResourceAsync", Path = AddIdentityResource,
            Permissions = new List<string>
                { ApiPermissionConstants.IdentityResourceWrite, ApiPermissionConstants.IdentityResourceManage }
        },
        new ApiRouteModel
        {
            Name = "UpdateIdentityResourceAsync", Path = UpdateIdentityResource,
            Permissions = new List<string>
                { ApiPermissionConstants.IdentityResourceWrite, ApiPermissionConstants.IdentityResourceManage }
        },
        new ApiRouteModel
        {
            Name = "DeleteIdentityResourceAsync", Path = DeleteIdentityResourceById,
            Permissions = new List<string>
                { ApiPermissionConstants.IdentityResourceDelete, ApiPermissionConstants.IdentityResourceManage }
        },
        new ApiRouteModel
        {
            Name = "DeleteIdentityResourceAsync", Path = DeleteIdentityResourceByName,
            Permissions = new List<string>
                { ApiPermissionConstants.IdentityResourceDelete, ApiPermissionConstants.IdentityResourceManage }
        },
        new ApiRouteModel
        {
            Name = "GetIdentityResourceAsync", Path = GetIdentityResourceById,
            Permissions = new List<string>
                { ApiPermissionConstants.IdentityResourceRead, ApiPermissionConstants.IdentityResourceManage }
        },
        new ApiRouteModel
        {
            Name = "GetIdentityResourceAsync", Path = GetIdentityResourceByName,
            Permissions = new List<string>
                { ApiPermissionConstants.IdentityResourceRead, ApiPermissionConstants.IdentityResourceManage }
        },
        new ApiRouteModel
        {
            Name = "GetAllIdentityResourcesAsync", Path = GetAllIdentityResources,
            Permissions = new List<string>
                { ApiPermissionConstants.IdentityResourceRead, ApiPermissionConstants.IdentityResourceManage }
        },
        new ApiRouteModel
        {
            Name = "AddIdentityResourceClaimAsync", Path = AddIdentityResourceClaim,
            Permissions = new List<string>
                { ApiPermissionConstants.IdentityResourceWrite, ApiPermissionConstants.IdentityResourceManage }
        },
        new ApiRouteModel
        {
            Name = "DeleteIdentityResourceClaimByIdAsync", Path = DeleteIdentityResourceClaimById,
            Permissions = new List<string>
                { ApiPermissionConstants.IdentityResourceDelete, ApiPermissionConstants.IdentityResourceManage }
        },
        new ApiRouteModel
        {
            Name = "DeleteIdentityResourceClaimAsync", Path = DeleteIdentityResourceClaimModel,
            Permissions = new List<string>
                { ApiPermissionConstants.IdentityResourceDelete, ApiPermissionConstants.IdentityResourceManage }
        },
        new ApiRouteModel
        {
            Name = "GetIdentityResourceClaimsAsync", Path = GetIdentityResourceClaims,
            Permissions = new List<string>
                { ApiPermissionConstants.IdentityResourceRead, ApiPermissionConstants.IdentityResourceManage }
        },

        // RoleService
        new ApiRouteModel
        {
            Name = "CreateRoleAsync", Path = CreateRole,
            Permissions = new List<string> { ApiPermissionConstants.RoleWrite, ApiPermissionConstants.RoleManage }
        },
        new ApiRouteModel
        {
            Name = "UpdateRoleAsync", Path = UpdateRole,
            Permissions = new List<string> { ApiPermissionConstants.RoleWrite, ApiPermissionConstants.RoleManage }
        },
        new ApiRouteModel
        {
            Name = "DeleteRoleAsync", Path = DeleteRoleById,
            Permissions = new List<string> { ApiPermissionConstants.RoleDelete, ApiPermissionConstants.RoleManage }
        },
        new ApiRouteModel
        {
            Name = "DeleteRoleAsync", Path = DeleteRoleByName,
            Permissions = new List<string> { ApiPermissionConstants.RoleDelete, ApiPermissionConstants.RoleManage }
        },
        new ApiRouteModel
        {
            Name = "GetRoleAsync", Path = GetRoleById,
            Permissions = new List<string> { ApiPermissionConstants.RoleRead, ApiPermissionConstants.RoleManage }
        },
        new ApiRouteModel
        {
            Name = "GetRoleAsync", Path = GetRoleByName,
            Permissions = new List<string> { ApiPermissionConstants.RoleRead, ApiPermissionConstants.RoleManage }
        },
        new ApiRouteModel
        {
            Name = "GetAllRolesAsync", Path = GetAllRoles,
            Permissions = new List<string> { ApiPermissionConstants.RoleRead, ApiPermissionConstants.RoleManage }
        },
        new ApiRouteModel
        {
            Name = "AddRoleClaimAsync", Path = AddRoleClaim,
            Permissions = new List<string> { ApiPermissionConstants.RoleWrite, ApiPermissionConstants.RoleManage }
        },
        new ApiRouteModel
        {
            Name = "AddRoleClaimsAsync", Path = AddRoleClaimList,
            Permissions = new List<string> { ApiPermissionConstants.RoleWrite, ApiPermissionConstants.RoleManage }
        },
        new ApiRouteModel
        {
            Name = "RemoveRoleClaimAsync", Path = RemoveRoleClaim,
            Permissions = new List<string> { ApiPermissionConstants.RoleDelete, ApiPermissionConstants.RoleManage }
        },
        new ApiRouteModel
        {
            Name = "RemoveRoleClaimsAsync", Path = RemoveRoleClaimsList,
            Permissions = new List<string> { ApiPermissionConstants.RoleDelete, ApiPermissionConstants.RoleManage }
        },
        new ApiRouteModel
        {
            Name = "RemoveRoleClaimsAsync", Path = RemoveRoleClaimsById,
            Permissions = new List<string> { ApiPermissionConstants.RoleDelete, ApiPermissionConstants.RoleManage }
        },
        new ApiRouteModel
        {
            Name = "GetRoleClaimAsync", Path = GetRoleClaim,
            Permissions = new List<string> { ApiPermissionConstants.RoleRead, ApiPermissionConstants.RoleManage }
        },

        // Authentication Service
        new ApiRouteModel
        {
            Name = "GenerateRecoveryCodesAsync", Path = GenerateRecoveryCodes,
            Permissions = new List<string>
            {
                ApiPermissionConstants.UserWrite, ApiPermissionConstants.AdminWrite, ApiPermissionConstants.AdminManage
            }
        },
        new ApiRouteModel
        {
            Name = "IsUserSignedInAsync", Path = IsUserSignedIn,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "PasswordSignInAsync", Path = PasswordSignIn,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "PasswordSignInAsync", Path = PasswordSignInByTwoFactorAuthenticatorToken,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "ResetAuthenticatorAppAsync", Path = ResetAuthenticatorApp,
            Permissions = new List<string>
            {
                ApiPermissionConstants.UserWrite, ApiPermissionConstants.AdminWrite, ApiPermissionConstants.AdminManage
            }
        },
        new ApiRouteModel
        {
            Name = "RopValidateCredentialsAsync", Path = RopValidateCredentials,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "SetupAuthenticatorAppAsync", Path = SetupAuthenticatorApp,
            Permissions = new List<string> { ApiPermissionConstants.UserWrite }
        },
        new ApiRouteModel
        {
            Name = "SignOutAsync", Path = SignOut, Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "TwoFactorAuthenticatorAppSignInAsync", Path = TwoFactorAuthenticatorAppSignIn,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "TwoFactorEmailSignInAsync", Path = TwoFactorEmailSignIn,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "TwoFactorRecoveryCodeSignInAsync", Path = TwoFactorRecoveryCodeSignIn,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "TwoFactorSmsSignInAsync", Path = TwoFactorSmsSignInAsync,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "VerifyAuthenticatorAppSetupAsync", Path = VerifyAuthenticatorAppSetup,
            Permissions = new List<string> { ApiPermissionConstants.UserWrite }
        },
        new ApiRouteModel
        {
            Name = "CountRecoveryCodesAsync", Path = CountRecoveryCodesAsync,
            Permissions = new List<string> { ApiPermissionConstants.UserWrite, ApiPermissionConstants.UserManage }
        },

        // Audit Trail
        new ApiRouteModel
        {
            Name = "AddAuditTrailAsync", Path = AddAuditTrail,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "AddAuditTrailAsync", Path = AddAuditTrailModel,
            Permissions = new List<string> { ApiPermissionConstants.Anonymous }
        },
        new ApiRouteModel
        {
            Name = "GetAuditDetailsAsync", Path = GetAuditDetails,
            Permissions = new List<string> { ApiPermissionConstants.AuditRead, ApiPermissionConstants.AuditManage }
        },
        //new ApiRouteModel { Name = "GetAuditDetailsAsync", Path = GetAuditDetailsByCreatedOn, Permissions = new List<string> { ApiPermissionConstants.AuditRead, ApiPermissionConstants.AuditManage } },
        //new ApiRouteModel { Name = "GetAuditDetailsAsync", Path = GetAuditDetailsByFromDate, Permissions = new List<string> { ApiPermissionConstants.AuditRead, ApiPermissionConstants.AuditManage } },
        //new ApiRouteModel { Name = "GetAuditDetailsAsync", Path = GetAuditDetailsByActionType, Permissions = new List<string> { ApiPermissionConstants.AuditRead, ApiPermissionConstants.AuditManage } },

        // Client
        new ApiRouteModel
        {
            Name = "DeleteClientAsync", Path = DeleteClient,
            Permissions = new List<string> { ApiPermissionConstants.ClientDelete, ApiPermissionConstants.ClientManage }
        },
        new ApiRouteModel
        {
            Name = "GenerateClientSecret", Path = GenerateClientSecret,
            Permissions = new List<string> { ApiPermissionConstants.ClientWrite, ApiPermissionConstants.ClientManage }
        },
        new ApiRouteModel
        {
            Name = "GetAllClientAsync", Path = GetAllClient,
            Permissions = new List<string> { ApiPermissionConstants.ClientRead, ApiPermissionConstants.ClientManage }
        },
        new ApiRouteModel
        {
            Name = "GetClientAsync", Path = GetClient,
            Permissions = new List<string> { ApiPermissionConstants.ClientRead, ApiPermissionConstants.ClientManage }
        },
        new ApiRouteModel
        {
            Name = "RegisterClientAsync", Path = RegisterClient,
            Permissions = new List<string> { ApiPermissionConstants.ClientWrite, ApiPermissionConstants.ClientManage }
        },
        new ApiRouteModel
        {
            Name = "UpdateClientAsync", Path = UpdateClient,
            Permissions = new List<string> { ApiPermissionConstants.ClientWrite, ApiPermissionConstants.ClientManage }
        },

        // Security Token
        new ApiRouteModel
        {
            Name = "GetActiveSecurityTokensAsync", Path = GetActiveSecurityTokensByClientIds,
            Permissions = new List<string>
                { ApiPermissionConstants.SecurityTokenManage, ApiPermissionConstants.SecurityTokenRead }
        },
        new ApiRouteModel
        {
            Name = "GetActiveSecurityTokensAsync", Path = GetActiveSecurityTokensByUserIds,
            Permissions = new List<string>
                { ApiPermissionConstants.SecurityTokenManage, ApiPermissionConstants.SecurityTokenRead }
        },
        new ApiRouteModel
        {
            Name = "GetActiveSecurityTokensAsync", Path = GetActiveSecurityTokensBetweenDates,
            Permissions = new List<string>
                { ApiPermissionConstants.SecurityTokenManage, ApiPermissionConstants.SecurityTokenRead }
        },
        new ApiRouteModel
        {
            Name = "GetActiveSecurityTokensAsync", Path = GetAllSecurityTokensBetweenDates,
            Permissions = new List<string>
                { ApiPermissionConstants.SecurityTokenManage, ApiPermissionConstants.SecurityTokenRead }
        },

        // Notification Management
        new ApiRouteModel
        {
            Name = "GetNotificationLogsAsync", Path = GetNotificationLogs,
            Permissions = new List<string>
                { ApiPermissionConstants.NotificationRead, ApiPermissionConstants.NotificationManage }
        },
        new ApiRouteModel
        {
            Name = "GetNotificationTemplatesAsync", Path = GetNotificationTemplates,
            Permissions = new List<string>
                { ApiPermissionConstants.NotificationRead, ApiPermissionConstants.NotificationManage }
        },
        new ApiRouteModel
        {
            Name = "GetProviderConfigAsync", Path = GetProviderConfig,
            Permissions = new List<string>
                { ApiPermissionConstants.NotificationRead, ApiPermissionConstants.NotificationManage }
        },
        new ApiRouteModel
        {
            Name = "GetAllProviderConfigsAsync", Path = GetAllProviderConfigs,
            Permissions = new List<string>
                { ApiPermissionConstants.NotificationRead, ApiPermissionConstants.NotificationManage }
        },
        new ApiRouteModel
        {
            Name = "SaveProviderConfigAsync", Path = SaveProviderConfig,
            Permissions = new List<string> { ApiPermissionConstants.NotificationManage }
        },
        new ApiRouteModel
        {
            Name = "SetActiveProviderAsync", Path = SetActiveProvider,
            Permissions = new List<string> { ApiPermissionConstants.NotificationManage }
        },
        new ApiRouteModel
        {
            Name = "DeleteProviderConfigAsync", Path = DeleteProviderConfig,
            Permissions = new List<string> { ApiPermissionConstants.NotificationManage }
        },
        new ApiRouteModel
        {
            Name = "GetProviderFieldDefinitionsAsync", Path = GetProviderFieldDefinitions,
            Permissions = new List<string>
                { ApiPermissionConstants.NotificationRead, ApiPermissionConstants.NotificationManage }
        },
        new ApiRouteModel
        {
            Name = "SendTestNotificationAsync", Path = SendTestNotification,
            Permissions = new List<string> { ApiPermissionConstants.NotificationManage }
        },

        // External Auth Management
        new ApiRouteModel
        {
            Name = "GetAllExternalAuthProvidersAsync", Path = GetAllExternalAuthProviders,
            Permissions = new List<string>
                { ApiPermissionConstants.ExternalAuthRead, ApiPermissionConstants.ExternalAuthManage }
        },
        new ApiRouteModel
        {
            Name = "GetExternalAuthProviderAsync", Path = GetExternalAuthProvider,
            Permissions = new List<string>
                { ApiPermissionConstants.ExternalAuthRead, ApiPermissionConstants.ExternalAuthManage }
        },
        new ApiRouteModel
        {
            Name = "SaveExternalAuthProviderAsync", Path = SaveExternalAuthProvider,
            Permissions = new List<string> { ApiPermissionConstants.ExternalAuthManage }
        },
        new ApiRouteModel
        {
            Name = "DeleteExternalAuthProviderAsync", Path = DeleteExternalAuthProvider,
            Permissions = new List<string> { ApiPermissionConstants.ExternalAuthManage }
        },
        new ApiRouteModel
        {
            Name = "TestExternalAuthProviderAsync", Path = TestExternalAuthProvider,
            Permissions = new List<string> { ApiPermissionConstants.ExternalAuthManage }
        },
        new ApiRouteModel
        {
            Name = "GetExternalAuthFieldDefinitionsAsync", Path = GetExternalAuthFieldDefinitions,
            Permissions = new List<string>
                { ApiPermissionConstants.ExternalAuthRead, ApiPermissionConstants.ExternalAuthManage }
        }
    };
}
