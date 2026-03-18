/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.ComponentModel.DataAnnotations;

namespace HCL.CS.SF.DemoClientMvc.Models;

public class UpdateViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [Required]
    [Phone]
    [Display(Name = "PhoneNumber")]
    public string PhoneNumber { get; set; }

    [Required]
    [Display(Name = "FirstName")]
    public string FirstName { get; set; }

    [Display(Name = "LastName")] public string LastName { get; set; }

    public string ReturnUrl { get; set; }
}
