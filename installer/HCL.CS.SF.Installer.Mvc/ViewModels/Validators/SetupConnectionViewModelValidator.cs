/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using FluentValidation;

namespace HCLCSSFInstallerMVC.ViewModels.Validators;

/// <summary>
/// FluentValidation rules for wizard step 2 (Connection String Input).
/// Ensures a provider is selected and a non-empty connection string is supplied.
/// </summary>
public sealed class SetupConnectionViewModelValidator : AbstractValidator<SetupConnectionViewModel>
{
    /// <summary>Configures validation rules for provider and connection string fields.</summary>
    public SetupConnectionViewModelValidator()
    {
        RuleFor(model => model.Provider)
            .NotNull()
            .WithMessage("Database provider is required.");

        RuleFor(model => model.ConnectionString)
            .NotEmpty()
            .WithMessage("Connection string is required.")
            .MaximumLength(2048);
    }
}
