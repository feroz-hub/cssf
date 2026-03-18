/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.Infrastructure.Services.Implementation;

namespace HCL.CS.SF.Infrastructure.Services.Extension;

/// <summary>
/// Manages named logger instances, allowing multiple independently configured loggers
/// to coexist within the application (e.g., separate loggers for different modules).
/// </summary>
public class LoggerInstance : ILoggerInstance
{
    /// <summary>
    /// Dictionary holding registered logger instances keyed by their instance name.
    /// </summary>
    private readonly Dictionary<string, ILoggerService> logInstanceCollection = new();

    /// <summary>
    /// Provides localized error message resolution for validation failures.
    /// </summary>
    private readonly IResourceStringHandler resourceStringHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggerInstance"/> class.
    /// </summary>
    /// <param name="resourceStringHandler">Handler for resolving localized resource strings.</param>
    public LoggerInstance(IResourceStringHandler resourceStringHandler)
    {
        this.resourceStringHandler = resourceStringHandler;
    }

    /// <summary>
    /// Initializes multiple logger instances from a list of log configurations.
    /// </summary>
    /// <param name="logConfig">The list of log configurations to initialize.</param>
    /// <exception cref="Exception">Thrown when the configuration list is null or empty.</exception>
    public void InitiateLoggerInstance(List<LogConfig> logConfig)
    {
        if (logConfig == null || logConfig.Count <= 0)
        {
            var errorMessage = resourceStringHandler.GetResourceString(ApiErrorCodes.LoggerConfigurationIsNull);
            throw new Exception(errorMessage);
        }

        // Initialize each logger configuration individually
        foreach (var logOption in logConfig) InitiateLoggerInstance(logOption);
    }

    /// <summary>
    /// Initializes a single logger instance from the provided configuration.
    /// </summary>
    /// <param name="logConfig">The log configuration specifying sink type, level, and instance name.</param>
    /// <exception cref="Exception">Thrown when the configuration is null.</exception>
    public void InitiateLoggerInstance(LogConfig logConfig)
    {
        if (logConfig == null)
        {
            var errorMessage = resourceStringHandler.GetResourceString(ApiErrorCodes.LoggerConfigurationIsNull);
            throw new Exception(errorMessage);
        }

        // Create a new LogService, configure it, and register under the instance name
        var loggerService = new LogService(resourceStringHandler);
        loggerService.InitializeConfiguration(logConfig);
        Register(logConfig.InstanceName, loggerService);
    }

    /// <summary>
    /// Retrieves a previously registered logger instance by its name.
    /// </summary>
    /// <param name="instanceName">The name of the logger instance to retrieve.</param>
    /// <returns>The <see cref="ILoggerService"/> if found; otherwise, <c>null</c>.</returns>
    public ILoggerService GetLoggerInstance(string instanceName)
    {
        if (instanceName != null && logInstanceCollection != null &&
            logInstanceCollection.ContainsKey(instanceName))
            return logInstanceCollection[instanceName];

        return null;
    }

    /// <summary>
    /// Registers a logger service under the specified name if not already registered.
    /// </summary>
    /// <param name="name">The unique instance name for the logger.</param>
    /// <param name="logService">The configured log service to register.</param>
    private void Register(string name, LogService logService)
    {
        if (logInstanceCollection != null && !logInstanceCollection.ContainsKey(name))
            logInstanceCollection.Add(name, logService);
    }
}
