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
[Route("Admin/IdentityResources")]
public class IdentityResourcesController : Controller
{
    private readonly IAdminApiService _api;

    public IdentityResourcesController(IAdminApiService api) => _api = api;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Identity Resources";
        ViewData["Breadcrumb"] = "Identity Resources";
        var resources = await _api.GetAllIdentityResourcesAsync();
        return View(resources);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Details(string id)
    {
        ViewData["Title"] = "Identity Resource Details";
        ViewData["Breadcrumb"] = "Identity Resources / Details";
        var result = await _api.GetIdentityResourceByIdAsync(id);
        if (result?.Data == null) return NotFound();
        return View(result.Data);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] IdentityResourcesModel resource)
    {
        var result = await _api.AddIdentityResourceAsync(resource);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }

    [HttpPost("Update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update([FromBody] IdentityResourcesModel resource)
    {
        var result = await _api.UpdateIdentityResourceAsync(resource);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }

    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _api.DeleteIdentityResourceByIdAsync(id);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }
}
