/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using HCL.CS.SF.DemoClientMvc.Constants;
using HCL.CS.SF.DemoClientMvc.Interface;
using HCL.CS.SF.Domain;

namespace HCL.CS.SF.DemoClientMvc.Service;

public class HttpService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    : IHttpService
{
    public async Task<T> PostAsync<T>(string url, object Value)
    {
        var content = string.Empty;
        try
        {
            url = ApplicationConstants.AuthenticationServerBaseUrl + url;
            var httpClient = httpClientFactory.CreateClient();
            var httpResponse = await httpClient.PostAsync(url,
                new StringContent(JsonConvert.SerializeObject(Value), Encoding.UTF8, "application/json"));
            content = await httpResponse.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<T>(content);
            if (typeof(FrameworkResult) != typeof(T))
            {
                if (httpResponse.StatusCode != HttpStatusCode.OK)
                {
                    content = httpResponse.ReasonPhrase;
                    throw new Exception(httpResponse.ReasonPhrase);
                }
            }
            else if (result == null)
            {
                if (httpResponse.StatusCode != HttpStatusCode.OK)
                {
                    content = httpResponse.ReasonPhrase;
                    throw new Exception(httpResponse.ReasonPhrase);
                }
            }

            return result;
        }
        catch (Exception)
        {
            throw new Exception(content);
        }
    }

    public async Task<T> PostSecureAsync<T>(string url, object Value)
    {
        var content = string.Empty;
        try
        {
            url = ApplicationConstants.AuthenticationServerBaseUrl + url;
            var httpClient = httpClientFactory.CreateClient();
            var accessToken = await httpContextAccessor.HttpContext.GetTokenAsync("access_token");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var httpResponse = await httpClient.PostAsync(url,
                new StringContent(JsonConvert.SerializeObject(Value), Encoding.UTF8, "application/json"));
            content = await httpResponse.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<T>(content);
            if (typeof(FrameworkResult) != typeof(T))
            {
                if (httpResponse.StatusCode != HttpStatusCode.OK)
                {
                    content = httpResponse.ReasonPhrase;
                    throw new Exception(httpResponse.ReasonPhrase);
                }
            }
            else if (result == null)
            {
                if (httpResponse.StatusCode != HttpStatusCode.OK)
                {
                    content = httpResponse.ReasonPhrase;
                    throw new Exception(httpResponse.ReasonPhrase);
                }
            }

            return result;
        }
        catch (Exception)
        {
            throw new Exception(content);
        }
    }

    public async Task<T> PostSecureResourceServerAsync<T>(string url, object Value)
    {
        var content = string.Empty;
        try
        {
            url = ApplicationConstants.ResourceServerBaseUrl + url;
            var httpClient = httpClientFactory.CreateClient();
            var accessToken = await httpContextAccessor.HttpContext.GetTokenAsync("access_token");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var httpResponse = await httpClient.PostAsync(url,
                new StringContent(JsonConvert.SerializeObject(Value), Encoding.UTF8, "application/json"));
            content = await httpResponse.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<T>(content);
            return result;
        }
        catch (Exception)
        {
            throw new Exception(content);
        }
    }
}
