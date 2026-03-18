/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using HCL.CS.SF.DemoServerApp.Options;
using HCL.CS.SF.DemoServerApp.Services.ExternalAuth;

namespace HCL.CS.SF.DemoServerApp.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class ExternalAuthController(
    IExternalAuthService externalAuthService,
    IOptions<GoogleOidcOptions> googleOptions)
    : Controller
{
    private readonly GoogleOidcOptions google = googleOptions.Value;

    [AllowAnonymous]
    [HttpGet("/auth/external/google/start")]
    public IActionResult GoogleStart(string? returnUrl = null, string? tenantId = null)
    {
        if (!google.Enabled)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                "Google sign-in is not enabled. Set Authentication:Google:Enabled to true and configure ClientId and ClientSecret.");
        }

        var properties = externalAuthService.BuildChallengeProperties(
            GoogleExternalAuthProvider.ProviderName,
            returnUrl,
            tenantId,
            false);

        return Challenge(properties, GoogleExternalAuthProvider.Scheme);
    }

    [AllowAnonymous]
    [HttpGet("/auth/external/google/callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        if (!google.Enabled)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                "Google sign-in is not enabled.");
        }

        var result = await externalAuthService.CompleteGoogleCallbackAsync(HttpContext);
        if (result.RequiresTwoFactor) return Redirect(result.RedirectUrl);

        if (!result.Succeeded)
        {
            TempData["ExternalAuthError"] = result.Message;
            return RedirectToAction("Login", "Account", new { returnUrl = result.RedirectUrl });
        }

        return RedirectSafely(result.RedirectUrl);
    }

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost("/auth/external/link/google")]
    public IActionResult LinkGoogle(string? returnUrl = null)
    {
        if (!google.Enabled) return NotFound();

        var properties = externalAuthService.BuildChallengeProperties(
            GoogleExternalAuthProvider.ProviderName,
            returnUrl,
            null,
            true);

        return Challenge(properties, GoogleExternalAuthProvider.Scheme);
    }

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost("/auth/external/unlink/google")]
    public async Task<IActionResult> UnlinkGoogle()
    {
        var result = await externalAuthService.UnlinkGoogleAsync(HttpContext);
        TempData["ExternalAuthMessage"] = result.Message;
        return RedirectSafely(result.RedirectUrl);
    }

    private IActionResult RedirectSafely(string redirectUrl)
    {
        return Url.IsLocalUrl(redirectUrl) ? LocalRedirect(redirectUrl) : Redirect(redirectUrl);
    }
}
