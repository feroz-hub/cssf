/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Globalization;
using System.Text.RegularExpressions;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Configurations.Api;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Api.Response;
using HCL.CS.SF.DomainServices.Infra;

namespace HCL.CS.SF.Service.Implementation.Api.Validators;

/// <summary>
/// Provides validation utilities for user management operations including password expiry checking,
/// email format validation, phone number validation, date-of-birth validation, and password
/// complexity enforcement. Used throughout the identity service to enforce security policies.
/// </summary>

internal class UserManagementValidator
{
    /// <summary>
    /// Validates whether a user's password has expired based on the configured maximum password age.
    /// Also warns when the password is approaching expiry within the notification window.
    /// </summary>
    /// <param name="user">The user entity with the last password change date.</param>
    /// <param name="settings">System settings containing password expiry configuration.</param>
    /// <param name="resourceStringHandler">Handler for resolving localized error messages.</param>
    /// <param name="loggerService">Logger for recording validation outcomes.</param>
    /// <returns>A sign-in response indicating success, warning, or password-expired error.</returns>

    internal SignInResponseModel ValidatePasswordExpiryAsync(
        Users user,
        SystemSettings settings,
        IResourceStringHandler resourceStringHandler,
        ILoggerService loggerService)
    {
        var signinResponse = new SignInResponseModel();
        signinResponse.Succeeded = true;
        if (user.LastPasswordChangedDate != null)
        {
            var days = Convert.ToDateTime(user.LastPasswordChangedDate)
                .AddDays(settings.PasswordConfig.MaxPasswordExpiry).Subtract(DateTime.UtcNow).TotalDays;
            if (days < 0)
            {
                loggerService.WriteTo(Log.Error, "Password expired for user: " + user.UserName);
                return ConstructError(ApiErrorCodes.PasswordExpired, resourceStringHandler);
            }

            if (days < settings.PasswordConfig.PasswordNotificationBeforeExpiry)
            {
                var day = days.ToString().Split('.');
                var message =
                    string.Format(resourceStringHandler.GetResourceString(ApiErrorCodes.PasswordAboutToExpire), day[0]);
                loggerService.WriteTo(Log.Debug, message);
                signinResponse.Message = message;
                return signinResponse;
            }
        }

        return signinResponse;
    }

    /// <summary>
    /// Validates an email address format using RFC-compliant regex and IDN (internationalized domain name) normalization.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns>True if the email format is valid; false otherwise.</returns>

    internal bool IsValidEmailAddress(string email)
    {
        // Reference - https://docs.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format
        if (string.IsNullOrWhiteSpace(email)) return false;

        try
        {
            // Normalize the domain
            email = Regex.Replace(email, @"(@)(.+)$", DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200));

            // Examines the domain part of the email and normalizes it.
            string DomainMapper(Match match)
            {
                // Use IdnMapping class to convert Unicode domain names.
                var idn = new IdnMapping();

                // Pull out and process domain name (throws ArgumentException on invalid)
                var domainName = idn.GetAscii(match.Groups[2].Value);

                return match.Groups[1].Value + domainName;
            }
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }

        try
        {
            return Regex.IsMatch(
                email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase,
                TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    /// <summary>
    /// Validates phone number length against configurable minimum and maximum bounds.
    /// </summary>
    /// <param name="phoneNumber">The phone number to validate.</param>
    /// <param name="minLength">Minimum acceptable length.</param>
    /// <param name="maxLength">Maximum acceptable length.</param>
    /// <returns>True if the phone number length is within bounds.</returns>

    internal bool IsValidPhoneNumber(string phoneNumber, int minLength, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)) return false;

        if (phoneNumber.Length > minLength && phoneNumber.Length <= maxLength) return true;

        return false;
    }

    /// <summary>
    /// Validates phone number format using a standard regex pattern supporting international prefixes.
    /// </summary>
    /// <param name="phoneNumber">The phone number to validate.</param>
    /// <returns>True if the phone number matches the expected format.</returns>

    internal bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)) return false;

        var regex = @"^(\+\d{1,2}\s?)?1?\-?\.?\s?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$";
        var match = Regex.Match(phoneNumber, regex, RegexOptions.IgnoreCase);

        if (!match.Success) return false;

        return true;
    }

    /// <summary>
    /// Validates that a date of birth falls within the acceptable age range.
    /// </summary>
    /// <param name="dob">The date of birth to validate.</param>
    /// <param name="minAge">Minimum acceptable age in years.</param>
    /// <param name="maxAge">Maximum acceptable age in years.</param>
    /// <returns>True if the age derived from the date of birth is within bounds.</returns>

    internal bool IsValidDateOfBirth(DateTime? dob, int minAge, int maxAge)
    {
        if (dob == null) return false;

        DateTime date;
        if (DateTime.TryParse(dob.ToString(), out date))
        {
            var diff = DateTime.UtcNow.Year - date.Year;
            if (diff > minAge && diff <= maxAge) return true;
        }

        return false;
    }

    /// <summary>
    /// Validates password complexity against the configured password policy. Supports either
    /// a custom regex pattern or individual rules for lowercase, uppercase, digits, special
    /// characters, and length constraints.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <param name="passwordConfig">The password policy configuration.</param>
    /// <param name="errorCodes">Output parameter containing the specific error code if validation fails.</param>
    /// <returns>True if the password meets all complexity requirements.</returns>

    internal bool IsComplexPassword(string password, PasswordConfig passwordConfig, out string errorCodes)
    {
        var input = password;
        errorCodes = string.Empty;
        try
        {
            if (!string.IsNullOrWhiteSpace(passwordConfig.PasswordPattern))
            {
                var pattern = new Regex(passwordConfig.PasswordPattern);
                if (pattern.IsMatch(input)) return true;

                errorCodes = ApiErrorCodes.PasswordPatternNotMatched;
                return false;
            }

            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasLowerChar = new Regex(@"[a-z]+");
            var hasMiniMaxChars = new Regex(@"^.{" + passwordConfig.MinPasswordLength + "," +
                                            passwordConfig.MaxPasswordLength + "}$");
            var hasSymbols = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");

            if (passwordConfig.RequireLowercase && !hasLowerChar.IsMatch(input))
            {
                errorCodes = ApiErrorCodes.PasswordRequiredLowerCase;
                return false;
            }

            if (passwordConfig.RequireUppercase && !hasUpperChar.IsMatch(input))
            {
                errorCodes = ApiErrorCodes.PasswordRequiredUpperCase;
                return false;
            }

            if (passwordConfig.MinPasswordLength > 0 && passwordConfig.MaxPasswordLength > 0 &&
                !hasMiniMaxChars.IsMatch(input))
            {
                errorCodes = ApiErrorCodes.InvalidPasswordLength;
                return false;
            }

            if (passwordConfig.RequireDigit && !hasNumber.IsMatch(input))
            {
                errorCodes = ApiErrorCodes.PasswordRequiredNumericValue;
                return false;
            }

            if (passwordConfig.RequireSpecialChar && !hasSymbols.IsMatch(input))
            {
                errorCodes = ApiErrorCodes.PasswordRequiredSpecialCharacters;
                return false;
            }

            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>
    /// Constructs a sign-in error response with the localized error message.
    /// </summary>

    private SignInResponseModel ConstructError(string errorCode, IResourceStringHandler resourceStringHandler)
    {
        var signinResponse = new SignInResponseModel();
        signinResponse.Succeeded = false;
        signinResponse.Message = resourceStringHandler.GetResourceString(errorCode);
        signinResponse.ErrorCode = errorCode;
        return signinResponse;
    }
}
