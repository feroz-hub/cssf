/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HCL.CS.SF.Infrastructure.Data;

/// <summary>
/// A no-op value converter for SQLite timestamp/row-version columns.
/// SQLite does not natively support row-version semantics, so this converter
/// passes byte[] values through unchanged to satisfy the EF Core conversion pipeline.
/// </summary>
public sealed class SqliteTimestampConverter : ValueConverter<byte[], byte[]>
{
    /// <summary>
    /// Initializes a new instance of <see cref="SqliteTimestampConverter"/> with identity conversion (no transformation).
    /// </summary>
    public SqliteTimestampConverter()
        : base(
            value => value,
            value => value)
    {
    }
}
