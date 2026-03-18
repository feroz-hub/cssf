/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.Extensions.Diagnostics.HealthChecks;
using HCLCSSFInstallerMVC.Application.Abstractions;

namespace HCLCSSFInstallerMVC.Infrastructure.Services;

/// <summary>
/// ASP.NET Core health check that reports whether the installer has completed.
/// Returns Healthy when the lock marker exists, Degraded otherwise.
/// </summary>
public sealed class InstallationHealthCheck : IHealthCheck
{
    private readonly IInstallationGateService _installationGateService;

    /// <summary>
    /// Initializes the health check with the gate service.
    /// </summary>
    public InstallationHealthCheck(IInstallationGateService installationGateService)
    {
        _installationGateService = installationGateService;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var completed = await _installationGateService.IsInstallationCompletedAsync(cancellationToken);
        return completed
            ? HealthCheckResult.Healthy("Installation marker is present.")
            : HealthCheckResult.Degraded("Installation has not completed.");
    }
}
