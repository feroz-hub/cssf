/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Api;

/// <summary>
/// Represents an API resource in the OAuth 2.0 resource model.
/// An API resource is a protected server-side resource (e.g., a Web API) that clients
/// request access to. It contains scopes and associated claim types that determine
/// what claims are included in access tokens for this resource.
/// </summary>
public class ApiResourcesModel : BaseModel
{
    /// <summary>The unique name of the API resource, used as the audience (aud) claim in access tokens.</summary>
    public virtual string Name { get; set; }

    /// <summary>The human-readable display name shown on consent screens.</summary>
    public virtual string DisplayName { get; set; }

    /// <summary>A description of the API resource shown on consent screens.</summary>
    public virtual string Description { get; set; }

    /// <summary>Indicates whether this API resource is enabled and accessible.</summary>
    public virtual bool Enabled { get; set; } = true;

    /// <summary>The claim types that are always included in access tokens for this API resource.</summary>
    public virtual List<ApiResourceClaimsModel> ApiResourceClaims { get; set; }

    /// <summary>The scopes defined within this API resource that clients can request.</summary>
    public virtual List<ApiScopesModel> ApiScopes { get; set; }
}
