/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCLCSSFInstallerMVC.Application.DTOs;

/// <summary>
/// Aggregates the client and admin user configuration required for the database seed step.
/// </summary>
public sealed class SeedConfigurationDto
{
    /// <summary>Configuration for the initial OAuth client to register.</summary>
    public ClientConfigurationDto Client { get; init; } = new();

    /// <summary>Configuration for the initial administrator user account.</summary>
    public AdminUserConfigurationDto AdminUser { get; init; } = new();
}
