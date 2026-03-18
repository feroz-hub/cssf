/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Net;

namespace IntegrationTests.Endpoint.Setup;

public class UserAgentHandler : DelegatingHandler
{
    private readonly CookieContainer cookieContainer = new();

    public UserAgentHandler(HttpMessageHandler next)
        : base(next)
    {
    }

    public bool AllowCookies { get; set; } = true;
    public bool AllowAutoRedirect { get; set; } = true;
    public int ErrorRedirectLimit { get; set; } = 20;
    public int StopRedirectingAfter { get; set; } = int.MaxValue;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await SendCookiesAsync(request, cancellationToken);

        var redirectCount = 0;

        while (AllowAutoRedirect &&
               300 <= (int)response.StatusCode && (int)response.StatusCode < 400 &&
               redirectCount < StopRedirectingAfter)
        {
            if (redirectCount >= ErrorRedirectLimit)
                throw new InvalidOperationException(string.Format("Too many redirects. Error limit = {0}",
                    redirectCount));

            var location = response.Headers.Location;
            if (!location.IsAbsoluteUri) location = new Uri(response.RequestMessage.RequestUri, location);

            request = new HttpRequestMessage(HttpMethod.Get, location);

            response = await SendCookiesAsync(request, cancellationToken).ConfigureAwait(false);

            redirectCount++;
        }

        return response;
    }

    internal Cookie GetCookie(string uri, string name)
    {
        return cookieContainer.GetCookies(new Uri(uri)).FirstOrDefault(x => x.Name == name);
    }

    internal void RemoveCookie(string uri, string name)
    {
        var cookie = cookieContainer.GetCookies(new Uri(uri)).FirstOrDefault(x => x.Name == name);
        if (cookie != null) cookie.Expired = true;
    }

    protected async Task<HttpResponseMessage> SendCookiesAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (AllowCookies)
        {
            var cookieHeader = cookieContainer.GetCookieHeader(request.RequestUri);
            if (!string.IsNullOrEmpty(cookieHeader)) request.Headers.Add("Cookie", cookieHeader);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (AllowCookies && response.Headers.Contains("Set-Cookie"))
        {
            var responseCookieHeader = string.Join(",", response.Headers.GetValues("Set-Cookie"));
            cookieContainer.SetCookies(request.RequestUri, responseCookieHeader);
        }

        return response;
    }
}
