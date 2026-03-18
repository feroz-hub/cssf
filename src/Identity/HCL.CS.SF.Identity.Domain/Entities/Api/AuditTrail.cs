/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Enums;

namespace HCL.CS.SF.Domain.Entities.Api;

/// <summary>
/// Records a change-level audit entry for data modifications within the identity system.
/// Captures before/after values at the column level to satisfy compliance and
/// forensic-investigation requirements for security-sensitive operations.
/// </summary>
public class AuditTrail : BaseEntity
{
    /// <summary>The type of data operation performed (e.g., Create, Update, Delete).</summary>
    public AuditType ActionType { get; set; }

    /// <summary>Name of the database table where the change occurred.</summary>
    public string TableName { get; set; }

    /// <summary>JSON-serialized previous value(s) before the change; null for insert operations.</summary>
    public string OldValue { get; set; }

    /// <summary>JSON-serialized new value(s) after the change; null for delete operations.</summary>
    public string NewValue { get; set; }

    /// <summary>Comma-separated list of column names that were modified.</summary>
    public string AffectedColumn { get; set; }

    /// <summary>Human-readable name describing the action (e.g., "UserPasswordChange", "ClientSecretRotation").</summary>
    public string ActionName { get; set; }
}
