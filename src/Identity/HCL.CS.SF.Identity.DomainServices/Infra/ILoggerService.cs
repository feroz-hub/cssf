/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Runtime.CompilerServices;
using HCL.CS.SF.Domain;

namespace HCL.CS.SF.DomainServices.Infra;

/// <summary>
/// Factory for creating and managing named <see cref="ILoggerService"/> instances.
/// Implementations initialize structured-logging sinks (e.g., Serilog) from configuration
/// and return per-module logger instances by name.
/// </summary>
public interface ILoggerInstance
{
    /// <summary>Initializes logger instances from a list of log configurations.</summary>
    /// <param name="logConfig">The logging configurations to apply.</param>
    void InitiateLoggerInstance(List<LogConfig> logConfig);

    /// <summary>Initializes a single logger instance from one configuration.</summary>
    /// <param name="logConfig">The logging configuration to apply.</param>
    void InitiateLoggerInstance(LogConfig logConfig);

    /// <summary>Retrieves a named logger instance.</summary>
    /// <param name="instanceName">The name identifying the logger instance.</param>
    /// <returns>The <see cref="ILoggerService"/> for the specified name.</returns>
    ILoggerService GetLoggerInstance(string instanceName);
}

/// <summary>
/// Structured logging service used throughout the framework. Supports multiple log levels,
/// structured property values, and automatic caller-information enrichment for diagnostics.
/// </summary>
public interface ILoggerService
{
    /// <summary>Sets the currently authenticated user name for log enrichment.</summary>
    /// <param name="userName">The username to include in subsequent log entries.</param>
    void SetLoggedUserName(string userName);

    /// <summary>Writes a log message at the specified level with optional structured property values.</summary>
    /// <param name="loggingOption">The log level/destination.</param>
    /// <param name="message">The message template.</param>
    /// <param name="propertyValues">Structured property values to embed in the message.</param>
    void WriteTo(Log loggingOption, string message, params object[] propertyValues);

    /// <summary>Writes a log message with an associated exception at the specified level.</summary>
    /// <param name="loggingOption">The log level/destination.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The message template.</param>
    /// <param name="propertyValues">Structured property values to embed in the message.</param>
    void WriteTo(Log loggingOption, Exception exception, string message, params object[] propertyValues);

    /// <summary>Writes a log message with auto-captured caller method and file information.</summary>
    /// <param name="loggingOption">The log level/destination.</param>
    /// <param name="message">The message template.</param>
    /// <param name="propertyValues">Structured property values to embed in the message.</param>
    /// <param name="callerMemberName">Auto-captured caller method name.</param>
    /// <param name="sourceFilePath">Auto-captured source file path.</param>
    void WriteToWithCaller(Log loggingOption, string message, object[] propertyValues = null,
        [CallerMemberName] string callerMemberName = null, [CallerFilePath] string sourceFilePath = null);

    /// <summary>Writes a log message with an exception and auto-captured caller information.</summary>
    /// <param name="loggingOption">The log level/destination.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The message template.</param>
    /// <param name="propertyValues">Structured property values to embed in the message.</param>
    /// <param name="callerMemberName">Auto-captured caller method name.</param>
    /// <param name="sourceFilePath">Auto-captured source file path.</param>
    void WriteToWithCaller(Log loggingOption, Exception exception, string message, object[] propertyValues = null,
        [CallerMemberName] string callerMemberName = null, [CallerFilePath] string sourceFilePath = null);
}
