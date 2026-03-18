/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Api;

/// <summary>
/// Represents a protected API route with its required permissions.
/// Used for route-level authorization to determine which permission claims
/// a user must have in their access token to access a given endpoint.
/// </summary>
public class ApiRouteModel
{
    /// <summary>The logical name of the API route.</summary>
    public virtual string Name { get; set; }

    /// <summary>The URL path of the API route.</summary>
    public virtual string Path { get; set; }

    /// <summary>The list of permission claim values required to access this route.</summary>
    public virtual List<string> Permissions { get; set; }
}

//public class ApiPermissionModel
//{
//    /// <summary>
//    /// Gets or sets the Permission for Api.
//    /// </summary>
//    public virtual string Permission { get; set; }
//}
