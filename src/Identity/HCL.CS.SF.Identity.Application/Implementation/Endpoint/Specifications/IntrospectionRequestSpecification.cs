/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.IdentityModel.Tokens.Jwt;
using AutoMapper;
using DomainValidation.Interfaces.Specification;
using DomainValidation.Validation;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.UnitOfWork.Endpoint;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Implementation.Endpoint.Utils;
using HCL.CS.SF.Service.Implementation.Endpoint.Validators;
using static HCL.CS.SF.Domain.Constants.Endpoint.OpenIdConstants;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Specifications;

/// <summary>
/// Validates an OAuth 2.0 token introspection request per RFC 7662.
/// Enforces that the token is present, within length limits, has a valid hint type,
/// has not expired, can be cryptographically validated, and has not been revoked.
/// </summary>
/// <summary>
/// Validates OAuth 2.0 Token Introspection requests per RFC 7662.
/// Checks token presence, length, type hint, expiry, signature validation, and revocation status.
/// Supports both access tokens (JWT) and refresh tokens (opaque handles).
/// </summary>
internal sealed class IntrospectionRequestSpecification : BaseRequestModelValidator<ValidatedIntrospectionRequestModel>
{
    /// <summary>
    /// Initializes the introspection validation pipeline with sequential rules.
    /// </summary>
    /// <param name="unitOfWork">Unit of work for accessing security token storage.</param>
    /// <param name="clientRepository">Repository for loading client entities.</param>
    /// <param name="mapper">AutoMapper instance for entity-to-model mapping.</param>
    /// <param name="keyStore">Dictionary of asymmetric keys used for token signature verification.</param>
    /// <param name="securityTokenRepository">Repository for querying persisted security tokens.</param>
    internal IntrospectionRequestSpecification(
        IClientsUnitOfWork unitOfWork,
        IRepository<Clients> clientRepository,
        IMapper mapper,
        Dictionary<string, AsymmetricKeyInfoModel> keyStore,
        IRepository<SecurityTokens> securityTokenRepository)
    {
        // Ensure the token parameter is present in the request
        Add("CheckTokenNull", new Rule<ValidatedIntrospectionRequestModel>(
            new IsRequestNull<ValidatedIntrospectionRequestModel>(request =>
                request.GetValue(IntrospectionRequest.Token)),
            Errors.InvalidToken,
            EndpointErrorCodes.TokenMissing));

        // Enforce JWT maximum length restriction on the token value
        Add("CheckLengthRestrictions", new Rule<ValidatedIntrospectionRequestModel>(
            new CheckLengthRestrictions<ValidatedIntrospectionRequestModel>(
                request => request.GetValue(IntrospectionRequest.Token),
                request => request.TokenConfigOptions.InputLengthRestrictionsConfig.Jwt,
                request => ">"),
            Errors.InvalidToken,
            EndpointErrorCodes.Invalid_Token_Length));

        // Validate the optional token_type_hint is a recognized hint type
        Add("CheckIntrospectionTokenHintType", new Rule<ValidatedIntrospectionRequestModel>(
            new CheckIntrospectionTokenHintType(),
            Errors.InvalidRequest,
            EndpointErrorCodes.InvalidTokenHintType));

        // Verify the token has not expired based on its claims or persisted metadata
        Add("CheckTokenExpiry", new Rule<ValidatedIntrospectionRequestModel>(
            new CheckTokenExpiry(unitOfWork),
            Errors.InvalidRequest,
            EndpointErrorCodes.TokenExpired));

        // Cryptographically validate the token signature and audience
        Add("ValidateIntrospectionToken", new Rule<ValidatedIntrospectionRequestModel>(
            new ValidateIntrospectionToken(clientRepository, mapper, keyStore),
            Errors.InvalidRequest,
            EndpointErrorCodes.InvalidTokenInIntrospection));

        // Verify the token has not been revoked in the security token store
        Add("CheckIsActiveToken", new Rule<ValidatedIntrospectionRequestModel>(
            new CheckIsActiveToken(securityTokenRepository),
            Errors.InvalidToken,
            EndpointErrorCodes.TokenRevoked));
    }
}

/// <summary>
/// Specification that checks whether a token has expired by examining JWT claims for access tokens
/// or persisted metadata for refresh tokens. Supports auto-detection of token type.
/// </summary>
internal class CheckTokenExpiry : ISpecification<ValidatedIntrospectionRequestModel>
{
    private readonly IClientsUnitOfWork unitOfWork;

    /// <summary>
    /// Initializes the expiry checker with a unit of work for refresh token lookups.
    /// </summary>
    /// <param name="unitOfWork">Unit of work providing access to the security tokens repository.</param>
    internal CheckTokenExpiry(IClientsUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Determines whether the introspected token is still within its validity period.
    /// For access tokens, expiry is computed from JWT claims. For refresh tokens,
    /// expiry is computed from persisted creation time and configured lifetime.
    /// </summary>
    /// <param name="model">The introspection request model containing the token and client info.</param>
    /// <returns>True if the token is still valid; false if expired or not found.</returns>
    public bool IsSatisfiedBy(ValidatedIntrospectionRequestModel model)
    {
        var token = model.GetValue(IntrospectionRequest.Token);

        // Attempt access token validation first (or when auto-detecting)
        if (model.TokenType == TokenResponseType.AccessToken || model.TokenType == "autodetect")
            try
            {
                var jwt = new JwtSecurityToken(token);
                model.TokenType = TokenResponseType.AccessToken;
                var expiryClaim = jwt.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Expiration);
                var creationDateClaim = jwt.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.IssuedAt);
                var scopeclaim = jwt.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Scope);
                if (expiryClaim == null || creationDateClaim == null) return false;

                // Calculate expiry using the client's configured access token lifetime
                var expiryDuration = model.Client.AccessTokenExpiration;
                var expiryTime = Convert.ToInt64(creationDateClaim.Value).ToDateTime().AddSeconds(expiryDuration);
                if (expiryTime > DateTime.UtcNow)
                {
                    model.ExpiresAt = Convert.ToInt64(expiryClaim.Value);
                    model.IsError = false;
                    model.Scopes = scopeclaim.Value;
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                // Token could not be parsed as JWT; fall through to refresh token check
            }

        // Attempt refresh token validation (or when auto-detecting and JWT parsing failed)
        if (model.TokenType == TokenType.RefreshToken || model.TokenType == "autodetect")
        {
            // Refresh tokens are stored as SHA-256 hashes
            var tokenHash = token.ComputeSha256Hash();
            var securityTokenList = unitOfWork.SecurityTokensRepository.GetAsync(
                    entity => entity.Key == tokenHash && entity.TokenType == TokenType.RefreshToken,
                    x => new { x.CreationTime, x.ExpiresAt, x.ConsumedAt, x.ConsumedTime, x.TokenReuseDetected })
                .GetAwaiter().GetResult();
            if (!securityTokenList.ContainsAny()) return false;

            var securityToken = securityTokenList.FirstOrDefault();
            if (securityToken == null) return false;

            // Reject tokens that have been consumed or flagged for reuse detection
            if (securityToken.TokenReuseDetected || securityToken.ConsumedAt.HasValue ||
                securityToken.ConsumedTime.HasValue) return false;

            model.TokenType = TokenResponseType.RefreshToken;
            var expiredTime = Convert.ToDateTime(securityToken.CreationTime).AddSeconds(securityToken.ExpiresAt);
            if (expiredTime > DateTime.UtcNow)
            {
                model.ExpiresAt = expiredTime.ToUnixTime();
                model.IssuedAt = securityToken.CreationTime.ToUnixTime();
                model.IsError = false;
                return true;
            }

            model.ExpiresAt = expiredTime.ToUnixTime();
            model.IssuedAt = securityToken.CreationTime.ToUnixTime();
            model.IsError = false;
        }

        model.IsError = true;
        return false;
    }
}

/// <summary>
/// Specification that cryptographically validates the introspected token.
/// For access tokens, verifies the signature using the client's asymmetric key and checks the audience.
/// For refresh tokens, simply marks the model as valid since they are opaque.
/// </summary>
internal class ValidateIntrospectionToken : ISpecification<ValidatedIntrospectionRequestModel>
{
    private readonly IRepository<Clients> clientRepository;
    private readonly Dictionary<string, AsymmetricKeyInfoModel> keyStore;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes the token validator with dependencies for client lookup and key resolution.
    /// </summary>
    /// <param name="clientRepository">Repository for loading client entities.</param>
    /// <param name="mapper">AutoMapper instance for entity-to-model mapping.</param>
    /// <param name="keyStore">Dictionary of asymmetric signing keys.</param>
    internal ValidateIntrospectionToken(
        IRepository<Clients> clientRepository,
        IMapper mapper,
        Dictionary<string, AsymmetricKeyInfoModel> keyStore)
    {
        this.clientRepository = clientRepository;
        this.mapper = mapper;
        this.keyStore = keyStore;
    }

    /// <summary>
    /// Validates the token's signature, algorithm, and audience for access tokens.
    /// Refresh tokens bypass cryptographic validation.
    /// </summary>
    /// <param name="model">The introspection request model containing the token to validate.</param>
    /// <returns>True if the token is cryptographically valid; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedIntrospectionRequestModel model)
    {
        var token = model.GetValue(IntrospectionRequest.Token);
        if (string.IsNullOrWhiteSpace(token)) return false;

        switch (model.TokenType)
        {
            case TokenResponseType.AccessToken:
            {
                try
                {
                    // Resolve the signing key for the client
                    model = GetTokenKey(model, model.Client.ClientId).GetAwaiter().GetResult();

                    if (!model.IsError)
                    {
                        var jwtToken = new JwtSecurityToken(token);

                        // Ensure the token's algorithm matches the client's configured algorithm
                        if (model.Client.AllowedSigningAlgorithm.Trim() != jwtToken.SignatureAlgorithm.Trim())
                        {
                            model.IsError = true;
                            return false;
                        }

                        var expectedAudience = model.TokenConfigOptions.TokenConfig.ApiIdentifier;
                        if (string.IsNullOrWhiteSpace(expectedAudience)) return false;

                        // Extract the subject claim for user identification
                        var subject = jwtToken.Claims;
                        if (subject != null)
                        {
                            var subjectIdentifier = subject.FirstOrDefault(x => x.Type == ClaimTypes.Sub);
                            model.UserId = subjectIdentifier.Value;
                        }

                        // Validate the token signature using asymmetric key verification
                        if (model.Client.AllowedSigningAlgorithm == Algorithms.RsaSha256
                            || model.Client.AllowedSigningAlgorithm == Algorithms.EcdsaSha256)
                        {
                            (model.DecodedToken, _) = token.ValidateAsymmetricToken(
                                model.Key,
                                model.TokenConfigOptions.TokenConfig.IssuerUri,
                                expectedAudience);
                        }
                        else
                        {
                            model.IsError = true;
                            return false;
                        }

                        model.IsError = false;
                        return true;
                    }

                    break;
                }
                catch (Exception)
                {
                    model.IsError = true;
                    return false;
                }
            }

            case TokenResponseType.RefreshToken:
                // Refresh tokens are opaque; no cryptographic validation needed
                model.IsError = false;
                return true;
        }

        return true;
    }

    /// <summary>
    /// Resolves the asymmetric signing key for the specified client from the key store.
    /// Falls back to RS256 if the client has no explicit algorithm configured.
    /// </summary>
    /// <param name="result">The introspection model to populate with the resolved key.</param>
    /// <param name="clientId">The client identifier to look up.</param>
    /// <returns>The updated model with the signing key or error information.</returns>
    public async Task<ValidatedIntrospectionRequestModel> GetTokenKey(ValidatedIntrospectionRequestModel result,
        string clientId)
    {
        var clientsEntity = await clientRepository.GetAsync(
            client => client.ClientId == clientId,
            new System.Linq.Expressions.Expression<Func<HCL.CS.SF.Domain.Entities.Endpoint.Clients, object>>[] { x => x.RedirectUris, x => x.PostLogoutRedirectUris });
        var client = mapper.Map<Clients, ClientsModel>(clientsEntity[0]);
        result.IsError = false;
        if (!string.IsNullOrWhiteSpace(client.AllowedSigningAlgorithm))
        {
            // Client has an explicit algorithm; resolve the matching key
            if (client.AllowedSigningAlgorithm == Algorithms.RsaSha256
                || client.AllowedSigningAlgorithm == Algorithms.EcdsaSha256)
            {
                if (keyStore.Values.Count > 0)
                {
                    var signingCredentials =
                        keyStore.GetAsymmetricVerificationCredentials(client.AllowedSigningAlgorithm);
                    if (signingCredentials != null)
                    {
                        result.IsError = false;
                        result.Key = signingCredentials.Key;
                    }
                    else
                    {
                        result.ErrorCode = Errors.InvalidRequest;
                        result.IsError = true;
                    }
                }
                else
                {
                    result.ErrorCode = Errors.InvalidRequest;
                    result.IsError = true;
                }
            }
            else if (client.AllowedSigningAlgorithm.Trim() == "none")
            {
                // "none" algorithm is not permitted
                result.ErrorCode = Errors.UnsupportedAlgorithm;
                result.IsError = true;
            }
            else
            {
                result.ErrorCode = Errors.UnsupportedAlgorithm;
                result.IsError = true;
            }
        }
        else
        {
            // No algorithm configured; default to RS256
            if (keyStore.Values.Count > 0)
            {
                var signingCredentials = keyStore.GetAsymmetricVerificationCredentials(Algorithms.RsaSha256);
                if (signingCredentials != null)
                {
                    result.Key = signingCredentials.Key;
                }
                else
                {
                    result.ErrorCode = Errors.InvalidRequest;
                    result.IsError = true;
                }
            }
            else
            {
                result.ErrorCode = Errors.InvalidRequest;
                result.IsError = true;
            }
        }

        return result;
    }
}

/// <summary>
/// Specification that validates the token_type_hint parameter in an introspection request.
/// If present, the hint must be a recognized token type (access_token or refresh_token).
/// If absent, the system auto-detects the token type.
/// </summary>
internal class CheckIntrospectionTokenHintType : ISpecification<ValidatedIntrospectionRequestModel>
{
    /// <summary>
    /// Validates the token_type_hint and sets the model's TokenType accordingly.
    /// </summary>
    /// <param name="model">The introspection request model.</param>
    /// <returns>True if the hint is valid or absent; false if the hint is unrecognized.</returns>
    public bool IsSatisfiedBy(ValidatedIntrospectionRequestModel model)
    {
        var tokenHintType = model.GetValue(IntrospectionRequest.TokenHintType);
        if (!string.IsNullOrEmpty(tokenHintType))
        {
            // Validate against the allowed hint types
            var allowedHintTypes = typeof(TokenHintTypes).GetArray().ToList().ConvertAll(x => x.ToLower());
            if (allowedHintTypes.Contains(tokenHintType))
                model.TokenType = tokenHintType;
            else
                return false;
        }
        else
        {
            // No hint provided; enable auto-detection
            model.TokenType = "autodetect";
        }

        return true;
    }
}

/// <summary>
/// Specification that checks whether the introspected token has been revoked
/// by querying the security token repository.
/// </summary>
internal class CheckIsActiveToken : ISpecification<ValidatedIntrospectionRequestModel>
{
    private readonly IRepository<SecurityTokens> securityTokenRepository;

    /// <summary>
    /// Initializes the active-token checker with the security token repository.
    /// </summary>
    /// <param name="securityTokenRepository">Repository for querying token revocation status.</param>
    internal CheckIsActiveToken(IRepository<SecurityTokens> securityTokenRepository)
    {
        this.securityTokenRepository = securityTokenRepository;
    }

    /// <summary>
    /// Checks whether the token has been revoked in the persisted store.
    /// </summary>
    /// <param name="model">The introspection request model containing the token.</param>
    /// <returns>True if the token is active (not revoked); false if revoked.</returns>
    public bool IsSatisfiedBy(ValidatedIntrospectionRequestModel model)
    {
        var token = model.GetValue(IntrospectionRequest.Token);
        // Delegate revocation check to TokenUtil which handles both access and refresh tokens
        if (new TokenUtil(securityTokenRepository).IsTokenRevoked(token, model.TokenType).GetAwaiter()
            .GetResult()) return false;

        return true;
    }
}
