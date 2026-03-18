/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain;

namespace HCL.CS.SF.DomainServices.Infra;

/// <summary>
/// Validates database connection strings before they are used for migrations or runtime connections.
/// Implementations should attempt a lightweight probe (e.g., opening and closing a connection)
/// and return any exception encountered, or <c>null</c> on success.
/// </summary>
public interface IDbConnectionValidator
{
    /// <summary>
    /// Validates that the given connection string can establish a connection to the specified database type.
    /// </summary>
    /// <param name="databaseType">The target database provider (SQL Server, PostgreSQL, MySQL, SQLite).</param>
    /// <param name="connectionString">The connection string to validate.</param>
    /// <returns>An <see cref="Exception"/> describing the failure, or <c>null</c> if the connection is valid.</returns>
    Exception? Validate(DbTypes databaseType, string connectionString);
}
