/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Reflection;
using System.Security.Claims;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Models.Api;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Extensions;

/// <summary>
/// Extension methods for resolving user claims from OpenID Connect identity resources.
/// Maps identity resource claim types to user model properties and custom user claims,
/// applying standard claim type mappings.
/// </summary>
public static class IdentityResourceExtension
{
    /// <summary>
    /// Retrieves the user identity resource.
    /// </summary>
    /// <returns>The matching collection of results.</returns>
    public static async Task<List<Claim>> GetUserIdentityResources(this UserModel userInfo,
        IList<IdentityResourcesByScopesModel> identityResources)
    {
        var userClaimsList = new List<string>();
        var resultClaims = new List<Claim>();

        var userClaims = userInfo.UserClaims;
        var userInfoProperties = userInfo.GetType().GetProperties();
        var userInfoPropertyNames = userInfoProperties.ToList().ConvertAll(property => property.Name.ToLower());
        if (userInfo.UserClaims.ContainsAny())
            userClaimsList.AddRange(userClaims.ConvertAll(userClaim => userClaim.ClaimType.ToLower()));

        foreach (var idResource in identityResources)
        {
            var claimType = !string.IsNullOrWhiteSpace(idResource.IdentityResourceClaimAliasType)
                ? idResource.IdentityResourceClaimAliasType
                : idResource.IdentityResourceClaimType;
            object infoValue = null;
            if (userClaimsList.Contains(claimType))
                infoValue = userClaims.FirstOrDefault(userClaim => userClaim.ClaimType.ToLower() == claimType)
                    ?.ClaimValue;
            else if (userInfoPropertyNames.Contains(claimType))
                infoValue = userInfo.GetType().GetProperty(
                        claimType,
                        BindingFlags.SetProperty | BindingFlags.IgnoreCase | BindingFlags.Public |
                        BindingFlags.Instance)
                    ?.GetValue(userInfo, null);

            if (infoValue != null)
            {
                var stdClaim = AuthenticationConstants.StandardClaims.FirstOrDefault(x => x.Key == claimType);
                if (stdClaim.Value != null)
                    resultClaims.Add(new Claim(stdClaim.Value, infoValue.ToString()));
                else
                    resultClaims.Add(new Claim(claimType, infoValue.ToString()));
            }
        }

        return await Task.FromResult(resultClaims);
    }
}
