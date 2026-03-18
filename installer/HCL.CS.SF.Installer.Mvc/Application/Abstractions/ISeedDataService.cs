/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCLCSSFInstallerMVC.Application.DTOs;

namespace HCLCSSFInstallerMVC.Application.Abstractions;

/// <summary>
/// Seeds master reference data, the initial admin user, and the first OAuth client
/// into a freshly migrated database.
/// </summary>
public interface ISeedDataService
{
    /// <summary>
    /// Inserts all master data (roles, identity resources, API resources, security questions),
    /// the admin user, and the initial OAuth client within a single transaction.
    /// </summary>
    /// <param name="databaseConfiguration">Provider and connection string for the target database.</param>
    /// <param name="seedConfiguration">Admin user and client details supplied by the installer wizard.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A result containing success status and the generated client ID/secret on success.</returns>
    Task<SeedExecutionResultDto> SeedAsync(
        DatabaseConfigurationDto databaseConfiguration,
        SeedConfigurationDto seedConfiguration,
        CancellationToken cancellationToken);
}
