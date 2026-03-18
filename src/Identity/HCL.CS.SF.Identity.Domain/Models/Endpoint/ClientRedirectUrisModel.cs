/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Endpoint;

/// <summary>
/// Represents a registered redirect URI for an OAuth 2.0 client.
/// The authorization server validates the redirect_uri parameter against these
/// registered values to prevent open-redirect attacks (RFC 6749 Section 3.1.2.2).
/// </summary>
public class ClientRedirectUrisModel : BaseModel
{
    /// <summary>The parent client's unique identifier (foreign key).</summary>
    public Guid ClientId { get; set; }

    /// <summary>The registered redirect URI where authorization codes and tokens are delivered.</summary>
    public string RedirectUri { get; set; }
}
