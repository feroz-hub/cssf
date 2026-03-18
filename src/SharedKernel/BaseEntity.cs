/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.SharedKernel;

/// <summary>
/// Abstract base class for all domain entities, providing identity and auditing fields.
/// Subclasses inherit a unique <see cref="Id"/>, creation timestamp, and an optional
/// last-updated timestamp that is refreshed via <see cref="Touch"/>.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity, auto-generated on construction.
    /// </summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <summary>
    /// UTC timestamp indicating when the entity was created.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; protected set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// UTC timestamp of the most recent update, or <c>null</c> if the entity has never been modified.
    /// </summary>
    public DateTimeOffset? UpdatedAtUtc { get; protected set; }

    /// <summary>
    /// Sets <see cref="UpdatedAtUtc"/> to the current UTC time, marking the entity as modified.
    /// </summary>
    protected void Touch()
    {
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
