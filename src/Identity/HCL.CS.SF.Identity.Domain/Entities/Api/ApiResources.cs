/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Entities.Api;

/// <summary>
/// Represents a protected API resource in the OAuth2 model.
/// An API resource groups one or more <see cref="ApiScopes"/> and defines which
/// user claims should be included in access tokens when the resource is requested.
/// </summary>
public class ApiResources : BaseEntity
{
    /// <summary>Unique programmatic name of the API resource (used as the audience value in access tokens).</summary>
    public virtual string Name { get; set; }

    /// <summary>Human-readable display name shown on consent screens.</summary>
    public virtual string DisplayName { get; set; }

    /// <summary>Brief description of the API resource, displayed on consent or admin screens.</summary>
    public virtual string Description { get; set; }

    /// <summary>Whether this API resource is active. Disabled resources are excluded from token issuance.</summary>
    public virtual bool Enabled { get; set; }

    /// <summary>Claims that should be included in access tokens when this API resource is part of the requested scope.</summary>
    public virtual List<ApiResourceClaims> ApiResourceClaims { get; set; }

    /// <summary>Scopes that belong to this API resource.</summary>
    public virtual List<ApiScopes> ApiScopes { get; set; }
}
