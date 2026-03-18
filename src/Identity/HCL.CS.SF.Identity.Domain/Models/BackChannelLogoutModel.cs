/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Models.Endpoint;

namespace HCL.CS.SF.Domain.Models;

/// <summary>
/// Represents the data required to perform an OIDC back-channel logout request.
/// The identity server sends an HTTP POST with a logout token to each client's
/// <see cref="LogoutUri"/> so sessions can be terminated server-side without browser involvement.
/// </summary>
public class BackChannelLogoutModel
{
    /// <summary>The OAuth 2.0 client identifier of the relying party to be logged out.</summary>
    public string ClientId { get; set; }

    /// <summary>The subject identifier (sub claim) of the user whose session is being terminated.</summary>
    public string SubjectId { get; set; }

    /// <summary>The session identifier that ties the user's authentication session to this client.</summary>
    public string SessionId { get; set; }

    /// <summary>The back-channel logout URI registered by the client to receive logout token POST requests.</summary>
    public string LogoutUri { get; set; }

    /// <summary>Indicates whether the logout token must include the session identifier (sid claim).</summary>
    public bool SessionIdRequired { get; set; }

    /// <summary>The full client configuration model associated with this logout operation.</summary>
    public ClientsModel Client { get; set; }
}
