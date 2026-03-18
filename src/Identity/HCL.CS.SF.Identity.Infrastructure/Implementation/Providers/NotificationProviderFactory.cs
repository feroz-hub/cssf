/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.DomainServices.Infra;

namespace HCL.CS.SF.Infrastructure.Services.Implementation.Providers;

/// <summary>
/// Factory that resolves email and SMS notification providers by name.
/// All registered <see cref="IEmailProvider"/> and <see cref="ISmsProvider"/> implementations
/// are injected via DI and indexed by their <c>ProviderName</c> for O(1) lookup.
/// </summary>
public class NotificationProviderFactory
{
    /// <summary>
    /// Case-insensitive dictionary of registered email providers keyed by provider name.
    /// </summary>
    private readonly Dictionary<string, IEmailProvider> _emailProviders;

    /// <summary>
    /// Case-insensitive dictionary of registered SMS providers keyed by provider name.
    /// </summary>
    private readonly Dictionary<string, ISmsProvider> _smsProviders;

    /// <summary>
    /// Initializes the factory by indexing all injected providers by their provider name.
    /// </summary>
    /// <param name="emailProviders">All registered email provider implementations.</param>
    /// <param name="smsProviders">All registered SMS provider implementations.</param>
    public NotificationProviderFactory(
        IEnumerable<IEmailProvider> emailProviders,
        IEnumerable<ISmsProvider> smsProviders)
    {
        // Build case-insensitive lookup dictionaries from the injected provider collections
        _emailProviders = emailProviders.ToDictionary(p => p.ProviderName, StringComparer.OrdinalIgnoreCase);
        _smsProviders = smsProviders.ToDictionary(p => p.ProviderName, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Retrieves a registered email provider by its name.
    /// </summary>
    /// <param name="providerName">The provider name (e.g., "SendGrid", "Smtp", "AmazonSes").</param>
    /// <returns>The matching <see cref="IEmailProvider"/> implementation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no provider is registered with the given name.</exception>
    public IEmailProvider GetEmailProvider(string providerName)
    {
        if (_emailProviders.TryGetValue(providerName, out var provider))
            return provider;

        throw new InvalidOperationException($"Email provider '{providerName}' is not registered.");
    }

    /// <summary>
    /// Retrieves a registered SMS provider by its name.
    /// </summary>
    /// <param name="providerName">The provider name (e.g., "Twilio", "Vonage", "AmazonSns").</param>
    /// <returns>The matching <see cref="ISmsProvider"/> implementation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no provider is registered with the given name.</exception>
    public ISmsProvider GetSmsProvider(string providerName)
    {
        if (_smsProviders.TryGetValue(providerName, out var provider))
            return provider;

        throw new InvalidOperationException($"SMS provider '{providerName}' is not registered.");
    }

    /// <summary>
    /// Returns the names of all registered email providers.
    /// </summary>
    /// <returns>A read-only list of email provider names.</returns>
    public IReadOnlyList<string> GetEmailProviderNames() => _emailProviders.Keys.ToList();

    /// <summary>
    /// Returns the names of all registered SMS providers.
    /// </summary>
    /// <returns>A read-only list of SMS provider names.</returns>
    public IReadOnlyList<string> GetSmsProviderNames() => _smsProviders.Keys.ToList();
}
