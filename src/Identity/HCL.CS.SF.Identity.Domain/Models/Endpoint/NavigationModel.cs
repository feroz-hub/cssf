/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Models.Endpoint.Response;

namespace HCL.CS.SF.Domain.Models.Endpoint;

/// <summary>
/// Represents the navigation result after processing an authentication or authorization action.
/// Used to determine whether to show the login page or redirect the user to a target URL.
/// </summary>
public class NavigationModel : ErrorResponseModel
{
    /// <summary>Indicates whether the user should be directed to the login page.</summary>
    public bool IsLogin { get; set; }

    /// <summary>Indicates whether a redirect URL is available, meaning the user should be redirected.</summary>
    public bool IsRedirect => !string.IsNullOrWhiteSpace(RedirectUrl);

    /// <summary>The target URL to redirect the user to (e.g., client redirect URI or post-login return URL).</summary>
    public string RedirectUrl { get; set; }
}
