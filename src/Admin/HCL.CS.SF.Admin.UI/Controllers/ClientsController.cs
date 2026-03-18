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
[Route("Admin/Clients")]
public class ClientsController : Controller
{
    private readonly IAdminApiService _api;

    public ClientsController(IAdminApiService api) => _api = api;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "OAuth Clients";
        ViewData["Breadcrumb"] = "Clients";
        var clients = await _api.GetAllClientsAsync();
        var scopes = await _api.GetAllApiScopesAsync();
        ViewBag.AllScopes = scopes;
        return View(clients);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Details(string id)
    {
        ViewData["Title"] = "Client Details";
        ViewData["Breadcrumb"] = "Clients / Details";
        var result = await _api.GetClientAsync(id);
        if (result?.Data == null) return NotFound();

        var scopes = await _api.GetAllApiScopesAsync();
        ViewBag.AllScopes = scopes;

        return View(result.Data);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] ClientsModel client)
    {
        var result = await _api.RegisterClientAsync(client);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }

    [HttpPost("Update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update([FromBody] ClientsModel client)
    {
        var result = await _api.UpdateClientAsync(client);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }

    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _api.DeleteClientAsync(id);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }

    [HttpPost("GenerateSecret/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateSecret(string id)
    {
        var result = await _api.GenerateClientSecretAsync(id);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }
}
