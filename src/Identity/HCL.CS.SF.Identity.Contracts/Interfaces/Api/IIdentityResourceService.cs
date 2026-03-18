/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Models.Api;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Api;

/// <summary>
/// Service for managing OIDC identity resources (e.g., openid, profile, email) and their
/// associated claims. Provides full CRUD for identity resources and claims, plus scope-based
/// look-ups used during token issuance to determine which claims to include in ID tokens.
/// </summary>
public interface IIdentityResourceService
{
    /// <summary>Creates a new identity resource.</summary>
    /// <param name="identityResourceModel">The identity resource to create.</param>
    Task<FrameworkResult> AddIdentityResourceAsync(IdentityResourcesModel identityResourceModel);

    /// <summary>Updates an existing identity resource.</summary>
    /// <param name="identityResourceModel">The identity resource with updated fields.</param>
    Task<FrameworkResult> UpdateIdentityResourceAsync(IdentityResourcesModel identityResourceModel);

    /// <summary>Deletes an identity resource by its unique identifier.</summary>
    /// <param name="identityResourceId">The unique identifier of the identity resource.</param>
    Task<FrameworkResult> DeleteIdentityResourceAsync(Guid identityResourceId);

    /// <summary>Deletes an identity resource by its name.</summary>
    /// <param name="identityResourceName">The name of the identity resource.</param>
    Task<FrameworkResult> DeleteIdentityResourceAsync(string identityResourceName);

    /// <summary>Retrieves an identity resource by its unique identifier.</summary>
    /// <param name="identityResourceId">The unique identifier of the identity resource.</param>
    Task<IdentityResourcesModel> GetIdentityResourceAsync(Guid identityResourceId);

    /// <summary>Retrieves an identity resource by its name.</summary>
    /// <param name="identityResourceName">The name of the identity resource.</param>
    Task<IdentityResourcesModel> GetIdentityResourceAsync(string identityResourceName);

    /// <summary>Retrieves all registered identity resources.</summary>
    Task<IList<IdentityResourcesModel>> GetAllIdentityResourcesAsync();

    /// <summary>Retrieves identity resources matching the requested scope list.</summary>
    /// <param name="requestedScopes">The scope names requested by the client.</param>
    Task<IList<IdentityResourcesByScopesModel>> GetAllIdentityResourcesByScopesAsync(IList<string> requestedScopes);

    /// <summary>Adds a claim to an identity resource.</summary>
    /// <param name="identityClaimsModel">The identity claim to add.</param>
    Task<FrameworkResult> AddIdentityResourceClaimAsync(IdentityClaimsModel identityClaimsModel);

    /// <summary>Deletes all claims for the specified identity resource.</summary>
    /// <param name="identityResourceId">The unique identifier of the identity resource.</param>
    Task<FrameworkResult> DeleteIdentityResourceClaimByResourceIdAsync(Guid identityResourceId);

    /// <summary>Deletes a single identity resource claim by its unique identifier.</summary>
    /// <param name="identityResourceClaimId">The unique identifier of the claim.</param>
    Task<FrameworkResult> DeleteIdentityResourceClaimByIdAsync(Guid identityResourceClaimId);

    /// <summary>Deletes an identity resource claim by its type and value.</summary>
    /// <param name="identityClaimsModel">The claim to delete.</param>
    Task<FrameworkResult> DeleteIdentityResourceClaimAsync(IdentityClaimsModel identityClaimsModel);

    /// <summary>Retrieves all claims for the specified identity resource.</summary>
    /// <param name="identityResourceId">The unique identifier of the identity resource.</param>
    Task<IList<IdentityClaimsModel>> GetIdentityResourceClaimsAsync(Guid identityResourceId);
}
