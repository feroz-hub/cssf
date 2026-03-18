/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Constants;

namespace HCL.CS.SF.ProxyService.Extension;

/// <summary>
/// Provides extension methods for expanding permission strings.
/// A "manage" permission is treated as a shorthand for read, write, and delete
/// permissions on the same resource, enabling coarse-grained role definitions.
/// </summary>
internal static class ScopesExtension
{
    /// <summary>
    /// Expands a list of permission strings by replacing any permission containing
    /// the "manage" keyword with three separate permissions (read, write, delete).
    /// Non-manage permissions are passed through unchanged. Duplicates are removed.
    /// </summary>
    /// <param name="permissions">The original list of permission strings to expand.</param>
    /// <returns>A deduplicated list of expanded permission strings.</returns>
    internal static List<string> ExpandPermissions(this List<string> permissions)
    {
        var newPermissionsList = new List<string>();
        if (permissions != null && permissions.Any())
            foreach (var scope in permissions)
                if (scope.Contains(PermissionConstants.Manage))
                {
                    // Expand "manage" into the three granular permission types.
                    newPermissionsList.Add(scope.Replace(PermissionConstants.Manage, PermissionConstants.Read));
                    newPermissionsList.Add(scope.Replace(PermissionConstants.Manage, PermissionConstants.Write));
                    newPermissionsList.Add(scope.Replace(PermissionConstants.Manage, PermissionConstants.Delete));
                }
                else
                {
                    newPermissionsList.Add(scope);
                }

        return newPermissionsList.Distinct().ToList();
    }
}
