/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HtmlAgilityPack;
using IdentityModel;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using HCL.CS.SF.DemoClientWpfApp.Constants;
using HCL.CS.SF.DemoClientWpfApp.DomainModel;
using static HCL.CS.SF.DemoClientWpfApp.DomainModel.AllowedScopesParserModel;

namespace HCL.CS.SF.DemoClientWpfApp.Extension
{
    internal static class EndpointExtension 
    {
        static ApplicationParameters ApplicationConstants = new ApplicationParameters();

        internal static string CreateAuthorizeRequestUrl(
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
                dict.Add(AuthorizeRequest.ClientId, clientId);
            }

            if (!string.IsNullOrEmpty(responseType))
            {
                dict.Add(AuthorizeRequest.ResponseType, responseType);
            }

            if (!string.IsNullOrEmpty(scope))
            {
                dict.Add(AuthorizeRequest.Scope, scope);
            }

            if (!string.IsNullOrEmpty(redirectUri))
            {
                dict.Add(AuthorizeRequest.RedirectUri, redirectUri);
            }

            if (!string.IsNullOrEmpty(state))
            {
                dict.Add(AuthorizeRequest.State, state);
            }

            if (!string.IsNullOrEmpty(nonce))
            {
                dict.Add(AuthorizeRequest.Nonce, nonce);
            }

            if (!string.IsNullOrEmpty(nonce))
            {
                dict.Add(AuthorizeRequest.Prompt, prompt);
            }

            if (!string.IsNullOrEmpty(nonce))
            {
                dict.Add(AuthorizeRequest.MaxAge, maxAge);
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
                dict.Add(AuthorizeRequest.ResponseMode, responseMode);
            }

            if (!string.IsNullOrEmpty(codeChallenge))
            {
                dict.Add(AuthorizeRequest.CodeChallenge, codeChallenge);
            }

            if (!string.IsNullOrEmpty(codeChallengeMethod))
            {
                dict.Add(AuthorizeRequest.CodeChallengeMethod, codeChallengeMethod);
            }

            var url = ApplicationConstants.AuthorizeEndpoint.AddQueryString(dict);
            return url;
        }

        public static Dictionary<string, string> CreateTokenRequest(
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
                dict.Add(TokenRequest.ClientId, clientId);
            }

            if (!string.IsNullOrEmpty(clientSecret))
            {
                dict.Add(TokenRequest.ClientSecret, clientSecret);
            }

            if (!string.IsNullOrEmpty(code))
            {
                dict.Add(TokenRequest.Code, code);
            }

            if (!string.IsNullOrEmpty(redirectUri))
            {
                dict.Add(TokenRequest.RedirectUri, redirectUri);
            }

            if (!string.IsNullOrEmpty(grantType))
            {
                dict.Add(TokenRequest.GrantType, grantType);
            }

            if (!string.IsNullOrEmpty(codeVerifier))
            {
                dict.Add(TokenRequest.CodeVerifier, codeVerifier);
            }

            if (!string.IsNullOrEmpty(userName))
            {
                dict.Add(TokenRequest.UserName, userName);
            }

            if (!string.IsNullOrEmpty(password))
            {
                dict.Add(TokenRequest.Password, password);
            }

            if (!string.IsNullOrEmpty(scope))
            {
                dict.Add(TokenRequest.Scope, scope);
            }

            if (!string.IsNullOrEmpty(refreshToken))
            {
                dict.Add(TokenRequest.RefreshToken, refreshToken);
            }

            if (!string.IsNullOrEmpty(clientAssertionType))
            {
                dict.Add(TokenRequest.ClientAssertionType, clientAssertionType);
            }

            if (!string.IsNullOrEmpty(clientAssertion))
            {
                dict.Add(TokenRequest.ClientAssertion, clientAssertion);
            }

            dict.Add("Content-Type", "application/x-www-form-urlencoded");
            return dict;
        }

        public static string AddQueryString(this string uri, Dictionary<string, string> queryString)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            if (queryString == null)
            {
                throw new ArgumentNullException(nameof(queryString));
            }

            var anchorIndex = uri.IndexOf('#');
            var uriToBeAppended = uri;
            var anchorText = string.Empty;
            // If there is an anchor, then the query string must be inserted before its first occurance.
            if (anchorIndex != -1)
            {
                anchorText = uri.Substring(anchorIndex);
                uriToBeAppended = uri.Substring(0, anchorIndex);
            }

            var queryIndex = uriToBeAppended.IndexOf('?');
            var hasQuery = queryIndex != -1;

            var sb = new StringBuilder();
            sb.Append(uriToBeAppended);
            foreach (var parameter in queryString)
            {
                if (parameter.Value == null)
                {
                    continue;
                }

                sb.Append(hasQuery ? '&' : '?');
                sb.Append(UrlEncoder.Default.Encode(parameter.Key));
                sb.Append('=');
                sb.Append(UrlEncoder.Default.Encode(parameter.Value));
                hasQuery = true;
            }

            sb.Append(anchorText);
            return sb.ToString();
        }

        public static AuthorizationResponseModel ParseQueryString(this string uri)
        {
            Uri myUri = new Uri(uri);

            string querystring = myUri.Query;

            var parameters = HttpUtility.ParseQueryString(querystring);

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
        }

        public static async Task<AllowedScopesParserModel.AuthorizeErrorResponseModel> ParseErrorQueryStringAsync(this string uri)
        {
            Uri myUri = new Uri(uri);

            string querystring = myUri.Query;

            var parameters = HttpUtility.ParseQueryString(querystring);

            var errorId = parameters["errorId"];

            var response = DataProtectionExtension.UnProtectDataAsync<AllowedScopesParserModel.AuthorizeErrorResponseModel>(errorId).Result;

            return response;
        }
        public static async Task<TokenResponseResultModel> ParseTokenResponseResult(this HttpResponseMessage tokenResponse)
        {
            TokenResponseResultModel tokenResponseResultModel= new TokenResponseResultModel();
            var content = tokenResponse.Content.ReadAsStringAsync().Result;
            if (content.Contains("error"))
            {
                var tokenErrorResult = JsonConvert.DeserializeObject<ErrorResponseResultModel>(content);
                tokenResponseResultModel.ErrorResponseResult = tokenErrorResult;
            }
            else
            { 
            var tokenResult = JsonConvert.DeserializeObject<TokenResponseResultModel>(content);
                tokenResponseResultModel = tokenResult;
            }
            return tokenResponseResultModel;
        }
        internal static string RandomString(this int keyLength)
        {
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                byte[] randomBytes = new byte[keyLength];
                rngCryptoServiceProvider.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }

        internal static string GenerateCodeChallenge(this string codeverifier)
        {
            var codeChallengeBytes = Encoding.ASCII.GetBytes(codeverifier);
            var hashedBytes = codeChallengeBytes.Sha256();
            var codeChallengeString = hashedBytes.Encode();

            return codeChallengeString;
        }

        public static byte[] Sha256(this byte[] input)
        {
            if (input != null)
            {
                using var sha = SHA256.Create();
                return sha.ComputeHash(input);
            }

            return Array.Empty<byte>();
        }

        internal static string Encode(this byte[] arg)
        {
            var val = Convert.ToBase64String(arg); // Standard base64 encoder
            val = val.Split('=')[0]; // Remove any trailing '='s
            val = val.Replace('+', '-'); // 62nd char of encoding
            val = val.Replace('/', '_'); // 63rd char of encoding
            return val;
        }
    }


    internal static class DataProtectionExtension
    {
        private const string ProtectorPurpose = "CyberHCL.CS.SF";
        private const string ApplicationName = "HCLCSSFFramework";

        public static AuthorizationResponseModel ParseFragmentString(this string uri)
        {
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
        }

        public static async Task<T> UnProtectDataAsync<T>(this string value)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
            };
            var dataProtectionProvider = DataProtectionProvider.Create(ApplicationName);
            var protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
            var bytes = Base64Url.Decode(value);
            bytes = protector.Unprotect(bytes);
            var json = Encoding.UTF8.GetString(bytes);
            var result = System.Text.Json.JsonSerializer.Deserialize<T>(json, options);
            return Task.FromResult(result).Result;
        }

        public static async Task<AuthorizationResponseModel> ParseAuthorizeResponse(this HttpResponseMessage authResponse)
        {
            AuthorizationResponseModel authModel = new AuthorizationResponseModel();
            authModel.Request = new ValidatedAuthorizeRequestModel();
            var content = authResponse.Content.ReadAsStringAsync().Result;

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

        public static void SetAccessTokenAuthorizationHeader(this HttpClient client, string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            var scheme = AuthenticationSchemes.AuthorizationHeaderBearer;

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, accessToken);
        }

        public static async Task<Dictionary<string, object>> ParseUserInfoResponse(this HttpResponseMessage userInfoResponse)
        {
            var content = userInfoResponse.Content.ReadAsStringAsync().Result;
            var userInfoResult = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
            return userInfoResult;
        }
        public static long ToUnixTime(this DateTime time)
        {
            var totalSeconds = time.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            if (totalSeconds < 0)
            {
                return 0;
            }

            return Convert.ToInt32(totalSeconds);
        }
    }
}


