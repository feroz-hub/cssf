/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using AutoMapper;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.Domain.Models.Api.Response;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.DomainServices.Repository.Api;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;

namespace HCL.CS.SF.Service.Implementation.Api.Services;

/// <summary>
/// Service for managing the security audit trail. Records all administrative actions
/// (create, update, delete) performed on identity entities for compliance, forensics,
/// and change tracking. Supports paginated search with date range and action type filters.
/// </summary>

public class AuditTrailService(
    IAuditRepository auditRepository,
    ILoggerInstance loggerInstance,
    IMapper mapper,
    IFrameworkResultService frameworkResult)
    : SecurityBase, IAuditTrailService
{
    private readonly ILoggerService loggerService =
        loggerInstance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);

    /// <summary>
    /// Records a single audit trail entry after validating the action type and field lengths.
    /// </summary>
    /// <param name="audit">The audit trail model to persist.</param>
    /// <returns>The result of the audit record insertion.</returns>

    public virtual async Task<FrameworkResult> AddAuditTrailAsync(AuditTrailModel audit)
    {
        if (audit == null) return frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.AuditModelIsNull);

        if (audit.ActionType == AuditType.None)
            return frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.InvalidAuditActionType);

        if (!string.IsNullOrWhiteSpace(audit.TableName) && audit.TableName.Length > Constants.ColumnLength255)
            return frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.AuditTableNameTooLong);

        if (!string.IsNullOrWhiteSpace(audit.CreatedBy) && audit.CreatedBy.Length > Constants.ColumnLength255)
            return frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.CreatedByTooLong);

        try
        {
            var auditTrail = mapper.Map<AuditTrailModel, AuditTrail>(audit);
            await auditRepository.InsertAsync(auditTrail);
            return await auditRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to add audit trail.");
            throw;
        }
    }

    /// <summary>
    /// Records multiple audit trail entries in a single transaction.
    /// </summary>
    /// <param name="audits">The collection of audit trail models to persist.</param>
    /// <returns>The result of the batch audit record insertion.</returns>

    public virtual async Task<FrameworkResult> AddAuditTrailAsync(IEnumerable<AuditTrailModel> audits)
    {
        try
        {
            if (audits == null) return frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.AuditModelIsNull);

            var auditTrails = mapper.Map<IEnumerable<AuditTrailModel>, IEnumerable<AuditTrail>>(audits);
            foreach (var audit in auditTrails)
            {
                if (audit.ActionType == AuditType.None)
                    return frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.InvalidAuditActionType);

                if (!string.IsNullOrWhiteSpace(audit.TableName) && audit.TableName.Length > Constants.ColumnLength255)
                    return frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.AuditTableNameTooLong);

                if (!string.IsNullOrWhiteSpace(audit.CreatedBy) && audit.CreatedBy.Length > Constants.ColumnLength255)
                    return frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.CreatedByTooLong);

                await auditRepository.InsertAsync(audit);
            }

            return await auditRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to add audit trail.");
            throw;
        }
    }

    /// <summary>
    /// Retrieves paginated audit trail entries with optional filtering by date range,
    /// action type, and user. Returns total item count for pagination support.
    /// </summary>
    /// <param name="auditSearchRequestModel">Search parameters including paging, date range, and filters.</param>
    /// <returns>Paginated audit response containing the matching audit records.</returns>

    public virtual async Task<AuditResponseModel> GetAuditDetailsAsync(AuditSearchRequestModel auditSearchRequestModel)
    {
        try
        {
            if (auditSearchRequestModel.Page == null)
                frameworkResult.Throw(ApiErrorCodes.AuditModelIsNull); // Change Error Code to PageModelIsNull.

            var auditResponse = new AuditResponseModel();
            if (auditSearchRequestModel.FromDate > auditSearchRequestModel.ToDate)
                frameworkResult.Throw(ApiErrorCodes.FromDateGreaterThanToDate);

            auditSearchRequestModel.Page.TotalItems = await auditRepository.GetFilteredCountAsync(auditSearchRequestModel);
            auditResponse.PageInfo = auditSearchRequestModel.Page;

            //  IList<AuditTrail> searchResults = await auditRepository.GetAuditDetailsAsync(page, option, createdBy, actionType, fromDate, toDate);
            var searchResults = await auditRepository.GetAuditDetailsAsync(auditSearchRequestModel);
            if (!searchResults.ContainsAny())
            {
                auditResponse.AuditList = new List<AuditTrailModel>();
                return auditResponse;
            }

            auditResponse.AuditList = mapper.Map<IEnumerable<AuditTrail>, IEnumerable<AuditTrailModel>>(searchResults)
                .ToList();
            return auditResponse;
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to retrieve audit details.");
            throw;
        }
    }

    //public virtual async Task<AuditResponseModel> GetAuditDetailsAsync(string createdBy, DateTime? createdOn, PagingModel page)
    //{
    //    return await GetAuditDetailsAsync(page, QueryOption.ChangeByAndDate, createdBy, AuditType.None, createdOn);
    //}

    //public virtual async Task<AuditResponseModel> GetAuditDetailsAsync(string createdBy, DateTime? fromDate, DateTime? toDate, PagingModel page)
    //{
    //    return await GetAuditDetailsAsync(page, QueryOption.ChangeByAndBetweenDates, createdBy, AuditType.None, fromDate, toDate);
    //}

    //public virtual async Task<AuditResponseModel> GetAuditDetailsAsync(string createdBy, AuditType actionType, DateTime? fromDate, DateTime? toDate, PagingModel page)
    //{
    //    return await GetAuditDetailsAsync(page, QueryOption.ChangeBywithActionAndBetweenDates, createdBy, actionType, fromDate, toDate);
    //}
}
