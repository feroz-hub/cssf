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

public class BasicAuthenticationFlowTests : HCLCSSFFakeSetup
{
    private const string Category = "BasicAuthenticationFlowTests";
    private readonly string passWord = "Test@123456789";
    private readonly string positiveCaseClientName = "HCL.CS.SF Plain PKCE Client";

    private readonly string redirectUri = "http://127.0.0.1:63562/";

    private readonly string userName = "checktest";
    private ClientsModel clientModel;

    // Basic Authentication Strart

    [Fact]
    public async Task TokenGeneration_AuthCodeBasicAuthenticationFloWInvalidCode_ReturnInvalidAuthorizationCodeError()
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
            "openid email profile offline_access phone",
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
        tokenClient.SetBasicAuthentication(clientModel.ClientId, clientModel.ClientSecret);
        var tokenRequest = CreateTokenRequest(
            code: code + "XYZ123",
            redirectUri: redirectUri,
            grantType: OpenIdConstants.GrantTypes.AuthorizationCode,
            codeVerifier: codeVerifier);
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        tokenResult.IsError.Should().Be(true);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidGrant);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidAuthorizationCode));
    }

    [Fact]
    public async Task TokenGeneration_AuthCodeBasicAuthenticationFloWMissingCode_ReturnAuthorizationCodeMissingError()
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
            "openid email profile offline_access phone",
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
        tokenClient.SetBasicAuthentication(clientModel.ClientId, clientModel.ClientSecret);

        var tokenRequest = CreateTokenRequest(
            redirectUri: redirectUri,
            grantType: OpenIdConstants.GrantTypes.AuthorizationCode,
            codeVerifier: codeVerifier);
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResult.IsError.Should().Be(true);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidGrant);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.AuthorizationCodeMissing));
    }

    [Fact]
    public async Task TokenGeneration_AuthCodeBasicAuthenticationFloWMissingRedirectUri_ReturnRedirectUriMissingError()
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
            "openid email profile offline_access phone",
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
        tokenClient.SetBasicAuthentication(clientModel.ClientId, clientModel.ClientSecret);
        var tokenRequest = CreateTokenRequest(
            code: code,
            grantType: OpenIdConstants.GrantTypes.AuthorizationCode,
            codeVerifier: codeVerifier);

        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));

        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        tokenResult.IsError.Should().Be(true);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.UnauthorizedClient);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.RedirectUriMissing));
    }

    [Fact]
    public async Task TokenGeneration_AuthCodeBasicAuthenticationFloWInvalidRedirectUri_ReturnInvalidRedirectUriError()
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
            "openid email profile offline_access phone",
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
        tokenClient.SetBasicAuthentication(clientModel.ClientId, clientModel.ClientSecret);
        var tokenRequest = CreateTokenRequest(
            code: code,
            redirectUri: redirectUri + "1",
            grantType: OpenIdConstants.GrantTypes.AuthorizationCode,
            codeVerifier: codeVerifier);

        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));

        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        tokenResult.IsError.Should().Be(true);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidGrant);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidRedirectUri));
    }

    [Fact]
    public async Task TokenGeneration_AuthCodeBasicAuthenticationFloWMissingGrantType_ReturnGrantTypeIsMissingError()
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
            "openid email profile offline_access phone",
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
        tokenClient.SetBasicAuthentication(clientModel.ClientId, clientModel.ClientSecret);
        var tokenRequest = CreateTokenRequest(
            code: code,
            redirectUri: redirectUri,
            codeVerifier: codeVerifier);
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        tokenResult.IsError.Should().Be(true);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.UnsupportedGrantType);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.GrantTypeIsMissing));
    }

    [Fact]
    public async Task TokenGeneration_AuthCodeBasicAuthenticationFloWClientCredentialGrantType_ReturnSuccess()
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
            "HCL.CS.SF.client",
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
        tokenClient.SetBasicAuthentication(clientModel.ClientId, clientModel.ClientSecret);
        var tokenRequest = CreateTokenRequest(
            code: code,
            grantType: OpenIdConstants.GrantTypes.ClientCredentials,
            codeVerifier: codeVerifier);
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenResponseResult();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        tokenResult.access_token.Should().NotBeNullOrEmpty();
        tokenResult.id_token.Should().BeNullOrEmpty();
        tokenResult.refresh_token.Should().BeNullOrEmpty();
        tokenResult.token_type.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task TokenGeneration_AuthCodeBasicAuthenticationFloWUnKnownGrantType_ReturnGrantTypeIsMissingError()
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
            "openid email profile offline_access phone",
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
        tokenClient.SetBasicAuthentication(clientModel.ClientId, clientModel.ClientSecret);
        var tokenRequest = CreateTokenRequest(
            code: code,
            redirectUri: redirectUri,
            grantType: "invalidGrantype",
            codeVerifier: codeVerifier);
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));

        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        tokenResult.IsError.Should().Be(true);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.UnsupportedGrantType);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.GrantTypeIsMissing));
    }

    [Fact]
    public async Task TokenGeneration_AuthCodeBasicAuthenticationFloWMissingCodeChallenge_InvalidCodeVerifierError()
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
            "openid email profile offline_access phone",
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
        tokenClient.SetBasicAuthentication(clientModel.ClientId, clientModel.ClientSecret);
        var tokenRequest = CreateTokenRequest(
            code: code,
            redirectUri: redirectUri,
            grantType: OpenIdConstants.GrantTypes.AuthorizationCode);

        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResult.IsError.Should().Be(true);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidGrant);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidCodeVerifier));
    }

    [Fact]
    public async Task
        TokenGeneration_AuthCodeBasicAuthenticationFloWInvalidShortCodeChallenge_ReturnCodeVerifierTooShortError()
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
            "openid email profile offline_access phone",
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
        tokenClient.SetBasicAuthentication(clientModel.ClientId, clientModel.ClientSecret);
        var tokenRequest = CreateTokenRequest(
            code: code,
            redirectUri: redirectUri,
            grantType: OpenIdConstants.GrantTypes.AuthorizationCode,
            codeVerifier: "Hi");

        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResult.IsError.Should().Be(true);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidGrant);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.CodeVerifierTooShort));
    }

    [Fact]
    public async Task TokenGeneration_AuthCodeBasicAuthenticationFloWInvalidCodeChallenge_ReturnCodeVerifierError()
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
            "openid email profile offline_access phone",
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
        tokenClient.SetBasicAuthentication(clientModel.ClientId, clientModel.ClientSecret);
        var tokenRequest = CreateTokenRequest(
            code: code,
            redirectUri: redirectUri,
            grantType: OpenIdConstants.GrantTypes.AuthorizationCode,
            codeVerifier:
            "test_codechallenge_which_is_greater_than_the_minimum_number_of_characters_required_that_is_65");

        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResult.IsError.Should().Be(true);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidGrant);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.UnsupportedCodeChallengeMethod));
    }

    // Basic Authentication Negative Scenario For ROP*******************ROP**********************************

    [Fact]
    [Trait("Category", Category)]
    public async Task BasicAuthentication_ROP_MissingGrantType_ReturnGrantTypeIsMissingError()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(positiveCaseClientName);
        client.Should().NotBeNull();

        var tokenRequest = CreateTokenRequest(
            userName: userName,
            password: passWord,
            redirectUri: redirectUri,
            scope: "HCL.CS.SF.client");
        var tokenClient = BackChannelClient;
        tokenClient.SetBasicAuthentication(client.ClientId, client.ClientSecret);
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResult.IsError.Should().Be(true);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.UnsupportedGrantType);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.GrantTypeIsMissing));
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task BasicAuthentication_ROP_InvalidGrantType_ReturnErrorOpenIdScopeNotAllowed()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(positiveCaseClientName);
        client.Should().NotBeNull();
        var tokenRequest = CreateTokenRequest(
            grantType: OpenIdConstants.GrantTypes.ClientCredentials,
            userName: userName,
            password: passWord,
            scope: "HCL.CS.SF.client openid offline_access");
        var tokenClient = BackChannelClient;
        tokenClient.SetBasicAuthentication(client.ClientId, client.ClientSecret);

        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResult.IsError.Should().Be(true);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidScope);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidScopeOrNotAllowed));
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task BasicAuthentication_ROP_UnknownGrantType_ReturnErrorUnsupportedGrantType()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(positiveCaseClientName);
        client.Should().NotBeNull();
        var tokenRequest = CreateTokenRequest(
            grantType: "Invalid",
            userName: userName,
            password: passWord,
            redirectUri: redirectUri,
            scope: "HCL.CS.SF.client");
        var tokenClient = BackChannelClient;
        tokenClient.SetBasicAuthentication(client.ClientId, client.ClientSecret);

        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResult.IsError.Should().Be(true);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.UnsupportedGrantType);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.GrantTypeIsMissing));
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task BasicAuthentication_ROP_MissingUserName_ReturnErrorUserNameMissing()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(positiveCaseClientName);
        client.Should().NotBeNull();
        var tokenRequest = CreateTokenRequest(
            grantType: OpenIdConstants.GrantTypes.Password,
            password: passWord,
            redirectUri: redirectUri,
            scope: "HCL.CS.SF.client");
        var tokenClient = BackChannelClient;
        tokenClient.SetBasicAuthentication(client.ClientId, client.ClientSecret);

        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResult.IsError.Should().Be(true);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidGrant);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.UserNameMissing));
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task BasicAuthentication_ROP_InvalidUserName_RetrunInvalidUserError()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(positiveCaseClientName);
        client.Should().NotBeNull();
        var tokenRequest = CreateTokenRequest(
            grantType: OpenIdConstants.GrantTypes.Password,
            userName: userName + "123",
            password: passWord,
            redirectUri: redirectUri,
            scope: "HCL.CS.SF.client");
        var tokenClient = BackChannelClient;
        tokenClient.SetBasicAuthentication(client.ClientId, client.ClientSecret);

        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        tokenResult.IsError.Should().Be(true);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidUser));
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task BasicAuthentication_ROP_MissingPassword_ReturnErrorUserAuthenticationFailed()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(positiveCaseClientName);
        client.Should().NotBeNull();
        var tokenRequest = CreateTokenRequest(
            grantType: OpenIdConstants.GrantTypes.Password,
            userName: userName,
            redirectUri: redirectUri,
            scope: "HCL.CS.SF.client");
        var tokenClient = BackChannelClient;
        tokenClient.SetBasicAuthentication(client.ClientId, client.ClientSecret);

        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        tokenResult.IsError.Should().Be(true);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidGrant);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.UserAuthenticationFailed));
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task BasicAuthentication_ROP_InvalidPassword_ReturnUserAuthenticationFailedError()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(positiveCaseClientName);
        client.ClientId = client.ClientId + "123";
        client.Should().NotBeNull();
        var tokenRequest = CreateTokenRequest(
            grantType: OpenIdConstants.GrantTypes.Password,
            userName: userName,
            password: passWord + "123",
            redirectUri: redirectUri,
            scope: "HCL.CS.SF.client");
        var tokenClient = BackChannelClient;
        tokenClient.SetBasicAuthentication(client.ClientId, client.ClientSecret);

        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResult.IsError.Should().Be(true);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidClient);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task BasicAuthentication_ROP_InvalidScope_ReturnInvalidScopeOrNotAllowed()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(positiveCaseClientName);
        client.Should().NotBeNull();
        var tokenRequest = CreateTokenRequest(
            grantType: OpenIdConstants.GrantTypes.Password,
            userName: userName,
            password: passWord,
            redirectUri: redirectUri,
            scope: "HCL.CS.SF.client testscope");
        var tokenClient = BackChannelClient;
        tokenClient.SetBasicAuthentication(client.ClientId, client.ClientSecret);
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResult.ErrorCode.Should().NotBeNull();
        tokenResult.IsError.Should().Be(true);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidScope);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidScopeOrNotAllowed));
    }

    // Basic Authentication Negative Scenario For ClientCredential*******************ClientCredential********************

    [Fact]
    [Trait("Category", Category)]
    public async Task
        TokenGeneration_BasicAuthenticationPassingCleintIdClientCredential_MissingclientIdToTheRequest_ReturnSuccess()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        FrontChannelClient.AllowAutoRedirect = false;
        var tokenClient = BackChannelClient;
        tokenClient.SetBasicAuthentication(clientModel.ClientId, clientModel.ClientSecret);
        var tokenRequest = CreateTokenRequest(
            clientSecret: clientModel.ClientSecret,
            grantType: OpenIdConstants.GrantTypes.ClientCredentials,
            scope: "HCL.CS.SF.client",
            userName: userName);
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenResponseResult();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        tokenResult.access_token.Should().NotBeNullOrEmpty();
        tokenResult.id_token.Should().BeNullOrEmpty();
        tokenResult.refresh_token.Should().BeNullOrEmpty();
        tokenResult.scope.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task TokenGeneration_BasicAuthenticationClientCredential_InvalidClientId()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        FrontChannelClient.AllowAutoRedirect = false;
        var tokenClient = BackChannelClient;
        tokenClient.SetBasicAuthentication(clientModel.ClientId + "123", clientModel.ClientSecret);
        var tokenRequest = CreateTokenRequest(
            grantType: OpenIdConstants.GrantTypes.ClientCredentials,
            scope: "HCL.CS.SF.client");
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        tokenResult.IsError.Should().Be(true);
        tokenResult.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidClient);
    }

    [Fact]
    public async Task
        TokenGeneration_BasicAuthenticationClientCredential_MissingClientSecretTotheRequest_RetunnSuccess()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        FrontChannelClient.AllowAutoRedirect = false;
        var tokenClient = BackChannelClient;
        tokenClient.SetBasicAuthentication(clientModel.ClientId, clientModel.ClientSecret);
        var tokenRequest = CreateTokenRequest(
            clientModel.ClientId,
            grantType: OpenIdConstants.GrantTypes.ClientCredentials,
            scope: "HCL.CS.SF.client",
            userName: userName);
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenResponseResult();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        tokenResult.access_token.Should().NotBeNullOrEmpty();
        tokenResult.id_token.Should().BeNullOrEmpty();
        tokenResult.refresh_token.Should().BeNullOrEmpty();
        tokenResult.scope.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task TokenGeneration_BasicAuthenticationClientCredential_MissingRedirectUri_ReturnSuccess()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        FrontChannelClient.AllowAutoRedirect = false;
        var tokenClient = BackChannelClient;
        tokenClient.SetBasicAuthentication(clientModel.ClientId, clientModel.ClientSecret);
        var tokenRequest = CreateTokenRequest(
            clientModel.ClientId,
            clientModel.ClientSecret,
            grantType: OpenIdConstants.GrantTypes.ClientCredentials,
            scope: "HCL.CS.SF.client");
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenResponseResult();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        tokenResult.access_token.Should().NotBeNullOrEmpty();
        tokenResult.id_token.Should().BeNullOrEmpty();
        tokenResult.refresh_token.Should().BeNullOrEmpty();
        tokenResult.scope.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task TokenGeneration_BasicAuthenticationClientCredential_IgnoreRedirectUri_ReturnSuccess()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        FrontChannelClient.AllowAutoRedirect = false;
        var tokenClient = BackChannelClient;
        tokenClient.SetBasicAuthentication(clientModel.ClientId, clientModel.ClientSecret);
        var tokenRequest = CreateTokenRequest(
            clientModel.ClientId,
            clientModel.ClientSecret,
            grantType: OpenIdConstants.GrantTypes.ClientCredentials,
            scope: "HCL.CS.SF.client");

        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));

        var tokenResult = await tokenResponse.ParseTokenResponseResult();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        tokenResult.access_token.Should().NotBeNullOrEmpty();
        tokenResult.id_token.Should().BeNullOrEmpty();
        tokenResult.refresh_token.Should().BeNullOrEmpty();
        tokenResult.scope.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task
        TokenGeneration_BasicAuthenticationClientCredential_MissingGrantType_ReturnErrorGrantTypeIsMissing()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        FrontChannelClient.AllowAutoRedirect = false;
        var tokenClient = BackChannelClient;
        tokenClient.SetBasicAuthentication(clientModel.ClientId, clientModel.ClientSecret);
        var tokenRequest = CreateTokenRequest(
            clientModel.ClientId,
            clientModel.ClientSecret,
            redirectUri: redirectUri,
            scope: "HCL.CS.SF.client");
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var tokenResult2 = await tokenResponse.ParseTokenErrorResponse();
        tokenResult2.ErrorCode.Should().NotBeNull();
        tokenResult2.IsError.Should().Be(true);
        tokenResult2.ErrorCode.Should().Be(OpenIdConstants.Errors.UnsupportedGrantType);
        tokenResult2.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.GrantTypeIsMissing));
    }

    [Fact]
    public async Task TokenGeneration_BasicAuthenticationClientCredentialInvalidGrantType_ReturnUnsupportedGrantType()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        FrontChannelClient.AllowAutoRedirect = false;
        var tokenClient = BackChannelClient;
        tokenClient.SetBasicAuthentication(clientModel.ClientId, clientModel.ClientSecret);
        var tokenRequest = CreateTokenRequest(
            clientModel.ClientId,
            clientModel.ClientSecret,
            redirectUri: redirectUri,
            grantType: "Invalid",
            scope: "HCL.CS.SF.client");
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));

        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var tokenResult2 = await tokenResponse.ParseTokenErrorResponse();
        tokenResult2.ErrorCode.Should().NotBeNull();
        tokenResult2.IsError.Should().Be(true);
        tokenResult2.ErrorCode.Should().Be(OpenIdConstants.Errors.UnsupportedGrantType);
        tokenResult2.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.GrantTypeIsMissing));
    }


    [Fact]
    public async Task
        TokenGeneration_BasicAuthenticationClientCredential_InvalidScope_ReturnInvalidScopeOrNotAllowedError()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        FrontChannelClient.AllowAutoRedirect = false;
        var tokenClient = BackChannelClient;
        tokenClient.SetBasicAuthentication(clientModel.ClientId, clientModel.ClientSecret);
        var tokenRequest = CreateTokenRequest(
            clientModel.ClientId,
            clientModel.ClientSecret,
            grantType: OpenIdConstants.GrantTypes.ClientCredentials,
            scope: "HCL.CS.SF.clientss");
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var tokenResult2 = await tokenResponse.ParseTokenErrorResponse();
        tokenResult2.IsError.Should().Be(true);
        tokenResult2.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidScope);
        tokenResult2.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidScopeOrNotAllowed));
    }

    // Old  Basic Authentication

    [Fact]
    public async Task<TokenResponseResultModel> TokenGeneration_AuthCodeFlow_BasicAuthentication_Success()
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
            "openid email profile offline_access phone",
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
        tokenClient.SetBasicAuthentication(clientModel.ClientId, clientModel.ClientSecret);
        var tokenRequest = CreateTokenRequest(
            code: code,
            redirectUri: redirectUri,
            grantType: OpenIdConstants.GrantTypes.AuthorizationCode,
            codeVerifier: codeVerifier);
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenResponseResult();
        tokenResult.access_token.Should().NotBeNull();
        tokenResult.id_token.Should().NotBeNull();
        tokenResult.refresh_token.Should().NotBeNull();
        tokenResult.expires_in.Should().BeGreaterThan(0);
        tokenResult.token_type.Should().Be(OpenIdConstants.TokenResponseType.BearerTokenType);

        return tokenResult;
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task TokenGeneration_ROP_BasicAuthentication_succeed()
    {
        await LoginAsync(User);
        var client = await FetchClientDetails(positiveCaseClientName);
        client.Should().NotBeNull();
        var tokenRequest = CreateTokenRequest(
            grantType: OpenIdConstants.GrantTypes.Password,
            userName: userName,
            password: passWord,
            redirectUri: redirectUri,
            scope: "HCL.CS.SF.client offline_access");
        var tokenClient = BackChannelClient;
        tokenClient.SetBasicAuthentication(client.ClientId, client.ClientSecret);

        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenResponseResult();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        tokenResult.access_token.Should().NotBeNull();
        tokenResult.id_token.Should().BeNullOrEmpty();
        tokenResult.refresh_token.Should().NotBeNull();
        tokenResult.expires_in.Should().BeGreaterThan(0);
        tokenResult.scope.Should().NotBeNullOrEmpty();
        tokenResult.token_type.Should().Be(OpenIdConstants.TokenResponseType.BearerTokenType);
    }

    [Fact]
    public async Task TokenGeneration_BasicAuthentication_ClientCredential()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        FrontChannelClient.AllowAutoRedirect = false;
        var tokenClient = BackChannelClient;
        tokenClient.SetBasicAuthentication(clientModel.ClientId, clientModel.ClientSecret);
        var tokenRequest = CreateTokenRequest(
            grantType: OpenIdConstants.GrantTypes.ClientCredentials,
            scope: "HCL.CS.SF.client",
            userName: userName);
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenResponseResult();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        tokenResult.access_token.Should().NotBeNull();
        tokenResult.id_token.Should().BeNull();
        tokenResult.refresh_token.Should().BeNullOrEmpty();
        tokenResult.expires_in.Should().BeGreaterThan(0);
        tokenResult.token_type.Should().Be(OpenIdConstants.TokenResponseType.BearerTokenType);
        tokenResult.scope.Should().NotBeNullOrEmpty();
    }
}
