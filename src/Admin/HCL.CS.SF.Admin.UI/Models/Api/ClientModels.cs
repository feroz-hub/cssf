/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Admin.UI.Models.Api;

public class ClientsModel
{
    public string Id { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string? ClientName { get; set; }

    public string? ClientUri { get; set; }

    public string? ClientSecret { get; set; }

    public string? LogoUri { get; set; }

    public string? TermsOfServiceUri { get; set; }

    public string? PolicyUri { get; set; }

    public int RefreshTokenExpiration { get; set; }

    public int AccessTokenExpiration { get; set; }

    public int IdentityTokenExpiration { get; set; }

    public int LogoutTokenExpiration { get; set; }

    public int AuthorizationCodeExpiration { get; set; }

    public int AccessTokenType { get; set; }

    public bool RequirePkce { get; set; }

    public bool IsPkceTextPlain { get; set; }

    public bool RequireClientSecret { get; set; }

    public bool IsFirstPartyApp { get; set; }

    public bool AllowOfflineAccess { get; set; }

    public bool AllowAccessTokensViaBrowser { get; set; }

    public string? ApplicationType { get; set; }

    public bool FrontChannelLogoutSessionRequired { get; set; }

    public string? FrontChannelLogoutUri { get; set; }

    public bool BackChannelLogoutSessionRequired { get; set; }

    public string? BackChannelLogoutUri { get; set; }

    public List<string> SupportedGrantTypes { get; set; } = new();

    public List<string> AllowedScopes { get; set; } = new();

    public List<ClientRedirectUrisModel> RedirectUris { get; set; } = new();

    public List<ClientPostLogoutRedirectUrisModel> PostLogoutRedirectUris { get; set; } = new();

    public string? PreferredAudience { get; set; }
}

public class ClientRedirectUrisModel
{
    public string Id { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string RedirectUri { get; set; } = string.Empty;
}

public class ClientPostLogoutRedirectUrisModel
{
    public string Id { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string PostLogoutRedirectUri { get; set; } = string.Empty;
}
