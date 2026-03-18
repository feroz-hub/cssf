/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCLCSSFInstallerMVC.Application.DTOs;

/// <summary>
/// Carries the OAuth client configuration entered during the seed step of the installer wizard.
/// Used to create the first registered client in the security framework database.
/// </summary>
public sealed class ClientConfigurationDto
{
    /// <summary>Human-readable display name for the OAuth client.</summary>
    public string ClientName { get; init; } = string.Empty;

    /// <summary>Base URI of the client application.</summary>
    public string ClientUri { get; init; } = string.Empty;

    /// <summary>OAuth grant types the client is allowed to use (e.g., authorization_code, client_credentials).</summary>
    public IReadOnlyCollection<string> GrantTypes { get; init; } = Array.Empty<string>();

    /// <summary>OAuth response types the client supports (e.g., code).</summary>
    public IReadOnlyCollection<string> ResponseTypes { get; init; } = Array.Empty<string>();

    /// <summary>When <c>true</c>, the client receives the full set of default scopes.</summary>
    public bool UseDefaultScopes { get; init; }

    /// <summary>Space-delimited scopes the client is allowed to request when <see cref="UseDefaultScopes"/> is false.</summary>
    public string AllowedScopes { get; init; } = string.Empty;

    /// <summary>URIs the authorization server may redirect to after authentication.</summary>
    public IReadOnlyCollection<string> RedirectUris { get; init; } = Array.Empty<string>();

    /// <summary>URIs the authorization server may redirect to after logout.</summary>
    public IReadOnlyCollection<string> PostLogoutRedirectUris { get; init; } = Array.Empty<string>();

    /// <summary>Optional front-channel logout URI for session management.</summary>
    public string? FrontChannelLogoutUri { get; init; }

    /// <summary>Optional back-channel logout URI for server-side session termination.</summary>
    public string? BackChannelLogoutUri { get; init; }
}
