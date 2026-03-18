/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Validation;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Extensions;

/// <summary>
/// General-purpose string utility extension methods for OAuth/OIDC endpoint processing.
/// Includes URL manipulation, scope parsing, constant-time string comparison,
/// claim extraction, permission expansion/shrinking, and collection helpers.
/// </summary>
internal static class StringExtension
{
    /// <summary>
    /// Performs the included front slash operation.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <returns>The operation result.</returns>
    internal static string IncludedFrontSlash(this string url)
    {
        if (url != null && !url.StartsWith("/")) return "/" + url;

        return url;
    }

    /// <summary>
    /// Performs the include end slash operation.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <returns>The operation result.</returns>
    internal static string IncludeEndSlash(this string url)
    {
        if (url != null && !url.EndsWith("/")) return url + "/";

        return url;
    }

    /// <summary>
    /// Deletes the specified front slash.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <returns>The operation result.</returns>
    internal static string RemoveFrontSlash(this string url)
    {
        if (url != null && url.StartsWith("/")) url = url[1..];

        return url;
    }

    /// <summary>
    /// Deletes the specified back slash.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <returns>The operation result.</returns>
    internal static string RemoveBackSlash(this string url)
    {
        if (url != null && url.EndsWith("/")) url = url[..^1];

        return url;
    }

    /// <summary>
    /// Retrieves the space separated value.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The operation result.</returns>
    internal static string GetSpaceSeparatedValues(this Type type)
    {
        var authenticationMethods = type
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly) // constants, not readonly
            .Where(fi => fi.FieldType == typeof(string)) // of type string
            .ToDictionary(fi => fi.Name, fi => fi.GetValue(null) as string);
        return authenticationMethods.Values.AsEnumerable().ConvertSpaceSeparatedString();
    }

    /// <summary>
    /// Retrieves the arra.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The operation result.</returns>
    internal static string[] GetArray(this Type type)
    {
        var authenticationMethods = type
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly) // constants, not readonly
            .Where(fi => fi.FieldType == typeof(string)) // of type string
            .ToDictionary(fi => fi.Name, fi => fi.GetValue(null) as string);
        return authenticationMethods.Values.ToArray();
    }

    /// <summary>
    /// Performs the compare string operation.
    /// </summary>
    /// <param name="string1">The string1.</param>
    /// <param name="string2">The string2.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    internal static bool CompareStrings(this string string1, string string2)
    {
        if (string1 == null && string2 == null) return true;

        if (string1 == null || string2 == null) return false;

        var string1Bytes = Encoding.UTF8.GetBytes(string1);
        var string2Bytes = Encoding.UTF8.GetBytes(string2);
        if (string1Bytes.Length != string2Bytes.Length) return false;

        return CryptographicOperations.FixedTimeEquals(string1Bytes, string2Bytes);
    }

    /// <summary>
    /// Converts the space separated string.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <returns>The operation result.</returns>
    internal static string ConvertSpaceSeparatedString(this IEnumerable<string> list)
    {
        if (list == null) return string.Empty;

        var sb = new StringBuilder(100);
        foreach (var element in list) sb.Append(element + " ");

        return sb.ToString().Trim();
    }

    /// <summary>
    /// Converts the space separated string.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <returns>The operation result.</returns>
    internal static string ConvertSpaceSeparatedString(this IEnumerable<Claim> list)
    {
        if (list == null) return string.Empty;

        var sb = new StringBuilder(100);
        foreach (var element in list) sb.Append(element.Value + " ");

        return sb.ToString().Trim();
    }

    /// <summary>
    /// Parses the scopes string.
    /// </summary>
    /// <param name="scopes">The scopes.</param>
    /// <returns>The matching collection of results.</returns>
    internal static IEnumerable<string> ParseScopesString(this string scopes)
    {
        if (string.IsNullOrWhiteSpace(scopes)) return Enumerable.Empty<string>();

        scopes = scopes.Trim();
        var parsedScopes = scopes.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();
        if (parsedScopes.ContainsAny())
        {
            parsedScopes.Sort();
            return parsedScopes;
        }

        return Enumerable.Empty<string>();
    }

    /// <summary>
    /// Performs the split by space operation.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>The matching collection of results.</returns>
    internal static IEnumerable<string> SplitBySpace(this string input)
    {
        if (!string.IsNullOrWhiteSpace(input))
        {
            input = input.Trim();
            return input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        return Enumerable.Empty<string>();
    }

    /// <summary>
    /// Retrieves the value from dictionar.
    /// </summary>
    /// <param name="requestCollection">The request collection.</param>
    /// <param name="key">The key.</param>
    /// <returns>The operation result.</returns>
    internal static string GetValueFromDictionary(this Dictionary<string, string> requestCollection, string key)
    {
        if (!string.IsNullOrWhiteSpace(key) && requestCollection.ContainsKey(key) &&
            requestCollection.TryGetValue(key, out var value)) return value.Trim();

        return null;
    }

    /// <summary>
    /// Retrieves the value.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="key">The key.</param>
    /// <returns>The operation result.</returns>
    internal static string GetValue(this ValidatedBaseModel request, string key)
    {
        if (!string.IsNullOrWhiteSpace(key) && request.RequestRawData.ContainsKey(key) &&
            request.RequestRawData.TryGetValue(key, out var value)) return value.Trim();

        return string.Empty;
    }

    /// <summary>
    /// Retrieves the authentication time from identit.
    /// </summary>
    /// <param name="principal">The principal.</param>
    /// <returns>The operation result.</returns>
    internal static DateTime? GetAuthenticationTimeFromIdentity(this IPrincipal principal)
    {
        if (principal != null && principal.Identity != null)
        {
            var id = principal.Identity as ClaimsIdentity;
            var claim = id?.FindFirst(OpenIdConstants.ClaimTypes.AuthenticationTime);
            if (claim != null)
            {
                var value = long.Parse(claim.Value);
                return DateTimeOffset.FromUnixTimeSeconds(value).UtcDateTime;
            }
        }

        return null;
    }

    /// <summary>
    /// Checks the local url.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    internal static bool CheckLocalUrl(this string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;

        if (url[0] == '/')
        {
            if (url.Length == 1) return true;

            if (url[1] != '/' && url[1] != '\\') return true;

            return false;
        }

        if (url[0] == '~' && url.Length > 1 && url[1] == '/')
        {
            if (url.Length == 2) return true;

            if (url[2] != '/' && url[2] != '\\') return true;

            return false;
        }

        return false;
    }

    /// <summary>
    /// Determines whether expired.
    /// </summary>
    /// <param name="createdTime">The created time.</param>
    /// <param name="expiryDuration">The expiry duration.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    internal static bool IsExpired(this DateTime createdTime, long expiryDuration)
    {
        if (createdTime.AddSeconds(expiryDuration) < DateTime.UtcNow) return true;

        return false;
    }

    /// <summary>
    /// Determines whether authenticated.
    /// </summary>
    /// <param name="principal">The principal.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    internal static bool IsAuthenticated(this IPrincipal principal)
    {
        return principal != null && principal.Identity != null && principal.Identity.IsAuthenticated;
    }

    /// <summary>
    /// Performs the contai operation.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    internal static bool ContainsAny<T>(this IEnumerable<T> data)
    {
        return data != null && data.Any();
    }

    /// <summary>
    /// Determines whether valid url.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    internal static bool IsValidUrl(this string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;

        if (url.Contains('*', StringComparison.Ordinal)) return false;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var validatedUri)) return false;

        var allowInsecureHttpDev = string.Equals(
            Environment.GetEnvironmentVariable("HCL.CS.SF_ALLOW_INSECURE_HTTP_DEV"),
            "true",
            StringComparison.OrdinalIgnoreCase);

        var isCustomScheme = validatedUri.Scheme != Uri.UriSchemeHttps
                             && validatedUri.Scheme != Uri.UriSchemeHttp;

        return validatedUri.IsWellFormedOriginalString()
               && validatedUri.IsAbsoluteUri
               && (validatedUri.Scheme == Uri.UriSchemeHttps
                   || (allowInsecureHttpDev && validatedUri.Scheme == Uri.UriSchemeHttp)
                   || (allowInsecureHttpDev && isCustomScheme && !string.IsNullOrWhiteSpace(validatedUri.Host)))
               && !string.IsNullOrWhiteSpace(validatedUri.Host);
    }

    /// <summary>
    /// Determines whether null.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    internal static bool IsNull<T>(this IEnumerable<T> data)
    {
        if (data != null && data.Any())
        {
            foreach (var item in data)
                if (item == null)
                    return true;

            return false;
        }

        return true;
    }

    /// <summary>
    /// Expands the permissio.
    /// </summary>
    /// <param name="permissions">The permissions.</param>
    /// <returns>The matching collection of results.</returns>
    internal static List<string> ExpandPermissions(this List<string> permissions)
    {
        var newPermissionsList = new List<string>();
        if (permissions.ContainsAny())
            foreach (var scope in permissions)
                if (scope.Contains(PermissionConstants.Manage))
                {
                    newPermissionsList.Add(scope.Replace(PermissionConstants.Manage, PermissionConstants.Read));
                    newPermissionsList.Add(scope.Replace(PermissionConstants.Manage, PermissionConstants.Write));
                    newPermissionsList.Add(scope.Replace(PermissionConstants.Manage, PermissionConstants.Delete));
                }
                else
                {
                    newPermissionsList.Add(scope);
                }

        return newPermissionsList.Distinct().ToList();
    }

    /// <summary>
    /// Shrinks the permissio.
    /// </summary>
    /// <param name="permissions">The permissions.</param>
    /// <returns>The matching collection of results.</returns>
    internal static List<string> ShrinkPermissions(this List<string> permissions)
    {
        var newPermissionsList = new List<string>();
        var scopeSeparator = ".";
        if (permissions.ContainsAny())
            foreach (var scope in permissions)
            {
                var splitScope = scope.Split(scopeSeparator);
                var scopePrefix = splitScope[0] + scopeSeparator + splitScope[1];
                var readScope = scopePrefix + PermissionConstants.Read;
                var writeScope = scopePrefix + PermissionConstants.Write;
                var deleteScope = scopePrefix + PermissionConstants.Delete;
                var manageScope = scopePrefix + PermissionConstants.Manage;
                if (permissions.Contains(readScope) && permissions.Contains(writeScope) &&
                    permissions.Contains(deleteScope))
                {
                    newPermissionsList.Add(manageScope);
                }
                else
                {
                    if (permissions.Contains(manageScope) && permissions.Contains(scope))
                        newPermissionsList.Add(manageScope);
                    else
                        newPermissionsList.Add(scope);
                }
            }

        return newPermissionsList.Distinct().ToList();
    }
}
