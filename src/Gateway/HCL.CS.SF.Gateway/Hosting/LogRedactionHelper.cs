/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Text.RegularExpressions;

namespace HCL.CS.SF.ProxyService.Hosting;

/// <summary>
/// Provides helper methods for redacting sensitive data (passwords, tokens, PII)
/// from log output to prevent accidental exposure in structured logs and metrics.
/// </summary>
internal static partial class LogRedactionHelper
{
    /// <summary>
    /// The replacement value used when a field is identified as sensitive.
    /// </summary>
    private const string RedactedValue = "[REDACTED]";

    /// <summary>
    /// A set of field name substrings that indicate the value contains sensitive data
    /// and must be redacted before logging.
    /// </summary>
    private static readonly HashSet<string> SensitiveFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "password",
        "secret",
        "token",
        "authorization",
        "cookie",
        "apikey",
        "api_key",
        "email",
        "phone",
        "ssn"
    };

    /// <summary>
    /// Returns the value if the field name is not sensitive; otherwise returns "[REDACTED]".
    /// </summary>
    /// <param name="fieldName">The name of the field being logged.</param>
    /// <param name="value">The raw value of the field.</param>
    /// <returns>The original value or "[REDACTED]" if the field is sensitive.</returns>
    internal static string RedactByFieldName(string fieldName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        return IsSensitiveField(fieldName) ? RedactedValue : value.Trim();
    }

    /// <summary>
    /// Returns a safe tenant ID for logging, redacting it if it matches a sensitive pattern.
    /// </summary>
    /// <param name="tenantId">The raw tenant identifier.</param>
    /// <returns>The tenant ID or an empty string if null/whitespace.</returns>
    internal static string GetSafeTenantId(string? tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) return string.Empty;
        return RedactByFieldName("tenantId", tenantId);
    }

    /// <summary>
    /// Returns a safe user ID for logging. Redacts values that look like email addresses,
    /// phone numbers, or excessively long strings to prevent PII leakage.
    /// Returns "anonymous" when no user ID is available.
    /// </summary>
    /// <param name="userId">The raw user identifier.</param>
    /// <returns>The sanitized user ID, "anonymous", or "[REDACTED]".</returns>
    internal static string GetSafeUserId(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return "anonymous";

        var trimmed = userId.Trim();
        // Redact if too long (likely an encoded token or garbage value)
        if (trimmed.Length > 64) return RedactedValue;
        // Redact if it looks like an email address
        if (trimmed.Contains('@', StringComparison.Ordinal)) return RedactedValue;
        // Redact if it contains whitespace (malformed or injected value)
        if (trimmed.Any(char.IsWhiteSpace)) return RedactedValue;
        // Redact if it looks like a phone number
        if (PhoneLikeRegex().IsMatch(trimmed)) return RedactedValue;

        return trimmed;
    }

    /// <summary>
    /// Checks whether the given field name contains any known sensitive keyword.
    /// </summary>
    /// <param name="fieldName">The field name to evaluate.</param>
    /// <returns><c>true</c> if the field name matches a sensitive keyword; otherwise, <c>false</c>.</returns>
    private static bool IsSensitiveField(string fieldName)
    {
        if (string.IsNullOrWhiteSpace(fieldName)) return false;
        return SensitiveFields.Any(s => fieldName.Contains(s, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Source-generated regex that matches strings resembling phone numbers (7-15 digits, optional leading +).
    /// </summary>
    [GeneratedRegex(@"^\+?\d{7,15}$", RegexOptions.Compiled)]
    private static partial Regex PhoneLikeRegex();
}
