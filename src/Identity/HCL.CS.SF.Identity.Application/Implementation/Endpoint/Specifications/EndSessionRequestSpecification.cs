/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using AutoMapper;
using DomainValidation.Interfaces.Specification;
using DomainValidation.Validation;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.UnitOfWork.Endpoint;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Implementation.Endpoint.Validators;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Specifications;

/// <summary>
/// Validates OpenID Connect end-session (logout) requests when an id_token_hint is present.
/// Checks user authentication, identity token expiry, token signature, post_logout_redirect_uri,
/// and session state parameter.
/// </summary>
internal sealed class EndSessionRequestSpecification : BaseRequestModelValidator<ValidatedEndSessionRequestModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EndSessionRequestSpecification"/> class.
    /// </summary>
    internal EndSessionRequestSpecification(
        IClientsUnitOfWork unitOfWork,
        IRepository<Clients> clientRepository,
        IMapper mapper,
        ISessionManagementService sessionManagementService,
        Dictionary<string, AsymmetricKeyInfoModel> keyStore)
    {
        Add("IsAuthenticated", new Rule<ValidatedEndSessionRequestModel>(
            new IsAuthenticated(request => request.Subject),
            OpenIdConstants.Errors.InvalidRequest,
            EndpointErrorCodes.UnauthenticatedUser));

        Add("CheckTokenExpiry", new Rule<ValidatedEndSessionRequestModel>(
            new CheckIdentityTokenExpiry(clientRepository, mapper, unitOfWork),
            OpenIdConstants.Errors.InvalidRequest,
            EndpointErrorCodes.TokenExpired));

        Add("CheckIdTokenHint", new Rule<ValidatedEndSessionRequestModel>(
            new ValidateIdentityToken(sessionManagementService, keyStore),
            OpenIdConstants.Errors.InvalidRequest,
            EndpointErrorCodes.InvalidIdentityToken));

        Add("CheckPostLogoutUris", new Rule<ValidatedEndSessionRequestModel>(
            new CheckPostLogoutUris(),
            OpenIdConstants.Errors.InvalidRequest,
            EndpointErrorCodes.InvalidPostLogoutRedirectUri));

        Add("CheckSessionEndState", new Rule<ValidatedEndSessionRequestModel>(
            new CheckSessionEndState(),
            OpenIdConstants.Errors.InvalidRequest,
            EndpointErrorCodes.InvalidState));
    }
}

/// <summary>
/// Specification that captures the optional state parameter for end-session requests.
/// State is included in the post-logout redirect when present.
/// </summary>
internal class CheckSessionEndState : ISpecification<ValidatedEndSessionRequestModel>
{
    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(ValidatedEndSessionRequestModel model)
    {
        var state = model.GetValue(OpenIdConstants.EndSessionRequest.State);
        if (model.PostLogOutUri != null && !string.IsNullOrWhiteSpace(state)) model.State = state;

        return true;
    }
}

/// <summary>
/// Specification that validates the post_logout_redirect_uri is one of the client's registered URIs.
/// </summary>
internal class CheckPostLogoutUris : ISpecification<ValidatedEndSessionRequestModel>
{
    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(ValidatedEndSessionRequestModel model)
    {
        var redirectUri = model.GetValue(OpenIdConstants.EndSessionRequest.PostLogoutRedirectUri);
        if (!string.IsNullOrWhiteSpace(redirectUri))
        {
            var uris = model.Client.PostLogoutRedirectUris.ConvertAll(x => x.PostLogoutRedirectUri);
            if (uris.Contains(redirectUri))
            {
                model.PostLogOutUri = redirectUri;
                return true;
            }

            return false;
        }

        return true;
    }
}

/// <summary>
/// Specification that verifies the user is authenticated before processing the end-session request.
/// </summary>
internal class IsAuthenticated : ISpecification<ValidatedEndSessionRequestModel>
{
    private readonly Expression<Func<ValidatedEndSessionRequestModel, object>> expression;

    /// <summary>
    /// Initializes a new instance of the <see cref="IsAuthenticated"/> class.
    /// </summary>
    internal IsAuthenticated(Expression<Func<ValidatedEndSessionRequestModel, object>> expression)
    {
        this.expression = expression;
    }

    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(ValidatedEndSessionRequestModel model)
    {
        var value = expression.Compile()(model);
        var type = value.GetType();
        if (type != typeof(ClaimsPrincipal)) return false;

        if (!(value as ClaimsPrincipal).IsAuthenticated()) return false;

        return true;
    }
}

/// <summary>
/// Specification that validates the id_token_hint provided in the end-session request.
/// Verifies the token signature using the client's asymmetric key and retrieves session data.
/// </summary>
internal class ValidateIdentityToken : ISpecification<ValidatedEndSessionRequestModel>
{
    private readonly Dictionary<string, AsymmetricKeyInfoModel> keyStore;
    private readonly ISessionManagementService sessionManagementService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateIdentityToken"/> class.
    /// </summary>
    internal ValidateIdentityToken(
        ISessionManagementService sessionManagementService,
        Dictionary<string, AsymmetricKeyInfoModel> keyStore)
    {
        this.sessionManagementService = sessionManagementService;
        this.keyStore = keyStore;
    }

    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(ValidatedEndSessionRequestModel model)
    {
        var token = model.GetValue(OpenIdConstants.EndSessionRequest.IdTokenHint);
        if (!string.IsNullOrWhiteSpace(token))
        {
            model = GetTokenKey(model);
            if (!model.IsError)
            {
                if (model.Client.AllowedSigningAlgorithm == OpenIdConstants.Algorithms.RsaSha256
                    || model.Client.AllowedSigningAlgorithm == OpenIdConstants.Algorithms.EcdsaSha256)
                {
                    try
                    {
                        (model.DecodedToken, _) =
                            token.ValidateAsymmetricToken(model.Key, model.TokenConfigOptions.TokenConfig.IssuerUri,
                                model.Client.ClientId);
                    }
                    catch
                    {
                        model.IsError = true;
                        return false;
                    }
                }
                else
                {
                    model.IsError = true;
                    return false;
                }

                model.SessionId = sessionManagementService.GetSessionId().GetAwaiter().GetResult();
                model.ClientIds = sessionManagementService.GetClientListAsync().GetAwaiter().GetResult();
                model.IsError = false;

                return true;
            }
        }

        return true;
    }

    /// <summary>
    /// Retrieves the token ke.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <returns>The operation result.</returns>
    public ValidatedEndSessionRequestModel GetTokenKey(ValidatedEndSessionRequestModel result)
    {
        var client = result.Client;
        if (!string.IsNullOrWhiteSpace(client.AllowedSigningAlgorithm))
        {
            if (client.AllowedSigningAlgorithm == OpenIdConstants.Algorithms.RsaSha256
                || client.AllowedSigningAlgorithm == OpenIdConstants.Algorithms.EcdsaSha256)
            {
                if (keyStore.Values.Count > 0)
                {
                    var signingCredentials = keyStore.GetAsymmetricSigningCredentials(client.AllowedSigningAlgorithm);
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
                var signingCredentials = keyStore.GetAsymmetricSigningCredentials(OpenIdConstants.Algorithms.RsaSha256);
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
/// Specification that validates the id_token_hint has not expired. Resolves the client from the token's
/// audience claim, checks JWT expiry claims, and falls back to persisted token metadata if needed.
/// </summary>
internal class CheckIdentityTokenExpiry : ISpecification<ValidatedEndSessionRequestModel>
{
    private readonly IRepository<Clients> clientRepository;
    private readonly IMapper mapper;
    private readonly IClientsUnitOfWork unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckIdentityTokenExpiry"/> class.
    /// </summary>
    internal CheckIdentityTokenExpiry(
        IRepository<Clients> clientRepository,
        IMapper mapper,
        IClientsUnitOfWork unitOfWork)
    {
        this.clientRepository = clientRepository;
        this.mapper = mapper;
        this.unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(ValidatedEndSessionRequestModel model)
    {
        var token = model.GetValue(OpenIdConstants.EndSessionRequest.IdTokenHint);
        var jwt = new JwtSecurityToken(token);
        var clientId = new JwtSecurityToken(token).Audiences.FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(clientId))
        {
            var clientsEntity = clientRepository.GetAsync(
                client => client.ClientId == clientId,
                new System.Linq.Expressions.Expression<Func<HCL.CS.SF.Domain.Entities.Endpoint.Clients, object>>[] { x => x.RedirectUris, x => x.PostLogoutRedirectUris }).GetAwaiter().GetResult();

            if (clientsEntity.ContainsAny())
            {
                model.Client = mapper.Map<Clients, ClientsModel>(clientsEntity[0]);
                model.ClientId = model.Client.ClientId;
            }

            model.SubjectId = new JwtSecurityToken(token).Subject;
        }
        else
        {
            return false;
        }

        var expiryClaim = jwt.Claims.FirstOrDefault(claim => claim.Type == OpenIdConstants.ClaimTypes.Expiration);
        var creationDateClaim = jwt.Claims.FirstOrDefault(claim => claim.Type == OpenIdConstants.ClaimTypes.IssuedAt);
        if (expiryClaim == null || creationDateClaim == null) return false;

        var expiryDuration = model.Client.IdentityTokenExpiration;
        var expiryTime = Convert.ToInt64(creationDateClaim.Value).ToDateTime().AddSeconds(expiryDuration);
        if (expiryTime > DateTime.UtcNow)
        {
            model.ExpiresAt = Convert.ToInt64(expiryClaim.Value);
            model.IsError = false;
            return true;
        }

        var securityTokenList = unitOfWork.SecurityTokensRepository.GetAsync(
            entity => entity.TokenValue == token,
            x => new { x.CreationTime, x.ExpiresAt }).GetAwaiter().GetResult();
        if (!securityTokenList.ContainsAny()) return false;

        var securityToken = securityTokenList.FirstOrDefault();
        if (securityToken == null) return false;

        var expiredTime = Convert.ToDateTime(securityToken.CreationTime).AddSeconds(securityToken.ExpiresAt);
        if (expiredTime > DateTime.UtcNow) return false;

        model.ExpiresAt = expiredTime.ToUnixTime();
        model.IsError = false;

        return true;
    }
}
