/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Endpoint;

/// <summary>
/// Represents the message payload used during OIDC end-session (logout) processing.
/// Carries the data needed to identify which user session to terminate and where to
/// redirect the user after logout.
/// </summary>
public class LogoutMessageModel
{
    /// <summary>The client identifier that initiated the logout request.</summary>
    public string ClientId { get; set; }

    /// <summary>The URI to redirect the user to after logout is complete, as registered by the client.</summary>
    public string PostLogoutRedirectUri { get; set; }

    /// <summary>The subject identifier (sub claim) of the user being logged out.</summary>
    public string SubjectId { get; set; }

    /// <summary>The session identifier tying this logout to a specific authentication session.</summary>
    public string SessionId { get; set; }

    /// <summary>Collection of all client identifiers that share the user's session and need to be notified of logout.</summary>
    public IEnumerable<string> ClientIdCollection { get; set; }

    /// <summary>Additional parameters passed along with the logout request (e.g., id_token_hint fields).</summary>
    public Dictionary<string, string> Parameters { get; set; } = new();

    /// <summary>Indicates whether at least one client is associated with this logout request.</summary>
    public bool HasClient => !string.IsNullOrWhiteSpace(ClientId) || ClientIdCollection?.Any() == true;
}
