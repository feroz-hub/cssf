/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Linq.Expressions;
using DomainValidation.Interfaces.Specification;
using DomainValidation.Validation;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Implementation.Endpoint.Validators;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Validators;
using static HCL.CS.SF.Domain.Constants.Endpoint.AuthenticationConstants;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Specifications;

/// <summary>
/// Validates an OAuth 2.0 client_credentials grant type token request.
/// Ensures the client is authorized for this grant type, requested scopes are valid
/// (excluding identity scopes), no identity resources are present, and offline_access is not requested.
/// </summary>
/// <summary>
/// Validates the token request for the OAuth 2.0 Client Credentials grant type (RFC 6749 Section 4.4).
/// Ensures the client is authorized for this grant type, validates requested scopes (excluding identity scopes),
/// and rejects offline_access since client_credentials has no user context for refresh tokens.
/// </summary>
internal sealed class ClientCredentialsFlowSpecification : BaseRequestModelValidator<ValidatedTokenRequestModel>
{
    /// <summary>
    /// Initializes the client credentials flow validation pipeline.
    /// </summary>
    /// <param name="resourceScopeValidator">Validator for scope resolution and permission checks.</param>
    internal ClientCredentialsFlowSpecification(IResourceScopeValidator resourceScopeValidator)
    {
        // Verify the client is authorized for the client_credentials grant type
        Add("CheckClientAuthorizedForGrantType", new Rule<ValidatedTokenRequestModel>(
            new CheckClientAuthorizedForGrantType<ValidatedTokenRequestModel>(new List<string>
            {
                GrantType.ClientCredentials
            }),
            OpenIdConstants.Errors.UnauthorizedClient,
            EndpointErrorCodes.ClientNotAuthorizedForGrantType));

        // Validate requested scopes, excluding identity scopes (openid, profile, etc.)
        Add("ValidateRequestedScopes", new Rule<ValidatedTokenRequestModel>(
            new ValidateRequestedClientCredentialScopes(resourceScopeValidator),
            OpenIdConstants.Errors.InvalidScope,
            EndpointErrorCodes.InvalidScopeOrNotAllowed));

        // Ensure no identity resources are requested (client_credentials is machine-to-machine only)
        Add("CheckParsedIdentityResources", new Rule<ValidatedTokenRequestModel>(
            new CheckParsedIdentityResources(request => request.AllowedScopesParserModel),
            OpenIdConstants.Errors.UnauthorizedClient,
            EndpointErrorCodes.OpenIdScopeNotAllowed));

        // Ensure offline_access (refresh tokens) is not requested for client_credentials
        Add("CheckOfflineAccess", new Rule<ValidatedTokenRequestModel>(
            new CheckOfflineAccess(request => request.AllowedScopesParserModel),
            OpenIdConstants.Errors.InvalidScope,
            EndpointErrorCodes.RefreshTokenRequestNotAllowed));
    }
}

/// <summary>
/// Specification that validates the requested scopes for a client_credentials token request.
/// Identity scopes (openid, profile, etc.) are rejected. If no scopes are requested,
/// the client's full allowed scope list (minus identity scopes) is used as a default.
/// </summary>
internal class ValidateRequestedClientCredentialScopes : ISpecification<ValidatedTokenRequestModel>
{
    private readonly IResourceScopeValidator resourceScopeValidator;

    /// <summary>
    /// Initializes the scope validator with the resource scope validation service.
    /// </summary>
    /// <param name="resourceScopeValidator">Validator for scope resolution.</param>
    internal ValidateRequestedClientCredentialScopes(IResourceScopeValidator resourceScopeValidator)
    {
        this.resourceScopeValidator = resourceScopeValidator;
    }

    /// <summary>
    /// Validates and resolves the requested scopes, rejecting identity scopes and
    /// enforcing client scope permissions.
    /// </summary>
    /// <param name="model">The token request model.</param>
    /// <returns>True if scopes are valid; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedTokenRequestModel model)
    {
        var scopes = model.GetValue(OpenIdConstants.TokenRequest.Scope);

        if (string.IsNullOrWhiteSpace(scopes))
        {
            // Default to client's allowed scopes, excluding identity scopes
            if (model.Client.AllowedScopes.ContainsAny())
            {
                var allowedScopes = model.Client.AllowedScopes.Except(typeof(IdentityScopes).GetArray().ToList());
                scopes = string.Join(" ", allowedScopes.ToArray());
            }
            else
            {
                return false;
            }
        }
        else
        {
            // Reject any identity scopes present in the explicit scope request
            var scopeList = scopes.SplitBySpace().Select(x => x.ToLower());
            var identityList = typeof(IdentityScopes).GetArray().ToList();
            if (scopeList.Intersect(identityList).ContainsAny()) return false;
        }

        // Enforce maximum scope string length
        if (scopes.Length > model.TokenConfigOptions.InputLengthRestrictionsConfig.Scope) return false;

        // Validate each scope against the client's allowed scope list
        var requestedScopes = scopes.ParseScopesString().ToList();
        var checkClientScope = resourceScopeValidator
            .ValidateRequestedScopeWithClientAsync(model.Client.AllowedScopes, requestedScopes).GetAwaiter()
            .GetResult();
        AllowedScopesParserModel allowedScopesParser;
        if (checkClientScope)
        {
            // Resolve scopes to identity resources, API resources, and API scopes
            var resourceScopeModel = new ResourceScopeModel
            {
                RawData = model.RequestRawData,
                RequestedScope = requestedScopes,
                Client = model.Client
            };

            allowedScopesParser = resourceScopeValidator.ValidateRequestedScopesAsync(resourceScopeModel).GetAwaiter()
                .GetResult();
            model.TokenDetails = allowedScopesParser.TokenDetails;
        }
        else
        {
            return false;
        }

        model.AllowedScopesParserModel = allowedScopesParser;

        return true;
    }
}

/// <summary>
/// Specification that ensures no identity resources were resolved from the requested scopes.
/// Client credentials flow is machine-to-machine and must not include identity scopes.
/// </summary>
internal class CheckParsedIdentityResources : ISpecification<ValidatedTokenRequestModel>
{
    private readonly Expression<Func<ValidatedTokenRequestModel, object>> expression;

    /// <summary>
    /// Initializes the checker with an expression to access the scope parser model.
    /// </summary>
    /// <param name="expression">Expression pointing to the AllowedScopesParserModel.</param>
    internal CheckParsedIdentityResources(Expression<Func<ValidatedTokenRequestModel, object>> expression)
    {
        this.expression = expression;
    }

    /// <summary>
    /// Returns false if any identity resources were parsed (indicating identity scopes were requested).
    /// </summary>
    /// <param name="model">The token request model.</param>
    /// <returns>True if no identity resources are present; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedTokenRequestModel model)
    {
        var value = expression.Compile()(model);
        if (value == null) return false;

        var type = value.GetType();
        if (type != typeof(AllowedScopesParserModel)) return false;

        // Identity resources must not be present in client_credentials flow
        if (model.AllowedScopesParserModel.ParsedIdentityResources.ContainsAny()) return false;

        return true;
    }
}

/// <summary>
/// Specification that ensures offline_access (refresh token) is not requested
/// in a client_credentials flow, as refresh tokens are not applicable for this grant type.
/// </summary>
internal class CheckOfflineAccess : ISpecification<ValidatedTokenRequestModel>
{
    private readonly Expression<Func<ValidatedTokenRequestModel, object>> expression;

    /// <summary>
    /// Initializes the checker with an expression to access the scope parser model.
    /// </summary>
    /// <param name="expression">Expression pointing to the AllowedScopesParserModel.</param>
    internal CheckOfflineAccess(Expression<Func<ValidatedTokenRequestModel, object>> expression)
    {
        this.expression = expression;
    }

    /// <summary>
    /// Returns false if offline_access was requested, preventing refresh token issuance.
    /// </summary>
    /// <param name="model">The token request model.</param>
    /// <returns>True if offline_access is not requested; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedTokenRequestModel model)
    {
        var value = expression.Compile()(model);
        if (value == null) return false;

        var type = value.GetType();
        if (type != typeof(AllowedScopesParserModel)) return false;

        if (model.AllowedScopesParserModel.AllowOfflineAccess) return false;

        return true;
    }
}
