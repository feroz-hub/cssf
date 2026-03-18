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
using static HCL.CS.SF.Domain.Constants.Endpoint.OpenIdConstants;

namespace IntegrationTests.Endpoint.FlowTests;

public class JWTPostFlowTest : HCLCSSFFakeSetup
{
    private readonly string positiveCaseClientName = "HCL.CS.SF Plain PKCE Client";
    private readonly string redirectUri = "http://127.0.0.1:63562/";
    private ClientsModel clientModel;

    // JWT Secret Start
    public string GenerateJWTSecretRequest(ClientsModel client)
    {
        var now = DateTime.UtcNow;
        var algorithm = Algorithms.HmacSha256;
        var securityKey = Encoding.ASCII.GetBytes(client.ClientSecret);
        var credentials = new SigningCredentials(new SymmetricSecurityKey(securityKey), algorithm);
        var claims = new List<Claim>
        {
            new("sub", client.ClientId),
            new("iat", now.ToUnixTime().ToString()),
            new("jti", AuthenticationConstants.KeySize32.RandomString())
        };

        // Create the JWT and write it to a string
        var jwt = new JwtSecurityToken(
            client.ClientId,
            TokenEndpoint,
            claims,
            now,
            DateTime.UtcNow.AddSeconds(client.AccessTokenExpiration),
            credentials);

        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
        return encodedJwt;
    }


    [Fact]
    public async Task TokenGeneration_AuthCodeFlow_JWTSecret_ValidInput_Success()
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
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();

        response.Code.Should().NotBeNull();
        var code = response.Code;

        var tokenClient = BackChannelClient;
        var tokenRequest = CreateTokenRequest(
            code: code,
            redirectUri: redirectUri,
            grantType: GrantTypes.AuthorizationCode,
            codeVerifier: codeVerifier,
            clientAssertionType: "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
            clientAssertion: GenerateJWTSecretRequest(clientModel));

        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));

        var tokenResult = await tokenResponse.ParseTokenResponseResult();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        tokenResult.access_token.Should().NotBeNull();
        tokenResult.id_token.Should().NotBeNull();
        tokenResult.refresh_token.Should().BeNullOrEmpty();
        tokenResult.scope.Should().NotBeNull();
        tokenResult.expires_in.Should().BeGreaterThan(0);
        tokenResult.token_type.Should().Be(TokenResponseType.BearerTokenType);
    }

    [Fact]
    public async Task TokenGeneration_AuthCodeFlow_MissingcodeJWTSecret_ReturnAuthorizationCodeMissingError()
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
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();

        response.Code.Should().NotBeNull();
        var code = response.Code;

        var tokenClient = BackChannelClient;
        var tokenRequest = CreateTokenRequest(
            redirectUri: redirectUri,
            grantType: GrantTypes.AuthorizationCode,
            codeVerifier: codeVerifier,
            clientAssertionType: "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
            clientAssertion: GenerateJWTSecretRequest(clientModel));
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResult.ErrorCode.Should().Be(Errors.InvalidGrant);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.AuthorizationCodeMissing));
    }

    [Fact]
    public async Task TokenGeneration_AuthCodeFlow_InvalidCodeJWTSecret_ReturnInvalidAuthorizationCode()
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
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();

        response.Code.Should().NotBeNull();
        var code = response.Code;

        var tokenClient = BackChannelClient;
        var tokenRequest = CreateTokenRequest(
            code: code + "123",
            redirectUri: redirectUri,
            grantType: GrantTypes.AuthorizationCode,
            codeVerifier: codeVerifier,
            clientAssertionType: "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
            clientAssertion: GenerateJWTSecretRequest(clientModel));

        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResult.ErrorCode.Should().Be(Errors.InvalidGrant);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidAuthorizationCode));
    }

    [Fact]
    public async Task TokenGeneration_AuthCodeFlow_MissingRedirectUriJWTSecret_ReturnRedirectUriMissingError()
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
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();

        response.Code.Should().NotBeNull();
        var code = response.Code;
        var tokenClient = BackChannelClient;
        var tokenRequest = CreateTokenRequest(
            code: code,
            grantType: GrantTypes.AuthorizationCode,
            codeVerifier: codeVerifier,
            clientAssertionType: "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
            clientAssertion: GenerateJWTSecretRequest(clientModel));
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        tokenResult.ErrorCode.Should().Be(Errors.UnauthorizedClient);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.RedirectUriMissing));
    }

    [Fact]
    public async Task TokenGeneration_AuthCodeFlow_InvalidRedirectUrIJWTSecret_ReturnInvalidRedirectUriError()
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
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();

        response.Code.Should().NotBeNull();
        var code = response.Code;

        var tokenClient = BackChannelClient;
        var tokenRequest = CreateTokenRequest(
            code: code,
            redirectUri: redirectUri + "123",
            grantType: GrantTypes.AuthorizationCode,
            codeVerifier: codeVerifier,
            clientAssertionType: "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
            clientAssertion: GenerateJWTSecretRequest(clientModel));
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        tokenResult.ErrorCode.Should().Be(Errors.InvalidGrant);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidRedirectUri));
    }

    [Fact]
    public async Task TokenGeneration_AuthCodeFlow_MissingGrantTypeJWTSecret_ReturnGrantTypeIsMissingError()
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
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();

        response.Code.Should().NotBeNull();
        var code = response.Code;

        var tokenClient = BackChannelClient;
        var tokenRequest = CreateTokenRequest(
            code: code,
            redirectUri: redirectUri,
            codeVerifier: codeVerifier,
            clientAssertionType: "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
            clientAssertion: GenerateJWTSecretRequest(clientModel));
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        tokenResult.ErrorCode.Should().Be(Errors.UnsupportedGrantType);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.GrantTypeIsMissing));
    }

    [Fact]
    public async Task TokenGeneration_AuthCodeFlow_InvalidGrantTypwJWTSecret_ReturnGrantTypeIsMissingError()
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
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();

        response.Code.Should().NotBeNull();
        var code = response.Code;

        var tokenClient = BackChannelClient;
        var tokenRequest = CreateTokenRequest(
            code: code,
            redirectUri: redirectUri,
            grantType: GrantTypes.AuthorizationCode + "123",
            codeVerifier: codeVerifier,
            clientAssertionType: "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
            clientAssertion: GenerateJWTSecretRequest(clientModel));
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        tokenResult.ErrorCode.Should().Be(Errors.UnsupportedGrantType);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.GrantTypeIsMissing));
    }

    [Fact]
    public async Task TokenGeneration_AuthCodeFlow_MissingCodeChallengeJWTSecret_ReturnInvalidCodeVerifierError()
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
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();

        response.Code.Should().NotBeNull();
        var code = response.Code;

        var tokenClient = BackChannelClient;
        var tokenRequest = CreateTokenRequest(
            code: code,
            redirectUri: redirectUri,
            grantType: GrantTypes.AuthorizationCode,
            clientAssertionType: "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
            clientAssertion: GenerateJWTSecretRequest(clientModel));
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        tokenResult.ErrorCode.Should().Be(Errors.InvalidGrant);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidCodeVerifier));
    }

    [Fact]
    public async Task
        TokenGeneration_AuthCodeFlow_InvalidCodeChallengeJWTSecret_ReturnUnsupportedCodeChallengeMethodError()
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
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();

        response.Code.Should().NotBeNull();
        var code = response.Code;

        var tokenClient = BackChannelClient;
        var tokenRequest = CreateTokenRequest(
            code: code,
            redirectUri: redirectUri,
            grantType: GrantTypes.AuthorizationCode,
            codeVerifier: codeVerifier + "123",
            clientAssertionType: "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
            clientAssertion: GenerateJWTSecretRequest(clientModel));
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        tokenResult.ErrorCode.Should().Be(Errors.InvalidGrant);
        tokenResult.ErrorDescription.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.UnsupportedCodeChallengeMethod));
    }

    [Fact]
    public async Task TokenGeneration_AuthCodeFlow_MissingClientAssertionTypeJWTSecret_ReturnInvalidClientError()
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
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();

        response.Code.Should().NotBeNull();
        var code = response.Code;

        var tokenClient = BackChannelClient;
        var tokenRequest = CreateTokenRequest(
            code: code,
            redirectUri: redirectUri,
            grantType: GrantTypes.AuthorizationCode,
            codeVerifier: codeVerifier,
            clientAssertion: GenerateJWTSecretRequest(clientModel));
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        tokenResult.ErrorCode.Should().Be(Errors.InvalidClient);
    }

    [Fact]
    public async Task TokenGeneration_AuthCodeFlow_InvalidClientAssertionJWTSecret_ReturnInvalidClientidError()
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
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();

        response.Code.Should().NotBeNull();
        var code = response.Code;

        var tokenClient = BackChannelClient;
        var tokenRequest = CreateTokenRequest(
            code: code,
            redirectUri: redirectUri,
            grantType: GrantTypes.AuthorizationCode,
            codeVerifier: codeVerifier,
            clientAssertionType: "urn:ietf:params:oauth:client-assertion-type:jwt-bearer" + "1234",
            clientAssertion: GenerateJWTSecretRequest(clientModel));
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        tokenResult.ErrorCode.Should().Be(Errors.InvalidClient);
    }

    [Fact]
    public async Task TokenGeneration_AuthCodeFlow_clientAssertionInvalidClientIDJWTSecret_returnInvalidClientIDError()
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
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();

        response.Code.Should().NotBeNull();
        var code = response.Code;

        clientModel.ClientId = clientModel.ClientId + "123";

        var tokenClient = BackChannelClient;
        var tokenRequest = CreateTokenRequest(
            code: code,
            redirectUri: redirectUri,
            grantType: GrantTypes.AuthorizationCode,
            codeVerifier: codeVerifier,
            clientAssertionType: "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
            clientAssertion: GenerateJWTSecretRequest(clientModel));
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        tokenResult.ErrorCode.Should().Be(Errors.InvalidClient);
    }

    [Fact]
    public async Task
        TokenGeneration_AuthCodeFlow_clientAssertionInvalidClientSecretJWTSecret_returnErrorInvalidClientSecretJWT()
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
            redirectUri: redirectUri,
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);

        var response = returnQuery.Headers.Location.ToString().ParseQueryString();

        response.Code.Should().NotBeNull();
        var code = response.Code;

        clientModel.ClientSecret = clientModel.ClientSecret + "123";

        var tokenClient = BackChannelClient;
        var tokenRequest = CreateTokenRequest(
            code: code,
            redirectUri: redirectUri,
            grantType: GrantTypes.AuthorizationCode,
            codeVerifier: codeVerifier,
            clientAssertionType: "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
            clientAssertion: GenerateJWTSecretRequest(clientModel));

        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        var tokenResult = await tokenResponse.ParseTokenErrorResponse();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        tokenResult.ErrorCode.Should().Be(Errors.InvalidClient);
    }
}
