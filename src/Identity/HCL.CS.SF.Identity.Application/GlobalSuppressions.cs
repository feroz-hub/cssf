/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly:
    SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private",
        Justification = "<Pending>", Scope = "member",
        Target = "~F:HCL.CS.SF.Service.Implementation.AuthorizationCodeBase.LoggerService")]
[assembly:
    SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private",
        Justification = "<Pending>", Scope = "member",
        Target = "~F:HCL.CS.SF.Service.Implementation.AuthorizationCodeBase.AuthorizationService")]
[assembly:
    SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private",
        Justification = "<Pending>", Scope = "member",
        Target = "~F:HCL.CS.SF.Service.Implementation.AuthorizationCodeBase.CsSignInManager")]
[assembly:
    SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced", Justification = "<Pending>",
        Scope = "member",
        Target =
            "~M:HCL.CS.SF.Service.Implementation.UserAccountService.RegisterUserAsync(HCL.CS.SF.Domain.Models.UserModel)~System.Threading.Tasks.Task{HCL.CS.SF.Domain.FrameworkResult}")]
[assembly:
    SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced", Justification = "<Pending>",
        Scope = "member",
        Target =
            "~M:HCL.CS.SF.Service.Implementation.UserAccountService.DeleteAccountAsync(HCL.CS.SF.Domain.Models.UserModel)~System.Threading.Tasks.Task{HCL.CS.SF.Domain.FrameworkResult}")]
[assembly:
    SuppressMessage("Minor Code Smell", "S1075:URIs should not be hardcoded", Justification = "<Pending>",
        Scope = "member",
        Target =
            "~M:HCL.CS.SF.Service.Implementation.AuthenticationService.GenerateQrCodeUri(System.String,System.String,System.String,System.Text.Encodings.Web.UrlEncoder)~System.String")]
[assembly:
    SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced", Justification = "<Pending>",
        Scope = "member",
        Target =
            "~M:HCL.CS.SF.Service.Implementation.AuthenticationService.TwoFactorRecoveryCodeSignInAsync(System.String,System.String)~System.Threading.Tasks.Task{HCL.CS.SF.Domain.Models.ConstructSignInResponseModel}")]
[assembly:
    SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced", Justification = "<Pending>",
        Scope = "member",
        Target =
            "~M:HCL.CS.SF.Service.Implementation.AuthenticationService.TwoFactorAuthenticatorSignInAsync(System.String,System.String)~System.Threading.Tasks.Task{HCL.CS.SF.Domain.Models.ConstructSignInResponseModel}")]
[assembly:
    SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced", Justification = "<Pending>",
        Scope = "member",
        Target =
            "~M:HCL.CS.SF.Service.Implementation.UserManagementValidator.ValidateAsync(System.String,System.String,System.String,System.Boolean,HCL.CS.SF.Domain.SystemSettings,HCL.CS.SF.Service.Implementation.UserManagerWrapper{HCL.CS.SF.Domain.Entities.Users},HCL.CS.SF.DomainServices.Infra.IFrameworkResultService,HCL.CS.SF.DomainServices.Infra.IResourceStringHandler,HCL.CS.SF.DomainServices.Infra.ILoggerService)~System.Threading.Tasks.Task{HCL.CS.SF.Domain.Models.ConstructSignInResponseModel}")]
[assembly:
    SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced", Justification = "<Pending>",
        Scope = "member",
        Target =
            "~M:HCL.CS.SF.Service.Implementation.AuthenticationService.TwoFactorSignInAsync(System.String,System.String,HCL.CS.SF.Domain.Enums.NotificationTypes)~System.Threading.Tasks.Task{HCL.CS.SF.Domain.Models.ConstructSignInResponseModel}")]
[assembly:
    SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced", Justification = "<Pending>",
        Scope = "member",
        Target =
            "~M:HCL.CS.SF.Service.Implementation.WinFormsAuthenticationService.TwoFactorAuthenticatorSignInAsync(System.String,System.String)~System.Threading.Tasks.Task{HCL.CS.SF.Domain.Models.ConstructSignInResponseModel}")]
[assembly:
    SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced", Justification = "<Pending>",
        Scope = "member",
        Target =
            "~M:HCL.CS.SF.Service.Implementation.WinFormsAuthenticationService.TwoFactorRecoveryCodeSignInAsync(System.String,System.String)~System.Threading.Tasks.Task{HCL.CS.SF.Domain.Models.ConstructSignInResponseModel}")]
[assembly:
    SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced", Justification = "<Pending>",
        Scope = "member",
        Target =
            "~M:HCL.CS.SF.Service.Implementation.WinFormsAuthenticationService.TwoFactorSignInAsync(System.String,System.String,HCL.CS.SF.Domain.Enums.NotificationTypes)~System.Threading.Tasks.Task{HCL.CS.SF.Domain.Models.ConstructSignInResponseModel}")]
[assembly:
    SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced", Justification = "<Pending>",
        Scope = "member",
        Target =
            "~M:HCL.CS.SF.Service.Implementation.AuthenticationService.VerifyAuthenticatorSetupAsync(System.Guid,System.String)~System.Threading.Tasks.Task{HCL.CS.SF.Domain.Models.AuthenticatorAppResponseModel}")]
[assembly:
    SuppressMessage("Major Code Smell", "S4457:Parameter validation in \"async\"/\"await\" methods should be wrapped",
        Justification = "<Pending>", Scope = "member",
        Target =
            "~M:HCL.CS.SF.Service.Implementation.UserManagerWrapper`1.VerifyTwoFactorTokenAsync(System.Guid,System.String)~System.Threading.Tasks.Task{System.Boolean}")]
[assembly:
    SuppressMessage("Major Code Smell", "S4457:Parameter validation in \"async\"/\"await\" methods should be wrapped",
        Justification = "<Pending>", Scope = "member",
        Target =
            "~M:HCL.CS.SF.Service.Implementation.WinFormsSignInWrapper`1.PasswordSignInAsync(`0,System.String,System.Boolean)~System.Threading.Tasks.Task{Microsoft.AspNetCore.Identity.SignInResult}")]
[assembly:
    SuppressMessage("Major Code Smell", "S4457:Parameter validation in \"async\"/\"await\" methods should be wrapped",
        Justification = "<Pending>", Scope = "member",
        Target =
            "~M:HCL.CS.SF.Service.Implementation.WinFormsSignInWrapper`1.CheckPasswordSignInAsync(`0,System.String,System.Boolean)~System.Threading.Tasks.Task{Microsoft.AspNetCore.Identity.SignInResult}")]
[assembly:
    SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced", Justification = "<Pending>",
        Scope = "member",
        Target =
            "~M:HCL.CS.SF.Service.Implementation.UserInfoServices.ProcessUserInfoAsync(HCL.CS.SF.Domain.Models.ValidatedUserInfoRequestModel)~System.Threading.Tasks.Task{System.Collections.Generic.Dictionary{System.String,System.Object}}")]
[assembly:
    SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced", Justification = "<Pending>",
        Scope = "member",
        Target =
            "~M:HCL.CS.SF.Service.Implementation.IdentityResourceService.AddIdentityResourceAsync(HCL.CS.SF.Domain.Models.IdentityResourcesModel)~System.Threading.Tasks.Task{HCL.CS.SF.Domain.FrameworkResult}")]
[assembly:
    SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced", Justification = "<Pending>",
        Scope = "member",
        Target =
            "~M:HCL.CS.SF.Service.Implementation.RoleService.CreateRoleAsync(HCL.CS.SF.Domain.Models.RoleModel)~System.Threading.Tasks.Task{HCL.CS.SF.Domain.FrameworkResult}")]
[assembly:
    SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced", Justification = "<Pending>",
        Scope = "member",
        Target =
            "~M:HCL.CS.SF.Service.Implementation.RoleService.UpdateRoleAsync(HCL.CS.SF.Domain.Models.RoleModel)~System.Threading.Tasks.Task{HCL.CS.SF.Domain.FrameworkResult}")]
[assembly:
    SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced", Justification = "<Pending>",
        Scope = "member",
        Target =
            "~M:HCL.CS.SF.Service.Implementation.RoleService.RemoveRoleAsync(HCL.CS.SF.Domain.Models.RoleModel)~System.Threading.Tasks.Task{HCL.CS.SF.Domain.FrameworkResult}")]
[assembly:
    SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced", Justification = "<Pending>",
        Scope = "member",
        Target =
            "~M:HCL.CS.SF.Service.Implementation.RoleService.UpdateUserRoleAsync(HCL.CS.SF.Domain.Models.UserRoleModel,System.Guid)~System.Threading.Tasks.Task{HCL.CS.SF.Domain.FrameworkResult}")]
[assembly:
    SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced", Justification = "<Pending>",
        Scope = "member",
        Target =
            "~M:HCL.CS.SF.Service.Implementation.RoleService.AddRoleClaimAsync(HCL.CS.SF.Domain.Models.RoleClaimModel)~System.Threading.Tasks.Task{HCL.CS.SF.Domain.FrameworkResult}")]
[assembly:
    SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced", Justification = "<Pending>",
        Scope = "member",
        Target =
            "~M:HCL.CS.SF.Service.Implementation.RoleService.RemoveRoleClaimAsync(HCL.CS.SF.Domain.Models.RoleClaimModel)~System.Threading.Tasks.Task{HCL.CS.SF.Domain.FrameworkResult}")]
[assembly:
    SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced", Justification = "<Pending>",
        Scope = "member",
        Target =
            "~M:HCL.CS.SF.Service.Implementation.RoleService.DeleteUserRole(HCL.CS.SF.Domain.Models.UserRoleModel,System.Boolean)~System.Threading.Tasks.Task{HCL.CS.SF.Domain.FrameworkResult}")]
[assembly:
    SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced", Justification = "<Pending>",
        Scope = "member",
        Target =
            "~M:HCL.CS.SF.Service.Implementation.SignInManagerWrapper`1.GetAuthenticationAsync~System.Threading.Tasks.Task{HCL.CS.SF.Domain.Models.AuthenticationPropertiesModel}")]
[assembly:
    SuppressMessage("Major Code Smell",
        "S3928:Parameter names used into ArgumentException constructors should match an existing one ",
        Justification = "<Pending>", Scope = "member",
        Target =
            "~M:HCL.CS.SF.Service.Implementation.TokenExtension.GetAsymmetricCertificateHash(System.Collections.Generic.Dictionary{System.String,HCL.CS.SF.Domain.Models.AsymmetricKeyInfoModel},System.String)~System.String")]
[assembly:
    SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced", Justification = "<Pending>",
        Scope = "member",
        Target =
            "~M:HCL.CS.SF.Service.Implementation.AuthorizationService.SaveReturnUrlAsync(HCL.CS.SF.Domain.Models.ValidatedAuthorizeRequestModel)~System.Threading.Tasks.Task{System.Guid}")]
[assembly:
    SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced", Justification = "<Pending>",
        Scope = "member",
        Target =
            "~M:HCL.CS.SF.Service.Implementation.AuthenticationService.RopValidateCredentialsAsync(HCL.CS.SF.Domain.Models.RopValidationModel)~System.Threading.Tasks.Task{HCL.CS.SF.Domain.Models.RopValidationModel}")]
[assembly:
    SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced", Justification = "<Pending>",
        Scope = "member",
        Target =
            "~M:HCL.CS.SF.Service.Implementation.UserAccountService.GetUsersForClaimAsync(System.Security.Claims.Claim)~System.Threading.Tasks.Task{System.Collections.Generic.IList{HCL.CS.SF.Domain.Models.UserModel}}")]
