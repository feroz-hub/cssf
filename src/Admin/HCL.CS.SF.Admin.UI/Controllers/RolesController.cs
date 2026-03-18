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
[Route("Admin/Roles")]
public class RolesController : Controller
{
    private readonly IAdminApiService _api;

    public RolesController(IAdminApiService api) => _api = api;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Roles & Claims";
        ViewData["Breadcrumb"] = "Roles";
        var roles = await _api.GetAllRolesAsync();
        return View(roles);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Details(string id)
    {
        ViewData["Title"] = "Role Details";
        ViewData["Breadcrumb"] = "Roles / Details";
        var result = await _api.GetRoleByIdAsync(id);
        if (result?.Data == null) return NotFound();

        var claims = await _api.GetRoleClaimsAsync(id);
        ViewBag.RoleClaims = claims;

        return View(result.Data);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] RoleModel role)
    {
        var result = await _api.CreateRoleAsync(role);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }

    [HttpPost("Update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update([FromBody] RoleModel role)
    {
        var result = await _api.UpdateRoleAsync(role);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }

    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _api.DeleteRoleByIdAsync(id);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }

    [HttpPost("AddClaim")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddClaim([FromBody] RoleClaimRequest request)
    {
        var result = await _api.AddRoleClaimAsync(request.RoleId, request.ClaimType, request.ClaimValue);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }

    [HttpPost("RemoveClaim")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveClaim([FromBody] RoleClaimModel claim)
    {
        var result = await _api.RemoveRoleClaimAsync(claim);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }
}

public class RoleClaimRequest
{
    public string RoleId { get; set; } = "";
    public string ClaimType { get; set; } = "";
    public string ClaimValue { get; set; } = "";
}
