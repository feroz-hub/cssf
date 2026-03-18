/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.Mvc;
using HCLCSSFInstallerMVC.Application.Exceptions;
using HCLCSSFInstallerMVC.Models;
using HCLCSSFInstallerMVC.Services;
using HCLCSSFInstallerMVC.ViewModels;

namespace HCLCSSFInstallerMVC.Controllers;

/// <summary>
/// MVC controller that drives the multi-step installer wizard UI.
/// Each action maps to a wizard step: Provider (1) -> Connection (2) -> Validate (3) -> Migrate (4) -> Seed (5) -> Complete (6).
/// Every action guards against re-installation by checking the installation gate.
/// </summary>
[Route("setup")]
public sealed class SetupController : Controller
{
    private readonly IInstallerWorkflowService _workflowService;

    /// <summary>
    /// Initializes the controller with the workflow service that bridges UI and domain logic.
    /// </summary>
    public SetupController(IInstallerWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    /// <summary>
    /// Root entry point that redirects to the appropriate wizard step or the installed page.
    /// </summary>
    [HttpGet("/")]
    public async Task<IActionResult> Root(CancellationToken cancellationToken)
    {
        if (await _workflowService.IsInstallationCompletedAsync(cancellationToken))
            return RedirectToAction(nameof(Installed));

        return RedirectToAction(nameof(Provider));
    }

    /// <summary>
    /// Step 1: Displays the database provider selection page.
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> Provider(CancellationToken cancellationToken)
    {
        if (await _workflowService.IsInstallationCompletedAsync(cancellationToken))
            return RedirectToAction(nameof(Installed));

        ViewData["CurrentStep"] = 1;
        var model = await _workflowService.GetProviderSelectionAsync(cancellationToken);
        return View(model);
    }

    /// <summary>
    /// Step 1 POST: Saves the selected database provider and advances to the connection step.
    /// </summary>
    [HttpPost("provider")]
    public async Task<IActionResult> SaveProvider(
        [Bind(nameof(SetupProviderViewModel.Provider))] SetupProviderViewModel model,
        CancellationToken cancellationToken)
    {
        if (await _workflowService.IsInstallationCompletedAsync(cancellationToken))
            return RedirectToAction(nameof(Installed));

        ViewData["CurrentStep"] = 1;
        if (!ModelState.IsValid) return View("Provider", model);

        try
        {
            await _workflowService.SaveProviderSelectionAsync(model, cancellationToken);
            return RedirectToAction(nameof(Connection));
        }
        catch (InstallationAlreadyCompletedException)
        {
            return RedirectToAction(nameof(Installed));
        }
    }

    /// <summary>
    /// Step 2: Displays the connection string input page for the chosen provider.
    /// </summary>
    [HttpGet("connection")]
    public async Task<IActionResult> Connection(CancellationToken cancellationToken)
    {
        if (await _workflowService.IsInstallationCompletedAsync(cancellationToken))
            return RedirectToAction(nameof(Installed));

        ViewData["CurrentStep"] = 2;
        var model = await _workflowService.GetConnectionConfigurationAsync(cancellationToken);
        if (model.Provider is null) return RedirectToAction(nameof(Provider));

        return View(model);
    }

    /// <summary>
    /// Step 2 POST: Saves the connection string and advances to the validation step.
    /// </summary>
    [HttpPost("connection")]
    public async Task<IActionResult> SaveConnection(
        [Bind(nameof(SetupConnectionViewModel.Provider), nameof(SetupConnectionViewModel.ConnectionString))]
        SetupConnectionViewModel model,
        CancellationToken cancellationToken)
    {
        if (await _workflowService.IsInstallationCompletedAsync(cancellationToken))
            return RedirectToAction(nameof(Installed));

        ViewData["CurrentStep"] = 2;
        if (!ModelState.IsValid) return View("Connection", model);

        try
        {
            await _workflowService.SaveConnectionConfigurationAsync(model, cancellationToken);
            return RedirectToAction(nameof(Validate));
        }
        catch (InstallationAlreadyCompletedException)
        {
            return RedirectToAction(nameof(Installed));
        }
    }

    /// <summary>
    /// Step 3: Displays the connection validation page with a button to test connectivity.
    /// </summary>
    [HttpGet("validate")]
    public async Task<IActionResult> Validate(CancellationToken cancellationToken)
    {
        if (await _workflowService.IsInstallationCompletedAsync(cancellationToken))
            return RedirectToAction(nameof(Installed));

        ViewData["CurrentStep"] = 3;
        var model = await _workflowService.GetConnectionValidationAsync(cancellationToken);
        if (!model.HasConfiguration || string.IsNullOrWhiteSpace(model.ConnectionString))
            return RedirectToAction(nameof(Connection));

        return View(model);
    }

    /// <summary>
    /// Step 3 POST: Executes the database connection test and advances to migration on success.
    /// </summary>
    [HttpPost("validate")]
    public async Task<IActionResult> ExecuteValidation(CancellationToken cancellationToken)
    {
        if (await _workflowService.IsInstallationCompletedAsync(cancellationToken))
            return RedirectToAction(nameof(Installed));

        ViewData["CurrentStep"] = 3;

        try
        {
            var model = await _workflowService.ValidateConnectionAsync(cancellationToken);
            if (model.IsSuccessful) return RedirectToAction(nameof(Migrate));

            return View("Validate", model);
        }
        catch (InstallationAlreadyCompletedException)
        {
            return RedirectToAction(nameof(Installed));
        }
    }

    /// <summary>
    /// Step 4: Displays the migration execution page.
    /// </summary>
    [HttpGet("migrate")]
    public async Task<IActionResult> Migrate(CancellationToken cancellationToken)
    {
        if (await _workflowService.IsInstallationCompletedAsync(cancellationToken))
            return RedirectToAction(nameof(Installed));

        ViewData["CurrentStep"] = 4;
        var model = await _workflowService.GetMigrationViewModelAsync(cancellationToken);
        if (!model.CanRun) return RedirectToAction(nameof(Validate));

        return View(model);
    }

    /// <summary>
    /// Step 4 POST: Applies all EF Core migrations and post-migration schema patches.
    /// </summary>
    [HttpPost("migrate")]
    public async Task<IActionResult> RunMigrations(CancellationToken cancellationToken)
    {
        if (await _workflowService.IsInstallationCompletedAsync(cancellationToken))
            return RedirectToAction(nameof(Installed));

        ViewData["CurrentStep"] = 4;

        try
        {
            var model = await _workflowService.RunMigrationAsync(cancellationToken);
            if (model.IsCompleted) return RedirectToAction(nameof(Seed));

            return View("Migrate", model);
        }
        catch (InstallerWorkflowException workflowException)
        {
            ModelState.AddModelError(string.Empty, workflowException.Message);
            var model = await _workflowService.GetMigrationViewModelAsync(cancellationToken);
            return View("Migrate", model);
        }
        catch (InstallationAlreadyCompletedException)
        {
            return RedirectToAction(nameof(Installed));
        }
    }

    /// <summary>
    /// Step 5 GET: Displays the seed data form (admin user and OAuth client configuration).
    /// </summary>
    [HttpGet("seed")]
    public async Task<IActionResult> Seed(CancellationToken cancellationToken)
    {
        if (await _workflowService.IsInstallationCompletedAsync(cancellationToken))
            return RedirectToAction(nameof(Installed));

        ViewData["CurrentStep"] = 5;
        var migrationState = await _workflowService.GetMigrationViewModelAsync(cancellationToken);
        if (!migrationState.IsCompleted) return RedirectToAction(nameof(Migrate));

        var model = await _workflowService.GetSeedViewModelAsync(cancellationToken);
        return View(model);
    }

    /// <summary>
    /// Step 5 POST: Executes database seeding with the submitted admin user and client details.
    /// </summary>
    [HttpPost("seed")]
    public async Task<IActionResult> Seed(
        [Bind(
            nameof(SeedStepViewModel.ClientName),
            nameof(SeedStepViewModel.ClientUri),
            nameof(SeedStepViewModel.UseAuthorizationCodeGrant),
            nameof(SeedStepViewModel.UseClientCredentialsGrant),
            nameof(SeedStepViewModel.UseRefreshTokenGrant),
            nameof(SeedStepViewModel.UsePasswordGrant),
            nameof(SeedStepViewModel.UseCodeResponseType),
            nameof(SeedStepViewModel.UseDefaultScopes),
            nameof(SeedStepViewModel.AllowedScopes),
            nameof(SeedStepViewModel.RedirectUris),
            nameof(SeedStepViewModel.PostLogoutRedirectUris),
            nameof(SeedStepViewModel.FrontChannelLogoutUri),
            nameof(SeedStepViewModel.BackChannelLogoutUri),
            nameof(SeedStepViewModel.UserName),
            nameof(SeedStepViewModel.Password),
            nameof(SeedStepViewModel.ConfirmPassword),
            nameof(SeedStepViewModel.FirstName),
            nameof(SeedStepViewModel.LastName),
            nameof(SeedStepViewModel.Email),
            nameof(SeedStepViewModel.PhoneNumber),
            nameof(SeedStepViewModel.IdentityProvider))]
        SeedStepViewModel model,
        CancellationToken cancellationToken)
    {
        if (await _workflowService.IsInstallationCompletedAsync(cancellationToken))
            return RedirectToAction(nameof(Installed));

        ViewData["CurrentStep"] = 5;
        if (!ModelState.IsValid) return View(model);

        try
        {
            var result = await _workflowService.ExecuteSeedAsync(model, cancellationToken);
            if (result.IsCompleted) return RedirectToAction(nameof(Complete));

            return View(result);
        }
        catch (InstallerWorkflowException workflowException)
        {
            ModelState.AddModelError(string.Empty, workflowException.Message);
            return View(model);
        }
        catch (InstallationAlreadyCompletedException)
        {
            return RedirectToAction(nameof(Installed));
        }
    }

    /// <summary>
    /// Allows the user to skip seeding and mark installation as completed without initial data.
    /// </summary>
    [HttpPost("skip-seed")]
    public async Task<IActionResult> SkipSeed(CancellationToken cancellationToken)
    {
        if (await _workflowService.IsInstallationCompletedAsync(cancellationToken))
            return RedirectToAction(nameof(Installed));

        try
        {
            await _workflowService.SkipSeedAsync(cancellationToken);
            return RedirectToAction(nameof(Complete));
        }
        catch (InstallationAlreadyCompletedException)
        {
            return RedirectToAction(nameof(Installed));
        }
    }

    /// <summary>
    /// Step 6: Displays the installation completion summary with generated credentials.
    /// </summary>
    [HttpGet("/complete")]
    public async Task<IActionResult> Complete(CancellationToken cancellationToken)
    {
        ViewData["CurrentStep"] = 6;
        var model = await _workflowService.GetCompletionViewModelAsync(cancellationToken);
        return View(model);
    }

    /// <summary>
    /// Landing page shown when setup has already been completed and the lock marker exists.
    /// </summary>
    [HttpGet("/installed")]
    public async Task<IActionResult> Installed(CancellationToken cancellationToken)
    {
        ViewData["CurrentStep"] = 6;
        var model = await _workflowService.GetCompletionViewModelAsync(cancellationToken);
        model.AlreadyInstalled = true;
        model.Message = "Setup is already completed for this environment.";
        return View("Complete", model);
    }

    /// <summary>
    /// Displays a generic error page with the current trace identifier for diagnostics.
    /// </summary>
    [HttpGet("/error")]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
    }
}
