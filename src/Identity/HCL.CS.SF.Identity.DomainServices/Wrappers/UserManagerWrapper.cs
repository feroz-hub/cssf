/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Google.Authenticator;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.DomainServices.Repository.Api;

namespace HCL.CS.SF.DomainServices.Wrappers;

/// <summary>
/// Custom wrapper around ASP.NET Core Identity's <see cref="UserManager{TUser}"/> that extends
/// it with TOTP-based authenticator app support (setup code generation and token verification)
/// by leveraging the Google Authenticator library and the <see cref="IUserTokenRepository"/>.
/// </summary>
/// <typeparam name="TUser">The type representing a user in the identity system.</typeparam>
public class UserManagerWrapper<TUser> : UserManager<TUser>
    where TUser : class
{
    /// <summary>The internal login provider name used by ASP.NET Identity for stored tokens.</summary>
    private const string InternalLoginProvider = "[AspNetUserStore]";

    /// <summary>The token name under which the authenticator key is persisted.</summary>
    private const string AuthenticatorKeyTokenName = "AuthenticatorKey";

    /// <summary>Repository for reading user token records directly from the database.</summary>
    private readonly IUserTokenRepository userTokenRepository;

    /// <summary>
    /// Initializes a new instance of <see cref="UserManagerWrapper{TUser}"/> with all required
    /// Identity dependencies plus the custom <see cref="IUserTokenRepository"/>.
    /// </summary>
    public UserManagerWrapper(
        IUserStore<TUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<TUser> passwordHasher,
        IEnumerable<IUserValidator<TUser>> userValidators,
        IEnumerable<IPasswordValidator<TUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManagerWrapper<TUser>> logger,
        IUserTokenRepository userTokenRepository)
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors,
            services, logger)
    {
        this.userTokenRepository = userTokenRepository;
    }

    /// <summary>
    /// Generates a TOTP authenticator setup code (including QR-code data) for the given user.
    /// </summary>
    /// <param name="applicationName">The display name of the application shown in authenticator apps.</param>
    /// <param name="email">The user's email address, used as the account label.</param>
    /// <param name="key">The shared secret key for TOTP generation.</param>
    /// <returns>A <see cref="SetupCode"/> containing the QR-code image and manual entry key.</returns>
    public async Task<SetupCode> GenerateAuthenticatorSetupCode(string applicationName, string email, string key)
    {
        var twoFactorAuthenticator = new TwoFactorAuthenticator();
        var setupCode = twoFactorAuthenticator.GenerateSetupCode(applicationName, email, key, false);
        return await Task.FromResult(setupCode);
    }

    /// <summary>
    /// Verifies a TOTP token against the user's stored authenticator key.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="token">The six-digit TOTP token to verify.</param>
    /// <returns><c>true</c> if the token is valid for the user's authenticator key; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="userId"/> is <see cref="Guid.Empty"/>.</exception>
    public async Task<bool> VerifyTwoFactorTokenAsync(Guid userId, string token)
    {
        if (userId == Guid.Empty) throw new ArgumentNullException(nameof(userId));

        // Retrieve the stored authenticator key for the user
        var entry = await GetUserToken(userId, InternalLoginProvider, AuthenticatorKeyTokenName);
        if (entry != null)
        {
            var twoFactorAuthenticator = new TwoFactorAuthenticator();
            return twoFactorAuthenticator.ValidateTwoFactorPIN(entry.Value, token);
        }

        return false;
    }

    /// <summary>
    /// Retrieves a user token record from the repository by user ID, login provider, and token name.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="loginProvider">The login provider that issued the token.</param>
    /// <param name="name">The token name.</param>
    /// <returns>The matching <see cref="UserTokens"/> record, or <c>null</c> if not found.</returns>
    private async Task<UserTokens?> GetUserToken(Guid userId, string loginProvider, string name)
    {
        var userToken = await userTokenRepository.GetUserTokenAsync(userId, name, loginProvider);
        if (userToken != null && userToken.Count > 0) return userToken[0];

        return null;
    }
}
