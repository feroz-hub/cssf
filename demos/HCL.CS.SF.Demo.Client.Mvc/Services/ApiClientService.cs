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
using System.Text.Json;
using HCL.CS.SF.DemoClientMvc.Interface;

namespace HCL.CS.SF.DemoClientMvc.Services;

public sealed class ApiClientService : IApiClientService
{
    public const string ResourceApiClientName = "ResourceApiClient";

    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<ApiClientService> logger;
    private readonly ITokenService tokenService;

    public ApiClientService(
        IHttpClientFactory httpClientFactory,
        ITokenService tokenService,
        ILogger<ApiClientService> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.tokenService = tokenService;
        this.logger = logger;
    }

    public Task<ApiClientResponse> GetAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        return SendWithRetryAsync(
            () => new HttpRequestMessage(HttpMethod.Get, NormalizeRelativePath(relativePath)),
            cancellationToken);
    }

    public Task<ApiClientResponse> PostAsync<TRequest>(string relativePath, TRequest payload,
        CancellationToken cancellationToken = default)
    {
        return SendWithRetryAsync(
            () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, NormalizeRelativePath(relativePath));
                request.Content =
                    new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                return request;
            },
            cancellationToken);
    }

    private async Task<ApiClientResponse> SendWithRetryAsync(Func<HttpRequestMessage> requestFactory,
        CancellationToken cancellationToken)
    {
        var firstAttempt = await SendAsync(requestFactory, cancellationToken);
        if (firstAttempt.StatusCode != (int)HttpStatusCode.Unauthorized) return firstAttempt;

        logger.LogInformation("Resource API call returned 401. Attempting one refresh-token retry.");

        var refreshed = await tokenService.RefreshTokenAsync(cancellationToken);
        if (!refreshed)
            return new ApiClientResponse
            {
                Succeeded = false,
                StatusCode = firstAttempt.StatusCode,
                ResponseBody = firstAttempt.ResponseBody,
                ErrorMessage = "Access token refresh failed after a 401 response."
            };

        return await SendAsync(requestFactory, cancellationToken);
    }

    private async Task<ApiClientResponse> SendAsync(Func<HttpRequestMessage> requestFactory,
        CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient(ResourceApiClientName);
        using var request = requestFactory.Invoke();

        var accessToken = await tokenService.GetAccessTokenAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(accessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await client.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.IsSuccessStatusCode)
            return new ApiClientResponse
            {
                Succeeded = true,
                StatusCode = (int)response.StatusCode,
                ResponseBody = body
            };

        logger.LogWarning(
            "Resource API request failed. Method: {Method}, Uri: {Uri}, StatusCode: {StatusCode}",
            request.Method,
            request.RequestUri,
            (int)response.StatusCode);

        return new ApiClientResponse
        {
            Succeeded = false,
            StatusCode = (int)response.StatusCode,
            ResponseBody = body,
            ErrorMessage = $"Resource API request failed with status {(int)response.StatusCode}."
        };
    }

    private static string NormalizeRelativePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return "/";

        if (Uri.TryCreate(relativePath, UriKind.Absolute, out _))
            throw new ArgumentException("Absolute URLs are not allowed for resource API calls.", nameof(relativePath));

        return relativePath.StartsWith("/", StringComparison.Ordinal) ? relativePath : "/" + relativePath;
    }
}
