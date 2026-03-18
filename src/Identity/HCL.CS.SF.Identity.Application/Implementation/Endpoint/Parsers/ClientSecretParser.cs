/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Http;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Parsers;


// TODO - MutualTlsSecretParser Parser
namespace HCL.CS.SF.Service.Implementation.Endpoint.Parsers;

/// <summary>
/// Parses client credentials from incoming HTTP requests per OAuth 2.0 client authentication methods.
/// Supports client_secret_basic (HTTP Basic auth), client_secret_post (form body),
/// private_key_jwt (client assertion), and mutual TLS (mTLS) certificate-based authentication.
/// Attempts each method in order until one succeeds.
/// </summary>
internal class ClientSecretParser : IClientSecretParser
{
    private readonly TokenSettings configSettings;
    private readonly IFrameworkResultService frameworkResultService;
    private readonly Dictionary<string, AsymmetricKeyInfoModel> keyStore;
    private readonly ILoggerService loggerService;


    /// <summary>
    /// Initializes a new instance of the <see cref="ClientSecretParser"/> class.
    /// </summary>
    public ClientSecretParser(
        ILoggerInstance instance,
        HCLCSSFConfig tokenSettings,
        IFrameworkResultService frameworkResultService,
        Dictionary<string, AsymmetricKeyInfoModel> keyStore
    )
    {
        loggerService = instance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
        configSettings = tokenSettings.TokenSettings;
        this.frameworkResultService = frameworkResultService;
        this.keyStore = keyStore;
    }

    /// <summary>
    /// Attempts to parse client credentials from the request using all supported authentication methods.
    /// Tries Basic auth header first, then POST body, then JWT client assertion, then mTLS.
    /// </summary>
    /// <param name="context">The HTTP context containing client credentials.</param>
    /// <returns>A <see cref="ParsedSecretModel"/> with the parsed client ID and credential.</returns>
    public async Task<ParsedSecretModel> ParseAsync(HttpContext context)
    {
        ParsedSecretModel parsedSecret = null;
        var secret = await ClientSecretBasic(context);
        if (secret.IsError || secret.Type == AuthenticationConstants.ParsedTypes.NoSecret)
        {
            secret = await ClientSecretPost(context);
            if (secret.IsError && secret.Type != AuthenticationConstants.ParsedTypes.NoSecret)
            {
                secret = await ClientSecretAndPrivateKeyJwt(context);
                if (secret.IsError && secret.Type != AuthenticationConstants.ParsedTypes.NoSecret)
                {
                    secret = await ClientSecretMtls(context);
                    if (secret.IsError && secret.Type != AuthenticationConstants.ParsedTypes.NoSecret)
                        parsedSecret = secret;
                }
            }
            else
            {
                parsedSecret = secret;
            }
        }
        else
        {
            parsedSecret = secret;
        }

        if (parsedSecret != null) return parsedSecret;

        loggerService.WriteTo(Log.Error, "Parsing failed.");
        return new ParsedSecretModel { IsError = true };
    }

    private async Task<ParsedSecretModel> ClientSecretBasic(HttpContext context)
    {
        loggerService.WriteTo(Log.Debug, "Parsing client secret basic.");

        string credentials;
        var parserType = "Basic ";
        var parsedSecret = new ParsedSecretModel { IsError = true };
        var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authorizationHeader) ||
            !authorizationHeader.StartsWith(parserType, StringComparison.OrdinalIgnoreCase))
            return await Task.FromResult(parsedSecret);

        var parameter = authorizationHeader[parserType.Length..];
        try
        {
            credentials = Encoding.UTF8.GetString(Convert.FromBase64String(parameter));
        }
        catch (Exception)
        {
            loggerService.WriteToWithCaller(Log.Error, "Client authentication failed.");
            return await Task.FromResult(parsedSecret);
        }

        var position = credentials.IndexOf(':');
        if (position == -1)
        {
            loggerService.WriteTo(Log.Error, "Invalid client credentials.");
            return await Task.FromResult(parsedSecret);
        }

        var clientId = credentials.Substring(0, position);
        var clientSecret = credentials[(position + 1)..];

        if (!string.IsNullOrWhiteSpace(clientId))
        {
            if (clientId.Length > configSettings.InputLengthRestrictionsConfig.ClientId)
                frameworkResultService.Throw(EndpointErrorCodes.ClientIdMaxLengthExceeded);

            if (!string.IsNullOrWhiteSpace(clientSecret))
            {
                if (clientSecret.Length > configSettings.InputLengthRestrictionsConfig.ClientSecret)
                    frameworkResultService.Throw(EndpointErrorCodes.ClientSecretMaxLengthExceeded);

                parsedSecret = new ParsedSecretModel
                {
                    ClientId = DecodeBasicCredentialComponent(clientId),
                    Credential = DecodeBasicCredentialComponent(clientSecret),
                    Type = AuthenticationConstants.ParsedTypes.SharedSecret,
                    IsError = false,
                    ParseMethod = ParseMethods.Basic
                };

                return await Task.FromResult(parsedSecret);
            }

            loggerService.WriteTo(Log.Debug, "Client id without secret found.");
            parsedSecret = new ParsedSecretModel
            {
                ClientId = DecodeBasicCredentialComponent(clientId),
                Type = AuthenticationConstants.ParsedTypes.NoSecret,
                IsError = false,
                ParseMethod = ParseMethods.Basic
            };

            return await Task.FromResult(parsedSecret);
        }

        loggerService.WriteTo(Log.Debug, "No secret found parsing client secret basic.");
        return await Task.FromResult(parsedSecret);
    }

    private async Task<ParsedSecretModel> ClientSecretAndPrivateKeyJwt(HttpContext context)
    {
        loggerService.WriteTo(Log.Debug, "Parsing client secret jwt.");
        var parsedSecret = new ParsedSecretModel { IsError = true };
        if (!context.Request.CheckHeaderContentType())
        {
            frameworkResultService.Failed<ParsedSecretModel>(EndpointErrorCodes.HeaderContentIsNotProper);
            return parsedSecret;
        }

        var body = await context.Request.ReadFormAsync();
        if (body != null)
        {
            var clientAssertionType = body[OpenIdConstants.TokenRequest.ClientAssertionType].FirstOrDefault();
            var clientAssertion = body[OpenIdConstants.TokenRequest.ClientAssertion].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(clientAssertion) &&
                clientAssertionType == OpenIdConstants.ClientAssertionTypes.JwtBearer)
            {
                if (clientAssertion.Length > configSettings.InputLengthRestrictionsConfig.Jwt)
                    frameworkResultService.Throw(EndpointErrorCodes.TokenMaxLengthExceeded);

                var clientId = GetClientIdFromToken(clientAssertion);
                if (string.IsNullOrWhiteSpace(clientId))
                {
                    frameworkResultService.Failed<ParsedSecretModel>(EndpointErrorCodes.ClientIdMissingInSecret);
                    return parsedSecret;
                }

                if (clientId?.Length > configSettings.InputLengthRestrictionsConfig.ClientId)
                    frameworkResultService.Throw(EndpointErrorCodes.ClientIdMaxLengthExceeded);
                var jwtToken = new JwtSecurityToken(clientAssertion);
                var parsemethod = ParseMethods.JwtSecret;
                if (jwtToken.SignatureAlgorithm.StartsWith("PS") || jwtToken.SignatureAlgorithm.StartsWith("RS"))
                    parsemethod = ParseMethods.JwtSecret;
                else if (jwtToken.SignatureAlgorithm.StartsWith("HS")) parsemethod = ParseMethods.JwtSecret;
                return new ParsedSecretModel
                {
                    ClientId = clientId,
                    Credential = clientAssertion,
                    Type = AuthenticationConstants.ParsedTypes.JwtBearer,
                    IsError = false,
                    ParseMethod = parsemethod
                };
            }
        }

        loggerService.WriteTo(Log.Debug, "No secret found parsing client secret jwt.");
        return parsedSecret;
    }

    private async Task<ParsedSecretModel> ClientSecretPost(HttpContext context)
    {
        loggerService.WriteTo(Log.Debug, "Parsing Client Secret Post");
        var parsedSecret = new ParsedSecretModel { IsError = true };
        if (!context.Request.CheckHeaderContentType()) return parsedSecret;

        var body = await context.Request.ReadFormAsync();
        if (body != null)
        {
            var clientId = body[OpenIdConstants.TokenRequest.ClientId].FirstOrDefault();
            var clientSecret = body[OpenIdConstants.TokenRequest.ClientSecret].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(clientId))
            {
                if (clientId.Length > configSettings.InputLengthRestrictionsConfig.ClientId)
                    frameworkResultService.Throw(EndpointErrorCodes.ClientIdMaxLengthExceeded);

                if (!string.IsNullOrWhiteSpace(clientSecret))
                {
                    if (clientSecret.Length > configSettings.InputLengthRestrictionsConfig.ClientSecret)
                        frameworkResultService.Throw(EndpointErrorCodes.ClientSecretMaxLengthExceeded);

                    return new ParsedSecretModel
                    {
                        ClientId = clientId,
                        Credential = clientSecret,
                        Type = AuthenticationConstants.ParsedTypes.SharedSecret,
                        IsError = false,
                        ParseMethod = ParseMethods.Post
                    };
                }

                loggerService.WriteTo(Log.Debug, "client id without secret found");
                return new ParsedSecretModel
                {
                    ClientId = clientId,
                    Type = AuthenticationConstants.ParsedTypes.NoSecret,
                    IsError = false,
                    ParseMethod = ParseMethods.Post
                };
            }
        }

        loggerService.WriteTo(Log.Debug, "No secret found parsing client secret post.");
        return parsedSecret;
    }

    private async Task<ParsedSecretModel> ClientSecretMtls(HttpContext context)
    {
        loggerService.WriteTo(Log.Debug, "Parsing Client Secret Mtls");
        var parsedSecret = new ParsedSecretModel { IsError = true };

        if (!context.Request.CheckHeaderContentType()) return parsedSecret;

        var body = await context.Request.ReadFormAsync();

        if (body != null)

        {
            var Clientid = body[OpenIdConstants.TokenRequest.ClientId].FirstOrDefault();

            // client id must be present


            if (!string.IsNullOrWhiteSpace(Clientid))
            {
                if (Clientid.Length > configSettings.InputLengthRestrictionsConfig.ClientId)
                    frameworkResultService.Throw(EndpointErrorCodes.ClientIdMaxLengthExceeded);

                var clientCertificate = await context.Connection.GetClientCertificateAsync();

                if (clientCertificate is null)
                {
                    loggerService.WriteTo(Log.Debug, "Client certificate not present");
                    return parsedSecret;
                }

                return new ParsedSecretModel
                {
                    ClientId = Clientid,
                    Credential = clientCertificate,
                    Type = AuthenticationConstants.SecretTypes.X509Certificate
                };
            }
        }

        loggerService.WriteTo(Log.Debug, "No post body found");
        return parsedSecret;
    }

    private static string DecodeBasicCredentialComponent(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;

        // Preserve literal '+' in secrets while still honoring percent-encoding.
        try
        {
            return Uri.UnescapeDataString(value);
        }
        catch (UriFormatException)
        {
            return value;
        }
    }

    private string GetClientIdFromToken(string token)
    {
        try
        {
            var jwt = new JwtSecurityToken(token);
            return jwt.Subject;
        }
        catch (Exception)
        {
            loggerService.WriteTo(Log.Error, "Client identifier not found in token.");
            return null;
        }
    }
}
