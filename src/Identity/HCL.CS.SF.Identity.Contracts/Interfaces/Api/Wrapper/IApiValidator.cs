/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Runtime.CompilerServices;
using HCL.CS.SF.Domain;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Api.Wrapper;

/// <summary>
/// Validates incoming API requests for authorization, ensuring the caller has the
/// required permissions. Automatically captures the calling method name for audit logging.
/// </summary>
public interface IApiValidator
{
    /// <summary>
    /// Validates that the current request is authorized to invoke the specified API operation.
    /// </summary>
    /// <param name="callerMemberName">Auto-captured name of the calling method for permission resolution.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating whether the request is authorized.</returns>
    Task<FrameworkResult> ValidateRequest([CallerMemberName] string callerMemberName = null);
}
