/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Admin.UI.Models.Api;

public class AuditTrailModel
{
    public string Id { get; set; } = string.Empty;

    public string? ActionType { get; set; }

    public string? TableName { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? AffectedColumn { get; set; }

    public string? ActionName { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }
}

public class AuditSearchRequestModel
{
    public string? ActionType { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public PagingModel Page { get; set; } = new();

    public string? SearchValue { get; set; }
}

public class AuditResponseModel
{
    public List<AuditTrailModel> AuditList { get; set; } = new();

    public PagingModel PageInfo { get; set; } = new();
}
