/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Models.Endpoint;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Api;

/// <summary>
/// Service for managing OAuth 2.0 / OIDC client registrations. Provides client CRUD,
/// client secret generation, and look-up operations used by the admin API.
/// </summary>
public interface IClientServices
{
    /// <summary>Registers a new OAuth/OIDC client and returns the created client model (including secrets).</summary>
    /// <param name="clientsModel">The client configuration to register.</param>
    Task<ClientsModel> RegisterClientAsync(ClientsModel clientsModel);

    /// <summary>Updates an existing client's configuration.</summary>
    /// <param name="clientsModel">The client model with updated fields.</param>
    Task<ClientsModel> UpdateClientAsync(ClientsModel clientsModel);

    /// <summary>Deletes a client registration by its client identifier.</summary>
    /// <param name="clientId">The client identifier to delete.</param>
    Task<FrameworkResult> DeleteClientAsync(string clientId);

    /// <summary>Generates a new client secret for the specified client, replacing the existing one.</summary>
    /// <param name="clientId">The client identifier to generate a secret for.</param>
    Task<ClientsModel> GenerateClientSecret(string clientId);

    /// <summary>Retrieves a client configuration by its client identifier.</summary>
    /// <param name="clientId">The client identifier to look up.</param>
    Task<ClientsModel> GetClientAsync(string clientId);

    /// <summary>Retrieves all registered clients as a dictionary of client ID to client name.</summary>
    Task<Dictionary<string, string>> GetAllClientAsync();
}
