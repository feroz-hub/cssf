/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Net;

namespace IntegrationTests.Endpoint.Setup;

public class UserAgent : HttpClient
{
    public UserAgent(UserAgentHandler userAgentHandler)
        : base(userAgentHandler)
    {
        UserAgentHandler = userAgentHandler;
    }

    public UserAgentHandler UserAgentHandler { get; }

    public bool AllowCookies
    {
        get => UserAgentHandler.AllowCookies;
        set => UserAgentHandler.AllowCookies = value;
    }

    public bool AllowAutoRedirect
    {
        get => UserAgentHandler.AllowAutoRedirect;
        set => UserAgentHandler.AllowAutoRedirect = value;
    }

    public int ErrorRedirectLimit
    {
        get => UserAgentHandler.ErrorRedirectLimit;
        set => UserAgentHandler.ErrorRedirectLimit = value;
    }

    public int StopRedirectingAfter
    {
        get => UserAgentHandler.StopRedirectingAfter;
        set => UserAgentHandler.StopRedirectingAfter = value;
    }

    internal void RemoveCookie(string uri, string name)
    {
        UserAgentHandler.RemoveCookie(uri, name);
    }

    internal Cookie GetCookie(string uri, string name)
    {
        return UserAgentHandler.GetCookie(uri, name);
    }
}
