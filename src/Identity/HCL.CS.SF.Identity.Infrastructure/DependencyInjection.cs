/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.Extensions.DependencyInjection;

namespace HCL.CS.SF.Infrastructure.Services;

/// <summary>
/// Entry point for registering Identity Infrastructure services into the DI container.
/// Serves as a placeholder for future infrastructure-specific registrations.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Identity Infrastructure services to the specified service collection.
    /// Currently a no-op; core registrations are handled by
    /// <see cref="Extension.InfrastructureServiceExtension.AddInfrastructureServices"/>.
    /// </summary>
    /// <param name="services">The service collection to extend.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddHCLCSSFIdentityInfrastructure(this IServiceCollection services)
    {
        return services;
    }
}
