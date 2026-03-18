/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Hosting;

// Placeholder for a dedicated executable API host.
/// <summary>
/// Composition root placeholder for the HCL.CS.SF Identity API.
/// This static class serves as the entry point marker for the Identity API hosting project.
/// Actual service composition is performed via the extension methods in
/// <see cref="Extensions.HCLCSSFExtension"/> and <see cref="Extensions.HCLCSSFBuilder"/>.
/// </summary>
public static class Program
{
    /// <summary>
    /// Human-readable description of this hosting project's purpose.
    /// </summary>
    public const string Description = "HCL.CS.SF Identity API composition root placeholder.";
}
