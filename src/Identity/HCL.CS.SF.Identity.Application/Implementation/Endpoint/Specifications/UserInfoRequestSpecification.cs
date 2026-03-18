/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using DomainValidation.Interfaces.Specification;
using DomainValidation.Validation;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Wrappers;
using HCL.CS.SF.Service.Extension;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Implementation.Endpoint.Utils;
using HCL.CS.SF.Service.Implementation.Endpoint.Validators;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Specifications;

/// <summary>
/// Validates the access token presented to the OpenID Connect UserInfo Endpoint.
/// Checks token format (JWT with dots), length, revocation status, signature, client existence,
/// user existence, and presence of the required 'openid' scope claim.
/// </summary>
internal sealed class UserInfoRequestSpecification : BaseRequestModelValidator<ValidatedUserInfoRequestModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserInfoRequestSpecification"/> class.
    /// </summary>
    internal UserInfoRequestSpecification(
        IRepository<Clients> clientRepository,
        IMapper mapper,
        Dictionary<string, AsymmetricKeyInfoModel> keyStore,
        UserManagerWrapper<Users> userManager,
        TokenSettings configSettings,
        IRepository<SecurityTokens> securityTokenRepository)
    {
        Add(
            "ValidateTokenFormat", new Rule<ValidatedUserInfoRequestModel>(
                new ValidateTokenFormat(),
                OpenIdConstants.Errors.InvalidFormat,
                EndpointErrorCodes.InvalidTokenFormat));

        Add(
            "ValidateTokenLength",
            new Rule<ValidatedUserInfoRequestModel>(
                new ValidateTokenLength(configSettings),
                OpenIdConstants.Errors.InvalidToken,
                EndpointErrorCodes.TokenMaxLengthExceeded));

        Add(
            "ValidateIsActiveToken",
            new Rule<ValidatedUserInfoRequestModel>(
                new ValidateIsActiveToken(securityTokenRepository),
                OpenIdConstants.Errors.InvalidToken,
                EndpointErrorCodes.TokenRevoked));

        Add(
            "ValidateToken",
            new Rule<ValidatedUserInfoRequestModel>(
                new ValidateToken(clientRepository, mapper, keyStore),
                OpenIdConstants.Errors.InvalidRequest,
                EndpointErrorCodes.TokenIsNullOrInvalid));

        Add(
            "ValidateClient",
            new Rule<ValidatedUserInfoRequestModel>(
                new ValidateClient(clientRepository),
                OpenIdConstants.Errors.InvalidClient,
                EndpointErrorCodes.ClientDoesNotExist));

        Add(
            "ValidateUserClaims",
            new Rule<ValidatedUserInfoRequestModel>(
                new ValidateUserClaims(clientRepository, userManager),
                OpenIdConstants.Errors.InvalidToken,
                EndpointErrorCodes.InvalidUserClaims));

        Add(
            "ValidateScopeClaims",
            new Rule<ValidatedUserInfoRequestModel>(
                new ValidateScopeClaims(),
                OpenIdConstants.Errors.InsufficientScope,
                EndpointErrorCodes.InvalidScopeClaims));
    }
}

/// <summary>
/// Specification that validates the access token has a valid JWT format (contains dot separators).
/// </summary>
internal class ValidateTokenFormat : ISpecification<ValidatedUserInfoRequestModel>
{
    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(ValidatedUserInfoRequestModel model)
    {
        if (!model.Token.Contains(".")) return false;

        return true;
    }
}

/// <summary>
/// Specification that validates the access token does not exceed the configured maximum JWT length.
/// </summary>
internal class ValidateTokenLength : ISpecification<ValidatedUserInfoRequestModel>
{
    private readonly TokenSettings tokenSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateTokenLength"/> class.
    /// </summary>
    internal ValidateTokenLength(TokenSettings tokenConfig)
    {
        tokenSettings = tokenConfig;
    }

    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(ValidatedUserInfoRequestModel model)
    {
        if (model.Token.Length > tokenSettings.InputLengthRestrictionsConfig.Jwt) return false;

        return true;
    }
}

/// <summary>
/// Specification that checks whether the access token has been revoked in the security token store.
/// </summary>
internal class ValidateIsActiveToken : ISpecification<ValidatedUserInfoRequestModel>
{
    private readonly IRepository<SecurityTokens> securityTokenRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateIsActiveToken"/> class.
    /// </summary>
    internal ValidateIsActiveToken(IRepository<SecurityTokens> securityTokenRepository)
    {
        this.securityTokenRepository = securityTokenRepository;
    }

    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(ValidatedUserInfoRequestModel model)
    {
        if (new TokenUtil(securityTokenRepository).IsTokenRevoked(model.Token, OpenIdConstants.TokenType.AccessToken)
            .GetAwaiter().GetResult()) return false;

        return true;
    }
}

/// <summary>
/// Specification that cryptographically validates the access token's signature, resolves the signing key
/// from the client's configuration, and populates the model with decoded token data.
/// </summary>
internal class ValidateToken : ISpecification<ValidatedUserInfoRequestModel>
{
    private readonly IRepository<Clients> clientRepository;
    private readonly Dictionary<string, AsymmetricKeyInfoModel> keyStore;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateToken"/> class.
    /// </summary>
    internal ValidateToken(
        IRepository<Clients> clientRepository,
        IMapper mapper,
        Dictionary<string, AsymmetricKeyInfoModel> keyStore)
    {
        this.clientRepository = clientRepository;
        this.mapper = mapper;
        this.keyStore = keyStore;
    }

    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(ValidatedUserInfoRequestModel model)
    {
        var token = model.Token;
        if (!string.IsNullOrWhiteSpace(token))
        {
            var claims = new JwtSecurityToken(token).Claims;
            if (claims == null) return false;

            string clientId = null;
            var client = claims.Where(x => x.Type == OpenIdConstants.ClaimTypes.ClientId).ToList();
            if (client.ContainsAny()) clientId = client[0].Value;

            if (clientId == null) return false;

            var signinKeys = GetTokenKey(clientId).GetAwaiter().GetResult();
            if (!signinKeys.IsError)
            {
                try
                {
                    var expectedAudience = model.TokenConfigOptions.TokenConfig.ApiIdentifier;
                    if (string.IsNullOrWhiteSpace(expectedAudience)) return false;

                    model.Client = signinKeys.Client;
                    model.Key = signinKeys.Key;
                    ClaimsPrincipal sub = null;
                    if (model.Client.AllowedSigningAlgorithm == OpenIdConstants.Algorithms.RsaSha256
                        || model.Client.AllowedSigningAlgorithm == OpenIdConstants.Algorithms.EcdsaSha256)
                    {
                        (model.DecodedToken, sub) = token.ValidateAsymmetricToken(
                            model.Key,
                            model.TokenConfigOptions.TokenConfig.IssuerUri,
                            expectedAudience);
                    }
                    else
                    {
                        model.IsError = true;
                        return false;
                    }
                }
                catch (Exception)
                {
                    model.IsError = true;
                    return false;
                }

                model.IsError = false;
                return true;
            }

            model.ErrorCode = signinKeys.ErrorCode;
            return false;
        }

        return false;
    }

    /// <summary>
    /// Retrieves the token ke.
    /// </summary>
    /// <param name="clientId">The client id.</param>
    /// <returns>The operation result.</returns>
    public async Task<ValidatedUserInfoRequestModel> GetTokenKey(string clientId)
    {
        var result = new ValidatedUserInfoRequestModel();
        var clientsEntity = await clientRepository.GetAsync(
            client => client.ClientId == clientId,
            new System.Linq.Expressions.Expression<Func<HCL.CS.SF.Domain.Entities.Endpoint.Clients, object>>[] { x => x.RedirectUris, x => x.PostLogoutRedirectUris });

        if (clientsEntity.ContainsAny())
        {
            result.Client = mapper.Map<Clients, ClientsModel>(clientsEntity[0]);
        }
        else
        {
            result.ErrorCode = OpenIdConstants.Errors.InvalidClient;
            result.IsError = true;
            return result;
        }

        var client = result.Client;
        if (!string.IsNullOrWhiteSpace(client.AllowedSigningAlgorithm))
        {
            if (client.AllowedSigningAlgorithm == OpenIdConstants.Algorithms.RsaSha256
                || client.AllowedSigningAlgorithm == OpenIdConstants.Algorithms.EcdsaSha256)
            {
                if (keyStore.Values.Count > 0)
                {
                    //var signingCredentials = keyStore.GetAsymmetricCredentials(client.AllowedSigningAlgorithm);
                    var signingCredentials =
                        keyStore.GetAsymmetricVerificationCredentials(client.AllowedSigningAlgorithm);
                    if (signingCredentials != null)
                    {
                        result.IsError = false;
                        result.Key = signingCredentials.Key;
                    }
                    else
                    {
                        result.ErrorCode = OpenIdConstants.Errors.InvalidRequest;
                    }
                }
                else
                {
                    result.ErrorCode = OpenIdConstants.Errors.InvalidRequest;
                }
            }
            else
            {
                result.ErrorCode = OpenIdConstants.Errors.UnsupportedAlgorithm;
            }
        }
        else
        {
            if (keyStore.Values.Count > 0)
            {
                var signingCredentials =
                    keyStore.GetAsymmetricVerificationCredentials(OpenIdConstants.Algorithms.RsaSha256);
                if (signingCredentials != null)
                    result.Key = signingCredentials.Key;
                else
                    result.ErrorCode = OpenIdConstants.Errors.InvalidRequest;
            }
            else
            {
                result.ErrorCode = OpenIdConstants.Errors.InvalidRequest;
            }
        }

        result.IsError = !string.IsNullOrWhiteSpace(result.ErrorCode) || result.Key == null;
        return result;
    }
}

/// <summary>
/// Specification that verifies the client referenced in the access token's client_id claim
/// exists in the database. Also normalizes space-separated scope claims into individual entries.
/// </summary>
internal class ValidateClient : ISpecification<ValidatedUserInfoRequestModel>
{
    private readonly IRepository<Clients> clientRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateClient"/> class.
    /// </summary>
    internal ValidateClient(
        IRepository<Clients> clientRepository)
    {
        this.clientRepository = clientRepository;
    }

    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(ValidatedUserInfoRequestModel model)
    {
        ClientsModel client = null;
        var accessTokenClaims = model.DecodedToken.Claims.ToList();
        var clientId = accessTokenClaims.FirstOrDefault(c => c.Type == OpenIdConstants.ClaimTypes.ClientId);
        if (clientId != null)
        {
            var clientIsExist = clientRepository.ActiveRecordExistsAsync(x => x.ClientId == clientId.Value).GetAwaiter()
                .GetResult();
            if (!clientIsExist) return false;
        }

        var claims = accessTokenClaims;
        var scopes = claims.Where(c => c.Type == OpenIdConstants.ClaimTypes.Scope).ToArray();
        if (scopes.ContainsAny())
            foreach (var scope in scopes)
                if (scope.Value.Contains(" "))
                {
                    claims.Remove(scope);
                    var values = scope.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var value in values) claims.Add(new Claim(OpenIdConstants.ClaimTypes.Scope, value));
                }

        model.Claims = claims;
        model.Client = client;

        return true;
    }
}

/// <summary>
/// Specification that verifies the user referenced in the 'sub' claim still exists in the identity store.
/// Prevents returning claims for deleted or deactivated users.
/// </summary>
internal class ValidateUserClaims : ISpecification<ValidatedUserInfoRequestModel>
{
    private readonly IRepository<Clients> clientRepository;
    private readonly UserManagerWrapper<Users> userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateUserClaims"/> class.
    /// </summary>
    internal ValidateUserClaims(
        IRepository<Clients> clientRepository,
        UserManagerWrapper<Users> userManager)
    {
        this.clientRepository = clientRepository;
        this.userManager = userManager;
    }

    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(ValidatedUserInfoRequestModel model)
    {
        // make sure user is still active (if sub claim is present)
        var userClaim = model.Claims.FirstOrDefault(c => c.Type == OpenIdConstants.ClaimTypes.Sub);
        if (userClaim != null && userClaim.Value.IsGuid())
        {
            var user = userManager.FindByIdAsync(userClaim.Value).GetAwaiter().GetResult();
            if (user == null)
            {
                model.Claims = null;
                return false;
            }
        }

        return true;
    }
}

/// <summary>
/// Specification that verifies the access token contains the required 'openid' scope claim.
/// Also constructs the ClaimsPrincipal subject from the filtered token claims for the UserInfo response.
/// </summary>
internal class ValidateScopeClaims : ISpecification<ValidatedUserInfoRequestModel>
{
    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(ValidatedUserInfoRequestModel model)
    {
        // check expected scope(s)
        var scope = model.Claims.FirstOrDefault(c =>
            c.Type == OpenIdConstants.ClaimTypes.Scope && c.Value == AuthenticationConstants.IdentityScopes.OpenId);
        if (scope == null) return false;

        var claims = model.Claims.Where(x => !AuthenticationConstants.AccessTokenFilters.ClaimsFilter.Contains(x.Type));
        var claimsIdentity = new ClaimsIdentity(claims.ToArray(), "UserInfo", OpenIdConstants.ClaimTypes.Name,
            OpenIdConstants.ClaimTypes.Role);
        model.Subject = new ClaimsPrincipal(claimsIdentity);

        return true;
    }
}
