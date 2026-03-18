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

namespace IntegrationTests.Endpoint.FlowTests;

public class ClientCredentialFlowTests : HCLCSSFFakeSetup
{
    private const string Category = "Client Credential";
    private readonly string positiveCaseClientName = "HCL.CS.SF Plain PKCE Client";
    private string userName = "BobUser";

    [Fact]
    [Trait("Category", Category)]
    public async Task ClientCredential_Valid_token_and_valid_scope_succeed()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(positiveCaseClientName);
        client.Should().NotBeNull();
        var tokenRequest = CreateTokenRequest(
            client.ClientId,
            client.ClientSecret,
            grantType: OpenIdConstants.GrantTypes.ClientCredentials,
            scope: "HCL.CS.SF.client");
        var tokenClient = BackChannelClient;
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenResponseResult();
        tokenResult.access_token.Should().NotBeNull();
        tokenResult.expires_in.ToString().Should().NotBeNull();
        tokenResult.refresh_token.Should().BeNull();
        tokenResult.id_token.Should().BeNull();
        tokenResult.scope.Should().NotBeNull();
        tokenResult.token_type.Should().Be(OpenIdConstants.TokenResponseType.BearerTokenType);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ClientCredential_MissingClientID_ReturnInvalidClient()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(positiveCaseClientName);
        client.Should().NotBeNull();
        var tokenRequest = CreateTokenRequest(
            clientSecret: client.ClientSecret,
            grantType: OpenIdConstants.GrantTypes.ClientCredentials,
            scope: "HCL.CS.SF.client");
        var tokenClient = BackChannelClient;
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidClient);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ClientCredential_InvalidClientID_ReturnInvalidClient()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(positiveCaseClientName);
        client.Should().NotBeNull();
        var tokenRequest = CreateTokenRequest(
            client.ClientId + "123",
            client.ClientSecret,
            grantType: OpenIdConstants.GrantTypes.ClientCredentials,
            scope: "HCL.CS.SF.client");
        var tokenClient = BackChannelClient;
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidClient);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ClientCredential_MissingClientSecret_ReturnInvalidClient()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(positiveCaseClientName);
        client.Should().NotBeNull();
        var tokenRequest = CreateTokenRequest(
            client.ClientId,
            grantType: OpenIdConstants.GrantTypes.ClientCredentials,
            scope: "HCL.CS.SF.client");
        var tokenClient = BackChannelClient;
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidClient);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ClientCredential_InvalidClientSecret_Return()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(positiveCaseClientName);
        client.Should().NotBeNull();
        var tokenRequest = CreateTokenRequest(
            client.ClientId,
            client.ClientSecret + "123",
            grantType: OpenIdConstants.GrantTypes.ClientCredentials,
            scope: "HCL.CS.SF.client");
        var tokenClient = BackChannelClient;
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidClient);
    }
}
