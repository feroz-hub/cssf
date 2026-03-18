/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.DirectoryServices.Protocols;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.Hosting.Health;
using HCL.CS.SF.Infrastructure.Data.Extension;
using HCL.CS.SF.Infrastructure.Data.Validation;
using HCL.CS.SF.Infrastructure.Resources.Extension;
using HCL.CS.SF.Infrastructure.Services.Extension;
using HCL.CS.SF.ProxyService.Extension;
using HCL.CS.SF.Service.Extension;

namespace HCL.CS.SF.Hosting.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IServiceCollection"/> to register the full
/// HCL.CS.SF Identity service graph -- configuration binding, infrastructure services,
/// distributed cache, health checks, asymmetric key stores, and logging.
/// </summary>
public static class HCLCSSFExtension
{
    /// <summary>
    /// Accumulates validation errors discovered during startup configuration validation.
    /// Thrown as an <see cref="AggregateException"/> if any errors are present.
    /// </summary>
    private static readonly List<Exception> ErrorList = new();

    /// <summary>
    /// Registers the HCL.CS.SF Identity services using pre-built settings objects.
    /// This overload is intended for hosts that have already deserialized their configuration.
    /// </summary>
    /// <param name="services">The DI service collection to populate.</param>
    /// <param name="systemSettings">Core system settings (database, LDAP, email, SMS, logging).</param>
    /// <param name="tokenSettings">OAuth 2.0 / OIDC token configuration (issuer, URLs, lifetimes).</param>
    /// <param name="templateSettings">Notification template settings for email/SMS content.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddHCLCSSF(
        this IServiceCollection services,
        SystemSettings systemSettings,
        TokenSettings tokenSettings,
        NotificationTemplateSettings templateSettings)
    {
        // Bundle the three settings objects into a single configuration container.
        var HCLCSSFConfig = new HCLCSSFConfig
        {
            SystemSettings = systemSettings,
            NotificationTemplateSettings = templateSettings,
            TokenSettings = tokenSettings
        };
        return services.AddSecurityExtensions(HCLCSSFConfig);
    }

    /// <summary>
    /// Registers the HCL.CS.SF Identity services by deserializing configuration from JSON
    /// file paths. This overload is useful when configuration lives in standalone JSON files
    /// rather than the standard <c>appsettings.json</c> pipeline.
    /// </summary>
    /// <param name="services">The DI service collection to populate.</param>
    /// <param name="systemSettingsJsonPath">Absolute path to the SystemSettings JSON file.</param>
    /// <param name="tokenConfigSettingsJsonPath">Absolute path to the TokenSettings JSON file.</param>
    /// <param name="notificationTemplateSettingsJsonPath">Absolute path to the NotificationTemplateSettings JSON file.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddHCLCSSF(
        this IServiceCollection services,
        string systemSettingsJsonPath,
        string tokenConfigSettingsJsonPath,
        string notificationTemplateSettingsJsonPath)
    {
        var HCLCSSFConfig = DeserializeConfiguration(systemSettingsJsonPath, notificationTemplateSettingsJsonPath,
            tokenConfigSettingsJsonPath);
        return services.AddSecurityExtensions(HCLCSSFConfig);
    }

    /// <summary>
    /// Registers asymmetric (RSA/EC) signing keys used for JWT token signing and validation.
    /// </summary>
    /// <param name="services">The DI service collection to populate.</param>
    /// <param name="securityKeys">The collection of asymmetric key descriptors to register.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    /// <exception cref="AggregateException">Thrown when <paramref name="securityKeys"/> is <c>null</c>.</exception>
    public static IServiceCollection AddAsymmetricKeystore(this IServiceCollection services,
        IEnumerable<AsymmetricKeyInfoModel> securityKeys)
    {
        if (securityKeys == null) throw new AggregateException("Security keys is null or invalid");

        // Delegate to the core security layer to store and wire up the key material.
        services.AddSecurityAsymmetricKeystore(securityKeys);
        return services;
    }

    /// <summary>
    /// Registers a standalone logger instance for scenarios where logging is configured
    /// independently from the main HCL.CS.SF service graph.
    /// </summary>
    /// <param name="services">The DI service collection to populate.</param>
    /// <param name="logConfig">The logging configuration (database or file sink).</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    /// <exception cref="AggregateException">Thrown when the log configuration is invalid.</exception>
    public static IServiceCollection AddLoggerInstance(this IServiceCollection services, LogConfig logConfig)
    {
        ErrorList.Clear();
        var dbConnectionValidator = new DbConnectionValidator();

        // Validate the log configuration; isHCLCSSFInstance = false for standalone loggers.
        ValidateLogConfiguration(logConfig, false, dbConnectionValidator);

        if (ErrorList.Count > 0) throw new AggregateException("Multiple Errors Occurred", ErrorList);

        // Build a temporary service provider to resolve dependencies needed by the logger factory.
        using var serviceProvider = services.BuildServiceProvider();
        services.AddSecurityLoggerInstance(serviceProvider, logConfig);
        return services;
    }

    /// <summary>
    /// Core DI registration method that validates configuration, registers all infrastructure
    /// layers in the correct order, wires up health checks, and initializes the logger.
    /// </summary>
    /// <param name="services">The DI service collection to populate.</param>
    /// <param name="HCLCSSFConfig">The fully populated configuration container.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    private static IServiceCollection AddSecurityExtensions(this IServiceCollection services, HCLCSSFConfig HCLCSSFConfig)
    {
        // Validate all configuration sections (DB, LDAP, email, SMS, token, logging).
        ValidateConfiguration(HCLCSSFConfig);

        // Setting HCL.CS.SF instance name.
        HCLCSSFConfig.SystemSettings.LogConfig.InstanceName = LoggerKeyConstants.DefaultLoggerKey;

        // Register services in dependency order. Do not change this order -- downstream
        // registrations depend on services registered by earlier calls.
        services.AddConfiguration(HCLCSSFConfig);        // Bind configuration POCOs into DI
        services.AddAutoMapper();                        // Register AutoMapper profiles
        services.AddInfrastructureResources();           // Localization / resource strings
        services.AddUtilityServices();                   // Cross-cutting utility services
        services.AddDistributedCacheFromConfig();        // Redis or in-memory distributed cache
        services.AddIdentityConfiguration();             // ASP.NET Core Identity + EF stores
        services.AddRepository();                        // Data-access repositories
        services.AddInfrastructureServices();            // Infrastructure-layer service implementations
        services.AddCoreServices();                      // Domain / application-layer services
        services.AddWrappers();                          // Thin wrappers over external libraries
        services.AddDefaultEndpoints();                  // Default OAuth 2.0 / OIDC endpoint handlers
        services.AddProxyServices();                     // Reverse-proxy service registrations
        services.AddProxyValidator();                    // Proxy request validation
        services.AddProxyRoutes();                       // Proxy route table

        // Register health checks: a lightweight self-check for liveness and dependency
        // checks (database + cache) for readiness probes.
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
            .AddCheck<DatabaseDependencyHealthCheck>("database", tags: new[] { "ready" })
            .AddCheck<CacheDependencyHealthCheck>("cache", tags: new[] { "ready" });

        // Build a temporary provider to initialize the structured logger instance.
        using var serviceProvider = services.BuildServiceProvider();
        services.AddSecurityLoggerInstance(serviceProvider, HCLCSSFConfig.SystemSettings.LogConfig);

        return services;
    }

    /// <summary>
    /// Registers a distributed cache implementation based on environment variables.
    /// When <c>HCL.CS.SF_REDIS_CONNECTION_STRING</c> is set, a Redis-backed cache is used;
    /// otherwise an in-memory cache is registered as a fallback.
    /// </summary>
    /// <param name="services">The DI service collection to populate.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    private static IServiceCollection AddDistributedCacheFromConfig(this IServiceCollection services)
    {
        // Check for Redis connection string in environment variables.
        var redisConnection = Environment.GetEnvironmentVariable("HCL.CS.SF_REDIS_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            // Use the optional instance name env var, defaulting to "HCL.CS.SF:".
            var instanceName = Environment.GetEnvironmentVariable("HCL.CS.SF_REDIS_INSTANCE_NAME") ?? "HCL.CS.SF:";

            // Register StackExchange.Redis as the IDistributedCache implementation.
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = instanceName;
            });
        }
        else
        {
            // Fallback: register an in-memory distributed cache (suitable for single-node deployments).
            services.AddDistributedMemoryCache();
        }

        return services;
    }

    /// <summary>
    /// Deserializes the three required JSON configuration files into their respective
    /// settings objects and bundles them into an <see cref="HCLCSSFConfig"/> instance.
    /// </summary>
    /// <param name="systemSettingsJsonPath">Path to the SystemSettings JSON file.</param>
    /// <param name="notificationTemplateSettingsJsonPath">Path to the NotificationTemplateSettings JSON file.</param>
    /// <param name="tokenConfigSettingsJsonPath">Path to the TokenSettings JSON file.</param>
    /// <returns>A fully populated <see cref="HCLCSSFConfig"/> configuration container.</returns>
    /// <exception cref="ArgumentException">Thrown when any file path is empty or the file does not exist.</exception>
    private static HCLCSSFConfig DeserializeConfiguration(
        string systemSettingsJsonPath,
        string notificationTemplateSettingsJsonPath,
        string tokenConfigSettingsJsonPath)
    {
        // Validate that each configuration file exists before attempting to load.
        if (string.IsNullOrWhiteSpace(systemSettingsJsonPath) || !File.Exists(systemSettingsJsonPath.Trim()))
            throw new ArgumentException("System Setting Configuration file not found");

        if (string.IsNullOrWhiteSpace(notificationTemplateSettingsJsonPath) ||
            !File.Exists(notificationTemplateSettingsJsonPath.Trim()))
            throw new ArgumentException("Notification template Configuration file not found");

        if (string.IsNullOrWhiteSpace(tokenConfigSettingsJsonPath) || !File.Exists(tokenConfigSettingsJsonPath.Trim()))
            throw new ArgumentException("Token Configuration file not found");

        // Build a merged IConfiguration from all three JSON files.
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile(systemSettingsJsonPath)
            .AddJsonFile(notificationTemplateSettingsJsonPath)
            .AddJsonFile(tokenConfigSettingsJsonPath)
            .Build();

        // Bind each configuration section to its strongly-typed POCO.
        var systemSettings = new SystemSettings();
        configuration.GetSection("SystemSettings").Bind(systemSettings);

        var templateSettings = new NotificationTemplateSettings();
        configuration.GetSection("NotificationTemplateSettings").Bind(templateSettings);

        var tokenSettings = new TokenSettings();
        configuration.GetSection("TokenSettings").Bind(tokenSettings);

        var HCLCSSFConfig = new HCLCSSFConfig
        {
            SystemSettings = systemSettings,
            NotificationTemplateSettings = templateSettings,
            TokenSettings = tokenSettings
        };
        return HCLCSSFConfig;
    }

    /// <summary>
    /// Orchestrates validation of all configuration sub-sections. Collects errors into
    /// <see cref="ErrorList"/> and throws an <see cref="AggregateException"/> if any are found.
    /// </summary>
    /// <param name="HCLCSSFConfig">The configuration container to validate.</param>
    private static void ValidateConfiguration(HCLCSSFConfig HCLCSSFConfig)
    {
        ErrorList.Clear();
        var dbConnectionValidator = new DbConnectionValidator();

        // Validate each configuration sub-section independently to collect all errors at once.
        ValidateDatabaseConfiguration(HCLCSSFConfig, dbConnectionValidator);
        ValidateLdapConfiguration(HCLCSSFConfig);
        ValidateEmailConfiguration(HCLCSSFConfig);
        ValidateSmsConfiguration(HCLCSSFConfig);
        ValidateTokenConfiguration(HCLCSSFConfig);
        ValidateLogConfiguration(HCLCSSFConfig.SystemSettings.LogConfig, true, dbConnectionValidator);

        if (ErrorList.Count > 0) throw new AggregateException("Multiple Errors Occurred", ErrorList);
    }

    /// <summary>
    /// Validates the primary database configuration (provider type and connection string)
    /// and tests the connection using the provided validator.
    /// </summary>
    /// <param name="HCLCSSFConfig">The configuration container whose DB settings are validated.</param>
    /// <param name="dbConnectionValidator">Validator that tests actual connectivity for the configured provider.</param>
    private static void ValidateDatabaseConfiguration(HCLCSSFConfig HCLCSSFConfig, IDbConnectionValidator dbConnectionValidator)
    {
        if (HCLCSSFConfig.SystemSettings.DBConfig.Database <= 0)
            ErrorList.Add(new Exception("Database name is null or invalid"));

        if (string.IsNullOrWhiteSpace(HCLCSSFConfig.SystemSettings.DBConfig.DBConnectionString))
            ErrorList.Add(new Exception("Database connection string is not configured"));
        else
        {
            // Attempt to open a real connection to surface driver-level errors at startup.
            var exception = dbConnectionValidator.Validate(
                HCLCSSFConfig.SystemSettings.DBConfig.Database,
                HCLCSSFConfig.SystemSettings.DBConfig.DBConnectionString);
            if (exception != null) ErrorList.Add(exception);
        }
    }

    /// <summary>
    /// Validates the LDAP configuration by attempting a bind operation against the configured
    /// LDAP server. Sets <see cref="GlobalConfiguration.IsLdapConfigurationValid"/> on success.
    /// Skipped silently when LDAP host/port are not configured.
    /// </summary>
    /// <param name="HCLCSSFConfig">The configuration container whose LDAP settings are validated.</param>
    private static void ValidateLdapConfiguration(HCLCSSFConfig HCLCSSFConfig)
    {
        if (!string.IsNullOrWhiteSpace(HCLCSSFConfig.SystemSettings.LdapConfig.LdapHostName) &&
            HCLCSSFConfig.SystemSettings.LdapConfig.LdapPort > 0)
            try
            {
                if (string.IsNullOrWhiteSpace(HCLCSSFConfig.SystemSettings.LdapConfig.LdapDomainName))
                    ErrorList.Add(new Exception("Ldap domain name is not configured"));

                // Build an LDAP directory identifier from the configured host and port.
                var serverId = new LdapDirectoryIdentifier(
                    HCLCSSFConfig.SystemSettings.LdapConfig.LdapHostName,
                    HCLCSSFConfig.SystemSettings.LdapConfig.LdapPort);
                using var ldapConnection = new LdapConnection(serverId);

                // Use LDAPv3 protocol for compatibility with modern directory servers.
                ldapConnection.SessionOptions.ProtocolVersion = 3;

                // Configure SSL and auth type based on the secure-connection flag.
                if (HCLCSSFConfig.SystemSettings.LdapConfig.IsSecureConnection)
                {
                    ldapConnection.AuthType = AuthType.External;
                    ldapConnection.SessionOptions.SecureSocketLayer = true;
                }
                else
                {
                    ldapConnection.AuthType = AuthType.Basic;
                    ldapConnection.SessionOptions.SecureSocketLayer = false;
                }

                // Attempt an anonymous bind to verify the server is reachable.
                ldapConnection.Bind();

                GlobalConfiguration.IsLdapConfigurationValid = true;
                if (ldapConnection != null) ldapConnection.Dispose();
            }
            catch (Exception ex)
            {
                ErrorList.Add(ex);
            }
    }

    /// <summary>
    /// Validates the email (SMTP) configuration by attempting a connection to the SMTP server.
    /// Sets <see cref="GlobalConfiguration.IsEmailConfigurationValid"/> on success.
    /// Skipped silently when email settings are incomplete.
    /// </summary>
    /// <param name="HCLCSSFConfig">The configuration container whose email settings are validated.</param>
    private static void ValidateEmailConfiguration(HCLCSSFConfig HCLCSSFConfig)
    {
        if (!string.IsNullOrWhiteSpace(HCLCSSFConfig.SystemSettings.EmailConfig.SmtpServer) &&
            !string.IsNullOrWhiteSpace(HCLCSSFConfig.SystemSettings.EmailConfig.UserName) &&
            !string.IsNullOrWhiteSpace(HCLCSSFConfig.SystemSettings.EmailConfig.Password) &&
            HCLCSSFConfig.SystemSettings.EmailConfig.Port > 0)
            try
            {
                if (!ShouldActivelyValidateExternalProvidersOnStartup())
                {
                    GlobalConfiguration.IsEmailConfigurationValid = true;
                    return;
                }

                using var client = new SmtpClient();
                using var validationTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));

                // Connect with full SSL or StartTLS depending on the configuration flag.
                if (HCLCSSFConfig.SystemSettings.EmailConfig.SecureSocketOptions)
                    client.ConnectAsync(
                        HCLCSSFConfig.SystemSettings.EmailConfig.SmtpServer,
                        HCLCSSFConfig.SystemSettings.EmailConfig.Port,
                        true,
                        validationTimeout.Token).GetAwaiter().GetResult();
                else
                    client.ConnectAsync(
                        HCLCSSFConfig.SystemSettings.EmailConfig.SmtpServer,
                        HCLCSSFConfig.SystemSettings.EmailConfig.Port,
                        SecureSocketOptions.StartTls,
                        validationTimeout.Token).GetAwaiter().GetResult();

                GlobalConfiguration.IsEmailConfigurationValid = true;

                // Gracefully disconnect if the SMTP handshake succeeded.
                if (client.IsConnected)
                {
                    client.DisconnectAsync(true, validationTimeout.Token).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                ErrorList.Add(ex);
            }
    }

    /// <summary>
    /// Validates the SMS (Twilio) configuration by checking that all required fields are
    /// present. Sets <see cref="GlobalConfiguration.IsSmsConfigurationValid"/> on success.
    /// Skipped silently when SMS settings are incomplete.
    /// </summary>
    /// <param name="HCLCSSFConfig">The configuration container whose SMS settings are validated.</param>
    private static void ValidateSmsConfiguration(HCLCSSFConfig HCLCSSFConfig)
    {
        if (!string.IsNullOrWhiteSpace(HCLCSSFConfig.SystemSettings.SMSConfig.SMSAccountIdentification) &&
            !string.IsNullOrWhiteSpace(HCLCSSFConfig.SystemSettings.SMSConfig.SMSAccountPassword) &&
            !string.IsNullOrWhiteSpace(HCLCSSFConfig.SystemSettings.SMSConfig.SMSAccountFrom) &&
            !string.IsNullOrWhiteSpace(HCLCSSFConfig.SystemSettings.SMSConfig.SMSStatusCallbackURL))
            try
            {
                // All required SMS fields are present; mark configuration as valid.
                GlobalConfiguration.IsSmsConfigurationValid = true;
            }
            catch (Exception ex)
            {
                ErrorList.Add(ex);
            }
    }

    /// <summary>
    /// Validates the OAuth 2.0 / OIDC token configuration, ensuring that the issuer URI and
    /// user-interaction URLs (login, logout, error) are set. In production environments,
    /// additionally rejects localhost-style issuer URIs.
    /// </summary>
    /// <param name="HCLCSSFConfig">The configuration container whose token settings are validated.</param>
    private static void ValidateTokenConfiguration(HCLCSSFConfig HCLCSSFConfig)
    {
        if (string.IsNullOrWhiteSpace(HCLCSSFConfig.TokenSettings.TokenConfig.IssuerUri))
            ErrorList.Add(new Exception("IssuerUri is not configured"));
        else if (IsProductionEnvironment() && LooksLikeLocalIssuer(HCLCSSFConfig.TokenSettings.TokenConfig.IssuerUri))
            ErrorList.Add(new Exception(
                $"IssuerUri '{HCLCSSFConfig.TokenSettings.TokenConfig.IssuerUri}' is not valid for Production. Configure TokenSettings__TokenConfig__IssuerUri with the public Railway domain."));

        if (string.IsNullOrWhiteSpace(HCLCSSFConfig.TokenSettings.UserInteractionConfig.LoginUrl))
            ErrorList.Add(new Exception("LoginUrl is not configured"));

        if (string.IsNullOrWhiteSpace(HCLCSSFConfig.TokenSettings.UserInteractionConfig.LogoutUrl))
            ErrorList.Add(new Exception("LogoutUrl is not configured"));

        if (string.IsNullOrWhiteSpace(HCLCSSFConfig.TokenSettings.UserInteractionConfig.ErrorUrl))
            ErrorList.Add(new Exception("ErrorUrl is not configured"));
    }

    /// <summary>
    /// Determines whether the current hosting environment is Production by inspecting
    /// the <c>ASPNETCORE_ENVIRONMENT</c> or <c>DOTNET_ENVIRONMENT</c> environment variables.
    /// </summary>
    /// <returns><c>true</c> if the environment is "Production"; otherwise <c>false</c>.</returns>
    private static bool IsProductionEnvironment()
    {
        var environment =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

        return string.Equals(environment, "Production", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks whether the given issuer URI points to a loopback address (localhost, 127.0.0.1,
    /// or ::1), which is invalid for production deployments.
    /// </summary>
    /// <param name="issuerUri">The issuer URI to inspect.</param>
    /// <returns><c>true</c> if the URI resolves to a loopback address; otherwise <c>false</c>.</returns>
    private static bool LooksLikeLocalIssuer(string issuerUri)
    {
        if (!Uri.TryCreate(issuerUri, UriKind.Absolute, out var issuer))
        {
            return false;
        }

        return issuer.IsLoopback
            || string.Equals(issuer.Host, "localhost", StringComparison.OrdinalIgnoreCase)
            || string.Equals(issuer.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(issuer.Host, "::1", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether startup should actively open external provider connections
    /// (for example SMTP) instead of only validating that required configuration is present.
    /// This is opt-in so local/dev startup is not blocked by external network dependencies.
    /// </summary>
    /// <returns><c>true</c> when external provider connectivity should be tested during startup.</returns>
    private static bool ShouldActivelyValidateExternalProvidersOnStartup()
    {
        var validateExternalProviders =
            Environment.GetEnvironmentVariable("HCL.CS.SF_VALIDATE_EXTERNAL_PROVIDERS_ON_STARTUP");

        return string.Equals(validateExternalProviders, "true", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates the logging configuration, dispatching to either database or file
    /// validation based on the configured <see cref="LogConfig.WriteLogTo"/> setting.
    /// </summary>
    /// <param name="logConfig">The logging configuration to validate.</param>
    /// <param name="isHCLCSSFInstance">
    /// When <c>true</c>, indicates this is the main HCL.CS.SF logger (allows defaulting
    /// the file path). When <c>false</c>, the caller is a standalone logger registration.
    /// </param>
    /// <param name="dbConnectionValidator">Validator used to test database connectivity for DB-based logging.</param>
    private static void ValidateLogConfiguration(LogConfig logConfig, bool isHCLCSSFInstance,
        IDbConnectionValidator dbConnectionValidator)
    {
        if (logConfig.WriteLogTo == WriteLogTo.DataBase)
        {
            // Validate the logging database connection string and provider.
            ValidateLoggerDatabaseConfiguration(logConfig, dbConnectionValidator);
        }
        else if (logConfig.WriteLogTo == WriteLogTo.File)
        {
            // Default the log file path for the main HCL.CS.SF instance when not explicitly set.
            if (isHCLCSSFInstance && string.IsNullOrWhiteSpace(logConfig.LogFileConfig.FilePath))
                logConfig.LogFileConfig.FilePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Logs\\HCL.CS.SF.txt");

            ValidateLoggerFileConfiguration(logConfig);
        }
    }

    /// <summary>
    /// Validates the database-sink logging configuration (provider type, connection string)
    /// and tests actual connectivity.
    /// </summary>
    /// <param name="logConfig">The logging configuration whose database sink is validated.</param>
    /// <param name="dbConnectionValidator">Validator that tests actual connectivity for the configured provider.</param>
    private static void ValidateLoggerDatabaseConfiguration(LogConfig logConfig,
        IDbConnectionValidator dbConnectionValidator)
    {
        if (logConfig.LogDbConfig.Database <= 0)
            ErrorList.Add(new Exception("Database name specified in logconfig is null or invalid"));

        if (string.IsNullOrWhiteSpace(logConfig.LogDbConfig.ConnectionString))
            ErrorList.Add(new Exception("Database connection string specified in logconfig is null or invalid"));
        else
        {
            // Attempt a real connection to surface driver-level errors at startup.
            var exception = dbConnectionValidator.Validate(logConfig.LogDbConfig.Database,
                logConfig.LogDbConfig.ConnectionString);
            if (exception != null) ErrorList.Add(exception);
        }
    }

    /// <summary>
    /// Validates the file-sink logging configuration by checking that the file path is set
    /// and the target directory exists on disk.
    /// </summary>
    /// <param name="logConfig">The logging configuration whose file sink is validated.</param>
    private static void ValidateLoggerFileConfiguration(LogConfig logConfig)
    {
        if (string.IsNullOrWhiteSpace(logConfig.LogFileConfig.FilePath))
        {
            ErrorList.Add(new Exception("File path specified in logconfig is null or invalid"));
        }
        else
        {
            // Verify the parent directory exists so the logger can create the log file.
            var directoryName = Path.GetDirectoryName(logConfig.LogFileConfig.FilePath);
            if (!Directory.Exists(directoryName))
                ErrorList.Add(new Exception("File path specified in logconfig is not exists"));
        }
    }
}
