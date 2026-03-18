/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using Npgsql;
using HCL.CS.SF.Domain;
using HCL.CS.SF.DomainServices.Infra;

namespace HCL.CS.SF.Infrastructure.Data.Validation;

/// <summary>
/// Validates database connectivity by attempting to open a connection using the appropriate
/// provider (SQL Server, MySQL, PostgreSQL, or SQLite). Returns the exception on failure, or null on success.
/// </summary>
public class DbConnectionValidator : IDbConnectionValidator
{
    /// <summary>
    /// Attempts to open a database connection using the given provider and connection string.
    /// </summary>
    /// <param name="databaseType">The type of database provider to use.</param>
    /// <param name="connectionString">The connection string to validate.</param>
    /// <returns>Null if the connection succeeds; otherwise the exception that was thrown.</returns>
    public Exception? Validate(DbTypes databaseType, string connectionString)
    {
        try
        {
            if (databaseType == DbTypes.SqlServer)
            {
                using var conn = new SqlConnection(connectionString);
                if (conn.State != ConnectionState.Open) conn.Open();
            }
            else if (databaseType == DbTypes.MySql)
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();
            }
            else if (databaseType == DbTypes.PostgreSQL)
            {
                using var conn = new NpgsqlConnection(connectionString);
                conn.Open();
            }
            else if (databaseType == DbTypes.SQLite)
            {
                using var conn = new SqliteConnection(connectionString);
                conn.Open();
            }
        }
        catch (Exception ex)
        {
            return ex;
        }

        return null;
    }
}
