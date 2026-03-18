/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Api;

/// <summary>
/// Represents a user's answer to a security question, used as an additional
/// identity verification factor during account recovery or sensitive operations.
/// </summary>
public class UserSecurityQuestionModel : BaseModel
{
    /// <summary>The unique identifier of the user who answered the security question.</summary>
    public virtual Guid UserId { get; set; }

    /// <summary>The unique identifier of the security question being answered.</summary>
    public virtual Guid SecurityQuestionId { get; set; }

    /// <summary>The user's answer to the security question (stored hashed or encrypted).</summary>
    public virtual string Answer { get; set; }
}
