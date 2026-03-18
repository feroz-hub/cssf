/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Enums;

namespace HCL.CS.SF.Domain.Models.Api;

/// <summary>
/// Represents a single audit trail log entry recording a data change in the identity system.
/// Stores the before/after state of database records for compliance and forensic analysis.
/// </summary>
public class AuditTrailModel
{
    /// <summary>The unique identifier of the audit trail entry.</summary>
    public virtual Guid Id { get; set; }

    /// <summary>The type of data modification action (Create, Update, Delete).</summary>
    public AuditType ActionType { get; set; } = AuditType.None;

    /// <summary>The name of the database table that was modified.</summary>
    public string TableName { get; set; }

    /// <summary>JSON-serialized old values before the change. Null for create operations.</summary>
    public string? OldValue { get; set; }

    /// <summary>JSON-serialized new values after the change. Null for delete operations.</summary>
    public string? NewValue { get; set; }

    /// <summary>JSON-serialized list of column names that were modified.</summary>
    public string? AffectedColumn { get; set; }

    /// <summary>A descriptive name for the action performed.</summary>
    public string ActionName { get; set; }

    /// <summary>The identifier of the user or system that performed the action.</summary>
    public string? CreatedBy { get; set; }

    /// <summary>The UTC timestamp when the audit entry was created.</summary>
    public virtual DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}
