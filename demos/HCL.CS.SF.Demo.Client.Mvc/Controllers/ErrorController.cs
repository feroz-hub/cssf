/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HCL.CS.SF.DemoClientMvc.Models;

namespace HCL.CS.SF.DemoClientMvc.Controllers;

public class ErrorController : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(string errorCode, string errorMessage)
    {
        var normalizedErrorCode = string.IsNullOrWhiteSpace(errorCode) ? "UnhandledError" : errorCode;

        var safeMessage = normalizedErrorCode switch
        {
            "OidcRemoteFailure" => "Remote authentication failure.",
            "OidcAuthenticationFailed" => "OpenID Connect authentication failed.",
            "UnhandledError" => "The request could not be completed due to an unexpected error.",
            _ => "The request could not be completed due to an unexpected error."
        };

        var errorViewModel = new ErrorViewModel
            { ErrorCode = normalizedErrorCode, ErrorMessage = safeMessage };
        errorViewModel.RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        return View(errorViewModel);
    }
}
