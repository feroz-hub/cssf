/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Validators;

// TODO: PrivateKeyJwtSecretValidator, X509NameSecretValidator, X509ThumbprintSecretValidator need to be added
namespace HCL.CS.SF.Service.Implementation.Endpoint.Validators;

/// <summary>
/// Validates client secrets against stored values. Supports shared secret comparison
/// (plain, SHA-256, SHA-512 hashes) and private_key_jwt assertion validation
/// using RSA/HMAC signing algorithms.
/// </summary>
internal class SecretValidator : ISecretValidator
{
    private readonly IFrameworkResultService frameworkResultService;
    private readonly HttpContext httpContext;
    private readonly Dictionary<string, AsymmetricKeyInfoModel> keyStore;
    private readonly ILoggerService loggerService;

    private readonly TokenSettings tokenSettings;
    //private readonly ITokenReplayCache tokenReplayCache;


    /// <summary>
    /// Initializes a new instance of the <see cref="SecretValidator"/> class.
    /// </summary>
    public SecretValidator(IHttpContextAccessor httpContextAccessor, ILoggerInstance instance,
        IFrameworkResultService frameworkResultService, Dictionary<string, AsymmetricKeyInfoModel> keyStore,
        HCLCSSFConfig config)
    {
        httpContext = httpContextAccessor.HttpContext;
        this.frameworkResultService = frameworkResultService;
        loggerService = instance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
        this.keyStore = keyStore;
        tokenSettings = config.TokenSettings;
    }

    /// <summary>
    /// Validates a parsed client secret against the client's stored secret.
    /// Checks for secret expiration, then delegates to shared secret or JWT comparison.
    /// </summary>
    /// <param name="client">The client model with the stored secret.</param>
    /// <param name="parsedSecret">The parsed secret from the request.</param>
    /// <returns>True if the secret is valid; false otherwise.</returns>
    public async Task<bool> ValidateSecretAsync(ClientsModel client, ParsedSecretModel parsedSecret)
    {
        var expiredSecrets = client.ClientSecretExpiresAt < DateTime.UtcNow;
        if (expiredSecrets)
        {
            loggerService.WriteTo(Log.Error, "Expired secret");
            return false;
        }

        if (parsedSecret.Type == AuthenticationConstants.ParsedTypes.SharedSecret)
            return await CompareClientSecret(client.ClientSecret, parsedSecret);

        if (parsedSecret.Type == AuthenticationConstants.ParsedTypes.JwtBearer)
            return await CompareJwtSecret(client, parsedSecret);

        loggerService.WriteTo(Log.Error, "Secret validators could not validate secret.");
        return false;
    }

    /// <summary>
    /// Compares the provided shared secret against the stored secret using plain, SHA-256, and SHA-512 hash comparisons.
    /// Uses constant-time comparison to prevent timing attacks.
    /// </summary>
    /// <param name="secret">The stored client secret (may be plain or hashed).</param>
    /// <param name="parsedSecret">The parsed secret from the request.</param>
    /// <returns>True if any comparison succeeds; false otherwise.</returns>
    internal Task<bool> CompareClientSecret(string secret, ParsedSecretModel parsedSecret)
    {
        var sharedSecret = Convert.ToString(parsedSecret.Credential);
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(sharedSecret))
            return Task.FromResult(false);

        var secretSha256 = sharedSecret.Sha256();
        var secretSha512 = sharedSecret.Sha512();
        var isValid = secret.CompareStrings(sharedSecret)
                      || secret.CompareStrings(secretSha256)
                      || secret.CompareStrings(secretSha512);

        if (!isValid) loggerService.WriteTo(Log.Debug, "No matching secret found.");
        return Task.FromResult(isValid);
    }

    /// <summary>
    /// Validates a private_key_jwt client assertion (RFC 7523). Verifies the JWT signature
    /// using HMAC or RSA algorithms, validates issuer/audience/expiry, and checks that
    /// sub == iss (both must be the client_id) and jti is present.
    /// </summary>
    /// <param name="client">The client model containing the stored secret and signing algorithm.</param>
    /// <param name="parsedSecret">The parsed JWT assertion from the request.</param>
    /// <returns>True if the JWT assertion is valid; false otherwise.</returns>
    internal Task<bool> CompareJwtSecret(ClientsModel client, ParsedSecretModel parsedSecret)
    {
        var token = parsedSecret.Credential.ToString();

        try
        {
            var tokenKey = keyStore.GetTokenKey(client);

            if (tokenKey != null)
            {
                var jwt = new JwtSecurityToken(token);

                var audience = httpContext.GetHCLCSSFBaseUrl().IncludeEndSlash() +
                               OpenIdConstants.EndpointRoutePaths.Token;
                if (jwt.SignatureAlgorithm.StartsWith("HS"))
                {
                    var (decodedToken, _) =
                        token.ValidateSymmetricJwtToken(client.ClientSecret, client.ClientId, audience);
                    return Task.FromResult(true);
                }

                if (jwt.SignatureAlgorithm.StartsWith("RS") || jwt.SignatureAlgorithm.StartsWith("PS"))
                {
                    var securityKeys = new List<SecurityKey>();
                    foreach (var keyvalue in keyStore)
                    {
                        var key = keyvalue.Value.Certificate
                            .GenerateAsymmetricVerificationCredentials(keyvalue.Key);
                        securityKeys.Add(key.Key);
                    }

                    var tokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKeys = securityKeys,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = tokenSettings.TokenConfig.IssuerUri,
                        ValidateIssuer = true,

                        ValidAudience = audience,
                        ValidateAudience = true,

                        RequireSignedTokens = true,
                        RequireExpirationTime = true,

                        ClockSkew = TimeSpan.FromMinutes(5)
                    };

                    var handler = new JwtSecurityTokenHandler();
                    handler.ValidateToken(token, tokenValidationParameters, out var validatedtoken);
                    var jwtToken = (JwtSecurityToken)validatedtoken;

                    if (jwtToken.Subject != jwtToken.Issuer)
                    {
                        loggerService.WriteTo(Log.Error,
                            "Both 'sub' and 'iss' in the client assertion token must have a value of client_id.");
                        return Task.FromResult(false);
                    }

                    var exp = jwtToken.Payload.Exp;
                    if (!exp.HasValue)
                    {
                        loggerService.WriteTo(Log.Error, "exp is missing in client assertion JWT.");
                        return Task.FromResult(false);
                    }

                    var jti = jwtToken.Payload.Jti;
                    if (jti.IsNull())
                    {
                        loggerService.WriteTo(Log.Error, "jti is missing in client assertion JWT.");
                        return Task.FromResult(false);
                    }

                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
        }
        catch (Exception)
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(false);
    }
}
