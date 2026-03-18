/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Api.Response;
using HCL.CS.SF.Domain.Models.Endpoint.Validation;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.DomainServices.Repository.Api;
using HCL.CS.SF.DomainServices.Wrappers;
using HCL.CS.SF.Service.Extension;
using HCL.CS.SF.Service.Implementation.Api.Extension;
using HCL.CS.SF.Service.Implementation.Api.Utils;
using HCL.CS.SF.Service.Implementation.Api.Validators;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;

namespace HCL.CS.SF.Service.Implementation.Api.Services;

/// <summary>
/// Core authentication service implementing the identity server's sign-in flows.
/// Supports multiple authentication methods: password-based local login, LDAP directory authentication,
/// two-factor authentication (email, SMS, authenticator app, recovery codes), and Resource Owner
/// Password (ROP) credential validation for the OAuth2 password grant. Also manages authenticator
/// app setup/verification/reset, recovery code generation, and user sign-out with token cleanup.
/// </summary>

public class AuthenticationService(
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
    ISessionManagementService session)
    : SecurityBase, IAuthenticationService
{
    private readonly ILoggerService loggerService = instance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
    private readonly SystemSettings settings = frameworkConfig.SystemSettings;

    /// <summary>
    /// Authenticates a user with username and password (no two-factor).
    /// </summary>
    /// <param name="username">The username to authenticate.</param>
    /// <param name="password">The password to verify.</param>
    /// <returns>A sign-in response indicating success, failure, or two-factor requirement.</returns>
    public virtual async Task<SignInResponseModel> PasswordSignInAsync(string username, string password)
    {
        return await LoginAsync(username, password, string.Empty, false);
    }

    /// <summary>
    /// Authenticates a user with username, password, and an authenticator app TOTP code.
    /// </summary>
    /// <param name="username">The username to authenticate.</param>
    /// <param name="password">The password to verify.</param>
    /// <param name="twoFactorAuthenticatorToken">The TOTP token from the authenticator app.</param>
    /// <returns>A sign-in response indicating success or failure.</returns>
    public virtual async Task<SignInResponseModel> PasswordSignInAsync(string username, string password,
        string twoFactorAuthenticatorToken)
    {
        return await LoginAsync(username, password, twoFactorAuthenticatorToken, true);
    }

    /// <summary>
    /// Completes two-factor authentication using an email-delivered verification code.
    /// </summary>
    /// <param name="code">The verification code sent via email.</param>
    /// <returns>A sign-in response indicating success or failure.</returns>
    public virtual async Task<SignInResponseModel> TwoFactorEmailSignInAsync(string code)
    {
        return await TwoFactorSignInAsync(code, NotificationTypes.Email);
    }

    /// <summary>
    /// Completes two-factor authentication using an SMS-delivered verification code.
    /// </summary>
    /// <param name="code">The verification code sent via SMS.</param>
    /// <returns>A sign-in response indicating success or failure.</returns>
    public virtual async Task<SignInResponseModel> TwoFactorSmsSignInAsync(string code)
    {
        return await TwoFactorSignInAsync(code, NotificationTypes.SMS);
    }

    /// <summary>
    /// Validates Resource Owner Password (ROP) credentials for the OAuth2 password grant type.
    /// Verifies the user exists, is not two-factor enabled, and the credentials are valid.
    /// </summary>
    /// <param name="validationModel">The model containing username and password to validate.</param>
    /// <returns>The validation model populated with the subject ID on success, or error codes on failure.</returns>
    public virtual async Task<RopValidationModel> RopValidateCredentialsAsync(RopValidationModel validationModel)
    {
        if (validationModel == null) frameworkResultService.Throw(ApiErrorCodes.Invalid_Rop_Validation_Model);

        if (string.IsNullOrWhiteSpace(validationModel.UserName))
        {
            validationModel.ErrorCode = OpenIdConstants.Errors.InvalidGrant;
            loggerService.WriteToWithCaller(Log.Error,
                validationModel.ErrorCode + " : " + "Invalid username or password.");
            return validationModel;
        }

        if (string.IsNullOrWhiteSpace(validationModel.Password))
        {
            validationModel.ErrorCode = OpenIdConstants.Errors.InvalidGrant;
            loggerService.WriteToWithCaller(Log.Error,
                validationModel.ErrorCode + " : " + "Invalid username or password.");
            return validationModel;
        }

        try
        {
            loggerService.WriteTo(Log.Debug,
                "Entered in validate resource owner password for user: " + validationModel.UserName);
            var user = await userManager.FindByNameAsync(validationModel.UserName);
            if (user != null && !user.TwoFactorEnabled)
            {
                var signInResult = await LoginAsync(validationModel.UserName, validationModel.Password, string.Empty,
                    false);
                if (signInResult.Succeeded)
                {
                    var claims = await userManager.GetClaimsAsync(user);
                    var result = new RopValidationModel(
                        user.Id.ToString(),
                        OpenIdConstants.UserAuthenticationMethods.Password,
                        DateTime.UtcNow,
                        claims);
                    validationModel.Subject = result.Subject;
                    validationModel.IsError = false;
                    return validationModel;
                }

                validationModel.ErrorCode = OpenIdConstants.Errors.InvalidGrant;
                loggerService.WriteToWithCaller(Log.Error,
                    validationModel.ErrorCode + " : " + "Invalid username or password.");
            }
            else
            {
                validationModel.ErrorCode = OpenIdConstants.Errors.InvalidGrant;
                loggerService.WriteToWithCaller(Log.Error,
                    validationModel.ErrorCode + " : " + "User has been disabled in the system.");
            }

            return validationModel;
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to validate credentials.");
            throw;
        }
    }

    /// <summary>
    /// Signs out the current user, updates the last-logout timestamp, and removes
    /// all active security tokens associated with the user.
    /// </summary>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> SignOutAsync()
    {
        try
        {
            var user = await session.GetUserPrincipalFromContextAsync();
            var subjectId = string.Empty;
            if (user != null) subjectId = user.GetSubjectId();

            await signInManager.SignOutAsync();
            if (!string.IsNullOrWhiteSpace(subjectId))
            {
                var userEntity = await FindByIdAsync(new Guid(subjectId));
                loggerService.WriteTo(Log.Debug, "Signout initiated for user : " + userEntity.UserName);
                userEntity.LastLogoutDateTime = DateTime.UtcNow;
                await userRepository.UpdateAsync(userEntity, new[] { "LastLogoutDateTime" });
                await userRepository.SaveChangesAsync();

                return await tokenGenerationService.RemoveUserTokensAsync(subjectId);
            }

            return frameworkResultService.Succeeded();
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to sign out user.");
            throw;
        }
    }

    /// <summary>
    /// Determines whether the specified claims principal represents a currently signed-in user.
    /// </summary>
    /// <param name="principal">The claims principal to check.</param>
    /// <returns><c>true</c> if the user is signed in; otherwise, <c>false</c>.</returns>
    public virtual async Task<bool> IsUserSignedInAsync(ClaimsPrincipal principal)
    {
        try
        {
            if (principal != null && principal.Identity != null)
            {
                loggerService.WriteTo(Log.Debug, "Is user signed in ? : " + principal.Identity.Name);
                return await Task.FromResult(signInManager.IsSignedIn(principal));
            }

            return await Task.FromResult(false);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to check if user is signed in.");
            throw;
        }
    }

    /// <summary>
    /// Sets up authenticator app (TOTP) for a user by generating or retrieving the shared key
    /// and building the QR code URI for scanning.
    /// </summary>
    /// <param name="userId">The user to set up the authenticator for.</param>
    /// <param name="applicationName">The application name displayed in the authenticator app.</param>
    /// <returns>The setup response containing the shared key and authenticator URI.</returns>
    public virtual async Task<AuthenticatorAppSetupResponseModel> SetupAuthenticatorAppAsync(Guid userId,
        string applicationName)
    {
        if (!userId.IsValid()) frameworkResultService.Throw(ApiErrorCodes.InvalidUserId);

        try
        {
            var user = await FindByIdAsync(userId);
            if (user != null)
            {
                await userRepository.EnableIdentityAutoSaveChanges();
                loggerService.WriteTo(Log.Debug, "Generating authenticator app QRCode URI for user: " + user.UserName);
                var authenticatorKey = await userManager.GetAuthenticatorKeyAsync(user);
                if (string.IsNullOrWhiteSpace(authenticatorKey))
                {
                    var result = await userManager.ResetAuthenticatorKeyAsync(user);
                    if (!result.Succeeded)
                        frameworkResultService.ThrowCustomMessage(result.ConstructIdentityErrorAsString());

                    authenticatorKey = await userManager.GetAuthenticatorKeyAsync(user);
                }

                var enableAuthenticatorModel = new AuthenticatorAppSetupResponseModel();
                enableAuthenticatorModel.SharedKey = FormatKey(authenticatorKey);
                enableAuthenticatorModel.AuthenticatorUri =
                    GenerateQrCodeUri(user.Email, authenticatorKey, applicationName, urlEncoder);
                return enableAuthenticatorModel;
            }
            else
            {
                frameworkResultService.Throw(ApiErrorCodes.InvalidUserId);
            }
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to set up authenticator.");
            throw;
        }
        finally
        {
            await userRepository.DisableIdentityAutoSaveChanges();
        }

        return null;
    }

    /// <summary>
    /// Verifies the authenticator app setup by validating the TOTP token, enabling two-factor
    /// authentication, and generating recovery codes if none exist.
    /// </summary>
    /// <param name="userId">The user verifying authenticator setup.</param>
    /// <param name="token">The TOTP token from the authenticator app.</param>
    /// <returns>The verification response with success status and optional recovery codes.</returns>
    public virtual async Task<AuthenticatorAppResponseModel> VerifyAuthenticatorAppSetupAsync(Guid userId, string token)
    {
        if (!userId.IsValid()) frameworkResultService.Throw(ApiErrorCodes.InvalidUserId);

        if (string.IsNullOrWhiteSpace(token)) frameworkResultService.Throw(ApiErrorCodes.InvalidTwoFactorTokenProvided);

        try
        {
            var user = await FindByIdAsync(userId);
            if (user != null)
            {
                await userRepository.EnableIdentityAutoSaveChanges();
                loggerService.WriteTo(Log.Debug, "Verifying authenticator app code for the user : " + user.UserName);
                var verificationCode = token.Replace(" ", string.Empty).Replace("-", string.Empty);
                var is2faTokenValid = await userManager.VerifyTwoFactorTokenAsync(
                    user,
                    userManager.Options.Tokens.AuthenticatorTokenProvider,
                    verificationCode);
                var verificationResponse = new AuthenticatorAppResponseModel();
                if (is2faTokenValid)
                {
                    verificationResponse.Succeeded = true;
                    loggerService.WriteTo(Log.Debug,
                        "Authenticator app token has been verified for user: " + user.UserName);
                }
                else
                {
                    verificationResponse.Succeeded = false;
                    verificationResponse.Message =
                        resourceStringHandler.GetResourceString(ApiErrorCodes.AuthenticatorAppVerificationFailed);
                    loggerService.WriteTo(Log.Error,
                        "Authenticator app token verification failed for user: " + user.UserName + " " +
                        verificationResponse.Message);
                    return verificationResponse;
                }

                user.TwoFactorEnabled = true;
                user.TwoFactorType = TwoFactorType.AuthenticatorApp;
                var result = await userManager.UpdateAsync(user);
                if (!result.Succeeded)
                    frameworkResultService.ThrowCustomMessage(result.ConstructIdentityErrorAsString());

                if (await userManager.CountRecoveryCodesAsync(user) != 0) return verificationResponse;

                var recoveryCodes =
                    await userManager.GenerateNewTwoFactorRecoveryCodesAsync(
                        user,
                        settings.UserConfig.RequiredRecoveryCodes);

                verificationResponse.RecoveryCodes = recoveryCodes;
                verificationResponse.Message += " New recovery codes generated";
                return verificationResponse;
            }
            else
            {
                frameworkResultService.Throw(ApiErrorCodes.InvalidUserId);
            }
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to verify authenticator setup.");
            throw;
        }
        finally
        {
            await userRepository.DisableIdentityAutoSaveChanges();
        }

        return null;
    }

    /// <summary>
    /// Completes two-factor authentication using an authenticator app TOTP code.
    /// </summary>
    /// <param name="code">The TOTP code from the authenticator app.</param>
    /// <returns>A sign-in response indicating success or failure.</returns>
    public virtual async Task<SignInResponseModel> TwoFactorAuthenticatorAppSignInAsync(string code)
    {
        try
        {
            loggerService.WriteTo(Log.Debug, "Entered into Two Factor authenticator app signIn");

            // Enabling auto save changes because we don�t have user to get concurrency stamp (if we need user we need to duplicate user calls)
            await userRepository.EnableIdentityAutoSaveChanges();

            // Code validation done in identity to increase access failed count.
            var signInResult = await signInManager.TwoFactorAuthenticatorSignInAsync(
                code,
                settings.LoginConfig.IsPersistent,
                settings.LoginConfig.RememberClient);

            var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
            return await ConstructSignInResponseModel(user, string.Empty, false, signInResult);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to sign in using two factor authenticator.");
            throw;
        }
        finally
        {
            await userRepository.EnableIdentityAutoSaveChanges();
        }
    }

    /// <summary>
    /// Resets the authenticator app key for a user, disabling authenticator-based two-factor
    /// if it was the active method, and refreshing the sign-in session.
    /// </summary>
    /// <param name="userId">The user whose authenticator to reset.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> ResetAuthenticatorAppAsync(Guid userId)
    {
        if (!userId.IsValid()) return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserId);

        try
        {
            var user = await FindByIdAsync(userId);
            if (user != null)
            {
                await userRepository.EnableIdentityAutoSaveChanges();
                loggerService.WriteTo(Log.Debug, "Entered into reset authenticator app for user: " + user.UserName);
                var result = await userManager.ResetAuthenticatorKeyAsync(user);
                if (!result.Succeeded) return frameworkResultService.Failed(result.ConstructIdentityErrorAsList());

                if (user.TwoFactorType == TwoFactorType.AuthenticatorApp)
                {
                    user.TwoFactorEnabled = false;
                    user.TwoFactorType = TwoFactorType.None;
                    result = await userManager.UpdateAsync(user);
                    if (!result.Succeeded)
                        frameworkResultService.ThrowCustomMessage(result.ConstructIdentityErrorAsString());
                }

                await signInManager.RefreshSignInAsync(user);
                loggerService.WriteTo(Log.Debug, "Authenticator app key has been reset");
                return frameworkResultService.Succeeded();
            }
            else
            {
                return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserId);
            }
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to reset authenticator.");
            throw;
        }
        finally
        {
            await userRepository.DisableIdentityAutoSaveChanges();
        }
    }

    /// <summary>
    /// Generates a new set of two-factor recovery codes for the specified user, replacing any existing codes.
    /// </summary>
    /// <param name="userId">The user to generate recovery codes for.</param>
    /// <returns>The generated recovery code strings.</returns>
    public virtual async Task<IEnumerable<string>> GenerateRecoveryCodesAsync(Guid userId)
    {
        if (!userId.IsValid()) frameworkResultService.Throw(ApiErrorCodes.InvalidUserId);

        try
        {
            var user = await FindByIdAsync(userId);
            if (user != null)
            {
                await userRepository.EnableIdentityAutoSaveChanges();
                loggerService.WriteTo(Log.Debug, "Entered into generate recovery codes for user: " + user.UserName);

                var recoveryCodes =
                    await userManager.GenerateNewTwoFactorRecoveryCodesAsync(
                        user,
                        settings.UserConfig.RequiredRecoveryCodes);

                return recoveryCodes;
            }
            else
            {
                frameworkResultService.Throw(ApiErrorCodes.InvalidUserId);
            }
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to generate recovery codes.");
            throw;
        }
        finally
        {
            await userRepository.DisableIdentityAutoSaveChanges();
        }

        return null;
    }

    /// <summary>
    /// Returns the number of remaining unused recovery codes for the specified user.
    /// </summary>
    /// <param name="userId">The user whose recovery codes to count.</param>
    /// <returns>The count of remaining recovery codes.</returns>
    public virtual async Task<int> CountRecoveryCodesAsync(Guid userId)
    {
        if (!userId.IsValid()) frameworkResultService.Throw(ApiErrorCodes.InvalidUserId);

        try
        {
            var user = await FindByIdAsync(userId);
            if (user != null)
            {
                loggerService.WriteTo(Log.Debug, "Entered into count recovery codes for user: " + user.UserName);
                return await userManager.CountRecoveryCodesAsync(user);
            }

            frameworkResultService.Throw(ApiErrorCodes.InvalidUserId);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to generate recovery codes.");
            throw;
        }

        return 0;
    }

    /// <summary>
    /// Completes two-factor authentication using a one-time recovery code.
    /// The code is consumed after use and cannot be reused.
    /// </summary>
    /// <param name="recoveryCode">The recovery code to validate and consume.</param>
    /// <returns>A sign-in response indicating success or failure.</returns>
    public virtual async Task<SignInResponseModel> TwoFactorRecoveryCodeSignInAsync(string recoveryCode)
    {
        // RecoveryCode validation done here, Because identity is not increasing access failed count from their end.
        if (string.IsNullOrWhiteSpace(recoveryCode)) return ConstructError(ApiErrorCodes.InvalidTwoFactorTokenProvided);

        try
        {
            loggerService.WriteTo(Log.Debug, "Entered into two factor recovery code signin");
            await userRepository.EnableIdentityAutoSaveChanges();

            var signInResult = await signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);
            var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
            return await ConstructSignInResponseModel(user, string.Empty, false, signInResult);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to sign in using recovery code.");
            throw;
        }
        finally
        {
            await userRepository.DisableIdentityAutoSaveChanges();
        }
    }

    private async Task<Users> FindByIdAsync(Guid userId)
    {
        return await userManager.FindByIdAsync(userId.ToString());
    }

    private async Task<SignInResponseModel> LoginAsync(
        string username,
        string password,
        string twoFactorAuthenticatorToken,
        bool isAuthenticatorApp)
    {
        // Password validation done in identity to increase access failed count.
        // Two factor code validation done in identity to increase access failed count.
        if (string.IsNullOrWhiteSpace(username)) return ConstructError(ApiErrorCodes.InvalidUserOrPassword);

        var signInResponse = new SignInResponseModel();
        loggerService.WriteTo(Log.Debug, "Entered into validate user credentials for user: " + username);
        var user = await userManager.FindByNameAsync(username);
        if (user == null)
        {
            if (GlobalConfiguration.IsLdapConfigurationValid)
            {
                // User is not in local DB, check in Ldap server.
                signInResponse = await LdapLogin(username, password);
                if (!signInResponse.Succeeded) return signInResponse;
            }
            else
            {
                return new SignInResponseModel
                {
                    Succeeded = false,
                    ErrorCode = ApiErrorCodes.InvalidUserOrPassword,
                    Message = "Login failed due to invalid credentials."
                };
            }
        }
        else if (user.IdentityProviderType == IdentityProvider.Ldap)
        {
            if (GlobalConfiguration.IsLdapConfigurationValid)
            {
                signInResponse = await LdapLogin(username, password);
                if (!signInResponse.Succeeded) return signInResponse;
            }
            else
            {
                return LdapLoginFailed();
            }
        }
        else if (user.IdentityProviderType == IdentityProvider.Local)
        {
            if (user.RequiresDefaultPasswordChange != null && (bool)user.RequiresDefaultPasswordChange)
                return ConstructError(ApiErrorCodes.DefaultPasswordNeedsToChange);

            // Password expiry validation done before checking user against password to avoid setting HttpContext in identity.
            var userManagementUtils = new UserManagementValidator();
            signInResponse = userManagementUtils.ValidatePasswordExpiryAsync(
                user,
                settings,
                resourceStringHandler,
                loggerService);

            if (!signInResponse.Succeeded && !string.IsNullOrWhiteSpace(signInResponse.ErrorCode))
            {
                loggerService.WriteToWithCaller(Log.Error, signInResponse.ErrorCode + ": " + signInResponse.Message);
                return signInResponse;
            }
        }

        try
        {
            loggerService.WriteTo(Log.Debug, "Entered into Login : " + username);

            await userRepository.EnableIdentityAutoSaveChanges();
            var signInResult = await signInManager.PasswordSignInAsync(
                username,
                password,
                settings.LoginConfig.IsPersistent,
                settings.LoginConfig.LockoutOnFailure);

            // If not success, fetching user to get access failed count.
            if (!signInResult.Succeeded) user = await userManager.FindByNameAsync(username);

            var signInResponseModel = await ConstructSignInResponseModel(
                user,
                twoFactorAuthenticatorToken,
                isAuthenticatorApp,
                signInResult);

            if ((signInResponseModel.Succeeded || signInResponseModel.RequiresTwoFactor) &&
                !string.IsNullOrWhiteSpace(signInResponse.Message))
            {
                signInResponseModel.Message += signInResponse.Message;
                loggerService.WriteTo(Log.Debug, signInResponseModel.Message);
            }

            return signInResponseModel;
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to sign in user.");
            throw;
        }
        finally
        {
            await userRepository.DisableIdentityAutoSaveChanges();
        }
    }

    private async Task<SignInResponseModel> LdapLogin(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(password)) return ConstructError(ApiErrorCodes.InvalidUserOrPassword);

        var signInResponseModel = new SignInResponseModel();
        var ldaputil = new LdapUtil(loggerService, settings.LdapConfig, frameworkResultService, userAccountService);
        var frameworkResult = await ldaputil.LdapLoginAsync(username, password);
        signInResponseModel.Succeeded = true;
        if (frameworkResult.Status == ResultStatus.Failed)
        {
            signInResponseModel.Succeeded = false;
            signInResponseModel.ErrorCode = frameworkResult.Errors.ToList()[0].Code;
            signInResponseModel.Message = frameworkResult.Errors.ToList()[0].Description;
            if (signInResponseModel.ErrorCode == ApiErrorCodes.InvalidLDAPUserNameOrPassword)
                return ConstructError(ApiErrorCodes.InvalidUserOrPassword);
        }

        return signInResponseModel;
    }

    private SignInResponseModel LdapLoginFailed()
    {
        return new SignInResponseModel
        {
            Succeeded = false,
            ErrorCode = ApiErrorCodes.InvalidLDAPConfiguration,
            Message = "Ldap configuration invalid."
        };
    }

    private async Task<SignInResponseModel> TwoFactorSignInAsync(
        string code,
        NotificationTypes notificationTypes)
    {
        try
        {
            // Code validation done in identity to increase access failed count.
            var provider = string.Empty;
            if (notificationTypes == NotificationTypes.Email)
                provider = Constants.DefaultEmailProvider;
            else if (notificationTypes == NotificationTypes.SMS) provider = Constants.DefaultPhoneProvider;

            // Enabling auto save changes because we don�t have user to get concurrency stamp (if we need user we need to duplicate user calls)
            await userRepository.EnableIdentityAutoSaveChanges();
            var signInResult = await signInManager.TwoFactorSignInAsync(
                provider,
                code,
                settings.LoginConfig.IsPersistent,
                settings.LoginConfig.RememberClient);

            var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
            return await ConstructSignInResponseModel(user, string.Empty, false, signInResult);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to sign in user using two factor.");
            throw;
        }
        finally
        {
            await userRepository.DisableIdentityAutoSaveChanges();
        }
    }

    private async Task<SignInResponseModel> ConstructSignInResponseModel(
        Users user,
        string twoFactorAuthenticatorToken,
        bool isAuthenticatorApp,
        SignInResult result)
    {
        var signInResponse = new SignInResponseModel();
        if (result.Succeeded)
        {
            if (user != null)
            {
                user.LastLoginDateTime = DateTime.UtcNow;
                await userRepository.UpdateAsync(user, new[] { "LastLoginDateTime" });
                await userRepository.SaveChangesAsync();
                signInResponse.UserVerificationCode =
                    await authorizationService.SaveVerificationCodeAsync(user.UserName);
            }

            signInResponse.Succeeded = true;
            signInResponse.Message = resourceStringHandler.GetResourceString(ApiErrorCodes.LoginSuccessful);
            loggerService.WriteTo(Log.Debug, signInResponse.Message);
            return signInResponse;
        }

        if (result.IsLockedOut)
        {
            signInResponse = ConstructError(ApiErrorCodes.UserAccountLocked);
            signInResponse.IsLockedOut = true;
            return signInResponse;
        }

        if (result.IsNotAllowed)
        {
            signInResponse = ConstructError(ApiErrorCodes.LoginNotAllowed);
            signInResponse.IsNotAllowed = true;
            return signInResponse;
        }

        if (result.RequiresTwoFactor)
        {
            if (isAuthenticatorApp) return await TwoFactorAuthenticatorAppSignInAsync(twoFactorAuthenticatorToken);

            signInResponse.RequiresTwoFactor = true;
            if (user != null)
            {
                FrameworkResult frameworkResult = null;
                if (user.TwoFactorType == TwoFactorType.None)
                {
                    signInResponse = ConstructError(ApiErrorCodes.InvalidTwoFactorTypeId);
                    signInResponse.RequiresTwoFactor = true;
                    return signInResponse;
                }

                if (user.TwoFactorType == TwoFactorType.AuthenticatorApp)
                {
                    signInResponse.TwoFactorVerificationCodeSent = false;
                    signInResponse.TwoFactorVerificationMode = TwoFactorType.AuthenticatorApp;
                    return signInResponse;
                }

                if (user.TwoFactorType == TwoFactorType.Email)
                {
                    frameworkResult = await userAccountService.GenerateEmailTwoFactorTokenAsync(user.UserName);
                    signInResponse.TwoFactorVerificationMode = TwoFactorType.Email;
                }
                else if (user.TwoFactorType == TwoFactorType.Sms)
                {
                    frameworkResult = await userAccountService.GenerateSmsTwoFactorTokenAsync(user.UserName);
                    signInResponse.TwoFactorVerificationMode = TwoFactorType.Sms;
                }

                if (frameworkResult != null && frameworkResult.Status == ResultStatus.Failed)
                {
                    signInResponse.Succeeded = false;
                    signInResponse.RequiresTwoFactor = false;
                    signInResponse.ErrorCode = frameworkResult.Errors.ToList()[0].Code;
                    signInResponse.Message = frameworkResult.Errors.ToList()[0].Description;
                    return signInResponse;
                }

                signInResponse.TwoFactorVerificationCodeSent = true;
            }

            return signInResponse;
        }

        if (!result.Succeeded)
        {
            if (user != null && user.AccessFailedCount > 0 &&
                user.AccessFailedCount < settings.UserConfig.MaxFailedAccessAttempts)
            {
                var count = 0;
                count = settings.UserConfig.MaxFailedAccessAttempts - user.AccessFailedCount;
                signInResponse.ErrorCode = ApiErrorCodes.LoginFailedMismatchRemainingCount;
                signInResponse.Message =
                    string.Format(
                        resourceStringHandler.GetResourceString(ApiErrorCodes.LoginFailedMismatchRemainingCount),
                        count);
                loggerService.WriteTo(Log.Error, signInResponse.ErrorCode + ": " + signInResponse.Message);
            }
            else
            {
                if (user != null && user.AccessFailedCount == settings.UserConfig.AccessFailedCount)
                    if (!await userManager.IsLockedOutAsync(user))
                        await userManager.SetLockoutEnabledAsync(user, true);

                signInResponse = ConstructError(ApiErrorCodes.UserAccountLocked);
            }

            signInResponse.Succeeded = false;
            return signInResponse;
        }

        return signInResponse;
    }

    private string FormatKey(string unformattedKey)
    {
        var result = new StringBuilder();
        var currentPosition = 0;
        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
            currentPosition += 4;
        }

        if (currentPosition < unformattedKey.Length) result.Append(unformattedKey.Substring(currentPosition));

        return result.ToString().ToLowerInvariant();
    }

    private string GenerateQrCodeUri(string email, string unformattedKey, string applicationName, UrlEncoder urlEncoder)
    {
        const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        return string.Format(
            AuthenticatorUriFormat,
            urlEncoder.Encode(applicationName),
            urlEncoder.Encode(email),
            unformattedKey);
    }

    private SignInResponseModel ConstructError(string errorCode)
    {
        var signinResponse = new SignInResponseModel();
        signinResponse.Succeeded = false;
        signinResponse.Message = resourceStringHandler.GetResourceString(errorCode);
        signinResponse.ErrorCode = errorCode;
        loggerService.WriteTo(Log.Error, signinResponse.ErrorCode + ": " + signinResponse.Message);
        return signinResponse;
    }
}
