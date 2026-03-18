/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Microsoft.IdentityModel.Tokens;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.ProxyService.Extension;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api.Wrapper;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;

namespace HCL.CS.SF.ProxyService.Validator;

/// <summary>
/// Validates inbound API requests by checking whether the caller has the required
/// permissions based on the bearer token claims and the route's permission configuration.
/// Anonymous APIs bypass permission checks entirely.
/// </summary>
public sealed class ApiValidator(
    IHttpContextAccessor httpContextAccessor,
    IFrameworkResultService frameworkResultService,
    ISessionManagementService sessionManagementService)
    : IApiValidator
{
    /// <summary>
    /// Session management service for managing user sessions during validation.
    /// </summary>
    private readonly ISessionManagementService sessionManagementService = sessionManagementService;

    /// <summary>
    /// Validates the current HTTP request by determining whether the calling method
    /// requires authorization and, if so, whether the caller's token contains sufficient permissions.
    /// </summary>
    /// <param name="callerMemberName">
    /// Automatically captured name of the calling method, used to look up the route's permission requirements.
    /// </param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or an authorization failure.</returns>
    public async Task<FrameworkResult> ValidateRequest(
        [CallerMemberName] string callerMemberName = null)
    {
        if (!ProxyConstants.AnonymousApis.Contains(callerMemberName))
            // Check is user has permission to access api.
            return await IsUserHasPermission(callerMemberName);

        return frameworkResultService.Succeeded();
    }

    /// <summary>
    /// Determines whether the current user has the required permissions for the specified API method.
    /// Extracts the bearer token from the Authorization header, decodes its claims, and compares
    /// them against the permissions defined on the API route model.
    /// </summary>
    /// <param name="callerMemberName">The name of the API method to check permissions for.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or an access-denied failure.</returns>
    private Task<FrameworkResult> IsUserHasPermission(string callerMemberName)
    {
        // Look up the API route definition by the calling method name.
        var api = ApiRoutePathConstants.ApiRouteModels.Find(x => x.Name == callerMemberName);
        if (api != null && api.Permissions != null && api.Permissions.Count > 0)
        {
            var anonymousPermission =
                api.Permissions.Where(x => x.Contains(ApiPermissionConstants.Anonymous)).ToList();
            if (anonymousPermission.Count > 0)
                // Requested api is anonymous, hence allowing permission.
                return Task.FromResult(frameworkResultService.Succeeded());
        }
        else
        {
            return Task.FromResult(frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.NotFound));
        }

        // Check is user has permission to access api.
        var authorization = httpContextAccessor.HttpContext?.Request?.Headers[HeaderNames.Authorization];
        if (!AuthenticationHeaderValue.TryParse(authorization, out var headerValue))
            return Task.FromResult(
                frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.UnauthorizedAccess));

        // Decode and normalize all claims from the JWT payload without signature validation.
        var tokenClaims = GetNormalizedTokenClaims(headerValue.Parameter);
        if (tokenClaims.Count <= 0) return Task.FromResult(frameworkResultService.Succeeded());

        // Expand "manage" permissions into read/write/delete equivalents, then intersect with token claims.
        var permissionsList = api.Permissions
            .ExpandPermissions()
            .Select(permission => permission.ToLowerInvariant())
            .ToList();
        var claimsList = tokenClaims.ToList().ExpandPermissions();

        var permissions = permissionsList.Intersect(claimsList).ToList();
        if (permissions.Count <= 0)
            return Task.FromResult(frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.AccessDenied));

        return Task.FromResult(frameworkResultService.Succeeded());
    }

    /// <summary>
    /// Extracts all claim values from the JWT payload and normalizes them to lowercase
    /// for case-insensitive permission comparison.
    /// </summary>
    /// <param name="token">The raw JWT string (without the "Bearer " prefix).</param>
    /// <returns>A set of normalized claim values from the token payload.</returns>
    private static HashSet<string> GetNormalizedTokenClaims(string token)
    {
        var tokenClaims = new HashSet<string>(StringComparer.Ordinal);
        foreach (var normalizedValue in ReadTokenValues(token))
        {
            if (!string.IsNullOrWhiteSpace(normalizedValue))
                tokenClaims.Add(normalizedValue.ToLowerInvariant());
        }

        return tokenClaims;
    }

    /// <summary>
    /// Reads and yields individual claim values from the JWT payload by Base64-decoding
    /// the second segment of the token and parsing the resulting JSON.
    /// </summary>
    /// <param name="token">The raw JWT string.</param>
    /// <returns>An enumerable of claim value strings extracted from the token.</returns>
    private static IEnumerable<string> ReadTokenValues(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            yield break;

        // JWT tokens have three dot-separated segments: header.payload.signature
        var sections = token.Split('.');
        if (sections.Length < 2)
            yield break;

        JsonDocument document;
        try
        {
            var payloadBytes = Base64UrlEncoder.DecodeBytes(sections[1]);
            document = JsonDocument.Parse(payloadBytes);
        }
        catch
        {
            yield break;
        }

        using (document)
        {
            foreach (var claim in document.RootElement.EnumerateObject())
            {
                foreach (var value in ExpandClaimValues(claim.Name, claim.Value))
                    yield return value;
            }
        }
    }

    /// <summary>
    /// Recursively expands a single JWT claim value into individual string values.
    /// Array claims are flattened, and scope/permission claims with space-separated values
    /// are split into discrete entries.
    /// </summary>
    /// <param name="claimType">The claim type (e.g., "scope", "permission").</param>
    /// <param name="claimValue">The JSON element representing the claim value.</param>
    /// <returns>An enumerable of individual claim value strings.</returns>
    private static IEnumerable<string> ExpandClaimValues(string claimType, JsonElement claimValue)
    {
        switch (claimValue.ValueKind)
        {
            case JsonValueKind.Array:
                foreach (var value in claimValue.EnumerateArray())
                {
                    foreach (var normalizedValue in ExpandClaimValues(claimType, value))
                        yield return normalizedValue;
                }

                yield break;

            case JsonValueKind.String:
                var stringValue = claimValue.GetString();
                if (string.IsNullOrWhiteSpace(stringValue))
                    yield break;

                // Scope and permission claims may contain space-delimited values per OAuth 2.0 convention.
                if (claimType == OpenIdConstants.ClaimTypes.Scope ||
                    claimType == OpenIdConstants.ClaimTypes.Permission)
                {
                    foreach (var value in stringValue.Split(
                                 new[] { ' ' },
                                 StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                        yield return value;

                    yield break;
                }

                yield return stringValue;
                yield break;

            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
                yield return claimValue.ToString();
                yield break;
        }
    }
}
