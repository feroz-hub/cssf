/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HCL.CS.SF.Hosting.Health;

/// <summary>
/// Health check that verifies the distributed cache (e.g. Redis or in-memory) is reachable
/// by performing a write-read-delete roundtrip with a short-lived probe key.
/// Registered under the "ready" tag so it participates in readiness probes only.
/// </summary>
public class CacheDependencyHealthCheck : IHealthCheck
{
    /// <summary>
    /// Maximum time allowed for the cache roundtrip before the check is considered timed out.
    /// </summary>
    private static readonly TimeSpan DependencyTimeout = TimeSpan.FromSeconds(2);

    /// <summary>
    /// The distributed cache instance injected via DI.
    /// </summary>
    private readonly IDistributedCache distributedCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheDependencyHealthCheck"/> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache service resolved from the DI container.</param>
    public CacheDependencyHealthCheck(IDistributedCache distributedCache)
    {
        this.distributedCache = distributedCache;
    }

    /// <summary>
    /// Executes the cache health check by writing a unique key, reading it back, and comparing
    /// the values. Returns <see cref="HealthCheckResult.Healthy"/> when the roundtrip succeeds,
    /// or <see cref="HealthCheckResult.Unhealthy"/> on timeout or any failure.
    /// </summary>
    /// <param name="context">The health check context provided by the framework.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A <see cref="HealthCheckResult"/> indicating the cache availability.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // Create a linked token that enforces the dependency timeout on top of any caller cancellation.
        using var timeoutToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutToken.CancelAfter(DependencyTimeout);

        // Generate a unique probe key and value for this check invocation.
        var key = $"health:{Guid.NewGuid():N}";
        var value = DateTime.UtcNow.ToString("O");
        var entryOptions = new DistributedCacheEntryOptions
        {
            // Short TTL so the probe key auto-expires even if cleanup fails.
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
        };

        try
        {
            // Write the probe value into the cache.
            await distributedCache.SetStringAsync(key, value, entryOptions, timeoutToken.Token);

            // Read it back and compare to confirm a successful roundtrip.
            var readValue = await distributedCache.GetStringAsync(key, timeoutToken.Token);

            if (string.Equals(value, readValue, StringComparison.Ordinal))
                return HealthCheckResult.Healthy("Cache reachable.");

            return HealthCheckResult.Unhealthy("Cache roundtrip failed.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // The timeout fired but the caller did not cancel -- report as unhealthy.
            return HealthCheckResult.Unhealthy("Cache health check timed out.");
        }
        catch
        {
            return HealthCheckResult.Unhealthy("Cache health check failed.");
        }
        finally
        {
            // Best-effort cleanup: remove the temporary probe key to avoid cache pollution.
            try
            {
                await distributedCache.RemoveAsync(key, CancellationToken.None);
            }
            catch
            {
                // Best effort cleanup for the temporary key.
            }
        }
    }
}
