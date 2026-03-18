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
/// Abstracts the creation of a database instance for a specific database provider.
/// Implementations ensure that the target database exists before migrations are applied.
/// </summary>
public interface IDatabaseProvisioner
{
    /// <summary>
    /// Checks whether the target database exists and creates it when automatic provisioning is allowed.
    /// </summary>
    /// <param name="configuration">The database provider type and connection string identifying the target database.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task EnsureDatabaseExistsAsync(
        DatabaseConfigurationDto configuration,
        CancellationToken cancellationToken);
}
