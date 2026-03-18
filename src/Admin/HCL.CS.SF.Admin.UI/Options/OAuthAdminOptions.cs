/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Admin.UI.Options;

public sealed class OAuthAdminOptions
{
    public const string SectionName = "OAuth";

    public string Authority { get; set; } = "https://localhost:5001";

    public string? MetadataAddress { get; set; }

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string CallbackPath { get; set; } = "/signin-oidc";

    public string SignedOutCallbackPath { get; set; } = "/signout-callback-oidc";

    public bool RequireHttpsMetadata { get; set; } = true;

    public string TokenEndpoint { get; set; } = "https://localhost:5001/security/token";

    public string RevocationEndpoint { get; set; } = "https://localhost:5001/security/revocation";

    public string ResourceApiBaseUrl { get; set; } = "https://localhost:5001";

    public int ApiTimeoutSeconds { get; set; } = 30;

    public int RefreshBeforeExpirySeconds { get; set; } = 120;

    public IList<string> Scopes { get; set; } = new List<string>
    {
        "openid",
        "profile",
        "email",
        "offline_access",
        "phone",
        "HCL.CS.SF.apiresource.manage",
        "HCL.CS.SF.client.manage",
        "HCL.CS.SF.user.read",
        "HCL.CS.SF.user.write",
        "HCL.CS.SF.role.manage",
        "HCL.CS.SF.identityresource.manage",
        "HCL.CS.SF.adminuser.manage",
        "HCL.CS.SF.securitytoken.manage",
        "HCL.CS.SF.notification.read",
        "HCL.CS.SF.notification.manage"
    };

    public IList<string> AdminRoles { get; set; } = new List<string>
    {
        "SystemAdmin",
        "ClientAdmin",
        "RoleAdmin",
        "ResourceAdmin",
        "HCLCSSFAdmin",
        "Admin"
    };

    public bool AllowInsecureTls { get; set; } = false;
}
