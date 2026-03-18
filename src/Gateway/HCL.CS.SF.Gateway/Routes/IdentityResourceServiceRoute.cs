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
/// Partial class of <see cref="ApiGateway"/> containing route handlers for OIDC identity
/// resource and identity claim management operations.
/// </summary>
internal partial class ApiGateway : BaseApiServiceInstance, IApiGateway
{
    /// <summary>
    /// Handles the route for adding a new identity resource.
    /// </summary>
    private async Task<bool> AddIdentityResource(string jsonContent)
    {
        var identityResourcesModel = jsonContent.JsonDeserialize<IdentityResourcesModel>();
        var frameworkResult = await IdentityResourceService.AddIdentityResourceAsync(identityResourcesModel);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for updating an existing identity resource.
    /// </summary>
    private async Task<bool> UpdateIdentityResource(string jsonContent)
    {
        var identityResourcesModel = jsonContent.JsonDeserialize<IdentityResourcesModel>();
        var frameworkResult = await IdentityResourceService.UpdateIdentityResourceAsync(identityResourcesModel);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for deleting an identity resource by its GUID.
    /// </summary>
    private async Task<bool> DeleteIdentityResourceById(string jsonContent)
    {
        var identityResourceId = jsonContent.JsonDeserialize<Guid>();
        var frameworkResult = await IdentityResourceService.DeleteIdentityResourceAsync(identityResourceId);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for deleting an identity resource by its name.
    /// </summary>
    private async Task<bool> DeleteIdentityResourceByName(string jsonContent)
    {
        var identityResourceName = jsonContent.JsonDeserialize<string>();
        var frameworkResult = await IdentityResourceService.DeleteIdentityResourceAsync(identityResourceName);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for retrieving an identity resource by its GUID.
    /// </summary>
    private async Task<bool> GetIdentityResourceById(string jsonContent)
    {
        var identityResourceId = jsonContent.JsonDeserialize<Guid>();
        var frameworkResult = await IdentityResourceService.GetIdentityResourceAsync(identityResourceId);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving an identity resource by its name.
    /// </summary>
    private async Task<bool> GetIdentityResourceByName(string jsonContent)
    {
        var identityResourceName = jsonContent.JsonDeserialize<string>();
        var frameworkResult = await IdentityResourceService.GetIdentityResourceAsync(identityResourceName);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving all identity resources.
    /// </summary>
    private async Task<bool> GetAllIdentityResources(string jsonContent)
    {
        var frameworkResult = await IdentityResourceService.GetAllIdentityResourcesAsync();
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for adding a claim to an identity resource.
    /// </summary>
    private async Task<bool> AddIdentityResourceClaim(string jsonContent)
    {
        var identityResourcesClaimModel = jsonContent.JsonDeserialize<IdentityClaimsModel>();
        var frameworkResult = await IdentityResourceService.AddIdentityResourceClaimAsync(identityResourcesClaimModel);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for deleting all claims of an identity resource by the resource's GUID.
    /// </summary>
    private async Task<bool> DeleteIdentityResourceClaimByResourceIdAsync(string jsonContent)
    {
        var identityResourceId = jsonContent.JsonDeserialize<Guid>();
        var frameworkResult =
            await IdentityResourceService.DeleteIdentityResourceClaimByResourceIdAsync(identityResourceId);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for deleting a specific identity resource claim by its GUID.
    /// </summary>
    private async Task<bool> DeleteIdentityResourceClaimByIdAsync(string jsonContent)
    {
        var identityResourceClaimId = jsonContent.JsonDeserialize<Guid>();
        var frameworkResult =
            await IdentityResourceService.DeleteIdentityResourceClaimByIdAsync(identityResourceClaimId);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for deleting an identity resource claim by matching a claim model.
    /// </summary>
    private async Task<bool> DeleteIdentityResourceClaimModel(string jsonContent)
    {
        var identityResourceClaimModel = jsonContent.JsonDeserialize<IdentityClaimsModel>();
        var frameworkResult =
            await IdentityResourceService.DeleteIdentityResourceClaimAsync(identityResourceClaimModel);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for retrieving all claims of an identity resource by resource GUID.
    /// </summary>
    private async Task<bool> GetIdentityResourceClaims(string jsonContent)
    {
        var identityClaimsId = jsonContent.JsonDeserialize<Guid>();
        var frameworkResult = await IdentityResourceService.GetIdentityResourceClaimsAsync(identityClaimsId);
        await GenerateApiResults(frameworkResult);
        return true;
    }
}
