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
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;

namespace IntegrationTests.Endpoint.FlowTests;

public class RevocationTestsFlow : HCLCSSFFakeSetup
{
    private readonly string positiveCaseClientName = "HCL.CS.SF Plain PKCE Client";
    private readonly string redirectUri = "http://127.0.0.1:63562/";
    private ClientsModel clientModel;

    [Fact]
    public async Task TokenRevocation_ValidInput_ReturnSuccess()
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
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseFragmentString();

        var tokenClient = BackChannelClient;

        var tokenRequest = CreateRevocationRequest(
            clientModel.ClientId,
            clientModel.ClientSecret,
            response.RefreshToken,
            OpenIdConstants.TokenType.RefreshToken);
        var revocationResponse =
            await tokenClient.PostAsync(RevocationEndpoint, new FormUrlEncodedContent(tokenRequest));
        revocationResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Token_Revocation_MissingToken_ReturnInvalidRequest()
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
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);

        var tokenClient = BackChannelClient;

        var tokenRequest = CreateRevocationRequest(
            clientModel.ClientId,
            clientModel.ClientSecret,
            tokentypehint: OpenIdConstants.TokenType.RefreshToken);
        var revocationResponse =
            await tokenClient.PostAsync(RevocationEndpoint, new FormUrlEncodedContent(tokenRequest));

        var revocationResult = await revocationResponse.ParseIntrospectionErrorResponse();
        revocationResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        revocationResult.IsError.Should().Be(true);
        revocationResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
    }

    [Fact]
    public async Task Token_Revocation_InvalidToken_ReturnInvalidRequest()
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
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseFragmentString();

        var tokenClient = BackChannelClient;

        var tokenRequest = CreateRevocationRequest(
            clientModel.ClientId,
            clientModel.ClientSecret,
            response.RefreshToken + "123",
            OpenIdConstants.TokenType.RefreshToken);
        var revocationResponse =
            await tokenClient.PostAsync(RevocationEndpoint, new FormUrlEncodedContent(tokenRequest));
        var revocationResult = await revocationResponse.ParseIntrospectionErrorResponse();
        revocationResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        revocationResult.IsError.Should().Be(true);
        revocationResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
    }


    [Fact]
    public async Task Token_Revocation_InvalidTokenHinttype_ReturnErrorUnsupportedTokenType()
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
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseFragmentString();

        var tokenClient = BackChannelClient;

        var tokenRequest = CreateRevocationRequest(
            clientModel.ClientId,
            clientModel.ClientSecret,
            response.RefreshToken,
            OpenIdConstants.TokenType.RefreshToken + "123");
        var revocationResponse =
            await tokenClient.PostAsync(RevocationEndpoint, new FormUrlEncodedContent(tokenRequest));
        var revocationResult = await revocationResponse.ParseIntrospectionErrorResponse();
        revocationResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        revocationResult.IsError.Should().Be(true);
        revocationResult.ErrorCode.Should().Be(OpenIdConstants.Errors.UnsupportedTokenType);
    }
}
