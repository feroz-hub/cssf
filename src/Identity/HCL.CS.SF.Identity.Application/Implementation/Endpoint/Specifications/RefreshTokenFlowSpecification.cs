/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using DomainValidation.Interfaces.Specification;
using DomainValidation.Validation;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Implementation.Endpoint.Validators;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Specifications;

/// <summary>
/// Validates an OAuth 2.0 refresh_token grant type token request.
/// Ensures the refresh token is present, within length limits, and can be validated
/// against the token store (not expired, not consumed, not reused).
/// </summary>
/// <summary>
/// Validates the token request for the OAuth 2.0 Refresh Token grant type (RFC 6749 Section 6).
/// Checks that the refresh_token parameter is present, within length limits, and valid
/// (not expired, not consumed, not reuse-detected).
/// </summary>
internal sealed class RefreshTokenFlowSpecification : BaseRequestModelValidator<ValidatedTokenRequestModel>
{
    /// <summary>
    /// Initializes the refresh token flow validation pipeline.
    /// </summary>
    /// <param name="tokenGenerationService">Service for validating refresh tokens against stored data.</param>
    internal RefreshTokenFlowSpecification(ITokenGenerationService tokenGenerationService)
    {
        // Ensure the refresh_token parameter is present
        Add("CheckRefreshToken", new Rule<ValidatedTokenRequestModel>(
            new IsRequestNull<ValidatedTokenRequestModel>(request =>
                request.GetValue(OpenIdConstants.TokenRequest.RefreshToken)),
            OpenIdConstants.Errors.InvalidRequest,
            EndpointErrorCodes.RefreshTokenMissing));

        // Enforce maximum length restriction on the refresh token value
        Add("CheckTokenLengthRestrictions", new Rule<ValidatedTokenRequestModel>(
            new CheckLengthRestrictions<ValidatedTokenRequestModel>(
                request => request.GetValue(OpenIdConstants.TokenRequest.RefreshToken),
                request => request.TokenConfigOptions.InputLengthRestrictionsConfig.RefreshToken,
                request => ">"),
            OpenIdConstants.Errors.InvalidGrant,
            EndpointErrorCodes.RefreshTokenTooLong));

        // Validate the refresh token against the token store (expiry, consumption, reuse detection)
        Add("ValidateRefreshToken", new Rule<ValidatedTokenRequestModel>(
            new ValidateRefreshToken(tokenGenerationService),
            OpenIdConstants.Errors.InvalidGrant,
            EndpointErrorCodes.RefreshTokenValidationFailed));
    }
}

/// <summary>
/// Specification that validates a refresh token by delegating to the token generation service,
/// which checks the token's existence, expiry, consumption status, and reuse detection.
/// </summary>
internal class ValidateRefreshToken : ISpecification<ValidatedTokenRequestModel>
{
    private readonly ITokenGenerationService tokenGenerationService;

    /// <summary>
    /// Initializes the refresh token validator with the token generation service.
    /// </summary>
    /// <param name="tokenGenerationService">Service that handles refresh token lifecycle validation.</param>
    internal ValidateRefreshToken(ITokenGenerationService tokenGenerationService)
    {
        this.tokenGenerationService = tokenGenerationService;
    }

    /// <summary>
    /// Validates the refresh token handle against the persistent store and client configuration.
    /// </summary>
    /// <param name="model">The token request model containing the refresh_token parameter.</param>
    /// <returns>True if the refresh token is valid; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedTokenRequestModel model)
    {
        var refreshTokenHandle = model.GetValue(OpenIdConstants.TokenRequest.RefreshToken);
        var result = tokenGenerationService.ValidateRefreshTokenAsync(refreshTokenHandle, model.Client).GetAwaiter()
            .GetResult();

        if (result.IsError) return false;

        model.RequestedRefreshToken = refreshTokenHandle;
        return true;
    }
}
