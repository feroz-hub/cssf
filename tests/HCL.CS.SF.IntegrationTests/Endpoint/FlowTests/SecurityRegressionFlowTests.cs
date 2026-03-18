/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using IntegrationTests.ApiDomainModel;
using IntegrationTests.Endpoint.Helper;
using IntegrationTests.Endpoint.Setup;
using Xunit;
using HCL.CS.SF.Domain.Constants.Endpoint;

namespace IntegrationTests.Endpoint.FlowTests;

public class SecurityRegressionFlowTests : HCLCSSFFakeSetup
{
    private const string Category = "SecurityRegression";
    private const string RedirectUri = "http://127.0.0.1:63562/";
    private const string StrictPkceClient = "HCL.CS.SF S256 Client";

    [Fact]
    [Trait("Category", Category)]
    public async Task PkcePlainDowngradeAttempt_MustBeRejected()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(StrictPkceClient);
        client.Should().NotBeNull();

        var codeVerifier = GenerateCodeVerifier();
        FrontChannelClient.AllowAutoRedirect = false;
        var requestUrl = CreateAuthorizeRequestUrl(
            client.ClientId,
            "code",
            "openid email profile offline_access HCL.CS.SF.client",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: RedirectUri,
            nonce: Guid.NewGuid().ToString("N"));

        var response = await FrontChannelClient.GetAsync(requestUrl);
        var error = response.Headers.Location.ToString().ParseErrorQueryStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Result.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task MissingClientSecret_MustReturnInvalidClient()
    {
        var (client, code, codeVerifier) = await GetAuthorizationCodeAsync();
        using var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                [OpenIdConstants.TokenRequest.ClientId] = client.ClientId,
                [OpenIdConstants.TokenRequest.GrantType] = OpenIdConstants.GrantTypes.AuthorizationCode,
                [OpenIdConstants.TokenRequest.Code] = code,
                [OpenIdConstants.TokenRequest.RedirectUri] = RedirectUri,
                [OpenIdConstants.TokenRequest.CodeVerifier] = codeVerifier
            })
        };

        var response = await BackChannelClient.SendAsync(request);
        var payload = await response.ParseTokenErrorResponse();

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        payload.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidClient);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ConfidentialHmacClient_MustBeRejected()
    {
        var client = await FetchClientDetails("HCL.CS.SF HS256");
        client.Should().NotBeNull();

        var response = await SendTokenRequestWithBasicAuthAsync(
            client,
            new Dictionary<string, string>
            {
                [OpenIdConstants.TokenRequest.GrantType] = OpenIdConstants.GrantTypes.ClientCredentials,
                [OpenIdConstants.TokenRequest.Scope] = "clientapi"
            });
        var payload = await response.ParseTokenErrorResponse();

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        payload.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidClient);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task AuthorizationCode_MustBeSingleUse()
    {
        var (client, code, codeVerifier) = await GetAuthorizationCodeAsync();

        var firstResponse = await SendTokenRequestWithBasicAuthAsync(
            client,
            new Dictionary<string, string>
            {
                [OpenIdConstants.TokenRequest.GrantType] = OpenIdConstants.GrantTypes.AuthorizationCode,
                [OpenIdConstants.TokenRequest.Code] = code,
                [OpenIdConstants.TokenRequest.RedirectUri] = RedirectUri,
                [OpenIdConstants.TokenRequest.CodeVerifier] = codeVerifier
            });
        var firstPayload = await firstResponse.ParseTokenResponseResult();
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        firstPayload.access_token.Should().NotBeNullOrWhiteSpace();

        var replayResponse = await SendTokenRequestWithBasicAuthAsync(
            client,
            new Dictionary<string, string>
            {
                [OpenIdConstants.TokenRequest.GrantType] = OpenIdConstants.GrantTypes.AuthorizationCode,
                [OpenIdConstants.TokenRequest.Code] = code,
                [OpenIdConstants.TokenRequest.RedirectUri] = RedirectUri,
                [OpenIdConstants.TokenRequest.CodeVerifier] = codeVerifier
            });
        var replayPayload = await replayResponse.ParseTokenErrorResponse();

        replayResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        replayPayload.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidGrant);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task RevocationUnknownToken_MustBeIdempotent()
    {
        var client = await FetchClientDetails(StrictPkceClient);
        client.Should().NotBeNull();

        var randomToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var first = await SendRevocationRequestAsync(client, randomToken);
        var second = await SendRevocationRequestAsync(client, randomToken);

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        second.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task RevokedRefreshToken_IntrospectionMustReturnActiveFalse()
    {
        var (client, token) = await GetAuthorizationCodeTokenAsync();

        var revokeResponse = await SendRevocationRequestAsync(client, token.refresh_token);
        revokeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var introspectionResponse = await SendIntrospectionRequestAsync(client, token.refresh_token,
            OpenIdConstants.TokenResponseType.RefreshToken);
        var introspectionPayload = await introspectionResponse.ParseIntrospectionResponse();

        introspectionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        introspectionPayload.Active.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task RefreshTokenReuse_MustBeDetectedAndRejected()
    {
        var (client, token) = await GetAuthorizationCodeTokenAsync();

        var firstRefreshResponse = await SendTokenRequestWithBasicAuthAsync(
            client,
            new Dictionary<string, string>
            {
                [OpenIdConstants.TokenRequest.GrantType] = OpenIdConstants.GrantTypes.RefreshToken,
                [OpenIdConstants.TokenRequest.RefreshToken] = token.refresh_token
            });
        var firstRefreshPayload = await firstRefreshResponse.ParseTokenResponseResult();
        firstRefreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        firstRefreshPayload.refresh_token.Should().NotBeNullOrWhiteSpace();

        var replayRefreshResponse = await SendTokenRequestWithBasicAuthAsync(
            client,
            new Dictionary<string, string>
            {
                [OpenIdConstants.TokenRequest.GrantType] = OpenIdConstants.GrantTypes.RefreshToken,
                [OpenIdConstants.TokenRequest.RefreshToken] = token.refresh_token
            });
        var replayRefreshPayload = await replayRefreshResponse.ParseTokenErrorResponse();
        replayRefreshResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        replayRefreshPayload.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidGrant);

        var introspectionResponse = await SendIntrospectionRequestAsync(client, token.refresh_token,
            OpenIdConstants.TokenResponseType.RefreshToken);
        var introspectionPayload = await introspectionResponse.ParseIntrospectionResponse();
        introspectionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        introspectionPayload.Active.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task AudienceMismatch_MustFailTokenValidation()
    {
        var (_, token) = await GetAuthorizationCodeTokenAsync();
        var validator = new JwksTestHelper();
        var rawToken = await validator.ValidateToken(token.access_token, "security.HCL.CS.SF.com", "wrong.audience");
        rawToken.Should().BeNull();
    }

    private async Task<(ClientsModel Client, string Code, string CodeVerifier)> GetAuthorizationCodeAsync()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(StrictPkceClient);
        client.Should().NotBeNull();

        var codeVerifier = GenerateCodeVerifier();
        var codeChallenge = ComputeS256CodeChallenge(codeVerifier);
        FrontChannelClient.AllowAutoRedirect = false;
        var authorizeRequest = CreateAuthorizeRequestUrl(
            client.ClientId,
            "code",
            "openid email profile phone offline_access HCL.CS.SF.client",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeChallenge,
            codeChallengeMethod: "S256",
            maxAge: "60",
            redirectUri: RedirectUri,
            nonce: Guid.NewGuid().ToString("N"));

        var authorizeResponse = await FrontChannelClient.GetAsync(authorizeRequest);
        var payload = authorizeResponse.Headers.Location.ToString().ParseQueryString();
        authorizeResponse.StatusCode.Should().Be(HttpStatusCode.Found);
        payload.Code.Should().NotBeNullOrWhiteSpace();

        return (client, payload.Code, codeVerifier);
    }

    private async Task<(ClientsModel Client, TokenResponseResultModel Token)> GetAuthorizationCodeTokenAsync()
    {
        var (client, code, codeVerifier) = await GetAuthorizationCodeAsync();
        var tokenResponse = await SendTokenRequestWithBasicAuthAsync(
            client,
            new Dictionary<string, string>
            {
                [OpenIdConstants.TokenRequest.GrantType] = OpenIdConstants.GrantTypes.AuthorizationCode,
                [OpenIdConstants.TokenRequest.Code] = code,
                [OpenIdConstants.TokenRequest.RedirectUri] = RedirectUri,
                [OpenIdConstants.TokenRequest.CodeVerifier] = codeVerifier
            });

        var tokenPayload = await tokenResponse.ParseTokenResponseResult();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        tokenPayload.access_token.Should().NotBeNullOrWhiteSpace();
        tokenPayload.refresh_token.Should().NotBeNullOrWhiteSpace();

        return (client, tokenPayload);
    }

    private async Task<HttpResponseMessage> SendTokenRequestWithBasicAuthAsync(
        ClientsModel client,
        Dictionary<string, string> form)
    {
        using var request = BuildBasicFormRequest(TokenEndpoint, client, form);
        return await BackChannelClient.SendAsync(request);
    }

    private async Task<HttpResponseMessage> SendRevocationRequestAsync(ClientsModel client, string token)
    {
        using var request = BuildBasicFormRequest(
            RevocationEndpoint,
            client,
            new Dictionary<string, string>
            {
                [OpenIdConstants.RevocationRequest.Token] = token,
                [OpenIdConstants.RevocationRequest.TokenTypeHint] = OpenIdConstants.TokenType.RefreshToken
            });
        return await BackChannelClient.SendAsync(request);
    }

    private async Task<HttpResponseMessage> SendIntrospectionRequestAsync(ClientsModel client, string token,
        string tokenTypeHint)
    {
        using var request = BuildBasicFormRequest(
            IntrospectionEndpoint,
            client,
            new Dictionary<string, string>
            {
                [OpenIdConstants.IntrospectionRequest.Token] = token,
                [OpenIdConstants.IntrospectionRequest.TokenHintType] = tokenTypeHint
            });
        return await BackChannelClient.SendAsync(request);
    }

    private static HttpRequestMessage BuildBasicFormRequest(string endpoint, ClientsModel client,
        Dictionary<string, string> form)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new FormUrlEncodedContent(form)
        };

        var clientId = Uri.EscapeDataString(client.ClientId);
        var clientSecret = Uri.EscapeDataString(client.ClientSecret);
        var credentials = $"{clientId}:{clientSecret}";
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encoded);
        return request;
    }

    private static string GenerateCodeVerifier()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return ToBase64Url(bytes);
    }

    private static string ComputeS256CodeChallenge(string codeVerifier)
    {
        var hash = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
        return ToBase64Url(hash);
    }

    private static string ToBase64Url(byte[] value)
    {
        return Convert.ToBase64String(value)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
