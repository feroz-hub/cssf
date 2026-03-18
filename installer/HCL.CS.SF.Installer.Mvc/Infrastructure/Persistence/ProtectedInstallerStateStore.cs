/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using HCLCSSFInstallerMVC.Application.Abstractions;
using HCLCSSFInstallerMVC.Application.DTOs;

namespace HCLCSSFInstallerMVC.Infrastructure.Persistence;

/// <summary>
/// Stores the installer session state in the HTTP session, encrypted with ASP.NET Core Data Protection.
/// This prevents tampering with wizard progress and protects connection strings in transit.
/// </summary>
public sealed class ProtectedInstallerStateStore : IInstallerStateStore
{
    private const string SessionKey = "HCL.CS.SF-installer-state";
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly IDataProtector _protector;

    /// <summary>
    /// Initializes the store with Data Protection for encrypting session payloads.
    /// </summary>
    public ProtectedInstallerStateStore(IHttpContextAccessor httpContextAccessor,
        IDataProtectionProvider dataProtectionProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        // Create a purpose-specific protector to isolate installer state encryption
        _protector = dataProtectionProvider.CreateProtector("HCLCSSFInstallerMVC.SessionState.v1");
        _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    /// <inheritdoc />
    public async Task<InstallerSessionState> GetAsync(CancellationToken cancellationToken)
    {
        var session = GetSession();
        await session.LoadAsync(cancellationToken);

        var protectedPayload = session.GetString(SessionKey);
        if (string.IsNullOrWhiteSpace(protectedPayload)) return new InstallerSessionState();

        try
        {
            var json = _protector.Unprotect(protectedPayload);
            return JsonSerializer.Deserialize<InstallerSessionState>(json, _jsonSerializerOptions) ??
                   new InstallerSessionState();
        }
        catch (CryptographicException)
        {
            await ClearAsync(cancellationToken);
            return new InstallerSessionState();
        }
    }

    /// <inheritdoc />
    public async Task SaveAsync(InstallerSessionState state, CancellationToken cancellationToken)
    {
        var session = GetSession();
        await session.LoadAsync(cancellationToken);

        var json = JsonSerializer.Serialize(state, _jsonSerializerOptions);
        var protectedPayload = _protector.Protect(json);

        session.SetString(SessionKey, protectedPayload);
        await session.CommitAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task ClearAsync(CancellationToken cancellationToken)
    {
        var session = GetSession();
        await session.LoadAsync(cancellationToken);
        session.Remove(SessionKey);
        await session.CommitAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves the current HTTP session, throwing if no HTTP context is available.
    /// </summary>
    private ISession GetSession()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null)
            throw new InvalidOperationException("No active HTTP context is available for installer state storage.");

        return context.Session;
    }
}
