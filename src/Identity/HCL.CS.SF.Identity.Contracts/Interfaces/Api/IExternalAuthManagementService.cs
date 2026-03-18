/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Models.Api;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Api;

/// <summary>
/// Administrative service for managing external authentication provider configurations
/// (e.g., Google, Microsoft, SAML). Provides CRUD operations, connectivity testing,
/// and field-definition retrieval for the admin UI.
/// </summary>
public interface IExternalAuthManagementService
{
    /// <summary>Retrieves all registered external authentication provider configurations.</summary>
    Task<List<ExternalAuthProviderConfigModel>> GetAllProvidersAsync();

    /// <summary>Retrieves a specific external authentication provider configuration by its identifier.</summary>
    /// <param name="id">The unique identifier of the provider configuration.</param>
    Task<ExternalAuthProviderConfigModel> GetProviderAsync(Guid id);

    /// <summary>Creates or updates an external authentication provider configuration.</summary>
    /// <param name="request">The provider configuration to save.</param>
    Task<FrameworkResult> SaveProviderAsync(SaveExternalAuthProviderRequest request);

    /// <summary>Deletes an external authentication provider configuration.</summary>
    /// <param name="request">The request identifying the provider to delete.</param>
    Task<FrameworkResult> DeleteProviderAsync(DeleteExternalAuthProviderRequest request);

    /// <summary>Tests connectivity to an external authentication provider.</summary>
    /// <param name="request">The test request containing the provider configuration to validate.</param>
    Task<FrameworkResult> TestProviderAsync(TestExternalAuthProviderRequest request);

    /// <summary>Retrieves the field definitions required to configure each supported external auth provider type.</summary>
    Task<ExternalAuthFieldDefinitionsResponse> GetFieldDefinitionsAsync();
}
