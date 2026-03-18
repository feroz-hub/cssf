/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Identity;

namespace HCL.CS.SF.Domain.Entities.Api;
// TODO: Jesu needs to remove this class as it is not required

/// <summary>
/// Extends ASP.NET Identity's <see cref="IdentityUserToken{TKey}"/> with soft-delete support.
/// Stores external login provider tokens (e.g., access/refresh tokens from Google or Azure AD)
/// associated with a user. Marked for potential removal as it may be unused.
/// </summary>
public class UserTokens : IdentityUserToken<Guid>
{
    /// <summary>Soft-delete flag; when true the token record is logically removed.</summary>
    public virtual bool IsDeleted { get; set; } = false;
}
