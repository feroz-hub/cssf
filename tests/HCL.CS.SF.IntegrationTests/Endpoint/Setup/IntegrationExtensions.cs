/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Web;
using HtmlAgilityPack;
using IntegrationTests.ApiDomainModel;
using Newtonsoft.Json;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;

//using HCL.CS.SF.Domain.Models;
namespace IntegrationTests.Endpoint.Setup;

public static class IntegrationExtensions
{
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

    public static AuthorizationResponseModel ParseFragmentString(this string uri)
    {
        var myUri = new Uri(uri);

        var fragmentstring = myUri.Fragment;

        var parameters = HttpUtility.ParseQueryString(fragmentstring);

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

    public static string ParseLogoutQueryStringAsync(this string uri)
    {
        var myUri = new Uri(uri);

        var querystring = myUri.Query;

        var parameters = HttpUtility.ParseQueryString(querystring);

        var logoutId = parameters["logoutId"];

        return logoutId;
    }

    public static async Task<LogoutMessageModel> ParseLogoutQueryMessageAsync(this string logoutId)
    {
        var response = await logoutId.UnProtectDataAsync<LogoutMessageModel>();
        return response;
    }

    public static async Task<AllowedScopesParserModel.AuthorizeErrorResponseModel> ParseErrorQueryStringAsync(
        this string uri)
    {
        var myUri = new Uri(uri);

        var querystring = myUri.Query;

        var parameters = HttpUtility.ParseQueryString(querystring);

        var errorId = parameters["errorId"];

        var response = await errorId.UnProtectDataAsync<AllowedScopesParserModel.AuthorizeErrorResponseModel>();

        return response;
    }

    //public static async Task<TokenResponseModel> ParseTokenResponse(this HttpResponseMessage tokenResponse)
    //{
    //    var content = await tokenResponse.Content.ReadAsStringAsync();
    //    var tokenResult = JsonConvert.DeserializeObject<TokenResponseModel>(content);
    //    return tokenResult;
    //}

    public static async Task<TokenResponseResultModel> ParseTokenResponseResult(this HttpResponseMessage tokenResponse)
    {
        var content = await tokenResponse.Content.ReadAsStringAsync();
        var tokenResult = JsonConvert.DeserializeObject<TokenResponseResultModel>(content);
        return tokenResult;
    }

    public static async Task<Dictionary<string, object>> ParseUserInfoResponse(
        this HttpResponseMessage userInfoResponse)
    {
        var content = await userInfoResponse.Content.ReadAsStringAsync();
        var userInfoResult = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
        return userInfoResult;
    }

    public static ErrorResponseModel ParseUserInfoErrorResponse(this HttpResponseMessage userInfoResponse)
    {
        var authorizationHeader = userInfoResponse.Headers.WwwAuthenticate;
        var authHeader = authorizationHeader.ToString();
        var result = authHeader.Replace("\"", string.Empty);
        var headers = result.Split(',');
        var errorResponse = new ErrorResponseModel();

        if (headers[0] == OpenIdConstants.AuthenticationSchemes.AuthorizationHeaderBearer)
        {
            errorResponse.ErrorCode = headers[1].Split('=')[1];
            errorResponse.ErrorDescription = headers[2].Split('=')[1];
        }

        return errorResponse;
    }

    public static string GenerateCodeChallenge(this string codeverifier)
    {
        var codeChallengeBytes = Encoding.ASCII.GetBytes(codeverifier);
        var hashedBytes = codeChallengeBytes.Sha256();
        var codeChallengeString = hashedBytes.Encode();

        return codeChallengeString;
    }

    //public static string GenerateCodeVerifier(this int size)
    //{
    //    var codeChallengeBytes = Encoding.ASCII.GetBytes(codeverifier);
    //    var hashedBytes = codeChallengeBytes.Sha256();
    //    var codeChallengeString = hashedBytes.Encode();

    //    return codeChallengeString;
    //}
    public static async Task<AuthorizationResponseModel> ParseAuthorizeResponse(this HttpResponseMessage authResponse)
    {
        var authModel = new AuthorizationResponseModel();
        authModel.Request = new ValidatedAuthorizeRequestModel();
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

    public static async Task<ErrorResponseModel> ParseTokenErrorResponse(this HttpResponseMessage tokenResponse)
    {
        var content = await tokenResponse.Content.ReadAsStringAsync();
        var tokenResult = JsonConvert.DeserializeObject<ErrorResponseResultModel>(content);
        return new ErrorResponseModel
            { ErrorCode = tokenResult.error, ErrorDescription = tokenResult.error_description, IsError = true };
    }

    public static async Task<AllowedScopesParserModel.IntrospectionResponseModel> ParseIntrospectionResponse(
        this HttpResponseMessage introspecResponse)
    {
        var content = await introspecResponse.Content.ReadAsStringAsync();
        var introspecResult =
            JsonConvert.DeserializeObject<AllowedScopesParserModel.IntrospectionResponseModel>(content);
        return introspecResult;
    }

    public static async Task<ErrorResponseModel> ParseIntrospectionErrorResponse(
        this HttpResponseMessage introspecResponse)
    {
        var content = await introspecResponse.Content.ReadAsStringAsync();
        var tokenResult = JsonConvert.DeserializeObject<ErrorResponseResultModel>(content);
        return new ErrorResponseModel
            { ErrorCode = tokenResult.error, ErrorDescription = tokenResult.error_description, IsError = true };
    }

    public static void SetBasicAuthentication(this HttpClient client, string clientId, string clientSecret)
    {
        if (string.IsNullOrWhiteSpace(clientId)) throw new ArgumentNullException(nameof(clientId));

        if (clientSecret == null) clientSecret = string.Empty;

        var encoding = Encoding.UTF8;
        var credential = $"{HttpUtility.UrlEncode(clientId)}:{HttpUtility.UrlEncode(clientSecret)}";
        var scheme = "Basic";

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(scheme, Convert.ToBase64String(encoding.GetBytes(credential)));
    }

    public static void SetAccessTokenAuthorizationHeader(this HttpClient client, string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken)) throw new ArgumentNullException(nameof(accessToken));

        var scheme = OpenIdConstants.AuthenticationSchemes.AuthorizationHeaderBearer;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, accessToken);
    }
}
