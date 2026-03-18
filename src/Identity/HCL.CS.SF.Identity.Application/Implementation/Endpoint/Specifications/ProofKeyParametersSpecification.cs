/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Text;
using System.Text.RegularExpressions;
using DomainValidation.Interfaces.Specification;
using DomainValidation.Validation;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Implementation.Endpoint.Validators;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Specifications;

/// <summary>
/// Validates PKCE (Proof Key for Code Exchange) parameters during the authorization code token exchange.
/// Ensures the code_challenge, code_challenge_method, and code_verifier are present, properly formatted,
/// within length bounds, use a supported challenge method, and that the verifier matches the challenge.
/// </summary>
/// <summary>
/// Validates PKCE (RFC 7636) parameters during the token exchange step of the authorization code flow.
/// Verifies code_challenge, code_challenge_method, and code_verifier are present and valid,
/// then checks that the code_verifier matches the stored code_challenge using S256 or plain method.
/// </summary>
internal sealed class ProofKeyParametersSpecification : BaseRequestModelValidator<ValidatedTokenRequestModel>
{
    /// <summary>
    /// Initializes the PKCE validation pipeline with all required proof key checks.
    /// </summary>
    internal ProofKeyParametersSpecification()
    {
        // Ensure the code_challenge from the authorization request is present
        Add("CheckCodeChallenge", new Rule<ValidatedTokenRequestModel>(
            new IsRequestNull<ValidatedTokenRequestModel>(request => request.AuthorizationCode.CodeChallenge),
            OpenIdConstants.Errors.InvalidGrant,
            EndpointErrorCodes.ClientMissingCodeChallenge));

        // Ensure the code_challenge_method from the authorization request is present
        Add("CheckCodeChallengeMethod", new Rule<ValidatedTokenRequestModel>(
            new IsRequestNull<ValidatedTokenRequestModel>(request => request.AuthorizationCode.CodeChallengeMethod),
            OpenIdConstants.Errors.InvalidGrant,
            EndpointErrorCodes.ClientMissingCodeChallengeMethod));

        // Ensure the code_verifier parameter is present in the token request
        Add("CheckCodeVerifier", new Rule<ValidatedTokenRequestModel>(
            new IsRequestNull<ValidatedTokenRequestModel>(request =>
                request.GetValue(OpenIdConstants.TokenRequest.CodeVerifier)),
            OpenIdConstants.Errors.InvalidGrant,
            EndpointErrorCodes.CodeVerifierMissing));

        // Enforce minimum length on the code_verifier (RFC 7636: min 43 chars)
        Add("CheckCodeVerifierLength<", new Rule<ValidatedTokenRequestModel>(
            new CheckLengthRestrictions<ValidatedTokenRequestModel>(
                request => request.GetValue(OpenIdConstants.TokenRequest.CodeVerifier),
                request => request.TokenConfigOptions.InputLengthRestrictionsConfig.CodeVerifierMinLength,
                request => "<"), OpenIdConstants.Errors.InvalidGrant,
            EndpointErrorCodes.CodeVerifierTooShort));

        // Enforce maximum length on the code_verifier (RFC 7636: max 128 chars)
        Add("CheckCodeVerifierLength>", new Rule<ValidatedTokenRequestModel>(
            new CheckLengthRestrictions<ValidatedTokenRequestModel>(
                request => request.GetValue(OpenIdConstants.TokenRequest.CodeVerifier),
                request => request.TokenConfigOptions.InputLengthRestrictionsConfig.CodeVerifierMaxLength,
                request => ">"), OpenIdConstants.Errors.InvalidGrant,
            EndpointErrorCodes.CodeVerifierTooLong));

        // Validate the code_verifier uses only allowed characters per RFC 7636
        Add("CheckCodeVerifierCharset", new Rule<ValidatedTokenRequestModel>(
            new CheckCodeVerifierCharset(),
            OpenIdConstants.Errors.InvalidGrant,
            EndpointErrorCodes.InvalidCodeVerifier));

        // Ensure the code_challenge_method is S256 (or plain if explicitly allowed)
        Add("CheckSupportedCodeChallengeMethod", new Rule<ValidatedTokenRequestModel>(
            new CheckSupportedCodeChallengeMethod(),
            OpenIdConstants.Errors.InvalidGrant,
            EndpointErrorCodes.UnsupportedCodeChallengeMethod));

        // Validate the code_challenge is in Base64URL format
        Add("CheckCodeChallengeFormat", new Rule<ValidatedTokenRequestModel>(
            new CheckCodeChallengeFormat(),
            OpenIdConstants.Errors.InvalidGrant,
            EndpointErrorCodes.InvalidCodeChallenge));

        // Verify the code_verifier matches the original code_challenge
        Add("CheckCodeVerifierAgainstCodeChallenge", new Rule<ValidatedTokenRequestModel>(
            new CheckCodeVerifierAgainstCodeChallenge(),
            OpenIdConstants.Errors.InvalidGrant,
            EndpointErrorCodes.UnsupportedCodeChallengeMethod));
    }
}

/// <summary>
/// Specification that validates the code_challenge_method is a supported PKCE method.
/// S256 is always supported; plain is only supported if the client explicitly allows it.
/// </summary>
internal class CheckSupportedCodeChallengeMethod : ISpecification<ValidatedTokenRequestModel>
{
    /// <summary>
    /// Checks whether the stored code challenge method is S256 or an allowed plain method.
    /// </summary>
    /// <param name="model">The token request model containing the authorization code details.</param>
    /// <returns>True if the challenge method is supported; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedTokenRequestModel model)
    {
        if (model.AuthorizationCode == null) return false;

        if (model.AuthorizationCode.CodeChallengeMethod == OpenIdConstants.CodeChallengeMethods.Sha256)
            return true;

        // Plain method requires explicit client opt-in
        return model.Client.IsPkceTextPlain &&
               model.AuthorizationCode.CodeChallengeMethod == OpenIdConstants.CodeChallengeMethods.Plain;
    }
}

/// <summary>
/// Specification that validates the code_challenge is a well-formed Base64URL-encoded string.
/// </summary>
internal class CheckCodeChallengeFormat : ISpecification<ValidatedTokenRequestModel>
{
    /// <summary>
    /// Compiled regex for Base64URL character set validation.
    /// </summary>
    private static readonly Regex Base64UrlPattern =
        new("^[A-Za-z0-9_-]+$", RegexOptions.CultureInvariant | RegexOptions.Compiled);

    /// <summary>
    /// Checks that the code_challenge is non-empty and matches the Base64URL pattern.
    /// </summary>
    /// <param name="model">The token request model.</param>
    /// <returns>True if the format is valid; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedTokenRequestModel model)
    {
        var challenge = model.AuthorizationCode?.CodeChallenge;
        return !string.IsNullOrWhiteSpace(challenge) && Base64UrlPattern.IsMatch(challenge);
    }
}

/// <summary>
/// Specification that validates the code_verifier uses only characters allowed by RFC 7636:
/// unreserved characters [A-Z], [a-z], [0-9], "-", ".", "_", "~".
/// </summary>
internal class CheckCodeVerifierCharset : ISpecification<ValidatedTokenRequestModel>
{
    /// <summary>
    /// Compiled regex for RFC 7636 code verifier character set.
    /// </summary>
    private static readonly Regex CodeVerifierPattern =
        new("^[A-Za-z0-9\\-._~]+$", RegexOptions.CultureInvariant | RegexOptions.Compiled);

    /// <summary>
    /// Validates that the code_verifier contains only permitted characters.
    /// </summary>
    /// <param name="model">The token request model containing the code_verifier.</param>
    /// <returns>True if the character set is valid; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedTokenRequestModel model)
    {
        var verifier = model.GetValue(OpenIdConstants.TokenRequest.CodeVerifier);
        if (string.IsNullOrWhiteSpace(verifier)) return false;

        return CodeVerifierPattern.IsMatch(verifier);
    }
}

/// <summary>
/// Specification that verifies the code_verifier against the stored code_challenge.
/// For the "plain" method, performs a direct string comparison.
/// For the "S256" method, computes SHA-256 of the verifier and compares the Base64URL-encoded result.
/// </summary>
/// <summary>
/// Verifies the PKCE code_verifier against the stored code_challenge.
/// For S256: SHA256(code_verifier) must equal the stored code_challenge (base64url-encoded).
/// For plain: code_verifier must exactly match the code_challenge.
/// </summary>
internal class CheckCodeVerifierAgainstCodeChallenge : ISpecification<ValidatedTokenRequestModel>
{
    /// <summary>
    /// Compares the code_verifier to the code_challenge using the appropriate challenge method.
    /// </summary>
    /// <param name="model">The token request model containing PKCE parameters.</param>
    /// <returns>True if the verifier matches the challenge; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedTokenRequestModel model)
    {
        if (model.AuthorizationCode == null) return true;

        // For plain method, the verifier must exactly match the challenge
        if (model.AuthorizationCode.CodeChallengeMethod == OpenIdConstants.CodeChallengeMethods.Plain)
            return model.CodeVerifier.CompareStrings(model.AuthorizationCode.CodeChallenge);

        // For S256: challenge = BASE64URL(SHA256(ASCII(code_verifier)))
        var codeVerifierBytes = Encoding.ASCII.GetBytes(model.CodeVerifier);
        var hashedBytes = codeVerifierBytes.Sha256();
        var transformedCodeVerifier = hashedBytes.Encode();

        return transformedCodeVerifier.CompareStrings(model.AuthorizationCode.CodeChallenge);
    }
}
