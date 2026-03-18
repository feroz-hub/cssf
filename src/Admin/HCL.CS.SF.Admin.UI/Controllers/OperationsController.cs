/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using HCL.CS.SF.Admin.UI.Interfaces;
using HCL.CS.SF.Admin.UI.Options;
using HCL.CS.SF.Admin.UI.Services;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace HCL.CS.SF.Admin.UI.Controllers;

[Authorize(Policy = "AdminOnly")]
[Route("Admin/Operations")]
public class OperationsController : Controller
{
    private readonly OAuthAdminOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITokenService _tokenService;

    public OperationsController(
        IOptions<OAuthAdminOptions> options,
        IHttpClientFactory httpClientFactory,
        ITokenService tokenService)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _tokenService = tokenService;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        ViewData["Title"] = "Operations Center";
        ViewData["Breadcrumb"] = "Operations";
        ViewBag.Authority = _options.Authority;
        return View();
    }

    [HttpGet("Endpoints")]
    public IActionResult Endpoints()
    {
        ViewData["Title"] = "Protocol Endpoints";
        ViewData["Breadcrumb"] = "Operations / Endpoints";
        ViewBag.Authority = _options.Authority;
        return View();
    }

    [HttpGet("ApiExplorer")]
    public IActionResult ApiExplorer()
    {
        ViewData["Title"] = "API Explorer";
        ViewData["Breadcrumb"] = "Operations / API Explorer";
        return View();
    }

    [HttpPost("ApiExplorer/Call")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApiExplorerCall([FromBody] ApiExplorerCallRequest request, CancellationToken ct)
    {
        if (request == null)
            return Json(new { ok = false, message = "Missing request payload.", data = (object?)null });

        var baseKey = (request.Base ?? "api").Trim();
        if (!string.Equals(baseKey, "api", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(baseKey, "installer", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(baseKey, "demo", StringComparison.OrdinalIgnoreCase))
        {
            return Json(new { ok = false, message = "Invalid base target.", data = (object?)null });
        }

        var path = (request.Path ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(path) || !path.StartsWith("/", StringComparison.Ordinal))
        {
            return Json(new { ok = false, message = "Path must start with '/'.", data = (object?)null });
        }

        var baseUrl = string.Equals(baseKey, "api", StringComparison.OrdinalIgnoreCase)
            ? _options.ResourceApiBaseUrl
            : _options.Authority;

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return Json(new { ok = false, message = "Base URL is not configured.", data = (object?)null });
        }

        var url = $"{baseUrl.TrimEnd('/')}{path}";

        var methodRaw = (request.Method ?? "POST").Trim().ToUpperInvariant();
        var effectiveMethod = string.Equals(baseKey, "api", StringComparison.OrdinalIgnoreCase)
            ? HttpMethod.Post
            : (methodRaw == "GET" ? HttpMethod.Get : HttpMethod.Post);

        string? jsonToSend = null;
        string? contentType = null;
        object? bodyPayload = null;

        if (effectiveMethod == HttpMethod.Post)
        {
            var raw = request.JsonBody ?? string.Empty;
            raw = raw.Trim();

            // Next.js explorer sends JSON string "" when no JSON is provided.
            string jsonPayload;
            if (string.IsNullOrWhiteSpace(raw))
            {
                jsonPayload = "\"\"";
            }
            else
            {
                try
                {
                    using var doc = JsonDocument.Parse(raw);
                    jsonPayload = JsonSerializer.Serialize(doc.RootElement);
                }
                catch
                {
                    return Json(new { ok = false, message = "Body must be valid JSON.", data = (object?)null });
                }
            }

            jsonToSend = jsonPayload;
        }

        var correlationId = Guid.NewGuid().ToString();

        var httpClient = string.Equals(baseKey, "api", StringComparison.OrdinalIgnoreCase)
            ? _httpClientFactory.CreateClient(ApiClientService.ResourceApiClientName)
            : _httpClientFactory.CreateClient();

        // Ensure timeouts are consistent across bases.
        httpClient.Timeout = TimeSpan.FromSeconds(_options.ApiTimeoutSeconds);

        async Task<ApiClientResponse> SendOnceAsync()
        {
            using var msg = new HttpRequestMessage(effectiveMethod, url);
            if (effectiveMethod == HttpMethod.Post && jsonToSend != null)
                msg.Content = new StringContent(jsonToSend, Encoding.UTF8, "application/json");

            var accessToken = await _tokenService.GetAccessTokenAsync(ct);
            if (!string.IsNullOrWhiteSpace(accessToken))
                msg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            msg.Headers.TryAddWithoutValidation("X-Correlation-ID", correlationId);

            using var response = await httpClient.SendAsync(msg, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);

            // Reuse the same response parsing strategy as the Next.js explorer.
            contentType = response.Content.Headers.ContentType?.MediaType;
            if (!string.IsNullOrWhiteSpace(contentType) &&
                contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    bodyPayload = JsonSerializer.Deserialize<object>(responseBody);
                }
                catch
                {
                    bodyPayload = responseBody;
                }
            }
            else
            {
                bodyPayload = responseBody;
            }

            return new ApiClientResponse
            {
                Succeeded = response.IsSuccessStatusCode,
                StatusCode = (int)response.StatusCode,
                ResponseBody = responseBody,
                ErrorMessage = response.IsSuccessStatusCode ? string.Empty : $"HTTP {(int)response.StatusCode}"
            };
        }

        // Note: we rebuild HttpRequestMessage (and body) on each attempt.
        // 1) Attempt request
        var firstResponse = await SendOnceAsync();

        // 2) If unauthorized, refresh token and retry once.
        if (firstResponse.StatusCode == (int)HttpStatusCode.Unauthorized)
        {
            var refreshed = await _tokenService.RefreshTokenAsync(ct);
            if (refreshed)
                firstResponse = await SendOnceAsync();
        }

        var status = firstResponse.StatusCode;
        var message = firstResponse.Succeeded ? "Request completed." : $"HTTP {status}";

        if (!firstResponse.Succeeded && status == 404 && string.Equals(baseKey, "api", StringComparison.OrdinalIgnoreCase))
            message = $"404 Not Found. The Gateway accepts only POST for /Security/Api routes. URL: {url}";

        return Json(new
        {
            ok = firstResponse.Succeeded,
            message,
            data = new
            {
                status,
                contentType = contentType,
                body = bodyPayload
            }
        });
    }
}

public sealed class ApiExplorerCallRequest
{
    // Matches Next.js explorer payload: { base: "api"|"installer"|"demo", method: "GET"|"POST", path: "/...", jsonBody: string }
    public string? Base { get; set; } = "api";
    public string? Method { get; set; } = "POST";
    public string Path { get; set; } = string.Empty;
    public string? JsonBody { get; set; } = "";
}
