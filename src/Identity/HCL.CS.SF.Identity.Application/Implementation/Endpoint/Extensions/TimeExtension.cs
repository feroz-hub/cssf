/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Service.Implementation.Endpoint.Extensions;

/// <summary>
/// Extension methods for Unix timestamp conversions used in JWT token claims.
/// Converts between <see cref="DateTime"/> and Unix epoch seconds (iat, exp, nbf claims).
/// </summary>
public static class TimeExtension
{
    /// <summary>
    /// Converts a <see cref="DateTime"/> to Unix epoch seconds (seconds since 1970-01-01T00:00:00Z).
    /// Used for JWT 'iat', 'exp', and 'nbf' numeric date claims per RFC 7519 Section 2.
    /// </summary>
    /// <param name="time">The DateTime to convert.</param>
    /// <returns>The number of seconds since the Unix epoch, or 0 if the time is before the epoch.</returns>
    public static long ToUnixTime(this DateTime time)
    {
        var unixEpoch = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc);
        var totalSeconds = time.Subtract(unixEpoch).TotalSeconds;
        if (totalSeconds < 0) return 0;

        return Convert.ToInt32(totalSeconds);
    }

    /// <summary>
    /// Converts Unix epoch seconds to a UTC <see cref="DateTime"/>.
    /// </summary>
    /// <param name="unixTime">The Unix timestamp in seconds.</param>
    /// <returns>The corresponding UTC DateTime.</returns>
    public static DateTime ToDateTime(this long unixTime)
    {
        var unixEpoch = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc);
        return unixEpoch.Add(TimeSpan.FromSeconds(unixTime));
    }
}
