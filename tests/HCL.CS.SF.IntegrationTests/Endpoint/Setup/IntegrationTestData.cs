/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

// * Copyright (c) 2021 HCL.CS.SF CORPORATION.
// * All rights reserved. HCL.CS.SF source code is an unpublished work and the use of a copyright notice does not imply otherwise.
// * This source code contains confidential, trade secret material of HCL.CS.SF. Any attempt or participation in deciphering,
// * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
// * HCL.CS.SF is obtained. This is proprietary and confidential to HCL.CS.SF.
// */

//using System;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Threading.Tasks;
//using FluentAssertions;
//using HCL.CS.SF.Domain.Constants;
//using IntegrationTests.ApiDomainModel;
//using IntegrationTests.MiddlewareRoutes;
//using HCL.CS.SF.Service.Implementation;
//using HCL.CS.SF.Service.Interfaces;
//using Microsoft.Extensions.DependencyInjection;
//using Newtonsoft.Json;

//namespace IntegrationTests
//{
//    /// <summary>
//    /// Integration Test Data.
//    /// </summary>
//    public class IntegrationTestData : HCLCSSFFakeSetup
//    {
//        private ClientsModel clientModel;
//        public readonly string userName = "JacobIsmail";
//            //"BobUser";
//        public TokenResponseResultModel tokenResponseResultModel;
//        private readonly IntegrationTestData testData;
//        private string positiveCaseClientName = "HCL.CS.SF Plain PKCE Client";
//        private readonly string hCLCSS256ClientName = "HCL.CS.SF S256 Client";

//        /// <summary>
//        /// Initializes a new instance of the <see cref="IntegrationTestData"/> class.
//        /// Integration Test Data.
//        /// </summary>
//        /// <param name="clientName">Client Id.</param>
//        /// <returns>Return Cleint Model.</returns>
//        public async Task<ClientsModel> FetchClientDetails(string clientName)
//        {
//            var client = clientMasterData;

//            foreach (var item in client)
//            {
//                if (item.Key.Contains(clientName))
//                {
//                    ClientsModel clientsModel = new ClientsModel();
//                    string clientId = item.Value.ClientId.ToString();
//                    string clientSecret = item.Value.ClientSecret.ToString();
//                    clientsModel.ClientId = clientId;
//                    clientsModel.ClientSecret = clientSecret;
//                    clientModel = clientsModel;
//                }
//            }

//            return clientModel;
//        }

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

//    }
//}



