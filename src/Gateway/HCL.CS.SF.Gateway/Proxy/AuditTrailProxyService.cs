/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using AutoMapper;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.Domain.Models.Api.Response;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.DomainServices.Repository.Api;
using HCL.CS.SF.Service.Implementation.Api.Services;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api.Wrapper;

namespace HCL.CS.SF.ProxyService.Proxy;

/// <summary>
/// Gateway proxy for the audit trail service. Routes requests to the backend
/// <see cref="AuditTrailService"/> after enforcing API-level permission validation.
/// Supports recording audit events and querying audit log history.
/// </summary>
public sealed class AuditTrailProxyService : AuditTrailService, IAuditTrailService
{
    /// <summary>
    /// Validator that checks whether the caller has permission to invoke the requested operation.
    /// </summary>
    private readonly IApiValidator apiValidator;

    /// <summary>
    /// Service used to construct failure responses when validation fails.
    /// </summary>
    private readonly IFrameworkResultService frameworkResult;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditTrailProxyService"/> class.
    /// </summary>
    public AuditTrailProxyService(
        IAuditRepository auditRepository,
        ILoggerInstance loggerInstance,
        IMapper mapper,
        IFrameworkResultService frameworkResult,
        IApiValidator apiValidator)
        : base
        (
            auditRepository,
            loggerInstance,
            mapper,
            frameworkResult)
    {
        this.apiValidator = apiValidator;
        this.frameworkResult = frameworkResult;
    }

    /// <summary>
    /// Records a batch of audit trail entries after permission validation.
    /// </summary>
    /// <param name="audits">The audit trail entries to record.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> AddAuditTrailAsync(IEnumerable<AuditTrailModel> audits)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.AddAuditTrailAsync(audits);
    }

    /// <summary>
    /// Records a single audit trail entry after permission validation.
    /// </summary>
    /// <param name="audit">The audit trail entry to record.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating the outcome.</returns>
    public override async Task<FrameworkResult> AddAuditTrailAsync(AuditTrailModel audit)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed) return result;

        return await base.AddAuditTrailAsync(audit);
    }

    /// <summary>
    /// Queries audit trail details using the provided search criteria after permission validation.
    /// </summary>
    /// <param name="auditSearchRequestModel">The search criteria for filtering audit records.</param>
    /// <returns>A paginated audit response containing matching records.</returns>
    public override async Task<AuditResponseModel> GetAuditDetailsAsync(AuditSearchRequestModel auditSearchRequestModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetAuditDetailsAsync(auditSearchRequestModel);
    }

    //public override async Task<AuditResponseModel> GetAuditDetailsAsync(string createdBy, DateTime? createdOn, PagingModel page)
    //{
    //    var result = await apiValidator.ValidateRequest();
    //    if (result.Status == ResultStatus.Failed)
    //    {
    //        frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);
    //    }

    //    return await base.GetAuditDetailsAsync(createdBy, createdOn, page);
    //}

    //public override async Task<AuditResponseModel> GetAuditDetailsAsync(string createdBy, DateTime? fromDate, DateTime? toDate, PagingModel page)
    //{
    //    var result = await apiValidator.ValidateRequest();
    //    if (result.Status == ResultStatus.Failed)
    //    {
    //        frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);
    //    }

    //    return await base.GetAuditDetailsAsync(createdBy, fromDate, toDate, page);
    //}

    //public override async Task<AuditResponseModel> GetAuditDetailsAsync(string createdBy, AuditType actionType, DateTime? fromDate, DateTime? toDate, PagingModel page)
    //{
    //    var result = await apiValidator.ValidateRequest();
    //    if (result.Status == ResultStatus.Failed)
    //    {
    //        frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);
    //    }

    //    return await base.GetAuditDetailsAsync(createdBy, actionType, fromDate, toDate, page);
    //}
}
