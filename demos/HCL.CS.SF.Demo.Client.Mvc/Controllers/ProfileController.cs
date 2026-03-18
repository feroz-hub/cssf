/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HCL.CS.SF.DemoClientMvc.ViewModels.Profile;
using HCL.CS.SF.DemoClientMvc.ViewModels.Shared;
using HCL.CS.SF.Domain.Constants.Endpoint;

namespace HCL.CS.SF.DemoClientMvc.Controllers;

[Authorize]
public class ProfileController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var claims = User.Claims
            .Select(claim => new ClaimItemViewModel
            {
                Type = claim.Type,
                Value = claim.Value
            })
            .OrderBy(claim => claim.Type)
            .ToList();

        var model = new ProfileViewModel
        {
            UserName = User.Identity?.Name ?? string.Empty,
            Email = User.Claims.FirstOrDefault(claim => claim.Type == OpenIdConstants.ClaimTypes.Email)?.Value ??
                    string.Empty,
            SubjectId = User.Claims.FirstOrDefault(claim => claim.Type == OpenIdConstants.ClaimTypes.Sub)?.Value ??
                        string.Empty,
            Roles = User.Claims
                .Where(claim => claim.Type == OpenIdConstants.ClaimTypes.Role || claim.Type == ClaimTypes.Role)
                .Select(claim => claim.Value)
                .Distinct()
                .ToList(),
            Claims = claims
        };

        return View(model);
    }
}
