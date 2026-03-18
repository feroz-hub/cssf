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

public class IntrospecFlowTest : HCLCSSFFakeSetup
{
    private const string Category = "Introspection endpoint";
    private readonly string hCLCSEarlyTokenExpireClient = "HCL.CS.SF Early Token Expire Client";
    private readonly string redirectUri = "http://127.0.0.1:63562/";
    private ClientsModel clientModel;
    private string positiveCaseClientName = "HCL.CS.SF Plain PKCE Client";

    private async Task<TokenResponseResultModel> TokenGeneration_AuthCodeFlow()
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
        return tokenResult;
    }

    private async Task<TokenResponseResultModel> TokenGeneration_AuthCodeExpireFlow()
    {
        await LoginAsync(User);
        positiveCaseClientName = hCLCSEarlyTokenExpireClient;
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code",
            "openid email profile phone HCL.CS.SF.client offline_access",
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
        return tokenResult;
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Intorspec_HintTypeAccesstoken_ReturnSuccess()
    {
        var tokenResult = await TokenGeneration_AuthCodeExpireFlow();
        var introspectRequest = CreateIntroSpecRequest(
            token: tokenResult.access_token,
            tokenTypeHint: OpenIdConstants.TokenResponseType.AccessToken,
            clientId: clientModel.ClientId,
            clientSecret: clientModel.ClientSecret);
        var introspecResponse1 =
            await BackChannelClient.PostAsync(IntrospectionEndpoint, new FormUrlEncodedContent(introspectRequest));
        var introspectokenResponseresult1 = await introspecResponse1.ParseIntrospectionResponse();
        introspectokenResponseresult1.Active.Should().BeTrue();
        introspectokenResponseresult1.Audience.Should().NotBeNull();
        introspectokenResponseresult1.ClientId.Should().NotBeNull();
        introspectokenResponseresult1.ExpiresAt.Should().NotBeNull();
        introspectokenResponseresult1.IssuedAt.Should().NotBeNull();
        introspectokenResponseresult1.Scope.Should().NotBeNull();
        introspectokenResponseresult1.SubjectId.Should().NotBeNull();
        introspectokenResponseresult1.UserName.Should().NotBeNull();
        introspectokenResponseresult1.Issuer.Should().NotBeNull();
        var introspectRequest_1 = CreateIntroSpecRequest(
            token: tokenResult.access_token,
            clientId: clientModel.ClientId,
            clientSecret: clientModel.ClientSecret);
        var introspecResponse2 =
            await BackChannelClient.PostAsync(IntrospectionEndpoint, new FormUrlEncodedContent(introspectRequest_1));
        var introspectokenResponseresult2 = await introspecResponse2.ParseIntrospectionResponse();

        introspectokenResponseresult2.Active.Should().BeTrue();
        introspectokenResponseresult2.Audience.Should().NotBeNull();
        introspectokenResponseresult2.ClientId.Should().NotBeNull();
        introspectokenResponseresult2.ExpiresAt.Should().NotBeNull();
        introspectokenResponseresult2.IssuedAt.Should().NotBeNull();
        introspectokenResponseresult2.Scope.Should().NotBeNull();
        introspectokenResponseresult2.SubjectId.Should().NotBeNull();
        introspectokenResponseresult2.UserName.Should().NotBeNull();
        introspectokenResponseresult2.Issuer.Should().NotBeNull();
        var introspectRequest_2 = CreateIntroSpecRequest(
            token: tokenResult.access_token,
            tokenTypeHint: OpenIdConstants.TokenResponseType.RefreshToken,
            clientId: clientModel.ClientId,
            clientSecret: clientModel.ClientSecret);

        var introspecResponse3 =
            await BackChannelClient.PostAsync(IntrospectionEndpoint, new FormUrlEncodedContent(introspectRequest_2));
        var introspectokenResponseresult3 = await introspecResponse3.ParseIntrospectionResponse();
        introspectokenResponseresult3.Active.Should().BeFalse();
        Thread.Sleep(10000);
        var dict5 = CreateIntroSpecRequest(
            token: tokenResult.access_token,
            tokenTypeHint: OpenIdConstants.TokenResponseType.AccessToken,
            clientId: clientModel.ClientId,
            clientSecret: clientModel.ClientSecret);
        var introspecResponse4 =
            await BackChannelClient.PostAsync(IntrospectionEndpoint, new FormUrlEncodedContent(dict5));
        var introspectokenResponseresult4 = await introspecResponse4.ParseIntrospectionResponse();
        introspectokenResponseresult4.Active.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Intorspec_HintTypeRefreshtoken_Success()
    {
        var tokenResult = await TokenGeneration_AuthCodeExpireFlow();
        var introspectRequest = CreateIntroSpecRequest(
            token: tokenResult.refresh_token,
            tokenTypeHint: OpenIdConstants.TokenResponseType.RefreshToken,
            clientId: clientModel.ClientId,
            clientSecret: clientModel.ClientSecret);

        var introspecResponse1 =
            await BackChannelClient.PostAsync(IntrospectionEndpoint, new FormUrlEncodedContent(introspectRequest));
        var introspectokenResponseresult1 = await introspecResponse1.ParseIntrospectionResponse();

        introspectokenResponseresult1.Active.Equals("Active");
        introspectokenResponseresult1.ExpiresAt.Should().NotBeNull();
        introspectokenResponseresult1.IssuedAt.Should().NotBeNull();
        var introspectRequest_1 = CreateIntroSpecRequest(
            token: tokenResult.refresh_token,
            clientId: clientModel.ClientId,
            clientSecret: clientModel.ClientSecret);
        var introspecResponse2 =
            await BackChannelClient.PostAsync(IntrospectionEndpoint, new FormUrlEncodedContent(introspectRequest_1));
        var introspectokenResponseresult2 = await introspecResponse2.ParseIntrospectionResponse();
        introspectokenResponseresult2.Active.Should().BeTrue();
        var introspectRequest_2 = CreateIntroSpecRequest(
            token: tokenResult.refresh_token,
            tokenTypeHint: OpenIdConstants.TokenResponseType.AccessToken,
            clientId: clientModel.ClientId,
            clientSecret: clientModel.ClientSecret);

        var introspecResponse3 =
            await BackChannelClient.PostAsync(IntrospectionEndpoint, new FormUrlEncodedContent(introspectRequest_2));
        var introspectokenResponseresult3 = await introspecResponse3.ParseIntrospectionResponse();
        introspectokenResponseresult3.Active.Should().BeFalse();
        Thread.Sleep(10000);
        var introspectRequest4 = CreateIntroSpecRequest(
            token: tokenResult.refresh_token,
            tokenTypeHint: OpenIdConstants.TokenResponseType.RefreshToken,
            clientId: clientModel.ClientId,
            clientSecret: clientModel.ClientSecret);
        var introspecResponse4 =
            await BackChannelClient.PostAsync(IntrospectionEndpoint, new FormUrlEncodedContent(introspectRequest4));
        var introspectokenResponseresult4 = await introspecResponse4.ParseIntrospectionResponse();
        introspectokenResponseresult4.Active.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Introspec_PassingAccesstoken_Success()
    {
        var tokenResult = await TokenGeneration_AuthCodeFlow();
        var introspectRequest = CreateIntroSpecRequest(
            token: tokenResult.access_token,
            clientId: clientModel.ClientId,
            clientSecret: clientModel.ClientSecret);

        var introspecResponse1 =
            await BackChannelClient.PostAsync(IntrospectionEndpoint, new FormUrlEncodedContent(introspectRequest));
        var introspectokenResponseresult1 = await introspecResponse1.ParseIntrospectionResponse();
        introspectokenResponseresult1.Active.Should().Be(true);
        introspectokenResponseresult1.Audience.Should().NotBeNull();
        introspectokenResponseresult1.ClientId.Should().NotBeNull();
        introspectokenResponseresult1.ExpiresAt.Should().NotBeNull();
        introspectokenResponseresult1.IssuedAt.Should().NotBeNull();
        introspectokenResponseresult1.Scope.Should().NotBeNull();
        introspectokenResponseresult1.SubjectId.Should().NotBeNull();
        introspectokenResponseresult1.UserName.Should().NotBeNull();
        introspectokenResponseresult1.Issuer.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Introspec_PassingRefreshToken_Success()
    {
        var tokenResult = await TokenGeneration_AuthCodeFlow();
        var introspectRequest = CreateIntroSpecRequest(
            token: tokenResult.refresh_token,
            tokenTypeHint: OpenIdConstants.TokenResponseType.RefreshToken,
            clientId: clientModel.ClientId,
            clientSecret: clientModel.ClientSecret);

        var introspecResponse1 =
            await BackChannelClient.PostAsync(IntrospectionEndpoint, new FormUrlEncodedContent(introspectRequest));
        var introspectokenResponseresult1 = await introspecResponse1.ParseIntrospectionResponse();
        introspectokenResponseresult1.Active.Should().Be(true);
        introspectokenResponseresult1.ExpiresAt.Should().NotBeNull();
        introspectokenResponseresult1.IssuedAt.Should().NotBeNull();
    }

    // Negative Secanario

    [Fact]
    [Trait("Category", Category)]
    public async Task Introspec_Missingaccesstoken_RetrunTokenMissingError()
    {
        var tokenResult = await TokenGeneration_AuthCodeFlow();

        var introspectRequest = CreateIntroSpecRequest(
            tokenTypeHint: OpenIdConstants.TokenResponseType.AccessToken,
            clientId: clientModel.ClientId,
            clientSecret: clientModel.ClientSecret);

        var introspecResponse1 =
            await BackChannelClient.PostAsync(IntrospectionEndpoint, new FormUrlEncodedContent(introspectRequest));
        var introspectokenResponseresult1 = await introspecResponse1.ParseIntrospectionErrorResponse();
        introspecResponse1.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        introspectokenResponseresult1.IsError.Should().Be(true);
        introspectokenResponseresult1.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidToken);
        introspectokenResponseresult1.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.TokenMissing));
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Introspec_accesstoken_InvalidToken_InvalidTokenInIntrospection()
    {
        var tokenResult = await TokenGeneration_AuthCodeFlow();
        var introspectRequest = CreateIntroSpecRequest(
            token: tokenResult.access_token + "123",
            tokenTypeHint: OpenIdConstants.TokenResponseType.AccessToken,
            clientId: clientModel.ClientId,
            clientSecret: clientModel.ClientSecret);

        var introspecResponse =
            await BackChannelClient.PostAsync(IntrospectionEndpoint, new FormUrlEncodedContent(introspectRequest));
        var introspectokenResponseresult1 = await introspecResponse.ParseIntrospectionErrorResponse();
        introspecResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        introspectokenResponseresult1.IsError.Should().Be(true);
        introspectokenResponseresult1.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        introspectokenResponseresult1.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidTokenInIntrospection));
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Introspec_accesstoken_InvalidHintype_ReturnInvalidTokenHintType()
    {
        var tokenResult = await TokenGeneration_AuthCodeFlow();
        var introspectRequest = CreateIntroSpecRequest(
            token: tokenResult.access_token,
            tokenTypeHint: OpenIdConstants.TokenResponseType.AccessToken + "123",
            clientId: clientModel.ClientId,
            clientSecret: clientModel.ClientSecret);

        var introspecResponse1 =
            await BackChannelClient.PostAsync(IntrospectionEndpoint, new FormUrlEncodedContent(introspectRequest));
        introspecResponse1.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var introspectokenResponseresult1 = await introspecResponse1.ParseIntrospectionErrorResponse();
        introspectokenResponseresult1.IsError.Should().Be(true);
        introspectokenResponseresult1.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        introspectokenResponseresult1.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidTokenHintType));
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Introspec_accesstoken_ReturnClientDoesNotExist()
    {
        var tokenResult = await TokenGeneration_AuthCodeFlow();
        var introspectRequest = CreateIntroSpecRequest(
            token: tokenResult.access_token,
            tokenTypeHint: OpenIdConstants.TokenResponseType.AccessToken,
            clientSecret: clientModel.ClientSecret);
        var introspecResponse1 =
            await BackChannelClient.PostAsync(IntrospectionEndpoint, new FormUrlEncodedContent(introspectRequest));
        introspecResponse1.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var introspectokenResponseresult1 = await introspecResponse1.ParseIntrospectionErrorResponse();
        introspectokenResponseresult1.IsError.Should().Be(true);
        introspectokenResponseresult1.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidClient);
        introspectokenResponseresult1.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.ClientDoesNotExist));
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Introspec_accesstoken_InvalidClientID_returnErrorInvalidClientID()
    {
        var tokenResult = await TokenGeneration_AuthCodeFlow();
        var introspectRequest = CreateIntroSpecRequest(
            token: tokenResult.access_token,
            tokenTypeHint: OpenIdConstants.TokenResponseType.AccessToken,
            clientId: clientModel.ClientId + "123",
            clientSecret: clientModel.ClientSecret);

        var introspecResponse1 =
            await BackChannelClient.PostAsync(IntrospectionEndpoint, new FormUrlEncodedContent(introspectRequest));
        introspecResponse1.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var introspectokenResponseresult1 = await introspecResponse1.ParseIntrospectionErrorResponse();
        introspectokenResponseresult1.IsError.Should().Be(true);
        introspectokenResponseresult1.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidClient);
        introspectokenResponseresult1.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.ClientDoesNotExist));
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Introspec_accesstoken_MissingClientSecret_ReturnInvalidCleint()
    {
        var tokenResult = await TokenGeneration_AuthCodeFlow();
        var introspectRequest = CreateIntroSpecRequest(
            token: tokenResult.access_token,
            tokenTypeHint: OpenIdConstants.TokenResponseType.AccessToken,
            clientId: clientModel.ClientId);

        var introspecResponse1 =
            await BackChannelClient.PostAsync(IntrospectionEndpoint, new FormUrlEncodedContent(introspectRequest));
        var introspectokenResponseresult1 = await introspecResponse1.ParseIntrospectionErrorResponse();
        introspecResponse1.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        introspectokenResponseresult1.IsError.Should().Be(true);
        introspectokenResponseresult1.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidClient);
        introspectokenResponseresult1.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.ClientDoesNotExist));
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Introspec_accesstoken_InvalidClientSecret_ReturnFail()
    {
        var tokenResult = await TokenGeneration_AuthCodeFlow();
        var introspectRequest = CreateIntroSpecRequest(
            token: tokenResult.access_token,
            tokenTypeHint: OpenIdConstants.TokenResponseType.AccessToken,
            clientId: clientModel.ClientId,
            clientSecret: clientModel.ClientSecret + "123");
        var introspecResponse1 =
            await BackChannelClient.PostAsync(IntrospectionEndpoint, new FormUrlEncodedContent(introspectRequest));
        var introspectokenResponseresult1 = await introspecResponse1.ParseIntrospectionErrorResponse();
        introspectokenResponseresult1.IsError.Should().BeTrue();
        introspectokenResponseresult1.ErrorCode.Should().NotBeNull();
        introspectokenResponseresult1.ErrorDescription.Should().NotBeNull();
        introspectokenResponseresult1.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidClient);
        introspectokenResponseresult1.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.ClientDoesNotExist));
    }
}
