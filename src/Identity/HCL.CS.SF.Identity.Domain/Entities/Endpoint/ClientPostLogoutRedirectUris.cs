/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Entities.Endpoint;

/// <summary>
/// Represents an allowed post-logout redirect URI for an OAuth2/OIDC client.
/// After a user signs out, the identity server will only redirect to URIs registered here,
/// preventing open-redirect attacks during the logout flow (per OpenID Connect RP-Initiated Logout).
/// </summary>
public class ClientPostLogoutRedirectUris : BaseEntity
{
    /// <summary>Foreign key referencing the owning <see cref="Clients"/> entity.</summary>
    public Guid ClientId { get; set; }

    /// <summary>The absolute URI the identity server may redirect to after logout.</summary>
    public string PostLogoutRedirectUri { get; set; }

    /// <summary>Navigation property to the parent client.</summary>
    public Clients Client { get; set; }
}
