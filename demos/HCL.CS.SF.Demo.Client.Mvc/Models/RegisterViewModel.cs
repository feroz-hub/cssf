/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.ComponentModel.DataAnnotations;

namespace HCL.CS.SF.DemoClientMvc.Models;

public class RegisterViewModel
{
    [Required]
    [Display(Name = "UserName")]
    public string UserName { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }

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

    [Required]
    [RegularExpression("^((?!00000000-0000-0000-0000-000000000000).)*$",
        ErrorMessage = "The Security Question field is required.")]
    [Display(Name = "Security Question")]
    public Guid SecurityQuestionId { get; set; }

    [Required] [Display(Name = "Answer")] public string Answer { get; set; }

    public string ReturnUrl { get; set; }
}
