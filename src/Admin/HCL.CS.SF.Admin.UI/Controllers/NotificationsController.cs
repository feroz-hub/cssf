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
[Route("Admin/Notifications")]
public class NotificationsController : Controller
{
    private readonly IAdminApiService _api;

    public NotificationsController(IAdminApiService api) => _api = api;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Notifications";
        ViewData["Breadcrumb"] = "Notifications";

        var providers = await _api.GetAllProviderConfigsAsync();
        ViewBag.Providers = providers;

        return View();
    }

    [HttpPost("SearchLogs")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SearchLogs([FromBody] NotificationSearchRequestModel request)
    {
        var result = await _api.GetNotificationLogsAsync(request);
        return Json(result);
    }

    [HttpPost("SaveProvider")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveProvider([FromBody] ProviderConfigModel config)
    {
        var result = await _api.SaveProviderConfigAsync(config);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }

    [HttpPost("DeleteProvider/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProvider(string id)
    {
        var result = await _api.DeleteProviderConfigAsync(id);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }

    [HttpPost("TestNotification")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TestNotification([FromBody] object request)
    {
        var result = await _api.SendTestNotificationAsync(request);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }
}
