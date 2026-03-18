/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using DomainValidation.Interfaces.Specification;
using DomainValidation.Validation;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Configurations.Endpoint;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Repository.Api;
using HCL.CS.SF.DomainServices.UnitOfWork.Endpoint;
using HCL.CS.SF.Service.Extension;
using HCL.CS.SF.Service.Implementation.Api.Validators;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;

namespace HCL.CS.SF.Service.Implementation.Api.Specifications;

/// <summary>
/// Comprehensive validator for OAuth2/OIDC client registration and update models.
/// Validates client name, grant types, response types, redirect URIs, post-logout redirect URIs,
/// PKCE requirements for public clients, token expiration ranges, signing algorithms,
/// and allowed scopes against registered API resources and identity resources.
/// </summary>

internal sealed class ClientModelSpecification : BaseDomainModelValidator<ClientsModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClientModelSpecification"/> class.
    /// </summary>
    internal ClientModelSpecification(IClientsUnitOfWork unitOfWork, TokenExpiration tokenExpiration, CrudMode crudMode,
        IApiResourceRepository apiResourceRepository, IRepository<ApiScopes> apiScopeRepository,
        IIdentityResourceRepository identityResourceRepository)
    {
        // Client
        Add("CheckClientName", new Rule<ClientsModel>(
            new IsNotNull<ClientsModel>(model => model.ClientName),
            EndpointErrorCodes.ClientNameIsRequired));
        Add("CheckClientCreatedByLength", new Rule<ClientsModel>(
            new IsValid255CharLength<ClientsModel>(model => model.CreatedBy),
            ApiErrorCodes.CreatedByTooLong));
        Add("CheckTermsOfServiceURI", new Rule<ClientsModel>(
            new IsValidUri<ClientsModel>(model => model.TermsOfServiceUri),
            EndpointErrorCodes.InvalidTermsOfService));
        Add("CheckValidLogoURI", new Rule<ClientsModel>(
            new IsValidUri<ClientsModel>(model => model.LogoUri),
            EndpointErrorCodes.InvalidLogoUri));
        Add("CheckValidClientURI", new Rule<ClientsModel>(
            new IsValidUri<ClientsModel>(model => model.ClientUri),
            EndpointErrorCodes.InvalidClientUri));
        Add("CheckValidPolicyURI", new Rule<ClientsModel>(
            new IsValidUri<ClientsModel>(model => model.PolicyUri),
            EndpointErrorCodes.InvalidPolicyUri));
        Add("CheckAllowedScopes", new Rule<ClientsModel>(
            new AreNotNull<ClientsModel>(model => model.AllowedScopes),
            EndpointErrorCodes.AllowedScopesIsRequired));
        Add("CheckSupportedGrantTypes", new Rule<ClientsModel>(
            new AreNotNull<ClientsModel>(model => model.SupportedGrantTypes),
            EndpointErrorCodes.SupportedGrantTypesIsRequired));
        Add("CheckSupportedGrantTypesProfile", new Rule<ClientsModel>(
            new CheckSupportedGrantTypesProfile(),
            EndpointErrorCodes.InvalidGrantTypeForClient));
        Add("CheckSupportedResponseTypes", new Rule<ClientsModel>(
            new AreNotNull<ClientsModel>(model => model.SupportedResponseTypes),
            EndpointErrorCodes.SupportedResponseTypesIsRequired));
        Add("CheckSupportedResponseTypesProfile", new Rule<ClientsModel>(
            new CheckSupportedResponseTypesProfile(),
            EndpointErrorCodes.ResponseTypeMissing));
        Add("CheckPkceRequirementForPublicClient", new Rule<ClientsModel>(
            new CheckPkceRequirementForPublicClient(),
            EndpointErrorCodes.InvalidCodeChallenge));
        Add("CheckRedirectUrisForAuthorizationCode", new Rule<ClientsModel>(
            new CheckRedirectUrisForAuthorizationCode(),
            EndpointErrorCodes.RedirectURIIsMandatory));
        Add("CheckClientNameLength", new Rule<ClientsModel>(
            new IsValid255CharLength<ClientsModel>(model => model.ClientName),
            EndpointErrorCodes.ClientNameTooLong));

        Add("CheckAccessTokenExpirationRange", new Rule<ClientsModel>(
            new CheckTokenExpirationRange<ClientsModel>(model => model.AccessTokenExpiration,
                OpenIdConstants.TokenType.AccessToken, tokenExpiration),
            EndpointErrorCodes.InvalidAccessTokenExpireRange));
        Add("CheckIdentityTokenExpirationRange", new Rule<ClientsModel>(
            new CheckTokenExpirationRange<ClientsModel>(model => model.IdentityTokenExpiration,
                OpenIdConstants.TokenType.IdentityToken, tokenExpiration),
            EndpointErrorCodes.InvalidIdentityTokenExpireRange));
        Add("CheckRefreshTokenExpirationRange", new Rule<ClientsModel>(
            new CheckTokenExpirationRange<ClientsModel>(model => model.RefreshTokenExpiration,
                OpenIdConstants.TokenType.RefreshToken, tokenExpiration),
            EndpointErrorCodes.InvalidRefreshTokenExpireRange));
        Add("CheckLogoutTokenExpirationRange", new Rule<ClientsModel>(
            new CheckTokenExpirationRange<ClientsModel>(model => model.LogoutTokenExpiration,
                OpenIdConstants.TokenType.LogoutToken, tokenExpiration),
            EndpointErrorCodes.InvalidLogoutTokenExpireRange));
        Add("CheckAuthorizationCodeExpirationRange", new Rule<ClientsModel>(
            new CheckTokenExpirationRange<ClientsModel>(model => model.AuthorizationCodeExpiration,
                OpenIdConstants.TokenType.AuthorizationCode, tokenExpiration),
            EndpointErrorCodes.InvalidAuthorizationCodeExpireRange));

        Add("CheckAlgorithm", new Rule<ClientsModel>(
            new CheckAlgorithm<ClientsModel>(model => model.AllowedSigningAlgorithm),
            EndpointErrorCodes.SigningAlgorithmIsInvalid));
        Add("CheckValidScopes", new Rule<ClientsModel>(
            new CheckAllowedScope(apiResourceRepository, apiScopeRepository, identityResourceRepository),
            EndpointErrorCodes.InvalidScopeOrNotAllowed));

        // Client Redirect Uris
        Add("CheckValidRedirectURI", new Rule<ClientsModel>(
            new IsValidUris<ClientsModel>(model =>
                model.RedirectUris.ContainsAny() ? model.RedirectUris.ConvertAll(uri => uri.RedirectUri) : null),
            EndpointErrorCodes.InvalidRedirectUri));
        Add("CheckValidRedirectURILength", new Rule<ClientsModel>(
            new IsValid2048CharLengths<ClientsModel>(model =>
                model.RedirectUris.ContainsAny() ? model.RedirectUris.ConvertAll(uri => uri.RedirectUri) : null),
            EndpointErrorCodes.RedirectUriTooLong));
        Add("CheckValidRedirectURICreatedByLength", new Rule<ClientsModel>(
            new IsValid255CharLengths<ClientsModel>(model =>
                model.RedirectUris.ContainsAny() ? model.RedirectUris.ConvertAll(uri => uri.CreatedBy) : null),
            EndpointErrorCodes.RedirectUriCreatedByTooLong));

        // Client Post Redirect Uris
        Add("CheckValidPostLogoutRedirectURI", new Rule<ClientsModel>(
            new IsValidUris<ClientsModel>(model =>
                model.PostLogoutRedirectUris.ContainsAny()
                    ? model.PostLogoutRedirectUris.ConvertAll(uri => uri.PostLogoutRedirectUri)
                    : null),
            EndpointErrorCodes.InvalidPostLogoutRedirectUri));
        Add("CheckValidPostRedirectURILength", new Rule<ClientsModel>(
            new IsValid2048CharLengths<ClientsModel>(model =>
                model.PostLogoutRedirectUris.ContainsAny()
                    ? model.PostLogoutRedirectUris.ConvertAll(uri => uri.PostLogoutRedirectUri)
                    : null),
            EndpointErrorCodes.PostRedirectUriTooLong));
        Add("CheckValidPostRedirectURICreatedByLength", new Rule<ClientsModel>(
            new IsValid255CharLengths<ClientsModel>(model =>
                model.PostLogoutRedirectUris.ContainsAny()
                    ? model.PostLogoutRedirectUris.ConvertAll(uri => uri.CreatedBy)
                    : null),
            EndpointErrorCodes.PostRedirectUriCreatedByTooLong));

        switch (crudMode)
        {
            case CrudMode.Add:
                Add("CheckClientNameAlreadyExists", new Rule<ClientsModel>(
                    new CheckClientNameExists(unitOfWork),
                    EndpointErrorCodes.ClientNameAlreadyExists));
                break;
            case CrudMode.Update:
                ClientModelUpdateValidation(unitOfWork);
                break;
        }
    }

    private void ClientModelUpdateValidation(IClientsUnitOfWork unitOfWork)
    {
        // Client
        Add("CheckClientModifiedByLength", new Rule<ClientsModel>(
            new IsValid255CharLength<ClientsModel>(model => model.ModifiedBy),
            ApiErrorCodes.ModifiedByTooLong));
        Add("CheckClientSecretExpiresAt", new Rule<ClientsModel>(
            new IsNotNull<ClientsModel>(model => model.ClientSecretExpiresAt),
            EndpointErrorCodes.ClientSecretExpiresAtRequired));

        // Client Redirect Uris
        Add("CheckValidRedirectURIModifiedByLength", new Rule<ClientsModel>(
            new IsValid255CharLengths<ClientsModel>(model =>
                model.RedirectUris.ContainsAny() ? model.RedirectUris.ConvertAll(uri => uri.ModifiedBy) : null),
            EndpointErrorCodes.RedirectUriModifiedByTooLong));
        Add("CheckValidRedirectURIClientID", new Rule<ClientsModel>(
            new IsValidIdentifierExists<ClientsModel>(model =>
                model.RedirectUris.ContainsAny() ? model.RedirectUris.ConvertAll(uri => uri.ClientId) : null),
            EndpointErrorCodes.InvalidRedirectUriClientId));
        Add("CheckClientRedirectUrlInactive", new Rule<ClientsModel>(
            new CheckClientRedirectUrlInactive(unitOfWork),
            EndpointErrorCodes.InactiveClientRedirectUri));

        // Client Post Redirect Uris
        Add("CheckValidPostRedirectURIModifiedByLength", new Rule<ClientsModel>(
            new IsValid255CharLengths<ClientsModel>(model =>
                model.PostLogoutRedirectUris.ContainsAny()
                    ? model.PostLogoutRedirectUris.ConvertAll(uri => uri.ModifiedBy)
                    : null),
            EndpointErrorCodes.PostRedirectUriModifiedByTooLong));
        Add("CheckValidPostRedirectURIClientID", new Rule<ClientsModel>(
            new IsValidIdentifierExists<ClientsModel>(model =>
                model.PostLogoutRedirectUris.ContainsAny()
                    ? model.PostLogoutRedirectUris.ConvertAll(uri => uri.ClientId)
                    : null),
            EndpointErrorCodes.InvalidPostRedirectUriClientId));
        Add("CheckClientPostRedirectUrlInactive", new Rule<ClientsModel>(
            new CheckClientPostRedirectUrlInactive(unitOfWork),
            EndpointErrorCodes.InactiveClientPostLogoutRedirectUri));
    }
}

/// <summary>
/// Checks that no active client with the same name already exists.
/// </summary>

internal class CheckClientNameExists : ISpecification<ClientsModel>
{
    private readonly IClientsUnitOfWork unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckClientNameExists"/> class.
    /// </summary>
    internal CheckClientNameExists(IClientsUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(ClientsModel model)
    {
        var isClientExists = unitOfWork.ClientRepository
            .ActiveRecordExistsAsync(client => client.ClientName == model.ClientName).GetAwaiter().GetResult();
        if (isClientExists) return false;

        return true;
    }
}

/// <summary>
/// Checks whether the client has been soft-deleted (inactive) to prevent duplicate registrations.
/// </summary>

internal class CheckClientInactive : ISpecification<ClientsModel>
{
    private readonly IClientsUnitOfWork unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckClientInactive"/> class.
    /// </summary>
    internal CheckClientInactive(IClientsUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(ClientsModel model)
    {
        var duplicateExists = unitOfWork.ClientRepository
            .DuplicateExistsAsync(client => client.ClientId == model.ClientId).GetAwaiter().GetResult();

        if (duplicateExists) return false;

        return true;
    }
}

/// <summary>
/// Validates that all requested scopes are registered (identity resources, API resources, API scopes,
/// or the built-in offline_access scope) and that there are no duplicates.
/// </summary>

internal class CheckAllowedScope : ISpecification<ClientsModel>
{
    private readonly IApiResourceRepository apiResourceRepository;
    private readonly IRepository<ApiScopes> apiScopeRepository;
    private readonly IIdentityResourceRepository identityResourceRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckAllowedScope"/> class.
    /// </summary>
    internal CheckAllowedScope(IApiResourceRepository apiResourceRepository, IRepository<ApiScopes> apiScopeRepository,
        IIdentityResourceRepository identityResourceRepository)
    {
        this.apiResourceRepository = apiResourceRepository;
        this.apiScopeRepository = apiScopeRepository;
        this.identityResourceRepository = identityResourceRepository;
    }

    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(ClientsModel model)
    {
        var commonScopes = identityResourceRepository.GetAllAsync().GetAwaiter().GetResult().Select(x => x.Name.Trim())
            .Union(apiResourceRepository.GetAllAsync().GetAwaiter().GetResult().Select(x => x.Name.Trim()))
            .Union(apiScopeRepository.GetAllAsync().GetAwaiter().GetResult().Select(x => x.Name.Trim()))
            .Union(new List<string> { AuthenticationConstants.IdentityScopes.OfflineAccess });

        var allowedScopes = model.AllowedScopes;

        if (allowedScopes.Count.Equals(allowedScopes.Distinct().Count()))
        {
            if (allowedScopes.Except(commonScopes).Any()) return false;

            return true;
        }

        return false;
    }
}

/// <summary>
/// Validates that the client's supported grant types are from the allowed set
/// (authorization_code, refresh_token, client_credentials, password, user_code) with no duplicates.
/// </summary>

internal class CheckSupportedGrantTypesProfile : ISpecification<ClientsModel>
{
    private static readonly HashSet<string> AllowedGrantTypes = new(StringComparer.Ordinal)
    {
        OpenIdConstants.GrantTypes.AuthorizationCode,
        OpenIdConstants.GrantTypes.RefreshToken,
        OpenIdConstants.GrantTypes.ClientCredentials,
        OpenIdConstants.GrantTypes.Password,
        OpenIdConstants.GrantTypes.UserCode
    };

    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(ClientsModel model)
    {
        if (model?.SupportedGrantTypes == null || model.SupportedGrantTypes.Count == 0) return false;

        return model.SupportedGrantTypes.Count == model.SupportedGrantTypes.Distinct(StringComparer.Ordinal).Count()
               && model.SupportedGrantTypes.All(grantType => AllowedGrantTypes.Contains(grantType));
    }
}

/// <summary>
/// Validates response types: clients using authorization_code must have exactly ["code"];
/// clients not using authorization_code must have an empty response types list.
/// </summary>

internal class CheckSupportedResponseTypesProfile : ISpecification<ClientsModel>
{
    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(ClientsModel model)
    {
        if (model?.SupportedGrantTypes == null || model.SupportedResponseTypes == null) return false;

        var usesAuthorizationCode = model.SupportedGrantTypes.Contains(
            OpenIdConstants.GrantTypes.AuthorizationCode,
            StringComparer.Ordinal);

        if (!usesAuthorizationCode)
            return model.SupportedResponseTypes.Count == 0;

        return model.SupportedResponseTypes.Count == 1
               && string.Equals(
                   model.SupportedResponseTypes[0],
                   OpenIdConstants.ResponseTypes.Code,
                   StringComparison.Ordinal);
    }
}

/// <summary>
/// Enforces that public clients (RequireClientSecret=false) must use PKCE (RequirePkce=true)
/// to prevent authorization code interception attacks per RFC 7636.
/// </summary>

internal class CheckPkceRequirementForPublicClient : ISpecification<ClientsModel>
{
    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(ClientsModel model)
    {
        if (model == null) return false;

        if (!model.RequireClientSecret && !model.RequirePkce) return false;

        return true;
    }
}

/// <summary>
/// Ensures that clients using the authorization_code grant type have at least one redirect URI configured.
/// </summary>

internal class CheckRedirectUrisForAuthorizationCode : ISpecification<ClientsModel>
{
    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(ClientsModel model)
    {
        if (model == null) return false;

        if (model.SupportedGrantTypes == null
            || !model.SupportedGrantTypes.Contains(OpenIdConstants.GrantTypes.AuthorizationCode,
                StringComparer.Ordinal))
            return true;

        return model.RedirectUris != null && model.RedirectUris.ContainsAny();
    }
}

/// <summary>
/// Checks that new redirect URIs being added during update are not duplicates of soft-deleted entries.
/// </summary>

internal class CheckClientRedirectUrlInactive : ISpecification<ClientsModel>
{
    private readonly IClientsUnitOfWork unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckClientRedirectUrlInactive"/> class.
    /// </summary>
    internal CheckClientRedirectUrlInactive(IClientsUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(ClientsModel model)
    {
        if (model.RedirectUris.ContainsAny())
            foreach (var redirectUrl in model.RedirectUris)
                // Considering the below record as new, inactive check needs to be done for new records alone. No specific api for child entries.
                if (!redirectUrl.Id.IsValid())
                {
                    var duplicateExists = unitOfWork.RedirectUrisRepository.DuplicateExistsAsync(x =>
                            x.ClientId == redirectUrl.ClientId &&
                            x.RedirectUri == redirectUrl.RedirectUri)
                        .GetAwaiter().GetResult();

                    if (duplicateExists) return false;
                }

        return true;
    }
}

/// <summary>
/// Checks that new post-logout redirect URIs being added during update are not duplicates of soft-deleted entries.
/// </summary>

internal class CheckClientPostRedirectUrlInactive : ISpecification<ClientsModel>
{
    private readonly IClientsUnitOfWork unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckClientPostRedirectUrlInactive"/> class.
    /// </summary>
    internal CheckClientPostRedirectUrlInactive(IClientsUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(ClientsModel model)
    {
        if (model.PostLogoutRedirectUris.ContainsAny())
            foreach (var redirectUrl in model.PostLogoutRedirectUris)
                // Considering the below record as new, inactive check needs to be done for new records alone. No specific api for child entries.
                if (!redirectUrl.Id.IsValid())
                {
                    var duplicateExists = unitOfWork.RedirectUrisRepository.DuplicateExistsAsync(x =>
                            x.ClientId == redirectUrl.ClientId &&
                            x.RedirectUri == redirectUrl.PostLogoutRedirectUri)
                        .GetAwaiter().GetResult();

                    if (duplicateExists) return false;
                }

        return true;
    }
}
