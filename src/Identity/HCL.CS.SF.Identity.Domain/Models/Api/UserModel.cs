/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Enums;

namespace HCL.CS.SF.Domain.Models.Api;

/// <summary>
/// Represents a user (resource owner) in the identity system.
/// Contains profile information, authentication settings, two-factor configuration,
/// account lockout state, and associated claims and security questions.
/// </summary>
public class UserModel : BaseModel
{
    /// <summary>The unique username used for authentication.</summary>
    public virtual string UserName { get; set; }

    /// <summary>The user's password (used during registration/update; not returned in queries).</summary>
    public virtual string Password { get; set; }

    /// <summary>The user's first (given) name.</summary>
    public virtual string FirstName { get; set; }

    /// <summary>The user's last (family) name.</summary>
    public virtual string LastName { get; set; }

    /// <summary>The user's date of birth.</summary>
    public virtual DateTime? DateOfBirth { get; set; }

    /// <summary>The user's email address, used for communication and as an alternate login identifier.</summary>
    public virtual string Email { get; set; }

    /// <summary>Indicates whether the user's email address has been confirmed via a verification link or code.</summary>
    public virtual bool EmailConfirmed { get; set; } = false;

    /// <summary>The user's phone number, used for SMS-based two-factor authentication.</summary>
    public virtual string PhoneNumber { get; set; }

    /// <summary>Indicates whether the user's phone number has been confirmed via SMS verification.</summary>
    public virtual bool PhoneNumberConfirmed { get; set; } = false;

    /// <summary>Indicates whether two-factor authentication is enabled for this user.</summary>
    public virtual bool TwoFactorEnabled { get; set; }

    /// <summary>The type of two-factor authentication configured (e.g., None, Email, SMS, Authenticator).</summary>
    public virtual TwoFactorType TwoFactorType { get; set; } = TwoFactorType.None;

    /// <summary>The UTC timestamp when the account lockout expires. Null if the account is not locked out.</summary>
    public virtual DateTimeOffset? LockoutEnd { get; set; }

    /// <summary>Indicates whether account lockout is enabled for this user on failed login attempts.</summary>
    public virtual bool LockoutEnabled { get; set; }

    /// <summary>The number of consecutive failed login attempts since the last successful login.</summary>
    public virtual int AccessFailedCount { get; set; }

    /// <summary>The UTC timestamp when the user last changed their password.</summary>
    public virtual DateTime? LastPasswordChangedDate { get; set; }

    /// <summary>Indicates whether the user must change their default/temporary password on next login.</summary>
    public virtual bool? RequiresDefaultPasswordChange { get; set; }

    /// <summary>The UTC timestamp of the user's most recent login.</summary>
    public virtual DateTime? LastLoginDateTime { get; set; }

    /// <summary>The UTC timestamp of the user's most recent logout.</summary>
    public virtual DateTime? LastLogoutDateTime { get; set; }

    /// <summary>The identity provider that manages this user's credentials (Local, Google, etc.).</summary>
    public virtual IdentityProvider IdentityProviderType { get; set; } = IdentityProvider.Local;

    /// <summary>The user's security question/answer pairs for account recovery.</summary>
    public virtual List<UserSecurityQuestionModel> UserSecurityQuestion { get; set; }

    /// <summary>Custom claims associated with this user, included in tokens when matching scopes are granted.</summary>
    public virtual List<UserClaimModel> UserClaims { get; set; }
}

/// <summary>
/// A simplified user display model containing only the fields needed for
/// user listing and management UIs. Excludes sensitive and detailed fields.
/// </summary>
public class UserDisplayModel : BaseModel
{
    /// <summary>The unique username.</summary>
    public virtual string UserName { get; set; }

    /// <summary>The user's password (used during registration/update only).</summary>
    public virtual string Password { get; set; }

    /// <summary>The user's first name.</summary>
    public virtual string FirstName { get; set; }

    /// <summary>The user's last name.</summary>
    public virtual string LastName { get; set; }

    /// <summary>The user's email address.</summary>
    public virtual string Email { get; set; }

    /// <summary>The user's phone number.</summary>
    public virtual string PhoneNumber { get; set; }

    /// <summary>The UTC timestamp when the account lockout expires.</summary>
    public virtual DateTimeOffset? LockoutEnd { get; set; }

    /// <summary>Indicates whether account lockout is enabled.</summary>
    public virtual bool LockoutEnabled { get; set; }
}
