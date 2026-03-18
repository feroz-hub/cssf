/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain;

public class LogConfig
{
    public string InstanceName { get; set; }

    public WriteLogTo WriteLogTo { get; set; } = WriteLogTo.File;

    public LogFileConfig LogFileConfig { get; set; } = new();

    public LogDbConfig LogDbConfig { get; set; } = new();
}

public class LogFileConfig
{
    private const string Format =
        "[{Timestamp:yyyy-MM-dd HH:mm:ss}|UserId-{UserId}|MachineName-{MachineName}|{Level}|{Message}|" +
        "{MethodName}|{FileName}|{Exception}]{NewLine}";


    // private static string logOutputFormat = "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level}] [ThreadId - {ThreadId}] [EnvironmentUserName - {EnvironmentUserName}] " +
    //    "[ProcessId - {ProcessId}] [ProcessName - {ProcessName}] [MachineName - {MachineName}] [SourceContext - {SourceContext}] " +
    //    "[Logged UserId - {UserId}] {NewLine}{Message}{NewLine}in method {MemberName} at {FilePath}:{LineNumber}{NewLine}at {Caller}{NewLine}{Exception}";

    public string FilePath { get; set; }

    public RollingIntervalType RollingIntervalType { get; set; } = RollingIntervalType.Day;

    public Log RestrictedToMinimumLevel { get; set; }

    public Log MinimumConfiguration { get; set; } = Log.Error;

    public bool SetLogFileSize { get; set; } = true;

    public long FileSizeInBytes { get; set; } = 5242880;

    public string OutputFormat { get; set; } = Format;
}

public class LogDbConfig
{
    public DbTypes Database { get; set; }

    public string ConnectionString { get; set; }

    public Log RestrictedToMinimumLevel { get; set; }

    public Log MinimumConfiguration { get; set; } = Log.Error;
}
