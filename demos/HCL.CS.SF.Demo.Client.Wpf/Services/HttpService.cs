/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using HCL.CS.SF.DemoClientWpfApp.Constants;
using HCL.CS.SF.DemoClientWpfApp.DomainModel;
using HCL.CS.SF.DemoClientWpfApp.Extension;
using HCL.CS.SF.DemoClientWpfApp.ViewModel;

namespace HCL.CS.SF.DemoClientWpfApp.Services
{

    internal static class Http
    {
        private static HttpClient httpClient = new HttpClient();
        private static bool timeOutSet = false;
        internal static HttpClient Client
        {
            get
            {
                if (!timeOutSet)
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(6);
                    timeOutSet = true;
                }
                return httpClient;
            }
        }
    }
    internal class HttpService :BaseModel
    {
        ApplicationParameters ApplicationConstants = new ApplicationParameters();
        public async Task<T> PostSecureAsync<T>(string url, object Value)
        {
            try
            {
                url = ApplicationConstants.BaseUrl + url;
                var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(5);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",Global.AccessToken );
                var httpResponse = httpClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(Value), Encoding.UTF8, "application/json")).Result;
                var content = httpResponse.Content.ReadAsStringAsync().Result;
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<T>(content);
                    return result;
                }
                else
                {
                    throw new Exception(content);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<TokenResponseResultModel> AuthCodeFlow()
        {
            TokenResponseResultModel tokenResponseResultModel = new TokenResponseResultModel();
            string state = GenerateRandomDataBase64url(32);
            string redirectUri = ApplicationConstants.RedirectUri;
            Log("redirect URI: " + redirectUri);
            var http = new HttpListener();
            http.Prefixes.Add(redirectUri);
            Log("Listening..");
            http.Start();

            var nonce = Guid.NewGuid().ToString();
            var codeVerifier = EndpointExtension.RandomString(32);
            var codeChallengeString = codeVerifier.GenerateCodeChallenge();
            var authcodeRequest = EndpointExtension.CreateAuthorizeRequestUrl(
                           clientId: ApplicationConstants.ClientId,
                           responseType: ApplicationConstants.AuthResponseType,
                           scope: ApplicationConstants.AuthScope,
                           responseMode: ApplicationConstants.AuthResponseMode,
                           prompt: ApplicationConstants.Prompt,
                           codeChallenge: codeChallengeString, 
                           codeChallengeMethod: "S256", 
                           maxAge: "60",
                           redirectUri: redirectUri,
                           nonce: nonce,
                           state:state);
            var returnQuery = Http.Client.GetAsync(authcodeRequest).Result;

            var authResponse = returnQuery.Headers.Location.ToString().ParseQueryString();
            http.Stop();
            Log("HTTP server stopped.");

            var tokenRequest = EndpointExtension.CreateTokenRequest(
            clientId: ApplicationConstants.ClientId,
            clientSecret: ApplicationConstants.ClientSecret,
            code: authResponse.Code,
            redirectUri: redirectUri,
            grantType: GrantTypes.AuthorizationCode,
            codeVerifier: codeVerifier);

            var tokenResponse = Http.Client.PostAsync(ApplicationConstants.TokenEndpoint, new FormUrlEncodedContent(tokenRequest)).Result;
          
            if (tokenResponse.StatusCode==HttpStatusCode.OK)
            {
                 tokenResponseResultModel = tokenResponse.ParseTokenResponseResult().Result;
                Global.AccessToken = tokenResponseResultModel.access_token;
                Global.IdToken = tokenResponseResultModel.id_token;
                Global.RefreshToken = tokenResponseResultModel.refresh_token;
                return tokenResponseResultModel;
            }
            else
            { 
            var errorResponse = tokenResponse.ParseTokenResponseResult();
                tokenResponseResultModel.ErrorResponseResult = errorResponse.Result.ErrorResponseResult;
            }

            return tokenResponseResultModel;
        }
        public async Task<TokenResponseResultModel> ClientCredentialFlow()
        {
            TokenResponseResultModel tokenResponseResultModel = new TokenResponseResultModel();
            var tokenRequest = EndpointExtension.CreateTokenRequest(
                clientId: ApplicationConstants.ClientId,
                clientSecret: ApplicationConstants.ClientSecret,
                grantType: GrantTypes.ClientCredentials,
                scope: ApplicationConstants.ClientScope);
         
            var tokenResponse = Http.Client.PostAsync(ApplicationConstants.TokenEndpoint, new FormUrlEncodedContent(tokenRequest)).Result;
            if (tokenResponse.StatusCode==HttpStatusCode.OK)
            {
                var tokenResult = tokenResponse.ParseTokenResponseResult().Result;
                tokenResponseResultModel = tokenResult;
                Global.AccessToken = tokenResult.access_token;
                Global.IdToken = tokenResult.id_token;
            }
            else
            {
                var errorRespose = tokenResponse.ParseTokenResponseResult();
                tokenResponseResultModel.ErrorResponseResult = errorRespose.Result.ErrorResponseResult;
            }
            
            return tokenResponseResultModel;
        }
        public async Task<AuthorizationResponseModel>HyBridFlow()
        {
            AuthorizationResponseModel authorizationResponseModel = new AuthorizationResponseModel();
            string state = GenerateRandomDataBase64url(32);
            string redirectUri = ApplicationConstants.RedirectUri;
            Log("redirect URI: " + redirectUri);
           
            var http = new HttpListener();
            http.Prefixes.Add(redirectUri);
            Log("Listening..");
            http.Start();

            var nonce = Guid.NewGuid().ToString();
            var codeVerifier = EndpointExtension.RandomString(32);
            var codeChallengeString = codeVerifier.GenerateCodeChallenge();
            var authcodeRequest = EndpointExtension.CreateAuthorizeRequestUrl(
                           clientId: ApplicationConstants.ClientId,
                           responseType: ApplicationConstants.HybridResponseType,
                           scope: ApplicationConstants.HybridScope,
                           responseMode: ApplicationConstants.HybridResponseMode,
                           prompt: ApplicationConstants.Prompt,
                           codeChallenge: codeChallengeString,
                           codeChallengeMethod: "S256",
                           maxAge: "60",
                           redirectUri: redirectUri,
                           nonce: nonce,
                           state: state);
            var returnQuery = Http.Client.GetAsync(authcodeRequest).Result;

            if (returnQuery.StatusCode==HttpStatusCode.OK)
            {
                var authResponse = returnQuery.ParseAuthorizeResponse().Result;
                authorizationResponseModel = authResponse;
                Global.AccessToken = authResponse.AccessToken;
                Global.IdToken = authResponse.IdentityToken;
                Global.RefreshToken = authResponse.RefreshToken;
            }
            else
            {
                var ErrorResonse = returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync().Result;
                authorizationResponseModel.ErrorCode = ErrorResonse.ErrorCode;
                authorizationResponseModel.ErrorDescription = ErrorResonse.ErrorDescription;
                authorizationResponseModel.IsError = ErrorResonse.IsError;
            }
            http.Stop();
            Log("HTTP server stopped.");
            return authorizationResponseModel;
        }
        public async Task<Dictionary<string, object>> UserInfo()
        {
            Dictionary<string, object> userifo = new Dictionary<string, object>();
            string state = GenerateRandomDataBase64url(32);
            string redirectUri = ApplicationConstants.RedirectUri;
            Log("redirect URI: " + redirectUri);
            var http = new HttpListener();
            http.Prefixes.Add(redirectUri);
            Log("Listening..");
            http.Start();

            var nonce = Guid.NewGuid().ToString();
            var codeVerifier = EndpointExtension.RandomString(32);
            var codeChallengeString = codeVerifier.GenerateCodeChallenge();
            var authcodeRequest = EndpointExtension.CreateAuthorizeRequestUrl(
                           clientId: ApplicationConstants.ClientId,
                           responseType: ApplicationConstants.UserInfoResponseType,
                           scope: ApplicationConstants.UserInfoScope,
                           responseMode: ApplicationConstants.UserInfoResponseMode,
                           prompt: ApplicationConstants.Prompt,
                           codeChallenge: codeChallengeString,
                           codeChallengeMethod: "S256",
                           maxAge: "60",
                           redirectUri: redirectUri,
                           nonce: nonce,
                           state: state);
            var returnQuery = Http.Client.GetAsync(authcodeRequest).Result;

            if (returnQuery.StatusCode==HttpStatusCode.OK)
            {
                var authResponse = returnQuery.ParseAuthorizeResponse().Result;
                Http.Client.SetAccessTokenAuthorizationHeader(authResponse.AccessToken);
                var userInfoResponse = Http.Client.GetAsync(ApplicationConstants.UserInfoEndpoint).Result;
                userifo = userInfoResponse.ParseUserInfoResponse().Result;
                return userifo;
            }
            http.Stop();
            Log("HTTP server stopped.");
            return userifo;
        }
        public async Task<TokenResponseResultModel> JWTPost()
        {
            string state = GenerateRandomDataBase64url(32);
            string redirectUri = ApplicationConstants.RedirectUri;
            Log("redirect URI: " + redirectUri);

            var http = new HttpListener();
            http.Prefixes.Add(redirectUri);
            Log("Listening..");
            http.Start();
            ClientsModel clientModel = new ClientsModel();
            clientModel.ClientId = ApplicationConstants.ClientId;
            clientModel.ClientSecret = ApplicationConstants.ClientSecret;
            var nonce = Guid.NewGuid().ToString();
            var codeVerifier = EndpointExtension.RandomString(32);
            var codeChallengeString = codeVerifier.GenerateCodeChallenge();
            var authcodeRequest = EndpointExtension.CreateAuthorizeRequestUrl(
                           clientId: ApplicationConstants.ClientId,
                           responseType: ApplicationConstants.JwtResponseType,
                           scope: ApplicationConstants.JwtScope,
                           responseMode: ApplicationConstants.JwtResponseMode,
                           prompt: ApplicationConstants.Prompt,
                           codeChallenge: codeChallengeString,
                           codeChallengeMethod: "S256",
                           maxAge: "60",
                           redirectUri: redirectUri,
                           nonce: nonce,
                           state: state);
            var returnQuery = Http.Client.GetAsync(authcodeRequest).Result;

            var authResponse = returnQuery.Headers.Location.ToString().ParseQueryString();
            http.Stop();
            Log("HTTP server stopped.");
            var tokenRequest = EndpointExtension.CreateTokenRequest(
            clientId: ApplicationConstants.ClientId,
            clientSecret: ApplicationConstants.ClientSecret,
            code: authResponse.Code,
            redirectUri: redirectUri,
            grantType: GrantTypes.AuthorizationCode,
            codeVerifier: codeVerifier,
            clientAssertionType: "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
            clientAssertion: GenerateJWTSecretRequest(clientModel)
            ); 

            var tokenResponse = Http.Client.PostAsync(ApplicationConstants.TokenEndpoint, new FormUrlEncodedContent(tokenRequest)).Result;
            var tokenResult = tokenResponse.ParseTokenResponseResult().Result;
            Global.AccessToken = tokenResult.access_token;
            Global.IdToken = tokenResult.id_token;
            Global.RefreshToken = tokenResult.refresh_token;
            return tokenResult;
        }
        public async Task<TokenResponseResultModel> ROP(string userName, string password)
        {
            TokenResponseResultModel tokenResponseResultModel = new TokenResponseResultModel();
            string redirectUri = ApplicationConstants.RedirectUri;
            Log("redirect URI: " + redirectUri);
            var tokenRequest = EndpointExtension.CreateTokenRequest(
                clientId: ApplicationConstants.ClientId,
                clientSecret: ApplicationConstants.ClientSecret,
                grantType: GrantTypes.Password,
                userName: userName,
                password: password,
                scope: ApplicationConstants.ROPScope);

            var tokenResponse = Http.Client.PostAsync(ApplicationConstants.TokenEndpoint, new FormUrlEncodedContent(tokenRequest)).Result;
            if (tokenResponse.StatusCode == HttpStatusCode.OK)
            {
                var tokenResult = tokenResponse.ParseTokenResponseResult().Result;
                Global.AccessToken = tokenResult.access_token;
                Global.IdToken = tokenResult.id_token;
                Global.RefreshToken = tokenResult.refresh_token;
                tokenResponseResultModel = tokenResult;
            }
            else
            {
                var errorRespose = tokenResponse.ParseTokenResponseResult();
                tokenResponseResultModel.ErrorResponseResult = errorRespose.Result.ErrorResponseResult;
            }
            return tokenResponseResultModel;
        }
        public async Task<TokenResponseResultModel> RefreshToken()
        {
            string state = GenerateRandomDataBase64url(32);
            string redirectUri = ApplicationConstants.RedirectUri;
            Log("redirect URI: " + redirectUri);
            var http = new HttpListener();
            http.Prefixes.Add(redirectUri);
            Log("Listening..");
            http.Start();

            var nonce = Guid.NewGuid().ToString();
            var codeVerifier = EndpointExtension.RandomString(32);
            var codeChallengeString = codeVerifier.GenerateCodeChallenge();
            var authcodeRequest = EndpointExtension.CreateAuthorizeRequestUrl(
                           clientId: ApplicationConstants.ClientId,
                           responseType: ApplicationConstants.RefreshTokenType,
                           scope: ApplicationConstants.RefreshTokenScope,
                           responseMode: ApplicationConstants.RefreshTokenMode,
                           prompt: ApplicationConstants.Prompt,
                           codeChallenge: codeChallengeString,
                           codeChallengeMethod: "S256", 
                           maxAge: "60",
                           redirectUri: redirectUri,
                           nonce: nonce,
                           state: state);
            var returnQuery = Http.Client.GetAsync(authcodeRequest).Result;
            var authResponse = returnQuery.Headers.Location.ToString().ParseFragmentString();
            http.Stop();
            Log("HTTP server stopped.");

            var tokenRequest = EndpointExtension.CreateTokenRequest(
            clientId: ApplicationConstants.ClientId,
            clientSecret: ApplicationConstants.ClientSecret,
            refreshToken: authResponse.RefreshToken,
            redirectUri: redirectUri,
            grantType: GrantTypes.RefreshToken,
            codeVerifier: codeVerifier);

            var tokenResponse = Http.Client.PostAsync(ApplicationConstants.TokenEndpoint, new FormUrlEncodedContent(tokenRequest)).Result;
            var tokenResult =  tokenResponse.ParseTokenResponseResult().Result;
            Global.AccessToken = tokenResult.access_token;
            Global.IdToken = tokenResult.id_token;
            Global.RefreshToken = tokenResult.refresh_token;
            return tokenResult;
        }
        private string GenerateRandomDataBase64url(uint length)
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] bytes = new byte[length];
            rng.GetBytes(bytes);
            return Base64UrlEncodeNoPadding(bytes);
        }
        private  string Base64UrlEncodeNoPadding(byte[] buffer)
        {
            string base64 = Convert.ToBase64String(buffer);
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            base64 = base64.Replace("=", "");
            return base64;
        }
        private void Log(string output)
        {
            Console.WriteLine(output);
        }
        public string GenerateJWTSecretRequest(ClientsModel client)
        {
            var now = DateTime.UtcNow;
            var algorithm = Algorithms.HmacSha256;
            byte[] securityKey = Encoding.ASCII.GetBytes(client.ClientSecret);
            var credentials = new SigningCredentials(new SymmetricSecurityKey(securityKey), algorithm);
            var claims = new List<Claim>
            {
                new Claim("sub", client.ClientId),
                new Claim("iat", now.ToUnixTime().ToString()),
                new Claim("jti", AuthenticationConstants.KeySize32.RandomString()),
            };

            var jwt = new JwtSecurityToken(
                issuer: client.ClientId,
                audience: ApplicationConstants.TokenEndpoint,
                claims: claims,
                notBefore: now,
                DateTime.UtcNow.AddSeconds(client.AccessTokenExpiration),
                signingCredentials: credentials);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return encodedJwt;
        }

        public string GeneratePrivateJWTRequest(ClientsModel client)
        {
            var now = DateTime.UtcNow;
            var algorithm = Algorithms.RsaSha256;
            string CertPath = "C:\\Source\\HCL.CS.SF.NET6.CROSS_PLATFORM\\PrivateAndPublicKey\\privatejwt.pfx";
            string password = "123456";
            var ClientCert = new X509Certificate2(CertPath, password);

            var credentials = GenerateAsymmetricSigningCredentials(ClientCert, algorithm);
            //var credentials = new RsaSecurityKey(LoadRsaPrivateKey());
            var claims = new List<Claim>
            {
                new Claim("sub", client.ClientId),
                new Claim("iat", now.ToUnixTime().ToString()),
                new Claim("jti", AuthenticationConstants.KeySize32.RandomString()),
            };

            var jwt = new JwtSecurityToken(
                issuer: client.ClientId,
                audience: ApplicationConstants.TokenEndpoint,
                claims: claims,
                notBefore: now,
                DateTime.UtcNow.AddSeconds(client.AccessTokenExpiration),
                signingCredentials: credentials);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return encodedJwt;
        }
        public SigningCredentials GenerateAsymmetricSigningCredentials(X509Certificate2 certificate, string algorithm)
        {
            SecurityKey securityKey = null;
            algorithm = algorithm.ToUpper();
            if (algorithm.StartsWith("RS") || algorithm.StartsWith("PS"))
            {
                if (certificate.HasPrivateKey)
                {
                    RSA rsa = certificate.GetRSAPrivateKey();
                    if (rsa == null)
                    {
                        throw new InvalidOperationException("InvalidCertificateRsa");
                    }

                    securityKey = new RsaSecurityKey(rsa);
                }
            }
            else if (algorithm.StartsWith("ES"))
            {
                if (certificate.HasPrivateKey)
                {
                    ECDsa ecdsa = certificate.GetECDsaPrivateKey();
                    if (ecdsa == null)
                    {
                        throw new InvalidOperationException("InvalidCertificatEcdsa");
                    }

                    securityKey = new ECDsaSecurityKey(ecdsa);
                }
            }
            else
            {
                throw new InvalidOperationException("UnsupportedAlgorithm");
            }

            SigningCredentials signingCredentials = new SigningCredentials(securityKey, algorithm);
            return signingCredentials;
        }
    }
}



