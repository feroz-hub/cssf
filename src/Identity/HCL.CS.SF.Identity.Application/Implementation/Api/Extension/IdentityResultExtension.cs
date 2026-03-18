/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Identity;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;

namespace HCL.CS.SF.Service.Implementation.Api.Extension;

/// <summary>
/// Extension methods for converting ASP.NET Identity <see cref="IdentityResult"/> errors
/// into the framework's error format for consistent error reporting across the security layer.
/// </summary>

internal static class IdentityResultExtension
{
    /// <summary>
    /// Converts identity result errors into a list of <see cref="FrameworkError"/> objects
    /// for structured error reporting in API responses.
    /// </summary>
    /// <param name="identityResult">The identity operation result containing errors.</param>
    /// <returns>A list of framework errors, or an empty enumerable if no errors exist.</returns>

    internal static IEnumerable<FrameworkError> ConstructIdentityErrorAsList(this IdentityResult identityResult)
    {
        if (identityResult.Errors.ContainsAny())
            return identityResult.Errors.Select(x => new FrameworkError { Code = x.Code, Description = x.Description })
                .ToList();

        return Enumerable.Empty<FrameworkError>();
    }

    /// <summary>
    /// Converts identity result errors into a single comma-separated error description string
    /// for logging or simple error display scenarios.
    /// </summary>
    /// <param name="identityResult">The identity operation result containing errors.</param>
    /// <returns>A comma-separated string of error descriptions, or null if no errors.</returns>

    internal static string ConstructIdentityErrorAsString(this IdentityResult identityResult)
    {
        if (identityResult.Errors.ContainsAny())
            return string.Join(", ", identityResult.Errors.Select(e => e.Description));

        return null;
    }
}
