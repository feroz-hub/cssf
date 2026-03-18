/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.Service.Implementation.Endpoint.Specifications;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Validators;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Validators;

/// <summary>
/// Validates OAuth 2.0 Token Revocation requests per RFC 7009.
/// Ensures the token is present, the token type hint is valid, and performs the actual revocation.
/// </summary>
internal class TokenRevocationRequestValidator : ITokenRevocationRequestValidator
{
    private readonly TokenSettings configSettings;
    private readonly IFrameworkResultService frameworkResultService;
    private readonly ILoggerService loggerService;
    private readonly IRepository<SecurityTokens> securityTokenRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenRevocationRequestValidator"/> class.
    /// </summary>
    public TokenRevocationRequestValidator(
        ILoggerInstance instance,
        IFrameworkResultService frameworkResultService,
        IRepository<SecurityTokens> securityTokenRepository,
        HCLCSSFConfig tokenSettings)
    {
        configSettings = tokenSettings.TokenSettings;
        this.frameworkResultService = frameworkResultService;
        this.securityTokenRepository = securityTokenRepository;
        loggerService = instance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
    }

    /// <summary>
    /// Validates a token revocation request and executes the revocation if valid.
    /// </summary>
    /// <param name="requestCollection">The parsed form parameters.</param>
    /// <param name="client">The authenticated client making the revocation request.</param>
    /// <returns>A <see cref="ValidatedRevocationRequestModel"/> with the validation result.</returns>
    public async Task<ValidatedRevocationRequestModel> ValidateRevocationRequestAsync(
        Dictionary<string, string> requestCollection,
        ClientsModel client)
    {
        loggerService.WriteTo(Log.Debug, "Entered into revocation request validation.");

        var validatedRequestModel = new ValidatedRevocationRequestModel
        {
            RequestRawData = requestCollection,
            TokenConfigOptions = configSettings,
            Client = client,
            ClientId = client?.ClientId
        };

        var tokenRevocationRequestValidation = new TokenRevocationRequestSpecification(securityTokenRepository);
        var validationError = await tokenRevocationRequestValidation.ValidateAsync(validatedRequestModel);
        if (!tokenRevocationRequestValidation.IsValid)
        {
            validationError = frameworkResultService.Failed(validationError.ErrorCode, validationError.ErrorMessage);
            validatedRequestModel.ErrorCode = validationError.ErrorCode;
            validatedRequestModel.ErrorDescription = validationError.ErrorMessage;
            return validatedRequestModel;
        }

        validatedRequestModel.IsError = false;
        return validatedRequestModel;
    }
}
