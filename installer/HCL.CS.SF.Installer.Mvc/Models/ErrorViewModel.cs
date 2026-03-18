/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCLCSSFInstallerMVC.Models;

/// <summary>
/// View model for the generic error page, carrying the trace identifier for diagnostics.
/// </summary>
public class ErrorViewModel
{
    /// <summary>The HTTP request trace identifier or Activity ID.</summary>
    public string? RequestId { get; set; }

    /// <summary>Whether the request ID should be displayed on the error page.</summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
