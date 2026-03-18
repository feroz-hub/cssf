/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Enums;

namespace HCLCSSFInstallerMVC.Application.DTOs;

/// <summary>
/// Carries the administrator user details entered during the seed step of the installer wizard.
/// </summary>
public sealed class AdminUserConfigurationDto
{
    /// <summary>Login username for the admin account.</summary>
    public string UserName { get; init; } = string.Empty;

    /// <summary>Plain-text password that will be hashed with Argon2 before storage.</summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>Admin user's first name.</summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>Admin user's optional last name.</summary>
    public string? LastName { get; init; }

    /// <summary>Admin user's email address.</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Admin user's phone number.</summary>
    public string PhoneNumber { get; init; } = string.Empty;

    /// <summary>Whether the admin authenticates via Local credentials or LDAP.</summary>
    public IdentityProvider IdentityProvider { get; init; }
}
