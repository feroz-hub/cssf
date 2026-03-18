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
/// Guards the installation process by tracking whether setup has already been completed.
/// Uses a persistent marker (lock file) to prevent reinstallation.
/// </summary>
public interface IInstallationGateService
{
    /// <summary>
    /// Determines whether the installer has already completed by checking for the lock marker.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><c>true</c> if the installation lock marker exists; otherwise <c>false</c>.</returns>
    Task<bool> IsInstallationCompletedAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Reads completion metadata (timestamp, provider, client credentials) from the lock marker file.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The metadata recorded at installation time, or <c>null</c> if installation has not completed.</returns>
    Task<InstallationCompletionMetadataDto?> GetCompletionMetadataAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Writes the lock marker file with the supplied metadata, permanently blocking further installations.
    /// </summary>
    /// <param name="metadata">Completion details to persist in the lock marker.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task MarkInstallationCompletedAsync(InstallationCompletionMetadataDto metadata,
        CancellationToken cancellationToken);
}
