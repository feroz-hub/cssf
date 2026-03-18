/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.ComponentModel.DataAnnotations;

namespace HCL.CS.SF.DemoClientMvc.Models;

public class ClientViewModel
{
    public int Id { get; set; }
    public string ClientId { get; set; }
    public string ClientName { get; set; }
}

public class ManageClientViewModel
{
    public string ClientId { get; set; }

    [Required]
    [Display(Name = "ClientName")]
    public string ClientName { get; set; }

    [Required]
    [Display(Name = "ClientUri")]
    public string ClientUri { get; set; }

    [Required] [Display(Name = "LogoUri")] public string LogoUri { get; set; }

    [Required]
    [Display(Name = "TermsOfServiceUri")]
    public string TermsOfServiceUri { get; set; }

    [Required]
    [Display(Name = "PolicyUri")]
    public string PolicyUri { get; set; }

    [Required]
    [Display(Name = "RefreshTokenExpiration")]
    public int RefreshTokenExpiration { get; set; }

    [Required]
    [Display(Name = "AccessTokenExpiration")]
    public int AccessTokenExpiration { get; set; }

    [Required]
    [Display(Name = "IdentityTokenExpiration")]
    public int IdentityTokenExpiration { get; set; }

    [Required]
    [Display(Name = "LogoutTokenExpiration")]
    public int LogoutTokenExpiration { get; set; }

    [Required]
    [Display(Name = "AuthorizationCodeExpiration")]
    public int AuthorizationCodeExpiration { get; set; }

    [Required]
    [Display(Name = "AllowedSigningAlgorithm")]
    public string AllowedSigningAlgorithm { get; set; }

    [Required]
    [Display(Name = "SupportedGrantTypes")]
    public string SupportedGrantTypes { get; set; }

    [Required]
    [Display(Name = "AllowedScopes")]
    public string AllowedScopes { get; set; }

    [Required]
    [Display(Name = "SupportedResponseTypes")]
    public string SupportedResponseTypes { get; set; }

    [Required]
    [Display(Name = "FrontChannelLogoutUri")]
    public string FrontChannelLogoutUri { get; set; }

    [Required]
    [Display(Name = "BackChannelLogoutUri")]
    public string BackChannelLogoutUri { get; set; }
}
