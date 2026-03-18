/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;

/// <summary>
/// Handles OIDC back-channel logout by sending logout tokens to all clients that participated
/// in the user's session, as defined by the OpenID Connect Back-Channel Logout specification.
/// </summary>
public interface IBackChannelLogoutService
{
    /// <summary>
    /// Sends back-channel logout notifications to all clients in the current session.
    /// </summary>
    Task ProcessLogoutAsync();
}
