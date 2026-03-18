/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCLCSSFInstallerMVC.Application.Exceptions;

/// <summary>
/// Thrown when an installer workflow step is executed out of order or with missing prerequisites
/// (e.g., running migrations before validating the connection).
/// </summary>
public sealed class InstallerWorkflowException : Exception
{
    /// <summary>
    /// Initializes a new instance with a message describing the workflow violation.
    /// </summary>
    /// <param name="message">Description of the step ordering or prerequisite violation.</param>
    public InstallerWorkflowException(string message)
        : base(message)
    {
    }
}
