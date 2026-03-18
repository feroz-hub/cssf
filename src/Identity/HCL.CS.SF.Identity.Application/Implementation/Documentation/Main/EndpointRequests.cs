/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

/**
 * @page EndpointRequests Endpoint requests
 * <p><strong>Authorize End point Request</strong></p>
 * \code
  public string CreateAuthorizeRequestUrl(
                string clientId = null,
                string responseType = null,
                string scope = null,
                string redirectUri = null,
                string state = null,
                string nonce = null,
                string prompt = null,
                string maxAge = null,
                string loginHint = null,
                string acrValues = null,
                string responseMode = null,
                string codeChallenge = null,
                string codeChallengeMethod = null)
        {
            var dict = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(clientId))
            {
                dict.Add(OpenIdConstants.AuthorizeRequest.ClientId, clientId);
            }

            if (!string.IsNullOrEmpty(responseType))
            {
                dict.Add(OpenIdConstants.AuthorizeRequest.ResponseType, responseType);
            }

            if (!string.IsNullOrEmpty(scope))
            {
                dict.Add(OpenIdConstants.AuthorizeRequest.Scope, scope);
            }

            if (!string.IsNullOrEmpty(redirectUri))
            {
                dict.Add(OpenIdConstants.AuthorizeRequest.RedirectUri, redirectUri);
            }

            if (!string.IsNullOrEmpty(state))
            {
                dict.Add(OpenIdConstants.AuthorizeRequest.State, state);
            }

            if (!string.IsNullOrEmpty(nonce))
            {
                dict.Add(OpenIdConstants.AuthorizeRequest.Nonce, nonce);
            }

            if (!string.IsNullOrEmpty(nonce))
            {
                dict.Add(OpenIdConstants.AuthorizeRequest.Prompt, prompt);
            }

            if (!string.IsNullOrEmpty(nonce))
            {
                dict.Add(OpenIdConstants.AuthorizeRequest.MaxAge, maxAge);
            }

            if (!string.IsNullOrEmpty(loginHint))
            {
                dict.Add("login_hint", loginHint);
            }

            if (!string.IsNullOrEmpty(acrValues))
            {
                dict.Add("acr_values", acrValues);
            }

            if (!string.IsNullOrEmpty(responseMode))
            {
                dict.Add(OpenIdConstants.AuthorizeRequest.ResponseMode, responseMode);
            }

            if (!string.IsNullOrEmpty(codeChallenge))
            {
                dict.Add(OpenIdConstants.AuthorizeRequest.CodeChallenge, codeChallenge);
            }

            if (!string.IsNullOrEmpty(codeChallengeMethod))
            {
                dict.Add(OpenIdConstants.AuthorizeRequest.CodeChallengeMethod, codeChallengeMethod);
            }

            var url = AuthorizeEndpoint.AddQueryString(dict);
            return url;
        }
 * \endcode
 * <p><strong>Authorize Endpoint Request Sample Code :</strong></p>
 * \code
    var request = CreateAuthorizeRequestUrl(clientId: <clientId>, responseType: <Code>, scope: <scope>,
                responseMode: <responseMode>, prompt: <prompt>, codeChallenge: <codeVerifier>, codeChallengeMethod: <plain/SHA256>,
                maxAge: <maxAge>, redirectUri: <redirectUri>, nonce: <nonce>);

    var authorizeResult = await httpclient.GetAsync(url);
 * \endcode
 * <p><strong>Token endpoint request:</strong></p>
 * \code
  public Dictionary<string, string> CreateTokenRequest(
            string clientId = null,
            string clientSecret = null,
            string code = null,
            string redirectUri = null,
            string grantType = null,
            string codeVerifier = null,
            string userName = null,
            string password = null,
            string scope = null,
            string refreshToken = null,
            string clientAssertionType = null,
            string clientAssertion = null)
        {
            var dict = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(clientId))
            {
                dict.Add(OpenIdConstants.TokenRequest.ClientId, clientId);
            }

            if (!string.IsNullOrEmpty(clientSecret))
            {
                dict.Add(OpenIdConstants.TokenRequest.ClientSecret, clientSecret);
            }

            if (!string.IsNullOrEmpty(code))
            {
                dict.Add(OpenIdConstants.TokenRequest.Code, code);
            }

            if (!string.IsNullOrEmpty(redirectUri))
            {
                dict.Add(OpenIdConstants.TokenRequest.RedirectUri, redirectUri);
            }

            if (!string.IsNullOrEmpty(grantType))
            {
                dict.Add(OpenIdConstants.TokenRequest.GrantType, grantType);
            }

            if (!string.IsNullOrEmpty(codeVerifier))
            {
                dict.Add(OpenIdConstants.TokenRequest.CodeVerifier, codeVerifier);
            }

            if (!string.IsNullOrEmpty(userName))
            {
                dict.Add(OpenIdConstants.TokenRequest.UserName, userName);
            }

            if (!string.IsNullOrEmpty(password))
            {
                dict.Add(OpenIdConstants.TokenRequest.Password, password);
            }

            if (!string.IsNullOrEmpty(scope))
            {
                dict.Add(OpenIdConstants.TokenRequest.Scope, scope);
            }

            if (!string.IsNullOrEmpty(refreshToken))
            {
                dict.Add(OpenIdConstants.TokenRequest.RefreshToken, refreshToken);
            }

            if (!string.IsNullOrEmpty(clientAssertionType))
            {
                dict.Add(OpenIdConstants.TokenRequest.ClientAssertionType, clientAssertionType);
            }

            if (!string.IsNullOrEmpty(clientAssertion))
            {
                dict.Add(OpenIdConstants.TokenRequest.ClientAssertion, clientAssertion);
            }

            dict.Add("Content-Type", "application/x-www-form-urlencoded");
            return dict;
        }
 * \endcode
 * <para><strong>Token Endpoint request - AuthorizationCode Grant Type Flow, Basic authentication</strong></para>
 * \code
        httpclient.SetBasicAuthentication(clientModel.ClientId, clientModel.ClientSecret);
        var request = CreateTokenRequest(code: <authorizationCode>, redirectUri: <redirectUri>, grantType: <GrantTypes.AuthorizationCode>, codeVerifier: <codeVerifier>);
        var tokenResult = await httpclient.PostAsync(<TokenEndpointUrl>, new FormUrlEncodedContent(request));
 * \endcode
 * <para><strong>Token Endpoint request - Client Credentials flow</strong></para>
 * \code
        var request = CreateTokenRequest(clientId: <clientId>, clientSecret: <ClientSecret>, grantType : <GrantTypes.ClientCredentials>,
        redirectUri: <redirectUri>, scope: <scope>, userName: <userName>);
        var tokenResponse = await httpclient.PostAsync(<TokenEndpointURL>, new FormUrlEncodedContent(request));
 * \endcode
 * <para><strong>Token Endpoint request - Resource Owner Password Flow</strong></para>
 * \code
        var request = CreateTokenRequest(clientId: <clientId>, clientSecret: <ClientSecret>, grantType :<GrantTypes.Password>,
        userName: <userName>, password: <passWord>, redirectUri: <redirectUri>, scope: <scope>);
        var tokenResponse = await httpclient.PostAsync(<TokenEndpointUrl>, new FormUrlEncodedContent(request));
 * \endcode
 * <para><strong>Token Endpoint request - Refresh Token Flow</strong></para>
 * \code
        var request = frameworkFSetup.CreateTokenRequest(clientId: <clientId>, clientSecret: <ClientSecret>, grantType :<GrantTypes.RefreshToken>,
        refreshToken : <refreshToken>, redirectUri: <redirectUri>);
        var tokenResponse = await httpclient.PostAsync(<TokenEndpointUrl>, new FormUrlEncodedContent(request));
 * \endcode
 * <p><strong>Introspection endpoint request:</strong></p>
 * \code
 public Dictionary<string, string> CreateIntroSpecRequest(
            string clientId = null,
            string clientSecret = null,
            string token = null,
            string tokenTypeHint = null,
            string scope = null
           )
        {
            var dict = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(clientId))
            {
                dict.Add(OpenIdConstants.TokenRequest.ClientId, clientId);
            }

            if (!string.IsNullOrEmpty(clientSecret))
            {
                dict.Add(OpenIdConstants.TokenRequest.ClientSecret, clientSecret);
            }

            if (!string.IsNullOrEmpty(token))
            {
                dict.Add(OpenIdConstants.IntrospectionRequest.Token, token);
            }

            if (!string.IsNullOrEmpty(tokenTypeHint))
            {
                dict.Add(OpenIdConstants.IntrospectionRequest.TokenHintType, tokenTypeHint);
            }

            if (!string.IsNullOrEmpty(scope))
            {
                dict.Add(OpenIdConstants.IntrospectionRequest.Scope, scope);
            }

            dict.Add("Content-Type", "application/x-www-form-urlencoded");
            return dict;
        }
 * \endcode
 * <p><strong>Introspection Endpoint request - Sample code </strong></p>
 * \code
        var request = CreateIntroSpecRequest(token : <access_token>, clientId: <clientId>, clientSecret: <clientSecret>);
        var response = await httpclient.PostAsync(<IntrospectionEndpointUrl>, new FormUrlEncodedContent(request));
        var introspectionResult = await response.ParseIntrospectionResponse();
 * \endcode
 * <p><strong>Token revocation endpoint request:</strong></p>
 * \code
 public Dictionary<string, string> CreateRevocationRequest(
          string clientId = null,
          string clientSecret = null,
          string token = null,
          string tokentypehint = null)
        {
            var dict = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(clientId))
            {
                dict.Add(OpenIdConstants.TokenRequest.ClientId, clientId);
            }

            if (!string.IsNullOrEmpty(clientSecret))
            {
                dict.Add(OpenIdConstants.TokenRequest.ClientSecret, clientSecret);
            }

            if (!string.IsNullOrEmpty(token))
            {
                dict.Add(OpenIdConstants.RevocationRequest.Token, token);
            }

            if (!string.IsNullOrEmpty(tokentypehint))
            {
                dict.Add(OpenIdConstants.RevocationRequest.TokenTypeHint, tokentypehint);
            }

            dict.Add("Content-Type", "application/x-www-form-urlencoded");
            return dict;
        }
 * \endcode
 * <p><strong>Token revocation Endpoint request - Sample code </strong></p>
 * \code
    var request = CreateRevocationRequest(clientId: <clientId>, clientSecret: <ClientSecret>, token : <refresh_token/access_token>,
    tokentypehint: <TokenType.RefreshToken/TokenType.AccessToken>);
    var revocationResponse = await tokenClient.PostAsync(<TokenRevocationEndpointUrl>, new FormUrlEncodedContent(request));
 * \endcode
 * <p><strong>End session endpoint request:</strong></p>
 * \code
 public Dictionary<string, string> CreateEndSessionRequest(
          string id_token_hint = null,
          string post_logout_redirect_uri = null,
          string state = null
        )
        {
            var dict = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(id_token_hint))
            {
                dict.Add(OpenIdConstants.EndSessionRequest.IdTokenHint, id_token_hint);
            }

            if (!string.IsNullOrEmpty(post_logout_redirect_uri))
            {
                dict.Add(OpenIdConstants.EndSessionRequest.PostLogoutRedirectUri, post_logout_redirect_uri);
            }

            if (!string.IsNullOrEmpty(state))
            {
                dict.Add(OpenIdConstants.EndSessionRequest.State, state);
            }

            dict.Add("Content-Type", "application/x-www-form-urlencoded");
            return dict;
        }
 * \endcode
 * <p><strong>End session Endpoint request - Sample code </strong></p>
 * \code
        var request = CreateEndSessionRequest(id_token_hint: <id_token_hint>, post_logout_redirect_uri: <post_logout_redirect_uri>, state: <state>);
        var result = await httpclient.PostAsync(<EndSessionEndpointUrl>, new FormUrlEncodedContent(request));
 * \endcode
 * <p><strong>User Info Endpoint request - Sample code </strong></p>
 * \code
        httpclient.SetAccessTokenAuthorizationHeader(<access_token>);
        var userInfoResponse = await httpclient.GetAsync(<UserInfoEndpointUrl>);
        var userInfoResult = await userInfoResponse.ParseUserInfoResponse();
 * \endcode
  * <p><strong>Discovery Endpoint request - Sample code </strong></p>
 * \code
        var result = await httpClient.GetAsync(< DiscoveryEndpointUrl>);
        var json = await result.Content.ReadAsStringAsync();
        var data = JObject.Parse(json);
 * \endcode
 */


