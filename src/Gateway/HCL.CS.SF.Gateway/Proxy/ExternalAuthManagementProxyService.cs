/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.Service.Implementation.Api.Services;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api.Wrapper;

namespace HCL.CS.SF.ProxyService.Proxy;

/// <summary>
/// Gateway proxy for the external authentication provider management service.
/// Routes requests to the backend <see cref="ExternalAuthManagementService"/> after
/// enforcing API-level permission validation. Manages external identity provider
/// configurations (e.g., Google, Microsoft) including CRUD operations and connectivity testing.
/// </summary>
public sealed class ExternalAuthManagementProxyService : ExternalAuthManagementService, IExternalAuthManagementService
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
    /// Initializes a new instance of the <see cref="ExternalAuthManagementProxyService"/> class.
    /// </summary>
    public ExternalAuthManagementProxyService(
        IRepository<ExternalAuthProviderConfig> providerConfigRepository,
        IFrameworkResultService frameworkResult,
        ILoggerInstance loggerInstance,
        IApiValidator apiValidator)
        : base(
            providerConfigRepository,
            frameworkResult,
            loggerInstance)
    {
        this.apiValidator = apiValidator;
        this.frameworkResult = frameworkResult;
    }

    /// <summary>
    /// Retrieves all configured external authentication providers after permission validation.
    /// </summary>
    /// <returns>A list of all external auth provider configurations.</returns>
    public override async Task<List<ExternalAuthProviderConfigModel>> GetAllProvidersAsync()
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetAllProvidersAsync();
    }

    /// <summary>
    /// Retrieves a specific external authentication provider by its identifier after permission validation.
    /// </summary>
    /// <param name="id">The unique identifier of the provider configuration.</param>
    /// <returns>The external auth provider configuration model.</returns>
    public override async Task<ExternalAuthProviderConfigModel> GetProviderAsync(Guid id)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetProviderAsync(id);
    }

    /// <summary>
    /// Creates or updates an external authentication provider configuration after permission validation.
    /// </summary>
    /// <param name="request">The provider configuration to save.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> SaveProviderAsync(SaveExternalAuthProviderRequest request)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.SaveProviderAsync(request);
    }

    /// <summary>
    /// Deletes an external authentication provider configuration after permission validation.
    /// </summary>
    /// <param name="request">The request identifying the provider to delete.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> DeleteProviderAsync(DeleteExternalAuthProviderRequest request)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.DeleteProviderAsync(request);
    }

    /// <summary>
    /// Tests connectivity to an external authentication provider after permission validation.
    /// </summary>
    /// <param name="request">The request containing provider details to test.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating whether the provider is reachable.</returns>
    public override async Task<FrameworkResult> TestProviderAsync(TestExternalAuthProviderRequest request)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.TestProviderAsync(request);
    }

    /// <summary>
    /// Retrieves the field definitions schema for external auth provider configuration
    /// forms after permission validation.
    /// </summary>
    /// <returns>The field definitions response describing required and optional configuration fields.</returns>
    public override async Task<ExternalAuthFieldDefinitionsResponse> GetFieldDefinitionsAsync()
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetFieldDefinitionsAsync();
    }
}
