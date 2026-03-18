/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Net;
using FluentAssertions;
using IntegrationTests.Endpoint.Setup;
using Xunit;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.ErrorCodes;

namespace IntegrationTests.Endpoint.FlowTests;

public class ROPFlowTests : HCLCSSFFakeSetup
{
    private const string Category = "ROP";
    private readonly string passWord = "Test@123456789";
    private readonly string positiveCaseClientName = "HCL.CS.SF Plain PKCE Client";
    private readonly string userName = "checktest";

    [Fact]
    [Trait("Category", Category)]
    public async Task ROP_ValidInput_ReturnSuccess()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(positiveCaseClientName);
        client.Should().NotBeNull();
        var tokenRequest = CreateTokenRequest(
            client.ClientId,
            client.ClientSecret,
            grantType: OpenIdConstants.GrantTypes.Password,
            userName: userName,
            password: passWord,
            scope: "openid email profile");
        var tokenClient = BackChannelClient;
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokenResult = await tokenResponse.ParseTokenResponseResult();
        tokenResult.access_token.Should().NotBeNull();
        tokenResult.id_token.Should().NotBeNull();
        tokenResult.refresh_token.Should().BeNull();
        tokenResult.expires_in.Should().BeGreaterThan(0);
        tokenResult.token_type.Should().Be(OpenIdConstants.TokenResponseType.BearerTokenType);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ROP_MissingClientID_ReturnInvalidClientError()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(positiveCaseClientName);
        client.Should().NotBeNull();
        var tokenRequest = CreateTokenRequest(
            clientSecret: client.ClientSecret,
            grantType: OpenIdConstants.GrantTypes.Password,
            userName: userName,
            password: passWord,
            scope: "openid email profile");
        var tokenClient = BackChannelClient;
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));

        tokenResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResult.ErrorDescription.Should().NotBeNull();
        tokenResult.IsError.Equals(true);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidClient);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ROP_InvalidClientID_Return_ReturnInvalidClientError()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(positiveCaseClientName);
        client.Should().NotBeNull();
        var tokenRequest = CreateTokenRequest(
            client.ClientId + "123",
            client.ClientSecret,
            grantType: OpenIdConstants.GrantTypes.Password,
            userName: userName,
            password: passWord,
            scope: "openid email profile");
        var tokenClient = BackChannelClient;
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResult.ErrorDescription.Should().NotBeNull();
        tokenResult.IsError.Equals(true);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidClient);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ROP_MissingClientSecret_ReturnInvalidClientError()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(positiveCaseClientName);
        client.Should().NotBeNull();
        var tokenRequest = CreateTokenRequest(
            client.ClientId,
            grantType: OpenIdConstants.GrantTypes.Password,
            userName: userName,
            password: passWord,
            scope: "openid email profile");
        var tokenClient = BackChannelClient;
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResult.ErrorDescription.Should().NotBeNull();
        tokenResult.IsError.Equals(true);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidClient);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ROP_InvalidClientSecret_ReturnInvalidClientError()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(positiveCaseClientName);
        client.Should().NotBeNull();
        var tokenRequest = CreateTokenRequest(
            client.ClientId,
            client.ClientSecret + "123",
            grantType: OpenIdConstants.GrantTypes.Password,
            userName: userName,
            password: passWord,
            scope: "openid email profile");
        var tokenClient = BackChannelClient;
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidClient);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.ClientSecretInvalid));
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ROP_MissinggrantType_ReturnGrantTypeIsMissingError()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(positiveCaseClientName);
        client.Should().NotBeNull();
        var tokenRequest = CreateTokenRequest(
            client.ClientId,
            client.ClientSecret,
            userName: userName,
            password: passWord,
            scope: "openid email profile");
        var tokenClient = BackChannelClient;
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));

        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResult.IsError.Equals(true);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.UnsupportedGrantType);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.GrantTypeIsMissing));
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ROP_InvalidgrantType_ReturnUnsupportedGrantTypeError()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(positiveCaseClientName);
        client.Should().NotBeNull();
        var tokenRequest = CreateTokenRequest(
            client.ClientId,
            client.ClientSecret,
            grantType: OpenIdConstants.GrantTypes.Password + "123",
            userName: userName,
            password: passWord,
            scope: "openid email profile");
        var tokenClient = BackChannelClient;
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));

        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResult.IsError.Equals(true);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.UnsupportedGrantType);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.GrantTypeIsMissing));
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ROP_MissingUserName_ReturnUserNameMissingError()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(positiveCaseClientName);
        client.Should().NotBeNull();
        var tokenRequest = CreateTokenRequest(
            client.ClientId,
            client.ClientSecret,
            grantType: OpenIdConstants.GrantTypes.Password,
            password: passWord,
            scope: "openid email profile");
        var tokenClient = BackChannelClient;
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResult.IsError.Equals(true);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidGrant);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.UserNameMissing));
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ROP_InvalidUserName_ReturnInvalidUserError()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(positiveCaseClientName);
        client.Should().NotBeNull();
        var tokenRequest = CreateTokenRequest(
            client.ClientId,
            client.ClientSecret,
            grantType: OpenIdConstants.GrantTypes.Password,
            userName: userName + "123",
            password: passWord,
            scope: "openid email profile");
        var tokenClient = BackChannelClient;
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResult.IsError.Equals(true);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidUser));
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ROP_MissingPassword_ReturnUserAuthenticationFailed()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(positiveCaseClientName);
        client.Should().NotBeNull();
        var tokenRequest = CreateTokenRequest(
            client.ClientId,
            client.ClientSecret,
            grantType: OpenIdConstants.GrantTypes.Password,
            userName: userName,
            scope: "openid email profile");
        var tokenClient = BackChannelClient;
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResult.IsError.Equals(true);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidGrant);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.UserAuthenticationFailed));
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ROP_InvalidPassword_ReturnUserAuthenticationFailed()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(positiveCaseClientName);
        client.Should().NotBeNull();
        var tokenRequest = CreateTokenRequest(
            client.ClientId,
            client.ClientSecret,
            grantType: OpenIdConstants.GrantTypes.Password,
            userName: userName,
            password: passWord + "123",
            scope: "openid email profile");
        var tokenClient = BackChannelClient;
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResult.IsError.Equals(true);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidGrant);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.UserAuthenticationFailed));
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ROP_MissingredirectUri_Return_Success()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(positiveCaseClientName);
        client.Should().NotBeNull();
        var tokenRequest = CreateTokenRequest(
            client.ClientId,
            client.ClientSecret,
            grantType: OpenIdConstants.GrantTypes.Password,
            userName: userName,
            password: passWord,
            scope: "openid email profile");
        var tokenClient = BackChannelClient;
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokenResult = await tokenResponse.ParseTokenResponseResult();
        tokenResult.access_token.Should().NotBeNull();
        tokenResult.id_token.Should().NotBeNull();
        tokenResult.refresh_token.Should().BeNull();
        tokenResult.expires_in.Should().BeGreaterThan(0);
        tokenResult.token_type.Should().Be(OpenIdConstants.TokenResponseType.BearerTokenType);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ROP_InvalidredirectUri_ReturnSuccess()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(positiveCaseClientName);
        client.Should().NotBeNull();
        var tokenRequest = CreateTokenRequest(
            client.ClientId,
            client.ClientSecret,
            grantType: OpenIdConstants.GrantTypes.Password,
            userName: userName,
            password: passWord,
            redirectUri: "https://localhost:44300/index.html" + "123",
            scope: "openid email profile");
        var tokenClient = BackChannelClient;
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokenResult = await tokenResponse.ParseTokenResponseResult();
        tokenResult.access_token.Should().NotBeNull();
        tokenResult.id_token.Should().NotBeNull();
        tokenResult.refresh_token.Should().BeNull();
        tokenResult.expires_in.Should().BeGreaterThan(0);
        tokenResult.token_type.Should().Be(OpenIdConstants.TokenResponseType.BearerTokenType);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ROP_IgnoreScope_ReturnSuccess()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(positiveCaseClientName);
        client.Should().NotBeNull();
        var tokenRequest = CreateTokenRequest(
            client.ClientId,
            client.ClientSecret,
            grantType: OpenIdConstants.GrantTypes.Password,
            userName: userName,
            password: passWord);
        var tokenClient = BackChannelClient;
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));

        tokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokenResult = await tokenResponse.ParseTokenResponseResult();
        tokenResult.access_token.Should().NotBeNull();
        tokenResult.id_token.Should().NotBeNull();
        tokenResult.refresh_token.Should().NotBeNullOrEmpty();
        tokenResult.expires_in.Should().BeGreaterThan(0);
        tokenResult.token_type.Should().Be(OpenIdConstants.TokenResponseType.BearerTokenType);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ROP_InvalidScope_ReturnInvalidScopeError()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(positiveCaseClientName);
        client.Should().NotBeNull();
        var tokenRequest = CreateTokenRequest(
            client.ClientId,
            client.ClientSecret,
            grantType: OpenIdConstants.GrantTypes.Password,
            userName: userName,
            password: passWord,
            scope: "openidemailprofile");
        var tokenClient = BackChannelClient;
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        tokenResult.IsError.Equals(true);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidScope);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidScopeOrNotAllowed));
    }
}
