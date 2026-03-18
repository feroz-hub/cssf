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
using IntegrationTests.Endpoint.Helper;
using IntegrationTests.Endpoint.Setup;
using Xunit;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;

namespace IntegrationTests.Endpoint.FlowTests;

public class JwksFlowTest : HCLCSSFFakeSetup
{
    private const string Category = "Introspection endpoint";
    private readonly string audience = "https://server/security/token";

    private readonly string hCLCSEarlyTokenExpireClient = "HCL.CS.SF Early Token Expire Client";
    private readonly string hclCSES256AlgorithmClient = "HCL.CS.SF ES256";
    private readonly string hclCSES512AlgorithmClient = "HCL.CS.SF ES512";

    private readonly string hclCSHS256AlgorithmClient = "HCL.CS.SF HS256";
    private readonly string hclCSHS512AlgorithmClient = "HCL.CS.SF HS512";

    private readonly string hclCSPS256AlgorithmClient = "HCL.CS.SF PS256";
    private readonly string hclCSPS512AlgorithmClient = "HCL.CS.SF PS512";
    private readonly string hclCSRS256AlgorithmClient = "HCL.CS.SF RS256";

    private readonly string hclCSRS512AlgorithmClient = "HCL.CS.SF RS512";
    private readonly string issuer = "security.HCL.CS.SF.com";
    private readonly JwksTestHelper JwksTestHelper = new();

    private readonly string redirectUri = "http://127.0.0.1:63562/";
    private ClientsModel clientModel;
    private string positiveCaseClientName = "HCL.CS.SF Plain PKCE Client";


    [Fact]
    [Trait("Category", Category)]
    public async Task JwksAsymmetricRS256_PassingValidAccessToken_ReturnSuccess()
    {
        string clientName = hclCSRS256AlgorithmClient,
            responseType = "code",
            scopes = "openid email profile phone offline_access",
            codeChallengeMethod = "plain";
        var tokenResult = await TokenGenerationFlow(clientName, responseType, scopes, codeChallengeMethod);
        var result = JwksTestHelper.ValidateToken(tokenResult.access_token, issuer, audience).GetAwaiter().GetResult();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task JwksAsymmetricRS256_PassingValidIssueandAudienceAccessToken_ReturnSuccess()
    {
        string clientName = hclCSRS256AlgorithmClient,
            responseType = "code",
            scopes = "openid email profile phone offline_access",
            codeChallengeMethod = "plain";
        var tokenResult = await TokenGenerationFlow(clientName, responseType, scopes, codeChallengeMethod);
        var result = JwksTestHelper.ValidateToken(tokenResult.access_token, issuer, audience).GetAwaiter().GetResult();
        result.Should().NotBeNull();
    }

    // RS 512

    [Fact]
    [Trait("Category", Category)]
    public async Task JwksAsymmetricRS512_PassingValidIssueandAudienceAccessToken_ReturnSuccess()
    {
        string clientName = hclCSRS512AlgorithmClient,
            responseType = "code",
            scopes = "openid email profile phone offline_access",
            codeChallengeMethod = "plain";
        var tokenResult = await TokenGenerationFlow(clientName, responseType, scopes, codeChallengeMethod);
        var result = JwksTestHelper.ValidateToken(tokenResult.access_token, issuer, audience).GetAwaiter().GetResult();
        result.Should().NotBeNull();
    }

    // ES 256

    [Fact]
    [Trait("Category", Category)]
    public async Task JwksAsymmetricES256_PassingValidIssueandAudienceAccessToken_ReturnSuccess()
    {
        string clientName = hclCSES256AlgorithmClient,
            responseType = "code",
            scopes = "openid email profile phone offline_access",
            codeChallengeMethod = "plain";
        var tokenResult = await TokenGenerationFlow(clientName, responseType, scopes, codeChallengeMethod);
        var result = JwksTestHelper.ValidateToken(tokenResult.access_token, issuer, audience, clientModel.ClientSecret)
            .GetAwaiter().GetResult();
        result.Should().NotBeNull();
    }

    // ES 512

    [Fact]
    [Trait("Category", Category)]
    public async Task JwksAsymmetricES512_PassingValidIssueandAudienceAccessToken_ReturnSuccess()
    {
        string clientName = hclCSES512AlgorithmClient,
            responseType = "code",
            scopes = "openid email profile phone offline_access",
            codeChallengeMethod = "plain";
        var tokenResult = await TokenGenerationFlow(clientName, responseType, scopes, codeChallengeMethod);
        var result = JwksTestHelper.ValidateToken(tokenResult.access_token, issuer, audience, clientModel.ClientSecret)
            .GetAwaiter().GetResult();
        result.Should().NotBeNull();
    }

    // PS 256

    [Fact]
    [Trait("Category", Category)]
    public async Task JwksAsymmetricPS256_PassingValidAccessToken_ReturnSuccess()
    {
        string clientName = hclCSPS256AlgorithmClient,
            responseType = "code",
            scopes = "openid email profile phone offline_access",
            codeChallengeMethod = "plain";
        var tokenResult = await TokenGenerationFlow(clientName, responseType, scopes, codeChallengeMethod);
        var result = JwksTestHelper.ValidateToken(tokenResult.access_token, issuer, audience, clientModel.ClientSecret)
            .GetAwaiter().GetResult();
        result.Should().NotBeNull();
    }

    // PS 512

    [Fact]
    [Trait("Category", Category)]
    public async Task JwksAsymmetricPS512_PassingValidAccessToken_ReturnSuccess()
    {
        string clientName = hclCSPS512AlgorithmClient,
            responseType = "code",
            scopes = "openid email profile phone offline_access",
            codeChallengeMethod = "plain";
        var tokenResult = await TokenGenerationFlow(clientName, responseType, scopes, codeChallengeMethod);
        var result = JwksTestHelper.ValidateToken(tokenResult.access_token, issuer, audience, clientModel.ClientSecret)
            .GetAwaiter().GetResult();
        result.Should().NotBeNull();
    }

    // Access Token - Symmetric -Positive Flows.
    // HS 256

    [Fact]
    [Trait("Category", Category)]
    public async Task JwksSymmetric_PassingValidAccess_token_ReturnSuccess()
    {
        string clientName = hclCSHS256AlgorithmClient,
            responseType = "code",
            scopes = "openid email profile phone offline_access",
            codeChallengeMethod = "plain";
        var tokenResult = await TokenGenerationFlow(clientName, responseType, scopes, codeChallengeMethod);
        var result = JwksTestHelper.ValidateToken(tokenResult.access_token, issuer, audience, clientModel.ClientSecret)
            .GetAwaiter().GetResult();
        result.Should().NotBeNull();
    }

    // HS 512

    [Fact]
    [Trait("Category", Category)]
    public async Task JwksSymmetricHS512_PassingValidAccess_token_ReturnSuccess()
    {
        string clientName = hclCSHS512AlgorithmClient,
            responseType = "code",
            scopes = "openid email profile phone offline_access",
            codeChallengeMethod = "plain";
        var tokenResult = await TokenGenerationFlow(clientName, responseType, scopes, codeChallengeMethod);
        var result = JwksTestHelper.ValidateToken(tokenResult.access_token, issuer, audience, clientModel.ClientSecret)
            .GetAwaiter().GetResult();
        result.Should().NotBeNull();
    }

    // Identity Token - Asymmetric -Positive Flows.

    // RS 256

    [Fact]
    [Trait("Category", Category)]
    public async Task JwksAsymmetricRS256_PassingValidIdentityToken_ReturnSuccess()
    {
        string clientName = hclCSRS256AlgorithmClient,
            responseType = "code",
            scopes = "openid email profile phone offline_access",
            codeChallengeMethod = "plain";
        var tokenResult = await TokenGenerationFlow(clientName, responseType, scopes, codeChallengeMethod);
        tokenResult.Should().NotBeNull();
        var result = JwksTestHelper.ValidateToken(tokenResult.id_token).GetAwaiter().GetResult();
        result.Id.Should().NotBeNull();
        result.SigningKey.Should().NotBeNull();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task JwksAsymmetricRS256_PassingValidIssueandAudienceIdentityToken_ReturnSuccess()
    {
        string clientName = hclCSRS256AlgorithmClient,
            responseType = "code",
            scopes = "openid email profile phone offline_access",
            codeChallengeMethod = "plain";
        var tokenResult = await TokenGenerationFlow(clientName, responseType, scopes, codeChallengeMethod);
        var result = JwksTestHelper.ValidateToken(tokenResult.id_token, issuer, clientModel.ClientId).GetAwaiter()
            .GetResult();
        result.Should().NotBeNull();
    }

    // RS 512

    [Fact]
    [Trait("Category", Category)]
    public async Task JwksAsymmetricRS512_PassingValidIssueandAudienceIdentityToken_ReturnSuccess()
    {
        string clientName = hclCSRS512AlgorithmClient,
            responseType = "code",
            scopes = "openid email profile phone offline_access",
            codeChallengeMethod = "plain";
        var tokenResult = await TokenGenerationFlow(clientName, responseType, scopes, codeChallengeMethod);
        var result = JwksTestHelper.ValidateToken(tokenResult.id_token, issuer, clientModel.ClientId).GetAwaiter()
            .GetResult();
        result.Should().NotBeNull();
    }

    // ES 256

    [Fact]
    [Trait("Category", Category)]
    public async Task JwksAsymmetricES256_PassingValidIssueandAudienceIdentityToken_ReturnSuccess()
    {
        string clientName = hclCSES256AlgorithmClient,
            responseType = "code",
            scopes = "openid email profile phone offline_access",
            codeChallengeMethod = "plain";
        var tokenResult = await TokenGenerationFlow(clientName, responseType, scopes, codeChallengeMethod);
        var result = JwksTestHelper
            .ValidateToken(tokenResult.id_token, issuer, clientModel.ClientId, clientModel.ClientSecret).GetAwaiter()
            .GetResult();
        result.Should().NotBeNull();
    }

    // ES 512

    [Fact]
    [Trait("Category", Category)]
    public async Task JwksAsymmetricES512_PassingValidIssueandAudienceIdentityToken_ReturnSuccess()
    {
        string clientName = hclCSES512AlgorithmClient,
            responseType = "code",
            scopes = "openid email profile phone offline_access",
            codeChallengeMethod = "plain";
        var tokenResult = await TokenGenerationFlow(clientName, responseType, scopes, codeChallengeMethod);
        var result = JwksTestHelper
            .ValidateToken(tokenResult.id_token, issuer, clientModel.ClientId, clientModel.ClientSecret).GetAwaiter()
            .GetResult();
        result.Should().NotBeNull();
    }

    // PS 256

    [Fact]
    [Trait("Category", Category)]
    public async Task JwksAsymmetricPS256_PassingValid_IdentityToken_ReturnSuccess()
    {
        string clientName = hclCSPS256AlgorithmClient,
            responseType = "code",
            scopes = "openid email profile phone offline_access",
            codeChallengeMethod = "plain";
        var tokenResult = await TokenGenerationFlow(clientName, responseType, scopes, codeChallengeMethod);
        var result = JwksTestHelper
            .ValidateToken(tokenResult.id_token, issuer, clientModel.ClientId, clientModel.ClientSecret).GetAwaiter()
            .GetResult();
        result.Should().NotBeNull();
    }

    // PS 512

    [Fact]
    [Trait("Category", Category)]
    public async Task JwksAsymmetricPS512_PassingValidIdentityToken_ReturnSuccess()
    {
        string clientName = hclCSPS512AlgorithmClient,
            responseType = "code",
            scopes = "openid email profile phone offline_access",
            codeChallengeMethod = "plain";
        var tokenResult = await TokenGenerationFlow(clientName, responseType, scopes, codeChallengeMethod);
        var result = JwksTestHelper
            .ValidateToken(tokenResult.id_token, issuer, clientModel.ClientId, clientModel.ClientSecret).GetAwaiter()
            .GetResult();
        result.Should().NotBeNull();
    }

    // Identity Token - Symmetric -Positive Flows.
    // HS 256

    [Fact]
    [Trait("Category", Category)]
    public async Task JwksSymmetricHS_PassingValidIdentity_token_ReturnSuccess()
    {
        string clientName = hclCSHS256AlgorithmClient,
            responseType = "code",
            scopes = "openid email profile phone offline_access",
            codeChallengeMethod = "plain";
        var tokenResult = await TokenGenerationFlow(clientName, responseType, scopes, codeChallengeMethod);
        var result = JwksTestHelper
            .ValidateToken(tokenResult.id_token, issuer, clientModel.ClientId, clientModel.ClientSecret).GetAwaiter()
            .GetResult();
        result.Should().NotBeNull();
    }

    // HS 512

    [Fact]
    [Trait("Category", Category)]
    public async Task JwksSymmetricHS512_PassingValidIdentity_token_ReturnSuccess()
    {
        string clientName = hclCSHS512AlgorithmClient,
            responseType = "code",
            scopes = "openid email profile phone offline_access",
            codeChallengeMethod = "plain";
        var tokenResult = await TokenGenerationFlow(clientName, responseType, scopes, codeChallengeMethod);
        var result = JwksTestHelper
            .ValidateToken(tokenResult.id_token, issuer, clientModel.ClientId, clientModel.ClientSecret).GetAwaiter()
            .GetResult();
        result.Should().NotBeNull();
    }

    // Negative Scenarios

    [Fact]
    [Trait("Category", Category)]
    public async Task JwksAsymmetricRS256_PassingInvalidAccessToken_ReturnsResultNull()
    {
        string clientName = hclCSRS256AlgorithmClient,
            responseType = "code",
            scopes = "openid email profile phone offline_access",
            codeChallengeMethod = "plain";
        var tokenResult = await TokenGenerationFlow(clientName, responseType, scopes, codeChallengeMethod);
        tokenResult.Should().NotBeNull();
        var accesstoken = tokenResult.access_token;
        var index = accesstoken.Length - 3;
        var resultAccessToken = accesstoken.Substring(2, index);
        var result = JwksTestHelper
            .ValidateToken(resultAccessToken, issuer, clientModel.ClientId, clientModel.ClientSecret).GetAwaiter()
            .GetResult();
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task JwksAsymmetricRS256_PassingInvalidAccessToken_ReturnNull()
    {
        string clientName = hclCSRS256AlgorithmClient,
            responseType = "code",
            scopes = "openid email profile phone offline_access",
            codeChallengeMethod = "plain";
        var tokenResult = await TokenGenerationFlow(clientName, responseType, scopes, codeChallengeMethod);
        var accesstoken = tokenResult.access_token + "zxo";
        var result = JwksTestHelper.ValidateToken(accesstoken).GetAwaiter().GetResult();
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task JwksAsymmetricRS256_PassingValidEarlyExipreAccessToken_ReturnNull()
    {
        var tokenResult = await TokenGenerationFlow_EarlyExpirationFlow();
        Thread.Sleep(20000);
        var result = JwksTestHelper.ValidateToken(tokenResult.access_token).GetAwaiter().GetResult();
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task JwksAsymmetric_PassingInvalidIssueandAudienceToken_ReturnNull()
    {
        string clientName = hclCSES256AlgorithmClient,
            responseType = "code",
            scopes = "openid email profile phone offline_access",
            codeChallengeMethod = "plain";
        var tokenResult = await TokenGenerationFlow(clientName, responseType, scopes, codeChallengeMethod);
        var result = JwksTestHelper.ValidateToken(tokenResult.access_token, issuer, "clietapi").GetAwaiter()
            .GetResult();
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task JwksAsymmetricRS256_PassingValidEarlyExipreIdentityToken_ReturnNull()
    {
        var tokenResult = await TokenGenerationFlow_EarlyExpirationFlow();
        Thread.Sleep(20000);
        var result = JwksTestHelper.ValidateToken(tokenResult.id_token).GetAwaiter().GetResult();
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task JwksAsymmetricRS256_PassingInvalid_Id_token_ReturnResultNull()
    {
        string clientName = hclCSRS256AlgorithmClient,
            responseType = "code",
            scopes = "openid email profile phone offline_access",
            codeChallengeMethod = "plain";
        var tokenResult = await TokenGenerationFlow(clientName, responseType, scopes, codeChallengeMethod);
        var result = JwksTestHelper.ValidateToken(tokenResult.id_token + "zxe").GetAwaiter().GetResult();
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task JwksAsymmetric_PassingInvalidIssueandAudienceIdentity_ReturnNull()
    {
        string clientName = hclCSES256AlgorithmClient,
            responseType = "code",
            scopes = "openid email profile phone offline_access",
            codeChallengeMethod = "plain";
        var tokenResult = await TokenGenerationFlow(clientName, responseType, scopes, codeChallengeMethod);
        var result = JwksTestHelper.ValidateToken(tokenResult.id_token, issuer, "clietapi").GetAwaiter().GetResult();
        result.Should().BeNull();
    }

    private async Task<TokenResponseResultModel> TokenGenerationFlow_EarlyExpirationFlow()
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

    private async Task<TokenResponseResultModel> TokenGenerationFlow(string clientName, string responseType,
        string scopes, string codeChallengeMethod)
    {
        await LoginAsync(User);
        clientModel = await FetchClientDetails(clientName);
        clientModel.Should().NotBeNull();
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        FrontChannelClient.AllowAutoRedirect = false;

        var authcodeRequest = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            responseType,
            scopes,
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeVerifier,
            codeChallengeMethod: codeChallengeMethod,
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);

        var returnQuery = await FrontChannelClient.GetAsync(authcodeRequest);
        returnQuery.StatusCode.Should().Be(HttpStatusCode.Found);
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
}
