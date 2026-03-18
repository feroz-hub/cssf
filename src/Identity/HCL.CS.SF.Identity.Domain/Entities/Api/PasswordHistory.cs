/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Entities.Api;

/// <summary>
/// Tracks previous password hashes for a user to enforce password-history policies.
/// Prevents users from reusing recent passwords, reducing the risk of credential compromise.
/// </summary>
public class PasswordHistory : BaseEntity
{
    /// <summary>Foreign key referencing the user whose password was changed.</summary>
    public virtual Guid UserId { get; set; }

    /// <summary>UTC timestamp when the password change occurred.</summary>
    public virtual DateTime ChangedOn { get; set; }

    /// <summary>The hashed value of the previous password, stored for reuse-prevention checks.</summary>
    public virtual string PasswordHash { get; set; }

    /// <summary>Navigation property to the owning user.</summary>
    public virtual Users User { get; set; }
}
