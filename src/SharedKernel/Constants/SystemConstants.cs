/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.SharedKernel.Constants;

/// <summary>
/// Provides system-wide constant values shared across all modules of the Security Framework.
/// </summary>
public static class SystemConstants
{
    /// <summary>
    /// The default culture/locale used for localization when no specific culture is configured.
    /// </summary>
    public const string DefaultCulture = "en-US";

    /// <summary>
    /// The OAuth/OIDC scope identifier for the identity service API.
    /// </summary>
    public const string IdentityScope = "HCL.CS.SF.identity";
}
