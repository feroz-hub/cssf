/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.DemoClientMvc.ViewModels.Shared;

namespace HCL.CS.SF.DemoClientMvc.ViewModels.Endpoint;

public sealed class AuthorizeEndpointViewModel
{
    public IReadOnlyCollection<ClaimItemViewModel> Claims { get; set; } = new List<ClaimItemViewModel>();

    public IReadOnlyCollection<ClaimItemViewModel> AuthenticationProperties { get; set; } =
        new List<ClaimItemViewModel>();
}
