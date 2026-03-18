/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Endpoint.Request;

/// <summary>
/// Represents a processed OIDC logout request, extending <see cref="LogoutMessageModel"/>
/// with the end-session callback URL and a flag to control whether a sign-out confirmation
/// prompt is shown to the user.
/// </summary>
public class LogoutRequestModel : LogoutMessageModel
{
    /// <summary>
    /// Constructs a logout request from a logout message and end-session callback URL.
    /// Copies all relevant fields from the message if provided.
    /// </summary>
    /// <param name="endSessionCallBackUrl">The callback URL for front-channel logout notifications.</param>
    /// <param name="message">The logout message containing client and session details.</param>
    public LogoutRequestModel(string endSessionCallBackUrl, LogoutMessageModel message)
    {
        if (message != null)
        {
            ClientId = message.ClientId;
            PostLogoutRedirectUri = message.PostLogoutRedirectUri;
            SubjectId = message.SubjectId;
            SessionId = message.SessionId;
            ClientIdCollection = message.ClientIdCollection;
            Parameters = message.Parameters;
        }

        EndSessionCallBackUrl = endSessionCallBackUrl;
    }

    /// <summary>The URL invoked for end-session callback to notify clients of the logout via front-channel.</summary>
    public string EndSessionCallBackUrl { get; set; }

    /// <summary>Indicates whether to show a sign-out confirmation prompt. True when no specific client initiated the logout.</summary>
    public bool ShowSignoutPrompt => string.IsNullOrWhiteSpace(ClientId);
}
