/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HCL.CS.SF.Admin.UI.Interfaces;

namespace HCL.CS.SF.Admin.UI.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly ITokenService _tokenService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(ITokenService tokenService, ILogger<AccountController> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(LoginCallback), "Account", new { returnUrl });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl ?? "/" };
        return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpGet]
    [Authorize]
    public IActionResult LoginCallback(string? returnUrl = null)
    {
        return Redirect(returnUrl ?? "/");
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await _tokenService.RevokeTokenAsync();
            _logger.LogInformation("Tokens revoked successfully during logout.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token revocation failed during logout. Proceeding with sign-out.");
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);

        return RedirectToAction(nameof(SignedOut));
    }

    [HttpGet]
    public IActionResult AuthError(string? errorCode, string? errorMessage)
    {
        ViewData["Title"] = "Authentication Error";
        ViewBag.ErrorCode = errorCode ?? "Unknown";
        ViewBag.ErrorMessage = errorMessage ?? "An authentication error occurred.";
        return View();
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        _logger.LogWarning(
            "Access denied for user {User} attempting to reach {ReturnUrl}.",
            User.Identity?.Name ?? "unknown",
            Request.Query["ReturnUrl"].FirstOrDefault());

        return View();
    }

    [HttpGet]
    public IActionResult SignedOut()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(Login));
        }

        return View();
    }
}
