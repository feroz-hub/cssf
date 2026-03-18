/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Newtonsoft.Json;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Enums;

namespace HCL.CS.SF.Domain.Models.Api;

/// <summary>
/// Captures changes made to a database entity for audit trail logging.
/// Tracks the table name, old and new values, affected columns, and the user
/// who made the change. Converts to an <see cref="AuditTrail"/> entity for persistence.
/// </summary>
public class AuditModel
{
    /// <summary>The name of the database table that was modified.</summary>
    public string TableName { get; set; }

    /// <summary>The identifier of the user or system that performed the change.</summary>
    public string CreatedBy { get; set; }

    /// <summary>The original column values before the change (for update and delete operations).</summary>
    public Dictionary<string, object> OldValues { get; } = new();

    /// <summary>The new column values after the change (for create and update operations).</summary>
    public Dictionary<string, object> NewValues { get; } = new();

    /// <summary>The type of audit action (Create, Update, Delete).</summary>
    public AuditType AuditType { get; set; }

    /// <summary>A descriptive name for the action performed.</summary>
    public string ActionName { get; set; }

    /// <summary>The list of column names that were modified in this operation.</summary>
    public List<string> AffectedColumns { get; } = new();

    /// <summary>
    /// Converts this audit model to an <see cref="AuditTrail"/> entity for database persistence.
    /// Serializes old values, new values, and affected columns to JSON strings.
    /// </summary>
    /// <returns>A new <see cref="AuditTrail"/> entity ready for persistence.</returns>
    public AuditTrail ToAudit()
    {
        var audit = new AuditTrail();
        audit.ActionType = AuditType;
        audit.TableName = TableName;
        audit.CreatedOn = DateTime.UtcNow;
        audit.CreatedBy = CreatedBy;
        audit.OldValue = OldValues.Count == 0 ? null : JsonConvert.SerializeObject(OldValues);
        audit.NewValue = NewValues.Count == 0 ? null : JsonConvert.SerializeObject(NewValues);
        audit.AffectedColumn = AffectedColumns.Count == 0 ? null : JsonConvert.SerializeObject(AffectedColumns);
        return audit;
    }
}
