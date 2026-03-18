/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Text.RegularExpressions;
using FluentValidation;

namespace HCLCSSFInstallerMVC.ViewModels.Validators;

/// <summary>
/// FluentValidation rules for the seed step view model.
/// Validates client name, URIs (must be HTTPS), grant/response type selections,
/// admin user credentials, and password complexity requirements.
/// </summary>
public sealed class SeedStepViewModelValidator : AbstractValidator<SeedStepViewModel>
{
    /// <summary>Regex pattern for validating phone numbers (US format with optional country code).</summary>
    private const string PhonePattern = @"^(\+\d{1,2}\s?)?1?\-?\.?\s?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$";

    /// <summary>
    /// Configures all validation rules for the seed step form fields.
    /// </summary>
    public SeedStepViewModelValidator()
    {
        RuleFor(model => model.ClientName)
            .NotEmpty()
            .WithMessage("Client name is required.")
            .Length(8, 254)
            .WithMessage("Client name must be between 8 and 254 characters.");

        RuleFor(model => model.ClientUri)
            .NotEmpty()
            .WithMessage("Client URI is required.")
            .Must(BeHttpsUrl)
            .WithMessage("Client URI must be a valid HTTPS URL.");

        RuleFor(model => model)
            .Must(HasAtLeastOneGrantType)
            .WithMessage("At least one supported grant type is required.");

        RuleFor(model => model)
            .Must(HasValidResponseTypeSelection)
            .WithMessage("code response type is required only when Authorization Code grant is enabled.");

        RuleFor(model => model.UseHybridGrant)
            .Equal(false)
            .WithMessage("Hybrid grant is not allowed.");

        RuleFor(model => model.UseIdTokenResponseType)
            .Equal(false)
            .WithMessage("id_token response type is not allowed.");

        RuleFor(model => model.UseTokenResponseType)
            .Equal(false)
            .WithMessage("token response type is not allowed.");

        RuleFor(model => model.AllowedScopes)
            .NotEmpty()
            .When(model => !model.UseDefaultScopes)
            .WithMessage("Allowed scopes are required when default scopes are disabled.");

        RuleFor(model => model.RedirectUris)
            .NotEmpty()
            .When(model => model.UseAuthorizationCodeGrant)
            .WithMessage("At least one redirect URI is required.")
            .Must(AllLinesContainValidUrls)
            .WithMessage("Each redirect URI must be a valid HTTPS URL.");

        RuleFor(model => model.PostLogoutRedirectUris)
            .Must(AllLinesContainValidUrls)
            .WithMessage("Each post logout redirect URI must be a valid HTTPS URL.");

        RuleFor(model => model.FrontChannelLogoutUri)
            .Must(BeOptionalHttpsUrl)
            .WithMessage("Front channel logout URI must be a valid HTTPS URL.");

        RuleFor(model => model.BackChannelLogoutUri)
            .Must(BeOptionalHttpsUrl)
            .WithMessage("Back channel logout URI must be a valid HTTPS URL.");

        RuleFor(model => model.UserName)
            .NotEmpty()
            .WithMessage("Username is required.")
            .Length(6, 254)
            .WithMessage("Username must be between 6 and 254 characters.");

        RuleFor(model => model.FirstName)
            .NotEmpty()
            .WithMessage("First name is required.")
            .Length(2, 254)
            .WithMessage("First name must be between 2 and 254 characters.");

        RuleFor(model => model.LastName)
            .Length(2, 254)
            .When(model => !string.IsNullOrWhiteSpace(model.LastName))
            .WithMessage("Last name must be between 2 and 254 characters when provided.");

        RuleFor(model => model.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email format is invalid.");

        RuleFor(model => model.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required.")
            .Matches(PhonePattern)
            .WithMessage("Phone number format is invalid.");

        RuleFor(model => model.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .Length(8, 250)
            .WithMessage("Password must be between 8 and 250 characters.")
            .Must(MeetsPasswordComplexity)
            .WithMessage("Password complexity is not met. Include at least one uppercase letter and one number.");

        RuleFor(model => model.ConfirmPassword)
            .Equal(model => model.Password)
            .WithMessage("Password and confirm password do not match.");

        RuleFor(model => model.IdentityProvider)
            .NotEmpty()
            .WithMessage("Identity provider type is required.")
            .Must(provider => provider.Equals("Local", StringComparison.OrdinalIgnoreCase) ||
                              provider.Equals("Ldap", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Identity provider must be either Local or Ldap.");
    }

    /// <summary>Ensures at least one OAuth grant type checkbox is selected.</summary>
    private static bool HasAtLeastOneGrantType(SeedStepViewModel model)
    {
        return model.UseAuthorizationCodeGrant
               || model.UseClientCredentialsGrant
               || model.UseRefreshTokenGrant
               || model.UsePasswordGrant;
    }

    /// <summary>
    /// Validates that response type selection is consistent with grant type selection:
    /// "code" is required with authorization_code grant, and id_token/token are blocked.
    /// </summary>
    private static bool HasValidResponseTypeSelection(SeedStepViewModel model)
    {
        if (model.UseIdTokenResponseType || model.UseTokenResponseType)
            return false;

        if (model.UseAuthorizationCodeGrant)
            return model.UseCodeResponseType;

        return !model.UseCodeResponseType;
    }

    /// <summary>Checks that the password contains at least one digit, one uppercase letter, and is at least 8 characters.</summary>
    private static bool MeetsPasswordComplexity(string password)
    {
        return Regex.IsMatch(password, "[0-9]")
               && Regex.IsMatch(password, "[A-Z]")
               && password.Length >= 8;
    }

    /// <summary>Validates that every non-empty line in a multi-line string is a valid HTTPS URL.</summary>
    private static bool AllLinesContainValidUrls(string values)
    {
        if (string.IsNullOrWhiteSpace(values)) return true;

        var lines = values
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase);

        return lines.All(BeHttpsUrl);
    }

    /// <summary>Validates that the URL is either empty/null or a valid HTTPS URL.</summary>
    private static bool BeOptionalHttpsUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;

        return BeHttpsUrl(url);
    }

    /// <summary>Validates that the URL is an absolute HTTPS URL without wildcards.</summary>
    private static bool BeHttpsUrl(string url)
    {
        if (url.Contains('*', StringComparison.Ordinal)) return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var validatedUri)
               && validatedUri.Scheme == Uri.UriSchemeHttps
               && !string.IsNullOrWhiteSpace(validatedUri.Host);
    }
}
