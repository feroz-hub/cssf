/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Models.Endpoint;

namespace HCL.CS.SF.DomainServices.Infra;

/// <summary>
/// In-memory store for asymmetric cryptographic keys used for JWT signing and validation.
/// Implementations should cache loaded keys and return them indexed by key identifier (kid).
/// </summary>
public interface IKeyStore
{
    /// <summary>
    /// Adds the provided security keys to the store and returns the complete set of stored keys.
    /// </summary>
    /// <param name="securityKeys">The asymmetric key information models to add.</param>
    /// <returns>A dictionary of all stored keys, keyed by their key identifier (kid).</returns>
    Dictionary<string, AsymmetricKeyInfoModel> Add(IEnumerable<AsymmetricKeyInfoModel> securityKeys);
}
