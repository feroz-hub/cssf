/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HCL.CS.SF.Admin.UI.Services;

namespace HCL.CS.SF.Admin.UI.Controllers;

[Authorize(Policy = "AdminOnly")]
[Route("Admin/Dashboard")]
public class DashboardController : Controller
{
    private readonly IAdminApiService _api;

    public DashboardController(IAdminApiService api) => _api = api;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Dashboard";
        ViewData["Breadcrumb"] = "Dashboard";

        // Fetch all stats in parallel
        var usersTask = _api.GetAllUsersAsync();
        var rolesTask = _api.GetAllRolesAsync();
        var clientsTask = _api.GetAllClientsAsync();
        var resourcesTask = _api.GetAllApiResourcesAsync();
        var identityResourcesTask = _api.GetAllIdentityResourcesAsync();

        await Task.WhenAll(usersTask, rolesTask, clientsTask, resourcesTask, identityResourcesTask);

        ViewBag.Users = await usersTask;
        ViewBag.Roles = await rolesTask;
        ViewBag.Clients = await clientsTask;
        ViewBag.ApiResources = await resourcesTask;
        ViewBag.IdentityResources = await identityResourcesTask;

        return View();
    }
}
