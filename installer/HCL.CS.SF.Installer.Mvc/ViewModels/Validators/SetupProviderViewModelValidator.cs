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
/// FluentValidation rules for wizard step 1 (Provider Selection).
/// Ensures a database provider is selected.
/// </summary>
public sealed class SetupProviderViewModelValidator : AbstractValidator<SetupProviderViewModel>
{
    /// <summary>Configures the validation rule requiring a non-null provider selection.</summary>
    public SetupProviderViewModelValidator()
    {
        RuleFor(model => model.Provider)
            .NotNull()
            .WithMessage("Database provider is required.");
    }
}
