/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Newtonsoft.Json;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;

namespace HCL.CS.SF.Service.Implementation.Api.Utils;

/// <summary>
/// Utility class for creating audit trail records that track all create, update, and delete
/// operations on identity entities. Supports property-level change detection for updates,
/// serializing old and new values as JSON for forensic review and compliance requirements.
/// </summary>

internal class AuditUtil(
    IAuditTrailService auditTrailService,
    ILoggerService loggerService)
{
    /// <summary>
    /// Records an audit trail entry for a create operation, capturing the new entity values.
    /// </summary>
    /// <param name="model">The newly created entity model to audit.</param>
    /// <param name="tableName">The database table name being audited.</param>
    /// <returns>The result of persisting the audit record.</returns>

    internal async Task<FrameworkResult> Create(object model, string tableName)
    {
        var value = GetAuditValues(model, out var createdBy);
        var audit = new AuditTrailModel
        {
            ActionType = AuditType.Create,
            TableName = tableName,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = createdBy,
            OldValue = null,
            NewValue = value,
            AffectedColumn = null
        };
        return await SaveAuditDetails(audit);
    }

    /// <summary>
    /// Records an audit trail entry for a delete operation, capturing the deleted entity values.
    /// </summary>
    /// <param name="model">The entity model being deleted.</param>
    /// <param name="tableName">The database table name being audited.</param>
    /// <returns>The result of persisting the audit record.</returns>

    internal async Task<FrameworkResult> Delete(object model, string tableName)
    {
        var value = GetAuditValues(model, out var createdBy);
        var audit = new AuditTrailModel
        {
            ActionType = AuditType.Delete,
            TableName = tableName,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = createdBy,
            OldValue = value,
            NewValue = null,
            AffectedColumn = null
        };
        return await SaveAuditDetails(audit);
    }

    /// <summary>
    /// Records an audit trail entry for an update operation by comparing old and new model
    /// property values and only logging the changed columns.
    /// </summary>
    /// <param name="oldModel">The entity state before the update.</param>
    /// <param name="newModel">The entity state after the update.</param>
    /// <param name="tableName">The database table name being audited.</param>
    /// <returns>The result of persisting the audit record, or null if no changes detected.</returns>

    internal async Task<FrameworkResult?> Update(object oldModel, object newModel, string tableName)
    {
        var audit = GenerateAuditObject(oldModel, newModel, tableName);
        if (audit != null) return await SaveAuditDetails(audit);

        return null;
    }

    /// <summary>
    /// Records an audit trail entry for an update using pre-built dictionaries of old and new values.
    /// </summary>

    internal async Task<FrameworkResult> Update(Dictionary<string, object> oldModel,
        Dictionary<string, object> newModel, string tableName, string createdBy)
    {
        var audit = new AuditTrailModel
        {
            ActionType = AuditType.Update,
            TableName = tableName,
            CreatedOn = DateTime.UtcNow,
            OldValue = JsonConvert.SerializeObject(oldModel),
            NewValue = JsonConvert.SerializeObject(newModel),
            AffectedColumn = newModel.Select(x => x.Key).ToString(),
            CreatedBy = createdBy
        };
        return await SaveAuditDetails(audit);
    }

    /// <summary>
    /// Compares old and new model objects property-by-property to build an audit record
    /// that contains only the changed columns, old values, and new values.
    /// </summary>

    private AuditTrailModel? GenerateAuditObject(object oldModel, object newModel, string tableName)
    {
        if (ReferenceEquals(oldModel, newModel))
        {
            loggerService.WriteTo(Log.Debug, "Source and destination model are same.");
            return null;
        }

        if (oldModel.GetType() != newModel.GetType())
        {
            loggerService.WriteTo(Log.Debug, "Different type od source and destination model passed");
            return null;
        }

        var audit = new AuditTrailModel
        {
            ActionType = AuditType.Update,
            TableName = tableName,
            CreatedOn = DateTime.UtcNow
        };

        var auditModel = new AuditModel();
        var valueChanged = false;
        foreach (var property in oldModel.GetType().GetProperties())
        {
            var propertyName = property.Name;
            if (Constants.IgnoredAuditColumns.Contains(propertyName)) continue;
            var oldValue = property.GetValue(oldModel);
            var newValue = property.GetValue(newModel);
            if ((oldValue == null && newValue != null) || (oldValue != null && newValue == null) ||
                (oldValue != null && newValue != null && !oldValue.Equals(newValue)))
            {
                auditModel.AffectedColumns.Add(propertyName);
                auditModel.OldValues[propertyName] = oldValue;
                auditModel.NewValues[propertyName] = newValue;
                valueChanged = true;
            }

            if (propertyName == Constants.CreatedBy) audit.CreatedBy = Convert.ToString(property.GetValue(newModel));
        }

        if (valueChanged)
        {
            audit.OldValue = JsonConvert.SerializeObject(auditModel.OldValues);
            audit.NewValue = JsonConvert.SerializeObject(auditModel.NewValues);
            audit.AffectedColumn = JsonConvert.SerializeObject(auditModel.AffectedColumns);
            return audit;
        }

        loggerService.WriteTo(Log.Debug, "Source and destination model are same.");
        return null;
    }

    /// <summary>
    /// Persists an audit trail model to the audit repository.
    /// </summary>

    private async Task<FrameworkResult> SaveAuditDetails(AuditTrailModel audit)
    {
        return await auditTrailService.AddAuditTrailAsync(audit);
    }

    /// <summary>
    /// Serializes a model's non-ignored properties into a JSON dictionary and extracts the CreatedBy value.
    /// </summary>

    private string GetAuditValues(object model, out string? createdBy)
    {
        createdBy = string.Empty;
        var values = new Dictionary<string, object>();
        foreach (var property in model.GetType().GetProperties())
        {
            var propertyName = property.Name;
            if (Constants.IgnoredAuditColumns.Contains(propertyName)) continue;
            values[propertyName] = property.GetValue(model);
            if (propertyName == Constants.CreatedBy) createdBy = Convert.ToString(property.GetValue(model));
        }

        return JsonConvert.SerializeObject(values);
    }
}
