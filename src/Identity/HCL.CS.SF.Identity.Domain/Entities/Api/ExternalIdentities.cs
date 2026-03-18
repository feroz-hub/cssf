/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Entities.Api;

/// <summary>
/// Links a local user account to an external (federated) identity from a third-party provider.
/// Stores the external provider's subject identifier so the system can match future
/// federated sign-ins to an existing local account without re-provisioning.
/// </summary>
public class ExternalIdentities : BaseEntity
{
    /// <summary>Foreign key referencing the local user account this external identity is linked to.</summary>
    public Guid UserId { get; set; }

    /// <summary>The external provider's tenant or directory identifier (relevant for multi-tenant providers like Azure AD).</summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>Name of the external identity provider (e.g., "Google", "AzureAD").</summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>The issuer URI from the external provider's ID token (e.g., "https://accounts.google.com").</summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>The external provider's unique subject identifier for this user (the "sub" claim from the external ID token).</summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>Email address reported by the external provider.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Whether the external provider has verified the user's email address.</summary>
    public bool EmailVerified { get; set; }

    /// <summary>UTC timestamp when this external identity was first linked to the local account.</summary>
    public DateTime LinkedAt { get; set; }

    /// <summary>UTC timestamp of the most recent sign-in via this external provider.</summary>
    public DateTime? LastSignInAt { get; set; }
}
