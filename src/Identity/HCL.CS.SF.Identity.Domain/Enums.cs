/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain;

/// <summary>
/// Defines the supported database provider types for the identity framework's persistence layer.
/// The selected provider determines which EF Core database driver and migration set is used.
/// </summary>
public enum DbTypes
{
    /// <summary>
    /// Microsoft SQL Server database provider (via Microsoft.EntityFrameworkCore.SqlServer).
    /// </summary>
    SqlServer = 1,

    /// <summary>
    /// MySQL database provider (via Pomelo.EntityFrameworkCore.MySql or MySql.EntityFrameworkCore).
    /// </summary>
    MySql = 2,

    /// <summary>
    /// PostgreSQL database provider (via Npgsql.EntityFrameworkCore.PostgreSQL).
    /// </summary>
    PostgreSQL = 3,

    /// <summary>
    /// SQLite database provider (via Microsoft.EntityFrameworkCore.Sqlite). Suitable for development and lightweight deployments.
    /// </summary>
    SQLite = 4
}

/// <summary>
/// Defines where structured log output is written by the logging infrastructure.
/// </summary>
public enum WriteLogTo
{
    /// <summary>
    /// Write log entries to rolling log files on the file system.
    /// </summary>
    File = 0,

    /// <summary>
    /// Write log entries to a database table for centralized querying and long-term retention.
    /// </summary>
    DataBase = 1
}

/// <summary>
/// Defines the rolling interval for log file rotation when file-based logging is enabled.
/// Controls how frequently a new log file is created.
/// </summary>
public enum RollingIntervalType
{
    /// <summary>
    /// Create a new log file each month.
    /// </summary>
    Month = 0,

    /// <summary>
    /// Create a new log file each day.
    /// </summary>
    Day = 1,

    /// <summary>
    /// Create a new log file each hour.
    /// </summary>
    Hour = 2,

    /// <summary>
    /// Create a new log file each minute. Typically used only in high-throughput diagnostic scenarios.
    /// </summary>
    Minute = 3
}

/// <summary>
/// Defines the severity levels for log messages, ordered from most verbose to most critical.
/// Maps to standard Serilog / Microsoft.Extensions.Logging log levels.
/// </summary>
public enum Log
{
    /// <summary>
    /// Debug-level messages containing detailed diagnostic information for developers.
    /// </summary>
    Debug = 0,

    /// <summary>
    /// Error-level messages indicating a failure in the current operation that does not crash the application.
    /// </summary>
    Error = 1,

    /// <summary>
    /// Fatal-level messages indicating a critical failure that may cause the application to terminate.
    /// </summary>
    Fatal = 2,

    /// <summary>
    /// Informational messages that highlight the general flow of the application.
    /// </summary>
    Information = 3,

    /// <summary>
    /// Verbose (trace) level messages containing the most detailed diagnostic information.
    /// </summary>
    Verbose = 4,

    /// <summary>
    /// Warning-level messages indicating a potential issue that does not yet represent a failure.
    /// </summary>
    Warning = 5
}

/// <summary>
/// Represents the outcome status of a service or API operation.
/// </summary>
public enum ResultStatus
{
    /// <summary>
    /// The operation completed successfully without errors.
    /// </summary>
    Succeeded = 0,

    /// <summary>
    /// The operation failed due to validation errors, business rule violations, or infrastructure issues.
    /// </summary>
    Failed = 1
}

/// <summary>
/// Defines the type of CRUD (Create, Read, Update, Delete) operation being performed.
/// Used to indicate the current operation mode in shared service methods.
/// </summary>
public enum CrudMode
{
    /// <summary>
    /// A new entity is being created (INSERT).
    /// </summary>
    Add = 0,

    /// <summary>
    /// An existing entity is being modified (UPDATE).
    /// </summary>
    Update = 1,

    /// <summary>
    /// An existing entity is being removed (DELETE).
    /// </summary>
    Delete = 2
}
