/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.Extensions.DependencyInjection;
using HCL.CS.SF.ProxyService.Proxy;
using HCL.CS.SF.ProxyService.Routes;
using HCL.CS.SF.ProxyService.Validator;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api.Wrapper;

namespace HCL.CS.SF.ProxyService.Extension;

/// <summary>
/// Provides extension methods for registering gateway proxy services, validators,
/// and route handlers into the dependency injection container.
/// Each proxy service wraps a backend domain service with gateway-level authorization checks.
/// </summary>
public static class ServiceExtension
{
    /// <summary>
    /// Registers all proxy service implementations as transient services.
    /// Each proxy decorates the corresponding domain service with API validation
    /// (permission and token checks) before forwarding calls to the backend.
    /// </summary>
    /// <param name="services">The service collection to register proxy services into.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddProxyServices(this IServiceCollection services)
    {
        services.AddTransient<IApiResourceService, ApiResourceProxyService>();
        services.AddTransient<IIdentityResourceService, IdentityResourceProxyService>();
        services.AddTransient<IRoleService, RoleProxyService>();
        services.AddTransient<IUserAccountService, UserAccountProxyServices>();
        services.AddTransient<IAuditTrailService, AuditTrailProxyService>();
        services.AddTransient<IAuthenticationService, AuthenticationProxyService>();
        services.AddTransient<IClientServices, ClientProxyService>();
        services.AddTransient<ISecurityTokenService, SecurityTokenProxyService>();
        services.AddTransient<INotificationManagementService, NotificationManagementProxyService>();
        services.AddTransient<IExternalAuthManagementService, ExternalAuthManagementProxyService>();
        return services;
    }

    /// <summary>
    /// Registers the <see cref="ApiValidator"/> as the implementation for <see cref="IApiValidator"/>.
    /// The validator enforces permission-based access control on every proxied API call.
    /// </summary>
    /// <param name="services">The service collection to register the validator into.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddProxyValidator(this IServiceCollection services)
    {
        services.AddTransient<IApiValidator, ApiValidator>();
        return services;
    }

    /// <summary>
    /// Registers the <see cref="ApiGateway"/> as the implementation for <see cref="IApiGateway"/>.
    /// The gateway dispatches inbound HTTP requests to the appropriate backend service handler.
    /// </summary>
    /// <param name="services">The service collection to register the route gateway into.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddProxyRoutes(this IServiceCollection services)
    {
        services.AddTransient<IApiGateway, ApiGateway>();
        return services;
    }
}
