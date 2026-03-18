/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Entities.Api;

/// <summary>
/// Maps a claim type to an <see cref="ApiResources"/> entry.
/// Claims listed here are included in access tokens whenever any scope
/// belonging to the parent API resource is granted to a client.
/// </summary>
public class ApiResourceClaims : BaseEntity
{
    /// <summary>Foreign key referencing the parent API resource.</summary>
    public virtual Guid ApiResourceId { get; set; }

    /// <summary>The claim type to include in access tokens (e.g., "role", "tenant_id").</summary>
    public virtual string Type { get; set; }

    /// <summary>Navigation property to the parent API resource.</summary>
    public virtual ApiResources ApiResource { get; set; }
}
