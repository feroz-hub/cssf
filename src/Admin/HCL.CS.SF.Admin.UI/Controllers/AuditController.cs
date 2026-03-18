/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HCL.CS.SF.Admin.UI.Models.Api;
using HCL.CS.SF.Admin.UI.Services;

namespace HCL.CS.SF.Admin.UI.Controllers;

[Authorize(Policy = "AdminOnly")]
[Route("Admin/Audit")]
public class AuditController : Controller
{
    private readonly IAdminApiService _api;

    public AuditController(IAdminApiService api) => _api = api;

    [HttpGet("")]
    public IActionResult Index()
    {
        ViewData["Title"] = "Audit Trail";
        ViewData["Breadcrumb"] = "Audit";
        return View();
    }

    [HttpPost("Search")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Search([FromBody] AuditSearchRequestModel request)
    {
        var result = await _api.GetAuditDetailsAsync(request);
        return Json(result);
    }
}
