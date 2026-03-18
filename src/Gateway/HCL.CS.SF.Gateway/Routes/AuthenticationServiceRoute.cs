/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using Newtonsoft.Json.Linq;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Models.Endpoint.Validation;
using HCL.CS.SF.ProxyService.Routes.Extension;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api.Wrapper;

namespace HCL.CS.SF.ProxyService.Routes;

/// <summary>
/// Partial class of <see cref="ApiGateway"/> containing route handlers for authentication
/// operations including password sign-in, two-factor authentication (authenticator app,
/// email, SMS, recovery code), sign-out, authenticator app setup/verification,
/// and resource owner password credential validation.
/// </summary>
internal partial class ApiGateway : BaseApiServiceInstance, IApiGateway
{
    /// <summary>
    /// Handles the route for generating new two-factor recovery codes for a user.
    /// </summary>
    private async Task<bool> GenerateRecoveryCodes(string jsonContent)
    {
        var userId = jsonContent.JsonDeserialize<Guid>();

        var recoveryCodes = await AuthenticationService.GenerateRecoveryCodesAsync(userId);
        await GenerateApiResults(recoveryCodes);
        return true;
    }

    /// <summary>
    /// Handles the route for counting remaining recovery codes for a user.
    /// </summary>
    private async Task<bool> CountRecoveryCodesAsync(string jsonContent)
    {
        var userId = jsonContent.JsonDeserialize<Guid>();

        var recoveryCodesCount = await AuthenticationService.CountRecoveryCodesAsync(userId);
        await GenerateApiResults(recoveryCodesCount);
        return true;
    }

    /// <summary>
    /// Handles the route for checking whether a user is currently signed in.
    /// </summary>
    private async Task<bool> IsUserSignedIn(string jsonContent)
    {
        var claimsPrincipal = jsonContent.JsonDeserialize<ClaimsPrincipal>();

        var frameworkResult = await AuthenticationService.IsUserSignedInAsync(claimsPrincipal);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for username/password sign-in.
    /// Parses username and password from the JSON body.
    /// </summary>
    private async Task<bool> PasswordSignIn(string jsonContent)
    {
        var jsonObjects = JObject.Parse(jsonContent);
        var userName = jsonObjects[ApiRouteParameterConstants.UserName].ToObject<string>();
        var password = jsonObjects[ApiRouteParameterConstants.Password].ToObject<string>();

        var frameworkResult = await AuthenticationService.PasswordSignInAsync(userName, password);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for username/password sign-in with a two-factor authenticator token.
    /// Parses username, password, and TOTP code from the JSON body.
    /// </summary>
    private async Task<bool> PasswordSignInByTwoFactorAuthenticatorToken(string jsonContent)
    {
        var jsonObjects = JObject.Parse(jsonContent);
        var userName = jsonObjects[ApiRouteParameterConstants.UserName].ToObject<string>();
        var password = jsonObjects[ApiRouteParameterConstants.Password].ToObject<string>();
        var twofactorToken = jsonObjects[ApiRouteParameterConstants.TwoFactorAuthenticatorToken].ToObject<string>();

        var frameworkResult = await AuthenticationService.PasswordSignInAsync(userName, password, twofactorToken);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for resetting a user's authenticator app configuration.
    /// </summary>
    private async Task<bool> ResetAuthenticatorApp(string jsonContent)
    {
        var userId = jsonContent.JsonDeserialize<Guid>();
        var frameworkResult = await AuthenticationService.ResetAuthenticatorAppAsync(userId);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for validating resource owner password (ROP) credentials.
    /// </summary>
    private async Task<bool> RopValidateCredentials(string jsonContent)
    {
        var ropValidationModel = jsonContent.JsonDeserialize<RopValidationModel>();
        var frameworkResult = await AuthenticationService.RopValidateCredentialsAsync(ropValidationModel);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for setting up an authenticator app for a user.
    /// Parses user ID and application name from the JSON body.
    /// </summary>
    private async Task<bool> SetupAuthenticatorApp(string jsonContent)
    {
        var jsonObjects = JObject.Parse(jsonContent);
        var userId = jsonObjects[ApiRouteParameterConstants.UserId].ToObject<Guid>();
        var applicationName = jsonObjects[ApiRouteParameterConstants.ApplicationName].ToObject<string>();

        var frameworkResult = await AuthenticationService.SetupAuthenticatorAppAsync(userId, applicationName);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for signing out the current user.
    /// </summary>
    private async Task<bool> SignOut(string jsonContent)
    {
        var frameworkResult = await AuthenticationService.SignOutAsync();
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for two-factor sign-in using an authenticator app code.
    /// </summary>
    private async Task<bool> TwoFactorAuthenticatorAppSignIn(string jsonContent)
    {
        var code = jsonContent.JsonDeserialize<string>();
        var frameworkResult = await AuthenticationService.TwoFactorAuthenticatorAppSignInAsync(code);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for two-factor sign-in using an email verification code.
    /// </summary>
    private async Task<bool> TwoFactorEmailSignIn(string jsonContent)
    {
        var code = jsonContent.JsonDeserialize<string>();
        var frameworkResult = await AuthenticationService.TwoFactorEmailSignInAsync(code);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for two-factor sign-in using a recovery code.
    /// </summary>
    private async Task<bool> TwoFactorRecoveryCodeSignIn(string jsonContent)
    {
        var code = jsonContent.JsonDeserialize<string>();
        var frameworkResult = await AuthenticationService.TwoFactorRecoveryCodeSignInAsync(code);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for two-factor sign-in using an SMS verification code.
    /// </summary>
    private async Task<bool> TwoFactorSmsSignInAsync(string jsonContent)
    {
        var code = jsonContent.JsonDeserialize<string>();
        var frameworkResult = await AuthenticationService.TwoFactorSmsSignInAsync(code);
        await GenerateApiResults(frameworkResult);
        return true;
    }

    /// <summary>
    /// Handles the route for verifying authenticator app setup with a TOTP token.
    /// Parses user ID and verification token from the JSON body.
    /// </summary>
    private async Task<bool> VerifyAuthenticatorAppSetup(string jsonContent)
    {
        var jsonObjects = JObject.Parse(jsonContent);
        var userId = jsonObjects[ApiRouteParameterConstants.UserId].ToObject<Guid>();
        var token = jsonObjects[ApiRouteParameterConstants.UserToken].ToObject<string>();
        var frameworkResult = await AuthenticationService.VerifyAuthenticatorAppSetupAsync(userId, token);
        await GenerateApiResults(frameworkResult);
        return true;
    }
}
