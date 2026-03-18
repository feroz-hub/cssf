/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HCL.CS.SF.DomainServices;

namespace HCL.CS.SF.Hosting.Health;

/// <summary>
/// Health check that verifies database connectivity by executing a lightweight query
/// against the Users table. Registered under the "ready" tag so it participates in
/// readiness probes only (not liveness).
/// </summary>
public class DatabaseDependencyHealthCheck : IHealthCheck
{
    /// <summary>
    /// Maximum time allowed for the database probe before the check is considered timed out.
    /// </summary>
    private static readonly TimeSpan DependencyTimeout = TimeSpan.FromSeconds(2);

    /// <summary>
    /// The application database context injected via DI.
    /// </summary>
    private readonly IApplicationDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseDependencyHealthCheck"/> class.
    /// </summary>
    /// <param name="dbContext">The application database context resolved from the DI container.</param>
    public DatabaseDependencyHealthCheck(IApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <summary>
    /// Executes the database health check by running a simple <c>AnyAsync</c> query on
    /// the Users table. Returns <see cref="HealthCheckResult.Healthy"/> when the query
    /// succeeds, or <see cref="HealthCheckResult.Unhealthy"/> on timeout or any failure.
    /// </summary>
    /// <param name="context">The health check context provided by the framework.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A <see cref="HealthCheckResult"/> indicating the database availability.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // Create a linked token that enforces the dependency timeout on top of any caller cancellation.
        using var timeoutToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutToken.CancelAfter(DependencyTimeout);

        try
        {
            // Execute a lightweight existence check to verify database connectivity.
            _ = await dbContext.Users.AnyAsync(timeoutToken.Token);
            return HealthCheckResult.Healthy("Database reachable.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // The timeout fired but the caller did not cancel -- report as unhealthy.
            return HealthCheckResult.Unhealthy("Database health check timed out.");
        }
        catch
        {
            return HealthCheckResult.Unhealthy("Database health check failed.");
        }
    }
}
