/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.Extensions.DependencyInjection;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.Infrastructure.Resources;

namespace HCL.CS.SF.Infrastructure.Resources.Extension;

/// <summary>
/// Provides extension methods on <see cref="IServiceCollection"/> for registering
/// infrastructure resource services, AutoMapper, configuration, and cryptographic key stores.
/// </summary>
public static class InfrastructureResourceExtension
{
    /// <summary>
    /// Registers the infrastructure resource services including the <see cref="IResourceStringHandler"/> singleton.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddInfrastructureResources(this IServiceCollection services)
    {
        services.AddSingleton<IResourceStringHandler, ResourceStringHandler>();
        // services.AddSingleton<ICSExceptionManager, CustomExceptionManager>();
        return services;
    }

    /// <summary>
    /// Initialises AutoMapper with all Security Framework mapping profiles and registers the
    /// resulting <see cref="AutoMapper.IMapper"/> as a singleton in the service collection.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddAutoMapper(this IServiceCollection services)
    {
        var config = new Mapper().InitializeMapper();
        var mapper = config.CreateMapper();
        services.AddSingleton(mapper);
        return services;
    }

    /// <summary>
    /// Registers the <see cref="HCLCSSFConfig"/> application configuration object as a singleton.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="config">The configuration instance to register.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddConfiguration(this IServiceCollection services, HCLCSSFConfig config)
    {
        services.AddSingleton(config);
        return services;
    }

    /// <summary>
    /// Registers utility services including the <see cref="IKeyStore"/> as a transient service.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddUtilityServices(this IServiceCollection services)
    {
        services.AddTransient<IKeyStore, KeyStore>();
        return services;
    }

    /// <summary>
    /// Validates the provided asymmetric security keys and registers the resulting key store dictionary
    /// as a singleton if at least one valid key is present.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="securityKeys">The asymmetric key entries to validate and load into the key store.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddSecurityAsymmetricKeystore(this IServiceCollection services,
        IEnumerable<AsymmetricKeyInfoModel> securityKeys)
    {
        // Validate all keys and build the dictionary; only register if non-empty
        var keyStore = new KeyStore().Add(securityKeys);
        if (keyStore.Count > 0) services.AddSingleton(keyStore);

        return services;
    }
}
