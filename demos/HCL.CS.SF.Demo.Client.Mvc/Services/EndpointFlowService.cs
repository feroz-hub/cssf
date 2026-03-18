/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using HCL.CS.SF.DemoClientMvc.Interface;
using HCL.CS.SF.DemoClientMvc.Options;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Response;

namespace HCL.CS.SF.DemoClientMvc.Services;

public sealed class EndpointFlowService(
    IHttpClientFactory httpClientFactory,
    IOptions<OAuthClientOptions> options)
    : IEndpointFlowService
{
    private readonly OAuthClientOptions options = options.Value;

    public async Task<EndpointTokenFlowResult> ExecuteResourceOwnerPasswordFlowAsync(
        string userName,
        string password,
        CancellationToken cancellationToken = default)
    {
        var form = new Dictionary<string, string>
        {
            [OpenIdConstants.TokenRequest.GrantType] = OpenIdConstants.GrantTypes.Password,
            [OpenIdConstants.TokenRequest.UserName] = userName,
            [OpenIdConstants.TokenRequest.Password] = password,
            [OpenIdConstants.TokenRequest.Scope] = BuildResourceOwnerScopes()
        };

        return await ExecuteTokenRequestAsync(form, cancellationToken, true);
    }

    public async Task<EndpointTokenFlowResult> ExecuteClientCredentialsFlowAsync(
        CancellationToken cancellationToken = default)
    {
        var form = new Dictionary<string, string>
        {
            [OpenIdConstants.TokenRequest.GrantType] = OpenIdConstants.GrantTypes.ClientCredentials,
            [OpenIdConstants.TokenRequest.Scope] = BuildClientCredentialsScopes()
        };

        return await ExecuteTokenRequestAsync(form, cancellationToken);
    }

    private async Task<EndpointTokenFlowResult> ExecuteTokenRequestAsync(
        Dictionary<string, string> form,
        CancellationToken cancellationToken,
        bool useResourceOwnerClient = false)
    {
        var client = httpClientFactory.CreateClient(nameof(EndpointFlowService));
        var credentials = ResolveClientCredentials(useResourceOwnerClient);
        client.DefaultRequestHeaders.Authorization =
            BuildClientAuthenticationHeader(credentials.clientId, credentials.clientSecret);
        using var response =
            await client.PostAsync(options.TokenEndpoint, new FormUrlEncodedContent(form), cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = TryDeserialize<ErrorResponseResultModel>(payload);
            var errorCode = error?.error ?? "token_request_failed";
            var errorDescription = error?.error_description ?? "Token request failed.";
            if (useResourceOwnerClient
                && string.Equals(errorCode, OpenIdConstants.Errors.UnauthorizedClient,
                    StringComparison.OrdinalIgnoreCase))
                errorDescription =
                    "Client is not authorized for password grant. Configure a dedicated ROP demo client with SupportedGrantTypes including 'password', then set OAuth:ResourceOwnerClientId and OAuth:ResourceOwnerClientSecret.";

            return new EndpointTokenFlowResult
            {
                ErrorCode = errorCode,
                ErrorDescription = errorDescription
            };
        }

        var token = TryDeserialize<TokenResponseResultModel>(payload);
        if (token == null)
            return new EndpointTokenFlowResult
            {
                ErrorCode = "invalid_token_response",
                ErrorDescription = "Token response could not be parsed."
            };

        return new EndpointTokenFlowResult
        {
            AccessToken = token.access_token,
            IdToken = token.id_token,
            RefreshToken = token.refresh_token,
            ExpiresIn = token.expires_in,
            TokenType = token.token_type,
            Claims = ParseClaims(token.access_token)
        };
    }

    private string BuildClientCredentialsScopes()
    {
        var identityScopes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "openid",
            "profile",
            "email",
            "address",
            "phone",
            "offline_access"
        };

        var scopes = options.Scopes
            .Where(scope => !string.IsNullOrWhiteSpace(scope))
            .Where(scope => !identityScopes.Contains(scope))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return string.Join(" ", scopes);
    }

    private string BuildResourceOwnerScopes()
    {
        if (options.ResourceOwnerScopes is { Count: > 0 })
            return string.Join(" ", options.ResourceOwnerScopes.Where(scope => !string.IsNullOrWhiteSpace(scope)));

        var ropScopes = options.Scopes
            .Where(scope => scope == "openid" || scope == "profile" || scope == "email")
            .ToArray();

        if (ropScopes.Length == 0) return "openid profile email";

        return string.Join(" ", ropScopes);
    }

    private (string clientId, string clientSecret) ResolveClientCredentials(bool useResourceOwnerClient)
    {
        if (useResourceOwnerClient
            && !string.IsNullOrWhiteSpace(options.ResourceOwnerClientId)
            && !string.IsNullOrWhiteSpace(options.ResourceOwnerClientSecret))
            return (options.ResourceOwnerClientId, options.ResourceOwnerClientSecret);

        return (options.ClientId, options.ClientSecret);
    }

    private static IReadOnlyDictionary<string, string> ParseClaims(string? accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken)) return new Dictionary<string, string>();

        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(accessToken)) return new Dictionary<string, string>();

        var token = handler.ReadJwtToken(accessToken);
        return token.Claims
            .GroupBy(claim => claim.Type)
            .ToDictionary(group => group.Key, group => string.Join(", ", group.Select(x => x.Value)));
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

    private static AuthenticationHeaderValue BuildClientAuthenticationHeader(string clientId, string clientSecret)
    {
        var escapedClientId = Uri.EscapeDataString(clientId);
        var escapedClientSecret = Uri.EscapeDataString(clientSecret);
        var credentials = $"{escapedClientId}:{escapedClientSecret}";
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
        return new AuthenticationHeaderValue("Basic", encoded);
    }
}
