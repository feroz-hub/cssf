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
/// Gateway proxy for the identity resource management service. Routes requests to the backend
/// <see cref="IdentityResourceService"/> after enforcing API-level permission validation.
/// Manages OIDC identity resources (e.g., openid, profile, email) and their associated claims.
/// </summary>
public sealed class IdentityResourceProxyService : IdentityResourceService, IIdentityResourceService
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
    /// Initializes a new instance of the <see cref="IdentityResourceProxyService"/> class.
    /// </summary>
    public IdentityResourceProxyService(
        ILoggerInstance instance,
        IMapper mapper,
        IFrameworkResultService frameworkResult,
        IIdentityResourceRepository identityResourceRepository,
        IRepository<IdentityClaims> identityClaimRepository,
        IApiValidator apiValidator)
        : base(
            instance,
            mapper,
            frameworkResult,
            identityResourceRepository,
            identityClaimRepository)
    {
        this.apiValidator = apiValidator;
        this.frameworkResult = frameworkResult;
    }

    /// <summary>
    /// Adds a new identity resource after permission validation.
    /// </summary>
    /// <param name="identityResourceModel">The identity resource model to create.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> AddIdentityResourceAsync(IdentityResourcesModel identityResourceModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.AddIdentityResourceAsync(identityResourceModel);
    }

    /// <summary>
    /// Updates an existing identity resource after permission validation.
    /// </summary>
    /// <param name="identityResourceModel">The updated identity resource model.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> UpdateIdentityResourceAsync(
        IdentityResourcesModel identityResourceModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.UpdateIdentityResourceAsync(identityResourceModel);
    }

    /// <summary>
    /// Deletes an identity resource by its unique identifier after permission validation.
    /// </summary>
    /// <param name="identityResourceId">The GUID of the identity resource to delete.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> DeleteIdentityResourceAsync(Guid identityResourceId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.DeleteIdentityResourceAsync(identityResourceId);
    }

    /// <summary>
    /// Deletes an identity resource by its name after permission validation.
    /// </summary>
    /// <param name="identityResourceName">The name of the identity resource to delete.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> DeleteIdentityResourceAsync(string identityResourceName)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.DeleteIdentityResourceAsync(identityResourceName);
    }

    /// <summary>
    /// Retrieves an identity resource by its unique identifier after permission validation.
    /// </summary>
    /// <param name="identityResourceId">The GUID of the identity resource.</param>
    /// <returns>The identity resource model.</returns>
    public override async Task<IdentityResourcesModel> GetIdentityResourceAsync(Guid identityResourceId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetIdentityResourceAsync(identityResourceId);
    }

    /// <summary>
    /// Retrieves an identity resource by its name after permission validation.
    /// </summary>
    /// <param name="identityResourceName">The name of the identity resource.</param>
    /// <returns>The identity resource model.</returns>
    public override async Task<IdentityResourcesModel> GetIdentityResourceAsync(string identityResourceName)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetIdentityResourceAsync(identityResourceName);
    }

    /// <summary>
    /// Retrieves all identity resources after permission validation.
    /// </summary>
    /// <returns>A list of all identity resource models.</returns>
    public override async Task<IList<IdentityResourcesModel>> GetAllIdentityResourcesAsync()
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetAllIdentityResourcesAsync();
    }

    /// <summary>
    /// Adds a claim to an identity resource after permission validation.
    /// </summary>
    /// <param name="identityClaimsModel">The identity claim to add.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> AddIdentityResourceClaimAsync(IdentityClaimsModel identityClaimsModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.AddIdentityResourceClaimAsync(identityClaimsModel);
    }

    /// <summary>
    /// Deletes all claims for an identity resource by resource ID after permission validation.
    /// </summary>
    /// <param name="identityResourceId">The GUID of the identity resource whose claims to delete.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> DeleteIdentityResourceClaimByResourceIdAsync(Guid identityResourceId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.DeleteIdentityResourceClaimByResourceIdAsync(identityResourceId);
    }

    /// <summary>
    /// Deletes a specific identity resource claim by its unique identifier after permission validation.
    /// </summary>
    /// <param name="identityResourceClaimId">The GUID of the claim to delete.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> DeleteIdentityResourceClaimByIdAsync(Guid identityResourceClaimId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.DeleteIdentityResourceClaimByIdAsync(identityResourceClaimId);
    }

    /// <summary>
    /// Deletes an identity resource claim matching the given model after permission validation.
    /// </summary>
    /// <param name="identityClaimsModel">The claim model to match and delete.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> DeleteIdentityResourceClaimAsync(
        IdentityClaimsModel identityClaimsModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.DeleteIdentityResourceClaimAsync(identityClaimsModel);
    }

    /// <summary>
    /// Retrieves all claims for the specified identity resource after permission validation.
    /// </summary>
    /// <param name="identityResourceId">The GUID of the identity resource.</param>
    /// <returns>A list of identity claim models.</returns>
    public override async Task<IList<IdentityClaimsModel>> GetIdentityResourceClaimsAsync(Guid identityResourceId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetIdentityResourceClaimsAsync(identityResourceId);
    }
}
