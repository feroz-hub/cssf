/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using HCL.CS.SF.DemoClientMvc.Constants;
using HCL.CS.SF.DemoClientMvc.Interface;
using HCL.CS.SF.DemoClientMvc.ViewModels.Home;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Models.Api;

namespace HCL.CS.SF.DemoClientMvc.Controllers;

public class HomeController(IHttpService httpService) : Controller
{
    public async Task<IActionResult> Index()
    {
        try
        {
            if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userid = HttpContext.Session.GetString(ApplicationConstants.SessionUserId);
                if (userid == null)
                {
                    var userModel = await httpService.PostSecureAsync<UserModel>(ApiRoutePathConstants.GetUserByName,
                        User.Identity.Name);
                    if (userModel != null)
                        HttpContext.Session.SetString(ApplicationConstants.SessionUserId, userModel.Id.ToString());
                }
            }

            var model = new HomeIndexViewModel
            {
                IsAuthenticated = User?.Identity?.IsAuthenticated == true,
                UserName = User?.Identity?.Name ?? string.Empty,
                Roles = User?.Claims
                    .Where(claim => claim.Type == OpenIdConstants.ClaimTypes.Role || claim.Type == ClaimTypes.Role)
                    .Select(claim => claim.Value)
                    .Distinct()
                    .ToList() ?? new List<string>()
            };

            return View(model);
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }
}
