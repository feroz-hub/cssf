/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.ProxyService.Routes.Extension;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api.Wrapper;

namespace HCL.CS.SF.ProxyService.Routes;

/// <summary>
/// Partial class of <see cref="ApiGateway"/> containing route handlers for API resource,
/// API scope, and their associated claim management operations. Each handler deserializes
/// the JSON request body, invokes the corresponding backend service method, and writes
/// the response.
/// </summary>
internal partial class ApiGateway : BaseApiServiceInstance, IApiGateway
{
    /// <summary>
    /// Handles the route for adding a new API resource.
    /// </summary>
    private async Task<bool> AddApiResource(string jsonContent)
    {
        var apiResourcesModel = jsonContent.JsonDeserialize<ApiResourcesModel>();
        var frameworkResult = await ApiResourceService.AddApiResourceAsync(apiResourcesModel);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for updating an existing API resource.
    /// </summary>
    private async Task<bool> UpdateApiResource(string jsonContent)
    {
        var apiResourcesModel = jsonContent.JsonDeserialize<ApiResourcesModel>();
        var frameworkResult = await ApiResourceService.UpdateApiResourceAsync(apiResourcesModel);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for deleting an API resource by its GUID.
    /// </summary>
    private async Task<bool> DeleteApiResourceById(string jsonContent)
    {
        var apiResourceId = jsonContent.JsonDeserialize<Guid>();
        var frameworkResult = await ApiResourceService.DeleteApiResourceAsync(apiResourceId);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for deleting an API resource by its name.
    /// </summary>
    private async Task<bool> DeleteApiResourceByName(string jsonContent)
    {
        var apiResourceName = jsonContent.JsonDeserialize<string>();
        var frameworkResult = await ApiResourceService.DeleteApiResourceAsync(apiResourceName);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for retrieving an API resource by its name.
    /// </summary>
    private async Task<bool> GetApiResourceByName(string jsonContent)
    {
        var apiResourceName = jsonContent.JsonDeserialize<string>();
        var apiresult = await ApiResourceService.GetApiResourceAsync(apiResourceName);
        await GenerateApiResults(apiresult);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving an API resource by its GUID.
    /// </summary>
    private async Task<bool> GetApiResourceById(string jsonContent)
    {
        var apiResourceId = jsonContent.JsonDeserialize<Guid>();
        var frameworkResult = await ApiResourceService.GetApiResourceAsync(apiResourceId);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving all API resources.
    /// </summary>
    private async Task<bool> GetAllApiResources(string jsonContent)
    {
        var frameworkResult = await ApiResourceService.GetAllApiResourcesAsync();
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for adding a claim to an API resource.
    /// </summary>
    private async Task<bool> AddApiResourceClaim(string jsonContent)
    {
        var apiResourcesClaimModel = jsonContent.JsonDeserialize<ApiResourceClaimsModel>();
        var frameworkResult = await ApiResourceService.AddApiResourceClaimAsync(apiResourcesClaimModel);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for deleting an API resource claim by its GUID.
    /// </summary>
    private async Task<bool> DeleteApiResourceClaimById(string jsonContent)
    {
        var apiResourceClaimId = jsonContent.JsonDeserialize<Guid>();
        var frameworkResult = await ApiResourceService.DeleteApiResourceClaimByIdAsync(apiResourceClaimId);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for deleting all claims of an API resource by the resource's GUID.
    /// </summary>
    private async Task<bool> DeleteApiResourceClaimByResourceIdAsync(string jsonContent)
    {
        var apiResourceId = jsonContent.JsonDeserialize<Guid>();
        var frameworkResult = await ApiResourceService.DeleteApiResourceClaimByResourceIdAsync(apiResourceId);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for deleting an API resource claim by matching a claim model.
    /// </summary>
    private async Task<bool> DeleteApiResourceClaimModel(string jsonContent)
    {
        var apiResourcesClaimModel = jsonContent.JsonDeserialize<ApiResourceClaimsModel>();
        var frameworkResult = await ApiResourceService.DeleteApiResourceClaimAsync(apiResourcesClaimModel);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for retrieving all claims of an API resource by resource GUID.
    /// </summary>
    private async Task<bool> GetApiResourceClaimsById(string jsonContent)
    {
        var apiResourceId = jsonContent.JsonDeserialize<Guid>();
        var frameworkResult = await ApiResourceService.GetApiResourceClaimsAsync(apiResourceId);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for adding a new API scope.
    /// </summary>
    private async Task<bool> AddApiScope(string jsonContent)
    {
        var apiScopeModel = jsonContent.JsonDeserialize<ApiScopesModel>();
        var frameworkResult = await ApiResourceService.AddApiScopeAsync(apiScopeModel);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for updating an existing API scope.
    /// </summary>
    private async Task<bool> UpdateApiScope(string jsonContent)
    {
        var apiScopeModel = jsonContent.JsonDeserialize<ApiScopesModel>();
        var frameworkResult = await ApiResourceService.UpdateApiScopeAsync(apiScopeModel);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for deleting an API scope by its GUID.
    /// </summary>
    private async Task<bool> DeleteApiScopeById(string jsonContent)
    {
        var apiScopeId = jsonContent.JsonDeserialize<Guid>();
        var frameworkResult = await ApiResourceService.DeleteApiScopeAsync(apiScopeId);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for deleting an API scope by its name.
    /// </summary>
    private async Task<bool> DeleteApiScopeByName(string jsonContent)
    {
        var apiScopeName = jsonContent.JsonDeserialize<string>();
        var frameworkResult = await ApiResourceService.DeleteApiScopeAsync(apiScopeName);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for retrieving an API scope by its GUID.
    /// </summary>
    private async Task<bool> GetApiScopeById(string jsonContent)
    {
        var apiScopeId = jsonContent.JsonDeserialize<Guid>();
        var frameworkResult = await ApiResourceService.GetApiScopeAsync(apiScopeId);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving an API scope by its name.
    /// </summary>
    private async Task<bool> GetApiScopeByName(string jsonContent)
    {
        var apiScopeName = jsonContent.JsonDeserialize<string>();
        var frameworkResult = await ApiResourceService.GetApiScopeAsync(apiScopeName);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving all API scopes.
    /// </summary>
    private async Task<bool> GetAllApiScopes(string jsonContent)
    {
        var frameworkResult = await ApiResourceService.GetAllApiScopesAsync();
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for adding a claim to an API scope.
    /// </summary>
    private async Task<bool> AddApiScopeClaim(string jsonContent)
    {
        var apiScopeClaimModel = jsonContent.JsonDeserialize<ApiScopeClaimsModel>();
        var frameworkResult = await ApiResourceService.AddApiScopeClaimAsync(apiScopeClaimModel);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for deleting all claims of an API scope by the scope's GUID.
    /// </summary>
    private async Task<bool> DeleteApiScopeClaimByScopeId(string jsonContent)
    {
        var apiScopeId = jsonContent.JsonDeserialize<Guid>();
        var frameworkResult = await ApiResourceService.DeleteApiScopeClaimByScopeIdAsync(apiScopeId);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for deleting a specific API scope claim by its GUID.
    /// </summary>
    private async Task<bool> DeleteApiScopeClaimById(string jsonContent)
    {
        var apiScopeClaimId = jsonContent.JsonDeserialize<Guid>();
        var frameworkResult = await ApiResourceService.DeleteApiScopeClaimByIdAsync(apiScopeClaimId);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for deleting an API scope claim by matching a claim model.
    /// </summary>
    private async Task<bool> DeleteApiScopeClaimModel(string jsonContent)
    {
        var apiScopeClaimModel = jsonContent.JsonDeserialize<ApiScopeClaimsModel>();
        var frameworkResult = await ApiResourceService.DeleteApiScopeClaimAsync(apiScopeClaimModel);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for retrieving all claims of an API scope by scope GUID.
    /// </summary>
    private async Task<bool> GetApiScopeClaims(string jsonContent)
    {
        var apiScopeId = jsonContent.JsonDeserialize<Guid>();
        var frameworkResult = await ApiResourceService.GetApiScopeClaimsAsync(apiScopeId);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving API resources filtered by a list of scope names.
    /// </summary>
    private async Task<bool> GetAllApiResourcesByScopesAsync(string jsonContent)
    {
        var apiScopeName = jsonContent.JsonDeserialize<IList<string>>();
        var frameworkResult = await ApiResourceService.GetAllApiResourcesByScopesAsync(apiScopeName);
        await GenerateApiResults(frameworkResult);
        return true;
    }
}
