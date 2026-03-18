/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Admin.UI.Models.Api;

public class FrameworkResult
{
    public object Status { get; set; } = 0;

    public List<FrameworkError>? Errors { get; set; }

    public bool IsSuccess => Status is 0 or "Succeeded" or "0";
}

public class FrameworkResult<T> : FrameworkResult
{
    public T? Data { get; set; }
}

public class FrameworkError
{
    public string Code { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}
