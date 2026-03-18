/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.Service.Implementation.Api.Wrappers;
using HCL.CS.SF.Service.Implementation.Endpoint;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Implementation.Endpoint.Parsers;
using HCL.CS.SF.Service.Implementation.Endpoint.Services;
using HCL.CS.SF.Service.Implementation.Endpoint.Validators;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Parsers;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Results;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Validators;
using static HCL.CS.SF.Domain.Constants.Endpoint.OpenIdConstants;

namespace HCL.CS.SF.Service.Extension;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> that register core identity
/// services, wrapper adapters, OAuth/OIDC endpoints, and endpoint validators into the DI container.
/// </summary>
public static class ServiceExtension
{
    /// <summary>
    /// Registers core identity services including authorization, token generation,
    /// discovery, user info, JWKS, back-channel logout, session management, and interaction services.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddTransient<IAuthorizationService, AuthorizationService>();
        services.AddTransient<ITokenGenerationService, TokenGenerationService>();
        services.AddTransient<IDiscoveryService, DiscoveryService>();
        services.AddTransient<IUserInfoServices, UserInfoServices>();
        services.AddTransient<IJWKSService, JWKSService>();
        services.AddTransient<IBackChannelLogoutService, BackChannelLogoutService>();

        services.AddScoped<ISessionManagementService, SessionManagementService>();
        services.AddScoped<IInteractionService, InteractionService>();

        services.AddHttpClient<BackChannelLogoutService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(5);
        });
        return services;
    }

    /// <summary>
    /// Registers wrapper/adapter services that decorate ASP.NET Core Identity infrastructure,
    /// including the authentication service wrapper, claims principal factory, and Argon2 password hasher.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddWrappers(this IServiceCollection services)
    {
        services.AddTransient<IAuthenticationService, AuthenticationServiceWrapper>();
        services.AddTransient<IUserClaimsPrincipalFactory<Users>, UserClaimsPrincipalWrapper>();

        services.AddScoped<IPasswordHasher<Users>, Argon2PasswordHasherWrapper<Users>>();
        return services;
    }

    /// <summary>
    /// Registers all default OAuth 2.0 / OpenID Connect endpoint handlers (token, authorize,
    /// introspection, discovery, end-session, revocation, JWKS, and user info) along with
    /// their associated request validators and parsers.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddDefaultEndpoints(this IServiceCollection services)
    {
        AddEndpoint<TokenEndpoint>(services, EndpointsName.Token, EndpointRoutePaths.Token.IncludedFrontSlash());
        AddEndpoint<AuthorizeEndpoint>(services, EndpointsName.Authorize,
            EndpointRoutePaths.Authorize.IncludedFrontSlash());
        AddEndpoint<AuthorizeCallBackEndpoint>(services, EndpointsName.Authorize,
            EndpointRoutePaths.AuthorizeCallback.IncludedFrontSlash());
        AddEndpoint<IntrospectionEndpoint>(services, EndpointsName.Introspection,
            EndpointRoutePaths.Introspection.IncludedFrontSlash());
        AddEndpoint<DiscoveryEndpoint>(services, EndpointsName.Discovery,
            EndpointRoutePaths.DiscoveryConfiguration.IncludedFrontSlash());
        AddEndpoint<EndSessionEndpoint>(services, EndpointsName.EndSession,
            EndpointRoutePaths.EndSession.IncludedFrontSlash());
        AddEndpoint<EndSessionCallbackEndpoint>(services, EndpointsName.EndSession,
            EndpointRoutePaths.EndSessionCallback.IncludedFrontSlash());
        AddEndpoint<TokenRevocationEndpoint>(services, EndpointsName.Revocation,
            EndpointRoutePaths.Revocation.IncludedFrontSlash());
        AddEndpoint<JwksEndpoint>(services, EndpointsName.Discovery,
            EndpointRoutePaths.JWKSWebKeys.IncludedFrontSlash());
        AddEndpoint<UserInfoEndpoint>(services, EndpointsName.UserInfo,
            EndpointRoutePaths.UserInfo.IncludedFrontSlash());
        AddEndpointValidations(services);
        return services;
    }

    /// <summary>
    /// Registers a single endpoint handler as a transient service and records its route metadata
    /// as a singleton <see cref="SecurityEndpointModel"/>.
    /// </summary>
    /// <typeparam name="T">The endpoint implementation type.</typeparam>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="name">The logical endpoint name (e.g., "Token", "Authorize").</param>
    /// <param name="path">The HTTP path that the endpoint is mapped to.</param>
    private static void AddEndpoint<T>(IServiceCollection services, string name, PathString path)
        where T : class, IEndpoint
    {
        services.AddTransient<T>();
        services.AddSingleton(new SecurityEndpointModel(name, path, typeof(T)));
    }

    /// <summary>
    /// Registers all endpoint-level request validators, parsers, and secret validators
    /// used during token, introspection, session, and revocation request processing.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    private static void AddEndpointValidations(IServiceCollection services)
    {
        // Token Request Validator
        services.AddTransient<ITokenRequestValidator, TokenRequestValidator>();
        services.AddTransient<ITokenParser, TokenParser>();

        // Client Validator
        services.AddTransient<IClientSecretValidator, ClientSecretValidator>();
        services.AddTransient<IClientSecretParser, ClientSecretParser>();
        services.AddTransient<ISecretValidator, SecretValidator>();

        services.AddTransient<IResourceScopeValidator, ResourceScopeValidator>();
        services.AddTransient<IIntrospectionRequestValidator, IntrospectionRequestValidator>();

        // Session Request Validator
        services.AddTransient<IEndSessionRequestValidator, EndSessionRequestValidator>();

        // Token Revocation Validator
        services.AddTransient<ITokenRevocationRequestValidator, TokenRevocationRequestValidator>();
    }
}
