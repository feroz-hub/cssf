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
/// Defines operations for validating database connections and executing EF Core migrations
/// during the installer workflow.
/// </summary>
public interface IDatabaseMigrationService
{
    /// <summary>
    /// Validates that the database connection described by <paramref name="configuration"/> is reachable,
    /// provisioning the database if necessary.
    /// </summary>
    /// <param name="configuration">The database provider type and connection string to validate.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A result indicating whether the connection was successful or the error encountered.</returns>
    Task<ConnectionValidationResultDto> ValidateConnectionAsync(
        DatabaseConfigurationDto configuration,
        CancellationToken cancellationToken);

    /// <summary>
    /// Applies all pending EF Core migrations against the database specified in <paramref name="configuration"/>,
    /// including post-migration schema patches for columns and tables added after the initial migration set.
    /// </summary>
    /// <param name="configuration">The database provider type and connection string to migrate.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A result indicating whether migrations completed successfully or the error encountered.</returns>
    Task<MigrationExecutionResultDto> RunMigrationsAsync(
        DatabaseConfigurationDto configuration,
        CancellationToken cancellationToken);
}
