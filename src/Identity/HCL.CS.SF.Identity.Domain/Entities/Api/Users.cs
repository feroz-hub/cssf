/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Identity;
using HCL.CS.SF.Domain.Enums;

namespace HCL.CS.SF.Domain.Entities.Api;

/// <summary>
/// Represents an identity user (resource owner) in the OAuth2/OIDC security framework.
/// Extends ASP.NET Identity's <see cref="IdentityUser{TKey}"/> with profile fields,
/// multi-factor authentication preferences, session tracking, and audit metadata.
/// This is the central subject entity referenced by tokens, claims, and role assignments.
/// </summary>
public class Users : IdentityUser<Guid>
{
    /// <summary>User's first (given) name, mapped to the OIDC "given_name" claim.</summary>
    public virtual string FirstName { get; set; }

    /// <summary>User's last (family) name, mapped to the OIDC "family_name" claim.</summary>
    public virtual string LastName { get; set; }

    /// <summary>User's date of birth, mapped to the OIDC "birthdate" claim.</summary>
    public virtual DateTime? DateOfBirth { get; set; }

    /// <summary>The two-factor authentication method configured for this user (e.g., Email, SMS, Authenticator App).</summary>
    public virtual TwoFactorType TwoFactorType { get; set; }

    /// <summary>UTC timestamp of the user's most recent password change; used to enforce password-age policies.</summary>
    public virtual DateTime? LastPasswordChangedDate { get; set; }

    /// <summary>When true, the user must change the system-assigned default password on next login.</summary>
    public virtual bool? RequiresDefaultPasswordChange { get; set; }

    /// <summary>UTC timestamp of the user's most recent successful login.</summary>
    public virtual DateTime? LastLoginDateTime { get; set; }

    /// <summary>UTC timestamp of the user's most recent logout.</summary>
    public virtual DateTime? LastLogoutDateTime { get; set; }

    /// <summary>Indicates how the user was originally authenticated/provisioned (e.g., Local, Google, Azure AD).</summary>
    public virtual IdentityProvider IdentityProviderType { get; set; }

    /// <summary>Soft-delete flag; when true the user account is logically deactivated.</summary>
    public virtual bool IsDeleted { get; set; }

    /// <summary>UTC timestamp when the user account was created.</summary>
    public virtual DateTime CreatedOn { get; set; }

    /// <summary>UTC timestamp of the last profile modification, or null if never modified.</summary>
    public virtual DateTime? ModifiedOn { get; set; }

    /// <summary>Identifier of the user or system that created this account.</summary>
    public virtual string CreatedBy { get; set; }

    /// <summary>Identifier of the user or system that last modified this account.</summary>
    public virtual string ModifiedBy { get; set; }
}
