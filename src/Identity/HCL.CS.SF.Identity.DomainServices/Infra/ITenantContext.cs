/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.DomainServices.Infra;

/// <summary>
/// Provides the current tenant identifier for multi-tenant deployments.
/// Implementations typically resolve the tenant from the HTTP request (header, subdomain, or path).
/// </summary>
public interface ITenantContext
{
    /// <summary>Gets the identifier of the current tenant.</summary>
    string TenantId { get; }
}
