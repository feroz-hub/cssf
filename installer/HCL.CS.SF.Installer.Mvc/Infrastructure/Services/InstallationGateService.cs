/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Text.Json;
using Microsoft.Extensions.Options;
using HCLCSSFInstallerMVC.Application.Abstractions;
using HCLCSSFInstallerMVC.Application.DTOs;
using HCLCSSFInstallerMVC.Infrastructure.Configuration;

namespace HCLCSSFInstallerMVC.Infrastructure.Services;

/// <summary>
/// File-based implementation of <see cref="IInstallationGateService"/>. Uses a JSON lock marker file
/// to track whether installation has completed. Once the marker exists, all further installer
/// operations are blocked.
/// </summary>
public sealed class InstallationGateService : IInstallationGateService
{
    private readonly IWebHostEnvironment _environment;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly InstallerLockOptions _options;

    /// <summary>
    /// Initializes the gate service with the host environment (for resolving relative paths)
    /// and lock file configuration.
    /// </summary>
    public InstallationGateService(IWebHostEnvironment environment, IOptions<InstallerLockOptions> options)
    {
        _environment = environment;
        _options = options.Value;
        _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };
    }

    /// <inheritdoc />
    public Task<bool> IsInstallationCompletedAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(File.Exists(GetMarkerPath()));
    }

    /// <inheritdoc />
    public async Task<InstallationCompletionMetadataDto?> GetCompletionMetadataAsync(
        CancellationToken cancellationToken)
    {
        var markerPath = GetMarkerPath();
        if (!File.Exists(markerPath)) return null;

        await using var stream = File.OpenRead(markerPath);
        return await JsonSerializer.DeserializeAsync<InstallationCompletionMetadataDto>(stream, _jsonSerializerOptions,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task MarkInstallationCompletedAsync(InstallationCompletionMetadataDto metadata,
        CancellationToken cancellationToken)
    {
        var markerPath = GetMarkerPath();
        var directoryPath = Path.GetDirectoryName(markerPath);
        if (!string.IsNullOrWhiteSpace(directoryPath)) Directory.CreateDirectory(directoryPath);

        await using var stream = File.Create(markerPath);
        await JsonSerializer.SerializeAsync(stream, metadata, _jsonSerializerOptions, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }

    /// <summary>
    /// Resolves the marker file path, combining it with the content root if relative.
    /// </summary>
    private string GetMarkerPath()
    {
        return Path.IsPathRooted(_options.MarkerFilePath)
            ? _options.MarkerFilePath
            : Path.Combine(_environment.ContentRootPath, _options.MarkerFilePath);
    }
}
