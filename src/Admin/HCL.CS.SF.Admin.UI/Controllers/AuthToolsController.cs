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

namespace HCL.CS.SF.Admin.UI.Controllers;

[Authorize(Policy = "AdminOnly")]
[Route("Admin/AuthTools")]
public class AuthToolsController : Controller
{
    private readonly OAuthAdminOptions _options;
    private readonly IApiClientService _apiClient;

    public AuthToolsController(IOptions<OAuthAdminOptions> options, IApiClientService apiClient)
    {
        _options = options.Value;
        _apiClient = apiClient;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        ViewData["Title"] = "Auth Tools";
        ViewData["Breadcrumb"] = "Operations / Auth Tools";
        ViewBag.Authority = _options.Authority;
        return View();
    }

    [HttpPost("TestEndpoint")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TestEndpoint([FromBody] EndpointTestRequest request)
    {
        try
        {
            using var httpClient = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = _options.AllowInsecureTls
                    ? HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    : null
            });

            httpClient.Timeout = TimeSpan.FromSeconds(_options.ApiTimeoutSeconds);

            var httpRequest = new HttpRequestMessage(
                string.Equals(request.Method, "POST", StringComparison.OrdinalIgnoreCase)
                    ? HttpMethod.Post
                    : HttpMethod.Get,
                request.Url);

            if (request.Headers != null)
            {
                foreach (var header in request.Headers)
                {
                    if (!string.IsNullOrWhiteSpace(header.Key) && !string.IsNullOrWhiteSpace(header.Value))
                    {
                        httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Body) &&
                string.Equals(request.Method, "POST", StringComparison.OrdinalIgnoreCase))
            {
                httpRequest.Content = new StringContent(request.Body, System.Text.Encoding.UTF8,
                    request.ContentType ?? "application/json");
            }

            var response = await httpClient.SendAsync(httpRequest);
            var responseBody = await response.Content.ReadAsStringAsync();

            var responseHeaders = new Dictionary<string, string>();
            foreach (var h in response.Headers)
            {
                responseHeaders[h.Key] = string.Join(", ", h.Value);
            }
            foreach (var h in response.Content.Headers)
            {
                responseHeaders[h.Key] = string.Join(", ", h.Value);
            }

            return Json(new
            {
                statusCode = (int)response.StatusCode,
                statusText = response.ReasonPhrase,
                headers = responseHeaders,
                body = responseBody
            });
        }
        catch (TaskCanceledException)
        {
            return Json(new { statusCode = 0, statusText = "Timeout", headers = new Dictionary<string, string>(), body = "Request timed out." });
        }
        catch (Exception ex)
        {
            return Json(new { statusCode = 0, statusText = "Error", headers = new Dictionary<string, string>(), body = ex.Message });
        }
    }
}

public class EndpointTestRequest
{
    public string Url { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
    public string? Body { get; set; }
    public string? ContentType { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
}
