/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Api;

/// <summary>
/// Represents a security question that users can select and answer
/// as an additional identity verification factor for account recovery.
/// </summary>
public class SecurityQuestionModel : BaseModel
{
    /// <summary>The text of the security question (e.g., "What is your mother's maiden name?").</summary>
    public virtual string Question { get; set; }
}
