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
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.Domain.Models.Endpoint.Validation;
using HCL.CS.SF.DomainServices.Wrappers;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Implementation.Endpoint.Validators;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Validators;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Specifications;

/// <summary>
/// Validates the token request for the Resource Owner Password Credentials grant type (RFC 6749 Section 4.3).
/// Checks client authorization, validates scopes, verifies username/password credentials,
/// ensures the user exists, has no two-factor authentication enabled, and authenticates via the profile service.
/// </summary>
internal sealed class ResourceOwnerFlowSpecification : BaseRequestModelValidator<ValidatedTokenRequestModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceOwnerFlowSpecification"/> class.
    /// </summary>
    internal ResourceOwnerFlowSpecification(IResourceScopeValidator resourceScopeValidator,
        IAuthenticationService authenticationService,
        UserManagerWrapper<Users> userManager)
    {
        Add("CheckClientAuthorizedForGrantType", new Rule<ValidatedTokenRequestModel>(
            new CheckClientAuthorizedForGrantType<ValidatedTokenRequestModel>(new List<string>
            {
                AuthenticationConstants.GrantType.ResourceOwnerPassword
            }),
            OpenIdConstants.Errors.UnauthorizedClient,
            EndpointErrorCodes.ClientNotAuthorizedForGrantType));

        Add("ValidateRequestedROPScopes", new Rule<ValidatedTokenRequestModel>(
            new ValidateRequestedRopScopes(resourceScopeValidator),
            OpenIdConstants.Errors.InvalidScope,
            EndpointErrorCodes.InvalidScopeOrNotAllowed));

        Add("CheckUserName", new Rule<ValidatedTokenRequestModel>(
            new IsRequestNull<ValidatedTokenRequestModel>(request =>
                request.GetValue(OpenIdConstants.TokenRequest.UserName)),
            OpenIdConstants.Errors.InvalidGrant,
            EndpointErrorCodes.UserNameMissing));

        Add("CheckUserNameLength", new Rule<ValidatedTokenRequestModel>(
            new CheckLengthRestrictions<ValidatedTokenRequestModel>(
                request => request.GetValue(OpenIdConstants.TokenRequest.UserName),
                request => request.TokenConfigOptions.InputLengthRestrictionsConfig.UserName,
                request => ">"),
            OpenIdConstants.Errors.InvalidGrant,
            EndpointErrorCodes.UserNameTooLong));

        Add("CheckUserPasswordLength", new Rule<ValidatedTokenRequestModel>(
            new CheckLengthRestrictions<ValidatedTokenRequestModel>(
                request => request.GetValue(OpenIdConstants.TokenRequest.Password),
                request => request.TokenConfigOptions.InputLengthRestrictionsConfig.Password,
                request => ">"),
            OpenIdConstants.Errors.InvalidGrant,
            EndpointErrorCodes.PasswordTooLong));

        Add("CheckUserExists", new Rule<ValidatedTokenRequestModel>(
            new CheckUserExistsByName(userManager),
            OpenIdConstants.Errors.InvalidRequest,
            EndpointErrorCodes.InvalidUser));

        Add("CheckUserTwoFactor", new Rule<ValidatedTokenRequestModel>(
            new CheckUserTwoFactor(userManager),
            OpenIdConstants.Errors.InvalidRequest,
            EndpointErrorCodes.TwoFactorEnabled));

        Add("CheckProfileServices", new Rule<ValidatedTokenRequestModel>(
            new CheckProfileServices(authenticationService),
            OpenIdConstants.Errors.InvalidGrant,
            EndpointErrorCodes.UserAuthenticationFailed));
    }
}

/// <summary>
/// Specification that authenticates the user via the Resource Owner Password Credentials flow.
/// Validates username and password against the authentication service and populates the subject principal.
/// </summary>
internal class CheckProfileServices : ISpecification<ValidatedTokenRequestModel>
{
    private readonly IAuthenticationService authenticationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckProfileServices"/> class.
    /// </summary>
    internal CheckProfileServices(IAuthenticationService authenticationService)
    {
        this.authenticationService = authenticationService;
    }

    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(ValidatedTokenRequestModel model)
    {
        var userName = model.GetValue(OpenIdConstants.TokenRequest.UserName);
        var password = model.GetValue(OpenIdConstants.TokenRequest.Password);
        // authenticate user
        var validationModel = new RopValidationModel
        {
            UserName = userName,
            Password = password,
            Request = model
        };

        validationModel = authenticationService.RopValidateCredentialsAsync(validationModel).GetAwaiter().GetResult();
        if (validationModel.IsError)
        {
            validationModel.ErrorCode ??= OpenIdConstants.Errors.InvalidGrant;
            if (validationModel.ErrorCode == OpenIdConstants.Errors.UnsupportedGrantType) return false;

            return false;
        }

        if (validationModel.Subject == null) return false;

        model.UserName = userName;
        model.Subject = validationModel.Subject;

        return true;
    }
}

/// <summary>
/// Specification that rejects resource owner password requests for users with two-factor authentication enabled.
/// ROP flow cannot handle interactive 2FA challenges.
/// </summary>
internal class CheckUserTwoFactor : ISpecification<ValidatedTokenRequestModel>
{
    private readonly UserManagerWrapper<Users> userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckUserTwoFactor"/> class.
    /// </summary>
    internal CheckUserTwoFactor(UserManagerWrapper<Users> userManager)
    {
        this.userManager = userManager;
    }

    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(ValidatedTokenRequestModel model)
    {
        // make sure user is enabled
        var userName = model.GetValue(OpenIdConstants.TokenRequest.UserName);
        var userInfo = userManager.FindByNameAsync(userName).GetAwaiter().GetResult();
        if (userInfo != null && !userInfo.TwoFactorEnabled) return true;

        return false;
    }
}

/// <summary>
/// Specification that verifies the user exists by looking up the username in the identity store.
/// </summary>
internal class CheckUserExistsByName : ISpecification<ValidatedTokenRequestModel>
{
    private readonly UserManagerWrapper<Users> userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckUserExistsByName"/> class.
    /// </summary>
    internal CheckUserExistsByName(UserManagerWrapper<Users> userManager)
    {
        this.userManager = userManager;
    }

    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(ValidatedTokenRequestModel model)
    {
        // make sure user is enabled
        var userName = model.GetValue(OpenIdConstants.TokenRequest.UserName);
        var userInfo = userManager.FindByNameAsync(userName).GetAwaiter().GetResult();
        if (userInfo == null) return false;

        model.UserName = userName;
        return true;
    }
}

/// <summary>
/// Specification that validates and resolves the requested scopes for the Resource Owner Password flow.
/// Defaults to the client's allowed scopes if none are explicitly requested.
/// </summary>
internal class ValidateRequestedRopScopes : ISpecification<ValidatedTokenRequestModel>
{
    private readonly IResourceScopeValidator resourceScopeValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateRequestedRopScopes"/> class.
    /// </summary>
    internal ValidateRequestedRopScopes(IResourceScopeValidator resourceScopeValidator)
    {
        this.resourceScopeValidator = resourceScopeValidator;
    }

    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(ValidatedTokenRequestModel model)
    {
        var scopes = model.GetValue(OpenIdConstants.TokenRequest.Scope);

        if (string.IsNullOrWhiteSpace(scopes))
        {
            if (model.Client.AllowedScopes.ContainsAny())
            {
                var allowedScopes = model.Client.AllowedScopes.ToList();
                scopes = string.Join(" ", allowedScopes.ToArray());
            }
            else
            {
                return false;
            }
        }

        if (scopes.Length > model.TokenConfigOptions.InputLengthRestrictionsConfig.Scope) return false;

        var requestedScopes = scopes.ParseScopesString().ToList();
        var checkClientScope = resourceScopeValidator
            .ValidateRequestedScopeWithClientAsync(model.Client.AllowedScopes, requestedScopes).GetAwaiter()
            .GetResult();
        AllowedScopesParserModel allowedScopesParser;
        if (checkClientScope)
        {
            model.RequestRawData.TryGetValue(OpenIdConstants.TokenRequest.UserName, out var username);
            var resourceScopeModel = new ResourceScopeModel
            {
                RawData = model.RequestRawData,
                RequestedScope = requestedScopes,
                Client = model.Client,
                UserName = username
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
