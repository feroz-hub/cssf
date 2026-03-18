/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Service.Extension;
using HCL.CS.SF.Service.Implementation.Api.Extension;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;

namespace HCL.CS.SF.Service.Implementation.Api.Services;

/// <summary>
/// Partial class extension of UserAccountService providing two-factor authentication (2FA)
/// type management. Allows enumeration of supported 2FA methods, enabling/disabling 2FA
/// for users, and switching between 2FA types (email, SMS, authenticator app, none).
/// </summary>

public partial class UserAccountService : SecurityBase, IUserAccountService
{
    /// <summary>
    /// Retrieves all available two-factor authentication types from the TwoFactorType enum.
    /// </summary>
    /// <returns>A list of all supported 2FA type names.</returns>

    public virtual async Task<IList<string>> GetAllTwoFactorTypeAsync()
    {
        try
        {
            var twoFactorTypes = Enum.GetValues(typeof(TwoFactorType)).Cast<string>().ToList();
            return await Task.FromResult(twoFactorTypes);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while getting all two factor type.");
            throw;
        }
    }

    /// <summary>
    /// Enables or disables two-factor authentication for a specific user.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="enabled">True to enable 2FA; false to disable.</param>
    /// <returns>The result of the 2FA state change operation.</returns>

    public virtual async Task<FrameworkResult> SetTwoFactorEnabledAsync(Guid userId, bool enabled)
    {
        if (!userId.IsValid()) return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserId);

        try
        {
            var user = await FindByIdAsync(userId);
            if (user != null)
            {
                var concurrencyStamp = user.ConcurrencyStamp;
                loggerService.WriteTo(Log.Debug,
                    "Entered into set two factor authentication for specified user : " + user.UserName);
                var result = await userManager.SetTwoFactorEnabledAsync(user, enabled);
                if (!result.Succeeded) return frameworkResultService.Failed(result.ConstructIdentityErrorAsList());

                await userManagementUnitOfWork.SetModifiedStatusAsync(user, concurrencyStamp);
                return await userManagementUnitOfWork.SaveChangesAsync();
            }

            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserId);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while Setting two Factor enabled.");
            throw;
        }
    }

    /// <summary>
    /// Updates the user's preferred two-factor authentication type. Setting the type to None
    /// automatically disables 2FA; setting any other type enables it.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="twoFactorType">The desired 2FA type (Email, SMS, AuthenticatorApp, or None).</param>
    /// <returns>The result of the 2FA type update operation.</returns>

    public virtual async Task<FrameworkResult> UpdateUserTwoFactorTypeAsync(Guid userId, TwoFactorType twoFactorType)
    {
        if (!userId.IsValid()) return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserId);

        try
        {
            var existingUser = await FindByIdAsync(userId);
            if (existingUser != null)
            {
                var concurrencyStamp = existingUser.ConcurrencyStamp;
                loggerService.WriteTo(Log.Debug,
                    "Entered in update user two factor type for user: " + existingUser.UserName);

                existingUser.TwoFactorType = twoFactorType;
                existingUser.TwoFactorEnabled = true;
                if (twoFactorType == TwoFactorType.None) existingUser.TwoFactorEnabled = false;

                var result = await userManager.UpdateAsync(existingUser);
                if (!result.Succeeded) return frameworkResultService.Failed(result.ConstructIdentityErrorAsList());

                await userManagementUnitOfWork.SetModifiedStatusAsync(existingUser, concurrencyStamp);
                return await userManagementUnitOfWork.SaveChangesAsync();
            }

            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserId);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while update user two factor type.");
            throw;
        }
    }
}
