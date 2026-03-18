/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Net;
using FluentAssertions;
using IntegrationTests.Endpoint.Setup;
using TestApp.Helper.Endpoint;
using Xunit;
using HCL.CS.SF.Domain.Models.Endpoint;

namespace IntegrationTests.Endpoint.Session;

public class SessionEndpointTests : HCLCSSFFakeSetup
{
    private const string Category = "SessionEndpoint";
    private readonly string redirectUri = "http://127.0.0.1:63562/";
    private ClientsModel clientModel;

    //[Fact]
    //[Trait("Category", Category)]
    //public async Task signout_request_should_redirect_to_logout_page()
    //{
    //    var response = await FrontChannelClient.GetAsync(HCLCSSFFakeSetup.EndSessionEndpoint);

    //    //LogoutCalled.Should().BeTrue();
    //}

    [Fact]
    public async Task EndSessionEndPoint()
    {
        Initialize();
        await LoginAsync(User);
        clientModel = ClientHelper.CreateClientsModel();
        var nonce = Guid.NewGuid().ToString();
        var codeChallengeString =
            "test_codechallenge_which_is_greater_than_the_minimum_number_of_characters_required_that_is_43";


        FrontChannelClient.AllowAutoRedirect = true;
        var url = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code id_token",
            "openid email profile phone",
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeChallengeString,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        // Frontchannel GET authorize request -> authorize endpoint
        var returnQuery = await FrontChannelClient.GetAsync(url);
        // Parse the authorize request response and convert to AuthorizationResponseModel
        if (returnQuery.StatusCode == HttpStatusCode.OK)
        {
            var response = await returnQuery.ParseAuthorizeResponse();
            response.AccessToken.Should().BeNull();
            response.AccessTokenLifetime.Should().Be(0);
            response.IdentityToken.Should().NotBeNullOrEmpty();
            response.Code.Should().NotBeNullOrEmpty();
            response.Scope.Should().NotBeNullOrEmpty();
            response.SessionState.Should().NotBeNullOrEmpty();

            var logoutUrl = BaseUrl + LogoutUrl;

            // backchannel client
            var tokenClient = FrontChannelClient;
            var dict = CreateEndSessionRequest(
                response.IdentityToken,
                logoutUrl,
                response.SessionState);

            // Back channel post token end point.
            var endSessionQuery = await tokenClient.PostAsync(EndSessionEndpoint, new FormUrlEncodedContent(dict));

            // Parse the token response and convert to TokenResponseModel
            LogoutPageCalled.Should().BeTrue();
            LogoutRequest.Should().NotBeNull();
            LogoutRequest.ClientId.Should().Be(clientModel.ClientId);
            LogoutRequest.PostLogoutRedirectUri.Should().Be(logoutUrl);
        }
        else
        {
            var errorResponse = await returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
            errorResponse.IsError.Should().Be(true);
            errorResponse.ErrorCode.Should().NotBeNullOrEmpty();
            errorResponse.ErrorDescription.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task EndSessionEndPointCallback()
    {
        Initialize();
        await LoginAsync(User);
        clientModel = ClientHelper.CreateClientsModel();
        var nonce = Guid.NewGuid().ToString();
        var codeChallengeString =
            "test_codechallenge_which_is_greater_than_the_minimum_number_of_characters_required_that_is_43";
        FrontChannelClient.AllowAutoRedirect = true;
        var url = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code id_token",
            "openid email profile phone",
            responseMode: "form_post",
            prompt: "none",
            codeChallenge: codeChallengeString,
            codeChallengeMethod: "plain",
            maxAge: "60",
            redirectUri: redirectUri,
            nonce: nonce);
        // Frontchannel GET authorize request -> authorize endpoint
        var returnQuery = await FrontChannelClient.GetAsync(url);
        // Parse the authorize request response and convert to AuthorizationResponseModel
        if (returnQuery.StatusCode == HttpStatusCode.OK)
        {
            var response = await returnQuery.ParseAuthorizeResponse();
            response.AccessToken.Should().BeNull();
            response.AccessTokenLifetime.Should().Be(0);
            response.IdentityToken.Should().NotBeNullOrEmpty();
            response.Code.Should().NotBeNullOrEmpty();
            response.Scope.Should().NotBeNullOrEmpty();
            response.SessionState.Should().NotBeNullOrEmpty();

            var logoutUrl = BaseUrl + LogoutUrl;

            // backchannel client
            var tokenClient = FrontChannelClient;
            var dict = CreateEndSessionRequest(
                response.IdentityToken,
                logoutUrl,
                response.SessionState);

            // Back channel post token end point.
            var endSessionQuery = await tokenClient.PostAsync(EndSessionEndpoint, new FormUrlEncodedContent(dict));

            // Parse the token response and convert to TokenResponseModel
            LogoutPageCalled.Should().BeTrue();
            LogoutRequest.Should().NotBeNull();
            LogoutRequest.ClientId.Should().Be(clientModel.ClientId);
            LogoutRequest.PostLogoutRedirectUri.Should().Be(logoutUrl);
        }
        else
        {
            var errorResponse = await returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
            errorResponse.IsError.Should().Be(true);
            errorResponse.ErrorCode.Should().NotBeNullOrEmpty();
            errorResponse.ErrorDescription.Should().NotBeNullOrEmpty();
        }
    }
}
