/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.Extensions.DependencyInjection;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;

namespace IntegrationTests;

public class AuditCheck : HCLCSSFFakeSetup
{
    private readonly IAuditTrailService auditTrailService;

    public AuditCheck()
    {
        auditTrailService = ServiceProvider.GetService<IAuditTrailService>();
    }

    //public async Task<AuditResponseModel> GetAudit (string createdBy, AuditType auditType)
    //{
    //    DateTime fromDate = DateTime.UtcNow.AddDays(-1);
    //    DateTime toDate = DateTime.UtcNow.AddDays(1);
    //    string roleCreatedModifedBy = createdBy;
    //    PagingModel page = new PagingModel()
    //    {
    //        CurrentPage = 1,
    //        ItemsPerPage = 1000,
    //    };
    //    // Audit Type => None:0, Create/Add:1, Update:2, Delete:3
    //    AuditResponseModel auditResponseModelResult = await auditTrailService.GetAuditDetailsAsync(roleCreatedModifedBy, auditType, fromDate, toDate, page);

    //    if (auditResponseModelResult != null)
    //    {
    //        return auditResponseModelResult;
    //    }
    //    else
    //    {
    //        return null;
    //    }

    //}
}
