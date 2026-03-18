/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Api;

/// <summary>
/// Represents a claim type associated with an API resource.
/// These claims are always included in access tokens when any scope
/// belonging to this API resource is granted.
/// </summary>
public class ApiResourceClaimsModel : BaseModel
{
    /// <summary>The unique identifier of the parent API resource.</summary>
    public virtual Guid ApiResourceId { get; set; }

    /// <summary>The claim type name to include in access tokens for this API resource.</summary>
    public virtual string Type { get; set; }
}
