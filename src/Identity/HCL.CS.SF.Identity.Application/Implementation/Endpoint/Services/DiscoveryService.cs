/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Linq.Expressions;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.DomainServices.Repository.Api;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;
using static HCL.CS.SF.Domain.Constants.Endpoint.OpenIdConstants;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Services;

/// <summary>
/// Service that generates the OpenID Connect Discovery metadata document.
/// Aggregates server capabilities including supported scopes, claims, grant types,
/// response types, signing algorithms, and endpoint URLs per RFC 8414 and OpenID Connect Discovery 1.0.
/// </summary>
internal class DiscoveryService : SecurityBase, IDiscoveryService
{
    private readonly IApiResourceRepository apiResourceRepository;
    private readonly TokenSettings configSettings;
    private readonly IIdentityResourceRepository identityResourceRepository;
    private readonly Dictionary<string, AsymmetricKeyInfoModel> keyStore;
    private readonly ILoggerService loggerService;
    private List<string> supportedClaimsList;
    private List<string> supportedScopesList;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscoveryService"/> class.
    /// </summary>
    public DiscoveryService(
        ILoggerInstance instance,
        IIdentityResourceRepository identityResourceRepository,
        IApiResourceRepository apiResourceRepository,
        Dictionary<string, AsymmetricKeyInfoModel> keyStore,
        HCLCSSFConfig tokenSettings)
    {
        this.identityResourceRepository = identityResourceRepository;
        this.apiResourceRepository = apiResourceRepository;
        this.keyStore = keyStore;
        configSettings = tokenSettings.TokenSettings;
        loggerService = instance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
        supportedClaimsList = new List<string>();
        supportedScopesList = new List<string>();
    }

    /// <summary>
    /// Generates the complete OpenID Provider metadata dictionary.
    /// Includes issuer, endpoints, supported scopes/claims, grant types, response types/modes,
    /// token endpoint auth methods, signing algorithms, code challenge methods, and logout support flags.
    /// </summary>
    /// <param name="request">The discovery request containing the base URL.</param>
    /// <returns>A dictionary of metadata key-value pairs for the discovery document.</returns>
    public async Task<Dictionary<string, object>> GenerateDiscoveryMetaData(DiscoveryRequestModel request)
    {
        loggerService.WriteTo(Log.Debug, "Entered into generate discovery metadata.");
        await GetSupportedClaims();
        var tokenAuthMethods = new[]
        {
            AuthenticationMethods.ClientSecretBasic,
            AuthenticationMethods.ClientSecretPost
        };
        var responseTypes = new[]
        {
            ResponseTypes.Code
        };
        var responseModes = new[]
        {
            ResponseModes.Query,
            ResponseModes.FormPost
        };
        var grantTypes = new[]
        {
            GrantTypes.AuthorizationCode,
            GrantTypes.RefreshToken,
            GrantTypes.ClientCredentials,
            GrantTypes.Password,
            GrantTypes.UserCode
        };
        var idTokenSigningAlgorithms = keyStore.Keys
            .Where(algorithm => !string.IsNullOrWhiteSpace(algorithm))
            .Distinct()
            .ToArray();
        if (idTokenSigningAlgorithms.Length == 0) idTokenSigningAlgorithms = new[] { Algorithms.RsaSha256 };

        var codeChallengeMethods = new[]
        {
            CodeChallengeMethods.Sha256
        };

        var metaData = new Dictionary<string, object>
        {
            // Issuer
            { "issuer", configSettings.TokenConfig.IssuerUri },

            // Token endpoint auth methods (RFC 8414)
            { "token_endpoint_auth_methods_supported", tokenAuthMethods },

            // Supported modes
            { "scopes_supported", supportedScopesList.ToArray() },
            { "claims_supported", supportedClaimsList.ToArray() },
            { "response_types_supported", responseTypes },
            { "response_modes_supported", responseModes },
            { "grant_types_supported", grantTypes },
            { "subject_types_supported", new[] { "public" } },

            { "id_token_signing_alg_values_supported", idTokenSigningAlgorithms },
            { "code_challenge_methods_supported", codeChallengeMethods }
        };

        if (configSettings.EndpointsConfig.EnableAuthorizeEndpoint)
            metaData.Add("authorization_endpoint", request.BaseUrl + EndpointRoutePaths.Authorize);

        if (configSettings.EndpointsConfig.EnableTokenEndpoint)
            metaData.Add("token_endpoint", request.BaseUrl + EndpointRoutePaths.Token);

        if (configSettings.EndpointsConfig.EnableIntrospectionEndpoint)
            metaData.Add("introspection_endpoint", request.BaseUrl + EndpointRoutePaths.Introspection);

        if (configSettings.EndpointsConfig.EnableJWKSEndpoint && keyStore.ContainsAny())
            metaData.Add("jwks_uri", request.BaseUrl.RemoveBackSlash() + EndpointRoutePaths.JWKSWebKeys);

        if (configSettings.EndpointsConfig.EnableUserInfoEndpoint)
            metaData.Add("userinfo_endpoint", request.BaseUrl + EndpointRoutePaths.UserInfo);

        if (configSettings.EndpointsConfig.EnableTokenRevocationEndpoint)
            metaData.Add("revocation_endpoint", request.BaseUrl + EndpointRoutePaths.Revocation);

        if (configSettings.EndpointsConfig.EnableEndSessionEndpoint)
            metaData.Add("end_session_endpoint", request.BaseUrl + EndpointRoutePaths.EndSession);

        if (configSettings.EndpointsConfig.FrontchannelLogoutSupported)
            metaData.Add("frontchannel_logout_supported", true);

        if (configSettings.EndpointsConfig.FrontchannelLogoutSessionRequired)
            metaData.Add("frontchannel_logout_session_supported", true);

        if (configSettings.EndpointsConfig.BackchannelLogoutSupported)
            metaData.Add("backchannel_logout_supported", true);

        if (configSettings.EndpointsConfig.BackchannelLogoutSessionRequired)
            metaData.Add("backchannel_logout_session_supported", true);

        return metaData;
    }

    /// <summary>
    /// Loads all supported claims and scopes from identity resources, API resources, and API scopes.
    /// </summary>
    private async Task GetSupportedClaims()
    {
        supportedClaimsList.Clear();
        supportedScopesList.Clear();
        var identityResources = await identityResourceRepository.GetAllAsync(new Expression<Func<IdentityResources, object>>[] { x => x.IdentityClaims });
        if (identityResources.ContainsAny())
            foreach (var identityResource in identityResources)
            {
                supportedClaimsList.AddRange(identityResource.IdentityClaims.ConvertAll(x => x.Type));
                supportedScopesList.Add(identityResource.Name);
            }

        var apiResources = await apiResourceRepository.GetAllApiResourcesAsync();
        if (apiResources.ContainsAny())
            foreach (var apiResource in apiResources)
                supportedClaimsList.AddRange(apiResource.ApiResourceClaims.ConvertAll(x => x.Type));

        var apiScopes = await apiResourceRepository.GetAllApiScopesAsync();
        if (apiScopes.ContainsAny())
            foreach (var apiScope in apiScopes)
            {
                supportedClaimsList.AddRange(apiScope.ApiScopeClaims.ConvertAll(x => x.Type));
                supportedScopesList.Add(apiScope.Name);
            }

        supportedClaimsList = supportedClaimsList.Distinct().ToList();
        supportedScopesList = supportedScopesList.Distinct().ToList();
    }
}
