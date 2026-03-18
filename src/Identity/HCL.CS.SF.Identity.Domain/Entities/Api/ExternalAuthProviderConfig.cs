/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Entities.Api;

/// <summary>
/// Stores configuration for an external (federated) authentication provider such as Google, Azure AD, or a custom OIDC provider.
/// Enables the identity server to delegate authentication to trusted third-party identity providers.
/// </summary>
public class ExternalAuthProviderConfig : BaseEntity
{
    /// <summary>Display name of the external provider (e.g., "Google", "Azure AD").</summary>
    public string ProviderName { get; set; }

    /// <summary>Numeric identifier for the provider protocol type (e.g., OIDC, SAML).</summary>
    public int ProviderType { get; set; }

    /// <summary>Whether this external provider is currently enabled for user authentication.</summary>
    public bool IsEnabled { get; set; }

    /// <summary>JSON-serialized provider configuration (client ID, client secret, authority URL, scopes, etc.).</summary>
    public string ConfigJson { get; set; }

    /// <summary>When true, users authenticating via this provider are automatically provisioned as local accounts.</summary>
    public bool AutoProvisionEnabled { get; set; }

    /// <summary>Comma-separated list of email domains allowed to authenticate through this provider (empty means all domains).</summary>
    public string AllowedDomains { get; set; }

    /// <summary>UTC timestamp of the last connectivity/configuration test against the provider.</summary>
    public DateTime? LastTestedOn { get; set; }

    /// <summary>Result of the last provider connectivity test; null if never tested.</summary>
    public bool? LastTestSuccess { get; set; }
}
