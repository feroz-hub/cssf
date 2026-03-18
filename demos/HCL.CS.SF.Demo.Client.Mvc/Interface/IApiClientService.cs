/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.DemoClientMvc.Services;

namespace HCL.CS.SF.DemoClientMvc.Interface;

public interface IApiClientService
{
    Task<ApiClientResponse> GetAsync(string relativePath, CancellationToken cancellationToken = default);

    Task<ApiClientResponse> PostAsync<TRequest>(string relativePath, TRequest payload,
        CancellationToken cancellationToken = default);
}
