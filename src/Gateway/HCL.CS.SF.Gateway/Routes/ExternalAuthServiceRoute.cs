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
/// Partial class of <see cref="ApiGateway"/> containing route handlers for external
/// authentication provider management. Handles routes for listing, retrieving, saving,
/// deleting, and testing external identity provider configurations (e.g., Google, Microsoft).
/// </summary>
internal partial class ApiGateway : BaseApiServiceInstance, IApiGateway
{
    /// <summary>
    /// Handles the route for retrieving all external authentication provider configurations.
    /// </summary>
    private async Task<bool> GetAllExternalAuthProviders(string jsonContent)
    {
        var result = await ExternalAuthManagementService.GetAllProvidersAsync();
        await GenerateApiResults(result);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving a specific external authentication provider by ID.
    /// </summary>
    private async Task<bool> GetExternalAuthProvider(string jsonContent)
    {
        var request = jsonContent.JsonDeserialize<DeleteExternalAuthProviderRequest>();
        var result = await ExternalAuthManagementService.GetProviderAsync(request.Id);
        await GenerateApiResults(result);
        return true;
    }

    /// <summary>
    /// Handles the route for creating or updating an external authentication provider configuration.
    /// </summary>
    private async Task<bool> SaveExternalAuthProvider(string jsonContent)
    {
        var request = jsonContent.JsonDeserialize<SaveExternalAuthProviderRequest>();
        var frameworkResult = await ExternalAuthManagementService.SaveProviderAsync(request);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for deleting an external authentication provider configuration.
    /// </summary>
    private async Task<bool> DeleteExternalAuthProvider(string jsonContent)
    {
        var request = jsonContent.JsonDeserialize<DeleteExternalAuthProviderRequest>();
        var frameworkResult = await ExternalAuthManagementService.DeleteProviderAsync(request);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for testing connectivity to an external authentication provider.
    /// </summary>
    private async Task<bool> TestExternalAuthProvider(string jsonContent)
    {
        var request = jsonContent.JsonDeserialize<TestExternalAuthProviderRequest>();
        var frameworkResult = await ExternalAuthManagementService.TestProviderAsync(request);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for retrieving the field definitions schema for external auth provider forms.
    /// </summary>
    private async Task<bool> GetExternalAuthFieldDefinitions(string jsonContent)
    {
        var result = await ExternalAuthManagementService.GetFieldDefinitionsAsync();
        await GenerateApiResults(result);
        return true;
    }
}
