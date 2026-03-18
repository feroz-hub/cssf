/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace IntegrationTests.Endpoint.Helper;

public class JwksTestHelper : HCLCSSFFakeSetup
{
    private Task<SecurityToken> ValidateSymmetricToken(string token, string issuer = null, string audience = null,
        string clientSecret = null)
    {
        SecurityToken rawToken = null;
        var securityKey = Encoding.ASCII.GetBytes(clientSecret);
        var validationParameters = new TokenValidationParameters
        {
            ValidateAudience = audience != null,
            ValidateIssuer = issuer != null,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(securityKey),
            ValidateLifetime = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            LifetimeValidator = CustomLifetimeValidator,
            RequireExpirationTime = true
        };
        try
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out rawToken);
        }
        catch (Exception)
        {
        }

        return Task.FromResult(rawToken);
    }

    public bool CustomLifetimeValidator(DateTime? notBefore, DateTime? expires, SecurityToken tokenToValidate,
        TokenValidationParameters param)
    {
        if (expires != null) return expires > DateTime.UtcNow;

        return false;
    }

    public async Task<SecurityToken> ValidateToken(string token, string issuer = null, string audience = null,
        string clientSecret = null)
    {
        try
        {
            SecurityToken securityToken = null;
            var jwt = new JwtSecurityToken(token);
            var algorithm = jwt.SignatureAlgorithm;
            if (algorithm.StartsWith("RS") || algorithm.StartsWith("ES") || algorithm.StartsWith("PS"))
                securityToken = await ValidateAsymmetricToken(token, issuer, audience);
            else if (algorithm.StartsWith("HS"))
                securityToken = await ValidateSymmetricToken(token, issuer, audience, clientSecret);

            return securityToken;
        }
        catch (Exception)
        {
        }

        return null;
    }

    private async Task<SecurityToken> ValidateAsymmetricToken(string token, string issuer = null,
        string audience = null)
    {
        SecurityToken rawToken = null;
        try
        {
            var jwt = new JwtSecurityToken(token);
            var algorithm = jwt.SignatureAlgorithm;
            if (algorithm.StartsWith("RS") || algorithm.StartsWith("ES") || algorithm.StartsWith("PS"))
            {
                var result = await BackChannelClient.GetAsync(DiscoveryKeysEndpoint);
                var json = await result.Content.ReadAsStringAsync();
                var data = JObject.Parse(json);

                var keys = new List<SecurityKey>();
                foreach (var webKey in data["keys"])
                {
                    var key = new JsonWebKey
                    {
                        Kty = webKey["Kty"]?.ToString(),
                        Alg = webKey["Alg"]?.ToString(),
                        Kid = webKey["Kid"]?.ToString(),
                        X = webKey["X"]?.ToString(),
                        Y = webKey["Y"]?.ToString(),
                        Crv = webKey["Crv"]?.ToString(),
                        E = webKey["E"]?.ToString(),
                        N = webKey["N"]?.ToString(),
                        Use = webKey["sig"]?.ToString(),
                        X5t = webKey["thumbprint"]?.ToString()
                    };

                    keys.Add(key);
                }

                var validationParameters = new TokenValidationParameters
                {
                    ValidateAudience = audience != null,
                    ValidateIssuer = issuer != null,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = keys,
                    ValidateLifetime = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    LifetimeValidator = CustomLifetimeValidator,
                    RequireExpirationTime = true
                };
                try
                {
                    var principal =
                        new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out rawToken);
                }
                catch (Exception)
                {
                }
            }
        }
        catch (Exception)
        {
        }

        return rawToken;
    }
}
