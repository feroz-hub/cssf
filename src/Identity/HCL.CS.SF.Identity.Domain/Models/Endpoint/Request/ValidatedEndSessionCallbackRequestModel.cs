/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Models.Endpoint.Validation;

namespace HCL.CS.SF.Domain.Models.Endpoint.Request;

/// <summary>
/// Represents a validated end-session callback request used during front-channel logout.
/// Contains the list of client front-channel logout URIs that should be rendered in iframes
/// to trigger client-side session cleanup.
/// </summary>
public class ValidatedEndSessionCallbackRequestModel : ValidatedBaseModel
{
    /// <summary>The front-channel logout URIs to be rendered as iframes in the browser for each client.</summary>
    public IEnumerable<string> FrontChannelLogoutUrls { get; set; }
}
