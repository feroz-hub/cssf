/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.Extensions.DependencyInjection;

namespace HCL.CS.SF.Service;

/// <summary>
/// Provides dependency injection registration for the Identity Application layer.
/// Acts as the composition root entry point for wiring application-level services
/// into the Microsoft DI container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Identity Application layer services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddHCLCSSFIdentityApplication(this IServiceCollection services)
    {
        return services;
    }
}
