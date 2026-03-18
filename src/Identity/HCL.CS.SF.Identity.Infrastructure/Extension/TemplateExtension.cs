/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Entities.Api;

namespace HCL.CS.SF.Infrastructure.Services.Extension;

/// <summary>
/// Provides extension methods for replacing placeholders in notification templates
/// with actual user data and custom parameters.
/// </summary>
internal static class TemplateExtension
{
    /// <summary>
    /// Replaces placeholder tokens in a notification template string with values
    /// from the provided user entity and custom parameter dictionary.
    /// </summary>
    /// <param name="template">The template string containing placeholder tokens (e.g., {USERNAME}, {EMAIL}).</param>
    /// <param name="users">The user entity whose properties are substituted into the template.</param>
    /// <param name="parameters">Additional key-value pairs for custom placeholder replacement.</param>
    /// <returns>The template string with all recognized placeholders replaced.</returns>
    internal static string UpdateNotificationTemplatePlaceholder(this string template, Users users,
        Dictionary<string, string> parameters)
    {
        // Replace custom parameters first (caller-supplied key-value pairs)
        foreach (var keyValue in parameters)
            if (template.Contains(keyValue.Key) && !string.IsNullOrWhiteSpace(keyValue.Value))
                template = template.Replace(keyValue.Key, keyValue.Value);

        // Replace built-in user-related placeholders
        if (template.Contains("{USERNAME}")) template = template.Replace("{USERNAME}", users.FirstName);

        if (template.Contains("{FIRSTNAME}")) template = template.Replace("{FIRSTNAME}", users.FirstName);

        if (template.Contains("{LASTNAME}")) template = template.Replace("{LASTNAME}", users.LastName);

        if (template.Contains("{FULLNAME}"))
            template = template.Replace("{FULLNAME}", users.FirstName + " " + users.LastName);

        if (template.Contains("{USERID}")) template = template.Replace("{USERID}", Convert.ToString(users.Id));

        if (template.Contains("{EMAIL}")) template = template.Replace("{EMAIL}", users.Email);

        return template;
    }
}
