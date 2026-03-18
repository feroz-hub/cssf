/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
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
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint.Validators;
using SystemClaimTypes = System.Security.Claims.ClaimTypes;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Specifications;

/// <summary>
/// Validates the user_code grant type flow used for external sign-in scenarios (e.g., device authorization).
/// Ensures the client is authorized for user_code grants, the code is present, the code resolves to a valid user,
/// and the requested scopes are allowed.
/// </summary>
/// <summary>
/// Validates the token request for the custom user_code grant type (external sign-in flow).
/// Verifies the client is authorized, validates the user verification code, resolves the user,
/// and validates the requested scopes.
/// </summary>
internal sealed class UserCodeFlowSpecification : BaseRequestModelValidator<ValidatedTokenRequestModel>
{
    /// <summary>
    /// Initializes the user code flow validation pipeline.
    /// </summary>
    /// <param name="resourceScopeValidator">Validator for scope resolution and permission checks.</param>
    /// <param name="authorizationService">Service for verifying user codes and managing security tokens.</param>
    /// <param name="userManager">ASP.NET Identity user manager for user lookups.</param>
    internal UserCodeFlowSpecification(
        IResourceScopeValidator resourceScopeValidator,
        IAuthorizationService authorizationService,
        UserManagerWrapper<Users> userManager)
    {
        // Verify the client is authorized to use the user_code grant type
        Add("CheckClientAuthorizedForUserCodeGrant", new Rule<ValidatedTokenRequestModel>(
            new CheckClientAuthorizedForGrantType<ValidatedTokenRequestModel>(new List<string>
            {
                AuthenticationConstants.GrantType.UserCode
            }),
            OpenIdConstants.Errors.UnauthorizedClient,
            EndpointErrorCodes.ClientNotAuthorizedForGrantType));

        // Ensure the user_code parameter is present in the request
        Add("CheckUserCodePresent", new Rule<ValidatedTokenRequestModel>(
            new IsRequestNull<ValidatedTokenRequestModel>(request =>
                request.GetValue(OpenIdConstants.TokenRequest.UserCode)),
            OpenIdConstants.Errors.InvalidGrant,
            EndpointErrorCodes.InvalidSecurityTokenId));

        // Validate the user code, resolve it to a user, and build the claims principal
        Add("ValidateUserCodeAndResolveUser", new Rule<ValidatedTokenRequestModel>(
            new ValidateUserCodeAndResolveUser(authorizationService, userManager),
            OpenIdConstants.Errors.InvalidGrant,
            EndpointErrorCodes.InvalidSecurityTokenId));

        // Validate the requested scopes against client and resource configuration
        Add("ValidateRequestedUserCodeScopes", new Rule<ValidatedTokenRequestModel>(
            new ValidateRequestedRopScopes(resourceScopeValidator),
            OpenIdConstants.Errors.InvalidScope,
            EndpointErrorCodes.InvalidScopeOrNotAllowed));
    }
}

/// <summary>
/// Specification that validates a user verification code, resolves the associated user,
/// consumes the one-time code, and constructs a <see cref="ClaimsPrincipal"/> for the authenticated user.
/// </summary>
internal class ValidateUserCodeAndResolveUser : ISpecification<ValidatedTokenRequestModel>
{
    private readonly IAuthorizationService authorizationService;
    private readonly UserManagerWrapper<Users> userManager;

    /// <summary>
    /// Initializes the user code resolver with authorization and user management services.
    /// </summary>
    /// <param name="authorizationService">Service for code verification and deletion.</param>
    /// <param name="userManager">ASP.NET Identity user manager for user lookups.</param>
    internal ValidateUserCodeAndResolveUser(
        IAuthorizationService authorizationService,
        UserManagerWrapper<Users> userManager)
    {
        this.authorizationService = authorizationService;
        this.userManager = userManager;
    }

    /// <summary>
    /// Validates the user code, looks up the associated user, deletes the consumed code,
    /// and populates the model with user identity claims.
    /// </summary>
    /// <param name="model">The token request model containing the user_code parameter.</param>
    /// <returns>True if the code is valid and the user exists; false otherwise.</returns>
    public bool IsSatisfiedBy(ValidatedTokenRequestModel model)
    {
        var userCode = model.GetValue(OpenIdConstants.TokenRequest.UserCode);
        if (string.IsNullOrWhiteSpace(userCode)) return false;

        // Verify the code exists and retrieve the associated token value (username)
        var securityToken = authorizationService.ValidateVerificationCodeAsync(userCode).GetAwaiter().GetResult();
        if (securityToken == null || string.IsNullOrWhiteSpace(securityToken.TokenValue))
            return false;

        // Look up the user by the username stored in the security token
        var userName = securityToken.TokenValue;
        var user = userManager.FindByNameAsync(userName).GetAwaiter().GetResult();
        if (user == null)
            return false;

        // Consume the one-time user code so it cannot be reused
        _ = authorizationService.DeleteSecurityTokenByTokenValueAsync(userCode).GetAwaiter().GetResult();

        model.UserName = userName;
        if (!model.RequestRawData.ContainsKey(OpenIdConstants.TokenRequest.UserName))
            model.RequestRawData[OpenIdConstants.TokenRequest.UserName] = userName;

        // Build a ClaimsPrincipal with essential identity claims for the resolved user
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(SystemClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(SystemClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(OpenIdConstants.ClaimTypes.AuthenticationTime, DateTime.UtcNow.ToUnixTime().ToString(), ClaimValueTypes.Integer64)
            },
            "user_code");
        model.Subject = new ClaimsPrincipal(identity);

        return true;
    }
}
