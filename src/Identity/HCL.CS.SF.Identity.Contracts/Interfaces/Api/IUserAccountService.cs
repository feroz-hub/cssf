/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Models.Api;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Api;

/// <summary>
/// Core user account management service providing registration, profile updates, deletion,
/// password management (change/reset), account locking/unlocking, and user look-up operations.
/// This is a partial interface -- additional members are defined in separate files for
/// claims, roles, tokens, security questions, and two-factor configuration.
/// </summary>
public partial interface IUserAccountService
{
    /// <summary>Registers a new user account.</summary>
    /// <param name="user">The user model containing registration details.</param>
    Task<FrameworkResult> RegisterUserAsync(UserModel user);

    /// <summary>Updates an existing user's profile information.</summary>
    /// <param name="userModel">The user model with updated fields.</param>
    Task<FrameworkResult> UpdateUserAsync(UserModel userModel);

    /// <summary>Soft-deletes a user account by username.</summary>
    /// <param name="username">The username of the account to delete.</param>
    Task<FrameworkResult> DeleteUserAsync(string username);

    /// <summary>Soft-deletes a user account by user ID.</summary>
    /// <param name="userId">The unique identifier of the user to delete.</param>
    Task<FrameworkResult> DeleteUserAsync(Guid userId);

    /// <summary>Changes the user's password after verifying the current password.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="currentPassword">The user's current password for verification.</param>
    /// <param name="newPassword">The new password to set.</param>
    Task<FrameworkResult> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);

    /// <summary>Resets a user's password using a previously generated reset token.</summary>
    /// <param name="username">The username of the account.</param>
    /// <param name="passwordResetToken">The password reset token.</param>
    /// <param name="newPassword">The new password to set.</param>
    Task<FrameworkResult> ResetPasswordAsync(string username, string passwordResetToken, string newPassword);

    /// <summary>Locks a user account indefinitely.</summary>
    /// <param name="userId">The unique identifier of the user to lock.</param>
    Task<FrameworkResult> LockUserAsync(Guid userId);

    /// <summary>Locks a user account until the specified date/time.</summary>
    /// <param name="userId">The unique identifier of the user to lock.</param>
    /// <param name="dateTime">The lockout end date, or <c>null</c> for indefinite lockout.</param>
    Task<FrameworkResult> LockUserAsync(Guid userId, DateTime? dateTime);

    /// <summary>Unlocks a previously locked user account by username.</summary>
    /// <param name="username">The username of the account to unlock.</param>
    Task<FrameworkResult> UnLockUserAsync(string username);

    /// <summary>Unlocks a user account using a token-based verification.</summary>
    /// <param name="username">The username of the account to unlock.</param>
    /// <param name="token">The unlock verification token.</param>
    /// <param name="purpose">The purpose of the token (e.g., "AccountUnlock").</param>
    Task<FrameworkResult> UnLockUserAsync(string username, string token, string purpose);

    /// <summary>Unlocks a user account by verifying security question answers.</summary>
    /// <param name="username">The username of the account to unlock.</param>
    /// <param name="userSecurityQuestions">The user's security question answers.</param>
    Task<FrameworkResult> UnlockUserAsync(string username, IList<UserSecurityQuestionModel> userSecurityQuestions);

    /// <summary>Retrieves a user by username.</summary>
    /// <param name="userName">The username to look up.</param>
    Task<UserModel> GetUserByNameAsync(string userName);

    /// <summary>Retrieves a user by email address.</summary>
    /// <param name="email">The email address to look up.</param>
    Task<UserModel> GetUserByEmailAsync(string email);

    /// <summary>Retrieves a user by unique identifier.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    Task<UserModel> GetUserByIdAsync(Guid userId);

    /// <summary>Retrieves all users that have a specific claim type and value.</summary>
    /// <param name="claimType">The claim type to filter by.</param>
    /// <param name="claimValue">The claim value to filter by.</param>
    Task<IList<UserModel>> GetUsersForClaimAsync(string claimType, string claimValue);

    /// <summary>Checks whether the user identified by the claims principal exists.</summary>
    /// <param name="claimsPrincipal">The claims principal to check.</param>
    Task<bool> IsUserExistsAsync(ClaimsPrincipal claimsPrincipal);

    /// <summary>Checks whether a user with the given ID exists.</summary>
    /// <param name="userId">The unique identifier to check.</param>
    Task<bool> IsUserExistsAsync(Guid userId);

    /// <summary>Checks whether a user with the given username exists.</summary>
    /// <param name="userName">The username to check.</param>
    Task<bool> IsUserExistsAsync(string userName);

    /// <summary>Retrieves all users as display models (lightweight projection).</summary>
    Task<IList<UserDisplayModel>> GetAllUsersAsync();
}
