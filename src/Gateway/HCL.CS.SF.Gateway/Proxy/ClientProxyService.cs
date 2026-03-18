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
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.DomainServices.Repository.Api;
using HCL.CS.SF.DomainServices.UnitOfWork.Endpoint;
using HCL.CS.SF.Service.Implementation.Api.Services;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api.Wrapper;

namespace HCL.CS.SF.ProxyService.Proxy;

/// <summary>
/// Gateway proxy for the client management service. Routes requests to the backend
/// <see cref="ClientService"/> after enforcing API-level permission validation.
/// Supports CRUD operations and secret generation for OAuth/OIDC client registrations.
/// </summary>
public sealed class ClientProxyService : ClientService, IClientServices
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
    /// Initializes a new instance of the <see cref="ClientProxyService"/> class.
    /// </summary>
    public ClientProxyService(
        ILoggerInstance instance,
        IMapper mapper,
        IFrameworkResultService frameworkResult,
        IClientsUnitOfWork unitOfWork,
        HCLCSSFConfig securityConfig,
        IUserAccountService userAccountService,
        IApiResourceRepository apiResourceRepository,
        IRepository<ApiScopes> apiScopeRepository,
        IIdentityResourceRepository identityResourceRepository,
        IApiValidator apiValidator)
        : base(
            instance,
            mapper,
            frameworkResult,
            unitOfWork,
            securityConfig,
            apiResourceRepository,
            apiScopeRepository,
            identityResourceRepository)
    {
        this.apiValidator = apiValidator;
        this.frameworkResult = frameworkResult;
    }

    /// <summary>
    /// Deletes a client registration by its client ID after permission validation.
    /// </summary>
    /// <param name="clientId">The unique client identifier.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> DeleteClientAsync(string clientId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.DeleteClientAsync(clientId);
    }

    /// <summary>
    /// Generates a new client secret for the specified client after permission validation.
    /// </summary>
    /// <param name="clientId">The unique client identifier.</param>
    /// <returns>The updated client model containing the new secret.</returns>
    public override async Task<ClientsModel> GenerateClientSecret(string clientId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GenerateClientSecret(clientId);
    }

    /// <summary>
    /// Retrieves all registered clients as a dictionary of client ID to name after permission validation.
    /// </summary>
    /// <returns>A dictionary mapping client IDs to client names.</returns>
    public override async Task<Dictionary<string, string>> GetAllClientAsync()
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetAllClientAsync();
    }

    /// <summary>
    /// Retrieves a single client's details by client ID after permission validation.
    /// </summary>
    /// <param name="clientId">The unique client identifier.</param>
    /// <returns>The client model with full configuration details.</returns>
    public override async Task<ClientsModel> GetClientAsync(string clientId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetClientAsync(clientId);
    }

    /// <summary>
    /// Registers a new OAuth/OIDC client after permission validation.
    /// </summary>
    /// <param name="clientsModel">The client configuration to register.</param>
    /// <returns>The newly registered client model.</returns>
    public override async Task<ClientsModel> RegisterClientAsync(ClientsModel clientsModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.RegisterClientAsync(clientsModel);
    }

    /// <summary>
    /// Updates an existing client registration after permission validation.
    /// </summary>
    /// <param name="clientsModel">The updated client configuration.</param>
    /// <returns>The updated client model.</returns>
    public override async Task<ClientsModel> UpdateClientAsync(ClientsModel clientsModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.UpdateClientAsync(clientsModel);
    }
}
