/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Entities.Endpoint;

/// <summary>
/// Represents an allowed redirect URI for an OAuth2/OIDC client.
/// During authorization code and implicit flows, the identity server validates
/// the requested redirect_uri against the registered entries to prevent authorization code interception attacks.
/// </summary>
public class ClientRedirectUris : BaseEntity
{
    /// <summary>Foreign key referencing the owning <see cref="Clients"/> entity.</summary>
    public Guid ClientId { get; set; }

    /// <summary>The absolute URI the identity server may redirect to after authentication.</summary>
    public string RedirectUri { get; set; }

    /// <summary>Navigation property to the parent client.</summary>
    public Clients Client { get; set; }
}
