/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.DomainServices.Repository.Api;
using HCL.CS.SF.DomainServices.UnitOfWork.Api;
using HCL.CS.SF.DomainServices.UnitOfWork.Endpoint;
using HCL.CS.SF.DomainServices.Wrappers;
using HCL.CS.SF.Infrastructure.Data.Repository.Api;
using HCL.CS.SF.Infrastructure.Data.UnitOfWork.Api;
using HCL.CS.SF.Infrastructure.Data.UnitOfWork.Endpoint;
using HCL.CS.SF.Infrastructure.Data.Validation;
using HCL.CS.SF.Infrastructure.Data.Wrappers;

namespace HCL.CS.SF.Infrastructure.Data.Extension;

/// <summary>
/// Provides extension methods for <see cref="IServiceCollection"/> to register the persistence layer:
/// DbContext configuration, ASP.NET Core Identity services, repository implementations,
/// unit-of-work classes, and token provider options.
/// </summary>
public static class InfrastructureDataExtension
{
    /// <summary>
    /// Registers the <see cref="ApplicationDbContext"/> with the appropriate database provider
    /// (SQL Server, MySQL, PostgreSQL, or SQLite) based on configuration, configures ASP.NET Core
    /// Identity with password, lockout, and token settings, and registers token providers.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services)
    {
        var configSettings = GetRegisteredConfiguration(services);

        if (configSettings.SystemSettings.DBConfig.Database == DbTypes.SqlServer)
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configSettings.SystemSettings.DBConfig.DBConnectionString));
        else if (configSettings.SystemSettings.DBConfig.Database == DbTypes.MySql)
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(configSettings.SystemSettings.DBConfig.DBConnectionString,
                    ServerVersion.AutoDetect(configSettings.SystemSettings.DBConfig.DBConnectionString)));
        else if (configSettings.SystemSettings.DBConfig.Database == DbTypes.PostgreSQL)
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(configSettings.SystemSettings.DBConfig.DBConnectionString));
        else if (configSettings.SystemSettings.DBConfig.Database == DbTypes.SQLite)
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(configSettings.SystemSettings.DBConfig.DBConnectionString));

        AddIdentityServices(services, configSettings);

        services.AddLogging();
        services.AddHttpContextAccessor();

        services.Configure<DataProtectionTokenProviderOptions>(opt =>
        {
            if (configSettings != null)
                opt.TokenLifespan =
                    TimeSpan.FromMinutes(configSettings.SystemSettings.UserConfig.UserTokenExpiry);
        });

        services.Configure<EmailConfirmationTokenProviderOptions>(opt =>
        {
            if (configSettings != null)
                opt.TokenLifespan =
                    TimeSpan.FromMinutes(configSettings.SystemSettings.UserConfig.EmailTokenExpiry);
        });

        services.Configure<ChangePhoneNumberTokenProviderOption>(opt =>
        {
            if (configSettings != null)
                opt.TokenLifespan =
                    TimeSpan.FromMinutes(configSettings.SystemSettings.UserConfig.OTPTokenExpiry);
        });

        services.Configure<PasswordResetTokenProviderOptions>(opt =>
        {
            if (configSettings != null)
                opt.TokenLifespan =
                    TimeSpan.FromMinutes(configSettings.SystemSettings.UserConfig.PasswordResetTokenExpiry);
        });

        services.AddTransient<IApplicationDbContext, ApplicationDbContext>();
        return services;
    }

    /// <summary>
    /// Retrieves the <see cref="HCLCSSFConfig"/> singleton previously registered in the service collection.
    /// Throws if the configuration has not been registered.
    /// </summary>
    private static HCLCSSFConfig GetRegisteredConfiguration(IServiceCollection services)
    {
        var configDescriptor = services.LastOrDefault(d => d.ServiceType == typeof(HCLCSSFConfig));
        var configSettings = configDescriptor?.ImplementationInstance as HCLCSSFConfig;
        if (configSettings == null)
            throw new InvalidOperationException("HCL.CS.SF configuration has not been registered.");

        return configSettings;
    }

    /// <summary>
    /// Registers all repository implementations, unit-of-work classes, and the database connection
    /// validator as transient services in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRepository(this IServiceCollection services)
    {
        services.AddTransient<IRepository<Notification>, BaseRepository<Notification>>();
        services.AddTransient<IRepository<NotificationProviderConfig>, BaseRepository<NotificationProviderConfig>>();
        services.AddTransient<IRepository<ExternalAuthProviderConfig>, BaseRepository<ExternalAuthProviderConfig>>();

        services.AddTransient<IAuditRepository, AuditRepository>();

        services.AddTransient<IRepository<Clients>, BaseRepository<Clients>>();
        services.AddTransient<IRepository<ClientRedirectUris>, BaseRepository<ClientRedirectUris>>();
        services
            .AddTransient<IRepository<ClientPostLogoutRedirectUris>, BaseRepository<ClientPostLogoutRedirectUris>>();

        services.AddTransient<IApiResourceRepository, ApiResourceRepository>();
        services.AddTransient<IRepository<ApiResources>, BaseRepository<ApiResources>>();
        services.AddTransient<IRepository<ApiResourceClaims>, BaseRepository<ApiResourceClaims>>();
        services.AddTransient<IRepository<ApiScopes>, BaseRepository<ApiScopes>>();
        services.AddTransient<IRepository<ApiScopeClaims>, BaseRepository<ApiScopeClaims>>();

        services.AddTransient<IIdentityResourceRepository, IdentityResourceRepository>();
        services.AddTransient<IRepository<IdentityResources>, BaseRepository<IdentityResources>>();
        services.AddTransient<IRepository<IdentityClaims>, BaseRepository<IdentityClaims>>();

        services.AddTransient<IRepository<SecurityQuestions>, BaseRepository<SecurityQuestions>>();
        services.AddTransient<IUserManagementUnitOfWork, UserManagementUnitOfWork>();
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IRepository<ExternalIdentities>, BaseRepository<ExternalIdentities>>();
        services.AddTransient<IRepository<PasswordHistory>, BaseRepository<PasswordHistory>>();
        services.AddTransient<IRepository<SecurityTokens>, BaseRepository<SecurityTokens>>();
        services.AddTransient<IRepository<UserSecurityQuestions>, BaseRepository<UserSecurityQuestions>>();

        services.AddTransient<IRoleRepository, RoleRepository>();
        services.AddTransient<ISecurityTokenCommandRepository, SecurityTokenCommandRepository>();
        services.AddTransient<IDbConnectionValidator, DbConnectionValidator>();
        services.AddTransient<IUserRoleRepository, UserRoleRepository>();
        services.AddTransient<IRoleClaimsRepository, RoleClaimsRepository>();
        services.AddTransient<IRoleManagementUnitOfWork, RoleManagementUnitOfWork>();
        services.AddTransient<IUserTokenRepository, UserTokenRepository>();
        services.AddTransient<ISecurityTokenRepository, SecurityTokenRepository>();

        services.AddTransient<IUserClaimRepository, UserClaimRepository>();
        services.AddTransient<IClientsUnitOfWork, ClientsUnitOfWork>();
        services.AddTransient<IResourceUnitOfWork, ResourceUnitOfWork>();
        return services;
    }

    /// <summary>
    /// Configures ASP.NET Core Identity services including password policies, user requirements,
    /// lockout settings, custom store wrappers, and token providers from the application configuration.
    /// </summary>
    private static IServiceCollection AddIdentityServices(IServiceCollection services, HCLCSSFConfig config)
    {
        services.AddIdentity<Users, Roles>(options =>
            {
                options.Password.RequiredLength = config.SystemSettings.PasswordConfig.MinPasswordLength;
                options.Password.RequiredUniqueChars = config.SystemSettings.PasswordConfig.RequiredUniqueChars;
                options.Password.RequireDigit = config.SystemSettings.PasswordConfig.RequireDigit;
                options.Password.RequireLowercase = config.SystemSettings.PasswordConfig.RequireLowercase;
                options.Password.RequireNonAlphanumeric = config.SystemSettings.PasswordConfig.RequireSpecialChar;
                options.Password.RequireUppercase = config.SystemSettings.PasswordConfig.RequireUppercase;

                options.User.RequireUniqueEmail = config.SystemSettings.UserConfig.RequireUniqueEmail;

                options.SignIn.RequireConfirmedEmail = config.SystemSettings.UserConfig.RequireConfirmedEmail;
                options.SignIn.RequireConfirmedPhoneNumber =
                    config.SystemSettings.UserConfig.RequireConfirmedPhoneNumber;

                options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
                options.Tokens.ChangePhoneNumberTokenProvider = TokenOptions.DefaultPhoneProvider;
                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;

                options.Lockout.AllowedForNewUsers = config.SystemSettings.UserConfig.LockOutAllowedForNewUsers;
                options.Lockout.DefaultLockoutTimeSpan =
                    TimeSpan.FromMinutes(config.SystemSettings.UserConfig.DefaultLockoutTimeSpanMin);
                options.Lockout.MaxFailedAccessAttempts = config.SystemSettings.UserConfig.MaxFailedAccessAttempts;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddUserStore<UserStoreWrapper>()
            .AddRoleStore<RoleStoreWrapper>()
            .AddUserManager<UserManagerWrapper<Users>>()
            .AddRoleManager<RoleManagerWrapper<Roles>>()
            .AddSignInManager<SignInManagerWrapper<Users>>()
            .AddTokenProvider<EmailConfirmationTokenProvider<Users>>(TokenOptions.DefaultEmailProvider)
            .AddTokenProvider<ChangePhoneNumberTokenProvider<Users>>(TokenOptions.DefaultPhoneProvider)
            .AddTokenProvider<PasswordResetTokenProvider<Users>>(TokenOptions.DefaultEmailProvider)
            .AddTokenProvider<UserTokenProvider<Users>>(TokenOptions.DefaultProvider)
            .AddDefaultTokenProviders();
        return services;
    }
}
