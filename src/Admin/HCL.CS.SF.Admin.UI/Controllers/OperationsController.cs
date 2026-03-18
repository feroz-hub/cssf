/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using HCL.CS.SF.Admin.UI.Options;

namespace HCL.CS.SF.Admin.UI.Controllers;

[Authorize(Policy = "AdminOnly")]
[Route("Admin/Operations")]
public class OperationsController : Controller
{
    private readonly OAuthAdminOptions _options;

    public OperationsController(IOptions<OAuthAdminOptions> options) => _options = options.Value;

    [HttpGet("")]
    public IActionResult Index()
    {
        ViewData["Title"] = "Operations Center";
        ViewData["Breadcrumb"] = "Operations";
        ViewBag.Authority = _options.Authority;
        return View();
    }

    [HttpGet("Endpoints")]
    public IActionResult Endpoints()
    {
        ViewData["Title"] = "Protocol Endpoints";
        ViewData["Breadcrumb"] = "Operations / Endpoints";
        ViewBag.Authority = _options.Authority;
        return View();
    }
}
