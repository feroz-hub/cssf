/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Net;
using System.Security.Cryptography;
using FluentAssertions;
using IntegrationTests.ApiDomainModel;
using IntegrationTests.Endpoint.Setup;
using Xunit;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;

namespace IntegrationTests.Endpoint.FlowTests;

public class AuthorizeCodeFlowTests : HCLCSSFFakeSetup
{
    private readonly string hCLCSS256ClientName = "HCL.CS.SF S256 Client";
    private readonly string redirectUri = "http://127.0.0.1:63562/";
    private ClientsModel clientModel;
    private string positiveCaseClientName = "HCL.CS.SF S256 Client";

    [Fact]
    public async Task AuthorizeCodeFlow_ValidInput_Success()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(hCLCSS256ClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code",
            "openid email profile HCL.CS.SF.client",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();
        response.IsError = false;
        returnQuery.StatusCode.Should().Be(HttpStatusCode.Found);
        response.Code.Should().NotBeNullOrEmpty();
        response.Scope.Should().NotBeNullOrEmpty();
        response.SessionState.Should().BeNullOrEmpty();
        response.AccessToken.Should().BeNullOrEmpty();
        response.AccessTokenLifetime.Should().Be(0);
        response.IdentityToken.Should().BeNullOrEmpty();
        response.RedirectUri.Should().BeNullOrEmpty();
        response.RefreshToken.Should().BeNullOrEmpty();
        response.Request.Should().BeNull();
    }

    [Fact]
    public async Task AuthorizeCodeFlow_MissingClientId_ReturnErrorClientIdMissingInRequest()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            responseType: "code",
            scope: "openid email profile phone HCL.CS.SF.client",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Result.IsError.Should().Be(true);
        response.Result.ClientId.Should().BeNullOrEmpty();
        response.Result.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        response.Result.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.ClientIdMissingInRequest));
        response.Result.ErrorUri.Should().Be(BaseUrl.IncludeEndSlash() + ErrorUrl);
        response.Result.ResponseMode.Should().BeNull();
        response.Result.RedirectUri.Should().BeNull();
        response.Result.TraceId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AuthorizeCodeFlow_InvalidClientId_ReturnErrorClientDoesNotExist()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId + "123",
            "code",
            "openid email profile phone HCL.CS.SF.client",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Result.IsError.Should().Be(true);
        response.Result.ClientId.Should().NotBeNullOrEmpty();
        response.Result.ErrorCode.Should().Be(OpenIdConstants.Errors.UnauthorizedClient);
        response.Result.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.ClientDoesNotExist));
        response.Result.ErrorUri.Should().Be(BaseUrl.IncludeEndSlash() + ErrorUrl);
        response.Result.ResponseMode.Should().BeNull();
        response.Result.RedirectUri.Should().BeNull();
        response.Result.TraceId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AuthorizeCodeFlow_MissingResponceType_ReturnErrorUnsupportedResponseType()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();

        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            scope: "openid email profile phone HCL.CS.SF.client",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Result.IsError.Should().Be(true);
        response.Result.ClientId.Should().NotBeNullOrEmpty();
        response.Result.ErrorCode.Should().Be(OpenIdConstants.Errors.UnsupportedResponseType);
        response.Result.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.ResponseTypeMissing));
        response.Result.ErrorUri.Should().Be(BaseUrl.IncludeEndSlash() + ErrorUrl);
        response.Result.ResponseMode.Should().BeNull();
        response.Result.RedirectUri.Should().BeNull();
        response.Result.TraceId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AuthorizeCodeFlow_responseType_Token_ReturnErrorUnsupportedResponseType()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();

        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "token",
            "openid email profile phone HCL.CS.SF.client",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Result.IsError.Should().Be(true);
        response.Result.ClientId.Should().NotBeNullOrEmpty();
        response.Result.ErrorCode.Should().Be(OpenIdConstants.Errors.UnsupportedResponseType);
        response.Result.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.ResponseTypeMissing));
        response.Result.ErrorUri.Should().Be(BaseUrl.IncludeEndSlash() + ErrorUrl);
        response.Result.ResponseMode.Should().BeNull();
        response.Result.RedirectUri.Should().BeNull();
        response.Result.TraceId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AuthorizeCodeFlow_ResponseType_codeid_token_ReturnErrorUnsupportedResponseType()
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
            "openid email profile phone HCL.CS.SF.client",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Result.IsError.Should().Be(true);
        response.Result.ClientId.Should().NotBeNullOrEmpty();
        response.Result.ErrorCode.Should().Be(OpenIdConstants.Errors.UnsupportedResponseType);
        response.Result.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidResponseMode));
        response.Result.ErrorUri.Should().Be(BaseUrl.IncludeEndSlash() + ErrorUrl);
        response.Result.ResponseMode.Should().NotBeNullOrEmpty();
        response.Result.RedirectUri.Should().NotBeNullOrEmpty();
        response.Result.TraceId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AuthCodeFlow_ResponseType_id_token_ReturnErrorUnsupportedResponseType()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();

        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "id_token",
            "openid offline_access",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Result.IsError.Should().Be(true);
        response.Result.ClientId.Should().NotBeNullOrEmpty();
        response.Result.ErrorCode.Should().Be(OpenIdConstants.Errors.UnsupportedResponseType);
        response.Result.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.ResponseTypeMissing));
        response.Result.ErrorUri.Should().Be(BaseUrl.IncludeEndSlash() + ErrorUrl);
        response.Result.ResponseMode.Should().BeNull();
        response.Result.RedirectUri.Should().BeNull();
        response.Result.TraceId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AuthCodeFlow_ResponseType_codetoken_ReturnErrorUnsupportedResponseType()
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
            "openid offline_access",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Result.IsError.Should().Be(true);
        response.Result.ClientId.Should().NotBeNullOrEmpty();
        response.Result.ErrorCode.Should().Be(OpenIdConstants.Errors.UnsupportedResponseType);
        response.Result.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidResponseMode));
        response.Result.ErrorUri.Should().Be(BaseUrl.IncludeEndSlash() + ErrorUrl);
        response.Result.ResponseMode.Should().NotBeNullOrEmpty();
        response.Result.RedirectUri.Should().NotBeNullOrEmpty();
        response.Result.TraceId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AuthCodeFlow_ResponseType_codeid_tokentoken_ReturnErrorUnsupportedResponseType()
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
            "openid offline_access",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Result.IsError.Should().Be(true);
        response.Result.ClientId.Should().NotBeNullOrEmpty();
        response.Result.ErrorCode.Should().Be(OpenIdConstants.Errors.UnsupportedResponseType);
        response.Result.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidResponseMode));
        response.Result.ErrorUri.Should().Be(BaseUrl.IncludeEndSlash() + ErrorUrl);
        response.Result.ResponseMode.Should().NotBeNullOrEmpty();
        response.Result.RedirectUri.Should().NotBeNullOrEmpty();
        response.Result.TraceId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AuthCodeFlow_ignoreScope__ReturnSuccess()
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
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();
        response.IsError = false;
        returnQuery.StatusCode.Should().Be(HttpStatusCode.Found);
        response.Code.Should().NotBeNullOrEmpty();
        response.Scope.Should().NotBeNullOrEmpty();
        response.SessionState.Should().BeNullOrEmpty();
        response.AccessToken.Should().BeNullOrEmpty();
        response.AccessTokenLifetime.Should().Be(0);
        response.IdentityToken.Should().BeNullOrEmpty();
        response.RedirectUri.Should().BeNullOrEmpty();
        response.RefreshToken.Should().BeNullOrEmpty();
        response.Request.Should().BeNull();
    }

    [Fact]
    public async Task AuthCodeFlow_InvalidScope_ReturnRequestedScopeNotAllowedForClient()
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
            "openidemailprofilephone",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Result.IsError.Should().Be(true);
        response.Result.ClientId.Should().NotBeNullOrEmpty();
        response.Result.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        response.Result.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.RequestedScopeNotAllowedForClient));
        response.Result.ErrorUri.Should().Be(BaseUrl.IncludeEndSlash() + ErrorUrl);
        response.Result.ResponseMode.Should().NotBeNullOrEmpty();
        response.Result.RedirectUri.Should().NotBeNullOrEmpty();
        response.Result.TraceId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AuthCodeFlow_FragmentResponseMode_ReturnInvalidResponseMode()
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
            "openid email profile phone",
            responseMode: "Fragment",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Result.IsError.Should().Be(true);
        response.Result.ClientId.Should().NotBeNullOrEmpty();
        response.Result.ErrorCode.Should().Be(OpenIdConstants.Errors.UnsupportedResponseType);
        response.Result.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidResponseMode));
        response.Result.ErrorUri.Should().Be(BaseUrl.IncludeEndSlash() + ErrorUrl);
        response.Result.ResponseMode.Should().NotBeNullOrEmpty();
        response.Result.RedirectUri.Should().NotBeNullOrEmpty();
        response.Result.TraceId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AuthCodeFlow_from_postResponseMode_ReturnInvalidResponseMode()
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
            "openid email profile phone",
            responseMode: "from_post",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Result.IsError.Should().Be(true);
        response.Result.ClientId.Should().NotBeNullOrEmpty();
        response.Result.ErrorCode.Should().Be(OpenIdConstants.Errors.UnsupportedResponseType);
        response.Result.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidResponseMode));
        response.Result.ErrorUri.Should().Be(BaseUrl.IncludeEndSlash() + ErrorUrl);
        response.Result.ResponseMode.Should().NotBeNullOrEmpty();
        response.Result.RedirectUri.Should().NotBeNullOrEmpty();
        response.Result.TraceId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AuthCodeFlow_InvalidResponseMode_ReturnInvalidResponseMode()
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
            "openid email profile phone",
            responseMode: "invalid",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Result.IsError.Should().Be(true);
        response.Result.ClientId.Should().NotBeNullOrEmpty();
        response.Result.ErrorCode.Should().Be(OpenIdConstants.Errors.UnsupportedResponseType);
        response.Result.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidResponseMode));
        response.Result.ErrorUri.Should().Be(BaseUrl.IncludeEndSlash() + ErrorUrl);
        response.Result.ResponseMode.Should().NotBeNullOrEmpty();
        response.Result.RedirectUri.Should().NotBeNullOrEmpty();
        response.Result.TraceId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AuthCodeFlow_loginPrompt_ReturnauthorizecallbackUrl()
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
            "openid email profile phone",
            responseMode: "query",
            prompt: "login",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().Contains("authorizecallback");
        returnQuery.StatusCode.Should().Be(HttpStatusCode.Found);
        response.Should().Be(true);
    }

    [Fact]
    public async Task AuthCodeFlow_consentPrompt_ReturInvalidPrompt()
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
            "openid email profile phone", // offline_access
            responseMode: "query",
            prompt: "consent",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var response = returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        response.Result.IsError.Should().Be(true);
        response.Result.ClientId.Should().NotBeNullOrEmpty();
        response.Result.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        response.Result.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidPrompt));
        response.Result.ErrorUri.Should().Be(BaseUrl.IncludeEndSlash() + ErrorUrl);
        response.Result.ResponseMode.Should().NotBeNullOrEmpty();
        response.Result.RedirectUri.Should().NotBeNullOrEmpty();
        response.Result.TraceId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AuthCodeFlow_select_accountPrompt_Returnauthorizecallback()
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
            "openid email profile phone",
            responseMode: "query",
            prompt: "select_account",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().Contains("authorizecallback");
        returnQuery.StatusCode.Should().Be(HttpStatusCode.Found);
        response.Should().Be(true);
    }

    [Fact]
    public async Task AuthCodeFlow_MissingCodeChallenge_ReturnInvalidRequest()
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code",
            "openid email profile phone",
            responseMode: "query",
            prompt: "none",
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Result.IsError.Should().Be(true);
        response.Result.ClientId.Should().NotBeNullOrEmpty();
        response.Result.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        response.Result.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidCodeChallenge));
        response.Result.ErrorUri.Should().Be(BaseUrl.IncludeEndSlash() + ErrorUrl);
        response.Result.ResponseMode.Should().NotBeNullOrEmpty();
        response.Result.RedirectUri.Should().NotBeNullOrEmpty();
        response.Result.TraceId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AuthCodeFlow_CodeChallengeMethodS256_ReturnSuccess()
    {
        await LoginAsync(User);
        positiveCaseClientName = hCLCSS256ClientName;
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        var codeChallengeString = codeVerifier.GenerateCodeChallenge();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code",
            "openid HCL.CS.SF.client offline_access HCL.CS.SF.role HCL.CS.SF.user HCL.CS.SF.apiresource",
            // "openid email profile phone",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeChallengeString, // Codeverifier
            codeChallengeMethod: "S256", // Plain
            maxAge: "60",
            redirectUri: "http://127.0.0.1:63562/",
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();
        var tokenRequest = CreateTokenRequest(
            clientModel.ClientId,
            clientModel.ClientSecret,
            response.Code,
            "http://127.0.0.1:63562/",
            OpenIdConstants.GrantTypes.AuthorizationCode,
            codeVerifier); // Code Challenge
        var tokenClient = BackChannelClient;
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = tokenResponse.ParseTokenResponseResult();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        tokenResult.Result.access_token.Should().NotBeNullOrEmpty();
        tokenResult.Result.id_token.Should().NotBeNullOrEmpty();
        tokenResult.Result.token_type.Should().NotBeNullOrEmpty();
        tokenResult.Result.refresh_token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task
        AuthCodeFlow_InvalidCodeChallengeMethodS256PassingCodeChallengeTocodeVerifier_ReturnUnsupportedCodeChallengeMethod()
    {
        await LoginAsync(User);
        positiveCaseClientName = hCLCSS256ClientName;
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        var codeChallengeString = codeVerifier.GenerateCodeChallenge();

        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code",
            "openid email profile phone",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "S256",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();

        var tokenRequest = CreateTokenRequest(
            clientModel.ClientId,
            clientModel.ClientSecret,
            response.Code,
            redirectUri,
            OpenIdConstants.GrantTypes.AuthorizationCode,
            codeChallengeString);
        var tokenClient = BackChannelClient;
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var tokenResult = tokenResponse.ParseTokenErrorResponse();
        tokenResult.Result.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidGrant);
        tokenResult.Result.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.UnsupportedCodeChallengeMethod));
    }

    [Fact]
    public async Task
        AuthCodeFlow_InvalidCodeChallengeMethodS256PassingCodeChallengeToTokenRequest_ReturnUnsupportedCodeChallengeMethod()
    {
        await LoginAsync(User);
        positiveCaseClientName = hCLCSS256ClientName;
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        var codeChallengeString = codeVerifier.GenerateCodeChallenge();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code",
            "openid email profile phone",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeChallengeString,
            codeChallengeMethod: "S256",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();

        var tokenRequest = CreateTokenRequest(
            clientModel.ClientId,
            clientModel.ClientSecret,
            response.Code,
            redirectUri,
            OpenIdConstants.GrantTypes.AuthorizationCode,
            codeChallengeString);
        var tokenClient = BackChannelClient;
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var tokenResult = tokenResponse.ParseTokenErrorResponse();
        tokenResult.Result.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidGrant);
        tokenResult.Result.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.UnsupportedCodeChallengeMethod));
    }

    [Fact]
    public async Task
        AuthCodeFlow_InvalidCodeChallengeMethodS256PassingValidCodeChallengeToTokenRequestButPassingInvalidCodeChallengetoAuthCode_ReturnUnsupportedCodeChallengeMethod()
    {
        await LoginAsync(User);
        positiveCaseClientName = hCLCSS256ClientName;
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code",
            "openid email profile phone",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "S256",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();

        var tokenRequest = CreateTokenRequest(
            clientModel.ClientId,
            clientModel.ClientSecret,
            response.Code,
            redirectUri,
            OpenIdConstants.GrantTypes.AuthorizationCode,
            codeVerifier);
        var tokenClient = BackChannelClient;
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var tokenResult = tokenResponse.ParseTokenErrorResponse();
        tokenResult.Result.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidGrant);
        tokenResult.Result.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.UnsupportedCodeChallengeMethod));
    }

    [Fact]
    public async Task AuthCodeFlow_MissingredirectUri_InvalidRedirectUri()
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
            "openid email profile phone",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Result.IsError.Should().Be(true);
        response.Result.ClientId.Should().NotBeNullOrEmpty();
        response.Result.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        response.Result.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidRedirectUri));
        response.Result.ErrorUri.Should().Be(BaseUrl.IncludeEndSlash() + ErrorUrl);
        response.Result.ResponseMode.Should().BeNull();
        response.Result.RedirectUri.Should().BeNull();
        response.Result.TraceId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AuthCodeFlow_InvalidredirectUri_RedirectUriNotRegistered()
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
            "openid email profile phone",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: "https://localhost:44300/index.htmlinvalid",
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
        returnQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Result.IsError.Should().Be(true);
        response.Result.ClientId.Should().NotBeNullOrEmpty();
        response.Result.ErrorCode.Should().Be(OpenIdConstants.Errors.InvalidRequest);
        response.Result.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.RedirectUriNotRegistered));
        response.Result.ErrorUri.Should().Be(BaseUrl.IncludeEndSlash() + ErrorUrl);
        response.Result.ResponseMode.Should().NotBeNullOrEmpty();
        response.Result.RedirectUri.Should().NotBeNullOrEmpty();
        response.Result.TraceId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task TokenGeneration_AuthCodeFlow_valid_Data()
    {
        var keyLength = 32;
        var rngCryptoServiceProvider = RandomNumberGenerator.Create();
        var randomBytes = new byte[keyLength];
        rngCryptoServiceProvider.GetBytes(randomBytes);

        await LoginAsync(User);
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;
        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code",
            "openid email profile phone",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();
        returnQuery.StatusCode.Should().Be(HttpStatusCode.Found);
        response.Code.Should().NotBeNullOrEmpty();
        response.Scope.Should().NotBeNullOrEmpty();
        response.SessionState.Should().BeNullOrEmpty();
        response.AccessToken.Should().BeNullOrEmpty();
        response.AccessTokenLifetime.Should().Be(0);
        response.IdentityToken.Should().BeNullOrEmpty();
        response.RedirectUri.Should().BeNullOrEmpty();
        response.RefreshToken.Should().BeNullOrEmpty();
        response.Request.Should().BeNull();
    }
}
