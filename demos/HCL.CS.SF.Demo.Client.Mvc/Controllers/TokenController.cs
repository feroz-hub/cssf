/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HCL.CS.SF.DemoClientMvc.Interface;
using HCL.CS.SF.DemoClientMvc.ViewModels.Shared;
using HCL.CS.SF.DemoClientMvc.ViewModels.Token;

namespace HCL.CS.SF.DemoClientMvc.Controllers;

[Authorize]
public class TokenController(ITokenService tokenService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var accessToken = await tokenService.GetAccessTokenAsync(cancellationToken);
        var refreshToken = await tokenService.GetRefreshTokenAsync(cancellationToken);
        var idToken = await tokenService.GetIdTokenAsync(cancellationToken);
        var expiresAt = await tokenService.GetAccessTokenExpiresAtAsync(cancellationToken);

        var model = new TokenViewModel
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            IdToken = idToken,
            ExpiresAtUtc = expiresAt?.UtcDateTime.ToString("u"),
            AccessTokenClaims = ParseAccessTokenClaims(accessToken)
        };

        return View(model);
    }

    private static IReadOnlyCollection<ClaimItemViewModel> ParseAccessTokenClaims(string? accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken)) return Array.Empty<ClaimItemViewModel>();

        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(accessToken)) return Array.Empty<ClaimItemViewModel>();

        var token = handler.ReadJwtToken(accessToken);
        return token.Claims
            .GroupBy(claim => claim.Type)
            .Select(group => new ClaimItemViewModel
            {
                Type = group.Key,
                Value = string.Join(", ", group.Select(c => c.Value))
            })
            .OrderBy(claim => claim.Type, StringComparer.Ordinal)
            .ToList();
    }
}
