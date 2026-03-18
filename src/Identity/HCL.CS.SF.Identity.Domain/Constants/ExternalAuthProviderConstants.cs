/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Constants;

/// <summary>
/// Constants and field definitions for supported external authentication (social login) providers.
/// Each provider has a unique identifier, a set of configuration fields for the admin UI,
/// and default values for well-known settings (e.g., Google's OIDC discovery endpoint).
/// </summary>
public static class ExternalAuthProviderConstants
{
    /// <summary>
    /// Google OIDC external authentication provider identifier.
    /// Used for "Sign in with Google" federated login.
    /// </summary>
    public const string Google = "Google";

    /// <summary>
    /// Maps each external auth provider identifier to its required and optional configuration fields.
    /// Used by the admin UI to dynamically render provider-specific configuration forms.
    /// </summary>
    public static readonly Dictionary<string, ProviderFieldDefinition[]> ProviderFields = new()
    {
        [Google] = new[]
        {
            new ProviderFieldDefinition("ClientId", "Client ID", "text", true),
            new ProviderFieldDefinition("ClientSecret", "Client Secret", "password", true),
            new ProviderFieldDefinition("Authority", "Authority", "text", false),
            new ProviderFieldDefinition("MetadataAddress", "Metadata Address", "text", false),
            new ProviderFieldDefinition("CallbackPath", "Callback Path", "text", false),
            new ProviderFieldDefinition("AllowedRedirectHosts", "Allowed Redirect Hosts", "textarea", false)
        }
    };

    /// <summary>
    /// Maps each external auth provider identifier to its default configuration values.
    /// These defaults are pre-populated in the admin UI when configuring a new provider instance.
    /// </summary>
    public static readonly Dictionary<string, Dictionary<string, string>> ProviderDefaults = new()
    {
        [Google] = new Dictionary<string, string>
        {
            { "Authority", "https://accounts.google.com" },
            { "MetadataAddress", "https://accounts.google.com/.well-known/openid-configuration" },
            { "CallbackPath", "/auth/external/google/signin-callback" }
        }
    };
}
