/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Infrastructure.Data;

/// <summary>
/// Abstract base class that implements the <see cref="IDisposable"/> pattern.
/// Provides a consistent disposal mechanism for persistence layer classes
/// such as repositories and unit-of-work implementations.
/// </summary>
public abstract class BaseDispose : IDisposable
{
    /// <summary>
    /// Releases all resources and suppresses finalization.
    /// </summary>
    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases managed resources when called with <paramref name="disposing"/> set to true.
    /// Override in derived classes to clean up managed resources.
    /// </summary>
    /// <param name="disposing">True if called from <see cref="Dispose()"/>; false if called from a finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
    }
}
