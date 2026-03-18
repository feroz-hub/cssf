/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

//using System;
//using System.Net;
//using System.Net.Http;
//using System.Threading.Tasks;
//using FluentAssertions;
//using HCL.CS.SF.Domain.Constants;
//using IntegrationTests.ApiDomainModel;
//using HCL.CS.SF.Service.Implementation;

//namespace IntegrationTests.MiddlewareRoutes
//{
//    /// <summary>
//    ///  Middleware Route Setup.
//    /// </summary>
//    public class MiddlewareRouteSetup : HCLCSSFFakeSetup
//    {
//        public ClientsModel clientModel;
//        public UserModel userModel;
//        public readonly string userName = "JacobIsmail";
//        public TokenResponseResultModel tokenResponseResultModel;
//        private readonly IntegrationTestData testData;
//        private string positiveCaseClientName = "HCL.CS.SF Plain PKCE Client";
//        private readonly string hCLCSS256ClientName = "HCL.CS.SF S256 Client";

//        /// <summary>
//        /// Initializes a new instance of the <see cref="MiddlewareRouteSetup"/> class.
//        /// </summary>
//        public MiddlewareRouteSetup()
//        {
//            testData = new IntegrationTestData();
//        }

//        /// <summary>
//        /// Get Access Token.
//        /// </summary>
//        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
//        public async Task<TokenResponseResultModel> GetAccessToken()
//        {
//            // Login User.
//            UserName = userName;
//            await LoginAsync(User);
//            positiveCaseClientName = hCLCSS256ClientName;
//            clientModel = await FetchClientDetails(positiveCaseClientName);
//            clientModel.Should().NotBeNull();

//            // Authorize endpoint calls.
//            var nonce = Guid.NewGuid().ToString();
//            var codeVerifier = EncryptionExtension.RandomString(32);
//            var codeChallengeString = codeVerifier.GenerateCodeChallenge();
//            FrontChannelClient.AllowAutoRedirect = false;
//            var url = CreateAuthorizeRequestUrl(
//                           clientId: clientModel.ClientId,
//                           responseType: "code",
//                           scope: "openid HCL.CS.SF.Client offline_access HCL.CS.SF.Role HCL.CS.SF.User HCL.CS.SF.ApiResource",
//                           responseMode: "query",
//                           prompt: "none",
//                           codeChallenge: codeChallengeString, // Codeverifier
//                           codeChallengeMethod: "S256", // Plain
//                           maxAge: "60",
//                           redirectUri: "https://localhost:44300/index.html",
//                           nonce: nonce);
//            var returnQuery = await FrontChannelClient.GetAsync(url);

//            // Token endpoint calls.
//            var response = returnQuery.Headers.Location.ToString().ParseQueryString();
//            var dict = CreateTokenRequest(
//            clientId: clientModel.ClientId,
//            clientSecret: clientModel.ClientSecret,
//            code: response.Code,
//            redirectUri: "https://localhost:44300/index.html",
//            grantType: OpenIdConstants.GrantTypes.AuthorizationCode,
//            codeVerifier: codeVerifier); // Code Challenge
//            var tokenClient = BackChannelClient;
//            var tokenResponse = await tokenClient.PostAsync(HCLCSSFFakeSetup.TokenEndpoint, new FormUrlEncodedContent(dict));
//            tokenResponseResultModel = await tokenResponse.ParseTokenResponseResult();
//            tokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
//            tokenResponseResultModel.access_token.Should().NotBeNullOrEmpty();
//            tokenResponseResultModel.id_token.Should().NotBeNullOrEmpty();
//            tokenResponseResultModel.token_type.Should().NotBeNullOrEmpty();
//            tokenResponseResultModel.refresh_token.Should().NotBeNullOrEmpty();
//            return tokenResponseResultModel;
//        }

//        /// <summary>
//        /// GetUserModel.
//        /// </summary>
//        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
//        //public Task<UserModel> GetUserModel()
//        //{
//        //    UserName = userName;
//        //    UserModel result = userModel = User;
//        //    return Task.FromResult(result);
//        //}
//    }
//}



