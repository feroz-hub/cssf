/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HCL.CS.SF.DemoServerApp.Constants;
using HCL.CS.SF.DemoServerApp.Models;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;

namespace HCL.CS.SF.DemoServerApp.Controllers;

public class AccountController(
    IAuthenticationService authenticationServices,
    IUserAccountService userAccountService,
    IInteractionService interactionService,
    ILoggerInstance instance)
    : Controller
{
    private readonly ILoggerService authenticationLoggerService =
        instance.GetLoggerInstance(LogKeyConstants.Authentication);

    [HttpGet]
    [AllowAnonymous]
    public ActionResult Login(string returnUrl = null)
    {
        var resolvedReturnUrl = ResolveReturnUrl(null, returnUrl);
        ViewData["ReturnUrl"] = resolvedReturnUrl;
        return View(new LoginViewModel { ReturnUrl = resolvedReturnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string button, string returnUrl = null)
    {
        var resolvedReturnUrl = ResolveReturnUrl(model.ReturnUrl, returnUrl);
        model.ReturnUrl = resolvedReturnUrl;
        ViewData["ReturnUrl"] = resolvedReturnUrl;
        if (button != "login")
            return Redirect(string.IsNullOrWhiteSpace(resolvedReturnUrl)
                ? Url.Action(nameof(Login), "Account")!
                : resolvedReturnUrl);

        if (!ModelState.IsValid) return View(model);
        authenticationLoggerService.WriteTo(Log.Debug, "Login request for user: " + model.UserName);
        HttpContext.Session.SetString("UserName", model.UserName);
        var signResult = await authenticationServices.PasswordSignInAsync(model.UserName, model.Password);
        if (signResult.Succeeded)
        {
            authenticationLoggerService.WriteTo(Log.Debug, "Login successfull for user: " + model.UserName);
            var url = await interactionService.ConstructUserVerificationCode(resolvedReturnUrl,
                signResult.UserVerificationCode);
            return Redirect(url);
        }

        if (signResult.RequiresTwoFactor)
        {
            authenticationLoggerService.WriteTo(Log.Debug, "Two factor required for user: " + model.UserName);
            switch (signResult.TwoFactorVerificationMode)
            {
                case TwoFactorType.Email:
                    return RedirectToAction("TwoFactorEmailSignIn", "Account", new { returnUrl = resolvedReturnUrl });
                case TwoFactorType.Sms:
                    return RedirectToAction("TwoFactorSmsSignIn", "Account", new { returnUrl = resolvedReturnUrl });
                case TwoFactorType.AuthenticatorApp:
                    return RedirectToAction("AuthenticatorSignIn", "Account", new { returnUrl = resolvedReturnUrl });
                case TwoFactorType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else if (signResult.IsLockedOut)
        {
            authenticationLoggerService.WriteTo(Log.Debug, "User account is locked for: " + model.UserName);
            ModelState.AddModelError(signResult.ErrorCode, signResult.Message);
        }
        else if (signResult.IsNotAllowed)
        {
            authenticationLoggerService.WriteTo(Log.Debug, "Login is not allowed for user: " + model.UserName);
            ModelState.AddModelError(signResult.ErrorCode, signResult.Message);
        }
        else
        {
            authenticationLoggerService.WriteTo(Log.Debug, "Login failed for user: " + model.UserName);
            ModelState.AddModelError(signResult.ErrorCode, signResult.Message);
        }

        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public ActionResult AuthenticationMethod(TwoFactorViewModel model, string returnUrl = null)
    {
        var resolvedReturnUrl = ResolveReturnUrl(model?.ReturnUrl, returnUrl);
        ViewData["ReturnUrl"] = resolvedReturnUrl;
        var viewModel = model ?? new TwoFactorViewModel();
        viewModel.ReturnUrl = resolvedReturnUrl;
        return View(viewModel);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AuthenticationMethod(TwoFactorViewModel model, string action,
        string returnUrl = null)
    {
        var resolvedReturnUrl = ResolveReturnUrl(model.ReturnUrl, returnUrl);
        model.ReturnUrl = resolvedReturnUrl;
        ViewData["ReturnUrl"] = resolvedReturnUrl;
        switch (action)
        {
            case "TFAEmail":
            {
                var username = HttpContext.Session.GetString("UserName");
                var result = await userAccountService.GenerateEmailTwoFactorTokenAsync(username);
                if (result.Status == ResultStatus.Failed) ShowErrorPage(result);

                return RedirectToAction("TwoFactorEmailSignIn", "Account", new { returnUrl = resolvedReturnUrl });
            }
            case "TFASms":
            {
                var username = HttpContext.Session.GetString("UserName");
                var result = await userAccountService.GenerateSmsTwoFactorTokenAsync(username);
                if (result.Status == ResultStatus.Failed) ShowErrorPage(result);

                return RedirectToAction("TwoFactorSmsSignIn", "Account", new { returnUrl = resolvedReturnUrl });
            }
            case "Authenticator":
                return RedirectToAction("AuthenticatorSignIn", "Account", new { returnUrl = resolvedReturnUrl });
            case "RecoveryCodes":
                return RedirectToAction("RecoveryCodeSignIn", "Account", new { returnUrl = resolvedReturnUrl });
            default:
                return View(model);
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public ActionResult TwoFactorEmailSignIn(string returnUrl = null)
    {
        var resolvedReturnUrl = ResolveReturnUrl(null, returnUrl);
        ViewData["ReturnUrl"] = resolvedReturnUrl;
        return View(new TwoFactorViewModel { ReturnUrl = resolvedReturnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TwoFactorEmailSignIn(TwoFactorViewModel twoFactorModel, string returnUrl = null)
    {
        var resolvedReturnUrl = ResolveReturnUrl(twoFactorModel.ReturnUrl, returnUrl);
        twoFactorModel.ReturnUrl = resolvedReturnUrl;
        ViewData["ReturnUrl"] = resolvedReturnUrl;
        if (!ModelState.IsValid) return View(twoFactorModel);
        var signResult = await authenticationServices.TwoFactorEmailSignInAsync(twoFactorModel.TwoFactorCode);
        if (signResult.Succeeded)
        {
            authenticationLoggerService.WriteTo(Log.Debug, "Login successfull");
            var url = await interactionService.ConstructUserVerificationCode(resolvedReturnUrl,
                signResult.UserVerificationCode);
            return Redirect(url);
        }

        ModelState.AddModelError(signResult.ErrorCode, signResult.Message);
        return View(twoFactorModel);
    }

    [HttpGet]
    [AllowAnonymous]
    public ActionResult TwoFactorSmsSignIn(string returnUrl = null)
    {
        var resolvedReturnUrl = ResolveReturnUrl(null, returnUrl);
        ViewData["ReturnUrl"] = resolvedReturnUrl;
        return View(new TwoFactorViewModel { ReturnUrl = resolvedReturnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TwoFactorSmsSignIn(TwoFactorViewModel twoFactorModel, string returnUrl = null)
    {
        var resolvedReturnUrl = ResolveReturnUrl(twoFactorModel.ReturnUrl, returnUrl);
        twoFactorModel.ReturnUrl = resolvedReturnUrl;
        ViewData["ReturnUrl"] = resolvedReturnUrl;
        if (!ModelState.IsValid) return View(twoFactorModel);
        var signResult = await authenticationServices.TwoFactorSmsSignInAsync(twoFactorModel.TwoFactorCode);
        if (signResult.Succeeded)
        {
            authenticationLoggerService.WriteTo(Log.Debug, "Login successfull");
            var url = await interactionService.ConstructUserVerificationCode(resolvedReturnUrl,
                signResult.UserVerificationCode);
            return Redirect(url);
        }

        ModelState.AddModelError(signResult.ErrorCode, signResult.Message);
        return View(twoFactorModel);
    }

    [HttpGet]
    [AllowAnonymous]
    public ActionResult AuthenticatorSignIn(string returnUrl = null)
    {
        var resolvedReturnUrl = ResolveReturnUrl(null, returnUrl);
        ViewData["ReturnUrl"] = resolvedReturnUrl;
        return View(new TwoFactorViewModel { ReturnUrl = resolvedReturnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AuthenticatorSignIn(TwoFactorViewModel twoFactorModel, string returnUrl = null)
    {
        var resolvedReturnUrl = ResolveReturnUrl(twoFactorModel.ReturnUrl, returnUrl);
        twoFactorModel.ReturnUrl = resolvedReturnUrl;
        ViewData["ReturnUrl"] = resolvedReturnUrl;
        if (!ModelState.IsValid) return View(twoFactorModel);
        var signResult =
            await authenticationServices.TwoFactorAuthenticatorAppSignInAsync(twoFactorModel.TwoFactorCode);
        if (signResult.Succeeded)
        {
            authenticationLoggerService.WriteTo(Log.Debug, "Login successfull");
            var url = await interactionService.ConstructUserVerificationCode(resolvedReturnUrl,
                signResult.UserVerificationCode);
            return Redirect(url);
        }

        ModelState.AddModelError(signResult.ErrorCode, signResult.Message);
        return View(twoFactorModel);
    }

    [HttpGet]
    [AllowAnonymous]
    public ActionResult RecoveryCodeSignIn(string returnUrl = null)
    {
        var resolvedReturnUrl = ResolveReturnUrl(null, returnUrl);
        ViewData["ReturnUrl"] = resolvedReturnUrl;
        return View(new TwoFactorViewModel { ReturnUrl = resolvedReturnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecoveryCodeSignIn(TwoFactorViewModel twoFactorModel, string returnUrl = null)
    {
        var resolvedReturnUrl = ResolveReturnUrl(twoFactorModel.ReturnUrl, returnUrl);
        twoFactorModel.ReturnUrl = resolvedReturnUrl;
        ViewData["ReturnUrl"] = resolvedReturnUrl;
        if (!ModelState.IsValid) return View(twoFactorModel);
        var signResult = await authenticationServices.TwoFactorRecoveryCodeSignInAsync(twoFactorModel.TwoFactorCode);
        if (signResult.Succeeded)
        {
            authenticationLoggerService.WriteTo(Log.Debug, "Login successfull");
            var url = await interactionService.ConstructUserVerificationCode(resolvedReturnUrl,
                signResult.UserVerificationCode);
            return Redirect(url);
        }

        ModelState.AddModelError(signResult.ErrorCode, signResult.Message);
        return View(twoFactorModel);
    }

    [HttpGet]
    public async Task<IActionResult> Logout(string logoutId)
    {
        var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = true };
        if (User?.Identity.IsAuthenticated != true)
        {
            vm.ShowLogoutPrompt = false;
            return await Logout(vm);
        }

        if (string.IsNullOrWhiteSpace(logoutId))
        {
            vm.ShowLogoutPrompt = false;
            return await Logout(vm);
        }

        var context = await interactionService.GetLogoutContextAsync(logoutId);
        if (context?.ShowSignoutPrompt != false) return View(vm);
        vm.ShowLogoutPrompt = false;
        return await Logout(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(LogoutViewModel model)
    {
        var logout = string.IsNullOrWhiteSpace(model.LogoutId)
            ? null
            : await interactionService.GetLogoutContextAsync(model.LogoutId);
        var postLogoutRedirectUri = logout?.PostLogoutRedirectUri;
        if (string.IsNullOrWhiteSpace(postLogoutRedirectUri))
        {
            postLogoutRedirectUri = Url.Action(nameof(Login), "Account");
        }

        var vm = new LoggedOutViewModel
        {
            AutomaticRedirectAfterSignOut = true,
            PostLogoutRedirectUri = postLogoutRedirectUri,
            SignOutIframeUrl = logout?.EndSessionCallBackUrl,
            LogoutId = model.LogoutId
        };

        if (User?.Identity.IsAuthenticated == true) await authenticationServices.SignOutAsync();

        return View("LoggedOut", vm);
    }

    [HttpGet]
    [AllowAnonymous]
    public ActionResult ForgetPassword(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel forgetPasswordViewModel, string button,
        string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View();
        HttpContext.Session.SetString("UserName", forgetPasswordViewModel.UserName);
        var frameworkResult = button switch
        {
            "email" => await userAccountService.GeneratePasswordResetTokenAsync(forgetPasswordViewModel.UserName),
            "sms" => await userAccountService.GeneratePasswordResetTokenAsync(forgetPasswordViewModel.UserName,
                NotificationTypes.SMS),
            _ => null
        };

        if (frameworkResult.Status != ResultStatus.Failed)
            return RedirectToAction("ResetPassword", "Account",
                new { returnUrl = forgetPasswordViewModel.ReturnUrl });
        ModelState.AddModelError(frameworkResult.Errors.ToList()[0].Code,
            frameworkResult.Errors.ToList()[0].Description);
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public ActionResult ResetPassword(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View();
        var username = HttpContext.Session.GetString("UserName");
        var frameworkResult = await userAccountService.ResetPasswordAsync(username, model.Token, model.Password);
        if (frameworkResult.Status == ResultStatus.Failed)
        {
            ModelState.AddModelError(frameworkResult.Errors.ToList()[0].Code,
                frameworkResult.Errors.ToList()[0].Description);
            return View();
        }

        HttpContext.Session.Remove("UserName");

        return RedirectToAction("Login", "Account", new { returnUrl = model.ReturnUrl });
    }

    private static string? ResolveReturnUrl(string? modelReturnUrl, string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(modelReturnUrl)) return modelReturnUrl;
        return string.IsNullOrWhiteSpace(returnUrl) ? null : returnUrl;
    }

    private ActionResult ShowErrorPage(FrameworkResult frameworkResult)
    {
        var errorModel = new ErrorViewModel
        {
            ErrorCode = frameworkResult.Errors.ToList()[0].Code,
            ErrorMessage = frameworkResult.Errors.ToList()[0].Description
        };
        return RedirectToAction("Error", "Home", errorModel);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
        }

        base.Dispose(disposing);
    }
}
