/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HCL.CS.SF.DemoClientMvc.Interface;
using HCL.CS.SF.DemoClientMvc.ViewModels.Api;

namespace HCL.CS.SF.DemoClientMvc.Controllers;

[Authorize]
public class ApiController(IApiClientService apiClientService) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View(new ApiTestViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Call(ApiTestViewModel model)
    {
        model ??= new ApiTestViewModel();

        if (Uri.TryCreate(model.RelativePath, UriKind.Absolute, out _))
        {
            model.ErrorMessage = "Absolute URLs are not allowed.";
            return View("Index", model);
        }

        var response = await apiClientService.GetAsync(model.RelativePath, HttpContext.RequestAborted);
        model.StatusCode = response.StatusCode;
        model.ResponseBody = response.ResponseBody;
        model.ErrorMessage = response.Succeeded ? null : response.ErrorMessage;

        return View("Index", model);
    }
}
