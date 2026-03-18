/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Admin.UI.Models.Api;

public class ApiResourcesModel
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public bool Enabled { get; set; }

    public List<ApiResourceClaimsModel> ApiResourceClaims { get; set; } = new();

    public List<ApiScopesModel> ApiScopes { get; set; } = new();
}

public class ApiScopesModel
{
    public string Id { get; set; } = string.Empty;

    public string? ApiResourceId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public bool Required { get; set; }

    public bool Emphasize { get; set; }

    public List<ApiScopeClaimsModel> ApiScopeClaims { get; set; } = new();
}

public class ApiResourceClaimsModel
{
    public string Id { get; set; } = string.Empty;

    public string ApiResourceId { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
}

public class ApiScopeClaimsModel
{
    public string Id { get; set; } = string.Empty;

    public string ApiScopeId { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
}

public class IdentityResourcesModel
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public bool Enabled { get; set; }

    public bool Required { get; set; }

    public bool Emphasize { get; set; }

    public List<IdentityClaimsModel> IdentityClaims { get; set; } = new();
}

public class IdentityClaimsModel
{
    public string Id { get; set; } = string.Empty;

    public string IdentityResourceId { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string? AliasType { get; set; }
}
