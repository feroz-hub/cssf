/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.Domain.Models.Api.Response;

namespace HCL.CS.SF.Domain.Models.Endpoint;

/// <summary>
/// Aggregates all the data needed to generate OAuth 2.0 / OIDC tokens.
/// During token creation, this model provides the authenticated user, the requesting client,
/// the resolved identity and API resources, scopes, and role/permission claims to be embedded in tokens.
/// </summary>
public class TokenDetailsModel
{
    /// <summary>The authenticated user for whom tokens are being generated.</summary>
    public virtual UserModel User { get; set; }

    /// <summary>The client application requesting the tokens.</summary>
    public virtual ClientsModel Client { get; set; }

    /// <summary>The identity resources (e.g., openid, profile, email) resolved for this token request.</summary>
    public virtual IList<IdentityResourcesModel> IdentityResources { get; set; } = new List<IdentityResourcesModel>();

    /// <summary>The API resources that the granted scopes map to.</summary>
    public virtual IList<ApiResourcesModel> ApiResources { get; set; } = new List<ApiResourcesModel>();

    /// <summary>The API scopes granted for this token request.</summary>
    public virtual IList<ApiScopesModel> ApiScopes { get; set; } = new List<ApiScopesModel>();

    /// <summary>Identity resources resolved by scope name, including their associated claim types.</summary>
    public virtual IList<IdentityResourcesByScopesModel> IdentityResourcesByScopes { get; set; } =
        new List<IdentityResourcesByScopesModel>();

    /// <summary>API resources resolved by scope name, including their associated claim types.</summary>
    public virtual IList<ApiResourcesByScopesModel> ApiResourcesByScopes { get; set; } =
        new List<ApiResourcesByScopesModel>();

    /// <summary>The claim types derived from the user's role assignments, used to populate role-based claims in tokens.</summary>
    public virtual IList<UserRoleClaimTypesModel> UserRoleClaimTypes { get; set; } =
        new List<UserRoleClaimTypesModel>();

    /// <summary>The names of roles assigned to the user.</summary>
    public virtual IList<string> UserRoles { get; set; } = new List<string>();

    /// <summary>The permission claims derived from the user's roles, embedded in access tokens for authorization.</summary>
    public virtual IList<UserRoleClaimsModel> RolePermissions { get; set; } = new List<UserRoleClaimsModel>();
}
