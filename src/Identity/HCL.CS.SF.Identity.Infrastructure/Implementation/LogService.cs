/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Collections.ObjectModel;
using System.Data;
using System.Runtime.CompilerServices;
using NpgsqlTypes;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Sinks.MariaDB.Extensions;
using Serilog.Sinks.MSSqlServer;
using Serilog.Sinks.PostgreSQL;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.DomainServices.Infra;
using ColumnOptions = Serilog.Sinks.MSSqlServer.ColumnOptions;
using Log = HCL.CS.SF.Domain.Log;

namespace HCL.CS.SF.Infrastructure.Services.Implementation;

/// <summary>
/// Serilog-based logging service that supports writing to file, SQL Server, MySQL, and PostgreSQL sinks.
/// Provides standard and caller-enriched log methods across all severity levels.
/// </summary>
public class LogService : ILoggerService
{
    /// <summary>
    /// Provides localized error message resolution for configuration validation failures.
    /// </summary>
    private readonly IResourceStringHandler resourceStringHandler;

    /// <summary>
    /// The underlying Serilog logger instance created during configuration initialization.
    /// </summary>
    private ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogService"/> class.
    /// </summary>
    /// <param name="resourceStringHandler">Handler for resolving localized resource strings.</param>
    public LogService(IResourceStringHandler resourceStringHandler)
    {
        this.resourceStringHandler = resourceStringHandler;
    }

    /// <summary>
    /// Pushes the authenticated user's name into the Serilog log context as the "UserId" property.
    /// </summary>
    /// <param name="userName">The username to include in subsequent log entries.</param>
    public void SetLoggedUserName(string userName)
    {
        LogContext.PushProperty("UserId", userName);
    }

    /// <summary>
    /// Writes a structured log message at the specified severity level.
    /// </summary>
    /// <param name="loggingOption">The severity level (Debug, Error, Information, Warning, Fatal, Verbose).</param>
    /// <param name="message">The message template with optional structured placeholders.</param>
    /// <param name="propertyValues">Values to substitute into the message template placeholders.</param>
    public void WriteTo(Log loggingOption, string message, params object[] propertyValues)
    {
        switch (loggingOption)
        {
            case Log.Debug:
                logger.Debug(message, propertyValues);
                break;
            case Log.Error:
                logger.Error(message, propertyValues);
                break;
            case Log.Information:
                logger.Information(message, propertyValues);
                break;
            case Log.Warning:
                logger.Warning(message, propertyValues);
                break;
            case Log.Fatal:
                logger.Fatal(message, propertyValues);
                break;
            case Log.Verbose:
                logger.Verbose(message, propertyValues);
                break;
        }
    }

    /// <summary>
    /// Writes a structured log message with an associated exception at the specified severity level.
    /// </summary>
    /// <param name="loggingOption">The severity level (Debug, Error, Information, Warning, Fatal, Verbose).</param>
    /// <param name="exception">The exception to include in the log entry.</param>
    /// <param name="message">The message template with optional structured placeholders.</param>
    /// <param name="propertyValues">Values to substitute into the message template placeholders.</param>
    public void WriteTo(Log loggingOption, Exception exception, string message, params object[] propertyValues)
    {
        switch (loggingOption)
        {
            case Log.Debug:
                logger.Debug(exception, message, propertyValues);
                break;
            case Log.Error:
                logger.Error(exception, message, propertyValues);
                break;
            case Log.Information:
                logger.Information(exception, message, propertyValues);
                break;
            case Log.Warning:
                logger.Warning(exception, message, propertyValues);
                break;
            case Log.Fatal:
                logger.Fatal(exception, message, propertyValues);
                break;
            case Log.Verbose:
                logger.Verbose(exception, message, propertyValues);
                break;
        }
    }

    /// <summary>
    /// Writes a structured log message enriched with the calling method name and source file name.
    /// </summary>
    /// <param name="loggingOption">The severity level.</param>
    /// <param name="message">The message template.</param>
    /// <param name="propertyValues">Values to substitute into the message template.</param>
    /// <param name="callerMemberName">Automatically captured caller method name.</param>
    /// <param name="sourceFilePath">Automatically captured source file path.</param>
    public void WriteToWithCaller(
        Log loggingOption,
        string message,
        object[] propertyValues = null,
        [CallerMemberName] string callerMemberName = null,
        [CallerFilePath] string sourceFilePath = null)
    {
        // Extract just the file name from the full path for concise log output
        var fileName = Path.GetFileName(sourceFilePath);
        switch (loggingOption)
        {
            case Log.Debug:
                logger.ForContext("MethodName", callerMemberName).ForContext("FileName", fileName)
                    .Debug(message, propertyValues);
                break;
            case Log.Error:
                logger.ForContext("MethodName", callerMemberName).ForContext("FileName", fileName)
                    .Error(message, propertyValues);
                break;
            case Log.Information:
                logger.ForContext("MethodName", callerMemberName).ForContext("FileName", fileName)
                    .Information(message, propertyValues);
                break;
            case Log.Warning:
                logger.ForContext("MethodName", callerMemberName).ForContext("FileName", fileName)
                    .Warning(message, propertyValues);
                break;
            case Log.Fatal:
                logger.ForContext("MethodName", callerMemberName).ForContext("FileName", fileName)
                    .Fatal(message, propertyValues);
                break;
            case Log.Verbose:
                logger.ForContext("MethodName", callerMemberName).ForContext("FileName", fileName)
                    .Verbose(message, propertyValues);
                break;
        }
    }

    /// <summary>
    /// Writes a structured log message with an exception, enriched with the calling method name and source file name.
    /// </summary>
    /// <param name="loggingOption">The severity level.</param>
    /// <param name="exception">The exception to include in the log entry.</param>
    /// <param name="message">The message template.</param>
    /// <param name="propertyValues">Values to substitute into the message template.</param>
    /// <param name="callerMemberName">Automatically captured caller method name.</param>
    /// <param name="sourceFilePath">Automatically captured source file path.</param>
    public void WriteToWithCaller(
        Log loggingOption,
        Exception exception,
        string message,
        object[] propertyValues = null,
        [CallerMemberName] string callerMemberName = null,
        [CallerFilePath] string sourceFilePath = null)
    {
        var fileName = Path.GetFileName(sourceFilePath);
        switch (loggingOption)
        {
            case Log.Debug:
                logger.ForContext("MethodName", callerMemberName).ForContext("FileName", fileName)
                    .Debug(exception, message, propertyValues);
                break;
            case Log.Error:
                logger.ForContext("MethodName", callerMemberName).ForContext("FileName", fileName)
                    .Error(exception, message, propertyValues);
                break;
            case Log.Information:
                logger.ForContext("MethodName", callerMemberName).ForContext("FileName", fileName)
                    .Information(exception, message, propertyValues);
                break;
            case Log.Warning:
                logger.ForContext("MethodName", callerMemberName).ForContext("FileName", fileName)
                    .Warning(exception, message, propertyValues);
                break;
            case Log.Fatal:
                logger.ForContext("MethodName", callerMemberName).ForContext("FileName", fileName)
                    .Fatal(exception, message, propertyValues);
                break;
            case Log.Verbose:
                logger.ForContext("MethodName", callerMemberName).ForContext("FileName", fileName)
                    .Verbose(exception, message, propertyValues);
                break;
        }
    }

    /// <summary>
    /// Initializes the Serilog logger from the provided configuration, setting up the appropriate
    /// sink (file or database) and minimum log level.
    /// </summary>
    /// <param name="logConfig">The log configuration specifying sink type, connection details, and log levels.</param>
    /// <exception cref="Exception">Thrown when required configuration values (connection string, file path) are missing.</exception>
    internal void InitializeConfiguration(LogConfig logConfig)
    {
        var loggerConfiguration = new LoggerConfiguration();
        if (logConfig.WriteLogTo == WriteLogTo.DataBase)
        {
            SetMinimumLevel(loggerConfiguration, logConfig.LogDbConfig.MinimumConfiguration);
            if (string.IsNullOrWhiteSpace(logConfig.LogDbConfig.ConnectionString))
            {
                var errorMessage = resourceStringHandler.GetResourceString(ApiErrorCodes.ConnectionStringInvalid);
                throw new Exception(errorMessage);
            }

            LogDataBaseSetup(loggerConfiguration, logConfig.LogDbConfig);
        }
        else
        {
            SetMinimumLevel(loggerConfiguration, logConfig.LogFileConfig.MinimumConfiguration);
            if (string.IsNullOrWhiteSpace(logConfig.LogFileConfig.FilePath))
            {
                var errorMessage = resourceStringHandler.GetResourceString(ApiErrorCodes.InvalidLogFilePath);
                throw new Exception(errorMessage);
            }

            LogFileSetup(loggerConfiguration, logConfig.LogFileConfig);
        }

        logger = loggerConfiguration.CreateLogger();
    }

    /// <summary>
    /// Configures Serilog to write logs asynchronously to a rolling file sink.
    /// </summary>
    /// <param name="loggerConfiguration">The Serilog configuration builder.</param>
    /// <param name="logConfig">File sink settings including path, rolling interval, and size limits.</param>
    private void LogFileSetup(LoggerConfiguration loggerConfiguration, LogFileConfig logConfig)
    {
        loggerConfiguration.WriteTo.Async(a =>
            a.File(
                logConfig.FilePath,
                rollingInterval: (RollingInterval)logConfig.RollingIntervalType,
                rollOnFileSizeLimit: logConfig.SetLogFileSize,
                fileSizeLimitBytes: logConfig.FileSizeInBytes,
                restrictedToMinimumLevel: (LogEventLevel)logConfig.RestrictedToMinimumLevel,
                outputTemplate: logConfig.OutputFormat,
                shared: true));
    }

    /// <summary>
    /// Routes database sink setup to the appropriate provider-specific method (SQL Server, MySQL, or PostgreSQL).
    /// </summary>
    /// <param name="loggerConfiguration">The Serilog configuration builder.</param>
    /// <param name="logConfig">Database sink settings including provider type, connection string, and log levels.</param>
    private void LogDataBaseSetup(LoggerConfiguration loggerConfiguration, LogDbConfig logConfig)
    {
        if (logConfig.Database == DbTypes.SqlServer)
            SetupMsSqlServer(loggerConfiguration, logConfig);
        else if (logConfig.Database == DbTypes.MySql)
            SetupMySql(loggerConfiguration, logConfig);
        else if (logConfig.Database == DbTypes.PostgreSQL) SetupPostGreSql(loggerConfiguration, logConfig);
    }

    /// <summary>
    /// Configures the Serilog MS SQL Server audit sink with a custom UserId column and auto-table creation.
    /// </summary>
    /// <param name="loggerConfiguration">The Serilog configuration builder.</param>
    /// <param name="logConfig">Database connection and level settings.</param>
    private void SetupMsSqlServer(LoggerConfiguration loggerConfiguration, LogDbConfig logConfig)
    {
        var sinkOptions = new MSSqlServerSinkOptions
        {
            TableName = "HCL.CS.SF_Logs",
            AutoCreateSqlTable = true
        };

        var columnOptions = new ColumnOptions
        {
            AdditionalColumns = new Collection<SqlColumn>
            {
                new()
                {
                    ColumnName = "UserId",
                    PropertyName = "UserId",
                    DataType = SqlDbType.NVarChar,
                    DataLength = 255
                }
            }
        };

        loggerConfiguration.AuditTo.MSSqlServer(
            logConfig.ConnectionString,
            restrictedToMinimumLevel: (LogEventLevel)logConfig.RestrictedToMinimumLevel,
            sinkOptions: sinkOptions,
            columnOptions: columnOptions);
    }

    /// <summary>
    /// Configures the Serilog MariaDB/MySQL audit sink with auto-table creation.
    /// </summary>
    /// <param name="loggerConfiguration">The Serilog configuration builder.</param>
    /// <param name="logConfig">Database connection and level settings.</param>
    private void SetupMySql(LoggerConfiguration loggerConfiguration, LogDbConfig logConfig)
    {
        loggerConfiguration.AuditTo.MariaDB(
            logConfig.ConnectionString,
            tableName: "HCL.CS.SF_Logs",
            autoCreateTable: true,
            restrictedToMinimumLevel: (LogEventLevel)logConfig.RestrictedToMinimumLevel);
    }

    /// <summary>
    /// Configures the Serilog PostgreSQL sink with custom column writers for structured log data
    /// including UserId and MachineName properties.
    /// </summary>
    /// <param name="loggerConfiguration">The Serilog configuration builder.</param>
    /// <param name="logConfig">Database connection and level settings.</param>
    private void SetupPostGreSql(LoggerConfiguration loggerConfiguration, LogDbConfig logConfig)
    {
        var connectionstring = logConfig.ConnectionString;
        var tableName = "HCL.CS.SF_Logs";
        IDictionary<string, ColumnWriterBase> columnWriters = new Dictionary<string, ColumnWriterBase>
        {
            { "Message", new RenderedMessageColumnWriter() },
            { "MessageTemplate", new MessageTemplateColumnWriter() },
            { "Level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
            { "TimeStamp", new TimestampColumnWriter() },
            { "Exception", new ExceptionColumnWriter() },
            { "Properties", new LogEventSerializedColumnWriter() },
            {
                "UserId", new SinglePropertyColumnWriter("UserId", PropertyWriteMethod.ToString, NpgsqlDbType.Text, "l")
            },
            {
                "MachineName",
                new SinglePropertyColumnWriter("MachineName", PropertyWriteMethod.ToString, NpgsqlDbType.Text, "l")
            }
        };
        loggerConfiguration.WriteTo.PostgreSQL(
            connectionstring,
            tableName,
            columnWriters,
            needAutoCreateTable: true,
            restrictedToMinimumLevel: (LogEventLevel)logConfig.RestrictedToMinimumLevel);
    }

    /// <summary>
    /// Sets the minimum log level on the Serilog configuration and enriches with machine name and log context.
    /// </summary>
    /// <param name="loggerConfiguration">The Serilog configuration builder.</param>
    /// <param name="minimumConfiguration">The minimum log severity to capture.</param>
    private void SetMinimumLevel(LoggerConfiguration loggerConfiguration, Log minimumConfiguration)
    {
        switch (minimumConfiguration)
        {
            case Log.Debug:
                loggerConfiguration.MinimumLevel.Debug();
                break;
            case Log.Error:
                loggerConfiguration.MinimumLevel.Error();
                break;
            case Log.Fatal:
                loggerConfiguration.MinimumLevel.Fatal();
                break;
            case Log.Information:
                loggerConfiguration.MinimumLevel.Information();
                break;
            case Log.Verbose:
                loggerConfiguration.MinimumLevel.Verbose();
                break;
            case Log.Warning:
                loggerConfiguration.MinimumLevel.Warning();
                break;
        }

        loggerConfiguration.Enrich.WithMachineName();
        loggerConfiguration.Enrich.FromLogContext();
    }
}
