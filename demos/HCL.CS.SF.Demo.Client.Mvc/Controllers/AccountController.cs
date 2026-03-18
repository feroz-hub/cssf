/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HCL.CS.SF.DemoClientMvc.Helper;
using HCL.CS.SF.DemoClientMvc.Interface;
using HCL.CS.SF.DemoClientMvc.Models;
using HCL.CS.SF.DemoClientMvc.ViewModels.Account;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.Models.Api;

namespace HCL.CS.SF.DemoClientMvc.Controllers;

public class AccountController(
    IHttpService httpService,
    ITokenService tokenService,
    ILogger<AccountController> logger)
    : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public ActionResult Login(string returnUrl = null)
    {
        try
        {
            var redirectUrl = Url.Action(nameof(Callback), new { returnUrl = ToSafeReturnUrl(returnUrl) });
            return Challenge(
                new AuthenticationProperties { RedirectUri = redirectUrl },
                OpenIdConnectDefaults.AuthenticationScheme);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Login challenge failed.");
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Callback(string returnUrl = null)
    {
        try
        {
            if (User?.Identity?.IsAuthenticated == true) return LocalRedirect(ToSafeReturnUrl(returnUrl));

            return RedirectToAction(nameof(Login), new { returnUrl = ToSafeReturnUrl(returnUrl) });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Post-login callback handling failed.");
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(string returnUrl = null)
    {
        try
        {
            await tokenService.RevokeTokenAsync(HttpContext.RequestAborted);
            HttpContext.Session.Clear();

            return SignOut(
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action(nameof(SignedOut), new { returnUrl = ToSafeReturnUrl(returnUrl) })
                },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Logout failed.");
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult SignedOut(string returnUrl = null)
    {
        return LocalRedirect(ToSafeReturnUrl(returnUrl));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RefreshToken(string returnUrl = null)
    {
        try
        {
            var refreshed = await tokenService.RefreshTokenAsync(HttpContext.RequestAborted);
            if (refreshed)
            {
                TempData["Message"] = "Access token refreshed.";
            }
            else
            {
                TempData["Title"] = "Error";
                TempData["Message"] = "Access token refresh failed. Please sign in again.";
            }

            var safeReturnUrl = ToSafeReturnUrl(returnUrl, Url.Action("Index", "Token"));
            return LocalRedirect(safeReturnUrl);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Token refresh endpoint failed.");
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string returnUrl = null)
    {
        var model = new ResetPasswordViewModel();
        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendResetPasswordToken(ResetPasswordViewModel model, string returnUrl = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.UserName))
            {
                ModelState.AddModelError(string.Empty, "Username is required.");
                return View("ResetPassword", model);
            }

            var tokenRequest = new
            {
                user_name = model.UserName,
                notification_type = NotificationTypes.SMS
            };

            var frameworkResult =
                await httpService.PostAsync<FrameworkResult>(ApiRoutePathConstants.GeneratePasswordResetToken,
                    tokenRequest);
            if (frameworkResult.Status == ResultStatus.Succeeded)
            {
                model.TokenIssued = true;
                TempData["Message"] = "Reset token sent to the registered mobile number.";
                return View("ResetPassword", model);
            }

            ModelState.AddModelError(frameworkResult.Errors.ToList()[0].Code,
                frameworkResult.Errors.ToList()[0].Description);
            return View("ResetPassword", model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Reset-password token generation failed.");
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, string returnUrl = null)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                model.TokenIssued = true;
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.ResetToken))
            {
                ModelState.AddModelError(string.Empty, "Reset token is required.");
                model.TokenIssued = true;
                return View(model);
            }

            var resetRequest = new
            {
                user_name = model.UserName,
                password_reset_token = model.ResetToken,
                new_password = model.NewPassword
            };

            var frameworkResult =
                await httpService.PostAsync<FrameworkResult>(ApiRoutePathConstants.ResetPassword, resetRequest);
            if (frameworkResult.Status == ResultStatus.Succeeded)
            {
                TempData["Message"] = "Password has been reset successfully.";
                return RedirectToAction(nameof(Login));
            }

            ModelState.AddModelError(frameworkResult.Errors.ToList()[0].Code,
                frameworkResult.Errors.ToList()[0].Description);
            model.TokenIssued = true;
            return View(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Reset-password submit failed.");
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    [AllowAnonymous]
    public ActionResult Register(string returnUrl = null)
    {
        try
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewBag.ListofSecurityQuestion = null;
            var userManagementHelper = new UserManagementHelper(httpService);
            ViewBag.ListofSecurityQuestion = userManagementHelper.LoadSecurityQuestionCombo();
            return View();
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Register(RegisterViewModel model, string returnUrl = null)
    {
        try
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new UserModel();
                user.UserName = model.UserName;
                user.Password = model.Password;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.UserSecurityQuestion = new List<UserSecurityQuestionModel>
                {
                    new()
                    {
                        SecurityQuestionId = model.SecurityQuestionId,
                        Answer = model.Answer
                    }
                };

                var frameworkResult =
                    await httpService.PostAsync<FrameworkResult>(ApiRoutePathConstants.RegisterUser, user);
                if (frameworkResult.Status == ResultStatus.Succeeded)
                {
                    TempData["username"] = user.UserName;
                    frameworkResult =
                        await httpService.PostAsync<FrameworkResult>(
                            ApiRoutePathConstants.GenerateEmailConfirmationToken, user.UserName);
                    if (frameworkResult.Status == ResultStatus.Succeeded)
                    {
                        frameworkResult =
                            await httpService.PostAsync<FrameworkResult>(
                                ApiRoutePathConstants.GeneratePhoneNumberConfirmationToken, user.UserName);
                        if (frameworkResult.Status == ResultStatus.Succeeded)
                        {
                            TempData["Message"] = "User registered successfully!";
                            return RedirectToAction("VerifyAccount", "Account");
                        }
                    }
                }

                ModelState.AddModelError(frameworkResult.Errors.ToList()[0].Code,
                    frameworkResult.Errors.ToList()[0].Description);
            }

            ViewBag.ListofSecurityQuestion = null;
            var userManagementHelper = new UserManagementHelper(httpService);
            ViewBag.ListofSecurityQuestion = userManagementHelper.LoadSecurityQuestionCombo();
            return View(model);
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public ActionResult VerifyAccount(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyAccount(VerifyAccountViewModel verifyAccountView, string button,
        string returnUrl = null)
    {
        try
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                FrameworkResult frameworkResult = null;
                var userName = TempData["userName"] as string;
                if (userName != null)
                {
                    if (button == "email")
                    {
                        frameworkResult =
                            await httpService.PostAsync<FrameworkResult>(
                                ApiRoutePathConstants.GenerateEmailConfirmationToken, userName);
                        if (frameworkResult.Status == ResultStatus.Failed)
                        {
                            ModelState.AddModelError(frameworkResult.Errors.ToList()[0].Code,
                                frameworkResult.Errors.ToList()[0].Description);
                            return View();
                        }

                        TempData["Message"] = "Email confirmation token send to registered email!";
                        return View();
                    }

                    if (button == "phonenumber")
                    {
                        frameworkResult =
                            await httpService.PostAsync<FrameworkResult>(
                                ApiRoutePathConstants.GeneratePhoneNumberConfirmationToken, userName);
                        if (frameworkResult.Status == ResultStatus.Failed)
                        {
                            ModelState.AddModelError(frameworkResult.Errors.ToList()[0].Code,
                                frameworkResult.Errors.ToList()[0].Description);
                            return View();
                        }

                        TempData["Message"] = "Phone number confirmation token send to registered phonenumber!";
                        return View();
                    }

                    if (string.IsNullOrWhiteSpace(verifyAccountView.EmailVerificationCode))
                    {
                        ModelState.AddModelError(string.Empty, "Email verification code required");
                        return View();
                    }

                    if (string.IsNullOrWhiteSpace(verifyAccountView.PhoneVerificationCode))
                    {
                        ModelState.AddModelError(string.Empty, "Phone number verification code required");
                        return View();
                    }

                    var emailToken = new
                        { user_name = userName, email_token = verifyAccountView.EmailVerificationCode };
                    frameworkResult =
                        await httpService.PostAsync<FrameworkResult>(ApiRoutePathConstants.VerifyEmailConfirmationToken,
                            emailToken);
                    if (frameworkResult.Status == ResultStatus.Failed)
                    {
                        ModelState.AddModelError(frameworkResult.Errors.ToList()[0].Code,
                            frameworkResult.Errors.ToList()[0].Description);
                        return View();
                    }

                    var phoneToken = new { user_name = userName, sms_token = verifyAccountView.PhoneVerificationCode };
                    frameworkResult =
                        await httpService.PostAsync<FrameworkResult>(
                            ApiRoutePathConstants.VerifyPhoneNumberConfirmationToken, phoneToken);
                    if (frameworkResult.Status == ResultStatus.Failed)
                    {
                        ModelState.AddModelError(frameworkResult.Errors.ToList()[0].Code,
                            frameworkResult.Errors.ToList()[0].Description);
                        return View();
                    }

                    // TempData["Message"] = "User account verified successfully!";
                    return RedirectToAction("Login", "Account");
                }

                ModelState.AddModelError("", "User not found");
            }

            return View();
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public ActionResult ManuallyVerifyAccount(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ManuallyVerifyAccount(VerifyAccountViewModel verifyAccountView, string action,
        string returnUrl = null)
    {
        try
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrWhiteSpace(verifyAccountView.UserName))
                ModelState.AddModelError("", "Username required.");

            if (ModelState.IsValid)
            {
                FrameworkResult frameworkResult = null;
                var userName = verifyAccountView.UserName;
                if (userName != null)
                {
                    if (action.Equals("GenerateEmailToken"))
                    {
                        frameworkResult =
                            await httpService.PostAsync<FrameworkResult>(
                                ApiRoutePathConstants.GenerateEmailConfirmationToken, userName);
                        if (frameworkResult.Status == ResultStatus.Failed)
                        {
                            ModelState.AddModelError(frameworkResult.Errors.ToList()[0].Code,
                                frameworkResult.Errors.ToList()[0].Description);
                            return View();
                        }

                        TempData["Message"] = "Email confirmation token send to registered email!";
                    }
                    else if (action.Equals("VerifyEmailToken"))
                    {
                        if (!string.IsNullOrWhiteSpace(verifyAccountView.EmailVerificationCode))
                        {
                            var emailToken = new
                                { user_name = userName, email_token = verifyAccountView.EmailVerificationCode };
                            frameworkResult =
                                await httpService.PostAsync<FrameworkResult>(
                                    ApiRoutePathConstants.VerifyEmailConfirmationToken, emailToken);
                            if (frameworkResult.Status == ResultStatus.Failed)
                            {
                                ModelState.AddModelError(frameworkResult.Errors.ToList()[0].Code,
                                    frameworkResult.Errors.ToList()[0].Description);
                                return View();
                            }

                            TempData["Message"] = "Email confirmation token verified!";
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Email verification code required");
                            return View();
                        }
                    }
                    else if (action.Equals("GeneratePhoneToken"))
                    {
                        frameworkResult =
                            await httpService.PostAsync<FrameworkResult>(
                                ApiRoutePathConstants.GeneratePhoneNumberConfirmationToken, userName);
                        if (frameworkResult.Status == ResultStatus.Failed)
                        {
                            ModelState.AddModelError(frameworkResult.Errors.ToList()[0].Code,
                                frameworkResult.Errors.ToList()[0].Description);
                            return View();
                        }

                        TempData["Message"] = "Phone number confirmation token send to registered phonenumber!";
                    }
                    else if (action.Equals("VerifyPhoneToken"))
                    {
                        if (!string.IsNullOrWhiteSpace(verifyAccountView.PhoneVerificationCode))
                        {
                            var phoneToken = new
                                { user_name = userName, sms_token = verifyAccountView.PhoneVerificationCode };
                            frameworkResult =
                                await httpService.PostAsync<FrameworkResult>(
                                    ApiRoutePathConstants.VerifyPhoneNumberConfirmationToken, phoneToken);
                            if (frameworkResult.Status == ResultStatus.Failed)
                            {
                                ModelState.AddModelError(frameworkResult.Errors.ToList()[0].Code,
                                    frameworkResult.Errors.ToList()[0].Description);
                                return View();
                            }

                            TempData["Message"] = "Phone number confirmation token verified!";
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Phone number verification code required");
                            return View();
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "User not found");
                    return View();
                }
            }

            return View();
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    private string ToSafeReturnUrl(string returnUrl, string fallback = "/")
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)) return returnUrl;

        if (!string.IsNullOrWhiteSpace(fallback) && Url.IsLocalUrl(fallback)) return fallback;

        return "/";
    }
}
