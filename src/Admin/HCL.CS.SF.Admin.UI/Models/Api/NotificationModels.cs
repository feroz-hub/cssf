/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Admin.UI.Models.Api;

public class NotificationLogModel
{
    public string Id { get; set; } = string.Empty;

    public string? UserId { get; set; }

    public string? MessageId { get; set; }

    public string? Type { get; set; }

    public string? Status { get; set; }

    public string? Provider { get; set; }

    public string? Recipient { get; set; }

    public string? Subject { get; set; }

    public string? Message { get; set; }

    public DateTime? SentDateTime { get; set; }

    public DateTime? CreatedOn { get; set; }
}

public class NotificationSearchRequestModel
{
    public string? Type { get; set; }

    public string? Status { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public string? SearchValue { get; set; }

    public PagingModel Page { get; set; } = new();
}

public class NotificationLogResponseModel
{
    public List<NotificationLogModel> Notifications { get; set; } = new();

    public PagingModel PageInfo { get; set; } = new();
}

public class ProviderConfigModel
{
    public string Id { get; set; } = string.Empty;

    public string Provider { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public Dictionary<string, object> Config { get; set; } = new();
}
