/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Service.Implementation.Endpoint.Comparers;
using HCL.CS.SF.Service.Implementation.Endpoint.Converters;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Extensions;

/// <summary>
/// HTTP context and request/response utility extension methods for the OAuth/OIDC endpoints.
/// Provides helpers for HTML form post rendering, JSON serialization, query string construction,
/// CSP header management, session ID handling, URL manipulation, and claim collection conversion.
/// </summary>
internal static class HttpContextExtension
{
    // TODO - rename applicable extension method names.
    private const string InputFieldFormat = "<input type='hidden' name='{0}' value='{1}' />\n";

    private static readonly string UrlStartsWith = "~/";
    private static readonly string UrlParameterStartsWith = "?";
    private static readonly string UrlParameterSeparator = "&";
    private static readonly string UrlFragmentSeparator = "#";
    private static readonly string UrlValueAssignSeparator = "=";

    /// <summary>
    /// Prepares the form post string.
    /// </summary>
    /// <param name="collectionLst">The collection lst.</param>
    /// <returns>The operation result.</returns>
    internal static string PrepareFormPostString(this Dictionary<string, string> collectionLst)
    {
        var builder = new StringBuilder(128);

        foreach (var name in collectionLst)
        {
            var value = HtmlEncoder.Default.Encode(name.Value);
            builder.AppendFormat(InputFieldFormat, name.Key, value);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Writes the html response.
    /// </summary>
    /// <param name="response">The response.</param>
    /// <param name="html">The html.</param>
    /// <returns>The operation result.</returns>
    internal static async Task WriteHtmlResponseAsync(this HttpResponse response, string html)
    {
        response.ContentType = "text/html; charset=UTF-8";
        await response.WriteAsync(html, Encoding.UTF8);
        await response.Body.FlushAsync();
    }

    /// <summary>
    /// Adds a new script csp header.
    /// </summary>
    /// <param name="response">The response.</param>
    /// <param name="cspLevel">The csp level.</param>
    /// <param name="hash">The hash.</param>
    internal static void AddScriptCspHeaders(this HttpResponse response, CspLevel cspLevel, string hash)
    {
        var csp1Part = cspLevel == CspLevel.One ? "'unsafe-inline' " : string.Empty;
        var cspHeader = $"default-src 'none'; script-src {csp1Part}'{hash}'";

        AddCspHeaders(response.Headers, cspHeader);
    }

    /// <summary>
    /// Adds a new style csp header.
    /// </summary>
    internal static void AddStyleCspHeaders(this HttpResponse response, CspLevel cspLevel, string hash,
        string frameSources)
    {
        var csp1Part = cspLevel == CspLevel.One ? "'unsafe-inline' " : string.Empty;
        var cspHeader = $"default-src 'none'; style-src {csp1Part}'{hash}'";

        if (!string.IsNullOrWhiteSpace(frameSources)) cspHeader += $"; frame-src {frameSources}";

        AddCspHeaders(response.Headers, cspHeader);
    }

    /// <summary>
    /// Adds a new csp header.
    /// </summary>
    /// <param name="headers">The headers.</param>
    /// <param name="cspHeader">The csp header.</param>
    internal static void AddCspHeaders(IHeaderDictionary headers, string cspHeader)
    {
        if (!headers.ContainsKey("Content-Security-Policy")) headers.Append("Content-Security-Policy", cspHeader);

        if (!headers.ContainsKey("X-Content-Security-Policy")) headers.Append("X-Content-Security-Policy", cspHeader);
    }

    /// <summary>
    /// Retrieves the subject id.
    /// </summary>
    /// <param name="principle">The principle.</param>
    /// <returns>The operation result.</returns>
    internal static string GetSubjectId(this ClaimsPrincipal principle)
    {
        if (principle.Identity is ClaimsIdentity id)
        {
            var claim = id.FindFirst(OpenIdConstants.ClaimTypes.Sub);
            if (claim != null) return claim.Value;
        }

        return null;
    }

    /// <summary>
    /// Retrieves the origi.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <returns>The operation result.</returns>
    internal static string GetOrigin(this string url)
    {
        if (url != null)
        {
            Uri uri;
            try
            {
                uri = new Uri(url);
            }
            catch (Exception)
            {
                return null;
            }

            if (uri.Scheme == "http" || uri.Scheme == "https") return $"{uri.Scheme}://{uri.Authority}";
        }

        return null;
    }

    /// <summary>
    /// Retrieves the session id.
    /// </summary>
    /// <param name="properties">The properties.</param>
    /// <returns>The operation result.</returns>
    internal static string GetSessionId(this AuthenticationProperties properties)
    {
        if (properties?.Items.ContainsKey("session_id") == true) return properties.Items["session_id"];

        return null;
    }

    /// <summary>
    /// Sets the session id.
    /// </summary>
    /// <param name="properties">The properties.</param>
    /// <param name="sessionId">The session id.</param>
    internal static void SetSessionId(this AuthenticationProperties properties, string sessionId)
    {
        properties.Items["session_id"] = sessionId;
    }

    /// <summary>
    /// Checks the sign out called.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    internal static bool CheckSignOutCalled(this HttpContext context)
    {
        return context.Items.ContainsKey(AuthenticationConstants.EnvironmentPaths.SignOutCalled);
    }

    /// <summary>
    /// Writes the response jso.
    /// </summary>
    /// <returns>The operation result.</returns>
    internal static async Task WriteResponseJsonAsync(this HttpResponse response, object content,
        string contentType = null)
    {
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        var json = JsonSerializer.Serialize(content, options);

        response.ContentType = contentType ?? "application/json; charset=UTF-8";
        await response.WriteResponseJsonAsync(json, contentType);
        await response.Body.FlushAsync();
    }

    /// <summary>
    /// Writes the response jso.
    /// </summary>
    /// <returns>The operation result.</returns>
    internal static async Task WriteResponseJsonAsync(this HttpResponse response, string json,
        string contentType = null)
    {
        response.ContentType = contentType ?? "application/json; charset=UTF-8";
        await response.WriteAsync(json);
        await response.Body.FlushAsync();
    }

    /// <summary>
    /// Performs the post operation.
    /// </summary>
    /// <returns>The operation result.</returns>
    internal static async Task PostAsync(this HttpClient httpClient, string redirectUrl,
        Dictionary<string, string> content, CancellationToken cancellationToken = default)
    {
        await httpClient.PostAsync(redirectUrl, new FormUrlEncodedContent(content), cancellationToken);
    }

    /// <summary>
    /// Performs the redirect to absolute url operation.
    /// </summary>
    /// <param name="response">The response.</param>
    /// <param name="url">The url.</param>
    internal static void RedirectToAbsoluteUrl(this HttpResponse response, string url)
    {
        if (url.CheckLocalUrl())
        {
            if (url.StartsWith(UrlStartsWith)) url = url[1..];

            var host = response.HttpContext.GetHCLCSSFBaseUrl().IncludeEndSlash();
            url = host + url.RemoveFrontSlash();
        }

        response.Redirect(url);
    }

    /// <summary>
    /// Performs the json serialize operation.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>The operation result.</returns>
    internal static string JsonSerialize<T>(T request)
    {
        var options = new JsonSerializerOptions
        {
            MaxDepth = 0,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            IgnoreReadOnlyProperties = true
        };

        return JsonSerializer.Serialize(request, options);
    }

    /// <summary>
    /// Performs the json deserialize operation.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>The operation result.</returns>
    internal static T JsonDeserialize<T>(this string request)
    {
        var options = new JsonSerializerOptions
        {
            MaxDepth = 0,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            IgnoreReadOnlyProperties = true
        };

        return JsonSerializer.Deserialize<T>(request, options);
    }

    /// <summary>
    /// Performs the json claim serialize operation.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>The operation result.</returns>
    internal static string JsonClaimSerialize<T>(this T request)
    {
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };
        settings.Converters.Add(new ClaimConverter());
        settings.Converters.Add(new ClaimsPrincipalConverter());

        return JsonConvert.SerializeObject(request, settings);
    }

    /// <summary>
    /// Performs the json claim deserialize operation.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>The operation result.</returns>
    internal static T JsonClaimDeserialize<T>(this string request)
    {
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };
        settings.Converters.Add(new ClaimConverter());
        settings.Converters.Add(new ClaimsPrincipalConverter());

        return JsonConvert.DeserializeObject<T>(request, settings);
    }

    /// <summary>
    /// Prepares the query string.
    /// </summary>
    /// <param name="collectionLst">The collection lst.</param>
    /// <returns>The operation result.</returns>
    internal static string PrepareQueryString(this Dictionary<string, string> collectionLst)
    {
        if (collectionLst != null && collectionLst.Count > 0)
        {
            var builder = new StringBuilder(AuthenticationConstants.QueryStringLength);
            var isFirstValue = true;
            foreach (var collection in collectionLst)
                isFirstValue = AppendString(builder, isFirstValue, true, collection.Key,
                    string.IsNullOrWhiteSpace(collection.Value) ? string.Empty : collection.Value);

            return builder.ToString();
        }

        return string.Empty;
    }

    /// <summary>
    /// Formats the query string.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <param name="query">The query.</param>
    /// <returns>The operation result.</returns>
    internal static string FormatQueryString(this string url, string query)
    {
        if (!url.Contains(UrlParameterStartsWith))
            url += UrlParameterStartsWith;
        else if (!url.EndsWith(UrlParameterSeparator)) url += UrlParameterSeparator;

        return url + query;
    }

    /// <summary>
    /// Formats the fragment string.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <param name="query">The query.</param>
    /// <returns>The operation result.</returns>
    internal static string FormatFragmentString(this string url, string query)
    {
        if (!url.Contains(UrlFragmentSeparator)) url += UrlFragmentSeparator;

        return url + query;
    }

    /// <summary>
    /// Adds a new query string.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <param name="name">The name.</param>
    /// <param name="value">The value.</param>
    /// <returns>The operation result.</returns>
    internal static string AddQueryString(this string url, string name, string value)
    {
        return url.FormatQueryString(name + "=" + UrlEncoder.Default.Encode(value));
    }

    /// <summary>
    /// Determines whether local url.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    internal static bool IsLocalUrl(this string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;

        // Allows "/" or "/foo" but not "//" or "/\".
        if (url[0] == '/')
        {
            // url is exactly "/"
            if (url.Length == 1) return true;

            // url doesn't start with "//" or "/\"
            if (url[1] != '/' && url[1] != '\\') return true;

            return false;
        }

        // Allows "~/" or "~/foo" but not "~//" or "~/\".
        if (url[0] == '~' && url.Length > 1 && url[1] == '/')
        {
            // url is exactly "~/"
            if (url.Length == 2) return true;

            // url doesn't start with "~//" or "~/\"
            if (url[2] != '/' && url[2] != '\\') return true;

            return false;
        }

        return false;
    }

    /// <summary>
    /// Checks the header content type.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    internal static bool CheckHeaderContentType(this HttpRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.ContentType) &&
            MediaTypeHeaderValue.TryParse(request.ContentType, out var header))
            return header.MediaType.Equals("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase);

        return false;
    }

    /// <summary>
    /// Sets the response no cache.
    /// </summary>
    /// <param name="response">The response.</param>
    internal static void SetResponseNoCache(this HttpResponse response)
    {
        response.Headers["Cache-Control"] = "no-store, no-cache, max-age=0";
        response.Headers["Pragma"] = "no-cache";
    }

    /// <summary>
    /// Sets the response cache.
    /// </summary>
    /// <param name="response">The response.</param>
    /// <param name="expiry">The expiry.</param>
    internal static void SetResponseCache(this HttpResponse response, int expiry)
    {
        if (expiry <= 0)
            response.SetResponseNoCache();
        else if (!response.Headers.ContainsKey("Cache-Control"))
            response.Headers.Append("Cache-Control", $"max-age={expiry}");
    }

    /// <summary>
    /// Converts the collectio.
    /// </summary>
    /// <returns>The operation result.</returns>
    internal static Dictionary<string, string> ConvertCollection(
        this IEnumerable<KeyValuePair<string, StringValues>> collection)
    {
        var keyValues = new Dictionary<string, string>();
        foreach (var field in collection)
            if (!keyValues.ContainsKey(field.Key))
                keyValues.Add(field.Key, field.Value.First());

        return keyValues;
    }

    /// <summary>
    /// Converts the collectio.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <returns>The operation result.</returns>
    internal static Dictionary<string, string> ConvertCollection(this IDictionary<string, StringValues> collection)
    {
        var keyValues = new Dictionary<string, string>();
        foreach (var field in collection)
            if (!keyValues.ContainsKey(field.Key))
                keyValues.Add(field.Key, field.Value.First());

        return keyValues;
    }

    /// <summary>
    /// Converts the collectio.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <returns>The operation result.</returns>
    internal static Dictionary<string, object> ConvertCollection(this List<Claim> collection)
    {
        var keyValues = new Dictionary<string, object>();
        var distinctClaims = collection.Distinct(new ClaimsComparer());

        foreach (var claim in distinctClaims)
            if (!keyValues.ContainsKey(claim.Type))
            {
                keyValues.Add(claim.Type, GetClaimValue(claim));
            }
            else
            {
                var value = keyValues[claim.Type];
                if (value is List<object> list)
                {
                    list.Add(GetClaimValue(claim));
                }
                else
                {
                    keyValues.Remove(claim.Type);
                    keyValues.Add(claim.Type, new List<object> { value, GetClaimValue(claim) });
                }
            }

        return keyValues;
    }

    /// <summary>
    /// Retrieves the h c l c s s f base url.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>The operation result.</returns>
    internal static string GetHCLCSSFBaseUrl(this HttpContext context)
    {
        return context.GetHCLCSSFHost() + context.GetHCLCSSFBasePath();
    }

    /// <summary>
    /// Retrieves the h c l c s s f host.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>The operation result.</returns>
    internal static string GetHCLCSSFHost(this HttpContext context)
    {
        var request = context.Request;
        return request.Scheme + "://" + request.Host.ToUriComponent();
    }

    /// <summary>
    /// Sets the h c l c s s f base path.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="value">The value.</param>
    internal static void SetHCLCSSFBasePath(this HttpContext context, string value)
    {
        if (context != null) context.Items[AuthenticationConstants.EnvironmentPaths.HCLCSSFBasePath] = value;
    }

    /// <summary>
    /// Retrieves the h c l c s s f base path.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>The operation result.</returns>
    internal static string GetHCLCSSFBasePath(this HttpContext context)
    {
        return context.Items[AuthenticationConstants.EnvironmentPaths.HCLCSSFBasePath] as string;
    }

    /// <summary>
    /// Retrieves the h c l c s s f relative path.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="path">The path.</param>
    /// <returns>The operation result.</returns>
    internal static string GetHCLCSSFRelativePath(this HttpContext context, string path)
    {
        if (!path.IsLocalUrl()) return null;

        if (path.StartsWith("~/")) path = path.Substring(1);

        path = context.GetHCLCSSFBaseUrl().IncludeEndSlash() + path.RemoveFrontSlash();
        return path;
    }

    private static object GetClaimValue(Claim claim)
    {
        if (claim.ValueType == ClaimValueTypes.Integer ||
            claim.ValueType == ClaimValueTypes.Integer32)
            if (int.TryParse(claim.Value, out var value))
                return value;

        if (claim.ValueType == ClaimValueTypes.Integer64)
            if (long.TryParse(claim.Value, out var value))
                return value;

        if (claim.ValueType == ClaimValueTypes.Boolean)
            if (bool.TryParse(claim.Value, out var value))
                return value;

        if (claim.ValueType == "json") return JsonSerializer.Deserialize<JsonElement>(claim.Value);

        return claim.Value;
    }

    private static bool AppendString(StringBuilder builder, bool isFirstValue, bool urlEncode, string key, string value)
    {
        var effectiveName = key ?? string.Empty;
        var encodedName = urlEncode ? UrlEncoder.Default.Encode(effectiveName) : effectiveName;

        var effectiveValue = value ?? string.Empty;
        var encodedValue = urlEncode ? UrlEncoder.Default.Encode(effectiveValue) : effectiveValue;
        encodedValue = ConvertEncodedSpaces(encodedValue);

        if (isFirstValue)
            isFirstValue = false;
        else
            builder.Append(UrlParameterSeparator);

        builder.Append(encodedName);
        if (!string.IsNullOrWhiteSpace(encodedValue))
        {
            builder.Append(UrlValueAssignSeparator);
            builder.Append(encodedValue);
        }

        return isFirstValue;
    }

    private static string ConvertEncodedSpaces(string str)
    {
        if (str != null && str.Contains('+')) str = str.Replace("+", "%20");

        return str;
    }
}
