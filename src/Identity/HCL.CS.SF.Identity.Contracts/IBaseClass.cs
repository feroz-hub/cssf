/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

//***********************************************************************************************************************************
//
// Copyright (c) 2017 HCL.CS.SF CORPORATION.
//
// All rights reserved. HCL.CS.SF source code is an unpublished work and the use of a copyright notice does not imply otherwise.
// This source code contains confidential, trade secret material of HCL.CS.SF. Any attempt or participation in deciphering,
// decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
// HCL.CS.SF is obtained. This is proprietary and confidential to HCL.CS.SF.
//
//***********************************************************************************************************************************

#region History

// File Name: IBaseClass.cs
// Rev 1.0 - 02-Mar-2021 - Jesu A - Initial revision.
//

#endregion

namespace HCL.CS.SF.Service.Interfaces;

/// <summary>
/// Marker interface for all service classes in the framework. Extends <see cref="IDisposable"/>
/// to ensure proper cleanup of resources (e.g., database connections, HTTP clients) when
/// services are disposed by the DI container.
/// </summary>
public interface IBaseClass : IDisposable
{
}
