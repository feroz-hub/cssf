/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Admin.UI.Models.Api;

public class PagingModel
{
    public int TotalItems { get; set; }

    public int ItemsPerPage { get; set; } = 10;

    public int CurrentPage { get; set; } = 1;

    public int TotalPages { get; set; }

    public int TotalDisplayPages { get; set; } = 10;
}
