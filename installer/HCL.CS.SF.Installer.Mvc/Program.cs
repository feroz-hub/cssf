/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using HCLCSSFInstallerMVC.Application.Abstractions;
using HCLCSSFInstallerMVC.Application.Services;
using HCLCSSFInstallerMVC.Infrastructure.Configuration;
using HCLCSSFInstallerMVC.Infrastructure.Persistence;
using HCLCSSFInstallerMVC.Infrastructure.Services;
using HCLCSSFInstallerMVC.Middleware;
using HCLCSSFInstallerMVC.Services;
using HCLCSSFInstallerMVC.ViewModels.Validators;

// ========================================================================
// HCL.CS.SF Installer - Application entry point (top-level statements)
// Configures services and middleware for the multi-step installation wizard.
// ========================================================================

var builder = WebApplication.CreateBuilder(args);

// Detect reverse-proxy and containerized deployment environment variables
var trustProxyHeaders = string.Equals(
    Environment.GetEnvironmentVariable("HCL.CS.SF_TRUST_PROXY_HEADERS"),
    "true",
    StringComparison.OrdinalIgnoreCase);
var railwayPort = Environment.GetEnvironmentVariable("PORT");

// When deployed on Railway (or similar PaaS), bind to the assigned port
if (!string.IsNullOrWhiteSpace(railwayPort))
    builder.WebHost.UseUrls($"http://+:{railwayPort}");

// Configure structured logging with Serilog
builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// Bind configuration sections for lock file and database provisioning options
builder.Services.Configure<InstallerLockOptions>(builder.Configuration.GetSection("InstallerLock"));
builder.Services.Configure<DatabaseProvisioningOptions>(builder.Configuration);

// Optionally persist Data Protection keys to a file system path (useful in containers)
var dataProtectionKeysPath = Environment.GetEnvironmentVariable("HCL.CS.SF_INSTALLER_DATA_PROTECTION_KEYS_PATH");
var dataProtectionBuilder = builder.Services.AddDataProtection();
if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath))
{
    Directory.CreateDirectory(dataProtectionKeysPath);
    dataProtectionBuilder.PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));
}

// Trust X-Forwarded-For/Proto headers when behind a reverse proxy (e.g., Railway, nginx)
if (trustProxyHeaders)
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });
}

// In-memory distributed cache backs the session store for wizard state
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "HCLCSSFInstaller.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

// MVC with automatic anti-forgery token validation on all POST actions
builder.Services
    .AddControllersWithViews(options => { options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()); });
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// Register FluentValidation validators from the assembly containing the view model validators
builder.Services.AddValidatorsFromAssemblyContaining<SetupProviderViewModelValidator>();

// Health check that reports whether the installer lock marker exists
builder.Services.AddHealthChecks()
    .AddCheck<InstallationHealthCheck>("installation_state");
builder.Services.AddHttpContextAccessor();

// Register installer services using scoped lifetime (one instance per HTTP request)
builder.Services.AddScoped<IInstallerStateStore, ProtectedInstallerStateStore>();
builder.Services.AddScoped<IInstallationGateService, InstallationGateService>();
builder.Services.AddScoped<IDatabaseMigrationService, DatabaseMigrationService>();
builder.Services.AddScoped<ISeedDataService, SeedDataService>();
builder.Services.AddScoped<IInstallerService, InstallerService>();
builder.Services.AddScoped<IInstallerWorkflowService, InstallerWorkflowService>();

var app = builder.Build();

// Apply forwarded headers before any other middleware when behind a proxy
if (trustProxyHeaders)
    app.UseForwardedHeaders();

// Global exception handler: redirects HTML requests to /error, returns problem+json for API
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSerilogRequestLogging();

if (!app.Environment.IsDevelopment()) app.UseHsts();

app.UseHttpsRedirection();
app.UseStaticFiles();

// Setup redirect middleware: enforces that non-setup routes are blocked until installation completes
app.UseMiddleware<SetupRedirectMiddleware>();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
