/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using HCL.CS.SF.Domain.Models.Endpoint;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;

/// <summary>
/// Manages server-side user sessions for the authorization server, including session cookie
/// creation, client tracking per session, and retrieval of the authenticated principal and
/// authentication properties from the HTTP context.
/// </summary>
public interface ISessionManagementService
{
    /// <summary>Records that the specified client participated in the current session (for back-channel logout).</summary>
    /// <param name="clientId">The client identifier to add to the session.</param>
    Task AddClientAsync(string clientId);

    /// <summary>Creates a session cookie and binds it to the specified principal and authentication properties.</summary>
    /// <param name="principal">The authenticated user's claims principal.</param>
    /// <param name="properties">The authentication properties (e.g., token expiration, persistence flag).</param>
    Task CreateAndBindSessionCookieAsync(ClaimsPrincipal principal, AuthenticationProperties properties);

    /// <summary>Returns the list of client identifiers that participated in the current session.</summary>
    Task<IList<string>> GetClientListAsync();

    /// <summary>Returns the current session identifier.</summary>
    Task<string> GetSessionId();

    /// <summary>Retrieves the authentication properties for the current session.</summary>
    Task<AuthenticationPropertiesModel> GetAuthenticationAsync();

    /// <summary>Returns the authentication scheme name used for the current session.</summary>
    Task<string> GetAuthenticationSchemeAsync();

    /// <summary>Retrieves the authenticated user's <see cref="ClaimsPrincipal"/> from the HTTP context.</summary>
    Task<ClaimsPrincipal> GetUserPrincipalFromContextAsync();

    /// <summary>Retrieves the <see cref="AuthenticationProperties"/> from the HTTP context.</summary>
    Task<AuthenticationProperties> GetPropertiesFromContextAsync();
}
