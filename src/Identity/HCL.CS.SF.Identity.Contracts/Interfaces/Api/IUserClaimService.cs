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
/// Partial interface extending <see cref="IUserAccountService"/> with user claim management.
/// Supports adding, removing, and replacing both regular user claims and admin-level claims.
/// Admin claims are used for administrative API authorization.
/// </summary>
public partial interface IUserAccountService
{
    /// <summary>Adds a single claim to a user.</summary>
    /// <param name="userClaimModel">The claim to add.</param>
    Task<FrameworkResult> AddClaimAsync(UserClaimModel userClaimModel);

    /// <summary>Adds multiple claims to a user in a single operation.</summary>
    /// <param name="userClaimModels">The claims to add.</param>
    Task<FrameworkResult> AddClaimAsync(IList<UserClaimModel> userClaimModels);

    /// <summary>Adds multiple admin-level claims to a user.</summary>
    /// <param name="userClaimModels">The admin claims to add.</param>
    Task<FrameworkResult> AddAdminClaimAsync(IList<UserClaimModel> userClaimModels);

    /// <summary>Adds a single admin-level claim to a user.</summary>
    /// <param name="userClaimModel">The admin claim to add.</param>
    Task<FrameworkResult> AddAdminClaimAsync(UserClaimModel userClaimModel);

    /// <summary>Removes a single claim from a user.</summary>
    /// <param name="userClaimModel">The claim to remove.</param>
    Task<FrameworkResult> RemoveClaimAsync(UserClaimModel userClaimModel);

    /// <summary>Removes multiple claims from a user.</summary>
    /// <param name="userClaimModel">The claims to remove.</param>
    Task<FrameworkResult> RemoveClaimAsync(IList<UserClaimModel> userClaimModel);

    /// <summary>Removes a single admin-level claim from a user.</summary>
    /// <param name="userClaimModel">The admin claim to remove.</param>
    Task<FrameworkResult> RemoveAdminClaimAsync(UserClaimModel userClaimModel);

    /// <summary>Removes multiple admin-level claims from a user.</summary>
    /// <param name="userClaimModel">The admin claims to remove.</param>
    Task<FrameworkResult> RemoveAdminClaimAsync(IList<UserClaimModel> userClaimModel);

    /// <summary>Replaces an existing user claim with a new one.</summary>
    /// <param name="existingUserClaimModel">The existing claim to replace.</param>
    /// <param name="newUserClaimModel">The new claim to set.</param>
    Task<FrameworkResult> ReplaceClaimAsync(UserClaimModel existingUserClaimModel, UserClaimModel newUserClaimModel);

    /// <summary>Retrieves all claims for the specified user as <see cref="Claim"/> instances.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    Task<IList<Claim>> GetClaimsAsync(Guid userId);

    /// <summary>Retrieves all regular user claims for the specified user.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    Task<IList<UserClaimModel>> GetUserClaimsAsync(Guid userId);

    /// <summary>Retrieves all admin-level claims for the specified user.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    Task<IList<UserClaimModel>> GetAdminUserClaimsAsync(Guid userId);
}
