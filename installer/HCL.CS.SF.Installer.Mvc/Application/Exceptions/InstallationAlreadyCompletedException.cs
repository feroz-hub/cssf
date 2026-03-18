/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCLCSSFInstallerMVC.Application.Exceptions;

/// <summary>
/// Thrown when an installer workflow operation is attempted after the installation lock marker
/// has already been written, preventing reinstallation.
/// </summary>
public sealed class InstallationAlreadyCompletedException : Exception
{
    /// <summary>
    /// Initializes a new instance with a default message indicating reinstallation is blocked.
    /// </summary>
    public InstallationAlreadyCompletedException()
        : base("Installation has already completed. Reinstallation is blocked.")
    {
    }
}
