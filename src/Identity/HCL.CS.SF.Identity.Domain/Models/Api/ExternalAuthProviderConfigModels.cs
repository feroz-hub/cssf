/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Constants;

namespace HCL.CS.SF.Domain.Models.Api;

/// <summary>
/// Represents the configuration for an external authentication provider (e.g., Google, Azure AD).
/// Used to enable federated identity login via OIDC or OAuth 2.0 external identity providers.
/// </summary>
public class ExternalAuthProviderConfigModel
{
    /// <summary>The unique identifier of the provider configuration. Null for new configurations.</summary>
    public Guid? Id { get; set; }

    /// <summary>The display name of the external provider (e.g., "Google", "Azure AD").</summary>
    public string ProviderName { get; set; }

    /// <summary>The type of external provider (e.g., OIDC, OAuth2, SAML).</summary>
    public int ProviderType { get; set; }

    /// <summary>Indicates whether this external provider is currently enabled for login.</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Provider-specific settings (e.g., client ID, client secret, authority URL).</summary>
    public Dictionary<string, string> Settings { get; set; }

    /// <summary>Indicates whether users authenticating via this provider are automatically provisioned in the local user store.</summary>
    public bool AutoProvisionEnabled { get; set; }

    /// <summary>Comma-separated list of email domains allowed to authenticate via this provider.</summary>
    public string AllowedDomains { get; set; }

    /// <summary>The UTC timestamp when this provider's connectivity was last tested.</summary>
    public DateTime? LastTestedOn { get; set; }

    /// <summary>Indicates whether the last connectivity test succeeded.</summary>
    public bool? LastTestSuccess { get; set; }
}

/// <summary>
/// Request model for creating or updating an external authentication provider configuration.
/// </summary>
public class SaveExternalAuthProviderRequest
{
    /// <summary>The unique identifier. Null for new configurations, set for updates.</summary>
    public Guid? Id { get; set; }

    /// <summary>The display name of the external provider.</summary>
    public string ProviderName { get; set; }

    /// <summary>The type of external provider.</summary>
    public int ProviderType { get; set; }

    /// <summary>Whether to enable this provider for login.</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Provider-specific settings.</summary>
    public Dictionary<string, string> Settings { get; set; }

    /// <summary>Whether to auto-provision users from this provider.</summary>
    public bool AutoProvisionEnabled { get; set; }

    /// <summary>Comma-separated list of allowed email domains.</summary>
    public string AllowedDomains { get; set; }
}

/// <summary>
/// Request model for deleting an external authentication provider configuration.
/// </summary>
public class DeleteExternalAuthProviderRequest
{
    /// <summary>The unique identifier of the provider configuration to delete.</summary>
    public Guid Id { get; set; }
}

/// <summary>
/// Request model for testing an external authentication provider's connectivity.
/// </summary>
public class TestExternalAuthProviderRequest
{
    /// <summary>The unique identifier of the provider configuration to test.</summary>
    public Guid Id { get; set; }
}

/// <summary>
/// Response model returning the available external auth provider types, their required
/// configuration field definitions, and default values for the admin UI.
/// </summary>
public class ExternalAuthFieldDefinitionsResponse
{
    /// <summary>Provider types mapped to their required configuration field definitions.</summary>
    public Dictionary<string, ProviderFieldDefinition[]> Providers { get; set; }

    /// <summary>Provider types mapped to default setting values for new configurations.</summary>
    public Dictionary<string, Dictionary<string, string>> Defaults { get; set; }
}
