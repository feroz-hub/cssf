/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Linq.Expressions;
using System.Text.RegularExpressions;
using AutoMapper;
using DomainValidation.Interfaces.Specification;
using DomainValidation.Validation;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Implementation.Endpoint.Validators;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Validators;
using static HCL.CS.SF.Domain.Constants.Endpoint.AuthenticationConstants;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Specifications;

/// <summary>
/// Validates an OAuth 2.0 / OpenID Connect authorization request.
/// Performs a comprehensive validation pipeline including JWT request rejection, client identification,
/// redirect URI validation, client existence check, signing algorithm verification, response type/mode
/// validation, PKCE parameters, scope validation, and core OIDC parameters (state, nonce, prompt, max_age).
/// </summary>
/// <summary>
/// Comprehensive specification for validating OAuth 2.0 / OIDC authorization requests.
/// Validates client_id, redirect_uri, response_type, response_mode, scopes, PKCE parameters,
/// state, nonce, prompt, and max_age per RFC 6749 and OpenID Connect Core 1.0.
/// </summary>
internal sealed class AuthorizeRequestSpecification : BaseRequestModelValidator<ValidatedAuthorizeRequestModel>
{
    /// <summary>
    /// Initializes the authorization request validation pipeline with all required rules.
    /// </summary>
    /// <param name="resourceScopeValidator">Validator for scope resolution and permission checks.</param>
    /// <param name="clientRepository">Repository for loading client entities from the database.</param>
    /// <param name="mapper">AutoMapper instance for entity-to-model mapping.</param>
    /// <param name="session">Session management service for session-related validation.</param>
    /// <param name="configSettings">Token configuration settings.</param>
    internal AuthorizeRequestSpecification(
        IResourceScopeValidator resourceScopeValidator,
        IRepository<Clients> clientRepository,
        IMapper mapper,
        ISessionManagementService session,
        TokenSettings configSettings)
    {
        // Reject JWT request objects and request_uri parameters (not currently supported)
        Add("ValidateJwtRequestUri", new Rule<ValidatedAuthorizeRequestModel>(
            new ValidateJwtRequest(),
            OpenIdConstants.Errors.InvalidRequest,
            EndpointErrorCodes.JwtRequestUriNotSupported));

        // Ensure client_id is present in the request
        Add("ValidateRequestedClient", new Rule<ValidatedAuthorizeRequestModel>(
            new ValidateRequestedClientId(),
            OpenIdConstants.Errors.InvalidRequest,
            EndpointErrorCodes.ClientIdMissingInRequest));

        // Enforce maximum length on the client_id value
        Add("ValidateRequestedClientLength", new Rule<ValidatedAuthorizeRequestModel>(
            new CheckLengthRestrictions<ValidatedAuthorizeRequestModel>(
                request => request.GetValue(OpenIdConstants.AuthorizeRequest.ClientId),
                request => request.TokenConfigOptions.InputLengthRestrictionsConfig.ClientId,
                request => ">"),
            OpenIdConstants.Errors.InvalidRequest,
            EndpointErrorCodes.ClientIdTooLong));

        // Ensure redirect_uri is present
        Add("ValidateUriNull", new Rule<ValidatedAuthorizeRequestModel>(
            new IsRequestNull<ValidatedAuthorizeRequestModel>(request =>
                request.GetValue(OpenIdConstants.AuthorizeRequest.RedirectUri)), OpenIdConstants.Errors.InvalidRequest,
            EndpointErrorCodes.InvalidRedirectUri));

        // Enforce maximum length on redirect_uri
        Add("ValidateUriLength", new Rule<ValidatedAuthorizeRequestModel>(
            new CheckLengthRestrictions<ValidatedAuthorizeRequestModel>(
                request => request.GetValue(OpenIdConstants.AuthorizeRequest.RedirectUri),
                request => request.TokenConfigOptions.InputLengthRestrictionsConfig.RedirectUri,
                request => ">"),
            OpenIdConstants.Errors.InvalidRequest,
            EndpointErrorCodes.RedirectUriTooLong));

        // Validate redirect_uri is a well-formed URL
        Add("ValidateUriFormat", new Rule<ValidatedAuthorizeRequestModel>(
            new IsRequestValidUri<ValidatedAuthorizeRequestModel>(request =>
                request.GetValue(OpenIdConstants.AuthorizeRequest.RedirectUri)),
            OpenIdConstants.Errors.InvalidRequest,
            EndpointErrorCodes.InvalidRedirectUri));

        // Verify the client exists in the database and load its configuration
        Add("CheckValidClientFromDB", new Rule<ValidatedAuthorizeRequestModel>(
            new CheckValidClient(clientRepository, mapper),
            OpenIdConstants.Errors.UnauthorizedClient,
            EndpointErrorCodes.ClientDoesNotExist));

        // Verify the client's signing algorithm is RS256 or ES256 (if configured)
        Add("CheckConfidentialClientSigningAlgorithm", new Rule<ValidatedAuthorizeRequestModel>(
            new CheckConfidentialClientSigningAlgorithm(),
            OpenIdConstants.Errors.UnauthorizedClient,
            EndpointErrorCodes.SigningAlgorithmIsInvalid));

        // Validate the response_type parameter and derive grant type and response mode
        Add("CheckResponseType", new Rule<ValidatedAuthorizeRequestModel>(
            new CheckResponseType(),
            OpenIdConstants.Errors.UnsupportedResponseType,
            EndpointErrorCodes.ResponseTypeMissing));

        // For authorization code flow, ensure redirect_uri is present (mandatory)
        Add("CheckClientRedirectUriForAuthCodeFlow", new Rule<ValidatedAuthorizeRequestModel>(
            new CheckClientRedirectUriForAuthCodeFlow(),
            OpenIdConstants.Errors.InvalidRequest,
            EndpointErrorCodes.RedirectURIIsMandatory));

        // Verify the redirect_uri matches one of the client's registered redirect URIs
        Add("CheckValidClientRedirectUris", new Rule<ValidatedAuthorizeRequestModel>(
            new CheckClientRedirectUri(request => request.Client.RedirectUris.ConvertAll(uri => uri.RedirectUri)),
            OpenIdConstants.Errors.InvalidRequest,
            EndpointErrorCodes.RedirectUriNotRegistered));

        ValidatePkceParameters();
        ValidateResponseMode();
        ValidateScopes(resourceScopeValidator);
        ValidateCoreParameters(session, configSettings);
    }

    /// <summary>
    /// Adds PKCE (code_challenge / code_challenge_method) validation rules for authorization code flow.
    /// </summary>
    public void ValidatePkceParameters()
    {
        Add("ValidatePkce", new Rule<ValidatedAuthorizeRequestModel>(
            new ValidatePkce(),
            OpenIdConstants.Errors.InvalidRequest,
            EndpointErrorCodes.InvalidCodeChallenge));
    }

    /// <summary>
    /// Adds response mode, grant type authorization, and access token via browser validation rules.
    /// </summary>
    private void ValidateResponseMode()
    {
        // Validate the optional response_mode parameter against allowed modes
        Add(
            "CheckResponseMode", new Rule<ValidatedAuthorizeRequestModel>(
                new CheckResponseMode(),
                OpenIdConstants.Errors.UnsupportedResponseType,
                EndpointErrorCodes.InvalidResponseMode));

        // Verify the client is authorized for the derived grant type
        Add("CheckAllowedGrantTypeForClient", new Rule<ValidatedAuthorizeRequestModel>(
            new CheckAllowedGrantTypeForClient(),
            OpenIdConstants.Errors.UnsupportedGrantType,
            EndpointErrorCodes.InvalidGrantTypeForClient));

        // If response_type includes "token", verify the client allows access tokens via browser
        Add("CheckAccessTokenInResponseType", new Rule<ValidatedAuthorizeRequestModel>(
            new CheckAccessTokenInResponseType(),
            OpenIdConstants.Errors.InvalidRequest,
            EndpointErrorCodes.ClientNotConfiguredToReceiveAccessToken));
    }

    /// <summary>
    /// Adds validation rules for core OIDC parameters: state, nonce, prompt, and max_age.
    /// </summary>
    /// <param name="session">Session management service (reserved for future session validation).</param>
    /// <param name="configSettings">Token configuration settings for length restrictions.</param>
    private void ValidateCoreParameters(ISessionManagementService session, TokenSettings configSettings)
    {
        Add("CheckState", new Rule<ValidatedAuthorizeRequestModel>(
            new CheckState(),
            OpenIdConstants.Errors.InvalidRequest,
            EndpointErrorCodes.InvalidState));

        Add("CheckNonce", new Rule<ValidatedAuthorizeRequestModel>(
            new CheckNonce(),
            OpenIdConstants.Errors.InvalidRequest,
            EndpointErrorCodes.InvalidNonce));

        Add("CheckPrompt", new Rule<ValidatedAuthorizeRequestModel>(
            new CheckPrompt(),
            OpenIdConstants.Errors.InvalidRequest,
            EndpointErrorCodes.InvalidPrompt));

        Add("CheckMaxAge", new Rule<ValidatedAuthorizeRequestModel>(
            new CheckMaxAge(),
            OpenIdConstants.Errors.InvalidRequest,
            EndpointErrorCodes.InvalidMaxAge));
    }

    /// <summary>
    /// Adds scope validation rules: scope presence, length, plausibility with response type,
    /// client permission, openid scope requirements, identity scope requirements, and offline_access restrictions.
    /// </summary>
    /// <param name="resourceScopeValidator">Validator for scope resolution.</param>
    private void ValidateScopes(IResourceScopeValidator resourceScopeValidator)
    {
        Add("CheckRequestedScopes", new Rule<ValidatedAuthorizeRequestModel>(
            new CheckRequestedScopes<ValidatedAuthorizeRequestModel>(),
            OpenIdConstants.Errors.InvalidScope,
            EndpointErrorCodes.InvalidScopeOrNotAllowed));

        Add("CheckScopeLengthRestrictions", new Rule<ValidatedAuthorizeRequestModel>(
            new CheckLengthRestrictions<ValidatedAuthorizeRequestModel>(
                request => request.GetValue(OpenIdConstants.AuthorizeRequest.Scope),
                request => request.TokenConfigOptions.InputLengthRestrictionsConfig.Scope,
                request => ">"),
            OpenIdConstants.Errors.InvalidRequest,
            EndpointErrorCodes.ScopeTooLong));

        Add("CheckScopeResponseTypePlausibility", new Rule<ValidatedAuthorizeRequestModel>(
            new CheckScopeResponseTypePlausibility(),
            OpenIdConstants.Errors.InvalidRequest,
            EndpointErrorCodes.ResponseTypeRequiresOpenIdScope));

        Add("CheckScopesAllowedForClient", new Rule<ValidatedAuthorizeRequestModel>(
            new CheckScopesAllowedForClient(resourceScopeValidator),
            OpenIdConstants.Errors.InvalidRequest,
            EndpointErrorCodes.RequestedScopeNotAllowedForClient));

        Add("CheckOpenId", new Rule<ValidatedAuthorizeRequestModel>(
            new CheckOpenId(),
            OpenIdConstants.Errors.InvalidScope,
            EndpointErrorCodes.OpenIdScopeMissing));

        Add("CheckScopeRequirementIdentity", new Rule<ValidatedAuthorizeRequestModel>(
            new CheckScopeRequirementIdentity(),
            OpenIdConstants.Errors.InvalidScope,
            EndpointErrorCodes.MustIncludeIdentityScopes));

        Add("CheckOfflineAccessForCodeIdToken", new Rule<ValidatedAuthorizeRequestModel>(
            new CheckOfflineAccessForCodeIdToken(),
            OpenIdConstants.Errors.InvalidScope,
            EndpointErrorCodes.MustNotIncludeOfflineAccessScope));
    }
}

/// <summary>
/// Specification that rejects authorization requests containing JWT request objects or request_uri parameters,
/// as these features are not currently supported.
/// </summary>
internal class ValidateJwtRequest : ISpecification<ValidatedAuthorizeRequestModel>
{
    // TODO: Check with team on removing this validation and related attributes.

    /// <summary>
    /// Returns true (valid) only if neither the request nor request_uri parameters are present.
    /// </summary>
    /// <param name="model">The authorization request model.</param>
    /// <returns>True if no JWT request parameters are present; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedAuthorizeRequestModel model)
    {
        var jwtRequest = model.GetValue(OpenIdConstants.AuthorizeRequest.Request);
        var jwtRequestUri = model.GetValue(OpenIdConstants.AuthorizeRequest.RequestUri);
        return string.IsNullOrWhiteSpace(jwtRequest) || string.IsNullOrWhiteSpace(jwtRequestUri);
    }
}

/// <summary>
/// Specification that validates the client_id parameter is present and non-empty.
/// </summary>
internal class ValidateRequestedClientId : ISpecification<ValidatedAuthorizeRequestModel>
{
    /// <summary>
    /// Checks the client_id is present and stores it on the model.
    /// </summary>
    /// <param name="model">The authorization request model.</param>
    /// <returns>True if client_id is present; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedAuthorizeRequestModel model)
    {
        var clientId = model.GetValue(OpenIdConstants.AuthorizeRequest.ClientId);
        if (string.IsNullOrWhiteSpace(clientId)) return false;

        model.ClientId = clientId;
        return true;
    }
}

/// <summary>
/// Specification that verifies the client exists in the database and loads its full configuration
/// including redirect URIs and post-logout redirect URIs.
/// </summary>
internal class CheckValidClient : ISpecification<ValidatedAuthorizeRequestModel>
{
    private readonly IRepository<Clients> clientRepository;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes the client validator with repository and mapper dependencies.
    /// </summary>
    /// <param name="clientRepository">Repository for loading client entities.</param>
    /// <param name="mapper">AutoMapper for entity-to-model mapping.</param>
    internal CheckValidClient(
        IRepository<Clients> clientRepository,
        IMapper mapper)
    {
        this.clientRepository = clientRepository;
        this.mapper = mapper;
    }

    /// <summary>
    /// Looks up the client by client_id and maps it to the model.
    /// </summary>
    /// <param name="model">The authorization request model.</param>
    /// <returns>True if the client exists; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedAuthorizeRequestModel model)
    {
        var clientId = model.GetValue(OpenIdConstants.AuthorizeRequest.ClientId);

        // Load client with eagerly loaded redirect URIs
        var clientsEntity = clientRepository.GetAsync(
            client => client.ClientId == clientId,
            new System.Linq.Expressions.Expression<Func<HCL.CS.SF.Domain.Entities.Endpoint.Clients, object>>[] { x => x.RedirectUris, x => x.PostLogoutRedirectUris }).GetAwaiter().GetResult();

        if (!clientsEntity.ContainsAny()) return false;

        model.Client = mapper.Map<Clients, ClientsModel>(clientsEntity[0]);
        return true;
    }
}

/// <summary>
/// Specification that validates the client's signing algorithm is a supported asymmetric algorithm
/// (RS256 or ES256). Clients with no explicit algorithm configured pass this check.
/// </summary>
internal class CheckConfidentialClientSigningAlgorithm : ISpecification<ValidatedAuthorizeRequestModel>
{
    /// <summary>
    /// Validates the client's signing algorithm configuration.
    /// </summary>
    /// <param name="model">The authorization request model with loaded client.</param>
    /// <returns>True if the algorithm is valid or not configured; false if unsupported.</returns>
    public bool IsSatisfiedBy(ValidatedAuthorizeRequestModel model)
    {
        if (model.Client == null) return false;

        // No algorithm configured is acceptable (will default later)
        if (string.IsNullOrWhiteSpace(model.Client.AllowedSigningAlgorithm)) return true;

        return string.Equals(model.Client.AllowedSigningAlgorithm, OpenIdConstants.Algorithms.RsaSha256,
                   StringComparison.Ordinal)
               || string.Equals(model.Client.AllowedSigningAlgorithm, OpenIdConstants.Algorithms.EcdsaSha256,
                   StringComparison.Ordinal);
    }
}

/// <summary>
/// Specification that validates the redirect_uri matches one of the client's registered redirect URIs.
/// Uses exact string comparison per OAuth 2.0 security best practices.
/// </summary>
internal class CheckClientRedirectUri : ISpecification<ValidatedAuthorizeRequestModel>
{
    private readonly Expression<Func<ValidatedAuthorizeRequestModel, List<string>>> expression;

    /// <summary>
    /// Initializes with an expression to extract the list of registered redirect URIs.
    /// </summary>
    /// <param name="expression">Expression to get the client's redirect URIs.</param>
    internal CheckClientRedirectUri(Expression<Func<ValidatedAuthorizeRequestModel, List<string>>> expression)
    {
        this.expression = expression;
    }

    /// <summary>
    /// Compares the requested redirect_uri against the client's registered URIs using exact matching.
    /// </summary>
    /// <param name="model">The authorization request model.</param>
    /// <returns>True if the URI matches or is not provided; false if it doesn't match any registered URI.</returns>
    public bool IsSatisfiedBy(ValidatedAuthorizeRequestModel model)
    {
        var value = expression.Compile()(model);

        if (value == null) return false;

        var type = value.GetType();
        if (!string.IsNullOrWhiteSpace(model.RedirectUri))
        {
            // Perform exact string comparison against registered URIs
            if (type == typeof(List<string>) &&
                value.Any(uri => string.Equals(uri, model.RedirectUri, StringComparison.Ordinal))) return true;

            return false;
        }

        return true;
    }
}

/// <summary>
/// Specification that enforces the redirect_uri is mandatory for authorization code flow requests.
/// </summary>
internal class CheckClientRedirectUriForAuthCodeFlow : ISpecification<ValidatedAuthorizeRequestModel>
{
    /// <summary>
    /// Checks that redirect_uri is present when the grant type is authorization_code.
    /// </summary>
    /// <param name="model">The authorization request model.</param>
    /// <returns>True if not auth code flow or redirect_uri is present; false if missing for auth code.</returns>
    public bool IsSatisfiedBy(ValidatedAuthorizeRequestModel model)
    {
        return model.GrantType != GrantType.AuthorizationCode
               || !string.IsNullOrWhiteSpace(model.RedirectUri);
    }
}

// TODO: Check and remove this spec[CheckAllowedForAuthorizeEndpoint] below if its not required. Currently its not referred anywhere.

/// <summary>
/// Specification that checks whether the grant type is allowed for the authorize endpoint.
/// Currently unused but retained for potential future use.
/// </summary>
internal class CheckAllowedForAuthorizeEndpoint : ISpecification<ValidatedAuthorizeRequestModel>
{
    private readonly Expression<Func<ValidatedAuthorizeRequestModel, string>> expression;

    /// <summary>
    /// Initializes with an expression to extract the grant type value.
    /// </summary>
    /// <param name="expression">Expression to get the grant type.</param>
    internal CheckAllowedForAuthorizeEndpoint(Expression<Func<ValidatedAuthorizeRequestModel, string>> expression)
    {
        this.expression = expression;
    }

    /// <summary>
    /// Checks if the value is an allowed grant type for the authorize endpoint.
    /// </summary>
    /// <param name="model">The authorization request model.</param>
    /// <returns>True if the grant type is allowed; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedAuthorizeRequestModel model)
    {
        var value = expression.Compile()(model);
        var type = value.GetType();
        return type == typeof(string) && AllowedGrantTypesForAuthorizeEndpoint.Contains(value);
    }
}

/// <summary>
/// Specification that validates PKCE parameters (code_challenge and code_challenge_method) for
/// authorization code flow requests. PKCE is required for public clients and clients with RequirePkce enabled.
/// </summary>
/// <summary>
/// Validates PKCE (Proof Key for Code Exchange, RFC 7636) parameters for authorization code requests.
/// Ensures code_challenge is present and well-formed, code_challenge_method is S256 (or plain if allowed),
/// and length constraints are met. Required for public clients and clients with RequirePkce enabled.
/// </summary>
internal class ValidatePkce : ISpecification<ValidatedAuthorizeRequestModel>
{
    /// <summary>
    /// Compiled regex for Base64URL character set validation of the code_challenge.
    /// </summary>
    private static readonly Regex CodeChallengePattern =
        new("^[A-Za-z0-9_-]+$", RegexOptions.CultureInvariant | RegexOptions.Compiled);

    /// <summary>
    /// Validates PKCE parameters: presence, length, format, and challenge method.
    /// </summary>
    /// <param name="model">The authorization request model.</param>
    /// <returns>True if PKCE is valid or not required; false if PKCE validation fails.</returns>
    public bool IsSatisfiedBy(ValidatedAuthorizeRequestModel model)
    {
        if (model.GrantType == GrantType.AuthorizationCode)
        {
            var codeChallenge = model.GetValue(OpenIdConstants.AuthorizeRequest.CodeChallenge);
            var codeChallengeMethod = model.GetValue(OpenIdConstants.AuthorizeRequest.CodeChallengeMethod);

            // PKCE is required for public clients (no client secret) or when explicitly configured
            var isPublicClient = !model.Client.RequireClientSecret;
            if (model.Client.RequirePkce || isPublicClient || !string.IsNullOrWhiteSpace(codeChallenge))
            {
                if (string.IsNullOrWhiteSpace(codeChallenge)) return false;

                // Validate code_challenge length is within configured bounds
                if (codeChallenge.Length <
                    model.TokenConfigOptions.InputLengthRestrictionsConfig.CodeChallengeMinLength ||
                    codeChallenge.Length >
                    model.TokenConfigOptions.InputLengthRestrictionsConfig.CodeChallengeMaxLength)
                    return false;

                // Validate code_challenge uses only Base64URL characters
                if (!CodeChallengePattern.IsMatch(codeChallenge)) return false;

                model.CodeChallenge = codeChallenge;

                if (string.IsNullOrWhiteSpace(codeChallengeMethod)) return false;

                // Validate the challenge method is S256 or plain (if explicitly allowed by the client)
                var allowsPlain = model.Client.IsPkceTextPlain &&
                                  codeChallengeMethod.Equals(
                                      OpenIdConstants.CodeChallengeMethods.Plain,
                                      StringComparison.Ordinal);
                var allowsSha256 = codeChallengeMethod.Equals(
                    OpenIdConstants.CodeChallengeMethods.Sha256,
                    StringComparison.Ordinal);
                if (!allowsPlain && !allowsSha256) return false;

                model.CodeChallengeMethod = codeChallengeMethod;
            }
        }

        return true;
    }
}

/// <summary>
/// Specification that validates the response_type parameter, derives the grant type and response mode,
/// and populates the model with redirect_uri.
/// </summary>
internal class CheckResponseType : ISpecification<ValidatedAuthorizeRequestModel>
{
    /// <summary>
    /// Validates response_type is present, recognized, and maps to an allowed grant type.
    /// </summary>
    /// <param name="model">The authorization request model.</param>
    /// <returns>True if response_type is valid; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedAuthorizeRequestModel model)
    {
        var responseType = model.GetValue(OpenIdConstants.AuthorizeRequest.ResponseType);
        var state = model.GetValue(OpenIdConstants.AuthorizeRequest.State);

        if (!string.IsNullOrWhiteSpace(state)) model.State = state;

        if (string.IsNullOrWhiteSpace(responseType)) return false;

        // check response type is in supported list
        if (!AllowedResponseTypes.Contains(responseType)) return false;

        if (!AllowedGrantTypeForResponseType.Keys.Contains(responseType)) return false;

        model.ResponseType = responseType;
        // Derive the grant type and default response mode from the response_type mapping
        if (AllowedGrantTypesForAuthorizeEndpoint.Contains(AllowedGrantTypeForResponseType[model.ResponseType]))
        {
            model.GrantType = AllowedGrantTypeForResponseType[model.ResponseType];
            model.ResponseMode = AllowedResponseModesForGrantType[model.GrantType].First();
            model.RedirectUri = model.GetValue(OpenIdConstants.AuthorizeRequest.RedirectUri);
        }
        else
        {
            return false;
        }

        return true;
    }
}

/// <summary>
/// Specification that validates whether the derived grant type is allowed for the authorize endpoint.
/// </summary>
internal class CheckAllowedGrantTypeForEndpoint : ISpecification<ValidatedAuthorizeRequestModel>
{
    /// <summary>
    /// Checks that the grant type is in the allowed list for authorize endpoint requests.
    /// </summary>
    /// <param name="model">The authorization request model.</param>
    /// <returns>True if the grant type is allowed; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedAuthorizeRequestModel model)
    {
        if (AllowedGrantTypesForAuthorizeEndpoint.Contains(model.GrantType)) return true;

        return false;
    }
}

/// <summary>
/// Specification that validates the optional response_mode parameter is a supported mode
/// and is compatible with the derived grant type.
/// </summary>
internal class CheckResponseMode : ISpecification<ValidatedAuthorizeRequestModel>
{
    /// <summary>
    /// Validates the response_mode is in the allowed list and permitted for the current grant type.
    /// </summary>
    /// <param name="model">The authorization request model.</param>
    /// <returns>True if valid or absent; false if unsupported or incompatible.</returns>
    public bool IsSatisfiedBy(ValidatedAuthorizeRequestModel model)
    {
        var responseMode = model.GetValue(OpenIdConstants.AuthorizeRequest.ResponseMode);
        if (!string.IsNullOrWhiteSpace(responseMode))
        {
            if (!AllowedResponseModes.Contains(responseMode)) return false;

            // Ensure the response_mode is allowed for the current grant type
            if (!AllowedResponseModesForGrantType[model.GrantType].Contains(responseMode)) return false;

            model.ResponseMode = responseMode;
        }

        return true;
    }
}

/// <summary>
/// Specification that verifies the client is authorized to use the derived grant type.
/// </summary>
internal class CheckAllowedGrantTypeForClient : ISpecification<ValidatedAuthorizeRequestModel>
{
    /// <summary>
    /// Checks the client's supported grant types include the current grant type.
    /// </summary>
    /// <param name="model">The authorization request model.</param>
    /// <returns>True if the client supports the grant type; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedAuthorizeRequestModel model)
    {
        if (!model.Client.SupportedGrantTypes.Contains(model.GrantType)) return false;

        return true;
    }
}

/// <summary>
/// Specification that ensures the client is configured to receive access tokens via browser
/// when the response_type includes "token" (implicit flow).
/// </summary>
internal class CheckAccessTokenInResponseType : ISpecification<ValidatedAuthorizeRequestModel>
{
    /// <summary>
    /// Checks that the client allows browser-based access tokens if "token" is in the response_type.
    /// </summary>
    /// <param name="model">The authorization request model.</param>
    /// <returns>True if access tokens via browser are allowed or not requested; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedAuthorizeRequestModel model)
    {
        var responseTypes = model.GetValue(OpenIdConstants.AuthorizeRequest.ResponseType).SplitBySpace();
        return !responseTypes.Contains(OpenIdConstants.ResponseTypes.Token) ||
               model.Client.AllowAccessTokensViaBrowser;
    }
}

/// <summary>
/// Specification that validates the nonce parameter for OpenID Connect requests.
/// Nonce is required for OpenID Connect requests and must be within length limits.
/// </summary>
internal class CheckNonce : ISpecification<ValidatedAuthorizeRequestModel>
{
    /// <summary>
    /// Validates the nonce: required for OIDC requests, optional otherwise, and within length limits.
    /// </summary>
    /// <param name="model">The authorization request model.</param>
    /// <returns>True if valid; false if missing for OIDC or exceeds length limit.</returns>
    public bool IsSatisfiedBy(ValidatedAuthorizeRequestModel model)
    {
        var nonce = model.GetValue(OpenIdConstants.AuthorizeRequest.Nonce);
        if (string.IsNullOrWhiteSpace(nonce)) return !model.IsOpenIdRequest;

        if (nonce.Length > model.TokenConfigOptions.InputLengthRestrictionsConfig.Nonce) return false;

        model.Nonce = nonce;
        return true;
    }
}

/// <summary>
/// Specification that validates the optional state parameter is within configured length limits.
/// </summary>
internal class CheckState : ISpecification<ValidatedAuthorizeRequestModel>
{
    /// <summary>
    /// Validates state length and stores it on the model if present.
    /// </summary>
    /// <param name="model">The authorization request model.</param>
    /// <returns>True if valid or absent; false if too long.</returns>
    public bool IsSatisfiedBy(ValidatedAuthorizeRequestModel model)
    {
        var state = model.GetValue(OpenIdConstants.AuthorizeRequest.State);
        if (string.IsNullOrWhiteSpace(state)) return true;

        if (state.Length > model.TokenConfigOptions.InputLengthRestrictionsConfig.State) return false;

        model.State = state;
        return true;
    }
}

/// <summary>
/// Specification that validates the optional prompt parameter contains only allowed prompt modes
/// and enforces that "none" cannot be combined with other modes (per OIDC Core spec).
/// </summary>
internal class CheckPrompt : ISpecification<ValidatedAuthorizeRequestModel>
{
    /// <summary>
    /// Validates prompt modes are recognized and "none" is not combined with others.
    /// </summary>
    /// <param name="model">The authorization request model.</param>
    /// <returns>True if valid or absent; false if invalid.</returns>
    public bool IsSatisfiedBy(ValidatedAuthorizeRequestModel model)
    {
        var prompt = model.GetValue(OpenIdConstants.AuthorizeRequest.Prompt);
        if (string.IsNullOrWhiteSpace(prompt)) return true;

        var prompts = prompt.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (prompts.All(x => AllowedPromptModes.Contains(x)))
        {
            // "none" must be the only prompt mode if present (OIDC Core Section 3.1.2.1)
            if (prompts.Contains(OpenIdConstants.PromptModes.None) && prompts.Length > 1) return false;

            model.PromptModes = prompts;
            return true;
        }

        return false;
    }
}

/// <summary>
/// Specification that validates the optional max_age parameter is a non-negative integer.
/// </summary>
internal class CheckMaxAge : ISpecification<ValidatedAuthorizeRequestModel>
{
    /// <summary>
    /// Parses and validates the max_age value.
    /// </summary>
    /// <param name="model">The authorization request model.</param>
    /// <returns>True if valid or absent; false if not a valid non-negative integer.</returns>
    public bool IsSatisfiedBy(ValidatedAuthorizeRequestModel model)
    {
        var maxAge = model.GetValue(OpenIdConstants.AuthorizeRequest.MaxAge);
        if (!string.IsNullOrWhiteSpace(maxAge))
        {
            if (!int.TryParse(maxAge, out var seconds)) return false;

            if (seconds >= 0)
            {
                model.MaxAge = seconds;
                return true;
            }
        }

        return true;
    }
}

/// <summary>
/// Specification that checks whether the requested scopes are plausible for the given response_type.
/// If the response type requires identity scopes, the openid scope must be present.
/// </summary>
internal class CheckScopeResponseTypePlausibility : ISpecification<ValidatedAuthorizeRequestModel>
{
    /// <summary>
    /// Validates scope and response_type compatibility per OIDC specification requirements.
    /// </summary>
    /// <param name="model">The authorization request model.</param>
    /// <returns>True if scopes are compatible with the response type; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedAuthorizeRequestModel model)
    {
        var scope = model.GetValue(OpenIdConstants.AuthorizeRequest.Scope);
        if (scope != null)
        {
            model.RequestedScopes = scope.ParseScopesString().ToList();
            if (model.RequestedScopes.Contains(IdentityScopes.OpenId)) model.IsOpenIdRequest = true;
        }

        // check scope vs response_type plausability
        var requirement = ResponseTypeToScope[model.ResponseType];
        if (requirement == ScopeRequirement.Identity ||
            requirement == ScopeRequirement.IdentityOnly)
            return model.IsOpenIdRequest;

        return true;
    }
}

/// <summary>
/// Specification that validates the requested scopes are allowed for the client and resolves
/// them to identity resources, API resources, and API scopes.
/// </summary>
internal class CheckScopesAllowedForClient : ISpecification<ValidatedAuthorizeRequestModel>
{
    private readonly IResourceScopeValidator resourceScopeValidator;

    /// <summary>
    /// Initializes with the resource scope validator service.
    /// </summary>
    /// <param name="resourceScopeValidator">Validator for scope-to-resource resolution.</param>
    internal CheckScopesAllowedForClient(IResourceScopeValidator resourceScopeValidator)
    {
        this.resourceScopeValidator = resourceScopeValidator;
    }

    /// <summary>
    /// Validates each requested scope is in the client's allowed scopes and resolves them to resources.
    /// </summary>
    /// <param name="model">The authorization request model.</param>
    /// <returns>True if all scopes are allowed; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedAuthorizeRequestModel model)
    {
        // First check that all requested scopes are in the client's allowed list
        var checkClientScope = resourceScopeValidator
            .ValidateRequestedScopeWithClientAsync(model.Client.AllowedScopes, model.RequestedScopes)
            .GetAwaiter().GetResult();
        if (!checkClientScope) return false;

        // Resolve scopes to identity resources, API resources, and API scopes
        var resourceScopeModel = new ResourceScopeModel
        {
            RawData = model.RequestRawData,
            RequestedScope = model.RequestedScopes,
            Client = model.Client,
            UserName = model.Subject is { Identity: not null } ? model.Subject.Identity.Name : string.Empty
        };

        model.AllowedScopesParserModel = resourceScopeValidator.ValidateRequestedScopesAsync(resourceScopeModel)
            .GetAwaiter().GetResult();

        return true;
    }
}

/// <summary>
/// Specification that validates the openid scope is present when identity resources are requested,
/// and flags the request as an API resource request when API scopes are present.
/// </summary>
internal class CheckOpenId : ISpecification<ValidatedAuthorizeRequestModel>
{
    /// <summary>
    /// Ensures openid scope accompanies identity resources and detects API resource requests.
    /// </summary>
    /// <param name="model">The authorization request model.</param>
    /// <returns>True if the openid scope is consistent with requested resources; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedAuthorizeRequestModel model)
    {
        // TODO Error boolean in AllowedScopesParserModel
        //if (model.AllowedScopesParserModel.IsError)
        //{
        //    return false;
        //}

        // Identity resources require the openid scope
        if (model.AllowedScopesParserModel.ParsedIdentityResources.ContainsAny() && !model.IsOpenIdRequest)
            return false;

        if (model.AllowedScopesParserModel.ParsedApiScopes.ContainsAny()) model.IsApiResourceRequest = true;

        return true;
    }
}

/// <summary>
/// Specification that ensures identity resources are present when the response type
/// requires identity scopes (e.g., id_token response type).
/// </summary>
internal class CheckScopeRequirementIdentity : ISpecification<ValidatedAuthorizeRequestModel>
{
    /// <summary>
    /// Validates identity resources are present when the response type mandates them.
    /// </summary>
    /// <param name="model">The authorization request model.</param>
    /// <returns>True if identity scopes are present when required; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedAuthorizeRequestModel model)
    {
        if (ResponseTypeToScope[model.ResponseType] == ScopeRequirement.Identity
            && !model.AllowedScopesParserModel.ParsedIdentityResources.ContainsAny())
            return false;

        return true;
    }
}

/// <summary>
/// Specification that prevents the offline_access scope from being used with the "code id_token"
/// response type, as refresh tokens should not be issued through this hybrid flow combination.
/// </summary>
internal class CheckOfflineAccessForCodeIdToken : ISpecification<ValidatedAuthorizeRequestModel>
{
    /// <summary>
    /// Rejects the request if offline_access is requested with the code id_token response type.
    /// </summary>
    /// <param name="model">The authorization request model.</param>
    /// <returns>True if offline_access is not used with code id_token; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedAuthorizeRequestModel model)
    {
        if (model.ResponseType == OpenIdConstants.ResponseTypes.CodeIdToken &&
            model.AllowedScopesParserModel.AllowOfflineAccess) return false;

        return true;
    }
}

/// <summary>
/// Specification that ensures both identity resources and API scopes are present when
/// the response type requires "identity only" scope (both must be present).
/// </summary>
internal class CheckScopeRequirementIdentityOnly : ISpecification<ValidatedAuthorizeRequestModel>
{
    /// <summary>
    /// Validates both identity and API scopes are present for identity-only response types.
    /// </summary>
    /// <param name="model">The authorization request model.</param>
    /// <returns>True if both resource types are present; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedAuthorizeRequestModel model)
    {
        if (ResponseTypeToScope[model.ResponseType] == ScopeRequirement.IdentityOnly
            && (!model.AllowedScopesParserModel.ParsedIdentityResources.ContainsAny() ||
                !model.AllowedScopesParserModel.ParsedApiScopes.ContainsAny()))
            return false;

        return true;
    }
}

/// <summary>
/// Specification that ensures only API scopes (no identity resources) are present when
/// the response type requires "resource only" scope.
/// </summary>
internal class CheckScopeRequirementResourceOnly : ISpecification<ValidatedAuthorizeRequestModel>
{
    /// <summary>
    /// Validates only API scopes are present for resource-only response types.
    /// </summary>
    /// <param name="model">The authorization request model.</param>
    /// <returns>True if only API scopes are present; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedAuthorizeRequestModel model)
    {
        if (ResponseTypeToScope[model.ResponseType] == ScopeRequirement.ResourceOnly
            && (model.AllowedScopesParserModel.ParsedIdentityResources.ContainsAny() ||
                !model.AllowedScopesParserModel.ParsedApiScopes.ContainsAny()))
            return false;

        return true;
    }
}
