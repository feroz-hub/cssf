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

namespace HCL.CS.SF.Service.Implementation.Endpoint.Specifications;

/// <summary>
/// Validates the initial structure and parameters of an OAuth 2.0 / OpenID Connect token request.
/// Checks that the grant type is present and recognized, that it does not exceed length limits,
/// and that the client's signing algorithm is one of the supported asymmetric algorithms.
/// </summary>
internal sealed class TokenRequestSpecification : BaseRequestModelValidator<ValidatedTokenRequestModel>
{
    /// <summary>
    /// Initializes the token request validation pipeline with rules for grant type and algorithm checks.
    /// </summary>
    internal TokenRequestSpecification()
    {
        // Ensure a grant_type parameter is present and is one of the recognized OAuth grant types
        Add("CheckGrantType", new Rule<ValidatedTokenRequestModel>(
            new CheckGrantType(),
            OpenIdConstants.Errors.UnsupportedGrantType,
            EndpointErrorCodes.GrantTypeIsMissing));

        // Enforce maximum length restriction on the grant_type value
        Add("CheckLengthRestrictions", new Rule<ValidatedTokenRequestModel>(
            new CheckLengthRestrictions<ValidatedTokenRequestModel>(
                request => request.GetValue(OpenIdConstants.TokenRequest.GrantType),
                request => request.TokenConfigOptions.InputLengthRestrictionsConfig.GrantType,
                request => ">"),
            OpenIdConstants.Errors.UnsupportedGrantType,
            EndpointErrorCodes.GrantTypeTooLong));

        // Validate that the client's configured signing algorithm is RS256 or ES256
        Add("CheckAlgorithm", new Rule<ValidatedTokenRequestModel>(
            new CheckAlgorithm(),
            OpenIdConstants.Errors.UnsupportedAlgorithm,
            EndpointErrorCodes.SigningAlgorithmIsInvalid));
    }
}

/// <summary>
/// Specification that validates the grant_type parameter is present, non-empty,
/// and matches one of the allowed OAuth 2.0 grant types defined in <see cref="OpenIdConstants.GrantTypes"/>.
/// </summary>
internal class CheckGrantType : ISpecification<ValidatedTokenRequestModel>
{
    /// <summary>
    /// Checks whether the grant_type value is non-empty and exists in the allowed grant types list.
    /// </summary>
    /// <param name="model">The validated token request model containing the raw request data.</param>
    /// <returns>True if the grant type is valid; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedTokenRequestModel model)
    {
        var grantType = model.GetValue(OpenIdConstants.TokenRequest.GrantType);

        if (string.IsNullOrWhiteSpace(grantType)) return false;

        // Reflect all allowed grant type constants and perform case-insensitive comparison
        var allowedGrantTypes = typeof(OpenIdConstants.GrantTypes).GetArray().ToList().ConvertAll(x => x.ToLower());
        if (!allowedGrantTypes.Contains(grantType.ToLower())) return false;

        return true;
    }
}

/// <summary>
/// Specification that validates the client's configured signing algorithm is one of
/// the supported asymmetric algorithms (RS256 or ES256).
/// </summary>
internal class CheckAlgorithm : ISpecification<ValidatedTokenRequestModel>
{
    /// <summary>
    /// Checks whether the client's allowed signing algorithm is a supported asymmetric algorithm.
    /// </summary>
    /// <param name="model">The validated token request model containing client configuration.</param>
    /// <returns>True if the algorithm is RS256 or ES256; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedTokenRequestModel model)
    {
        var algorithm = model.Client.AllowedSigningAlgorithm;
        if (string.IsNullOrWhiteSpace(algorithm)) return false;

        var allowedSigningAlgorithms = new[]
        {
            OpenIdConstants.Algorithms.RsaSha256,
            OpenIdConstants.Algorithms.EcdsaSha256
        };

        if (!allowedSigningAlgorithms.Contains(algorithm, StringComparer.Ordinal)) return false;

        return true;
    }
}
