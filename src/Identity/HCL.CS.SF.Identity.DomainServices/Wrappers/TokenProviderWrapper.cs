/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HCL.CS.SF.DomainServices.Wrappers;

/// <summary>
/// Custom data-protection-based token provider for email confirmation tokens.
/// Allows independent configuration of token lifetime via <see cref="EmailConfirmationTokenProviderOptions"/>.
/// </summary>
/// <typeparam name="TUser">The type representing a user in the identity system.</typeparam>
public class EmailConfirmationTokenProvider<TUser> : DataProtectorTokenProvider<TUser>
    where TUser : class
{
    /// <summary>
    /// Initializes a new instance of <see cref="EmailConfirmationTokenProvider{TUser}"/>.
    /// </summary>
    public EmailConfirmationTokenProvider(
        IDataProtectionProvider dataProtectionProvider,
        IOptions<EmailConfirmationTokenProviderOptions> options,
        ILogger<DataProtectorTokenProvider<TUser>> logger)
        : base(dataProtectionProvider, options, logger)
    {
    }
}

/// <summary>
/// Configuration options for the <see cref="EmailConfirmationTokenProvider{TUser}"/>,
/// allowing customization of token lifetime and data protection purpose strings.
/// </summary>
public class EmailConfirmationTokenProviderOptions : DataProtectionTokenProviderOptions
{
}

/// <summary>
/// Custom data-protection-based token provider for phone number change verification tokens.
/// Allows independent configuration of token lifetime via <see cref="ChangePhoneNumberTokenProviderOption"/>.
/// </summary>
/// <typeparam name="TUser">The type representing a user in the identity system.</typeparam>
public class ChangePhoneNumberTokenProvider<TUser> : DataProtectorTokenProvider<TUser>
    where TUser : class
{
    /// <summary>
    /// Initializes a new instance of <see cref="ChangePhoneNumberTokenProvider{TUser}"/>.
    /// </summary>
    public ChangePhoneNumberTokenProvider(
        IDataProtectionProvider dataProtectionProvider,
        IOptions<ChangePhoneNumberTokenProviderOption> options,
        ILogger<DataProtectorTokenProvider<TUser>> logger)
        : base(dataProtectionProvider, options, logger)
    {
    }
}

/// <summary>
/// Configuration options for the <see cref="ChangePhoneNumberTokenProvider{TUser}"/>,
/// allowing customization of token lifetime and data protection purpose strings.
/// </summary>
public class ChangePhoneNumberTokenProviderOption : DataProtectionTokenProviderOptions
{
}

/// <summary>
/// Custom data-protection-based token provider for password reset tokens.
/// Allows independent configuration of token lifetime via <see cref="PasswordResetTokenProviderOptions"/>.
/// </summary>
/// <typeparam name="TUser">The type representing a user in the identity system.</typeparam>
public class PasswordResetTokenProvider<TUser> : DataProtectorTokenProvider<TUser>
    where TUser : class
{
    /// <summary>
    /// Initializes a new instance of <see cref="PasswordResetTokenProvider{TUser}"/>.
    /// </summary>
    public PasswordResetTokenProvider(
        IDataProtectionProvider dataProtectionProvider,
        IOptions<PasswordResetTokenProviderOptions> options,
        ILogger<DataProtectorTokenProvider<TUser>> logger)
        : base(dataProtectionProvider, options, logger)
    {
    }
}

/// <summary>
/// Configuration options for the <see cref="PasswordResetTokenProvider{TUser}"/>,
/// allowing customization of token lifetime and data protection purpose strings.
/// </summary>
public class PasswordResetTokenProviderOptions : DataProtectionTokenProviderOptions
{
}

/// <summary>
/// Custom data-protection-based token provider for general-purpose user tokens
/// (e.g., account unlock, custom verification flows).
/// Allows independent configuration via <see cref="UserTokenProviderOptions"/>.
/// </summary>
/// <typeparam name="TUser">The type representing a user in the identity system.</typeparam>
public class UserTokenProvider<TUser> : DataProtectorTokenProvider<TUser>
    where TUser : class
{
    /// <summary>
    /// Initializes a new instance of <see cref="UserTokenProvider{TUser}"/>.
    /// </summary>
    public UserTokenProvider(
        IDataProtectionProvider dataProtectionProvider,
        IOptions<UserTokenProviderOptions> options,
        ILogger<DataProtectorTokenProvider<TUser>> logger)
        : base(dataProtectionProvider, options, logger)
    {
    }
}

/// <summary>
/// Configuration options for the <see cref="UserTokenProvider{TUser}"/>,
/// allowing customization of token lifetime and data protection purpose strings.
/// </summary>
public class UserTokenProviderOptions : DataProtectionTokenProviderOptions
{
}
