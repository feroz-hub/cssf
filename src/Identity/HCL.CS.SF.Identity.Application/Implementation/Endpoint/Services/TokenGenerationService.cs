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
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Configurations.Api;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.Domain.Models.Endpoint.Response;
using HCL.CS.SF.Domain.Models.Endpoint.Validation;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.DomainServices.Repository.Api;
using HCL.CS.SF.DomainServices.Wrappers;
using HCL.CS.SF.Service.Implementation.Endpoint.Comparers;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;
using static HCL.CS.SF.Domain.Constants.Endpoint.OpenIdConstants;
using ClaimTypes = HCL.CS.SF.Domain.Constants.Endpoint.OpenIdConstants.ClaimTypes;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Services;

/// <summary>
/// Core service responsible for generating OAuth 2.0 / OpenID Connect tokens.
/// Handles access token (JWT), identity token (id_token), and refresh token generation
/// for all supported grant types. Manages refresh token rotation, token claims assembly,
/// PKCE validation integration, and back-channel logout token creation.
/// </summary>
internal class TokenGenerationService(
    ILoggerInstance instance,
    IMapper mapper,
    HCLCSSFConfig settings,
    UserManagerWrapper<Users> userManager,
    IFrameworkResultService frameworkResultService,
    IRepository<SecurityTokens> securityTokenRepository,
    ISecurityTokenCommandRepository securityTokenCommandRepository,
    ITenantContext tenantContext,
    Dictionary<string, AsymmetricKeyInfoModel> keyStore)
    : SecurityBase, ITokenGenerationService
{
    private readonly CryptoConfig cryptoConfig = settings.SystemSettings.CryptoConfig;
    private readonly ILoggerService loggerService = instance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
    private readonly TokenSettings tokenSettings = settings.TokenSettings;

    /// <summary>
    /// Processes the toke.
    /// </summary>
    /// <param name="tokenRequest">The token request.</param>
    /// <returns>The operation result.</returns>
    public async Task<TokenResponseModel> ProcessTokenAsync(ValidatedTokenRequestModel tokenRequest)
    {
        var tokenResponse = new TokenResponseModel();
        try
        {
            if (tokenRequest.Client != null)
            {
                if (tokenRequest.GrantType == GrantTypes.RefreshToken)
                {
                    tokenResponse = await RenewRefreshTokenAsync(tokenRequest);
                    return tokenResponse;
                }

                var accessToken = string.Empty;
                loggerService.WriteTo(Log.Debug,
                    "Entered into process token for client : " + tokenRequest.Client.ClientName);
                if (!string.IsNullOrWhiteSpace(tenantContext.TenantId))
                    loggerService.WriteTo(Log.Debug, "Token request tenant: " + tenantContext.TenantId);
                var resultClaims = await GetTokenClaimsAsync(tokenRequest);
                if (tokenRequest.GrantType != GrantTypes.RefreshToken &&
                    CanGenerateToken(tokenRequest, ResponseTypes.Token))
                {
                    accessToken = await GenerateAccessTokenAsync(tokenRequest, resultClaims);
                    tokenResponse.AccessToken = accessToken;
                    tokenResponse.AccessTokenExpiresIn = GetEffectiveAccessTokenLifetime(tokenRequest.Client);
                }

                tokenResponse.State = tokenRequest.State;
                tokenResponse.Scope =
                    tokenRequest.AllowedScopesParserModel.ParsedAllowedScopes.ConvertSpaceSeparatedString();

                if (tokenRequest.AllowedScopesParserModel.AllowOfflineAccess
                    && !string.IsNullOrWhiteSpace(accessToken) &&
                    tokenRequest.GrantType != GrantTypes.ClientCredentials)
                {
                    var refreshToken = await GenerateRefreshTokenAsync(tokenRequest, accessToken);
                    if (!string.IsNullOrWhiteSpace(refreshToken))
                        tokenResponse.RefreshToken = refreshToken;
                    else
                        frameworkResultService.Throw(EndpointErrorCodes.FailedToStoreRefreshToken);
                }

                if (!string.IsNullOrWhiteSpace(accessToken)) await StoreAccessTokenAsync(tokenRequest, accessToken);

                if (tokenRequest.AllowedScopesParserModel.CreateIdentityToken
                    && CanGenerateToken(tokenRequest, ResponseTypes.IdToken))
                    tokenResponse.IdentityToken = await GenerateIdentityTokenAsync(tokenRequest, resultClaims);

                tokenResponse.TokenType = TokenResponseType.BearerTokenType;
            }
            else
            {
                frameworkResultService.Throw(EndpointErrorCodes.InvalidRequest);
            }
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, ex.Message);
            throw;
        }

        return tokenResponse;
    }

    /// <summary>
    /// Validates the refresh toke.
    /// </summary>
    /// <param name="refreshTokenKey">The refresh token key.</param>
    /// <param name="client">The client.</param>
    /// <returns>The operation result.</returns>
    public async Task<TokenValidationModel> ValidateRefreshTokenAsync(string refreshTokenKey, ClientsModel client)
    {
        var error = new TokenValidationModel { IsError = true, ErrorCode = Errors.InvalidGrant };
        if (refreshTokenKey == null) return error;

        loggerService.WriteTo(Log.Debug, "Entered into validate refresh token for client : " + client.ClientName);

        var refreshTokenHash = refreshTokenKey.ComputeSha256Hash();
        var refreshTokenEntityList = await securityTokenRepository.GetAsync(entity =>
            entity.Key == refreshTokenHash
            && entity.TokenType == TokenType.RefreshToken);
        if (!refreshTokenEntityList.ContainsAny()) return error;

        var refreshTokenEntity = refreshTokenEntityList.FirstOrDefault();
        if (refreshTokenEntity.TokenReuseDetected || refreshTokenEntity.ConsumedAt.HasValue ||
            refreshTokenEntity.ConsumedTime.HasValue)
        {
            await HandleRefreshTokenReuseAsync(refreshTokenEntity);
            return error;
        }

        var refreshTokenModel = mapper.Map<SecurityTokens, SecurityTokensModel>(refreshTokenEntity);

        if (refreshTokenModel.CreationTime.IsExpired(refreshTokenModel.ExpiresAt))
        {
            loggerService.WriteTo(Log.Error, "Refresh token has expired for client : " + client.ClientName);
            return error;
        }

        // check if client belongs to requested refresh token
        if (client.ClientId != refreshTokenModel.ClientId) return error;

        // check if client still has offline_access scope
        if (!client.AllowOfflineAccess)
        {
            loggerService.WriteTo(Log.Error, "Client does not have access to offline_access scope anymore.");
            return error;
        }

        // make sure user is enabled
        var user = await userManager.FindByIdAsync(refreshTokenModel.SubjectId);
        if (user == null)
        {
            loggerService.WriteTo(Log.Error, "Subject is not active.");
            return error;
        }

        return new TokenValidationModel
        {
            IsError = false,
            RefreshToken = refreshTokenModel,
            Client = client
        };
    }

    /// <summary>
    /// Deletes the specified user toke.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public async Task<FrameworkResult> RemoveUserTokensAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return frameworkResultService.Failed<FrameworkResult>(EndpointErrorCodes.ArgumentNullError);

        try
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user != null)
            {
                // Clear's Access Token
                var accessTokenList = await securityTokenRepository.GetAsync(entity =>
                    entity.SubjectId == userId && entity.TokenType == TokenType.AccessToken);
                if (accessTokenList.ContainsAny())
                {
                    loggerService.WriteTo(Log.Debug,
                        "Entered into remove user tokens for user - Count :" + accessTokenList.Count);
                    await securityTokenRepository.DeleteAsync(accessTokenList.ToList());
                }

                // Clear's Refresh Token
                var refreshTokenList = await securityTokenRepository.GetAsync(entity =>
                    entity.SubjectId == userId && entity.TokenType == TokenType.RefreshToken);
                if (refreshTokenList != null && refreshTokenList.Count > 0)
                    await securityTokenRepository.DeleteAsync(refreshTokenList.ToList());

                // Clear's Identity Token
                var identityTokenList = await securityTokenRepository.GetAsync(entity =>
                    entity.SubjectId == user.UserName && entity.TokenType == TokenType.IdentityToken);
                if (identityTokenList.ContainsAny())
                    await securityTokenRepository.DeleteAsync(identityTokenList.ToList());

                return await securityTokenRepository.SaveChangesWithHardDeleteAsync();
            }

            return frameworkResultService.Failed<FrameworkResult>(EndpointErrorCodes.InvalidSubjectId);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Revokes the specified toke.
    /// </summary>
    /// <param name="revocationRequest">The revocation request.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public async Task<FrameworkResult> RevokeTokenAsync(ValidatedRevocationRequestModel revocationRequest)
    {
        if (revocationRequest == null)
            return frameworkResultService.Failed<FrameworkResult>(EndpointErrorCodes.ArgumentNullError);

        try
        {
            var token = revocationRequest.Token;
            var tokenHash = token.ComputeSha256Hash();
            var refreshTokenList = await securityTokenRepository.GetAsync(entity =>
                (entity.TokenType == TokenType.RefreshToken && entity.Key == tokenHash)
                || (entity.TokenType == TokenType.AccessToken && entity.TokenValue == token));
            if (refreshTokenList.ContainsAny())
            {
                loggerService.WriteTo(Log.Debug, "Entered into revoke token - Count : " + refreshTokenList.Count);
                await securityTokenRepository.DeleteAsync(refreshTokenList.ToList());
            }
            else
            {
                frameworkResultService.Failed<FrameworkResult>(EndpointErrorCodes.InvalidRevocationRequest);
            }

            return await securityTokenRepository.SaveChangesWithHardDeleteAsync();
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Generates back channel logout toke.
    /// </summary>
    /// <param name="backChannelLogoutModel">The back channel logout model.</param>
    /// <returns>The operation result.</returns>
    public async Task<string> GenerateBackChannelLogoutTokenAsync(BackChannelLogoutModel backChannelLogoutModel)
    {
        var algorithm = GetAlgorithmAsync(backChannelLogoutModel.Client.AllowedSigningAlgorithm);

        // create JwtHeader
        var headerLogoutToken =
            await CreateJwtHeader(backChannelLogoutModel.Client.ClientSecret, algorithm, "logout+jwt");

        // create Payload
        var json = "{\"" + LogoutTokenEvents.BackChannelLogoutUri + "\":{} }";
        var claims = new List<Claim>
        {
            new(ClaimTypes.Sub, backChannelLogoutModel.SubjectId),
            new(ClaimTypes.IssuedAt, DateTime.UtcNow.ToUnixTime().ToString()),
            new(ClaimTypes.JwtId, AuthenticationConstants.KeySize32.RandomString()),
            new(ClaimTypes.Events, json)
        };

        if (backChannelLogoutModel.Client.BackChannelLogoutSessionRequired && backChannelLogoutModel.SessionId != null)
            claims.Add(new Claim(ClaimTypes.SessionId, backChannelLogoutModel.SessionId));

        // create JwtPayLoad
        var payload = new JwtPayload(
            tokenSettings.TokenConfig.IssuerUri,
            backChannelLogoutModel.ClientId,
            claims,
            DateTime.UtcNow,
            DateTime.UtcNow.AddSeconds(backChannelLogoutModel.Client.LogoutTokenExpiration),
            DateTime.UtcNow);

        // Logout Token encode and generation
        var encodedLogoutToken = string.Empty;
        if (headerLogoutToken != null)
        {
            loggerService.WriteTo(Log.Debug,
                "Entered into generate back channel logout token for client : " +
                backChannelLogoutModel.Client.ClientName);
            encodedLogoutToken = headerLogoutToken.GenerateToken(payload);
        }
        else
        {
            frameworkResultService.Throw(EndpointErrorCodes.InvalidClientSecret);
        }

        return encodedLogoutToken;
    }

    private async Task<string> GenerateAccessTokenAsync(ValidatedTokenRequestModel tokenRequest,
        ResultClaimsModel resultClaims)
    {
        var algorithm = GetAlgorithmAsync(tokenRequest.Client.AllowedSigningAlgorithm);

        // create JwtHeader
        var headerAccessToken =
            await CreateJwtHeader(tokenRequest.TokenDetails.Client.ClientSecret, algorithm, "at+jwt");

        // create Payload
        var payload = CreateAccessTokenPayload(tokenRequest, resultClaims);

        // Access Token encode and generation
        var encodedAccessToken = string.Empty;
        if (headerAccessToken != null)
            encodedAccessToken = headerAccessToken.GenerateToken(payload);
        else
            frameworkResultService.Throw(EndpointErrorCodes.InvalidClientSecret);

        if (tokenRequest.GrantType == GrantTypes.AuthorizationCode) tokenRequest.AccessTokenToHash = encodedAccessToken;

        return encodedAccessToken;
    }

    private async Task<string> GenerateRefreshTokenAsync(ValidatedTokenRequestModel tokenRequest, string accesstoken)
    {
        var refreshTokenModel = new SecurityTokensModel();
        if (tokenRequest == null || string.IsNullOrWhiteSpace(accesstoken))
            frameworkResultService.Throw(EndpointErrorCodes.ArgumentNullError);

        string refreshTokenKey;
        try
        {
            loggerService.WriteTo(Log.Debug,
                "Entered into save refresh token for client :" + tokenRequest.Client.ClientName);
            refreshTokenKey = cryptoConfig.RandomStringLength.RandomString();
            refreshTokenModel.Key = refreshTokenKey.ComputeSha256Hash();
            refreshTokenModel.TokenValue = accesstoken;
            refreshTokenModel.TokenType = TokenType.RefreshToken;
            refreshTokenModel.ExpiresAt = GetEffectiveRefreshTokenLifetime(tokenRequest.TokenDetails.Client);
            refreshTokenModel.SubjectId = tokenRequest.TokenDetails.User.Id.ToString();
            refreshTokenModel.ClientId = tokenRequest.ClientId;
            refreshTokenModel.SessionId = tokenRequest.SessionId;
            refreshTokenModel.CreationTime = DateTime.UtcNow;
            refreshTokenModel.ConsumedAt = null;
            refreshTokenModel.ConsumedTime = null;
            refreshTokenModel.TokenReuseDetected = false;

            // store refresh token
            var result = await StoreSecurityTokenAsync(refreshTokenModel, tokenRequest.TokenDetails.User.Id.ToString());
            if (result.Status == ResultStatus.Failed)
                ThrowIfPersistenceFailed(result, EndpointErrorCodes.FailedToStoreRefreshToken);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, ex.Message);
            throw;
        }

        return refreshTokenKey;
    }

    private JwtPayload CreateAccessTokenPayload(ValidatedTokenRequestModel tokenRequest, ResultClaimsModel resultClaims)
    {
        var accessTokenClaims = resultClaims.AccessTokenClaims;
        // Per-client preferred audience (e.g. rentflow.api); else fall back to global ApiIdentifier.
        var audienceClaims = !string.IsNullOrWhiteSpace(tokenRequest.TokenDetails?.Client?.PreferredAudience)
            ? tokenRequest.TokenDetails.Client.PreferredAudience.Trim()
            : tokenSettings.TokenConfig.ApiIdentifier;
        if (string.IsNullOrWhiteSpace(audienceClaims)) frameworkResultService.Throw(EndpointErrorCodes.InvalidRequest);

        var scopeClaims = resultClaims.AccessTokenScopeClaims.Select(claim => new Claim(ClaimTypes.Scope, claim.Value));

        accessTokenClaims.AddRange(GetNormalizedClaimsList(tokenRequest));

        // add scope as a single claim with space separated string.
        accessTokenClaims.AddRange(scopeClaims);

        loggerService.WriteTo(Log.Debug,
            "Entered into generate access token for client :" + tokenRequest.Client.ClientName);

        // create JwtPayLoad
        var payload = new JwtPayload(
            tokenRequest.Issuer,
            audienceClaims,
            accessTokenClaims,
            DateTime.UtcNow,
            DateTime.UtcNow.AddSeconds(GetEffectiveAccessTokenLifetime(tokenRequest.TokenDetails.Client)),
            DateTime.UtcNow);

        return payload;
    }

    private string GetAlgorithmAsync(string allowedSigningAlgorithm)
    {
        if (string.IsNullOrWhiteSpace(allowedSigningAlgorithm)) return Algorithms.RsaSha256;

        if (string.Equals(allowedSigningAlgorithm, Algorithms.RsaSha256, StringComparison.Ordinal)
            || string.Equals(allowedSigningAlgorithm, Algorithms.EcdsaSha256, StringComparison.Ordinal))
            return allowedSigningAlgorithm;

        frameworkResultService.Throw(EndpointErrorCodes.SigningAlgorithmIsInvalid);
        return Algorithms.RsaSha256;
    }

    private bool CanGenerateToken(ValidatedTokenRequestModel tokenRequest, string tokenType)
    {
        if (tokenRequest.IsRequestFromAuthorizationEndpoint)
        {
            // Additionally check response type as request is from authorization end point.
            if (tokenRequest.ResponseTypes.Contains(tokenType)) return true;

            return false;
        }

        // request not from authorization endpoint. Token will be generated.
        return true;
    }

    private List<Claim> GetNormalizedClaimsList(ValidatedTokenRequestModel tokenRequest, bool isIdentityToken = false)
    {
        var now = DateTime.UtcNow.ToUnixTime();
        Claim authTime = null;
        var expiryDuration = isIdentityToken
            ? GetEffectiveIdentityTokenLifetime(tokenRequest.TokenDetails.Client)
            : GetEffectiveAccessTokenLifetime(tokenRequest.TokenDetails.Client);

        var exp = DateTime.UtcNow.AddSeconds(expiryDuration).ToUnixTime();
        string subject;
        if (isIdentityToken)
        {
            var claimIdentity = tokenRequest.Subject.Identity as ClaimsIdentity;
            authTime = claimIdentity.FindFirst(ClaimTypes.AuthenticationTime);
            subject = tokenRequest.TokenDetails.User.Id.ToString();
        }
        else if (tokenRequest.GrantType == GrantTypes.ClientCredentials)
        {
            subject = tokenRequest.TokenDetails.Client.ClientId;
        }
        else
        {
            subject = tokenRequest.TokenDetails.User.Id.ToString();
        }

        var claimList = new List<Claim>
        {
            new(ClaimTypes.Issuer, tokenRequest.Issuer),
            new(ClaimTypes.IssuedAt, now.ToString()),
            new(ClaimTypes.NotBefore, now.ToString()),
            new(ClaimTypes.Sub, subject),
            new(ClaimTypes.Expiration, exp.ToString()),
            new(ClaimTypes.JwtId, AuthenticationConstants.KeySize32.RandomString()),
            new(ClaimTypes.ClientId, tokenRequest.Client.ClientId)
        };

        if (tokenRequest.GrantType != GrantTypes.ClientCredentials)
        {
            claimList.Add(new Claim(ClaimTypes.IdentityProvider,
                Enum.GetName(typeof(IdentityProvider), tokenRequest.TokenDetails.User.IdentityProviderType).ToLower()));
            if (tokenRequest.Nonce != null) claimList.Add(new Claim(ClaimTypes.Nonce, tokenRequest.Nonce));

            if (authTime != null)
                claimList.Add(new Claim(ClaimTypes.AuthenticationTime, long.Parse(authTime.Value).ToString(),
                    ClaimValueTypes.Integer64));

            if (tokenRequest.SessionId != null) claimList.Add(new Claim(ClaimTypes.SessionId, tokenRequest.SessionId));

            var algorithm = GetAlgorithmAsync(tokenRequest.Client.AllowedSigningAlgorithm);
            if (!string.IsNullOrWhiteSpace(tokenRequest.AccessTokenToHash) && isIdentityToken)
                claimList.Add(new Claim(
                    ClaimTypes.AccessTokenHash,
                    tokenRequest.AccessTokenToHash.CreateHashClaimValue(algorithm)));

            if (!string.IsNullOrWhiteSpace(tokenRequest.AuthorizationCodeToHash) && isIdentityToken)
                claimList.Add(new Claim(
                    ClaimTypes.AuthorizationCodeHash,
                    tokenRequest.AuthorizationCodeToHash.CreateHashClaimValue(algorithm)));
        }

        return claimList;
    }

    private async Task<ResultClaimsModel> GetUserIdentityResourcesAsync(TokenDetailsModel tokenDetails,
        ResultClaimsModel resultClaims)
    {
        var userInfo = tokenDetails.User;
        try
        {
            resultClaims.IdentityClaims.AddRange(
                await userInfo.GetUserIdentityResources(tokenDetails.IdentityResourcesByScopes));
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, ex.Message);
            throw;
        }

        return resultClaims;
    }

    private async Task<ResultClaimsModel> GetClaimsFromParsedTransactionScopes(List<string> parsedTransactionScopes,
        ResultClaimsModel resultClaims)
    {
        if (!parsedTransactionScopes.ContainsAny()) return resultClaims;

        loggerService.WriteTo(Log.Debug,
            "Entered into get claims from parsed transaction scopes : " +
            parsedTransactionScopes.ConvertSpaceSeparatedString());
        var transactionClaims = (from scope in parsedTransactionScopes
            where scope.Contains(":")
            select scope.Split(':')[1]
            into claimValue
            select new Claim(ClaimTypes.Transaction, claimValue)).ToList();

        resultClaims.TransactionClaims.AddRange(transactionClaims);
        return await Task.FromResult(resultClaims);
    }

    private async Task<TokenResponseModel> RenewRefreshTokenAsync(ValidatedTokenRequestModel tokenRequest)
    {
        var tokenResponse = new TokenResponseModel();
        try
        {
            var refreshTokenKey = tokenRequest.RequestedRefreshToken;
            // check null for refresh token key.
            if (string.IsNullOrWhiteSpace(refreshTokenKey))
                return frameworkResultService.Failed<TokenResponseModel>(EndpointErrorCodes.ArgumentNullError);

            loggerService.WriteTo(Log.Debug,
                "Entered into renew refresh token for client : " + tokenRequest.Client.ClientName);

            var refreshTokenHash = refreshTokenKey.ComputeSha256Hash();
            var refreshTokenEntityList = await securityTokenRepository.GetAsync(entity =>
                entity.Key == refreshTokenHash
                && entity.TokenType == TokenType.RefreshToken);

            if (refreshTokenEntityList.ContainsAny() && refreshTokenEntityList.Count > 0)
            {
                var refreshTokenEntity = refreshTokenEntityList.FirstOrDefault();
                if (refreshTokenEntity.TokenReuseDetected || refreshTokenEntity.ConsumedAt.HasValue ||
                    refreshTokenEntity.ConsumedTime.HasValue)
                {
                    await HandleRefreshTokenReuseAsync(refreshTokenEntity);
                    return frameworkResultService.Failed<TokenResponseModel>(EndpointErrorCodes.TokenIsNullOrInvalid);
                }

                if (refreshTokenEntity.CreationTime.IsExpired(refreshTokenEntity.ExpiresAt))
                    return frameworkResultService.Failed<TokenResponseModel>(EndpointErrorCodes.TokenExpired);

                var algorithm = GetAlgorithmAsync(tokenRequest.Client.AllowedSigningAlgorithm);
                // create header
                var accessTokenHeader = await CreateJwtHeader(tokenRequest.Client.ClientSecret, algorithm, "at+jwt");
                var token = new JwtSecurityToken(refreshTokenEntity.TokenValue);

                var identity = new ClaimsIdentity(token.Claims);
                var expiryClaim = identity.FindFirst(ClaimTypes.Expiration);
                if (expiryClaim != null) identity.RemoveClaim(expiryClaim);

                var expiryTime = DateTime.UtcNow.AddSeconds(GetEffectiveAccessTokenLifetime(tokenRequest.Client))
                    .ToUnixTime();
                identity.AddClaim(new Claim(ClaimTypes.Expiration, expiryTime.ToString()));
                var payload = new JwtPayload(identity.Claims);

                var encodedAccessToken = accessTokenHeader.GenerateToken(payload);

                var consumedAt = DateTime.UtcNow;
                var rowsConsumed =
                    await securityTokenCommandRepository.ConsumeRefreshTokenAsync(refreshTokenEntity.Id, consumedAt);

                if (rowsConsumed != 1)
                    return frameworkResultService.Failed<TokenResponseModel>(EndpointErrorCodes.TokenIsNullOrInvalid);

                var nextRefreshTokenHandle = cryptoConfig.RandomStringLength.RandomString();
                var nextRefreshToken = new SecurityTokens
                {
                    Key = nextRefreshTokenHandle.ComputeSha256Hash(),
                    TokenType = TokenType.RefreshToken,
                    TokenValue = encodedAccessToken,
                    ExpiresAt = GetEffectiveRefreshTokenLifetime(tokenRequest.Client),
                    CreationTime = DateTime.UtcNow,
                    SubjectId = refreshTokenEntity.SubjectId,
                    ClientId = refreshTokenEntity.ClientId,
                    SessionId = refreshTokenEntity.SessionId,
                    ConsumedAt = null,
                    ConsumedTime = null,
                    TokenReuseDetected = false
                };
                await securityTokenRepository.InsertAsync(nextRefreshToken);
                var saveResult = await securityTokenRepository.SaveChangesAsync();
                if (saveResult.Status == ResultStatus.Failed)
                    ThrowIfPersistenceFailed(saveResult, EndpointErrorCodes.FailedToStoreRefreshToken);

                // assign access token and refresh token to the response.
                tokenResponse.AccessToken = encodedAccessToken;
                tokenResponse.RefreshToken = nextRefreshTokenHandle;
                tokenResponse.AccessTokenExpiresIn = GetEffectiveAccessTokenLifetime(tokenRequest.Client);
            }
            else
            {
                return frameworkResultService.Failed<TokenResponseModel>(EndpointErrorCodes.TokenIsNullOrInvalid);
            }
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, ex.Message);
            throw;
        }

        return tokenResponse;
    }

    private async Task<ResultClaimsModel> GetTokenClaimsAsync(ValidatedTokenRequestModel tokenRequest)
    {
        var parser = tokenRequest.AllowedScopesParserModel;
        var tokenDetails = tokenRequest.TokenDetails;
        loggerService.WriteTo(Log.Debug, "Entered into get token claims" + tokenDetails.Client.ClientName);
        var resultClaims = new ResultClaimsModel
        {
            AudienceClaims = new List<Claim>(),
            IdentityTokenScopeClaims = new List<Claim>(),
            AccessTokenScopeClaims = new List<Claim>(),
            IdentityClaims = new List<Claim>(),
            RoleClaims = new List<Claim>(),
            TransactionClaims = new List<Claim>(),
            PermissionClaims = new List<Claim>(),
            CustomAccessTokenClaims = new List<Claim>()
        };

        if (tokenRequest.GrantType != GrantTypes.ClientCredentials)
        {
            if (parser.CreateIdentityToken)
                resultClaims = await GetUserIdentityResourcesAsync(tokenDetails, resultClaims);

            resultClaims = await GetClaimsFromApiResources(tokenRequest, tokenDetails, resultClaims);
            resultClaims.IdentityTokenScopeClaims = tokenDetails.IdentityResourcesByScopes
                .Select(x => x.IdentityResourceName).Distinct().Select(x => new Claim(ClaimTypes.Scope, x)).ToList();
        }
        else
        {
            resultClaims = await GetClientCredentialScopes(tokenDetails, resultClaims);
        }

        resultClaims = await GetClaimsFromParsedTransactionScopes(parser.ParsedTransactionScopes, resultClaims);
        resultClaims.AudienceClaims = resultClaims.AudienceClaims.Distinct(new ClaimsComparer()).ToList();
        resultClaims.RoleClaims = resultClaims.RoleClaims.Distinct(new ClaimsComparer()).ToList();
        resultClaims.IdentityTokenScopeClaims =
            resultClaims.IdentityTokenScopeClaims.Distinct(new ClaimsComparer()).ToList();
        resultClaims.AccessTokenScopeClaims =
            resultClaims.AccessTokenScopeClaims.Distinct(new ClaimsComparer()).ToList();
        resultClaims.TransactionClaims = resultClaims.TransactionClaims.Distinct(new ClaimsComparer()).ToList();
        resultClaims.PermissionClaims = resultClaims.PermissionClaims.Distinct(new ClaimsComparer()).ToList();
        resultClaims.IdentityClaims = resultClaims.IdentityClaims.Distinct(new ClaimsComparer()).ToList();
        if (resultClaims.CustomAccessTokenClaims != null)
            resultClaims.CustomAccessTokenClaims = resultClaims.CustomAccessTokenClaims.Distinct(new ClaimsComparer()).ToList();
        return resultClaims;
    }

    private async Task<ResultClaimsModel> GetClaimsFromApiResources(ValidatedTokenRequestModel tokenRequest,
        TokenDetailsModel tokenDetails, ResultClaimsModel resultClaims)
    {
        var apiRoleClaims = new List<Claim>();
        var apiPermissionClaims = new List<Claim>();
        var userClaims = new List<Claim>();
        var scopePermissions =
            tokenRequest.AllowedScopesParserModel.ParsedApiScopes
                .ExpandPermissions(); //.Select(scopeClaim => new Claim(ClaimTypes.Permission, scopeClaim));

        if (tokenDetails.User != null && tokenDetails.User.UserClaims != null &&
            tokenDetails.User.UserClaims.ContainsAny())
        {
            var convertedUserclaims = from claim in tokenDetails.User.UserClaims
                select new Claim(claim.ClaimType, claim.ClaimValue);
            userClaims = convertedUserclaims.ToList();
        }

        var roleClaims = (from userRoleClaims in tokenDetails.UserRoleClaimTypes
            select new Claim(ClaimTypes.Role, userRoleClaims.RoleName)).Distinct(new ClaimsComparer()).ToList();

        var claimTypes = tokenDetails.ApiResourcesByScopes
            .Where(x => !string.IsNullOrWhiteSpace(x.ApiResourceClaimType)).Select(x => x.ApiResourceClaimType)
            .Union(tokenDetails.ApiResourcesByScopes.Where(x => !string.IsNullOrWhiteSpace(x.ApiScopeClaimType))
                .Select(x => x.ApiScopeClaimType))
            .Distinct();

        var userRoleclaims = (from userRoleClaims in tokenDetails.UserRoleClaimTypes.Where(roleClaimType =>
                    !string.IsNullOrWhiteSpace(roleClaimType.RoleClaimType) &&
                    !string.IsNullOrWhiteSpace(roleClaimType.RoleClaimValue))
                select new Claim(userRoleClaims.RoleClaimType.ToLower(), userRoleClaims.RoleClaimValue))
            .Distinct(new ClaimsComparer());

        var consolidatedUserClaims = userRoleclaims.Union(userClaims);

        foreach (var claimType in claimTypes)
            if (claimType == ClaimTypes.Role)
            {
                if (roleClaims.ContainsAny()) apiRoleClaims.AddRange(roleClaims);
            }
            else if (string.Equals(claimType, ClaimTypes.Capabilities, StringComparison.OrdinalIgnoreCase))
            {
                var capabilityClaims = consolidatedUserClaims
                    .Where(x => string.Equals(x.Type, ClaimTypes.Capabilities, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (capabilityClaims.ContainsAny())
                    resultClaims.CustomAccessTokenClaims.AddRange(capabilityClaims);
            }
            else
            {
                var selectedClaims = consolidatedUserClaims.Where(x => x.Type.ToLower() == claimType);

                if (selectedClaims.ContainsAny()) apiPermissionClaims.AddRange(selectedClaims);
            }

        // TODO : Return requested scope as output or detailed api scopes to be confirmed.
        //var scopeClaims = from scope in tokenDetails.ApiResourcesByScopes select (ClaimTypes.Permission, scope.ApiScopeName);
        //var newRoleClaims = from roleClaim in userRoleclaims select (roleClaim.Type, roleClaim.Value);
        //var apiScopeClaims = newRoleClaims.Intersect(scopeClaims).Select(x => new Claim(x.Item1, x.Item2));
        var permissionsList = apiPermissionClaims.Select(claim => claim.Value).ToList();
        var userPermissionClaims = permissionsList.ExpandPermissions();
        // var scopeClaimsList = userPermissionClaims.Where(x => !scopePermissions.Contains(x)).ToList();
        var scopeClaimsList =
            userPermissionClaims.Intersect(scopePermissions)
                .ToList(); //.Select(scope => new Claim(ClaimTypes.Permission, scope));
        var apiScopeClaims = scopeClaimsList.ShrinkPermissions()
            .Select(scope => new Claim(ClaimTypes.Permission, scope)).ToList();
        apiPermissionClaims = permissionsList.ShrinkPermissions()
            .Select(scope => new Claim(ClaimTypes.Permission, scope)).ToList();
        // audience claims
        var apiAudienceClaims = (from claims in tokenDetails.ApiResourcesByScopes
            where apiScopeClaims.ToList().ConvertAll(x => x.Value).Contains(claims.ApiScopeName)
            select new Claim(ClaimTypes.Audience, claims.ApiResourceName)).Distinct(new ClaimsComparer()).ToList();

        resultClaims.AudienceClaims.AddRange(apiAudienceClaims);
        resultClaims.AccessTokenScopeClaims.AddRange(apiScopeClaims);
        resultClaims.RoleClaims.AddRange(apiRoleClaims);
        resultClaims.PermissionClaims.AddRange(apiPermissionClaims);
        return await Task.FromResult(resultClaims);
    }


    private async Task<ResultClaimsModel> GetClientCredentialScopes(TokenDetailsModel tokenDetails,
        ResultClaimsModel resultClaims)
    {
        // TODO : Return requested scope as output or detailed api scopes to be confirmed.
        var scopeClaims = from scope in tokenDetails.ApiResourcesByScopes
            select new Claim(ClaimTypes.Scope, scope.ApiScopeName);

        // audience claims
        var apiAudienceClaims = from claims in tokenDetails.ApiResourcesByScopes
            select new Claim(ClaimTypes.Audience, claims.ApiResourceName);

        resultClaims.AudienceClaims.AddRange(apiAudienceClaims);
        resultClaims.AccessTokenScopeClaims.AddRange(scopeClaims);

        return await Task.FromResult(resultClaims);
    }

    private async Task StoreAccessTokenAsync(ValidatedTokenRequestModel tokenRequest, string accesstoken)
    {
        if (tokenRequest == null) frameworkResultService.Throw(EndpointErrorCodes.ArgumentNullError);

        if (tokenRequest.IsRequestFromAuthorizationEndpoint && string.IsNullOrWhiteSpace(accesstoken)) return;

        if (string.IsNullOrWhiteSpace(accesstoken)) frameworkResultService.Throw(EndpointErrorCodes.ArgumentNullError);

        var refreshTokenModel = new SecurityTokensModel();
        try
        {
            var accessTokenClaims = new JwtSecurityToken(accesstoken).Claims;
            if (accessTokenClaims != null)
            {
                var jwtClaim = accessTokenClaims.ToList().Find(x => x.Type == ClaimTypes.JwtId);
                if (jwtClaim != null)
                {
                    loggerService.WriteTo(Log.Debug,
                        "Entered into save access token for client :" + tokenRequest.Client.ClientName);

                    refreshTokenModel.Key = jwtClaim.Value;
                    refreshTokenModel.TokenValue = accesstoken;
                    refreshTokenModel.TokenType = TokenType.AccessToken;
                    refreshTokenModel.ExpiresAt = GetEffectiveAccessTokenLifetime(tokenRequest.TokenDetails.Client);
                    refreshTokenModel.SubjectId = tokenRequest.TokenDetails.User != null
                        ? tokenRequest.TokenDetails.User.Id.ToString()
                        : null;
                    refreshTokenModel.ClientId = tokenRequest.ClientId;
                    refreshTokenModel.SessionId = tokenRequest.SessionId;
                    refreshTokenModel.UserId = tokenRequest.UserName;
                    refreshTokenModel.CreationTime = DateTime.UtcNow;

                    var entityToken = mapper.Map<SecurityTokensModel, SecurityTokens>(refreshTokenModel);
                    await securityTokenRepository.InsertAsync(entityToken);
                    var result = await securityTokenRepository.SaveChangesAsync();
                    if (result.Status == ResultStatus.Failed)
                        frameworkResultService.ThrowCustomMessage(result.Errors.ToList()[0].Description);
                }
            }
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, ex.Message);
            throw;
        }
    }

    private async Task<FrameworkResult> StoreSecurityTokenAsync(SecurityTokensModel securityToken, string userId)
    {
        var entityToken = mapper.Map<SecurityTokensModel, SecurityTokens>(securityToken);
        if (securityToken.TokenType == TokenType.RefreshToken)
        {
            var consumedAt = DateTime.UtcNow;
            await securityTokenCommandRepository
                .ConsumeActiveRefreshTokensAsync(userId, securityToken.ClientId, consumedAt);

            await securityTokenRepository.InsertAsync(entityToken);
            return await securityTokenRepository.SaveChangesAsync();
        }

        var tokenList = await securityTokenRepository.GetAsync(entity => entity.SubjectId == userId &&
                                                                         entity.ClientId == securityToken.ClientId &&
                                                                         entity.TokenType == securityToken.TokenType);

        if (tokenList.ContainsAny())
        {
            var token = tokenList.FirstOrDefault();
            token.Key = entityToken.Key;
            token.TokenValue = entityToken.TokenValue;
            token.ExpiresAt = entityToken.ExpiresAt;
            token.SessionId = entityToken.SessionId;
            token.CreationTime = entityToken.CreationTime;

            loggerService.WriteTo(Log.Debug, "Entered into store security token for user : " + userId);
            await securityTokenRepository.UpdateAsync(token);
        }
        else
        {
            await securityTokenRepository.InsertAsync(entityToken);
        }

        return await securityTokenRepository.SaveChangesAsync();
    }

    private async Task HandleRefreshTokenReuseAsync(SecurityTokens refreshTokenEntity)
    {
        if (refreshTokenEntity == null) return;

        var relatedTokens = await securityTokenRepository.GetAsync(entity =>
            entity.TokenType == TokenType.RefreshToken
            && entity.SubjectId == refreshTokenEntity.SubjectId
            && entity.ClientId == refreshTokenEntity.ClientId);
        if (!relatedTokens.ContainsAny()) return;

        foreach (var token in relatedTokens)
        {
            token.TokenReuseDetected = true;
            token.ConsumedAt ??= DateTime.UtcNow;
            token.ConsumedTime ??= token.ConsumedAt;
            await securityTokenRepository.UpdateAsync(token);
        }

        await securityTokenRepository.SaveChangesAsync();
    }

    private void ThrowIfPersistenceFailed(FrameworkResult result, string fallbackErrorCode)
    {
        var storageError = result.Errors?.FirstOrDefault()?.Description;
        if (!string.IsNullOrWhiteSpace(storageError)) frameworkResultService.ThrowCustomMessage(storageError);

        frameworkResultService.Throw(fallbackErrorCode);
    }

    private static int ClampLifetime(int requestedLifetime, int maxLifetime)
    {
        var sanitizedLifetime = requestedLifetime <= 0 ? maxLifetime : requestedLifetime;
        return Math.Min(sanitizedLifetime, maxLifetime);
    }

    private static int GetEffectiveAccessTokenLifetime(ClientsModel client)
    {
        return ClampLifetime(client.AccessTokenExpiration, 900);
    }

    private static int GetEffectiveIdentityTokenLifetime(ClientsModel client)
    {
        return ClampLifetime(client.IdentityTokenExpiration, 900);
    }

    private static int GetEffectiveRefreshTokenLifetime(ClientsModel client)
    {
        return ClampLifetime(client.RefreshTokenExpiration, 86400);
    }

    private Task<JwtHeader> CreateJwtHeader(string clientSecret, string algorithm = "none",
        string tokenType = "JWT")
    {
        JwtHeader headerAccessToken = null;
        if (algorithm.StartsWith("HS"))
        {
            if (string.IsNullOrWhiteSpace(clientSecret))
                frameworkResultService.Throw(EndpointErrorCodes.ClientSecretInvalid);

            headerAccessToken = new JwtHeader(clientSecret.GetSymmetricCredentials(algorithm));
        }
        else if (algorithm.StartsWith("RS") || algorithm.StartsWith("PS") || algorithm.StartsWith("ES"))
        {
            headerAccessToken = new JwtHeader(keyStore.GetAsymmetricSigningCredentials(algorithm));

            if (keyStore.TryGetValue(algorithm, out var keyInfo) && !string.IsNullOrWhiteSpace(keyInfo.KeyId))
                headerAccessToken["kid"] = keyInfo.KeyId;

            var certificateHash = keyStore.GetAsymmetricCertificateHash(algorithm);
            if (!string.IsNullOrWhiteSpace(certificateHash)) headerAccessToken["x5t"] = certificateHash;
        }
        else
        {
            frameworkResultService.Throw(EndpointErrorCodes.SigningAlgorithmIsInvalid);
        }

        if (headerAccessToken != null)
        {
            if (headerAccessToken.ContainsKey("typ"))
                headerAccessToken["typ"] = tokenType;
            else
                headerAccessToken.Add("typ", tokenType);
        }

        return Task.FromResult(headerAccessToken);
    }

    private async Task<string> GenerateIdentityTokenAsync(ValidatedTokenRequestModel tokenRequest,
        ResultClaimsModel resultClaims)
    {
        var algorithm = GetAlgorithmAsync(tokenRequest.TokenDetails.Client.AllowedSigningAlgorithm);
        var audienceClaims = tokenRequest.TokenDetails.Client.ClientId;


        // create Jwt header.
        var headerIdentity = await CreateJwtHeader(tokenRequest.TokenDetails.Client.ClientSecret, algorithm);
        var claimslist = GetNormalizedClaimsList(tokenRequest, true);
        claimslist.AddRange(resultClaims.IdentityClaims);
        // create Jwt payload.
        var identityPayLoad = new JwtPayload(
            tokenRequest.Issuer,
            audienceClaims,
            claimslist,
            DateTime.UtcNow,
            DateTime.UtcNow.AddSeconds(GetEffectiveIdentityTokenLifetime(tokenRequest.TokenDetails.Client)),
            DateTime.UtcNow);
        loggerService.WriteTo(Log.Debug,
            "Entered into generate identity token for client" + tokenRequest.TokenDetails.Client.ClientName);
        var encodedIdentityToken = headerIdentity.GenerateToken(identityPayLoad);
        return encodedIdentityToken;
    }
}
