/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using IntegrationTests.ApiDomainModel;
using IntegrationTests.Endpoint.Setup;
using Microsoft.IdentityModel.Tokens;
using Xunit;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;

namespace IntegrationTests.Endpoint.FlowTests;

public class HybridFlowTests : HCLCSSFFakeSetup
{
    private readonly string positiveCaseClientName = "HCL.CS.SF Plain PKCE Client";
    private readonly string redirectUri = "http://127.0.0.1:63562/";
    private ClientsModel clientModel;

    [Fact]
    public async Task TokenGeneration_HybridFlow_Only_IdToken_ReturnSuccess()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code id_token",
            "openid email profile phone",
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.OK);
        var response = await returnQuery.ParseAuthorizeResponse();
        response.AccessToken.Should().BeNull();
        response.AccessTokenLifetime.Should().Be(0);
        response.IdentityToken.Should().NotBeNullOrEmpty();
        response.Code.Should().NotBeNullOrEmpty();
        response.Scope.Should().NotBeNullOrEmpty();
        response.SessionState.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task TokenGeneration_HybridFlow_Only_IdToken_GetAccesstokenByAuthCode()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code id_token",
            "openid email profile phone",
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.OK);
        var response = await returnQuery.ParseAuthorizeResponse();
        response.AccessToken.Should().BeNull();
        response.AccessTokenLifetime.Should().Be(0);
        response.IdentityToken.Should().NotBeNullOrEmpty();
        response.Code.Should().NotBeNullOrEmpty();
        response.Scope.Should().NotBeNullOrEmpty();
        response.SessionState.Should().BeNullOrEmpty();

        var idtoken0 = new JwtSecurityToken(response.IdentityToken);
        var test0 = idtoken0.Claims;
        test0.Should().NotBeEmpty();
        var tokenClient = BackChannelClient;

        var tokenRequest = CreateTokenRequest(
            clientModel.ClientId,
            clientModel.ClientSecret,
            response.Code,
            redirectUri,
            OpenIdConstants.GrantTypes.AuthorizationCode,
            codeVerifier);
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenResponseResult();
        tokenResult.access_token.Should().NotBeNull();
        tokenResult.id_token.Should().NotBeNull();
        tokenResult.refresh_token.Should().BeNull();
        tokenResult.expires_in.Should().BeGreaterThan(0);
        tokenResult.token_type.Should().Be(OpenIdConstants.TokenResponseType.BearerTokenType);
        var token = new JwtSecurityToken(tokenResult.access_token);
        var test1 = token.Claims;
        test1.Should().NotBeEmpty();
        var idtoken = new JwtSecurityToken(tokenResult.id_token);
        var test2 = idtoken.Claims;
        test2.Should().NotBeEmpty();
    }

    [Fact]
    public async Task TokenGeneration_HybridFlow_Only_AccessToken()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        var clientId = clientModel.ClientId;
        var clientSecret = clientModel.ClientSecret;
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientId,
            "code token",
            "openid email profile phone",
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = await returnQuery.ParseAuthorizeResponse();
        returnQuery.StatusCode.Should().Be(HttpStatusCode.OK);
        response.AccessToken.Should().NotBeNullOrEmpty();
        response.AccessTokenLifetime.ToString().Should().NotBeNullOrEmpty();
        response.IdentityToken.Should().BeNullOrEmpty();
        response.Scope.Should().NotBeNullOrEmpty();
        response.SessionState.Should().BeNullOrEmpty();
        response.Code.Should().NotBeNullOrEmpty();
        response.RedirectUri.Should().BeNullOrEmpty();
        response.RefreshToken.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task TokenGeneration_HybridFlow_All_Token_form_post()
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
        // Frontchannel GET authorize request -> authorize endpoint
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        // Parse the authorize request response and convert to AuthorizationResponseModel
        var response = await returnQuery.ParseAuthorizeResponse();
        returnQuery.StatusCode.Should().Be(HttpStatusCode.OK);
        response.AccessToken.Should().NotBeNullOrEmpty();
        response.AccessTokenLifetime.ToString().Should().NotBeNullOrEmpty();
        response.RefreshToken.Should().NotBeNullOrEmpty();
        response.IdentityToken.Should().NotBeNullOrEmpty();
        response.Scope.Should().NotBeNullOrEmpty();
        response.SessionState.Should().BeNullOrEmpty();
        response.State.Should().NotBeNullOrEmpty();
        response.RedirectUri.Should().BeNullOrEmpty();
        var idtoken0 = new JwtSecurityToken(response.IdentityToken);
        var test0 = idtoken0.Claims;
        test0.Should().NotBeEmpty();
    }

    [Fact]
    public async Task TokenGeneration_HybridFlow_All_Token_Jwt_fragment()
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
            nonce: nonce,
            state: "TestState");
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseFragmentString();
        returnQuery.StatusCode.Should().Be(HttpStatusCode.Found);
        response.AccessToken.Should().NotBeNullOrEmpty();
        response.AccessTokenLifetime.ToString().Should().NotBeNullOrEmpty();
        response.RefreshToken.Should().NotBeNullOrEmpty();
        response.IdentityToken.Should().NotBeNullOrEmpty();
        response.Scope.Should().NotBeNullOrEmpty();
        response.SessionState.Should().BeNullOrEmpty();
        response.State.Should().BeNullOrEmpty();
        response.IsError.Should().BeFalse();
        response.Code.Should().BeNullOrEmpty();
        response.Request.Should().BeNull();
        response.RedirectUri.Should().BeNullOrEmpty();
    }

    // HyBrid Flow Negative

    [Fact]
    public async Task HybridFlow_Only_IdToken_MissingClientID_ReturnClientIdMissingInRequest()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            responseType: "code id_token",
            scope: "openid email profile phone",
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var errorResponse = await returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorResponse.IsError.Should().Be(true);
        errorResponse.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.ClientIdMissingInRequest));
        errorResponse.ClientId.Should().BeNull();
        errorResponse.TraceId.Should().NotBeNullOrEmpty();
        errorResponse.RedirectUri.Should().BeNull();
        errorResponse.ResponseMode.Should().BeNull();
    }

    [Fact]
    public async Task HybridFlow_Only_IdToken_InvalidClientID_ReturnUnauthorizedClient()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId + "123",
            "code id_token",
            "openid email profile phone",
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var errorResponse = await returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        errorResponse.IsError.Should().Be(true);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorResponse.ErrorCode.Should().Be(OpenIdConstants.Errors.UnauthorizedClient);
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.ClientDoesNotExist));
        errorResponse.ClientId.Should().NotBeNullOrEmpty();
        errorResponse.TraceId.Should().NotBeNullOrEmpty();
        errorResponse.RedirectUri.Should().BeNull();
        errorResponse.ResponseMode.Should().BeNull();
    }

    [Fact]
    public async Task HybridFlow_Only_IdToken_InvalidScope_ReturnResponseTypeRequiresOpenIdScope()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code id_token",
            "openidemailprofilephone",
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var errorResponse = await returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        errorResponse.IsError.Should().Be(true);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorResponse.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.ResponseTypeRequiresOpenIdScope));
        errorResponse.ClientId.Should().NotBeNullOrEmpty();
        errorResponse.TraceId.Should().NotBeNullOrEmpty();
        errorResponse.RedirectUri.Should().NotBeNullOrEmpty();
        errorResponse.ResponseMode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HybridFlow_Only_IdToken_InvalidPrompt_ReturnFail()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code id_token",
            "openid email profile phone",
            responseMode: "form_post",
            prompt: "Invalid",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorResponse = await returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        errorResponse.IsError.Should().Be(true);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorResponse.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidPrompt));
        errorResponse.ClientId.Should().NotBeNullOrEmpty();
        errorResponse.TraceId.Should().NotBeNullOrEmpty();
        errorResponse.RedirectUri.Should().NotBeNullOrEmpty();
        errorResponse.ResponseMode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HybridFlow_Only_IdToken_MissingcodeVerifier_ReturnInvalidCodeChallenge()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code id_token",
            "openid email profile phone",
            responseMode: "form_post",
            prompt: "Invalid",
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var errorResponse = await returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        errorResponse.IsError.Should().Be(true);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorResponse.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidCodeChallenge));
        errorResponse.ClientId.Should().NotBeNullOrEmpty();
        errorResponse.TraceId.Should().NotBeNullOrEmpty();
        errorResponse.RedirectUri.Should().NotBeNullOrEmpty();
        errorResponse.ResponseMode.Should().NotBeNullOrEmpty();
    }


    [Fact]
    public async Task HybridFlow_Only_IdToken_InvalidCodeChallengeMethod_ReturnInvalidCodeChallenge()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code id_token",
            "openid email profile phone",
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "invalid",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var errorResponse = await returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        errorResponse.IsError.Should().Be(true);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorResponse.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidCodeChallenge));
        errorResponse.ClientId.Should().NotBeNullOrEmpty();
        errorResponse.TraceId.Should().NotBeNullOrEmpty();
        errorResponse.RedirectUri.Should().NotBeNullOrEmpty();
        errorResponse.ResponseMode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HybridFlow_Only_IdToken_MissingredirectUri_ReturnInvalidRedirectUri()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code id_token",
            "openid email profile phone",
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            //redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var errorResponse = await returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        errorResponse.IsError.Should().Be(true);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorResponse.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidRedirectUri));
        errorResponse.ClientId.Should().NotBeNullOrEmpty();
        errorResponse.TraceId.Should().NotBeNullOrEmpty();
        errorResponse.RedirectUri.Should().BeNullOrEmpty();
        errorResponse.ResponseMode.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task HybridFlow_Only_IdToken_InvalidredirectUri_ReturnInvalidRedirectUri()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code id_token",
            "openid email profile phone",
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri + "123",
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var errorResponse = await returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        errorResponse.IsError.Should().Be(true);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorResponse.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.RedirectUriNotRegistered));
        errorResponse.ClientId.Should().NotBeNullOrEmpty();
        errorResponse.TraceId.Should().NotBeNullOrEmpty();
        errorResponse.RedirectUri.Should().NotBeNullOrEmpty();
        errorResponse.ResponseMode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HybridFlow_Only_IdToken_MissingNonce_ReturnInvalidNonce()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code id_token",
            "openid email profile phone",
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri
            //nonce: nonce
        );
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var errorResponse = await returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        errorResponse.IsError.Should().Be(true);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorResponse.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidNonce));
        errorResponse.ClientId.Should().NotBeNullOrEmpty();
        errorResponse.TraceId.Should().NotBeNullOrEmpty();
        errorResponse.RedirectUri.Should().NotBeNullOrEmpty();
        errorResponse.ResponseMode.Should().NotBeNullOrEmpty();
    }


    // Only Token

    [Fact]
    public async Task HybridFlow_Only_Token_MissingClientID_ReturnClientIdMissingInRequest()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            responseType: "code token",
            scope: "openid email profile phone",
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var errorResponse = await returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorResponse.IsError.Should().Be(true);
        errorResponse.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.ClientIdMissingInRequest));
        errorResponse.ClientId.Should().BeNull();
        errorResponse.TraceId.Should().NotBeNullOrEmpty();
        errorResponse.RedirectUri.Should().BeNull();
        errorResponse.ResponseMode.Should().BeNull();
    }

    [Fact]
    public async Task HybridFlow_Only_Token_InvalidClientID_ReturnUnauthorizedClient()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId + "123",
            "code token",
            "openid email profile phone",
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var errorResponse = await returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        errorResponse.IsError.Should().Be(true);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorResponse.ErrorCode.Should().Be(OpenIdConstants.Errors.UnauthorizedClient);
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.ClientDoesNotExist));
        errorResponse.ClientId.Should().NotBeNullOrEmpty();
        errorResponse.TraceId.Should().NotBeNullOrEmpty();
        errorResponse.RedirectUri.Should().BeNull();
        errorResponse.ResponseMode.Should().BeNull();
    }

    [Fact]
    public async Task HybridFlow_Only_Token_InvalidScope_ReturnResponseTypeRequiresOpenIdScope()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code token",
            "openidemailprofilephone",
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var errorResponse = await returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        errorResponse.IsError.Should().Be(true);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorResponse.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.ResponseTypeRequiresOpenIdScope));
        errorResponse.ClientId.Should().NotBeNullOrEmpty();
        errorResponse.TraceId.Should().NotBeNullOrEmpty();
        errorResponse.RedirectUri.Should().NotBeNullOrEmpty();
        errorResponse.ResponseMode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HybridFlow_Only_Token_InvalidPrompt_ReturnFail()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code token",
            "openid email profile phone",
            responseMode: "form_post",
            prompt: "Invalid",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var errorResponse = await returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        errorResponse.IsError.Should().Be(true);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorResponse.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidPrompt));
        errorResponse.ClientId.Should().NotBeNullOrEmpty();
        errorResponse.TraceId.Should().NotBeNullOrEmpty();
        errorResponse.RedirectUri.Should().NotBeNullOrEmpty();
        errorResponse.ResponseMode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HybridFlow_Only_Token_MissingcodeVerifier_ReturnInvalidCodeChallenge()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code token",
            "openid email profile phone",
            responseMode: "form_post",
            prompt: "Invalid",
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var errorResponse = await returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        errorResponse.IsError.Should().Be(true);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorResponse.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidCodeChallenge));
        errorResponse.ClientId.Should().NotBeNullOrEmpty();
        errorResponse.TraceId.Should().NotBeNullOrEmpty();
        errorResponse.RedirectUri.Should().NotBeNullOrEmpty();
        errorResponse.ResponseMode.Should().NotBeNullOrEmpty();
    }


    [Fact]
    public async Task HybridFlow_Only_Token_InvalidCodeChallengeMethod_ReturnSuccess()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code token",
            "openid email profile phone",
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "invalid",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var errorResponse = await returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        errorResponse.IsError.Should().Be(true);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorResponse.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidCodeChallenge));
        errorResponse.ClientId.Should().NotBeNullOrEmpty();
        errorResponse.TraceId.Should().NotBeNullOrEmpty();
        errorResponse.RedirectUri.Should().NotBeNullOrEmpty();
        errorResponse.ResponseMode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HybridFlow_Only_Token_MissingredirectUri_ReturnInvalidRedirectUri()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code token",
            "openid email profile phone",
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            //redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var errorResponse = await returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        errorResponse.IsError.Should().Be(true);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorResponse.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidRedirectUri));
        errorResponse.ClientId.Should().NotBeNullOrEmpty();
        errorResponse.TraceId.Should().NotBeNullOrEmpty();
        errorResponse.RedirectUri.Should().BeNullOrEmpty();
        errorResponse.ResponseMode.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task HybridFlow_Only_Token_InvalidredirectUri_ReturnInvalidRedirectUri()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code token",
            "openid email profile phone",
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri + "123",
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var errorResponse = await returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        errorResponse.IsError.Should().Be(true);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorResponse.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.RedirectUriNotRegistered));
        errorResponse.ClientId.Should().NotBeNullOrEmpty();
        errorResponse.TraceId.Should().NotBeNullOrEmpty();
        errorResponse.RedirectUri.Should().NotBeNullOrEmpty();
        errorResponse.ResponseMode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HybridFlow_Only_Token_MissingNonce_ReturnInvalidNonce()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code token",
            "openid email profile phone",
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri
            //nonce: nonce
        );
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var errorResponse = await returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        errorResponse.IsError.Should().Be(true);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorResponse.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidNonce));
        errorResponse.ClientId.Should().NotBeNullOrEmpty();
        errorResponse.TraceId.Should().NotBeNullOrEmpty();
        errorResponse.RedirectUri.Should().NotBeNullOrEmpty();
        errorResponse.ResponseMode.Should().NotBeNullOrEmpty();
    }


    [Fact]
    public async Task HybridFlow_AllToken_MissingClientID_ReturnClientIdMissingInRequest()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            responseType: "code id_token token",
            scope: "openid email profile phone offline_access",
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var errorResponse = await returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorResponse.IsError.Should().Be(true);
        errorResponse.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.ClientIdMissingInRequest));
        errorResponse.ClientId.Should().BeNull();
        errorResponse.TraceId.Should().NotBeNullOrEmpty();
        errorResponse.RedirectUri.Should().BeNull();
        errorResponse.ResponseMode.Should().BeNull();
    }

    [Fact]
    public async Task HybridFlow_AllToken_InvalidClientID_ReturnUnauthorizedClient()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId + "123",
            "code id_token token",
            "openid email profile phone offline_access",
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var errorResponse = await returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        errorResponse.IsError.Should().Be(true);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorResponse.ErrorCode.Should().Be(OpenIdConstants.Errors.UnauthorizedClient);
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.ClientDoesNotExist));
        errorResponse.ClientId.Should().NotBeNullOrEmpty();
        errorResponse.TraceId.Should().NotBeNullOrEmpty();
        errorResponse.RedirectUri.Should().BeNull();
        errorResponse.ResponseMode.Should().BeNull();
    }

    [Fact]
    public async Task HybridFlow_AllToken_IgnoreScope_ReturnSuccess()
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
            //scope: "openidemailprofilephone",
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.OK);
        var response = returnQuery.ParseAuthorizeResponse();
        response.Result.AccessToken.Should().NotBeNullOrEmpty();
        response.Result.AccessTokenLifetime.Should().Be(0);
        response.Result.IdentityToken.Should().NotBeNullOrEmpty();
        response.Result.Code.Should().NotBeNullOrEmpty();
        response.Result.Scope.Should().NotBeNullOrEmpty();
        response.Result.SessionState.Should().BeNullOrEmpty();
        response.Result.RedirectUri.Should().BeNullOrEmpty();
        response.Result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HybridFlow_AllToken_InvalidScope_ReturnResponseTypeRequiresOpenIdScope()
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
            "openidemailprofilephone",
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var errorResponse = await returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        errorResponse.IsError.Should().Be(true);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorResponse.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.ResponseTypeRequiresOpenIdScope));
        errorResponse.ClientId.Should().NotBeNullOrEmpty();
        errorResponse.TraceId.Should().NotBeNullOrEmpty();
        errorResponse.RedirectUri.Should().NotBeNullOrEmpty();
        errorResponse.ResponseMode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HybridFlow_AllToken_InvalidPrompt_ReturnSuccess()
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
            "openid email profile phone offline_access",
            responseMode: "form_post",
            prompt: "Invalid",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorResponse = await returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        errorResponse.IsError.Should().Be(true);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorResponse.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidPrompt));
        errorResponse.ClientId.Should().NotBeNullOrEmpty();
        errorResponse.TraceId.Should().NotBeNullOrEmpty();
        errorResponse.RedirectUri.Should().NotBeNullOrEmpty();
        errorResponse.ResponseMode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HybridFlow_AllToken_MissingcodeVerifier_ReturnInvalidCodeChallenge()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code id_token token",
            "openid email profile phone offline_access",
            responseMode: "form_post",
            prompt: "none",
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var errorResponse = await returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        errorResponse.IsError.Should().Be(true);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorResponse.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidCodeChallenge));
        errorResponse.ClientId.Should().NotBeNullOrEmpty();
        errorResponse.TraceId.Should().NotBeNullOrEmpty();
        errorResponse.RedirectUri.Should().NotBeNullOrEmpty();
        errorResponse.ResponseMode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HybridFlow_AllToken_InvalidcodeVerifier_Returnfail()
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
            "openid email profile phone offline_access",
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier + "123",
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.OK);
        var response = await returnQuery.ParseAuthorizeResponse();
        response.AccessToken.Should().NotBeNullOrEmpty();
        response.AccessTokenLifetime.Should().Be(0);
        response.IdentityToken.Should().NotBeNullOrEmpty();
        response.Code.Should().NotBeNullOrEmpty();
        response.Scope.Should().NotBeNullOrEmpty();
        response.SessionState.Should().BeNullOrEmpty();
        response.RedirectUri.Should().BeNullOrEmpty();
        response.RefreshToken.Should().NotBeNullOrEmpty();
    }


    [Fact]
    public async Task HybridFlow_AllToken_InvalidCodeChallengeMethod_ReturnInvalidRequest()
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
            "openid email profile phone offline_access",
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "invalid",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var errorResponse = await returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        errorResponse.IsError.Should().Be(true);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorResponse.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidCodeChallenge));
        errorResponse.ClientId.Should().NotBeNullOrEmpty();
        errorResponse.TraceId.Should().NotBeNullOrEmpty();
        errorResponse.RedirectUri.Should().NotBeNullOrEmpty();
        errorResponse.ResponseMode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HybridFlow_AllToken_MissingredirectUri_ReturnInvalidRedirectUri()
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
            "openid email profile phone offline_access",
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            //redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var errorResponse = await returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        errorResponse.IsError.Should().Be(true);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorResponse.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidRedirectUri));
        errorResponse.ClientId.Should().NotBeNullOrEmpty();
        errorResponse.TraceId.Should().NotBeNullOrEmpty();
        errorResponse.RedirectUri.Should().BeNullOrEmpty();
        errorResponse.ResponseMode.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task HybridFlow_AllToken_InvalidredirectUri_ReturnInvalidRedirectUri()
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
            "openid email profile phone offline_access",
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri + "123",
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var errorResponse = await returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        errorResponse.IsError.Should().Be(true);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorResponse.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.RedirectUriNotRegistered));
        errorResponse.ClientId.Should().NotBeNullOrEmpty();
        errorResponse.TraceId.Should().NotBeNullOrEmpty();
        errorResponse.RedirectUri.Should().NotBeNullOrEmpty();
        errorResponse.ResponseMode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HybridFlow_AllToken_MissingNonce_ReturnInvalidNonce()
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
            "openid email profile phone offline_access",
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri
            //nonce: nonce
        );
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var errorResponse = await returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        errorResponse.IsError.Should().Be(true);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorResponse.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        errorResponse.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidNonce));
        errorResponse.ClientId.Should().NotBeNullOrEmpty();
        errorResponse.TraceId.Should().NotBeNullOrEmpty();
        errorResponse.RedirectUri.Should().NotBeNullOrEmpty();
        errorResponse.ResponseMode.Should().NotBeNullOrEmpty();
    }


    // JWT Secret Start
    public string GenerateJWTSecretRequest(string clientId, string clientSecret, string algorithm)
    {
        var now = DateTime.UtcNow;
        var exp = now.AddMinutes(5);
        var securityKey = Encoding.ASCII.GetBytes(clientSecret);
        var credentials = new SigningCredentials(new SymmetricSecurityKey(securityKey), algorithm);
        var claims = new List<Claim>
        {
            new("sub", clientId),
            new("iat", now.ToUnixTime().ToString()),
            new("jti", AuthenticationConstants.KeySize32.RandomString()),
            new("exp", exp.ToUnixTime().ToString())
        };

        // Create the JWT and write it to a string
        var jwt = new JwtSecurityToken(
            "Testissuer",
            "Testaudience",
            claims,
            now,
            signingCredentials: credentials);

        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
        return encodedJwt;
    }
}
