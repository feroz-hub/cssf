/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using HCL.CS.SF.Domain.Models.Endpoint.Validation;

namespace HCL.CS.SF.Domain.Models.Endpoint.Request;

/// <summary>
/// Represents a validated request to the OIDC UserInfo endpoint.
/// The client presents an access token, which is decoded and validated
/// to extract the user's identity claims.
/// </summary>
public class ValidatedUserInfoRequestModel : ValidatedBaseModel
{
    /// <summary>The user's identity claims to be returned in the UserInfo response.</summary>
    public List<Claim> Claims { get; set; }

    /// <summary>The raw access token string submitted by the client.</summary>
    public string Token { get; set; }

    /// <summary>The security key used to validate the access token signature.</summary>
    public SecurityKey Key { get; set; }

    /// <summary>The decoded JWT access token after signature and lifetime validation.</summary>
    public JwtSecurityToken DecodedToken { get; set; }
}
