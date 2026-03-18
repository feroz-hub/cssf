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
[Route("Admin/Users")]
public class UsersController : Controller
{
    private readonly IAdminApiService _api;

    public UsersController(IAdminApiService api) => _api = api;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Users";
        ViewData["Breadcrumb"] = "Users";
        var users = await _api.GetAllUsersAsync();
        return View(users);
    }

    [HttpGet("Details/{id}")]
    [HttpGet("{id}")]
    public async Task<IActionResult> Details(string id)
    {
        ViewData["Title"] = "User Details";
        ViewData["Breadcrumb"] = "Users / Details";

        var result = await _api.GetUserByIdAsync(id);
        if (result?.Data == null)
        {
            // Return a visible error instead of bare 404
            ViewData["Title"] = "User Not Found";
            ViewBag.ErrorMessage = $"User with ID '{id}' was not found or the API returned no data.";
            return View("NotFound");
        }

        var rolesTask = _api.GetUserRolesAsync(id);
        var claimsTask = _api.GetUserClaimsAsync(id);
        var allRolesTask = _api.GetAllRolesAsync();
        await Task.WhenAll(rolesTask, claimsTask, allRolesTask);

        ViewBag.UserRoles = await rolesTask;
        ViewBag.UserClaims = await claimsTask;
        ViewBag.AllRoles = await allRolesTask;

        return View(result.Data);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] UserModel user)
    {
        var result = await _api.RegisterUserAsync(user);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }

    [HttpPost("Update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update([FromBody] UserModel user)
    {
        var result = await _api.UpdateUserAsync(user);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }

    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _api.DeleteUserByIdAsync(id);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }

    [HttpPost("Lock/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lock(string id)
    {
        var result = await _api.LockUserAsync(id);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }

    [HttpPost("Unlock")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlock([FromBody] string userName)
    {
        var result = await _api.UnlockUserAsync(userName);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }

    [HttpPost("AddRole")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRole([FromBody] UserRoleRequest request)
    {
        var result = await _api.AddUserRoleAsync(request.UserId, request.RoleId);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }

    [HttpPost("RemoveRole")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveRole([FromBody] UserRoleRequest request)
    {
        var result = await _api.RemoveUserRoleAsync(request.UserId, request.RoleId);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }

    [HttpPost("AddClaim")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddClaim([FromBody] UserClaimRequest request)
    {
        var result = await _api.AddUserClaimAsync(request.UserId, request.ClaimType, request.ClaimValue);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }

    [HttpPost("RemoveClaim")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveClaim([FromBody] UserClaimModel claim)
    {
        var result = await _api.RemoveUserClaimAsync(claim);
        return Json(new { success = result.IsSuccess, errors = result.Errors });
    }
}

public class UserRoleRequest
{
    public string UserId { get; set; } = "";
    public string RoleId { get; set; } = "";
}

public class UserClaimRequest
{
    public string UserId { get; set; } = "";
    public string ClaimType { get; set; } = "";
    public string ClaimValue { get; set; } = "";
}
