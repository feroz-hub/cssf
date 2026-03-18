/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Contracts.Requests;

/// <summary>
/// Immutable request used to obtain an OAuth access token via the client-credentials grant.
/// </summary>
/// <param name="ClientId">The unique identifier of the registered OAuth client.</param>
/// <param name="ClientSecret">The client secret used for authentication.</param>
/// <param name="Scope">Space-delimited list of requested scopes.</param>
public sealed record AuthTokenRequest(string ClientId, string ClientSecret, string Scope);
