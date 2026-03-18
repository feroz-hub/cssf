/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using AutoMapper;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.DomainServices.Repository.Api;
using HCL.CS.SF.Service.Implementation.Api.Services;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api.Wrapper;

namespace HCL.CS.SF.ProxyService.Proxy;

/// <summary>
/// Gateway proxy for the API resource management service. Routes requests to the backend
/// <see cref="ApiResourceService"/> after enforcing API-level permission validation.
/// Manages API resources, API scopes, and their associated claims.
/// </summary>
public sealed class ApiResourceProxyService : ApiResourceService, IApiResourceService
{
    /// <summary>
    /// Validator that checks whether the caller has permission to invoke the requested operation.
    /// </summary>
    private readonly IApiValidator apiValidator;

    /// <summary>
    /// Service used to construct failure responses when validation fails.
    /// </summary>
    private readonly IFrameworkResultService frameworkResult;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiResourceProxyService"/> class.
    /// </summary>
    public ApiResourceProxyService(
        IApiValidator apiValidator,
        ILoggerInstance instance,
        IMapper mapper,
        IFrameworkResultService frameworkResult,
        IApiResourceRepository apiResourceRepository,
        IRepository<ApiResourceClaims> apiResourceClaimRepository,
        IRepository<ApiScopes> apiScopeRepository,
        IRepository<ApiScopeClaims> apiScopeClaimRepository)
        : base(
            instance,
            mapper,
            frameworkResult,
            apiResourceRepository,
            apiResourceClaimRepository,
            apiScopeRepository,
            apiScopeClaimRepository)
    {
        this.apiValidator = apiValidator;
        this.frameworkResult = frameworkResult;
    }

    /// <summary>
    /// Adds a new API resource after permission validation.
    /// </summary>
    /// <param name="apiResourceModel">The API resource model to create.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> AddApiResourceAsync(ApiResourcesModel apiResourceModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.AddApiResourceAsync(apiResourceModel);
    }

    /// <summary>
    /// Updates an existing API resource after permission validation.
    /// </summary>
    /// <param name="apiResourceModel">The updated API resource model.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> UpdateApiResourceAsync(ApiResourcesModel apiResourceModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.UpdateApiResourceAsync(apiResourceModel);
    }

    /// <summary>
    /// Deletes an API resource by its unique identifier after permission validation.
    /// </summary>
    /// <param name="apiResourceId">The GUID of the API resource to delete.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> DeleteApiResourceAsync(Guid apiResourceId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.DeleteApiResourceAsync(apiResourceId);
    }

    /// <summary>
    /// Deletes an API resource by its name after permission validation.
    /// </summary>
    /// <param name="apiResourceName">The name of the API resource to delete.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> DeleteApiResourceAsync(string apiResourceName)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.DeleteApiResourceAsync(apiResourceName);
    }

    /// <summary>
    /// Retrieves an API resource by its unique identifier after permission validation.
    /// </summary>
    /// <param name="apiResourceId">The GUID of the API resource.</param>
    /// <returns>The API resource model, or throws if validation fails.</returns>
    public override async Task<ApiResourcesModel> GetApiResourceAsync(Guid apiResourceId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetApiResourceAsync(apiResourceId);
    }

    /// <summary>
    /// Retrieves an API resource by its name after permission validation.
    /// </summary>
    /// <param name="apiResourceName">The name of the API resource.</param>
    /// <returns>The API resource model, or throws if validation fails.</returns>
    public override async Task<ApiResourcesModel> GetApiResourceAsync(string apiResourceName)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetApiResourceAsync(apiResourceName);
    }

    /// <summary>
    /// Retrieves all API resources after permission validation.
    /// </summary>
    /// <returns>A list of all API resource models.</returns>
    public override async Task<IList<ApiResourcesModel>> GetAllApiResourcesAsync()
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetAllApiResourcesAsync();
    }

    /// <summary>
    /// Retrieves API resources filtered by their associated scopes after permission validation.
    /// </summary>
    /// <param name="requestedScopes">The list of scope names to filter by.</param>
    /// <returns>A list of API resources matching the requested scopes.</returns>
    public override async Task<IList<ApiResourcesByScopesModel>> GetAllApiResourcesByScopesAsync(
        IList<string> requestedScopes)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetAllApiResourcesByScopesAsync(requestedScopes);
    }

    /// <summary>
    /// Adds a claim to an API resource after permission validation.
    /// </summary>
    /// <param name="apiResourceClaimModel">The claim to associate with the resource.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> AddApiResourceClaimAsync(ApiResourceClaimsModel apiResourceClaimModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.AddApiResourceClaimAsync(apiResourceClaimModel);
    }

    /// <summary>
    /// Deletes all claims for an API resource by resource ID after permission validation.
    /// </summary>
    /// <param name="apiResourceId">The GUID of the API resource whose claims to delete.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> DeleteApiResourceClaimByResourceIdAsync(Guid apiResourceId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.DeleteApiResourceClaimByResourceIdAsync(apiResourceId);
    }

    /// <summary>
    /// Deletes a specific API resource claim by its unique identifier after permission validation.
    /// </summary>
    /// <param name="apiResourceClaimId">The GUID of the claim to delete.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> DeleteApiResourceClaimByIdAsync(Guid apiResourceClaimId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.DeleteApiResourceClaimByIdAsync(apiResourceClaimId);
    }

    /// <summary>
    /// Deletes an API resource claim matching the given model after permission validation.
    /// </summary>
    /// <param name="apiResourceClaimModel">The claim model to match and delete.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> DeleteApiResourceClaimAsync(
        ApiResourceClaimsModel apiResourceClaimModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.DeleteApiResourceClaimAsync(apiResourceClaimModel);
    }

    /// <summary>
    /// Retrieves all claims for the specified API resource after permission validation.
    /// </summary>
    /// <param name="apiResourceId">The GUID of the API resource.</param>
    /// <returns>A list of claim models for the resource.</returns>
    public override async Task<IList<ApiResourceClaimsModel>> GetApiResourceClaimsAsync(Guid apiResourceId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetApiResourceClaimsAsync(apiResourceId);
    }

    /// <summary>
    /// Adds a new API scope after permission validation.
    /// </summary>
    /// <param name="apiScopesModel">The scope model to create.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> AddApiScopeAsync(ApiScopesModel apiScopesModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.AddApiScopeAsync(apiScopesModel);
    }

    /// <summary>
    /// Updates an existing API scope after permission validation.
    /// </summary>
    /// <param name="apiScopesModel">The updated scope model.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> UpdateApiScopeAsync(ApiScopesModel apiScopesModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.UpdateApiScopeAsync(apiScopesModel);
    }

    /// <summary>
    /// Deletes an API scope by its unique identifier after permission validation.
    /// </summary>
    /// <param name="apiScopeId">The GUID of the scope to delete.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> DeleteApiScopeAsync(Guid apiScopeId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.DeleteApiScopeAsync(apiScopeId);
    }

    /// <summary>
    /// Deletes an API scope by its name after permission validation.
    /// </summary>
    /// <param name="apiScopeName">The name of the scope to delete.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> DeleteApiScopeAsync(string apiScopeName)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.DeleteApiScopeAsync(apiScopeName);
    }

    /// <summary>
    /// Retrieves an API scope by its unique identifier after permission validation.
    /// </summary>
    /// <param name="apiScopeId">The GUID of the scope.</param>
    /// <returns>The scope model.</returns>
    public override async Task<ApiScopesModel> GetApiScopeAsync(Guid apiScopeId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetApiScopeAsync(apiScopeId);
    }

    /// <summary>
    /// Retrieves an API scope by its name after permission validation.
    /// </summary>
    /// <param name="apiScopeName">The name of the scope.</param>
    /// <returns>The scope model.</returns>
    public override async Task<ApiScopesModel> GetApiScopeAsync(string apiScopeName)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetApiScopeAsync(apiScopeName);
    }

    /// <summary>
    /// Retrieves all API scopes after permission validation.
    /// </summary>
    /// <returns>A list of all API scope models.</returns>
    public override async Task<IList<ApiScopesModel>> GetAllApiScopesAsync()
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetAllApiScopesAsync();
    }

    /// <summary>
    /// Adds a claim to an API scope after permission validation.
    /// </summary>
    /// <param name="apiScopeClaimModel">The scope claim to add.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> AddApiScopeClaimAsync(ApiScopeClaimsModel apiScopeClaimModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.AddApiScopeClaimAsync(apiScopeClaimModel);
    }

    /// <summary>
    /// Deletes all claims for the specified API scope after permission validation.
    /// </summary>
    /// <param name="apiScopeId">The GUID of the scope whose claims to delete.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> DeleteApiScopeClaimByScopeIdAsync(Guid apiScopeId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.DeleteApiScopeClaimByScopeIdAsync(apiScopeId);
    }

    /// <summary>
    /// Deletes a specific API scope claim by its unique identifier after permission validation.
    /// </summary>
    /// <param name="apiScopeClaimId">The GUID of the scope claim to delete.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> DeleteApiScopeClaimByIdAsync(Guid apiScopeClaimId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.DeleteApiScopeClaimByIdAsync(apiScopeClaimId);
    }

    /// <summary>
    /// Deletes an API scope claim matching the given model after permission validation.
    /// </summary>
    /// <param name="apiScopeClaimModel">The scope claim model to match and delete.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> DeleteApiScopeClaimAsync(ApiScopeClaimsModel apiScopeClaimModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.DeleteApiScopeClaimAsync(apiScopeClaimModel);
    }

    /// <summary>
    /// Retrieves all claims for the specified API scope after permission validation.
    /// </summary>
    /// <param name="apiScopeId">The GUID of the scope.</param>
    /// <returns>A list of scope claim models.</returns>
    public override async Task<IList<ApiScopeClaimsModel>> GetApiScopeClaimsAsync(Guid apiScopeId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetApiScopeClaimsAsync(apiScopeId);
    }
}
