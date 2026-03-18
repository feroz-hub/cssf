/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Api;

/// <summary>
/// Represents an API scope in the OAuth 2.0 resource model.
/// API scopes define granular access levels (e.g., "read", "write") within an API resource.
/// Clients request these scopes, and they appear in the access token's scope claim.
/// </summary>
public class ApiScopesModel : BaseModel
{
    /// <summary>The identifier of the parent API resource this scope belongs to.</summary>
    public virtual Guid ApiResourceId { get; set; }

    /// <summary>The unique scope name used in OAuth 2.0 scope parameters (e.g., "api.read").</summary>
    public virtual string Name { get; set; }

    /// <summary>The human-readable display name shown on consent screens.</summary>
    public virtual string DisplayName { get; set; }

    /// <summary>A description of what this scope grants access to, shown on consent screens.</summary>
    public virtual string Description { get; set; }

    /// <summary>Indicates whether this scope must be granted (cannot be deselected on consent screen).</summary>
    public virtual bool Required { get; set; } = false;

    /// <summary>Indicates whether this scope should be visually emphasized on the consent screen.</summary>
    public virtual bool Emphasize { get; set; } = false;

    /// <summary>The claim types that are included in access tokens when this scope is granted.</summary>
    public virtual List<ApiScopeClaimsModel> ApiScopeClaims { get; set; }
}
