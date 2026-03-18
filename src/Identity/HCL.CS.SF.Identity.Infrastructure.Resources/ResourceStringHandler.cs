/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using HCL.CS.SF.DomainServices.Infra;

namespace HCL.CS.SF.Infrastructure.Resources;

/// <summary>
/// Provides culture-aware access to validation message resource strings.
/// Wraps a <see cref="ResourceManager"/> backed by the <c>ValidationMessages.resx</c> resource file
/// and supports both forward lookup (key to value) and reverse lookup (value to key).
/// </summary>
internal class ResourceStringHandler : IResourceStringHandler
{
    /// <summary>
    /// Backing field for the lazily initialised <see cref="ResourceManager"/>.
    /// </summary>
    private ResourceManager resourceMan;

    /// <summary>
    /// Gets the <see cref="ResourceManager"/> instance used to retrieve validation message strings.
    /// Lazily initialised on first access, targeting the <c>ValidationMessages</c> embedded resource.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public ResourceManager ResourceManager
    {
        get
        {
            if (ReferenceEquals(resourceMan, null))
            {
                var temp = new ResourceManager(
                    "HCL.CS.SF.Infrastructure.Resources.ValidationMessages",
                    typeof(ResourceStringHandler).Assembly);
                resourceMan = temp;
            }

            return resourceMan;
        }
    }

    /// <summary>
    /// Gets or sets the culture used for resource string lookups. Defaults to <see cref="CultureInfo.CurrentCulture"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;

    /// <summary>
    /// Retrieves a resource string by its identifier key.
    /// </summary>
    /// <param name="id">The resource key to look up (e.g., "USR4001").</param>
    /// <param name="skipError">
    /// When <c>false</c> (default), an exception is thrown if the key is not found.
    /// When <c>true</c>, returns <c>null</c> for missing keys instead of throwing.
    /// </param>
    /// <returns>The localised resource string, or <c>null</c> if <paramref name="skipError"/> is <c>true</c> and the key is missing.</returns>
    /// <exception cref="Exception">Thrown when the key is not found and <paramref name="skipError"/> is <c>false</c>.</exception>
    public string GetResourceString(string id, bool skipError = false)
    {
        var result = ResourceManager.GetString(id, Culture);
        if (result == null && !skipError) throw new Exception("Missing validation message for ID : " + id);

        return result;
    }

    /// <summary>
    /// Performs a reverse lookup to find the resource key associated with a given string value.
    /// Iterates through all entries in the current culture's resource set to find a match.
    /// </summary>
    /// <param name="value">The resource string value to search for.</param>
    /// <returns>The resource key whose value matches <paramref name="value"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the resource set for the current culture cannot be loaded.</exception>
    public string GetResourceKeyByValue(string value)
    {
        // Load the full resource set and scan entries for a matching value
        var entry = (ResourceManager.GetResourceSet(Culture, true, true) ?? throw new InvalidOperationException())
            .OfType<DictionaryEntry>()
            .FirstOrDefault(e => e.Value != null && e.Value.ToString() == value);

        var key = entry.Key.ToString();
        return key;
    }
}
