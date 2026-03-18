/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using DomainValidation.Interfaces.Specification;
using DomainValidation.Validation;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Implementation.Endpoint.Validators;
using static HCL.CS.SF.Domain.Constants.Endpoint.OpenIdConstants;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Specifications;

/// <summary>
/// Validates an OAuth 2.0 token revocation request per RFC 7009.
/// Ensures the token is present, the token_type_hint is valid (if provided),
/// and performs the actual revocation of owned tokens.
/// </summary>
/// <summary>
/// Validates OAuth 2.0 Token Revocation requests per RFC 7009.
/// Checks that the token is present, the token_type_hint is valid (if provided),
/// and performs the actual token revocation (deleting matching tokens from the database).
/// </summary>
internal sealed class TokenRevocationRequestSpecification : BaseRequestModelValidator<ValidatedRevocationRequestModel>
{
    /// <summary>
    /// Initializes the revocation validation pipeline.
    /// </summary>
    /// <param name="securityTokenRepository">Repository for querying and deleting persisted security tokens.</param>
    internal TokenRevocationRequestSpecification(IRepository<SecurityTokens> securityTokenRepository)
    {
        // Ensure the token parameter is present in the revocation request
        Add("CheckTokenIsPresent", new Rule<ValidatedRevocationRequestModel>(
            new CheckTokenIsPresent(),
            Errors.InvalidRequest,
            EndpointErrorCodes.NoTokenFound));

        // Validate the optional token_type_hint is a recognized token type
        Add("CheckTokenTypeHint", new Rule<ValidatedRevocationRequestModel>(
            new CheckTokenTypeHint(),
            Errors.UnsupportedTokenType,
            EndpointErrorCodes.UnsupportedTokenType));

        // Look up the token in storage and revoke it if owned by the requesting client
        Add("CheckTokenAndRevoke", new Rule<ValidatedRevocationRequestModel>(
            new CheckTokenAndRevoke(securityTokenRepository),
            Errors.InvalidRequest,
            EndpointErrorCodes.InvalidRevocationRequest));
    }
}

/// <summary>
/// Specification that validates the token parameter is present and non-empty in a revocation request.
/// </summary>
internal class CheckTokenIsPresent : ISpecification<ValidatedRevocationRequestModel>
{
    /// <summary>
    /// Checks that the token value exists and sets it on the model.
    /// </summary>
    /// <param name="model">The revocation request model.</param>
    /// <returns>True if the token is present; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedRevocationRequestModel model)
    {
        var token = model.GetValue(RevocationRequest.Token);
        if (!string.IsNullOrWhiteSpace(token))
        {
            model.Token = token;
            return true;
        }

        return false;
    }
}

/// <summary>
/// Specification that validates the optional token_type_hint parameter.
/// Accepts "refresh_token" and "access_token" as valid hint values.
/// </summary>
internal class CheckTokenTypeHint : ISpecification<ValidatedRevocationRequestModel>
{
    /// <summary>
    /// Validates the token_type_hint value if present.
    /// </summary>
    /// <param name="model">The revocation request model.</param>
    /// <returns>True if the hint is valid or absent; false if unrecognized.</returns>
    public bool IsSatisfiedBy(ValidatedRevocationRequestModel model)
    {
        var tokenTypeHint = model.GetValue(RevocationRequest.TokenTypeHint);
        if (!string.IsNullOrWhiteSpace(tokenTypeHint))
        {
            if (tokenTypeHint == TokenType.RefreshToken || tokenTypeHint == TokenType.AccessToken)
            {
                model.TokenTypeHint = tokenTypeHint;
                return true;
            }

            return false;
        }

        return true;
    }
}

/// <summary>
/// Specification that locates the token in persistent storage and revokes it.
/// Follows RFC 7009 semantics: returns success for unknown tokens and does not
/// reveal whether a token exists for another client.
/// </summary>
internal class CheckTokenAndRevoke : ISpecification<ValidatedRevocationRequestModel>
{
    private readonly IRepository<SecurityTokens> securityTokenRepository;

    /// <summary>
    /// Initializes the revocation handler with the security token repository.
    /// </summary>
    /// <param name="securityTokenRepository">Repository for token lookup and deletion.</param>
    public CheckTokenAndRevoke(IRepository<SecurityTokens> securityTokenRepository)
    {
        this.securityTokenRepository = securityTokenRepository;
    }

    /// <summary>
    /// Looks up the token by type hint (or both types if no hint), filters to tokens owned
    /// by the requesting client, and performs a hard delete for revocation.
    /// </summary>
    /// <param name="model">The revocation request model containing token and client info.</param>
    /// <returns>True if revocation succeeded or the token was not found; false on error.</returns>
    public bool IsSatisfiedBy(ValidatedRevocationRequestModel model)
    {
        var token = model.GetValue(RevocationRequest.Token);
        var tokenTypeHint = model.GetValue(RevocationRequest.TokenTypeHint);
        try
        {
            IList<SecurityTokens> tokenList = null;
            // Refresh tokens are stored as SHA-256 hashes of the raw token value
            var refreshTokenHash = token.ComputeSha256Hash();
            if (tokenTypeHint == TokenType.RefreshToken)
                tokenList = securityTokenRepository
                    .GetAsync(entity => entity.Key == refreshTokenHash && entity.TokenType == TokenType.RefreshToken)
                    .GetAwaiter().GetResult();
            else if (tokenTypeHint == TokenType.AccessToken)
                tokenList = securityTokenRepository
                    .GetAsync(entity => entity.TokenValue == token && entity.TokenType == TokenType.AccessToken)
                    .GetAwaiter().GetResult();
            else
                // No hint: search both token types
                tokenList = securityTokenRepository
                    .GetAsync(entity =>
                        (entity.TokenType == TokenType.RefreshToken && entity.Key == refreshTokenHash)
                        || (entity.TokenType == TokenType.AccessToken && entity.TokenValue == token))
                    .GetAwaiter().GetResult();

            if (!tokenList.ContainsAny())
                // RFC 7009: idempotent success for unknown tokens.
                return true;

            // Filter to only tokens owned by the requesting client
            var ownedTokenList = tokenList
                .Where(x => x.ClientId == model.ClientId)
                .ToList();

            if (!ownedTokenList.ContainsAny())
                // RFC 7009: do not reveal whether token exists for another client.
                return true;

            // Hard-delete the owned tokens from storage
            securityTokenRepository.DeleteAsync(ownedTokenList).GetAwaiter().GetResult();
            securityTokenRepository.SaveChangesWithHardDeleteAsync().GetAwaiter().GetResult();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
