/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.Domain.Models.Endpoint.Validation;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.DomainServices.Wrappers;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Implementation.Endpoint.Specifications;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Validators;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Validators;

/// <summary>
/// Validates OAuth 2.0 token requests by checking the grant type and delegating
/// to the appropriate flow-specific specification (authorization_code, client_credentials,
/// password, refresh_token, or user_code). Includes PKCE validation for authorization code flow.
/// </summary>
internal class TokenRequestValidator : ITokenRequestValidator
{
    private readonly IAuthenticationService authenticationService;
    private readonly IAuthorizationService authorizationService;
    private readonly TokenSettings configSettings;
    private readonly IFrameworkResultService frameworkResultService;
    private readonly ILoggerService loggerService;
    private readonly IResourceScopeValidator resourceScopeValidator;
    private readonly ITokenGenerationService tokenGenerationService;
    private readonly UserManagerWrapper<Users> userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenRequestValidator"/> class.
    /// </summary>
    public TokenRequestValidator(
        ILoggerInstance instance,
        IFrameworkResultService frameworkResultService,
        UserManagerWrapper<Users> userManager,
        IAuthenticationService authenticationService,
        IAuthorizationService authorizationService,
        ITokenGenerationService tokenGenerationService,
        IResourceScopeValidator resourceScopeValidator,
        HCLCSSFConfig tokenSettings)
    {
        this.frameworkResultService = frameworkResultService;
        this.userManager = userManager;
        this.authenticationService = authenticationService;
        this.authorizationService = authorizationService;
        this.tokenGenerationService = tokenGenerationService;
        this.resourceScopeValidator = resourceScopeValidator;
        configSettings = tokenSettings.TokenSettings;
        loggerService = instance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
    }

    /// <summary>
    /// Validates a token request by first checking general parameters (grant type, algorithm),
    /// then routing to the appropriate grant-type-specific validation specification.
    /// </summary>
    /// <param name="requestCollection">The parsed form parameters from the token request.</param>
    /// <param name="clientValidationModel">The validated client information.</param>
    /// <returns>A <see cref="ValidatedTokenRequestModel"/> with validation results and parsed parameters.</returns>
    public async Task<ValidatedTokenRequestModel> ValidateTokenRequestAsync(
        Dictionary<string, string> requestCollection, ClientSecretValidationModel clientValidationModel)
    {
        var validatedTokenRequestModel = new ValidatedTokenRequestModel
        {
            RequestRawData = requestCollection,
            TokenConfigOptions = configSettings
        };

        if (clientValidationModel == null) frameworkResultService.Throw(EndpointErrorCodes.InvalidClientObject);

        validatedTokenRequestModel.SetClient(clientValidationModel?.Client, clientValidationModel?.Secret);

        var tokenRequestValidation = new TokenRequestSpecification();
        var validationError = await tokenRequestValidation.ValidateAsync(validatedTokenRequestModel);
        if (tokenRequestValidation.IsValid)
        {
            loggerService.WriteTo(Log.Debug, "Entered into validate token request.");
            validatedTokenRequestModel.Issuer = configSettings.TokenConfig.IssuerUri;
            switch (validatedTokenRequestModel.GetValue(OpenIdConstants.TokenRequest.GrantType))
            {
                case OpenIdConstants.GrantTypes.AuthorizationCode:
                    loggerService.WriteTo(Log.Debug, "Entered into authorization code flow.");
                    validatedTokenRequestModel.GrantType = OpenIdConstants.GrantTypes.AuthorizationCode;
                    var authorizationCodeValidation =
                        new AuthorizationCodeFlowSpecification(authorizationService, userManager);
                    validationError = await authorizationCodeValidation.ValidateAsync(validatedTokenRequestModel);
                    if (authorizationCodeValidation.IsValid
                        && (validatedTokenRequestModel.Client.RequirePkce
                            || !string.IsNullOrWhiteSpace(validatedTokenRequestModel.AuthorizationCode.CodeChallenge)))
                    {
                        loggerService.WriteTo(Log.Debug,
                            "Client required a proof key for code exchange. Starting PKCE validation");
                        var proofKeyParametersValidation = new ProofKeyParametersSpecification();
                        validationError = await proofKeyParametersValidation.ValidateAsync(validatedTokenRequestModel);
                    }

                    break;
                case OpenIdConstants.GrantTypes.ClientCredentials:
                    loggerService.WriteTo(Log.Debug, "Entered into client credentials flow.");
                    validatedTokenRequestModel.GrantType = OpenIdConstants.GrantTypes.ClientCredentials;
                    var clientCredentialsValidation =
                        new ClientCredentialsFlowSpecification(resourceScopeValidator);
                    validationError = await clientCredentialsValidation.ValidateAsync(validatedTokenRequestModel);

                    break;
                case OpenIdConstants.GrantTypes.Password:
                    loggerService.WriteTo(Log.Debug, "Entered into resource owner password flow.");
                    validatedTokenRequestModel.GrantType = OpenIdConstants.GrantTypes.Password;
                    var resourceOwnerPasswordValidation =
                        new ResourceOwnerFlowSpecification(resourceScopeValidator, authenticationService, userManager);
                    validationError =
                        await resourceOwnerPasswordValidation.ValidateAsync(validatedTokenRequestModel);

                    break;
                case OpenIdConstants.GrantTypes.RefreshToken:
                    loggerService.WriteTo(Log.Debug, "Entered into refresh token flow.");
                    validatedTokenRequestModel.GrantType = OpenIdConstants.GrantTypes.RefreshToken;
                    var refreshTokenFlowValidation = new RefreshTokenFlowSpecification(tokenGenerationService);
                    validationError = await refreshTokenFlowValidation.ValidateAsync(validatedTokenRequestModel);

                    break;
                case OpenIdConstants.GrantTypes.UserCode:
                    loggerService.WriteTo(Log.Debug, "Entered into user_code (external sign-in) flow.");
                    validatedTokenRequestModel.GrantType = OpenIdConstants.GrantTypes.UserCode;
                    var userCodeFlowValidation =
                        new UserCodeFlowSpecification(resourceScopeValidator, authorizationService, userManager);
                    validationError = await userCodeFlowValidation.ValidateAsync(validatedTokenRequestModel);

                    break;
            }
        }

        if (validationError != null && !string.IsNullOrWhiteSpace(validationError.ErrorCode))
        {
            validationError = frameworkResultService.Failed(validationError.ErrorCode, validationError.ErrorMessage);
            validatedTokenRequestModel.ErrorCode = validationError.ErrorCode;
            validatedTokenRequestModel.ErrorDescription = validationError.ErrorMessage;
            loggerService.WriteTo(Log.Error, validationError.ErrorMessage);
        }
        else
        {
            validatedTokenRequestModel.IsError = false;
        }

        return validatedTokenRequestModel;
    }
}
