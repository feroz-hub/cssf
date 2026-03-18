/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.ComponentModel.DataAnnotations;

namespace HCL.CS.SF.DemoClientMvc.Models;

public class VerifyAccountViewModel
{
    [Display(Name = "UserName")] public string UserName { get; set; }

    [Display(Name = "Email verification code")]
    public string EmailVerificationCode { get; set; }

    [Display(Name = "Phone number verification code")]
    public string PhoneVerificationCode { get; set; }

    public string ReturnUrl { get; set; }
}

//public class VerifyUserAccountViewModel
//{
//    [Display(Name = "Email verification code")]
//    public string EmailVerificationCode { get; set; }

//    [Display(Name = "Phone number verification code")]
//    public string PhoneVerificationCode { get; set; }

//    public string ReturnUrl { get; set; }
//}
