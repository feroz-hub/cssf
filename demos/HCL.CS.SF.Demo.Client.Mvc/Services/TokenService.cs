/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using HCL.CS.SF.DemoClientMvc.Interface;
using HCL.CS.SF.DemoClientMvc.Options;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Response;

namespace HCL.CS.SF.DemoClientMvc.Services;

public sealed class TokenService(
    IHttpClientFactory httpClientFactory,
    IHttpContextAccessor httpContextAccessor,
    IOptions<OAuthClientOptions> options,
    ILogger<TokenService> logger)
    : ITokenService
{
    private readonly OAuthClientOptions options = options.Value;

    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var context = httpContextAccessor.HttpContext;
        if (context == null) return null;

        return await context.GetTokenAsync("access_token");
    }

    public async Task<string?> GetRefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        var context = httpContextAccessor.HttpContext;
        if (context == null) return null;

        return await context.GetTokenAsync("refresh_token");
    }

    public async Task<string?> GetIdTokenAsync(CancellationToken cancellationToken = default)
    {
        var context = httpContextAccessor.HttpContext;
        if (context == null) return null;

        return await context.GetTokenAsync("id_token");
    }

    public async Task<DateTimeOffset?> GetAccessTokenExpiresAtAsync(CancellationToken cancellationToken = default)
    {
        var context = httpContextAccessor.HttpContext;
        if (context == null) return null;

        var expiresAt = await context.GetTokenAsync("expires_at");
        if (!DateTimeOffset.TryParse(expiresAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind,
                out var parsed)) return null;

        return parsed;
    }

    public async Task<bool> RefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        var context = httpContextAccessor.HttpContext;
        if (context == null) return false;

        var authenticateResult = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!authenticateResult.Succeeded || authenticateResult.Properties == null || authenticateResult.Principal == null)
        {
            logger.LogWarning("Refresh token requested, but no authenticated cookie principal was found.");
            return false;
        }

        return await RefreshTokenInternalAsync(
            context,
            authenticateResult.Properties,
            authenticateResult.Principal,
            persistCookie: true,
            cancellationToken);
    }

    public async Task<bool> RefreshTokenAsync(
        AuthenticationProperties properties,
        ClaimsPrincipal principal,
        bool persistCookie,
        CancellationToken cancellationToken = default)
    {
        var context = httpContextAccessor.HttpContext;
        if (context == null) return false;

        return await RefreshTokenInternalAsync(context, properties, principal, persistCookie, cancellationToken);
    }

    private async Task<bool> RefreshTokenInternalAsync(
        HttpContext context,
        AuthenticationProperties properties,
        ClaimsPrincipal principal,
        bool persistCookie,
        CancellationToken cancellationToken)
    {
        try
        {
            var refreshToken = properties.GetTokenValue("refresh_token");
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                logger.LogWarning("Refresh token requested, but no refresh token exists in auth properties.");
                return false;
            }

            var form = new Dictionary<string, string>
            {
                [OpenIdConstants.TokenRequest.GrantType] = OpenIdConstants.GrantTypes.RefreshToken,
                [OpenIdConstants.TokenRequest.RefreshToken] = refreshToken
            };

            var client = httpClientFactory.CreateClient(nameof(TokenService));
            client.DefaultRequestHeaders.Authorization = BuildClientBasicAuthHeader();
            using var response = await client.PostAsync(options.TokenEndpoint, new FormUrlEncodedContent(form),
                cancellationToken);
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = TryDeserialize<ErrorResponseResultModel>(payload);
                logger.LogWarning(
                    "Refresh token request failed. StatusCode: {StatusCode}, ErrorCode: {ErrorCode}, ErrorDescription: {ErrorDescription}",
                    (int)response.StatusCode,
                    errorResponse?.error,
                    errorResponse?.error_description);
                return false;
            }

            var tokenResponse = TryDeserialize<TokenResponseResultModel>(payload);
            if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.access_token))
            {
                logger.LogWarning("Refresh token response did not include an access token.");
                return false;
            }

            var expiresAt =
                DateTimeOffset.UtcNow.AddSeconds(tokenResponse.expires_in > 0 ? tokenResponse.expires_in : 1);

            properties.UpdateTokenValue("access_token", tokenResponse.access_token);
            properties.UpdateTokenValue("expires_at", expiresAt.ToString("o", CultureInfo.InvariantCulture));

            if (!string.IsNullOrWhiteSpace(tokenResponse.refresh_token))
                properties.UpdateTokenValue("refresh_token", tokenResponse.refresh_token);

            if (!string.IsNullOrWhiteSpace(tokenResponse.id_token))
                properties.UpdateTokenValue("id_token", tokenResponse.id_token);

            properties.IssuedUtc = DateTimeOffset.UtcNow;
            properties.ExpiresUtc = expiresAt;

            if (persistCookie)
                await context.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    properties);

            logger.LogInformation("Access token refreshed successfully.");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected exception while refreshing the token.");
            return false;
        }
    }

    public async Task<bool> RevokeTokenAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(options.RevocationEndpoint))
        {
            logger.LogWarning("Revocation requested, but revocation endpoint is not configured.");
            return false;
        }

        var accessToken = await GetAccessTokenAsync(cancellationToken);
        var refreshToken = await GetRefreshTokenAsync(cancellationToken);

        var accessRevoked = await RevokeTokenInternalAsync(accessToken, OpenIdConstants.TokenHintTypes.AccessToken,
            cancellationToken);
        var refreshRevoked = await RevokeTokenInternalAsync(refreshToken, OpenIdConstants.TokenHintTypes.RefreshToken,
            cancellationToken);

        return accessRevoked || refreshRevoked;
    }

    private async Task<bool> RevokeTokenInternalAsync(string? token, string tokenHint,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;

        try
        {
            var form = new Dictionary<string, string>
            {
                [OpenIdConstants.RevocationRequest.Token] = token,
                [OpenIdConstants.RevocationRequest.TokenTypeHint] = tokenHint
            };

            var client = httpClientFactory.CreateClient(nameof(TokenService));
            client.DefaultRequestHeaders.Authorization = BuildClientBasicAuthHeader();
            using var response = await client.PostAsync(options.RevocationEndpoint, new FormUrlEncodedContent(form),
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Token revocation succeeded for token hint {TokenHint}.", tokenHint);
                return true;
            }

            logger.LogWarning(
                "Token revocation failed for token hint {TokenHint}. StatusCode: {StatusCode}.",
                tokenHint,
                (int)response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected exception while revoking token with hint {TokenHint}.", tokenHint);
            return false;
        }
    }

    private static T? TryDeserialize<T>(string payload)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(payload);
        }
        catch
        {
            return default;
        }
    }

    private AuthenticationHeaderValue BuildClientBasicAuthHeader()
    {
        var clientId = Uri.EscapeDataString(options.ClientId);
        var clientSecret = Uri.EscapeDataString(options.ClientSecret);
        var raw = $"{clientId}:{clientSecret}";
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
        return new AuthenticationHeaderValue("Basic", encoded);
    }
}
