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
/// Persists and retrieves the installer session state across HTTP requests.
/// The state tracks which wizard steps have been completed and their configuration.
/// </summary>
public interface IInstallerStateStore
{
    /// <summary>
    /// Loads the current session state, returning a default instance if no state exists.
    /// </summary>
    Task<InstallerSessionState> GetAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Persists the supplied session state so it survives across requests.
    /// </summary>
    Task SaveAsync(InstallerSessionState state, CancellationToken cancellationToken);

    /// <summary>
    /// Removes the stored session state, effectively resetting the wizard.
    /// </summary>
    Task ClearAsync(CancellationToken cancellationToken);
}
