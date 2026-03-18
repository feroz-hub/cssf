/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCLCSSFInstallerMVC.Application.Abstractions;

namespace HCLCSSFInstallerMVC.Middleware;

/// <summary>
/// Request pipeline middleware that enforces installer access rules:
/// - If setup is NOT completed, all non-setup/non-health requests are redirected to /setup.
/// - If setup IS completed, any attempt to access /setup is redirected to /installed.
/// Health and error endpoints are always allowed through.
/// </summary>
public sealed class SetupRedirectMiddleware
{
    private static readonly PathString SetupPath = new("/setup");
    private static readonly PathString HealthPath = new("/health");
    private static readonly PathString InstalledPath = new("/installed");
    private static readonly PathString ErrorPath = new("/error");
    private readonly ILogger<SetupRedirectMiddleware> _logger;

    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes the middleware with the next delegate and logger.
    /// </summary>
    public SetupRedirectMiddleware(RequestDelegate next, ILogger<SetupRedirectMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Evaluates the request path against the installation gate and redirects as necessary.
    /// </summary>
    public async Task InvokeAsync(HttpContext context, IInstallationGateService installationGateService)
    {
        var path = context.Request.Path;
        if (IsAlwaysAllowedPath(path))
        {
            await _next(context);
            return;
        }

        var isInstalled = await installationGateService.IsInstallationCompletedAsync(context.RequestAborted);
        if (!isInstalled && !path.StartsWithSegments(SetupPath, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Setup is not completed. Redirecting {Path} to /setup.", path);
            context.Response.Redirect(SetupPath);
            return;
        }

        if (isInstalled && path.StartsWithSegments(SetupPath, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Setup already completed. Blocking access to {Path}.", path);
            context.Response.Redirect(InstalledPath);
            return;
        }

        await _next(context);
    }

    /// <summary>
    /// Returns true for paths that should bypass the setup redirect logic (health, error).
    /// </summary>
    private static bool IsAlwaysAllowedPath(PathString path)
    {
        if (path.StartsWithSegments(HealthPath, StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments(ErrorPath, StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }
}
