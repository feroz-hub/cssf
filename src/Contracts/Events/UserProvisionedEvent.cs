/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Contracts.Events;

/// <summary>
/// Domain event raised when a new user has been provisioned in the identity system.
/// Consumers can subscribe to this event to trigger downstream workflows such as
/// sending welcome notifications or syncing user data to external systems.
/// </summary>
/// <param name="UserId">The unique identifier of the newly provisioned user.</param>
/// <param name="UserName">The username assigned to the new user.</param>
/// <param name="OccurredAtUtc">The UTC timestamp when the provisioning occurred.</param>
public sealed record UserProvisionedEvent(Guid UserId, string UserName, DateTimeOffset OccurredAtUtc);
