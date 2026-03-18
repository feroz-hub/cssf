/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCLCSSFInstallerMVC.ViewModels;

/// <summary>
/// View model for wizard step 5 (Seed Data).
/// Collects the initial OAuth client configuration and administrator user details,
/// and displays the generated credentials after successful seeding.
/// </summary>
public sealed class SeedStepViewModel
{
    // --- Client configuration ---

    /// <summary>Display name for the initial OAuth client.</summary>
    public string ClientName { get; set; } = string.Empty;

    /// <summary>Base URI of the client application (must be HTTPS).</summary>
    public string ClientUri { get; set; } = string.Empty;

    /// <summary>Enable the authorization_code grant type.</summary>
    public bool UseAuthorizationCodeGrant { get; set; }

    /// <summary>Enable the client_credentials grant type.</summary>
    public bool UseClientCredentialsGrant { get; set; }

    /// <summary>Enable the hybrid grant type (not allowed by validation).</summary>
    public bool UseHybridGrant { get; set; }

    /// <summary>Enable the refresh_token grant type.</summary>
    public bool UseRefreshTokenGrant { get; set; }

    /// <summary>Enable the resource owner password grant type.</summary>
    public bool UsePasswordGrant { get; set; }

    /// <summary>Enable the "code" response type.</summary>
    public bool UseCodeResponseType { get; set; }

    /// <summary>Enable the "id_token" response type (not allowed by validation).</summary>
    public bool UseIdTokenResponseType { get; set; }

    /// <summary>Enable the "token" response type (not allowed by validation).</summary>
    public bool UseTokenResponseType { get; set; }

    /// <summary>When true, the client receives the full set of default scopes.</summary>
    public bool UseDefaultScopes { get; set; } = true;

    /// <summary>Space-delimited custom scopes when default scopes are disabled.</summary>
    public string AllowedScopes { get; set; } = string.Empty;

    /// <summary>Newline-separated redirect URIs for the client.</summary>
    public string RedirectUris { get; set; } = string.Empty;

    /// <summary>Newline-separated post-logout redirect URIs for the client.</summary>
    public string PostLogoutRedirectUris { get; set; } = string.Empty;

    /// <summary>Optional front-channel logout URI.</summary>
    public string? FrontChannelLogoutUri { get; set; }

    /// <summary>Optional back-channel logout URI.</summary>
    public string? BackChannelLogoutUri { get; set; }

    // --- Admin user configuration ---

    /// <summary>Login username for the admin account.</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>Password for the admin account.</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>Password confirmation (must match Password).</summary>
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>Admin user's first name.</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Admin user's optional last name.</summary>
    public string? LastName { get; set; }

    /// <summary>Admin user's email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Admin user's phone number.</summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>Identity provider type ("Local" or "Ldap").</summary>
    public string IdentityProvider { get; set; } = "Local";

    // --- Seed result ---

    /// <summary>Error message if seeding failed.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Whether the seed completed successfully.</summary>
    public bool IsCompleted { get; set; }

    /// <summary>The auto-generated OAuth client ID shown after successful seeding.</summary>
    public string? GeneratedClientId { get; set; }

    /// <summary>The auto-generated OAuth client secret shown after successful seeding.</summary>
    public string? GeneratedClientSecret { get; set; }
}
