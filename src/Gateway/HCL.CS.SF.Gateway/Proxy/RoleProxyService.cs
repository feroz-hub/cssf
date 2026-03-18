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
using HCL.CS.SF.DomainServices.UnitOfWork.Api;
using HCL.CS.SF.DomainServices.Wrappers;
using HCL.CS.SF.Service.Implementation.Api.Services;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api.Wrapper;

namespace HCL.CS.SF.ProxyService.Proxy;

/// <summary>
/// Gateway proxy for the role management service. Routes requests to the backend
/// <see cref="RoleService"/> after enforcing API-level permission validation.
/// Supports CRUD operations for roles and their associated claims.
/// </summary>
public sealed class RoleProxyService : RoleService, IRoleService
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
    /// Initializes a new instance of the <see cref="RoleProxyService"/> class.
    /// </summary>
    public RoleProxyService(
        RoleManagerWrapper<Roles> roleManager,
        ILoggerInstance logger,
        IUserRepository userRepository,
        IMapper mapper,
        IRoleManagementUnitOfWork roleUnitOfWork,
        IFrameworkResultService frameworkResultService,
        IApiResourceRepository apiResourceRepository,
        IRepository<ApiScopes> apiScopeRepository,
        IIdentityResourceRepository identityResourceRepository,
        IAuditTrailService auditTrailService,
        IApiValidator apiValidator)
        : base(
            roleManager,
            logger,
            userRepository,
            mapper,
            roleUnitOfWork,
            frameworkResultService,
            apiResourceRepository,
            apiScopeRepository,
            identityResourceRepository,
            auditTrailService)
    {
        this.apiValidator = apiValidator;
        frameworkResult = frameworkResultService;
    }

    /// <summary>
    /// Creates a new role after permission validation.
    /// </summary>
    /// <param name="roleModel">The role definition to create.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> CreateRoleAsync(RoleModel roleModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.CreateRoleAsync(roleModel);
    }

    /// <summary>
    /// Updates an existing role after permission validation.
    /// </summary>
    /// <param name="roleModel">The updated role definition.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> UpdateRoleAsync(RoleModel roleModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.UpdateRoleAsync(roleModel);
    }

    /// <summary>
    /// Deletes a role by its unique identifier after permission validation.
    /// </summary>
    /// <param name="roleId">The GUID of the role to delete.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> DeleteRoleAsync(Guid roleId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.DeleteRoleAsync(roleId);
    }

    /// <summary>
    /// Deletes a role by its name after permission validation.
    /// </summary>
    /// <param name="roleName">The name of the role to delete.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> DeleteRoleAsync(string roleName)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.DeleteRoleAsync(roleName);
    }

    /// <summary>
    /// Retrieves a role by its unique identifier after permission validation.
    /// </summary>
    /// <param name="roleId">The GUID of the role.</param>
    /// <returns>The role model.</returns>
    public override async Task<RoleModel> GetRoleAsync(Guid roleId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetRoleAsync(roleId);
    }

    /// <summary>
    /// Retrieves a role by its name after permission validation.
    /// </summary>
    /// <param name="roleName">The name of the role.</param>
    /// <returns>The role model.</returns>
    public override async Task<RoleModel> GetRoleAsync(string roleName)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetRoleAsync(roleName);
    }

    /// <summary>
    /// Retrieves all roles after permission validation.
    /// </summary>
    /// <returns>A list of all role models.</returns>
    public override async Task<IList<RoleModel>> GetAllRolesAsync()
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetAllRolesAsync();
    }

    /// <summary>
    /// Adds a single claim to a role after permission validation.
    /// </summary>
    /// <param name="roleClaimModel">The role claim to add.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> AddRoleClaimAsync(RoleClaimModel roleClaimModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.AddRoleClaimAsync(roleClaimModel);
    }

    /// <summary>
    /// Adds multiple claims to a role after permission validation.
    /// </summary>
    /// <param name="roleClaimsModel">The list of role claims to add.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> AddRoleClaimsAsync(IList<RoleClaimModel> roleClaimsModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.AddRoleClaimsAsync(roleClaimsModel);
    }

    /// <summary>
    /// Removes a role claim by its numeric identifier after permission validation.
    /// </summary>
    /// <param name="roleClaimId">The ID of the role claim to remove.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> RemoveRoleClaimAsync(int roleClaimId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.RemoveRoleClaimAsync(roleClaimId);
    }

    /// <summary>
    /// Removes a role claim matching the given model after permission validation.
    /// </summary>
    /// <param name="roleClaimModel">The role claim to match and remove.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> RemoveRoleClaimAsync(RoleClaimModel roleClaimModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.RemoveRoleClaimAsync(roleClaimModel);
    }

    /// <summary>
    /// Removes multiple role claims after permission validation.
    /// </summary>
    /// <param name="roleClaimsModel">The list of role claims to remove.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> RemoveRoleClaimsAsync(IList<RoleClaimModel> roleClaimsModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.RemoveRoleClaimsAsync(roleClaimsModel);
    }

    /// <summary>
    /// Retrieves all claims associated with the specified role after permission validation.
    /// </summary>
    /// <param name="roleModel">The role whose claims to retrieve.</param>
    /// <returns>A list of role claim models.</returns>
    public override async Task<IList<RoleClaimModel>> GetRoleClaimAsync(RoleModel roleModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetRoleClaimAsync(roleModel);
    }
}
