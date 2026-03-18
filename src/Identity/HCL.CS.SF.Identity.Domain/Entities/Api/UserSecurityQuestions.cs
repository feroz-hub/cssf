/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Entities.Api;

/// <summary>
/// Associates a user with a selected security question and their hashed answer.
/// Used as an additional identity-verification factor during account recovery flows.
/// </summary>
public class UserSecurityQuestions : BaseEntity
{
    /// <summary>Foreign key referencing the user who set up the security question.</summary>
    public virtual Guid UserId { get; set; }

    /// <summary>Foreign key referencing the selected security question.</summary>
    public virtual Guid SecurityQuestionId { get; set; }

    /// <summary>The user's answer to the security question (should be stored hashed for security).</summary>
    public virtual string Answer { get; set; }

    /// <summary>Navigation property to the owning user.</summary>
    public virtual Users User { get; set; }

    /// <summary>Navigation property to the associated security question.</summary>
    public virtual SecurityQuestions SecurityQuestion { get; set; }
}
