/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.Domain.Models.Api.Response;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.DomainServices.UnitOfWork.Api;
using HCL.CS.SF.DomainServices.Wrappers;
using HCL.CS.SF.Service.Implementation.Api.Services;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api.Wrapper;

namespace HCL.CS.SF.ProxyService.Proxy;

/// <summary>
/// Gateway proxy for the user account management service. Routes requests to the backend
/// <see cref="UserAccountService"/> after enforcing API-level permission validation.
/// Covers user registration, profile management, claims, roles, security questions,
/// password management, two-factor configuration, and token generation/verification.
/// </summary>
public sealed class UserAccountProxyServices(
    UserManagerWrapper<Users> userManager,
    ILoggerInstance instance,
    IResourceStringHandler resourceStringHandler,
    IUserManagementUnitOfWork userManagementUnitOfWork,
    IMapper mapper,
    HCLCSSFConfig securityConfig,
    IEmailService emailService,
    ISmsService smsService,
    IFrameworkResultService frameworkResultService,
    IRepository<UserSecurityQuestions> userSecurityQuestionsRepository,
    IPasswordHasher<Users> passwordHasher,
    IRoleService roleService,
    IRepository<SecurityTokens> securityTokenRepository,
    IApiValidator apiValidator,
    RoleManagerWrapper<Roles> roleManager)
    : UserAccountService(userManager,
        instance,
        resourceStringHandler,
        userManagementUnitOfWork,
        mapper,
        securityConfig,
        emailService,
        smsService,
        frameworkResultService,
        userSecurityQuestionsRepository,
        passwordHasher,
        roleService,
        securityTokenRepository,
        roleManager), IUserAccountService
{
    /// <summary>
    /// Service used to construct failure responses when validation fails.
    /// </summary>
    private readonly IFrameworkResultService frameworkResult = frameworkResultService;

    /// <summary>
    /// Adds admin-level claims to a user after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> AddAdminClaimAsync(IList<UserClaimModel> userClaimModels)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.AddAdminClaimAsync(userClaimModels);
    }

    /// <summary>
    /// Adds a single admin-level claim to a user after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> AddAdminClaimAsync(UserClaimModel userClaimModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.AddAdminClaimAsync(userClaimModel);
    }

    /// <summary>
    /// Adds a single claim to a user after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> AddClaimAsync(UserClaimModel userClaimModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.AddClaimAsync(userClaimModel);
    }

    /// <summary>
    /// Adds multiple claims to a user after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> AddClaimAsync(IList<UserClaimModel> userClaimModels)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.AddClaimAsync(userClaimModels);
    }

    /// <summary>
    /// Adds a security question to the system after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> AddSecurityQuestionAsync(SecurityQuestionModel securityQuestionModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.AddSecurityQuestionAsync(securityQuestionModel);
    }

    /// <summary>
    /// Assigns a role to a user after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> AddUserRoleAsync(UserRoleModel userRoleModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.AddUserRoleAsync(userRoleModel);
    }

    /// <summary>
    /// Assigns multiple roles to a user after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> AddUserRolesAsync(IList<UserRoleModel> modelList)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.AddUserRolesAsync(modelList);
    }

    /// <summary>
    /// Associates a security question answer with a user after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> AddUserSecurityQuestionAsync(
        UserSecurityQuestionModel userSecurityQuestionModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.AddUserSecurityQuestionAsync(userSecurityQuestionModel);
    }

    /// <summary>
    /// Associates multiple security question answers with a user after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> AddUserSecurityQuestionAsync(
        IList<UserSecurityQuestionModel> userSecurityQuestionModels)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.AddUserSecurityQuestionAsync(userSecurityQuestionModels);
    }

    /// <summary>
    /// Changes the user's password after verifying the current password and permission validation.
    /// </summary>
    public override async Task<FrameworkResult> ChangePasswordAsync(Guid userId, string currentPassword,
        string newPassword)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.ChangePasswordAsync(userId, currentPassword, newPassword);
    }

    /// <summary>
    /// Deletes a system-level security question after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> DeleteSecurityQuestionAsync(Guid securityQuestionId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.DeleteSecurityQuestionAsync(securityQuestionId);
    }

    /// <summary>
    /// Deletes a user account by username after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> DeleteUserAsync(string username)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.DeleteUserAsync(username);
    }

    /// <summary>
    /// Deletes a user account by user ID after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> DeleteUserAsync(Guid userId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.DeleteUserAsync(userId);
    }

    /// <summary>
    /// Removes a single security question answer from a user after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> DeleteUserSecurityQuestionAsync(
        UserSecurityQuestionModel userSecurityQuestionModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.DeleteUserSecurityQuestionAsync(userSecurityQuestionModel);
    }

    /// <summary>
    /// Removes multiple security question answers from a user after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> DeleteUserSecurityQuestionAsync(
        IList<UserSecurityQuestionModel> userSecurityQuestionModels)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.DeleteUserSecurityQuestionAsync(userSecurityQuestionModels);
    }

    /// <summary>
    /// Generates an email confirmation token and sends it to the user after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> GenerateEmailConfirmationTokenAsync(string username)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.GenerateEmailConfirmationTokenAsync(username);
    }

    /// <summary>
    /// Generates a phone number confirmation token and sends it via SMS after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> GeneratePhoneNumberConfirmationTokenAsync(string username)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.GeneratePhoneNumberConfirmationTokenAsync(username);
    }

    /// <summary>
    /// Generates a two-factor email verification token and sends it after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> GenerateEmailTwoFactorTokenAsync(string username)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.GenerateEmailTwoFactorTokenAsync(username);
    }

    /// <summary>
    /// Generates a password reset token and sends it via the specified notification type after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> GeneratePasswordResetTokenAsync(string username,
        NotificationTypes notificationType = NotificationTypes.Email)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.GeneratePasswordResetTokenAsync(username, notificationType);
    }

    /// <summary>
    /// Generates a two-factor SMS verification token and sends it after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> GenerateSmsTwoFactorTokenAsync(string username)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.GenerateSmsTwoFactorTokenAsync(username);
    }

    /// <summary>
    /// Generates a custom user token for the specified purpose and sends a notification after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> GenerateUserTokenAsync(string username, string purpose,
        string templateName, NotificationTypes notificationType = NotificationTypes.Email)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.GenerateUserTokenAsync(username, purpose, templateName, notificationType);
    }

    /// <summary>
    /// Retrieves all system-level security questions after permission validation.
    /// </summary>
    public override async Task<IList<SecurityQuestionModel>> GetAllSecurityQuestionsAsync()
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetAllSecurityQuestionsAsync();
    }

    /// <summary>
    /// Retrieves all available two-factor authentication type names after permission validation.
    /// </summary>
    public override async Task<IList<string>> GetAllTwoFactorTypeAsync()
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetAllTwoFactorTypeAsync();
    }

    /// <summary>
    /// Retrieves the raw identity claims for the specified user after permission validation.
    /// </summary>
    public override async Task<IList<Claim>> GetClaimsAsync(Guid userId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetClaimsAsync(userId);
    }

    /// <summary>
    /// Retrieves a user by email address after permission validation.
    /// </summary>
    public override async Task<UserModel> GetUserByEmailAsync(string email)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetUserByEmailAsync(email);
    }

    /// <summary>
    /// Retrieves a user by unique identifier after permission validation.
    /// </summary>
    public override async Task<UserModel> GetUserByIdAsync(Guid userId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetUserByIdAsync(userId);
    }

    /// <summary>
    /// Retrieves a user by username after permission validation.
    /// </summary>
    public override async Task<UserModel> GetUserByNameAsync(string userName)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetUserByNameAsync(userName);
    }

    /// <summary>
    /// Retrieves custom user claims for the specified user after permission validation.
    /// </summary>
    public override async Task<IList<UserClaimModel>> GetUserClaimsAsync(Guid userId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetUserClaimsAsync(userId);
    }

    /// <summary>
    /// Retrieves admin-level user claims for the specified user after permission validation.
    /// </summary>
    public override async Task<IList<UserClaimModel>> GetAdminUserClaimsAsync(Guid userId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetAdminUserClaimsAsync(userId);
    }

    /// <summary>
    /// Retrieves the combined role and claim permissions for a user by ID after permission validation.
    /// </summary>
    public override async Task<UserPermissionsResponseModel> GetUserRoleClaimsByIdAsync(Guid userId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetUserRoleClaimsByIdAsync(userId);
    }

    /// <summary>
    /// Retrieves the combined role and claim permissions for a user by username after permission validation.
    /// </summary>
    public override async Task<UserPermissionsResponseModel> GetUserRoleClaimsByNameAsync(string userName)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetUserRoleClaimsByNameAsync(userName);
    }

    /// <summary>
    /// Retrieves the role names assigned to the specified user after permission validation.
    /// </summary>
    public override async Task<IList<string>> GetUserRolesAsync(Guid userId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetUserRolesAsync(userId);
    }

    /// <summary>
    /// Retrieves the security question answers for the specified user after permission validation.
    /// </summary>
    public override async Task<IList<UserSecurityQuestionModel>> GetUserSecurityQuestionsAsync(Guid userId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetUserSecurityQuestionsAsync(userId);
    }

    /// <summary>
    /// Retrieves all users that have a specific claim type and value after permission validation.
    /// </summary>
    public override async Task<IList<UserModel>> GetUsersForClaimAsync(string claimType, string claimValue)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetUsersForClaimAsync(claimType, claimValue);
    }

    /// <summary>
    /// Retrieves all users assigned to the specified role after permission validation.
    /// </summary>
    public override async Task<IList<UserModel>> GetUsersInRoleAsync(string roleName)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetUsersInRoleAsync(roleName);
    }

    /// <summary>
    /// Checks whether a user exists based on a claims principal after permission validation.
    /// </summary>
    public override async Task<bool> IsUserExistsAsync(ClaimsPrincipal claimsPrincipal)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.IsUserExistsAsync(claimsPrincipal);
    }

    /// <summary>
    /// Checks whether a user exists by unique identifier after permission validation.
    /// </summary>
    public override async Task<bool> IsUserExistsAsync(Guid userId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.IsUserExistsAsync(userId);
    }

    /// <summary>
    /// Checks whether a user exists by username after permission validation.
    /// </summary>
    public override async Task<bool> IsUserExistsAsync(string userName)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.IsUserExistsAsync(userName);
    }

    /// <summary>
    /// Locks a user account indefinitely after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> LockUserAsync(Guid userId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.LockUserAsync(userId);
    }

    /// <summary>
    /// Locks a user account until the specified date/time after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> LockUserAsync(Guid userId, DateTime? dateTime)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.LockUserAsync(userId, dateTime);
    }

    /// <summary>
    /// Registers a new user account after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> RegisterUserAsync(UserModel user)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.RegisterUserAsync(user);
    }

    /// <summary>
    /// Removes a single admin-level claim from a user after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> RemoveAdminClaimAsync(UserClaimModel userClaimModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.RemoveAdminClaimAsync(userClaimModel);
    }

    /// <summary>
    /// Removes multiple admin-level claims from a user after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> RemoveAdminClaimAsync(IList<UserClaimModel> userClaimModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.RemoveAdminClaimAsync(userClaimModel);
    }

    /// <summary>
    /// Removes a single claim from a user after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> RemoveClaimAsync(UserClaimModel userClaimModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.RemoveClaimAsync(userClaimModel);
    }

    /// <summary>
    /// Removes multiple claims from a user after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> RemoveClaimAsync(IList<UserClaimModel> userClaimModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.RemoveClaimAsync(userClaimModel);
    }

    /// <summary>
    /// Removes a role assignment from a user after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> RemoveUserRoleAsync(UserRoleModel userRoleModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.RemoveUserRoleAsync(userRoleModel);
    }

    /// <summary>
    /// Removes multiple role assignments from a user after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> RemoveUserRolesAsync(IList<UserRoleModel> modelList)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.RemoveUserRolesAsync(modelList);
    }

    /// <summary>
    /// Replaces an existing user claim with a new one after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> ReplaceClaimAsync(UserClaimModel existingUserClaimModel,
        UserClaimModel newUserClaimModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.ReplaceClaimAsync(existingUserClaimModel, newUserClaimModel);
    }

    /// <summary>
    /// Resets a user's password using a password reset token after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> ResetPasswordAsync(string username, string passwordResetToken,
        string newPassword)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.ResetPasswordAsync(username, passwordResetToken, newPassword);
    }

    /// <summary>
    /// Enables or disables two-factor authentication for a user after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> SetTwoFactorEnabledAsync(Guid userId, bool enabled)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.SetTwoFactorEnabledAsync(userId, enabled);
    }

    /// <summary>
    /// Unlocks a user account by username after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> UnLockUserAsync(string username)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.UnLockUserAsync(username);
    }

    /// <summary>
    /// Unlocks a user account by verifying a token for the specified purpose after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> UnLockUserAsync(string username, string token, string purpose)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.UnLockUserAsync(username, token, purpose);
    }

    /// <summary>
    /// Unlocks a user account by verifying security question answers after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> UnlockUserAsync(string username,
        IList<UserSecurityQuestionModel> userSecurityQuestions)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.UnlockUserAsync(username, userSecurityQuestions);
    }

    /// <summary>
    /// Updates a system-level security question after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> UpdateSecurityQuestionAsync(SecurityQuestionModel securityQuestionModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.UpdateSecurityQuestionAsync(securityQuestionModel);
    }

    /// <summary>
    /// Updates a user's profile information after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> UpdateUserAsync(UserModel userModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.UpdateUserAsync(userModel);
    }

    /// <summary>
    /// Retrieves all users as display models after permission validation.
    /// </summary>
    public override async Task<IList<UserDisplayModel>> GetAllUsersAsync()
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetAllUsersAsync();
    }

    /// <summary>
    /// Updates a user's security question answer after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> UpdateUserSecurityQuestionAsync(
        UserSecurityQuestionModel userSecurityQuestionModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.UpdateUserSecurityQuestionAsync(userSecurityQuestionModel);
    }

    /// <summary>
    /// Updates the two-factor authentication type for a user after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> UpdateUserTwoFactorTypeAsync(Guid userId, TwoFactorType twoFactorType)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.UpdateUserTwoFactorTypeAsync(userId, twoFactorType);
    }

    /// <summary>
    /// Verifies an email confirmation token for the specified user after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> VerifyEmailConfirmationTokenAsync(string username, string emailToken)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.VerifyEmailConfirmationTokenAsync(username, emailToken);
    }

    /// <summary>
    /// Verifies a two-factor email token for the specified user after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> VerifyEmailTwoFactorTokenAsync(string username, string emailToken)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.VerifyEmailTwoFactorTokenAsync(username, emailToken);
    }

    /// <summary>
    /// Verifies a phone number confirmation token for the specified user after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> VerifyPhoneNumberConfirmationTokenAsync(string username,
        string smsToken)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.VerifyPhoneNumberConfirmationTokenAsync(username, smsToken);
    }

    /// <summary>
    /// Verifies a two-factor SMS token for the specified user after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> VerifySmsTwoFactorTokenAsync(string username, string smsToken)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.VerifySmsTwoFactorTokenAsync(username, smsToken);
    }

    /// <summary>
    /// Verifies a custom user token for the specified purpose after permission validation.
    /// </summary>
    public override async Task<FrameworkResult> VerifyUserTokenAsync(string username, string purpose, string token)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.VerifyUserTokenAsync(username, purpose, token);
    }
}
