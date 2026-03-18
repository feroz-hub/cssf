/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Net;
using FluentAssertions;
using IntegrationTests.ApiDomainModel;
using IntegrationTests.Endpoint.Setup;
using Xunit;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;

namespace IntegrationTests.Endpoint.FlowTests;

public class RefreshTokenFlowTest : HCLCSSFFakeSetup
{
    private readonly string positiveCaseClientName = "HCL.CS.SF Plain PKCE Client";
    private readonly string redirectUri = "http://127.0.0.1:63562/";
    private ClientsModel clientModel;

    [Fact]
    public async Task TokenGeneration_Renew_Refresh_Token()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code id_token token",
            "openid email profile offline_access phone",
            responseMode: "fragment",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "S256",
            maxAge: "60",
            redirectUri: "http://127.0.0.1:63562/",
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseFragmentString();

        var tokenClient = BackChannelClient;
        var tokenRequest = CreateTokenRequest(
            clientModel.ClientId,
            clientModel.ClientSecret,
            refreshToken: response.RefreshToken,
            grantType: OpenIdConstants.GrantTypes.RefreshToken);

        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));

        // Parse the token response and convert to TokenResponseModel
        var tokenResult = await tokenResponse.ParseTokenResponseResult();
        tokenResult.access_token.Should().NotBeNull();
        tokenResult.id_token.Should().BeNullOrEmpty();
        tokenResult.refresh_token.Should().NotBeNull();
        tokenResult.expires_in.Should().BeGreaterThan(0);
        tokenResult.token_type.Should().Be(OpenIdConstants.TokenResponseType.BearerTokenType);
    }

    [Fact]
    public async Task Refreshtoken_ValidInput_Success()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code",
            "openid email profile phone offline_access",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();

        response.Code.Should().NotBeNull();
        var code = response.Code;

        var tokenClient = BackChannelClient;

        var tokenRequest = CreateTokenRequest(
            clientModel.ClientId,
            clientModel.ClientSecret,
            code,
            redirectUri,
            OpenIdConstants.GrantTypes.AuthorizationCode,
            codeVerifier);

        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenResponseResult();

        tokenResult.access_token.Should().NotBeNull();
        tokenResult.id_token.Should().NotBeNullOrEmpty();
        tokenResult.refresh_token.Should().NotBeNullOrEmpty();
        tokenResult.expires_in.Should().BeGreaterThan(0);
        tokenResult.token_type.Should().Be(OpenIdConstants.TokenResponseType.BearerTokenType);

        var tokenRequest2 = CreateIntroSpecRequest(
            token: tokenResult.refresh_token,
            tokenTypeHint: OpenIdConstants.TokenResponseType.RefreshToken,
            clientId: clientModel.ClientId,
            clientSecret: clientModel.ClientSecret);

        var introspecResponse1 =
            await BackChannelClient.PostAsync(IntrospectionEndpoint, new FormUrlEncodedContent(tokenRequest2));
        var introspectokenResponseresult1 = await introspecResponse1.ParseIntrospectionResponse();
        introspecResponse1.StatusCode.Should().Be(HttpStatusCode.OK);
        introspectokenResponseresult1.Active.Should().BeTrue();
        introspectokenResponseresult1.ExpiresAt.Should().NotBeNull();
        introspectokenResponseresult1.IssuedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RefreshtokenMissingClientID_ReturnInvalidClient()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code",
            "openid email profile phone offline_access",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();

        response.Code.Should().NotBeNull();
        var code = response.Code;

        var tokenClient = BackChannelClient;

        var tokenRequest = CreateTokenRequest(
            clientModel.ClientSecret,
            code,
            redirectUri,
            OpenIdConstants.GrantTypes.AuthorizationCode,
            codeVerifier);
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidClient);
    }

    [Fact]
    public async Task RefreshtokenInvalidClientID_RetrunInvalidClient()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code",
            "openid email profile phone offline_access",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();

        response.Code.Should().NotBeNull();
        var code = response.Code;

        var tokenClient = BackChannelClient;

        var tokenRequest = CreateTokenRequest(
            clientModel.ClientId + "123",
            clientModel.ClientSecret,
            code,
            redirectUri,
            OpenIdConstants.GrantTypes.AuthorizationCode,
            codeVerifier);
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidClient);
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshtokenMissingClientSecret_ReturnInvalidClient()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code",
            "openid email profile phone offline_access",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();

        response.Code.Should().NotBeNull();
        var code = response.Code;

        var tokenClient = BackChannelClient;

        var tokenRequest = CreateTokenRequest(
            clientModel.ClientId,
            code: code,
            redirectUri: redirectUri,
            grantType: OpenIdConstants.GrantTypes.AuthorizationCode,
            codeVerifier: codeVerifier);
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidClient);
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshtokenInvalidClientSecret_ReturnFail()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code",
            "openid email profile phone offline_access",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();

        response.Code.Should().NotBeNull();
        var code = response.Code;

        var tokenClient = BackChannelClient;

        var tokenRequest = CreateTokenRequest(
            clientModel.ClientId,
            clientModel.ClientSecret + "123",
            code,
            redirectUri,
            OpenIdConstants.GrantTypes.AuthorizationCode,
            codeVerifier);
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidClient);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.ClientSecretInvalid));
    }

    [Fact]
    public async Task RefreshtokenMissingCode_ReturenAuthorizationCodeMissing()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code",
            "openid email profile phone offline_access",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();

        response.Code.Should().NotBeNull();
        var tokenClient = BackChannelClient;

        var tokenRequest = CreateTokenRequest(
            clientModel.ClientId,
            clientModel.ClientSecret,
            redirectUri: redirectUri,
            grantType: OpenIdConstants.GrantTypes.AuthorizationCode,
            codeVerifier: codeVerifier);
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidGrant);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.AuthorizationCodeMissing));
    }

    [Fact]
    public async Task RefreshtokenInvalidCode_ReturnInvalidAuthorizationCode()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code",
            "openid email profile phone offline_access",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();
        response.Code.Should().NotBeNull();
        var code = response.Code;
        var tokenClient = BackChannelClient;
        var tokenRequest = CreateTokenRequest(
            clientModel.ClientId,
            clientModel.ClientSecret,
            code + "123",
            redirectUri,
            OpenIdConstants.GrantTypes.AuthorizationCode,
            codeVerifier);
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidGrant);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidAuthorizationCode));
    }

    [Fact]
    public async Task RefreshtokenMissingRedirectURI_ReturnRedirectUriMissing()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code",
            "openid email profile phone offline_access",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();

        response.Code.Should().NotBeNull();
        var code = response.Code;
        var tokenClient = BackChannelClient;

        var tokenRequest = CreateTokenRequest(
            clientModel.ClientId,
            clientModel.ClientSecret,
            code,
            grantType: OpenIdConstants.GrantTypes.AuthorizationCode,
            codeVerifier: codeVerifier);
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.UnauthorizedClient);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.RedirectUriMissing));
    }

    [Fact]
    public async Task RefreshtokenInvalidRedirectUri_InvalidRedirectUri()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code",
            "openid email profile phone offline_access",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();

        response.Code.Should().NotBeNull();
        var code = response.Code;

        var tokenClient = BackChannelClient;

        var tokenRequest = CreateTokenRequest(
            clientModel.ClientId,
            clientModel.ClientSecret,
            code,
            redirectUri + "123",
            OpenIdConstants.GrantTypes.AuthorizationCode,
            codeVerifier);
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidGrant);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidRedirectUri));
    }

    [Fact]
    public async Task RefreshtokenMissingGrantType_ReturnUnsupportedGrantTypeIsMissingError()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code",
            "openid email profile phone offline_access",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();

        response.Code.Should().NotBeNull();
        var code = response.Code;

        var tokenClient = BackChannelClient;

        var tokenRequest = CreateTokenRequest(
            clientModel.ClientId,
            clientModel.ClientSecret,
            code,
            redirectUri,
            codeVerifier);
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.UnsupportedGrantType);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.GrantTypeIsMissing));
    }

    [Fact]
    public async Task RefreshtokenInvalidGranttype_RetrunGrantTypeIsMissingError()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code",
            "openid email profile phone offline_access",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();

        response.Code.Should().NotBeNull();
        var code = response.Code;

        var tokenClient = BackChannelClient;

        var tokenRequest = CreateTokenRequest(
            clientModel.ClientId,
            clientModel.ClientSecret,
            code,
            redirectUri,
            OpenIdConstants.GrantTypes.AuthorizationCode + "invalid",
            codeVerifier);
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.UnsupportedGrantType);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.GrantTypeIsMissing));
    }

    [Fact]
    public async Task RefreshtokenMissingCodeChallenge_ReturnInvalidCodeVerifierError()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code",
            "openid email profile phone offline_access",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();

        response.Code.Should().NotBeNull();
        var code = response.Code;

        var tokenClient = BackChannelClient;

        var tokenRequest = CreateTokenRequest(
            clientModel.ClientId,
            clientModel.ClientSecret,
            code,
            redirectUri,
            OpenIdConstants.GrantTypes.AuthorizationCode);
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidGrant);
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidCodeVerifier));
    }

    [Fact]
    public async Task RefreshtokenInvalidCodeChallenge_returnUnsupportedCodeChallengeMethodError()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code",
            "openid email profile phone offline_access",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();

        response.Code.Should().NotBeNull();
        var code = response.Code;

        var tokenClient = BackChannelClient;

        var tokenRequest = CreateTokenRequest(
            clientModel.ClientId,
            clientModel.ClientSecret,
            code,
            redirectUri,
            OpenIdConstants.GrantTypes.AuthorizationCode,
            codeVerifier + "123");
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidGrant);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.UnsupportedCodeChallengeMethod));
    }
}
