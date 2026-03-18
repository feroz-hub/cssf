/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using System.Text.Encodings.Web;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Models.Api.Response;
using HCL.CS.SF.Domain.Models.Endpoint.Validation;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.DomainServices.Repository.Api;
using HCL.CS.SF.DomainServices.Wrappers;
using HCL.CS.SF.Service.Implementation.Api.Services;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api.Wrapper;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;

namespace HCL.CS.SF.ProxyService.Proxy;

/// <summary>
/// Gateway proxy for the authentication service. Routes requests to the backend
/// <see cref="AuthenticationService"/> after enforcing API-level permission validation.
/// Covers sign-in flows (password, two-factor via email/SMS/authenticator/recovery code),
/// sign-out, authenticator app setup, and credential validation.
/// </summary>
public sealed class AuthenticationProxyService : AuthenticationService, IAuthenticationService
{
    /// <summary>
    /// Validator that checks whether the caller has permission to invoke the requested operation.
    /// </summary>
    private readonly IApiValidator apiValidator;

    /// <summary>
    /// Service used to construct failure responses when validation fails.
    /// </summary>
    private readonly IFrameworkResultService frameworkResult;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationProxyService"/> class.
    /// </summary>
    public AuthenticationProxyService(
        UserManagerWrapper<Users> userManager,
        SignInManagerWrapper<Users> signInManager,
        ILoggerInstance instance,
        IResourceStringHandler resourceStringHandler,
        IFrameworkResultService frameworkResultService,
        HCLCSSFConfig frameworkConfig,
        UrlEncoder urlEncoder,
        IUserAccountService userAccountService,
        IAuthorizationService authorizationService,
        ITokenGenerationService tokenGenerationService,
        IUserRepository userRepository,
        ISessionManagementService session,
        IApiValidator apiValidator)
        : base(
            userManager,
            signInManager,
            instance,
            resourceStringHandler,
            frameworkResultService,
            frameworkConfig,
            urlEncoder,
            userAccountService,
            authorizationService,
            tokenGenerationService,
            userRepository,
            session)
    {
        this.apiValidator = apiValidator;
        frameworkResult = frameworkResultService;
    }

    /// <summary>
    /// Generates new two-factor recovery codes for the specified user after permission validation.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A collection of recovery code strings.</returns>
    public override async Task<IEnumerable<string>> GenerateRecoveryCodesAsync(Guid userId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GenerateRecoveryCodesAsync(userId);
    }

    /// <summary>
    /// Counts the remaining unused recovery codes for the specified user after permission validation.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>The number of remaining recovery codes.</returns>
    public override async Task<int> CountRecoveryCodesAsync(Guid userId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.CountRecoveryCodesAsync(userId);
    }

    /// <summary>
    /// Determines whether the given claims principal represents a signed-in user after permission validation.
    /// </summary>
    /// <param name="principal">The claims principal to evaluate.</param>
    /// <returns><c>true</c> if the user is signed in; otherwise, <c>false</c>.</returns>
    public override async Task<bool> IsUserSignedInAsync(ClaimsPrincipal principal)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.IsUserSignedInAsync(principal);
    }

    /// <summary>
    /// Authenticates a user with username and password after permission validation.
    /// </summary>
    /// <param name="username">The user's login name.</param>
    /// <param name="password">The user's password.</param>
    /// <returns>A sign-in response indicating success, failure, or two-factor requirement.</returns>
    public override async Task<SignInResponseModel> PasswordSignInAsync(string username, string password)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.PasswordSignInAsync(username, password);
    }

    /// <summary>
    /// Authenticates a user with username, password, and a two-factor authenticator token
    /// after permission validation.
    /// </summary>
    /// <param name="username">The user's login name.</param>
    /// <param name="password">The user's password.</param>
    /// <param name="twoFactorAuthenticatorToken">The TOTP code from the authenticator app.</param>
    /// <returns>A sign-in response indicating the authentication outcome.</returns>
    public override async Task<SignInResponseModel> PasswordSignInAsync(string username, string password,
        string twoFactorAuthenticatorToken)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.PasswordSignInAsync(username, password, twoFactorAuthenticatorToken);
    }

    /// <summary>
    /// Resets the authenticator app configuration for the specified user after permission validation.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public override async Task<FrameworkResult> ResetAuthenticatorAppAsync(Guid userId)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.ResetAuthenticatorAppAsync(userId);
    }

    /// <summary>
    /// Validates resource owner password (ROP) credentials after permission validation.
    /// </summary>
    /// <param name="validationModel">The model containing the credentials to validate.</param>
    /// <returns>The validation result model.</returns>
    public override async Task<RopValidationModel> RopValidateCredentialsAsync(RopValidationModel validationModel)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.RopValidateCredentialsAsync(validationModel);
    }

    /// <summary>
    /// Sets up an authenticator app for the specified user after permission validation.
    /// Returns the shared key and QR code URI needed by the authenticator app.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="applicationName">The display name for the authenticator entry.</param>
    /// <returns>The authenticator setup response containing the shared key.</returns>
    public override async Task<AuthenticatorAppSetupResponseModel> SetupAuthenticatorAppAsync(Guid userId,
        string applicationName)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.SetupAuthenticatorAppAsync(userId, applicationName);
    }

    /// <summary>
    /// Signs out the current user after permission validation.
    /// </summary>
    /// <returns>A <see cref="FrameworkResult"/> indicating the sign-out outcome.</returns>
    public override async Task<FrameworkResult> SignOutAsync()
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.SignOutAsync();
    }

    /// <summary>
    /// Completes two-factor sign-in using an authenticator app code after permission validation.
    /// </summary>
    /// <param name="code">The TOTP code from the authenticator app.</param>
    /// <returns>A sign-in response indicating the outcome.</returns>
    public override async Task<SignInResponseModel> TwoFactorAuthenticatorAppSignInAsync(string code)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.TwoFactorAuthenticatorAppSignInAsync(code);
    }

    /// <summary>
    /// Completes two-factor sign-in using an email verification code after permission validation.
    /// </summary>
    /// <param name="code">The verification code sent via email.</param>
    /// <returns>A sign-in response indicating the outcome.</returns>
    public override async Task<SignInResponseModel> TwoFactorEmailSignInAsync(string code)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.TwoFactorEmailSignInAsync(code);
    }

    /// <summary>
    /// Completes two-factor sign-in using a recovery code after permission validation.
    /// </summary>
    /// <param name="recoveryCode">The recovery code to consume.</param>
    /// <returns>A sign-in response indicating the outcome.</returns>
    public override async Task<SignInResponseModel> TwoFactorRecoveryCodeSignInAsync(string recoveryCode)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.TwoFactorRecoveryCodeSignInAsync(recoveryCode);
    }

    /// <summary>
    /// Completes two-factor sign-in using an SMS verification code after permission validation.
    /// </summary>
    /// <param name="code">The verification code sent via SMS.</param>
    /// <returns>A sign-in response indicating the outcome.</returns>
    public override async Task<SignInResponseModel> TwoFactorSmsSignInAsync(string code)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.TwoFactorSmsSignInAsync(code);
    }

    /// <summary>
    /// Verifies the authenticator app setup by validating the provided TOTP token
    /// after permission validation.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="token">The TOTP token to verify.</param>
    /// <returns>The verification response model.</returns>
    public override async Task<AuthenticatorAppResponseModel> VerifyAuthenticatorAppSetupAsync(Guid userId,
        string token)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.VerifyAuthenticatorAppSetupAsync(userId, token);
    }
}
