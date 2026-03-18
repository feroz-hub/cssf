/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using HCL.CS.SF.DemoClientMvc.Constants;
using HCL.CS.SF.DemoClientMvc.Models;

namespace HCL.CS.SF.DemoClientMvc.Controllers;

public class SignOutController(IHttpClientFactory httpClientFactory) : Controller
{
    [AllowAnonymous]
    public async Task<IActionResult> FrontChannelLogout(string sid)
    {
        if (User.Identity is not { IsAuthenticated: true }) return NoContent();
        var currentSid = User.FindFirst("sid")?.Value ?? "";
        if (!string.Equals(currentSid, sid, StringComparison.Ordinal)) return NoContent();
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return NoContent();
    }

    [AllowAnonymous]
    public async Task<IActionResult> BackChannelLogout(string logoutToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(logoutToken)) return ShowErrorPage("Invalid logout token");

            var token = new JwtSecurityTokenHandler().ReadJwtToken(logoutToken);
            if (token == null) return NoContent();
            var isValidToken = false;
            if (!token.Header.TryGetValue("alg", out var algorithmObj)) return ShowErrorPage("Invalid logout token");

            var algorithm = algorithmObj.ToString();
            if (!string.Equals(algorithm, SecurityAlgorithms.RsaSha256, StringComparison.Ordinal)
                && !string.Equals(algorithm, SecurityAlgorithms.EcdsaSha256, StringComparison.Ordinal))
                return ShowErrorPage("Invalid logout token");

            var logoutTokenClaims = await ValidateAsymmetricTokenAsync(logoutToken);
            isValidToken = ValidateToken(logoutTokenClaims);

            if (isValidToken)
            {
                HttpContext.Session.Clear();
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            else
            {
                return ShowErrorPage("Invalid logout token");
            }

            return NoContent();
        }
        catch (Exception)
        {
            return RedirectToAction("Error", "Error",
                new { errorCode = "", errorMessage = "Back-channel logout failed." });
        }
    }

    private static bool ValidateToken(ClaimsPrincipal logoutTokenClaims)
    {
        if (logoutTokenClaims.FindFirst("sub") == null && logoutTokenClaims.FindFirst("sid") == null) return false;

        var eventsJson = logoutTokenClaims.FindFirst("events")?.Value;
        if (string.IsNullOrWhiteSpace(eventsJson)) return false;

        var events = JObject.Parse(eventsJson);
        var logoutEvent = events.TryGetValue(ApplicationConstants.BackChannelLogoutEvent, out var values);
        if (values == null) return false;

        var nonce = logoutTokenClaims.FindFirstValue("nonce");
        return string.IsNullOrWhiteSpace(nonce);
    }

    private async Task<ClaimsPrincipal> ValidateAsymmetricTokenAsync(string token)
    {
        var httpClient = httpClientFactory.CreateClient(nameof(SignOutController));
        var result = await httpClient.GetAsync(ApplicationConstants.JwksEndpoint);
        result.EnsureSuccessStatusCode();
        var json = await result.Content.ReadAsStringAsync();
        var data = JObject.Parse(json);

        if (data["keys"] is not JArray webKeys || webKeys.Count == 0)
            throw new SecurityTokenValidationException("No signing keys available.");

        var keys = (from webKey in webKeys
                let kty = GetValueCaseInsensitive(webKey, "kty")
                let alg = GetValueCaseInsensitive(webKey, "alg")
                let kid = GetValueCaseInsensitive(webKey, "kid")
                let x = GetValueCaseInsensitive(webKey, "x")
                let y = GetValueCaseInsensitive(webKey, "y")
                let crv = GetValueCaseInsensitive(webKey, "crv")
                let e = GetValueCaseInsensitive(webKey, "e")
                let n = GetValueCaseInsensitive(webKey, "n")
                let use = GetValueCaseInsensitive(webKey, "use")
                let x5t = GetValueCaseInsensitive(webKey, "x5t")
                select new JsonWebKey
                {
                    Kty = kty,
                    Alg = alg,
                    Kid = kid,
                    X = x,
                    Y = y,
                    Crv = crv,
                    E = e,
                    N = n,
                    Use = use,
                    X5t = x5t
                }).Cast<SecurityKey>()
            .ToList();

        var validationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = ApplicationConstants.ClientId,
            ValidateIssuer = true,
            ValidIssuer = ApplicationConstants.IssuerUri,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = keys,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ValidAlgorithms =
            [
                SecurityAlgorithms.RsaSha256,
                SecurityAlgorithms.EcdsaSha256
            ]
        };
        var principal =
            new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out var rawValidatedToken);
        return principal;
    }

    private static string? GetValueCaseInsensitive(JToken token, string propertyName)
    {
        var pascalCaseName = char.ToUpperInvariant(propertyName[0]) + propertyName[1..];
        var property = token[propertyName] ?? token[pascalCaseName] ?? token[propertyName.ToUpperInvariant()];
        return property?.ToString();
    }

    private RedirectToActionResult ShowErrorPage(string errorMessage)
    {
        var errorModel = new ErrorViewModel
        {
            ErrorCode = "APP_ERROR",
            ErrorMessage = errorMessage
        };

        return RedirectToAction("Error", "Error", errorModel);
    }
}
