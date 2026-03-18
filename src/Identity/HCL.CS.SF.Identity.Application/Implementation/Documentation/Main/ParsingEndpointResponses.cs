/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

/**
* @page ParsingEndpointResponses  Parsing Endpoint responses
* <p><strong>Authorize End point Response parsing Sample code</strong></p>
* \code
        public static async Task<AuthorizationResponseModel> ParseAuthorizeResponse(this HttpResponseMessage authResponse)
        {
            AuthorizationResponseModel authModel = new AuthorizationResponseModel();
            authModel.Request = new ValidatedAuthorizeRequestModel();
            var content = await authResponse.Content.ReadAsStringAsync();
            HtmlAgilityPack.HtmlDocument docToParse = new HtmlAgilityPack.HtmlDocument();
            docToParse.LoadHtml(content);
            foreach (HtmlNode node in docToParse.DocumentNode.SelectNodes("//form//input"))
            {
                if (node.Attributes["name"].Value == "code")
                {
                    authModel.Code = node.Attributes["value"].Value;
                }
                if (node.Attributes["name"].Value == "id_token")
                {
                    authModel.IdentityToken = node.Attributes["value"].Value;
                }
                if (node.Attributes["name"].Value == "session_state")
                {
                    authModel.SessionState = node.Attributes["value"].Value;
                }
                if (node.Attributes["name"].Value == "scope")
                {
                    authModel.Scope = node.Attributes["value"].Value;
                }
                if (node.Attributes["name"].Value == "access_token")
                {
                    authModel.AccessToken = node.Attributes["value"].Value;
                }
                if (node.Attributes["name"].Value == "refresh_token")
                {
                    authModel.RefreshToken = node.Attributes["value"].Value;
                }
                if (node.Attributes["name"].Value == "state")
                {
                    authModel.Request.State = node.Attributes["value"].Value;
                }
            }
            return authModel;
        }
* \endcode
* <p><strong>Token End point Response parsing Sample code</strong></p>
* \code
        public static async Task<TokenResponseResultModel> ParseTokenResponseResult(this HttpResponseMessage tokenResponse)
        {
            var content = await tokenResponse.Content.ReadAsStringAsync();
            var tokenResult = JsonConvert.DeserializeObject<TokenResponseResultModel>(content);
            return tokenResult;
        }
* \endcode
* <p><strong>Introspection End point Response parsing Sample code</strong></p>
* \code
        public static async Task<IntrospectionResponseModel> ParseIntrospectionResponse(this HttpResponseMessage introspecResponse)
        {
            var content = await introspecResponse.Content.ReadAsStringAsync();
            var introspecResult = JsonConvert.DeserializeObject<IntrospectionResponseModel>(content);
            return introspecResult;
        }
* \endcode
* <p><strong>Token Revocation End point Response parsing Sample code</strong></p>
* \code
        Uri myUri = new Uri(uri);
        string fragmentstring = myUri.Fragment;
        var parameters = HttpUtility.ParseQueryString(fragmentstring);
        var response = new AuthorizationResponseModel()
        {
            Code = parameters["code"],
            AccessToken = parameters["access_token"],
            RefreshToken = parameters["refresh_token"],
            Scope = parameters["scope"],
            IdentityToken = parameters["id_token"],
            AccessTokenLifetime = string.IsNullOrWhiteSpace(parameters["AccessTokenLifetime"]) ? 0 : Convert.ToInt32(parameters["AccessTokenLifetime"]),
            SessionState = parameters["session_state"],
            IsError = Convert.ToBoolean(parameters["IsError"]),
        };
        return response;
* \endcode
* <p><strong>UserInfo End point Response parsing Sample code</strong></p>
* \code
        public static ErrorResponseModel ParseUserInfoErrorResponse(this HttpResponseMessage userInfoResponse)
        {
            var authorizationHeader = userInfoResponse.Headers.WwwAuthenticate;
            var authHeader = authorizationHeader.ToString();
            string result = authHeader.Replace("\"", string.Empty);
            var headers = result.Split(',');
            var errorResponse = new ErrorResponseModel();
            if (headers[0] == OpenIdConstants.AuthenticationSchemes.AuthorizationHeaderBearer)
            {
                errorResponse.ErrorCode = headers[1].Split('=')[1];
                errorResponse.ErrorDescription = headers[2].Split('=')[1];
            }
            return errorResponse;
        }
* \endcode
* <p><strong>Logout Querystring parsing Sample code</strong></p>
* \code
        public static string ParseLogoutQueryStringAsync(this string uri)
        {
            Uri myUri = new Uri(uri);
            string querystring = myUri.Query;
            var parameters = HttpUtility.ParseQueryString(querystring);
            var logoutId = parameters["logoutId"];
            return logoutId.ToString();
        }
* \endcode
* <p><strong>Logout Query message parsing Sample code</strong></p>
* \code
        public static async Task<LogoutMessageModel> ParseLogoutQueryMessageAsync(this string logoutId)
        {
            var response = await DataProtectionExtension.UnProtectDataAsync<LogoutMessageModel>(logoutId);
            return response;
        }
* \endcode
*/


