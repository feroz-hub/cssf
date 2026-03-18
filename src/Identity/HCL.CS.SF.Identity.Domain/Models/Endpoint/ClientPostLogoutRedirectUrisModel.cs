/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Endpoint;

/// <summary>
/// Represents a registered post-logout redirect URI for an OAuth 2.0 client.
/// After the OIDC end-session process completes, the identity server redirects
/// the user to one of these URIs if it matches the post_logout_redirect_uri parameter.
/// </summary>
public class ClientPostLogoutRedirectUrisModel : BaseModel
{
    /// <summary>The parent client's unique identifier (foreign key).</summary>
    public Guid ClientId { get; set; }

    /// <summary>The registered URI where the user is redirected after logout.</summary>
    public string PostLogoutRedirectUri { get; set; }
}
