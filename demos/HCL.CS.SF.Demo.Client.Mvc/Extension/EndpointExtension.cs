/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using HtmlAgilityPack;
using Newtonsoft.Json;
using HCL.CS.SF.DemoClientMvc.Constants;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.Domain.Models.Endpoint.Response;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace HCL.CS.SF.DemoClientMvc.Extension;

public static class EndpointExtension
{
    public static string CreateAuthorizeRequestUrl(
        string clientId = null,
        string responseType = null,
        string scope = null,
        string responseMode = null,
        string prompt = null,
        string codeChallenge = null,
        string codeChallengeMethod = null,
        string maxAge = null,
        string redirectUri = null,
        string nonce = null)
    {
        var dict = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(clientId)) dict.Add(OpenIdConstants.AuthorizeRequest.ClientId, clientId);

        if (!string.IsNullOrEmpty(responseType)) dict.Add(OpenIdConstants.AuthorizeRequest.ResponseType, responseType);

        if (!string.IsNullOrEmpty(scope)) dict.Add(OpenIdConstants.AuthorizeRequest.Scope, scope);

        if (!string.IsNullOrEmpty(redirectUri)) dict.Add(OpenIdConstants.AuthorizeRequest.RedirectUri, redirectUri);

        if (!string.IsNullOrEmpty(nonce)) dict.Add(OpenIdConstants.AuthorizeRequest.Nonce, nonce);

        if (!string.IsNullOrEmpty(nonce)) dict.Add(OpenIdConstants.AuthorizeRequest.Prompt, prompt);

        if (!string.IsNullOrEmpty(nonce)) dict.Add(OpenIdConstants.AuthorizeRequest.MaxAge, maxAge);

        if (!string.IsNullOrEmpty(responseMode)) dict.Add(OpenIdConstants.AuthorizeRequest.ResponseMode, responseMode);

        if (!string.IsNullOrEmpty(codeChallenge))
            dict.Add(OpenIdConstants.AuthorizeRequest.CodeChallenge, codeChallenge);

        if (!string.IsNullOrEmpty(codeChallengeMethod))
            dict.Add(OpenIdConstants.AuthorizeRequest.CodeChallengeMethod, codeChallengeMethod);

        var url = ApplicationConstants.AuthorizeEndpoint.AddQueryString(dict);
        return url;
    }

    public static Dictionary<string, string> CreateRefreshTokenRequest(
        string clientId = null,
        string clientSecret = null,
        string grantType = null,
        string refreshToken = null)
    {
        var dict = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(clientId)) dict.Add(OpenIdConstants.TokenRequest.ClientId, clientId);

        if (!string.IsNullOrEmpty(clientSecret)) dict.Add(OpenIdConstants.TokenRequest.ClientSecret, clientSecret);

        if (!string.IsNullOrEmpty(grantType)) dict.Add(OpenIdConstants.TokenRequest.GrantType, grantType);

        if (!string.IsNullOrEmpty(refreshToken)) dict.Add(OpenIdConstants.TokenRequest.RefreshToken, refreshToken);

        dict.Add("Content-Type", "application/x-www-form-urlencoded");
        return dict;
    }

    public static Dictionary<string, string> CreateIntroSpecRequest(
        string clientId = null,
        string clientSecret = null,
        string token = null,
        string tokenTypeHint = null,
        string scope = null
    )
    {
        var dict = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(clientId)) dict.Add(OpenIdConstants.TokenRequest.ClientId, clientId);

        if (!string.IsNullOrEmpty(clientSecret)) dict.Add(OpenIdConstants.TokenRequest.ClientSecret, clientSecret);

        if (!string.IsNullOrEmpty(token)) dict.Add(OpenIdConstants.IntrospectionRequest.Token, token);

        if (!string.IsNullOrEmpty(tokenTypeHint))
            dict.Add(OpenIdConstants.IntrospectionRequest.TokenHintType, tokenTypeHint);

        if (!string.IsNullOrEmpty(scope)) dict.Add(OpenIdConstants.IntrospectionRequest.Scope, scope);

        dict.Add("Content-Type", "application/x-www-form-urlencoded");
        return dict;
    }

    public static Dictionary<string, string> CreateROPTokenRequest(
        string clientId = null,
        string clientSecret = null,
        string grantType = null,
        string userName = null,
        string password = null,
        string scope = null)
    {
        var dict = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(clientId)) dict.Add(OpenIdConstants.TokenRequest.ClientId, clientId);

        if (!string.IsNullOrEmpty(clientSecret)) dict.Add(OpenIdConstants.TokenRequest.ClientSecret, clientSecret);

        if (!string.IsNullOrEmpty(grantType)) dict.Add(OpenIdConstants.TokenRequest.GrantType, grantType);

        if (!string.IsNullOrEmpty(userName)) dict.Add(OpenIdConstants.TokenRequest.UserName, userName);

        if (!string.IsNullOrEmpty(password)) dict.Add(OpenIdConstants.TokenRequest.Password, password);

        if (!string.IsNullOrEmpty(scope)) dict.Add(OpenIdConstants.TokenRequest.Scope, scope);

        dict.Add("Content-Type", "application/x-www-form-urlencoded");
        return dict;
    }

    public static Dictionary<string, string> CreateClientCredentialsRequest(
        string clientId = null,
        string clientSecret = null,
        string grantType = null,
        string scope = null)
    {
        var dict = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(clientId)) dict.Add(OpenIdConstants.TokenRequest.ClientId, clientId);

        if (!string.IsNullOrEmpty(clientSecret)) dict.Add(OpenIdConstants.TokenRequest.ClientSecret, clientSecret);

        if (!string.IsNullOrEmpty(grantType)) dict.Add(OpenIdConstants.TokenRequest.GrantType, grantType);

        if (!string.IsNullOrEmpty(scope)) dict.Add(OpenIdConstants.TokenRequest.Scope, scope);

        dict.Add("Content-Type", "application/x-www-form-urlencoded");
        return dict;
    }

    public static string AddQueryString(this string uri, Dictionary<string, string> queryString)
    {
        if (uri == null) throw new ArgumentNullException(nameof(uri));

        if (queryString == null) throw new ArgumentNullException(nameof(queryString));

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
            if (parameter.Value == null) continue;

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
        var myUri = new Uri(uri);

        var querystring = myUri.Query;

        var parameters = HttpUtility.ParseQueryString(querystring);

        var response = new AuthorizationResponseModel
        {
            Code = parameters["code"],
            AccessToken = parameters["access_token"],
            RefreshToken = parameters["refresh_token"],
            Scope = parameters["scope"],
            IdentityToken = parameters["id_token"],
            AccessTokenLifetime = string.IsNullOrWhiteSpace(parameters["AccessTokenLifetime"])
                ? 0
                : Convert.ToInt32(parameters["AccessTokenLifetime"]),
            SessionState = parameters["session_state"],
            IsError = Convert.ToBoolean(parameters["IsError"])
        };
        return response;
    }

    public static async Task<AuthorizationResponseModel> ParseAuthorizeResponse(this HttpResponseMessage authResponse)
    {
        var authModel = new AuthorizationResponseModel
        {
            Request = new ValidatedAuthorizeRequestModel()
        };
        var content = await authResponse.Content.ReadAsStringAsync();

        var docToParse = new HtmlDocument();
        docToParse.LoadHtml(content);
        foreach (var node in docToParse.DocumentNode.SelectNodes("//form//input"))
        {
            if (node.Attributes["name"].Value == "code") authModel.Code = node.Attributes["value"].Value;

            if (node.Attributes["name"].Value == "id_token") authModel.IdentityToken = node.Attributes["value"].Value;

            if (node.Attributes["name"].Value == "session_state")
                authModel.SessionState = node.Attributes["value"].Value;

            if (node.Attributes["name"].Value == "scope") authModel.Scope = node.Attributes["value"].Value;

            if (node.Attributes["name"].Value == "access_token") authModel.AccessToken = node.Attributes["value"].Value;

            if (node.Attributes["name"].Value == "refresh_token")
                authModel.RefreshToken = node.Attributes["value"].Value;

            if (node.Attributes["name"].Value == "state") authModel.Request.State = node.Attributes["value"].Value;
        }

        return authModel;
    }

    public static async Task<TokenResponseResultModel> ParseTokenResponseResult(this HttpResponseMessage tokenResponse)
    {
        var content = await tokenResponse.Content.ReadAsStringAsync();
        var tokenResult = JsonConvert.DeserializeObject<TokenResponseResultModel>(content);
        return tokenResult;
    }

    public static async Task<IntrospectionResponseModel> ParseIntrospectionResponse(
        this HttpResponseMessage introspecResponse)
    {
        var content = await introspecResponse.Content.ReadAsStringAsync();
        var introspectResult = JsonConvert.DeserializeObject<IntrospectionResponseModel>(content);
        return introspectResult;
    }

    public static async Task<ErrorResponseModel> ParseTokenErrorResponse(this HttpResponseMessage tokenResponse)
    {
        var content = await tokenResponse.Content.ReadAsStringAsync();
        var tokenResult = JsonConvert.DeserializeObject<ErrorResponseResultModel>(content);
        return new ErrorResponseModel
            { ErrorCode = tokenResult.error, ErrorDescription = tokenResult.error_description, IsError = true };
    }

    public static async Task WriteResponseJsonAsync(this HttpResponse response, object content,
        string contentType = null)
    {
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        var json = JsonSerializer.Serialize(content, options);

        response.ContentType = contentType ?? "application/json; charset=UTF-8";
        await response.WriteAsync(json);
        await response.Body.FlushAsync();
    }

    public static string RandomString(this int keyLength)
    {
        using (var rngCryptoServiceProvider = RandomNumberGenerator.Create())
        {
            var randomBytes = new byte[keyLength];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }

    public static string GenerateCodeChallenge(this string codeverifier)
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

    public static string Encode(this byte[] arg)
    {
        var val = Convert.ToBase64String(arg); // Standard base64 encoder
        val = val.Split('=')[0]; // Remove any trailing '='s
        val = val.Replace('+', '-'); // 62nd char of encoding
        val = val.Replace('/', '_'); // 63rd char of encoding
        return val;
    }

    public static string GenerateRandomDataBase64url(uint length)
    {
        var rng = RandomNumberGenerator.Create();
        var bytes = new byte[length];
        rng.GetBytes(bytes);
        return Base64UrlEncodeNoPadding(bytes);
    }

    private static string Base64UrlEncodeNoPadding(byte[] buffer)
    {
        var base64 = Convert.ToBase64String(buffer);

        // Converts base64 to base64url.
        base64 = base64.Replace("+", "-");
        base64 = base64.Replace("/", "_");
        // Strips padding.
        base64 = base64.Replace("=", "");

        return base64;
    }
}
