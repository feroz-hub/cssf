/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Service.Interfaces;

namespace HCL.CS.SF.Service.Implementation;
// TODO: Need to fix warning S3881: Fix implementation of IDisposable to conform to Dispose pattern.

/// <summary>
/// Abstract base class for all security framework service implementations.
/// Provides a standard <see cref="IDisposable"/> pattern that derived classes can override
/// to release managed and unmanaged resources.
/// </summary>
public abstract class SecurityBase : IBaseClass
{
    /// <summary>
    /// Releases all resources used by this instance. Calls <see cref="Dispose(bool)"/>
    /// with <c>true</c> and suppresses finalization.
    /// </summary>
    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~SecurityBase()
    {
        Dispose(false);
    }

    /// <summary>
    /// Releases the unmanaged resources used by this instance and optionally releases managed resources.
    /// Override in derived classes to clean up service-specific resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
    }
}
