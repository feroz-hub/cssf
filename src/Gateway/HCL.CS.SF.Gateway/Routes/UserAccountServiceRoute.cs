/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using Newtonsoft.Json.Linq;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.ProxyService.Routes.Extension;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api.Wrapper;

namespace HCL.CS.SF.ProxyService.Routes;

/// <summary>
/// Partial class of <see cref="ApiGateway"/> containing route handlers for user account
/// management operations. Covers user registration, profile updates, deletion, locking/unlocking,
/// claims management (add/remove/replace), role assignments, security questions,
/// password management, two-factor configuration, and token generation/verification.
/// </summary>
internal partial class ApiGateway : BaseApiServiceInstance, IApiGateway
{
    /// <summary>
    /// Handles the route for locking a user account indefinitely by user ID.
    /// </summary>
    private async Task<bool> LockUser(string jsonContent)
    {
        var userId = jsonContent.JsonDeserialize<Guid>();
        var frameworkResult = await UserAccountService.LockUserAsync(userId);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for locking a user account until a specified end date.
    /// Parses user ID and end date from the JSON body.
    /// </summary>
    private async Task<bool> LockUserWithEndDate(string jsonContent)
    {
        var jsonObjects = JObject.Parse(jsonContent);
        var userId = jsonObjects[ApiRouteParameterConstants.UserId].ToObject<Guid>();
        var enddate = jsonObjects[ApiRouteParameterConstants.EndDate].ToObject<DateTime>();

        var frameworkResult = await UserAccountService.LockUserAsync(userId, enddate);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for adding a single admin-level claim to a user.
    /// </summary>
    private async Task<bool> AddAdminClaim(string jsonContent)
    {
        var userClaimModel = jsonContent.JsonDeserialize<UserClaimModel>();
        var frameworkResult = await UserAccountService.AddAdminClaimAsync(userClaimModel);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for adding multiple admin-level claims to a user.
    /// </summary>
    private async Task<bool> AddAdminClaimList(string jsonContent)
    {
        var roleModel = jsonContent.JsonDeserialize<IList<UserClaimModel>>();
        var frameworkResult = await UserAccountService.AddAdminClaimAsync(roleModel);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for adding a single claim to a user.
    /// </summary>
    private async Task<bool> AddClaim(string jsonContent)
    {
        var userClaimmodel = jsonContent.JsonDeserialize<UserClaimModel>();
        var frameworkResult = await UserAccountService.AddClaimAsync(userClaimmodel);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for adding multiple claims to a user.
    /// </summary>
    private async Task<bool> AddClaimList(string jsonContent)
    {
        var roleModel = jsonContent.JsonDeserialize<IList<UserClaimModel>>();
        var frameworkResult = await UserAccountService.AddClaimAsync(roleModel);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for adding a security question to the system.
    /// </summary>
    private async Task<bool> AddSecurityQuestion(string jsonContent)
    {
        var securityQuestionModel = jsonContent.JsonDeserialize<SecurityQuestionModel>();
        var frameworkResult = await UserAccountService.AddSecurityQuestionAsync(securityQuestionModel);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for assigning a role to a user.
    /// </summary>
    private async Task<bool> AddUserRole(string jsonContent)
    {
        var userRoleModel = jsonContent.JsonDeserialize<UserRoleModel>();
        var frameworkResult = await UserAccountService.AddUserRoleAsync(userRoleModel);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for assigning multiple roles to a user.
    /// </summary>
    private async Task<bool> AddUserRolesList(string jsonContent)
    {
        var userRoleModel = jsonContent.JsonDeserialize<IList<UserRoleModel>>();
        var frameworkResult = await UserAccountService.AddUserRolesAsync(userRoleModel);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for associating a security question answer with a user.
    /// </summary>
    private async Task<bool> AddUserSecurityQuestion(string jsonContent)
    {
        var userSecurityQuestionModel = jsonContent.JsonDeserialize<UserSecurityQuestionModel>();
        var frameworkResult = await UserAccountService.AddUserSecurityQuestionAsync(userSecurityQuestionModel);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for associating multiple security question answers with a user.
    /// </summary>
    private async Task<bool> AddUserSecurityQuestionList(string jsonContent)
    {
        var userSecurityQuestionModel = jsonContent.JsonDeserialize<IList<UserSecurityQuestionModel>>();
        var frameworkResult = await UserAccountService.AddUserSecurityQuestionAsync(userSecurityQuestionModel);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for changing a user's password.
    /// Parses user ID, current password, and new password from the JSON body.
    /// </summary>
    private async Task<bool> ChangePassword(string jsonContent)
    {
        var jsonObjects = JObject.Parse(jsonContent);
        var userId = jsonObjects[ApiRouteParameterConstants.UserId].ToObject<Guid>();
        var currentPassword = jsonObjects[ApiRouteParameterConstants.CurrentPassword].ToObject<string>();
        var newPassword = jsonObjects[ApiRouteParameterConstants.NewPassword].ToObject<string>();

        var frameworkResult = await UserAccountService.ChangePasswordAsync(userId, currentPassword, newPassword);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for deleting a system-level security question by its GUID.
    /// </summary>
    private async Task<bool> DeleteSecurityQuestion(string jsonContent)
    {
        var securityQuestionId = jsonContent.JsonDeserialize<Guid>();

        var frameworkResult = await UserAccountService.DeleteSecurityQuestionAsync(securityQuestionId);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for deleting a user account by user ID.
    /// </summary>
    private async Task<bool> DeleteUserById(string jsonContent)
    {
        var userId = jsonContent.JsonDeserialize<Guid>();

        var frameworkResult = await UserAccountService.DeleteUserAsync(userId);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for deleting a user account by username.
    /// </summary>
    private async Task<bool> DeleteUserByName(string jsonContent)
    {
        var userName = jsonContent.JsonDeserialize<string>();

        var frameworkResult = await UserAccountService.DeleteUserAsync(userName);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for removing a single security question answer from a user.
    /// </summary>
    private async Task<bool> DeleteUserSecurityQuestion(string jsonContent)
    {
        var userSecurityQuestionModel = jsonContent.JsonDeserialize<UserSecurityQuestionModel>();

        var frameworkResult = await UserAccountService.DeleteUserSecurityQuestionAsync(userSecurityQuestionModel);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for removing multiple security question answers from a user.
    /// </summary>
    private async Task<bool> DeleteUserSecurityQuestionList(string jsonContent)
    {
        var userSecurityQuestionModel = jsonContent.JsonDeserialize<List<UserSecurityQuestionModel>>();

        var frameworkResult = await UserAccountService.DeleteUserSecurityQuestionAsync(userSecurityQuestionModel);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for generating an email confirmation token for a user.
    /// </summary>
    private async Task<bool> GenerateEmailConfirmationToken(string jsonContent)
    {
        var username = jsonContent.JsonDeserialize<string>();
        var frameworkResult = await UserAccountService.GenerateEmailConfirmationTokenAsync(username);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for generating a phone number confirmation token for a user.
    /// </summary>
    private async Task<bool> GeneratePhoneNumberConfirmationToken(string jsonContent)
    {
        var username = jsonContent.JsonDeserialize<string>();
        var frameworkResult = await UserAccountService.GeneratePhoneNumberConfirmationTokenAsync(username);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for generating a two-factor email verification token.
    /// </summary>
    private async Task<bool> GenerateEmailTwoFactorToken(string jsonContent)
    {
        var username = jsonContent.JsonDeserialize<string>();
        var frameworkResult = await UserAccountService.GenerateEmailTwoFactorTokenAsync(username);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for generating a password reset token.
    /// Parses username and notification type from the JSON body.
    /// </summary>
    private async Task<bool> GeneratePasswordResetToken(string jsonContent)
    {
        var jsonObjects = JObject.Parse(jsonContent);
        var username = jsonObjects[ApiRouteParameterConstants.UserName].ToObject<string>();
        var notificatype = jsonObjects[ApiRouteParameterConstants.NotificationType].ToObject<NotificationTypes>();

        var frameworkResult = await UserAccountService.GeneratePasswordResetTokenAsync(username, notificatype);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for generating a two-factor SMS verification token.
    /// </summary>
    private async Task<bool> GenerateSmsTwoFactorToken(string jsonContent)
    {
        var username = jsonContent.JsonDeserialize<string>();
        var frameworkResult = await UserAccountService.GenerateSmsTwoFactorTokenAsync(username);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for generating a custom user token with a notification template.
    /// Parses username, purpose, template name, and notification type from the JSON body.
    /// </summary>
    private async Task<bool> GenerateUserTokenAsync(string jsonContent)
    {
        var jsonObjects = JObject.Parse(jsonContent);
        var username = jsonObjects[ApiRouteParameterConstants.UserName].ToObject<string>();
        var purpose = jsonObjects[ApiRouteParameterConstants.TokenPurpose].ToObject<string>();
        var template = jsonObjects[ApiRouteParameterConstants.NotificationTemplate].ToObject<string>();
        var notificatype = jsonObjects[ApiRouteParameterConstants.NotificationType].ToObject<NotificationTypes>();

        var frameworkResult =
            await UserAccountService.GenerateUserTokenAsync(username, purpose, template, notificatype);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for retrieving all system-level security questions.
    /// </summary>
    private async Task<bool> GetAllSecurityQuestions(string jsonContent)
    {
        var frameworkResult = await UserAccountService.GetAllSecurityQuestionsAsync();
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving all available two-factor authentication types.
    /// </summary>
    private async Task<bool> GetAllTwoFactorType(string jsonContent)
    {
        var frameworkResult = await UserAccountService.GetAllTwoFactorTypeAsync();
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving raw identity claims for a user by user ID.
    /// </summary>
    private async Task<bool> GetClaims(string jsonContent)
    {
        var userId = jsonContent.JsonDeserialize<Guid>();
        var frameworkResult = await UserAccountService.GetClaimsAsync(userId);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving a user by email address.
    /// </summary>
    private async Task<bool> GetUserByEmail(string jsonContent)
    {
        var email = jsonContent.JsonDeserialize<string>();
        var frameworkResult = await UserAccountService.GetUserByEmailAsync(email);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving all users.
    /// </summary>
    private async Task<bool> GetAllUsers(string jsonContent)
    {
        var userList = await UserAccountService.GetAllUsersAsync();
        await GenerateApiResults(userList);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving a user by unique identifier.
    /// </summary>
    private async Task<bool> GetUserById(string jsonContent)
    {
        var userId = jsonContent.JsonDeserialize<Guid>();
        var frameworkResult = await UserAccountService.GetUserByIdAsync(userId);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving a user by username.
    /// </summary>
    private async Task<bool> GetUserByName(string jsonContent)
    {
        var userName = jsonContent.JsonDeserialize<string>();
        var frameworkResult = await UserAccountService.GetUserByNameAsync(userName);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving custom user claims by user ID.
    /// </summary>
    private async Task<bool> GetUserClaims(string jsonContent)
    {
        var userId = jsonContent.JsonDeserialize<Guid>();
        var frameworkResult = await UserAccountService.GetUserClaimsAsync(userId);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving admin-level user claims by user ID.
    /// </summary>
    private async Task<bool> GetAdminUserClaims(string jsonContent)
    {
        var userId = jsonContent.JsonDeserialize<Guid>();
        var frameworkResult = await UserAccountService.GetAdminUserClaimsAsync(userId);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving combined role and claim permissions by user ID.
    /// </summary>
    private async Task<bool> GetUserRoleClaimsById(string jsonContent)
    {
        var userId = jsonContent.JsonDeserialize<Guid>();
        var frameworkResult = await UserAccountService.GetUserRoleClaimsByIdAsync(userId);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving combined role and claim permissions by username.
    /// </summary>
    private async Task<bool> GetUserRoleClaimsByName(string jsonContent)
    {
        var userName = jsonContent.JsonDeserialize<string>();
        var frameworkResult = await UserAccountService.GetUserRoleClaimsByNameAsync(userName);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving a user's assigned role names.
    /// </summary>
    private async Task<bool> GetUserRoles(string jsonContent)
    {
        var userId = jsonContent.JsonDeserialize<Guid>();
        var frameworkResult = await UserAccountService.GetUserRolesAsync(userId);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving a user's security question answers.
    /// </summary>
    private async Task<bool> GetUserSecurityQuestions(string jsonContent)
    {
        var userId = jsonContent.JsonDeserialize<Guid>();
        var frameworkResult = await UserAccountService.GetUserSecurityQuestionsAsync(userId);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving users who have a specific claim type and value.
    /// Parses claim type and claim value from the JSON body.
    /// </summary>
    private async Task<bool> GetUsersForClaim(string jsonContent)
    {
        var jsonObjects = JObject.Parse(jsonContent);
        var claimType = jsonObjects[ApiRouteParameterConstants.ClaimType].ToObject<string>();
        var claimValue = jsonObjects[ApiRouteParameterConstants.ClaimValue].ToObject<string>();
        var frameworkResult = await UserAccountService.GetUsersForClaimAsync(claimType, claimValue);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving all users assigned to a specific role.
    /// </summary>
    private async Task<bool> GetUsersInRole(string jsonContent)
    {
        var roleName = jsonContent.JsonDeserialize<string>();
        var frameworkResult = await UserAccountService.GetUsersInRoleAsync(roleName);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for checking user existence by claims principal.
    /// </summary>
    private async Task<bool> IsUserExistsByClaimPrincipal(string jsonContent)
    {
        var claimsPrincipal = jsonContent.JsonDeserialize<ClaimsPrincipal>();
        var frameworkResult = await UserAccountService.IsUserExistsAsync(claimsPrincipal);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for checking user existence by user ID.
    /// </summary>
    private async Task<bool> IsUserExistsById(string jsonContent)
    {
        var userId = jsonContent.JsonDeserialize<Guid>();
        var frameworkResult = await UserAccountService.IsUserExistsAsync(userId);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for checking user existence by username.
    /// </summary>
    private async Task<bool> IsUserExistsByName(string jsonContent)
    {
        var userName = jsonContent.JsonDeserialize<string>();
        var frameworkResult = await UserAccountService.IsUserExistsAsync(userName);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for registering a new user account.
    /// </summary>
    private async Task<bool> RegisterUser(string jsonContent)
    {
        var userModel = jsonContent.JsonDeserialize<UserModel>();
        var frameworkResult = await UserAccountService.RegisterUserAsync(userModel);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for removing a single admin-level claim from a user.
    /// </summary>
    private async Task<bool> RemoveAdminClaim(string jsonContent)
    {
        var userClaimModel = jsonContent.JsonDeserialize<UserClaimModel>();
        var frameworkResult = await UserAccountService.RemoveAdminClaimAsync(userClaimModel);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for removing multiple admin-level claims from a user.
    /// </summary>
    private async Task<bool> RemoveAdminClaimList(string jsonContent)
    {
        var userClaimModel = jsonContent.JsonDeserialize<IList<UserClaimModel>>();
        var frameworkResult = await UserAccountService.RemoveAdminClaimAsync(userClaimModel);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for removing a single claim from a user.
    /// </summary>
    private async Task<bool> RemoveClaim(string jsonContent)
    {
        var userClaimModel = jsonContent.JsonDeserialize<UserClaimModel>();
        var frameworkResult = await UserAccountService.RemoveClaimAsync(userClaimModel);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for removing multiple claims from a user.
    /// </summary>
    private async Task<bool> RemoveClaimList(string jsonContent)
    {
        var userClaimModel = jsonContent.JsonDeserialize<IList<UserClaimModel>>();
        var frameworkResult = await UserAccountService.RemoveClaimAsync(userClaimModel);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for removing a role assignment from a user.
    /// </summary>
    private async Task<bool> RemoveUserRole(string jsonContent)
    {
        var userRoleModel = jsonContent.JsonDeserialize<UserRoleModel>();
        var frameworkResult = await UserAccountService.RemoveUserRoleAsync(userRoleModel);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for removing multiple role assignments from a user.
    /// </summary>
    private async Task<bool> RemoveUserRoleList(string jsonContent)
    {
        var userRoleModel = jsonContent.JsonDeserialize<IList<UserRoleModel>>();
        var frameworkResult = await UserAccountService.RemoveUserRolesAsync(userRoleModel);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for replacing an existing user claim with a new one.
    /// </summary>
    private async Task<bool> ReplaceClaim(string jsonContent)
    {
        var existingUserClaimModel = jsonContent.JsonDeserialize<UserClaimModel>();
        var newUserClaimModel = jsonContent.JsonDeserialize<UserClaimModel>();
        var frameworkResult = await UserAccountService.ReplaceClaimAsync(existingUserClaimModel, newUserClaimModel);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for resetting a user's password using a reset token.
    /// Parses username, reset token, and new password from the JSON body.
    /// </summary>
    private async Task<bool> ResetPassword(string jsonContent)
    {
        var jsonObjects = JObject.Parse(jsonContent);
        var username = jsonObjects[ApiRouteParameterConstants.UserName].ToObject<string>();
        var passwordResetToken = jsonObjects[ApiRouteParameterConstants.PasswordResetToken].ToObject<string>();
        var newPassword = jsonObjects[ApiRouteParameterConstants.NewPassword].ToObject<string>();
        var frameworkResult = await UserAccountService.ResetPasswordAsync(username, passwordResetToken, newPassword);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for enabling or disabling two-factor authentication for a user.
    /// Parses user ID and enabled flag from the JSON body.
    /// </summary>
    private async Task<bool> SetTwoFactorEnabled(string jsonContent)
    {
        var jsonObjects = JObject.Parse(jsonContent);
        var userId = jsonObjects[ApiRouteParameterConstants.UserId].ToObject<Guid>();
        var enabled = jsonObjects[ApiRouteParameterConstants.Enabled].ToObject<bool>();
        var frameworkResult = await UserAccountService.SetTwoFactorEnabledAsync(userId, enabled);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for unlocking a user account using a verification token.
    /// Parses username, token, and purpose from the JSON body.
    /// </summary>
    private async Task<bool> UnLockUserByToken(string jsonContent)
    {
        var jsonObjects = JObject.Parse(jsonContent);
        var username = jsonObjects[ApiRouteParameterConstants.UserName].ToObject<string>();
        var token = jsonObjects[ApiRouteParameterConstants.UserToken].ToObject<string>();
        var purpose = jsonObjects[ApiRouteParameterConstants.TokenPurpose].ToObject<string>();
        var frameworkResult = await UserAccountService.UnLockUserAsync(username, token, purpose);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for unlocking a user account by username.
    /// </summary>
    private async Task<bool> UnLockUser(string jsonContent)
    {
        // Roshan Bashyam : Commented because not sending as an object.Commented Code to be removed after discussion
        //JObject jsonObjects = JObject.Parse(jsonContent);
        //var userId = jsonObjects[ApiRouteParameterConstants.UserId].ToObject<Guid>();
        var username = jsonContent.JsonDeserialize<string>();
        var frameworkResult = await UserAccountService.UnLockUserAsync(username);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for unlocking a user account by verifying security question answers.
    /// Parses username and security question answers from the JSON body.
    /// </summary>
    private async Task<bool> UnLockUserByuserSecurityQuestions(string jsonContent)
    {
        var jsonObjects = JObject.Parse(jsonContent);
        var username = jsonObjects[ApiRouteParameterConstants.UserName].ToObject<string>();
        var securityQuestionModel = jsonObjects[ApiRouteParameterConstants.ListOfUserSecurityQuestions]
            .ToObject<List<UserSecurityQuestionModel>>();
        var frameworkResult = await UserAccountService.UnlockUserAsync(username, securityQuestionModel);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for updating a system-level security question.
    /// </summary>
    private async Task<bool> UpdateSecurityQuestion(string jsonContent)
    {
        var securityQuestionModel = jsonContent.JsonDeserialize<SecurityQuestionModel>();
        var frameworkResult = await UserAccountService.UpdateSecurityQuestionAsync(securityQuestionModel);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for updating a user's profile information.
    /// </summary>
    private async Task<bool> UpdateUser(string jsonContent)
    {
        var userModel = jsonContent.JsonDeserialize<UserModel>();
        var frameworkResult = await UserAccountService.UpdateUserAsync(userModel);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for updating a user's security question answer.
    /// </summary>
    private async Task<bool> UpdateUserSecurityQuestion(string jsonContent)
    {
        var userSecurityQuestionModel = jsonContent.JsonDeserialize<UserSecurityQuestionModel>();
        var frameworkResult = await UserAccountService.UpdateUserSecurityQuestionAsync(userSecurityQuestionModel);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for updating a user's two-factor authentication type.
    /// Parses user ID and two-factor type from the JSON body.
    /// </summary>
    private async Task<bool> UpdateUserTwoFactorType(string jsonContent)
    {
        var jsonObjects = JObject.Parse(jsonContent);
        var userId = jsonObjects[ApiRouteParameterConstants.UserId].ToObject<Guid>();
        var twoFactorType = jsonObjects[ApiRouteParameterConstants.TwoFactorType].ToObject<TwoFactorType>();

        var frameworkResult = await UserAccountService.UpdateUserTwoFactorTypeAsync(userId, twoFactorType);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for verifying an email confirmation token.
    /// Parses username and email token from the JSON body.
    /// </summary>
    private async Task<bool> VerifyEmailConfirmationToken(string jsonContent)
    {
        var jsonObjects = JObject.Parse(jsonContent);
        var username = jsonObjects[ApiRouteParameterConstants.UserName].ToObject<string>();
        var emailToken = jsonObjects[ApiRouteParameterConstants.EmailToken].ToObject<string>();
        var frameworkResult = await UserAccountService.VerifyEmailConfirmationTokenAsync(username, emailToken);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for verifying a two-factor email token.
    /// Parses username and email token from the JSON body.
    /// </summary>
    private async Task<bool> VerifyEmailTwoFactorToken(string jsonContent)
    {
        var jsonObjects = JObject.Parse(jsonContent);
        var username = jsonObjects[ApiRouteParameterConstants.UserName].ToObject<string>();
        var emailToken = jsonObjects[ApiRouteParameterConstants.EmailToken].ToObject<string>();
        var frameworkResult = await UserAccountService.VerifyEmailTwoFactorTokenAsync(username, emailToken);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for verifying a phone number confirmation token.
    /// Parses username and SMS token from the JSON body.
    /// </summary>
    private async Task<bool> VerifyPhoneNumberConfirmationToken(string jsonContent)
    {
        var jsonObjects = JObject.Parse(jsonContent);
        var username = jsonObjects[ApiRouteParameterConstants.UserName].ToObject<string>();
        var smsToken = jsonObjects[ApiRouteParameterConstants.SmsToken].ToObject<string>();
        var frameworkResult = await UserAccountService.VerifyPhoneNumberConfirmationTokenAsync(username, smsToken);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for verifying a two-factor SMS token.
    /// Parses username and SMS token from the JSON body.
    /// </summary>
    private async Task<bool> VerifySmsTwoFactorToken(string jsonContent)
    {
        var jsonObjects = JObject.Parse(jsonContent);
        var username = jsonObjects[ApiRouteParameterConstants.UserName].ToObject<string>();
        var smsToken = jsonObjects[ApiRouteParameterConstants.SmsToken].ToObject<string>();
        var frameworkResult = await UserAccountService.VerifySmsTwoFactorTokenAsync(username, smsToken);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for verifying a custom user token for a specified purpose.
    /// Parses username, purpose, and token from the JSON body.
    /// </summary>
    private async Task<bool> VerifyUserToken(string jsonContent)
    {
        var jsonObjects = JObject.Parse(jsonContent);
        var username = jsonObjects[ApiRouteParameterConstants.UserName].ToObject<string>();
        var purpose = jsonObjects[ApiRouteParameterConstants.TokenPurpose].ToObject<string>();
        var token = jsonObjects[ApiRouteParameterConstants.UserToken].ToObject<string>();
        var frameworkResult = await UserAccountService.VerifyUserTokenAsync(username, purpose, token);
        await GenerateApiResults(frameworkResult);
        return true;
    }
}
