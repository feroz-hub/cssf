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
//using System.Collections.Generic;
//using System.Net;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Threading.Tasks;
//using FluentAssertions;
//using HCL.CS.SF.Domain.Constants;
//using IntegrationTests.ApiDomainModel;
//using HCL.CS.SF.Service.Implementation;
//using HCL.CS.SF.TestApp.Helpers;
//using Newtonsoft.Json;
//using Xunit;

//namespace IntegrationTests.MiddlewareRoutes
//{
//    /// <summary>
//    /// Service to check middleware route for user account service.
//    /// </summary>
//    public class UserAccountMiddlewareRouteTests : HCLCSSFFakeSetup
//    {
//        private ClientsModel clientModel;
//        private string positiveCaseClientName = "HCL.CS.SF Plain PKCE Client";
//        private readonly string hCLCSS256ClientName = "HCL.CS.SF S256 Client";
//        private readonly Random random;

//        /// <summary>
//        /// Initializes a new instance of the <see cref="UserAccountProxyTests"/> class.
//        /// </summary>
//        public UserAccountMiddlewareRouteTests()
//        {
//            random = new Random();
//        }

//        /// <summary>
//        /// Lock user via middleware.
//        /// </summary>
//        /// <returns> <see cref="LockUser_ValidToken_ReturnSuccess"/> representing the asynchronous unit test.</returns>
//        [Fact]
//        public async Task LockUser_ReturnSuccess()
//        {
//            TokenResponseResultModel token = await GetAccessToken();

//            // Call lock user middleware
//            Guid userId = new Guid("B18364C2-E518-45F1-FE85-08DA13A1C54E");
//            FrontChannelClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
//            var url = HCLCSSFFakeSetup.BaseUrl + ApiRoutePathConstants.LockUserPath;
//            var response = await FrontChannelClient.PostAsync(
//                url,
//                new StringContent(JsonConvert.SerializeObject(userId), Encoding.UTF8, "application/json"));
//            var lockUserResopnse = await response.Content.ReadAsStringAsync();
//            var frameworkResult = JsonConvert.DeserializeObject<ApiDomainModel.FrameworkResult>(lockUserResopnse);
//            frameworkResult.Should().BeOfType<ApiDomainModel.FrameworkResult>();
//            frameworkResult.Status.Should().Be(ApiDomainModel.ResultStatus.Success);
//        }

//        /// <summary>
//        /// Lock user via middleware.
//        /// </summary>
//        /// <returns> <see cref="LockUser_ValidToken_ReturnSuccess"/> representing the asynchronous unit test.</returns>
//        [Fact]
//        public async Task LockUser_WithEndDate_ReturnSuccess()
//        {
//            var data = new
//            {
//                user_id = "744EDDC4-B287-4BF7-CE20-08DA11479DA6",
//                end_date = DateTime.UtcNow,
//            };

//            TokenResponseResultModel token = await GetAccessToken();
//            // Call lock user middleware
//            FrontChannelClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
//            var url = HCLCSSFFakeSetup.BaseUrl + ApiRoutePathConstants.LockUserWithEndDatePath;
//            var response = await FrontChannelClient.PostAsync(
//                url,
//                new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));
//            var lockUserResopnse = await response.Content.ReadAsStringAsync();
//            var frameworkResult = JsonConvert.DeserializeObject<ApiDomainModel.FrameworkResult>(lockUserResopnse);
//            //frameworkResult.Should().BeOfType<FrameworkResult>();
//            //frameworkResult.Status.Should().Be(ResultStatus.Success);
//        }

//        /// <summary>
//        /// Lock user via middleware.
//        /// </summary>
//        /// <returns> <see cref="LockUser_ValidToken_ReturnSuccess"/> representing the asynchronous unit test.</returns>
//        [Fact]
//        public async Task Add_ApiResource_ReturnSuccess()
//        {
//            var apiResourcesModelInput = ApiResourceHelper.CreateApiResourceModel();
//            apiResourcesModelInput.Name = string.Concat("AlphaClientOne", "_", random.Next().ToString());

//            TokenResponseResultModel token = await GetAccessToken();
//            // Call lock user middleware
//            FrontChannelClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
//            var url = HCLCSSFFakeSetup.BaseUrl + ApiRoutePathConstants.AddApiResource;
//            var response = await FrontChannelClient.PostAsync(
//                url,
//                new StringContent(JsonConvert.SerializeObject(apiResourcesModelInput), Encoding.UTF8, "application/json"));
//            var lockUserResopnse = await response.Content.ReadAsStringAsync();
//            //var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(lockUserResopnse);
//            //frameworkResult.Should().BeOfType<FrameworkResult>();
//            //frameworkResult.Status.Should().Be(ResultStatus.Success);
//        }

//        [Fact]
//        public async Task Get_SecurityToken_By_Client_ReturnSuccess()
//        {
//            List<string> clientlist = new List<string> { "9i6DL8zPzIgAVwZOvtG+/0saFcptwi1kiVv77RmBW/k=", "ZqWJUx2H09BegYdhYCfNqyaRR/5WKdPW7PYQYC8jG3U=" };
//            Domain.Models.PagingModel paging = new Domain.Models.PagingModel { ItemsPerPage = 5, CurrentPage = 0, TotalDisplayPages = 5 };

//            var data = new
//            {
//                clients_list = clientlist,
//                paging_model = paging,
//            };

//            TokenResponseResultModel token = await GetAccessToken();
//            // Call lock user middleware
//            FrontChannelClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
//            var url = HCLCSSFFakeSetup.BaseUrl + ApiRoutePathConstants.GetActiveSecurityTokensByClientIds;
//            var httpresponse = await FrontChannelClient.PostAsync(
//                url,
//                new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));
//            var response = await httpresponse.Content.ReadAsStringAsync();
//            var result = JsonConvert.DeserializeObject<List<string>>(response);
//        }

//        [Fact]
//        public async Task Get_SecurityToken_By_User_ReturnSuccess()
//        {
//            Domain.Models.PagingModel paging = new Domain.Models.PagingModel { ItemsPerPage = 5, CurrentPage = 0, TotalDisplayPages = 5 };
//            List<string> userlist = new List<string> { "F31FE17E-EDDE-4358-927B-08DA430E2495", "ACE1B668-4637-462F-969B-D46DAA0A2826" };
//            var data = new
//            {
//                user_list = userlist,
//                paging_model = paging,
//            };

//            TokenResponseResultModel token = await GetAccessToken();
//            // Call lock user middleware
//            FrontChannelClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
//            var url = HCLCSSFFakeSetup.BaseUrl + ApiRoutePathConstants.GetActiveSecurityTokensByUserIds;
//            var httpresponse = await FrontChannelClient.PostAsync(
//                url,
//                new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));
//            var response = await httpresponse.Content.ReadAsStringAsync();
//            var result = JsonConvert.DeserializeObject<List<string>>(response);
//        }

//        [Fact]
//        public async Task Get_SecurityToken_By_Dates_ReturnSuccess()
//        {
//            Domain.Models.PagingModel paging = new Domain.Models.PagingModel { ItemsPerPage = 5, CurrentPage = 0, TotalDisplayPages = 5 };
//            var data = new
//            {
//                from_date = DateTime.UtcNow.AddDays(-20),
//                to_date = DateTime.UtcNow,
//                paging_model = paging,
//            };

//            TokenResponseResultModel token = await GetAccessToken();
//            // Call lock user middleware
//            FrontChannelClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
//            var url = HCLCSSFFakeSetup.BaseUrl + ApiRoutePathConstants.GetActiveSecurityTokensBetweenDates;
//            var httpresponse = await FrontChannelClient.PostAsync(
//                url,
//                new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));
//            var response = await httpresponse.Content.ReadAsStringAsync();
//            var result = JsonConvert.DeserializeObject<List<string>>(response);
//        }

//        [Fact]
//        public async Task Get_SecurityToken_By_All_Dates_ReturnSuccess()
//        {
//            Domain.Models.PagingModel paging = new Domain.Models.PagingModel { ItemsPerPage = 5, CurrentPage = 0, TotalDisplayPages = 5 };
//            var data = new
//            {
//                from_date = DateTime.UtcNow.AddDays(-20),
//                to_date = DateTime.UtcNow,
//                paging_model = paging,
//            };

//            TokenResponseResultModel token = await GetAccessToken();
//            // Call lock user middleware
//            FrontChannelClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
//            var url = HCLCSSFFakeSetup.BaseUrl + ApiRoutePathConstants.GetAllSecurityTokensBetweenDates;
//            var httpresponse = await FrontChannelClient.PostAsync(
//                url,
//                new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));
//            var response = await httpresponse.Content.ReadAsStringAsync();
//            var result = JsonConvert.DeserializeObject<List<string>>(response);
//        }

//        private async Task<TokenResponseResultModel> GetAccessToken()
//        {
//            // Login User.
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
//                           scope: "openid HCL.CS.SF.client offline_access HCL.CS.SF.role HCL.CS.SF.user HCL.CS.SF.apiresource",
//                           responseMode: "query",
//                           prompt: "none",
//                           codeChallenge: codeChallengeString, // Codeverifier
//                           codeChallengeMethod: "S256", // Plain
//                           maxAge: "60",
//                           redirectUri: "https://localhost:5001/index.html",
//                           nonce: nonce);
//            var returnQuery = await FrontChannelClient.GetAsync(url);

//            // Token endpoint calls.
//            var response = returnQuery.Headers.Location.ToString().ParseQueryString();
//            var dict = CreateTokenRequest(
//            clientId: clientModel.ClientId,
//            clientSecret: clientModel.ClientSecret,
//            code: response.Code,
//            redirectUri: "https://localhost:5001/index.html",
//            grantType: OpenIdConstants.GrantTypes.AuthorizationCode,
//            codeVerifier: codeVerifier); // Code Challenge
//            var tokenClient = BackChannelClient;
//            var tokenResponse = await tokenClient.PostAsync(HCLCSSFFakeSetup.TokenEndpoint, new FormUrlEncodedContent(dict));
//            var tokenResult = await tokenResponse.ParseTokenResponseResult();
//            tokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
//            tokenResult.access_token.Should().NotBeNullOrEmpty();
//            tokenResult.id_token.Should().NotBeNullOrEmpty();
//            tokenResult.token_type.Should().NotBeNullOrEmpty();
//            tokenResult.refresh_token.Should().NotBeNullOrEmpty();
//            return tokenResult;
//        }
//    }
//}



