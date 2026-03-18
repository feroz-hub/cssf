/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using HCL.CS.SF.Domain;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.Infrastructure.Services.Implementation;
using HCL.CS.SF.Infrastructure.Services.Implementation.Providers;
using HCL.CS.SF.Infrastructure.Services.Implementation.Providers.Email;
using HCL.CS.SF.Infrastructure.Services.Implementation.Providers.Sms;

namespace HCL.CS.SF.Infrastructure.Services.Extension;

/// <summary>
/// Provides extension methods for registering all infrastructure-layer services
/// (email, SMS, logging, tenant context, notification providers) into the DI container.
/// </summary>
public static class InfrastructureServiceExtension
{
    /// <summary>
    /// Registers core infrastructure services including email/SMS services, all notification
    /// providers, the provider factory, tenant context, and framework result handling.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<IFrameworkResultService, FrameworkResultService>();
        services.AddSingleton<ILoggerInstance, LoggerInstance>();
        services.AddScoped<ITenantContext, HttpTenantContext>();
        services.AddTransient<IEmailService, EmailService>();
        services.AddTransient<ISmsService, SmsService>();

        // Email Providers - each registered as IEmailProvider for factory resolution
        services.AddTransient<IEmailProvider, SmtpEmailProvider>();
        services.AddTransient<IEmailProvider, SendGridEmailProvider>();
        services.AddTransient<IEmailProvider, BrevoEmailProvider>();
        services.AddTransient<IEmailProvider, ResendEmailProvider>();
        services.AddTransient<IEmailProvider, AmazonSesEmailProvider>();
        services.AddTransient<IEmailProvider, MailgunEmailProvider>();
        services.AddTransient<IEmailProvider, PostmarkEmailProvider>();

        // SMS Providers - each registered as ISmsProvider for factory resolution
        services.AddTransient<ISmsProvider, TwilioSmsProvider>();
        services.AddTransient<ISmsProvider, BrevoSmsProvider>();
        services.AddTransient<ISmsProvider, VonageSmsProvider>();
        services.AddTransient<ISmsProvider, AmazonSnsSmsProvider>();
        services.AddTransient<ISmsProvider, MessageBirdSmsProvider>();
        services.AddTransient<ISmsProvider, PlivoSmsProvider>();

        // Provider Factory - resolves the correct provider by name at runtime
        services.AddTransient<NotificationProviderFactory>();

        return services;
    }

    /// <summary>
    /// Initializes and registers a security logger instance using the provided log configuration.
    /// </summary>
    /// <param name="services">The service collection to add the logger to.</param>
    /// <param name="serviceProvider">The service provider used to resolve the logger instance.</param>
    /// <param name="logConfig">The configuration specifying log sink, level, and output settings.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddSecurityLoggerInstance(this IServiceCollection services,
        IServiceProvider serviceProvider, LogConfig logConfig)
    {
        var loggerInstance = serviceProvider.GetService<ILoggerInstance>();
        if (loggerInstance != null)
        {
            loggerInstance.InitiateLoggerInstance(logConfig);
            services.AddSingleton(loggerInstance);
        }

        return services;
    }
}
