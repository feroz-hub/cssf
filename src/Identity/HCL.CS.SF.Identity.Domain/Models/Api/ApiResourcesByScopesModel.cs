/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Api;

/// <summary>
/// Flattened view of the relationship between API resources, their scopes, and associated claim types.
/// Used during token generation to resolve which claims should be included in access tokens
/// based on the granted scopes.
/// </summary>
public class ApiResourcesByScopesModel : BaseModel
{
    /// <summary>The unique identifier of the API resource.</summary>
    public virtual Guid ApiResourceId { get; set; }

    /// <summary>The name of the API resource (used as the audience claim in access tokens).</summary>
    public virtual string ApiResourceName { get; set; }

    /// <summary>A claim type associated with the API resource.</summary>
    public virtual string ApiResourceClaimType { get; set; }

    /// <summary>The name of the API scope within this resource.</summary>
    public virtual string ApiScopeName { get; set; }

    /// <summary>A claim type associated with the API scope.</summary>
    public virtual string ApiScopeClaimType { get; set; }
}
