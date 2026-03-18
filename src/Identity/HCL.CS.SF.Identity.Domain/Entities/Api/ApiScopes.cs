/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Entities.Api;

/// <summary>
/// Represents an OAuth2 scope belonging to an <see cref="ApiResources"/>.
/// Scopes define granular permission boundaries that clients can request
/// (e.g., "read:orders", "write:users") and determine which claims appear in access tokens.
/// </summary>
public class ApiScopes : BaseEntity
{
    /// <summary>Foreign key referencing the parent API resource.</summary>
    public virtual Guid ApiResourceId { get; set; }

    /// <summary>Unique programmatic name of the scope (sent as the "scope" parameter in OAuth2 requests).</summary>
    public virtual string Name { get; set; }

    /// <summary>Human-readable display name shown on consent screens.</summary>
    public virtual string DisplayName { get; set; }

    /// <summary>Brief description of what access this scope grants, displayed on consent screens.</summary>
    public virtual string Description { get; set; }

    /// <summary>When true, the user cannot deselect this scope on the consent screen.</summary>
    public virtual bool Required { get; set; }

    /// <summary>When true, this scope is visually highlighted on the consent screen to draw user attention.</summary>
    public virtual bool Emphasize { get; set; }

    /// <summary>Navigation property to the parent API resource.</summary>
    public virtual ApiResources ApiResource { get; set; }

    /// <summary>Claims that are included in access tokens when this scope is granted.</summary>
    public virtual List<ApiScopeClaims> ApiScopeClaims { get; set; }
}
