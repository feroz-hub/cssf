/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Entities.Api;

/// <summary>
/// Represents a predefined security question available for account recovery.
/// Users select from these questions and provide answers via <see cref="UserSecurityQuestions"/>
/// to serve as a knowledge-based identity verification factor.
/// </summary>
public class SecurityQuestions : BaseEntity
{
    /// <summary>The security question text presented to the user (e.g., "What is your mother's maiden name?").</summary>
    public virtual string Question { get; set; }
}
