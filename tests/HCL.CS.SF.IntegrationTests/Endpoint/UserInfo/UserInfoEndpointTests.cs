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

namespace IntegrationTests.Endpoint.UserInfo;

public class UserInfoEndpointTests : HCLCSSFFakeSetup
{
    private const string Category = "UserInfoEndpointTests";
    private readonly string positiveCaseClientName = "HCL.CS.SF Plain PKCE Client";
    private readonly string redirectUri = "http://127.0.0.1:63562/";
    private ClientsModel clientModel;

    [Fact]
    [Trait("Category", Category)]
    public async Task UserInfoEndpoint_ValidInput_Success()
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
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce,
            state: "TestState");
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.OK);
        var response = await returnQuery.ParseAuthorizeResponse();
        FrontChannelClient.SetAccessTokenAuthorizationHeader(response.AccessToken);
        var userInfoResponse = await FrontChannelClient.GetAsync(UserInfoEndpoint);
        var userInfoResult = await userInfoResponse.ParseUserInfoResponse();
        userInfoResult.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task UserInfoEndpoint_InvalidAccessToken_ReturnTokenIsNullOrInvalidError()
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
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce,
            state: "TestState");

        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await returnQuery.ParseAuthorizeResponse();

        FrontChannelClient.SetAccessTokenAuthorizationHeader(response.AccessToken + "test001");
        var userInfoResponse = await FrontChannelClient.GetAsync(UserInfoEndpoint);
        userInfoResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorResponse = userInfoResponse.ParseUserInfoErrorResponse();
        errorResponse.IsError = true;
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.TokenRevoked));
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task UserInfoEndpoint_PassingRefreeshToken_ReturnInvalidTokenFormatError()
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
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce,
            state: "TestState");

        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.OK);
        var response = await returnQuery.ParseAuthorizeResponse();
        FrontChannelClient.SetAccessTokenAuthorizationHeader(response.RefreshToken);
        var userInfoResponse = await FrontChannelClient.GetAsync(UserInfoEndpoint);
        userInfoResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorResponse = userInfoResponse.ParseUserInfoErrorResponse();
        errorResponse.IsError = true;
        errorResponse.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidFormat.ToLower());
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidTokenFormat));
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task UserInfoEndpoint_PassingIdentityToken_ReturnInvalidTokenFormatError()
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
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce,
            state: "TestState");

        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = await returnQuery.ParseAuthorizeResponse();
        FrontChannelClient.SetAccessTokenAuthorizationHeader(response.IdentityToken);
        var userInfoResponse = await FrontChannelClient.GetAsync(UserInfoEndpoint);
        userInfoResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorResponse = userInfoResponse.ParseUserInfoErrorResponse();
        errorResponse.IsError = true;
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.TokenRevoked));
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task UserInfoEndpoint_MissingTokenAuthorizationHeader_ReturnInvalidUserClaimsError()
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
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce,
            state: "TestState");

        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var userInfoResponse = await FrontChannelClient.GetAsync(UserInfoEndpoint);
        userInfoResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorResponse = userInfoResponse.ParseUserInfoErrorResponse();
        errorResponse.IsError = true;
        errorResponse.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidToken.ToLower());
    }
}
