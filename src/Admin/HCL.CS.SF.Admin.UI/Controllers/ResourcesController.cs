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
[Route("Admin/Resources")]
public class ResourcesController : Controller
{
    private readonly IAdminApiService _api;

    public ResourcesController(IAdminApiService api) => _api = api;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "API Resources & Scopes";
        ViewData["Breadcrumb"] = "Resources";
        var resources = await _api.GetAllApiResourcesAsync();
        return View(resources);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Details(string id)
    {
        ViewData["Title"] = "Resource Details";
        ViewData["Breadcrumb"] = "Resources / Details";
        var result = await _api.GetApiResourceByIdAsync(id);
        if (result?.Data == null) return NotFound();
        return View(result.Data);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] ApiResourcesModel resource)
    {
        var result = await _api.AddApiResourceAsync(resource);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }

    [HttpPost("Update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update([FromBody] ApiResourcesModel resource)
    {
        var result = await _api.UpdateApiResourceAsync(resource);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }

    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _api.DeleteApiResourceByIdAsync(id);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }

    [HttpPost("AddScope")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddScope([FromBody] ApiScopesModel scope)
    {
        var result = await _api.AddApiScopeAsync(scope);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }

    [HttpPost("UpdateScope")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateScope([FromBody] ApiScopesModel scope)
    {
        var result = await _api.UpdateApiScopeAsync(scope);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }

    [HttpPost("DeleteScope/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteScope(string id)
    {
        var result = await _api.DeleteApiScopeByIdAsync(id);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }
}
