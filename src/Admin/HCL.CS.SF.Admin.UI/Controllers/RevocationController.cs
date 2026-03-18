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
[Route("Admin/Revocation")]
public class RevocationController : Controller
{
    private readonly IAdminApiService _api;

    public RevocationController(IAdminApiService api) => _api = api;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Token Revocation";
        ViewData["Breadcrumb"] = "Revocation";

        var clients = await _api.GetAllClientsAsync();
        ViewBag.Clients = clients;

        return View();
    }

    [HttpPost("SearchByClient")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SearchByClient([FromBody] List<string> clientIds)
    {
        var result = await _api.GetActiveTokensByClientIdsAsync(clientIds);
        return Json(result);
    }

    [HttpPost("SearchByUser")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SearchByUser([FromBody] List<string> userIds)
    {
        var result = await _api.GetActiveTokensByUserIdsAsync(userIds);
        return Json(result);
    }

    [HttpPost("Revoke")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Revoke([FromBody] RevokeTokenRequest request)
    {
        var result = await _api.RevokeTokenAsync(request.Token, request.TokenTypeHint);
        return Json(new { success = result });
    }

    [HttpPost("RevokeAllByUserId")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeAllByUserId([FromBody] RevokeAllByUserIdRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return Json(new { success = false, message = "UserId is required.", revokedCount = 0 });
        }

        var tokens = await _api.GetActiveTokensByUserIdsAsync(new List<string> { request.UserId });

        var revokedCount = 0;
        foreach (var t in tokens)
        {
            var tokenValue = t.Token ?? t.TokenValue ?? t.Key ?? t.Id;
            var tokenTypeHint = t.TokenTypeHint ?? t.TokenType ?? "access_token";

            if (string.IsNullOrWhiteSpace(tokenValue))
                continue;

            var revoked = await _api.RevokeTokenAsync(tokenValue, tokenTypeHint);
            if (revoked)
                revokedCount++;
        }

        return Json(new { success = true, revokedCount });
    }
}

public class RevokeAllByUserIdRequest
{
    public string UserId { get; set; } = string.Empty;
}
