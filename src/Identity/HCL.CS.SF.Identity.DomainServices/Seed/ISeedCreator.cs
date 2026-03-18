/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain;

namespace HCL.CS.SF.DomainServices.Seed;

/// <summary>
/// Creates master/seed data required for a fresh installation of the identity system
/// (e.g., default roles, admin user, initial API resources and scopes).
/// Called by the installer during first-time setup.
/// </summary>
public interface ISeedCreator
{
    /// <summary>
    /// Executes all registered seed models to populate the database with initial master data.
    /// </summary>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure of the seeding process.</returns>
    Task<FrameworkResult> CreateMasterDataAsync();

    /// <summary>
    /// Registers a seed model to be executed during <see cref="CreateMasterDataAsync"/>.
    /// </summary>
    /// <param name="orderNumber">The execution order for this seed model (lower numbers run first).</param>
    /// <param name="model">The seed data model to register.</param>
    /// <returns><c>true</c> if the model was successfully registered.</returns>
    Task<bool> AddSeedModelsAsync(int orderNumber, BaseModel model);
}
