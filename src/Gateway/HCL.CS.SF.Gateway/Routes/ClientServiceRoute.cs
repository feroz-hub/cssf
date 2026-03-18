/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.ProxyService.Routes.Extension;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api.Wrapper;

namespace HCL.CS.SF.ProxyService.Routes;

/// <summary>
/// Partial class of <see cref="ApiGateway"/> containing route handlers for OAuth/OIDC
/// client management operations including registration, retrieval, update, deletion,
/// and secret generation.
/// </summary>
internal partial class ApiGateway : BaseApiServiceInstance, IApiGateway
{
    /// <summary>
    /// Handles the route for deleting a client by its client ID.
    /// </summary>
    private async Task<bool> DeleteClient(string jsonContent)
    {
        var clientId = jsonContent.JsonDeserialize<string>();
        var frameworkResult = await ClientServices.DeleteClientAsync(clientId);
        return await GenerateApiResults(frameworkResult);
    }

    /// <summary>
    /// Handles the route for generating a new client secret for the specified client.
    /// </summary>
    private async Task<bool> GenerateClientSecret(string jsonContent)
    {
        var clientId = jsonContent.JsonDeserialize<string>();
        var frameworkResult = await ClientServices.GenerateClientSecret(clientId);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving all registered clients.
    /// </summary>
    private async Task<bool> GetAllClient(string jsonContent)
    {
        var frameworkResult = await ClientServices.GetAllClientAsync();
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving a single client by its client ID.
    /// </summary>
    private async Task<bool> GetClient(string jsonContent)
    {
        var clientId = jsonContent.JsonDeserialize<string>();
        var frameworkResult = await ClientServices.GetClientAsync(clientId);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for registering a new OAuth/OIDC client.
    /// </summary>
    private async Task<bool> RegisterClient(string jsonContent)
    {
        var clientModel = jsonContent.JsonDeserialize<ClientsModel>();
        var frameworkResult = await ClientServices.RegisterClientAsync(clientModel);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for updating an existing client registration.
    /// </summary>
    private async Task<bool> UpdateClient(string jsonContent)
    {
        var clientModel = jsonContent.JsonDeserialize<ClientsModel>();
        var frameworkResult = await ClientServices.UpdateClientAsync(clientModel);
        await GenerateApiResults(frameworkResult);
        return true;
    }
}
