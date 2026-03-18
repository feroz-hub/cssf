/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using AutoMapper;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.DomainServices.Repository.Api;
using HCL.CS.SF.DomainServices.Wrappers;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Validators;
using static HCL.CS.SF.Domain.Constants.Endpoint.AuthenticationConstants;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Validators;

/// <summary>
/// Validates requested OAuth 2.0 scopes against the client's allowed scopes and available resources.
/// Resolves identity resources, API resources/scopes, transaction scopes, and offline_access.
/// Builds the <see cref="AllowedScopesParserModel"/> used during token generation.
/// </summary>
internal class ResourceScopeValidator : IResourceScopeValidator
{
    private readonly IApiResourceRepository apiResourceRepository;
    private readonly IIdentityResourceRepository identityResourceRepository;
    private readonly IMapper mapper;
    private readonly IRoleClaimsRepository roleClaimsRepository;
    private readonly IUserClaimRepository userClaimRepository;
    private readonly UserManagerWrapper<Users> userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceScopeValidator"/> class.
    /// </summary>
    public ResourceScopeValidator(
        IIdentityResourceRepository identityResourceRepository,
        IApiResourceRepository apiResourceRepository,
        UserManagerWrapper<Users> userManager,
        IUserClaimRepository userClaimRepository,
        IRoleClaimsRepository roleClaimsRepository,
        IMapper mapper)
    {
        this.identityResourceRepository = identityResourceRepository;
        this.apiResourceRepository = apiResourceRepository;
        this.userManager = userManager;
        this.userClaimRepository = userClaimRepository;
        this.roleClaimsRepository = roleClaimsRepository;
        this.mapper = mapper;
    }

    /// <summary>
    /// Validates that all requested scopes are present in the client's allowed scopes list.
    /// </summary>
    /// <param name="clientScopes">The client's configured allowed scopes.</param>
    /// <param name="requestedScopes">The scopes requested in the authorization/token request.</param>
    /// <returns>True if all requested scopes are allowed for this client.</returns>
    public async Task<bool> ValidateRequestedScopeWithClientAsync(IList<string> clientScopes,
        IList<string> requestedScopes)
    {
        var returnValue = false;
        foreach (var scope in requestedScopes)
            if (clientScopes.Contains(scope))
                returnValue = true;
            else
                return false;

        return await Task.FromResult(returnValue);
    }

    /// <summary>
    /// Parses and validates the requested scopes against identity resources, API resources,
    /// API scopes, transaction scopes, and offline_access. Populates the scope parser model
    /// with token generation details including user claims and role claim types.
    /// </summary>
    /// <param name="resourceScopeModel">The resource scope model with requested scopes and client info.</param>
    /// <returns>An <see cref="AllowedScopesParserModel"/> with parsed scope information for token generation.</returns>
    public async Task<AllowedScopesParserModel> ValidateRequestedScopesAsync(ResourceScopeModel resourceScopeModel)
    {
        var parser = new AllowedScopesParserModel();
        var tokenDetails = await GetTokenDetailsAsync(resourceScopeModel);
        tokenDetails.Client = resourceScopeModel.Client;
        parser.TokenDetails = tokenDetails;

        if (resourceScopeModel.RequestedScope != null && resourceScopeModel.RequestedScope.Count > 0)
        {
            // Parse the allowed scopes and get the list of scopes for IdentityResources
            parser.ParsedIdentityResources = tokenDetails.IdentityResourcesByScopes
                .Select(resource => resource.IdentityResourceName).Distinct().ToList();
            resourceScopeModel.RequestedScope =
                resourceScopeModel.RequestedScope.Except(parser.ParsedIdentityResources).ToList();

            // Parse the allowedScopes and get the apiScopes with resource Names
            parser.ParsedApiResources = tokenDetails.ApiResourcesByScopes.Select(resource => resource.ApiResourceName)
                .Distinct().ToList();
            parser.ParsedApiScopes = tokenDetails.ApiResourcesByScopes.Select(resource => resource.ApiScopeName)
                .Distinct().ToList();
            var apiScopesWithResourceNames = parser.ParsedApiResources.Union(parser.ParsedApiScopes);
            resourceScopeModel.RequestedScope =
                resourceScopeModel.RequestedScope.Except(apiScopesWithResourceNames).ToList();

            // Parse the transaction scopes
            var transactionScopes = await ParseTransactionScopesAsync(resourceScopeModel.RequestedScope);
            resourceScopeModel.RequestedScope = resourceScopeModel.RequestedScope.Except(transactionScopes).ToList();

            // Parse for Offline Access
            var offlineAccessScope = await ParseOfflineAccessScopesAsync(resourceScopeModel.RequestedScope);
            resourceScopeModel.RequestedScope = resourceScopeModel.RequestedScope.Except(offlineAccessScope).ToList();

            if (resourceScopeModel.RequestedScope.Count > 0)
            {
                parser.InvalidScopes = resourceScopeModel.RequestedScope;

                parser.ParsedIdentityResources = null;
                parser.ParsedApiResources = null;
                parser.ParsedApiScopes = null;
            }
            else
            {
                parser.ParsedTransactionScopes = transactionScopes;

                parser.AllowOfflineAccess = tokenDetails.Client.AllowOfflineAccess && offlineAccessScope.Count >= 1;

                parser.CreateIdentityToken = false;
                if (parser.ParsedIdentityResources.FindIndex(x =>
                        x.Equals(IdentityScopes.OpenId, StringComparison.OrdinalIgnoreCase)) !=
                    -1) parser.CreateIdentityToken = true;
            }
        }

        return parser;
    }

    // TODO - need to analyze on implementation
    private static Task<List<string>> ParseTransactionScopesAsync(List<string> requestedScopeList)
    {
        var allowedScopes = new List<string>();
        if (requestedScopeList.Contains("transaction"))
            foreach (var scope in requestedScopeList)
                if (scope.Contains(":"))
                    allowedScopes.Add(scope);

        return Task.FromResult(allowedScopes);
    }

    private static Task<List<string>> ParseOfflineAccessScopesAsync(List<string> requestedScopeList)
    {
        var allowedScopes = new List<string>();
        if (requestedScopeList.Contains(IdentityScopes.OfflineAccess))
            foreach (var scope in requestedScopeList)
                allowedScopes.Add(scope);

        return Task.FromResult(allowedScopes.Distinct().ToList());
    }

    /// <summary>
    /// Loads token generation details by resolving identity resources, API resources/scopes,
    /// user claims, and role claim types for the requested scopes and user.
    /// </summary>
    /// <param name="resourceScopeModel">The resource scope model with requested scopes and user info.</param>
    /// <returns>A <see cref="TokenDetailsModel"/> with all resources and user data needed for token generation.</returns>
    private async Task<TokenDetailsModel> GetTokenDetailsAsync(ResourceScopeModel resourceScopeModel)
    {
        var tokenDetails = new TokenDetailsModel();
        if (resourceScopeModel.RequestedScope.ContainsAny())
        {
            // get the identity resources
            var identityResourcesEntity =
                await identityResourceRepository.GetAllIdentityResourcesByScopesAsync(resourceScopeModel
                    .RequestedScope);
            if (identityResourcesEntity.ContainsAny())
                tokenDetails.IdentityResourcesByScopes = identityResourcesEntity.ToList();

            // get the api resources
            var apiResourcesEntity =
                await apiResourceRepository.GetAllApiResourcesByScopesAsync(resourceScopeModel.RequestedScope);
            if (apiResourcesEntity.ContainsAny()) tokenDetails.ApiResourcesByScopes = apiResourcesEntity.ToList();
        }

        tokenDetails.IdentityResourcesByScopes = tokenDetails.IdentityResourcesByScopes.ContainsAny()
            ? tokenDetails.IdentityResourcesByScopes
            : new List<IdentityResourcesByScopesModel>();
        tokenDetails.ApiResourcesByScopes = tokenDetails.ApiResourcesByScopes.ContainsAny()
            ? tokenDetails.ApiResourcesByScopes
            : new List<ApiResourcesByScopesModel>();

        if (!string.IsNullOrWhiteSpace(resourceScopeModel.UserName))
        {
            // get the user model
            tokenDetails.User = await GetByIdAsync(resourceScopeModel.UserName);
            if (tokenDetails.User != null)
            {
                tokenDetails.UserRoleClaimTypes =
                    await roleClaimsRepository.GetRolesAndClaimsForUser(tokenDetails.User.Id);
                tokenDetails.UserRoleClaimTypes = tokenDetails.UserRoleClaimTypes.ContainsAny()
                    ? tokenDetails.UserRoleClaimTypes
                    : new List<UserRoleClaimTypesModel>();
            }
        }

        return tokenDetails;
    }

    /// <summary>
    /// Retrieves a user model by username, including any custom user claims from the claims repository.
    /// </summary>
    /// <param name="userName">The username to look up.</param>
    /// <returns>A <see cref="UserModel"/> with claims, or null if the user is not found.</returns>
    private async Task<UserModel> GetByIdAsync(string userName)
    {
        var user = await userManager.FindByNameAsync(userName);
        if (user != null)
        {
            var userModel = mapper.Map<Users, UserModel>(user);
            var userClaims = await userClaimRepository.GetClaimsAsync(user.Id);
            if (userClaims.ContainsAny())
            {
                var userClaimModel = mapper.Map<IList<UserClaims>, IList<UserClaimModel>>(userClaims);
                userModel.UserClaims = (List<UserClaimModel>)userClaimModel;
            }

            return userModel;
        }

        return null;
    }
}
