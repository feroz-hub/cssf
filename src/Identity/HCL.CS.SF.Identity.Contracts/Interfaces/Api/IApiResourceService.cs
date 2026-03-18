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
/// Service for managing OAuth 2.0 API resources, API scopes, and their associated claims.
/// Provides full CRUD for API resources and scopes, scope-based resource look-ups for
/// token issuance, and claim management for fine-grained access control.
/// </summary>
public interface IApiResourceService
{
    /// <summary>Registers a new API resource.</summary>
    /// <param name="apiResourceModel">The API resource to create.</param>
    Task<FrameworkResult> AddApiResourceAsync(ApiResourcesModel apiResourceModel);

    /// <summary>Updates an existing API resource.</summary>
    /// <param name="apiResourceModel">The API resource with updated fields.</param>
    Task<FrameworkResult> UpdateApiResourceAsync(ApiResourcesModel apiResourceModel);

    /// <summary>Deletes an API resource by its unique identifier.</summary>
    /// <param name="apiResourceId">The unique identifier of the API resource.</param>
    Task<FrameworkResult> DeleteApiResourceAsync(Guid apiResourceId);

    /// <summary>Deletes an API resource by its name.</summary>
    /// <param name="apiResourceName">The name of the API resource.</param>
    Task<FrameworkResult> DeleteApiResourceAsync(string apiResourceName);

    /// <summary>Retrieves an API resource by its name.</summary>
    /// <param name="apiResourceName">The name of the API resource.</param>
    Task<ApiResourcesModel> GetApiResourceAsync(string apiResourceName);

    /// <summary>Retrieves an API resource by its unique identifier.</summary>
    /// <param name="apiResourceId">The unique identifier of the API resource.</param>
    Task<ApiResourcesModel> GetApiResourceAsync(Guid apiResourceId);

    /// <summary>Retrieves all registered API resources.</summary>
    Task<IList<ApiResourcesModel>> GetAllApiResourcesAsync();

    /// <summary>Retrieves all registered API scopes.</summary>
    Task<IList<ApiScopesModel>> GetAllApiScopesAsync();

    /// <summary>Retrieves API resources that match the requested scope list.</summary>
    /// <param name="requestedScopes">The scope names requested by the client.</param>
    Task<IList<ApiResourcesByScopesModel>> GetAllApiResourcesByScopesAsync(IList<string> requestedScopes);

    /// <summary>Adds a claim to an API resource.</summary>
    /// <param name="apiResourceClaimModel">The resource claim to add.</param>
    Task<FrameworkResult> AddApiResourceClaimAsync(ApiResourceClaimsModel apiResourceClaimModel);

    /// <summary>Deletes all claims for the specified API resource.</summary>
    /// <param name="apiResourceId">The unique identifier of the API resource.</param>
    Task<FrameworkResult> DeleteApiResourceClaimByResourceIdAsync(Guid apiResourceId);

    /// <summary>Deletes a single API resource claim by its unique identifier.</summary>
    /// <param name="apiResourceClaimId">The unique identifier of the claim.</param>
    Task<FrameworkResult> DeleteApiResourceClaimByIdAsync(Guid apiResourceClaimId);

    /// <summary>Deletes an API resource claim by its type and value.</summary>
    /// <param name="apiResourceClaimModel">The claim to delete.</param>
    Task<FrameworkResult> DeleteApiResourceClaimAsync(ApiResourceClaimsModel apiResourceClaimModel);

    /// <summary>Retrieves all claims for the specified API resource.</summary>
    /// <param name="apiResourceId">The unique identifier of the API resource.</param>
    Task<IList<ApiResourceClaimsModel>> GetApiResourceClaimsAsync(Guid apiResourceId);

    /// <summary>Creates a new API scope.</summary>
    /// <param name="apiScopesModel">The API scope to create.</param>
    Task<FrameworkResult> AddApiScopeAsync(ApiScopesModel apiScopesModel);

    /// <summary>Updates an existing API scope.</summary>
    /// <param name="apiScopesModel">The API scope with updated fields.</param>
    Task<FrameworkResult> UpdateApiScopeAsync(ApiScopesModel apiScopesModel);

    /// <summary>Deletes an API scope by its unique identifier.</summary>
    /// <param name="apiScopeId">The unique identifier of the scope.</param>
    Task<FrameworkResult> DeleteApiScopeAsync(Guid apiScopeId);

    /// <summary>Deletes an API scope by its name.</summary>
    /// <param name="apiScopeName">The name of the scope.</param>
    Task<FrameworkResult> DeleteApiScopeAsync(string apiScopeName);

    /// <summary>Retrieves an API scope by its unique identifier.</summary>
    /// <param name="apiScopeId">The unique identifier of the scope.</param>
    Task<ApiScopesModel> GetApiScopeAsync(Guid apiScopeId);

    /// <summary>Retrieves an API scope by its name.</summary>
    /// <param name="apiScopeName">The name of the scope.</param>
    Task<ApiScopesModel> GetApiScopeAsync(string apiScopeName);

    /// <summary>Adds a claim to an API scope.</summary>
    /// <param name="apiScopeClaimModel">The scope claim to add.</param>
    Task<FrameworkResult> AddApiScopeClaimAsync(ApiScopeClaimsModel apiScopeClaimModel);

    /// <summary>Deletes all claims for the specified API scope.</summary>
    /// <param name="apiScopeId">The unique identifier of the scope.</param>
    Task<FrameworkResult> DeleteApiScopeClaimByScopeIdAsync(Guid apiScopeId);

    /// <summary>Deletes a single API scope claim by its unique identifier.</summary>
    /// <param name="apiScopeClaimId">The unique identifier of the claim.</param>
    Task<FrameworkResult> DeleteApiScopeClaimByIdAsync(Guid apiScopeClaimId);

    /// <summary>Deletes an API scope claim by its type and value.</summary>
    /// <param name="apiScopeClaimModel">The claim to delete.</param>
    Task<FrameworkResult> DeleteApiScopeClaimAsync(ApiScopeClaimsModel apiScopeClaimModel);

    /// <summary>Retrieves all claims for the specified API scope.</summary>
    /// <param name="apiScopeId">The unique identifier of the scope.</param>
    Task<IList<ApiScopeClaimsModel>> GetApiScopeClaimsAsync(Guid apiScopeId);
}
