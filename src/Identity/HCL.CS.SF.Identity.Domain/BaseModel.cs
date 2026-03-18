/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.ComponentModel.DataAnnotations;

namespace HCL.CS.SF.Domain;

public abstract class BaseModel : BaseTrailModel
{
    public virtual Guid Id { get; set; }
}

public abstract class BaseTrailModel
{
    public string CreatedBy { get; set; }

    public string ModifiedBy { get; set; }

    public virtual DateTime CreatedOn { get; set; }

    public virtual DateTime? ModifiedOn { get; set; }

    public virtual bool IsDeleted { get; set; }

    [Timestamp] public byte[] RowVersion { get; set; }
}

public abstract class BaseResponseModel : BaseModel
{
    public bool IsError { get; set; } = true;

    public string ErrorCode { get; set; }

    public string ErrorDescription { get; set; }
}
