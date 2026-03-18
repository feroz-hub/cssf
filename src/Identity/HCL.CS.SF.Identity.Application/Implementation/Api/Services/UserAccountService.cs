/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using System.Transactions;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Configurations.Api;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.DomainServices.UnitOfWork.Api;
using HCL.CS.SF.DomainServices.Wrappers;
using HCL.CS.SF.Service.Extension;
using HCL.CS.SF.Service.Implementation.Api.Extension;
using HCL.CS.SF.Service.Implementation.Api.Validators;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;

namespace HCL.CS.SF.Service.Implementation.Api.Services;

/// <summary>
/// Core identity management service handling the full user lifecycle within the OAuth2/OIDC
/// identity server. Provides user registration, profile updates, deletion (with cascading cleanup
/// of claims, tokens, roles, security questions, and password history), password management
/// (change, reset, expiry, reuse prevention), account locking/unlocking, and user lookup.
/// This is a partial class with extensions in UserRoleService, UserClaimService,
/// UserSecurityQuestionService, UserTokenService, and TwoFactorTypeService.
/// </summary>

public partial class UserAccountService : SecurityBase, IUserAccountService
{
    private readonly IEmailService emailSender;
    private readonly IFrameworkResultService frameworkResultService;
    private readonly ILoggerService loggerService;
    private readonly IMapper mapper;
    private readonly IPasswordHasher<Users> passwordHasher;
    private readonly IResourceStringHandler resourceStringHandler;
    private readonly RoleManagerWrapper<Roles> roleManager;
    private readonly IRoleService roleService;
    private readonly HCLCSSFConfig securityConfig;
    private readonly IRepository<SecurityTokens> securityTokenRepository;
    private readonly ISmsService smsSender;
    private readonly IUserManagementUnitOfWork userManagementUnitOfWork;
    private readonly UserManagerWrapper<Users> userManager;
    private readonly IRepository<UserSecurityQuestions> userSecurityQuestionsRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserAccountService"/> class.
    /// </summary>
    public UserAccountService(
        UserManagerWrapper<Users> userManager,
        ILoggerInstance instance,
        IResourceStringHandler resourceStringHandler,
        IUserManagementUnitOfWork userManagementUnitOfWork,
        IMapper mapper,
        HCLCSSFConfig securityConfig,
        IEmailService emailService,
        ISmsService smsService,
        IFrameworkResultService frameworkResultService,
        IRepository<UserSecurityQuestions> userSecurityQuestionsRepository,
        IPasswordHasher<Users> passwordHasher,
        IRoleService roleService,
        IRepository<SecurityTokens> securityTokenRepository,
        RoleManagerWrapper<Roles> roleManager)
    {
        this.userManager = userManager;
        this.userManagementUnitOfWork = userManagementUnitOfWork;
        loggerService = instance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
        this.resourceStringHandler = resourceStringHandler;
        this.mapper = mapper;
        this.frameworkResultService = frameworkResultService;
        this.securityConfig = securityConfig;
        emailSender = emailService;
        smsSender = smsService;
        this.userSecurityQuestionsRepository = userSecurityQuestionsRepository;
        this.passwordHasher = passwordHasher;
        this.roleService = roleService;
        this.securityTokenRepository = securityTokenRepository;
        this.roleManager = roleManager;
    }

    private async Task PrepareLockoutStateChangeAsync(Users user, DateTimeOffset? lockoutEnd, bool lockoutEnabled)
    {
        var concurrencyStamp = user.ConcurrencyStamp;
        user.LockoutEnd = lockoutEnd;
        user.LockoutEnabled = lockoutEnabled;
        user.ConcurrencyStamp = Guid.NewGuid().ToString();

        await userManagementUnitOfWork.SetConcurrencyOriginalValueAsync(user, concurrencyStamp);
        await userManagementUnitOfWork.SetPropertyModifiedStatusAsync(user, nameof(Users.LockoutEnd));
        await userManagementUnitOfWork.SetPropertyModifiedStatusAsync(user, nameof(Users.LockoutEnabled));
        await userManagementUnitOfWork.SetPropertyModifiedStatusAsync(user, nameof(Users.ConcurrencyStamp));
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> RegisterUserAsync(UserModel user)
    {
        if (user == null) return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.UserModelIsNull);

        var frameworkResult = ValidateUser(user, securityConfig.SystemSettings.UserConfig, true);
        if (frameworkResult.Status == ResultStatus.Failed) return frameworkResult;

        frameworkResult = ValidatePassword(user);
        if (frameworkResult.Status == ResultStatus.Failed) return frameworkResult;

        frameworkResult = ValidateUserSecurityQuestion(user, securityConfig.SystemSettings.UserConfig, true);
        if (frameworkResult.Status == ResultStatus.Failed) return frameworkResult;

        frameworkResult = ValidateUserClaims(user, true);
        if (frameworkResult.Status == ResultStatus.Failed) return frameworkResult;

        loggerService.WriteTo(Log.Debug, "Entered in register new user: " + user.UserName + " by: " + user.CreatedBy);
        try
        {
            var existingUser = await FindUserByUserName(user.UserName);
            if (!existingUser.Item1)
            {
                SetDefaultValuesForCreate(user);

                var usersEntity = mapper.Map<UserModel, Users>(user);
                var result = await userManager.CreateAsync(usersEntity, user.Password);
                if (result.Succeeded)
                {
                    await userManagementUnitOfWork.SetAddedStatusAsync(usersEntity);

                    await AddPasswordHistory(usersEntity);

                    if (user.UserSecurityQuestion.ContainsAny())
                        foreach (var userSecurityQuestion in user.UserSecurityQuestion)
                        {
                            userSecurityQuestion.UserId = usersEntity.Id;
                            await AddUserQuestionAsync(userSecurityQuestion);
                        }

                    if (user.UserClaims.ContainsAny())
                        foreach (var claims in user.UserClaims)
                        {
                            claims.UserId = usersEntity.Id;
                            await AddUserClaimsAsync(claims);
                        }

                    // Assigning default HCL.CS.SF role.
                    var role = await roleManager.FindByNameAsync(securityConfig.SystemSettings.UserConfig
                        .DefaultUserRole);
                    if (role != null)
                    {
                        var userRole = new UserRoles
                        {
                            UserId = usersEntity.Id,
                            RoleId = role.Id,
                            ValidFrom = DateTime.UtcNow,
                            ValidTo = DateTime.MaxValue,
                            CreatedBy = usersEntity.CreatedBy,
                            CreatedOn = usersEntity.CreatedOn
                        };
                        await userManagementUnitOfWork.UserRoleRepository.InsertAsync(userRole);
                    }

                    frameworkResult = await userManagementUnitOfWork.SaveChangesAsync();
                    if (frameworkResult.Status == ResultStatus.Failed) return frameworkResult;

                    loggerService.WriteTo(Log.Debug, "User account created successfully. for user: " + user.UserName);
                    return frameworkResultService.Succeeded();
                }

                return frameworkResultService.Failed(result.ConstructIdentityErrorAsList());
            }

            return frameworkResultService.Failed<FrameworkResult>(existingUser.Item2);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to create user: " + user.UserName);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="userModel">The user model.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> UpdateUserAsync(UserModel userModel)
    {
        if (userModel == null) return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.UserModelIsNull);

        try
        {
            var frameworkResult = ValidateUser(userModel, securityConfig.SystemSettings.UserConfig, false);
            if (frameworkResult.Status == ResultStatus.Failed) return frameworkResult;

            frameworkResult = ValidateUserSecurityQuestion(userModel, securityConfig.SystemSettings.UserConfig, false);
            if (frameworkResult.Status == ResultStatus.Failed) return frameworkResult;

            frameworkResult = ValidateUserClaims(userModel, false);
            if (frameworkResult.Status == ResultStatus.Failed) return frameworkResult;

            loggerService.WriteTo(Log.Debug, "Entered in update user for " + userModel.UserName);
            var existingUser = await FindByIdAsync(userModel.Id);
            if (existingUser != null)
            {
                var concurrencyStamp = existingUser.ConcurrencyStamp;
                userModel = SetDefaultValuesForUpdate(userModel, existingUser);

                existingUser = mapper.Map(userModel, existingUser);
                var result = await userManager.UpdateAsync(existingUser);
                if (!result.Succeeded) return frameworkResultService.Failed(result.ConstructIdentityErrorAsList());

                await userManagementUnitOfWork.SetModifiedStatusAsync(existingUser, concurrencyStamp);
                if (userModel.UserSecurityQuestion.ContainsAny())
                    foreach (var userSecurityQuestion in userModel.UserSecurityQuestion)
                    {
                        frameworkResult = await UpdateUserQuestionAsync(userSecurityQuestion);
                        if (frameworkResult.Status == ResultStatus.Failed) return frameworkResult;
                    }

                if (userModel.UserClaims.ContainsAny())
                    foreach (var claims in userModel.UserClaims)
                    {
                        frameworkResult = await UpdateClaimAsync(claims);
                        if (frameworkResult.Status == ResultStatus.Failed) return frameworkResult;
                    }

                frameworkResult = await userManagementUnitOfWork.SaveChangesAsync();
                if (frameworkResult.Status == ResultStatus.Failed) return frameworkResult;
            }
            else
            {
                return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserId);
            }
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while updating user.");
            throw;
        }

        return frameworkResultService.Succeeded();
    }

    /// <summary>
    /// Deletes the specified user.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> DeleteUserAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUsername);

        try
        {
            var user = await userManager.FindByNameAsync(username);
            if (user != null) return await DeleteUserAsync(user);

            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUsername);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to Delete user");
            throw;
        }
    }

    /// <summary>
    /// Deletes the specified user.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> DeleteUserAsync(Guid userId)
    {
        if (!userId.IsValid()) return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserId);

        try
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user != null) return await DeleteUserAsync(user);

            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserId);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to Delete user");
            throw;
        }
    }

    /// <summary>
    /// Changes the password.
    /// </summary>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> ChangePasswordAsync(Guid userId, string currentPassword,
        string newPassword)
    {
        if (!userId.IsValid()) return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserId);

        if (string.IsNullOrWhiteSpace(currentPassword))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidCurrentPassword);

        if (string.IsNullOrWhiteSpace(newPassword))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidNewPassword);

        if (currentPassword == newPassword)
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.CurrentAndNewPasswordAreSame);

        try
        {
            var errorCode = string.Empty;
            var userManagementUtils = new UserManagementValidator();
            var isComplexPassword = userManagementUtils.IsComplexPassword(
                newPassword,
                securityConfig.SystemSettings.PasswordConfig,
                out errorCode);

            if (!isComplexPassword) return frameworkResultService.Failed<FrameworkResult>(errorCode);

            var user = await FindByIdAsync(userId);
            if (user != null)
            {
                if (user.IdentityProviderType == IdentityProvider.Ldap)
                    return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.RestrictedApiForLdapUser);

                var concurrencyStamp = user.ConcurrencyStamp;
                loggerService.WriteTo(Log.Debug, "Entered into change password for user: " + user.UserName);

                if (!await userManager.CheckPasswordAsync(user, currentPassword))
                    return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.PasswordMismatch);

                var passwordReused = await IsPasswordReused(
                    user,
                    newPassword,
                    securityConfig.SystemSettings.PasswordConfig.MaxLimitPasswordReuse);
                if (passwordReused)
                    return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.PasswordFromPreviousList);

                user.LastPasswordChangedDate = DateTime.UtcNow;
                user.RequiresDefaultPasswordChange = false;
                var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
                if (result.Succeeded)
                {
                    await userManagementUnitOfWork.SetModifiedStatusAsync(user, concurrencyStamp);
                    await AddPasswordHistory(user);
                    return await userManagementUnitOfWork.SaveChangesAsync();
                }

                return frameworkResultService.Failed(result.ConstructIdentityErrorAsList());
            }

            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserId);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while changing password.");
            throw;
        }
    }

    /// <summary>
    /// Resets the password.
    /// </summary>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> ResetPasswordAsync(
        string username,
        string passwordResetToken,
        string newPassword)
    {
        if (string.IsNullOrWhiteSpace(username))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUsername);

        if (string.IsNullOrWhiteSpace(passwordResetToken))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidResetTokenProvided);

        if (string.IsNullOrWhiteSpace(newPassword))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidNewPassword);

        try
        {
            var userManagementUtils = new UserManagementValidator();
            var errorCode = string.Empty;
            var isComplexPassword = userManagementUtils.IsComplexPassword(
                newPassword,
                securityConfig.SystemSettings.PasswordConfig,
                out errorCode);

            if (!isComplexPassword) return frameworkResultService.Failed<FrameworkResult>(errorCode);

            var user = await userManager.FindByNameAsync(username);
            if (user != null)
            {
                if (user.IdentityProviderType == IdentityProvider.Ldap)
                    return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.RestrictedApiForLdapUser);

                var concurrencyStamp = user.ConcurrencyStamp;
                var passwordNotReused = await IsPasswordReused(
                    user,
                    newPassword,
                    securityConfig.SystemSettings.PasswordConfig.MaxLimitPasswordReuse);
                if (passwordNotReused)
                    return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.PasswordFromPreviousList);

                loggerService.WriteTo(Log.Debug, "Entered in verify password reset token for user: " + user.UserName);

                user.LastPasswordChangedDate = DateTime.UtcNow;
                user.RequiresDefaultPasswordChange = false;
                var result = await userManager.ResetPasswordAsync(user, passwordResetToken, newPassword);
                if (result.Succeeded)
                {
                    await userManagementUnitOfWork.SetModifiedStatusAsync(user, concurrencyStamp);
                    await AddPasswordHistory(user);
                    return await userManagementUnitOfWork.SaveChangesAsync();
                }

                return frameworkResultService.Failed(result.ConstructIdentityErrorAsList());
            }

            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserId);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while verifying password reset token.");
            throw;
        }
    }

    /// <summary>
    /// Locks the specified user.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> LockUserAsync(Guid userId)
    {
        if (!userId.IsValid()) return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserId);

        try
        {
            var user = await FindByIdAsync(userId);
            if (user != null)
            {
                if (user.IdentityProviderType == IdentityProvider.Ldap)
                    return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.RestrictedApiForLdapUser);

                loggerService.WriteTo(Log.Debug, "Entered in Disable/Lock account for user: " + user.UserName);
                await PrepareLockoutStateChangeAsync(
                    user,
                    new DateTimeOffset(DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc)),
                    true);
                return await userManagementUnitOfWork.SaveChangesAsync();
            }

            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserId);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while disabling user.");
            throw;
        }
    }

    /// <summary>
    /// Locks the specified user.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <param name="dateTime">The date time.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> LockUserAsync(Guid userId, DateTime? dateTime)
    {
        if (!userId.IsValid()) return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserId);

        if (dateTime != null && dateTime <= DateTime.UtcNow)
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidLockoutEndDate);

        try
        {
            var user = await FindByIdAsync(userId);
            if (user != null)
            {
                if (user.IdentityProviderType == IdentityProvider.Ldap)
                    return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.RestrictedApiForLdapUser);

                loggerService.WriteTo(Log.Debug, "Entered in LockUserAsync for user: " + user.UserName);
                await PrepareLockoutStateChangeAsync(
                    user,
                    dateTime.HasValue
                        ? new DateTimeOffset(DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc))
                        : null,
                    true);
                return await userManagementUnitOfWork.SaveChangesAsync();
            }

            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserId);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while locking User.");
            throw;
        }
    }

    /// <summary>
    /// Unlocks the specified user.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> UnLockUserAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUsername);

        try
        {
            var user = await userManager.FindByNameAsync(username);
            if (user != null)
            {
                if (user.IdentityProviderType == IdentityProvider.Ldap)
                    return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.RestrictedApiForLdapUser);

                loggerService.WriteTo(Log.Debug, "Entered into unlock user for " + user.UserName);
                await PrepareLockoutStateChangeAsync(user, null, false);
                return await userManagementUnitOfWork.SaveChangesAsync();
            }

            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserId);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while locking a user.");
            throw;
        }
    }

    /// <summary>
    /// Unlocks the specified user.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="token">The token.</param>
    /// <param name="purpose">The purpose.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> UnLockUserAsync(string username, string token, string purpose)
    {
        if (string.IsNullOrWhiteSpace(username))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUsername);

        try
        {
            var user = await userManager.FindByNameAsync(username);
            if (user != null)
            {
                if (user.IdentityProviderType == IdentityProvider.Ldap)
                    return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.RestrictedApiForLdapUser);

                var frameworkResult = await VerifyUserTokenAsync(user, purpose, token);
                loggerService.WriteTo(Log.Debug, "Entered into unlock user using security token for " + user.UserName);
                if (frameworkResult.Status == ResultStatus.Succeeded)
                {
                    await PrepareLockoutStateChangeAsync(user, null, false);
                    return await userManagementUnitOfWork.SaveChangesAsync();
                }

                return frameworkResult;
            }

            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserId);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while un-locking User.");
            throw;
        }
    }

    /// <summary>
    /// Unlocks the specified user.
    /// </summary>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> UnlockUserAsync(string username,
        IList<UserSecurityQuestionModel> userSecurityQuestions)
    {
        if (string.IsNullOrWhiteSpace(username))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUsername);

        try
        {
            var user = await userManager.FindByNameAsync(username);
            if (user != null)
            {
                if (user.IdentityProviderType == IdentityProvider.Ldap)
                    return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.RestrictedApiForLdapUser);

                var frameworkResult = await VerifySecurityQuestions(user.Id, userSecurityQuestions);
                loggerService.WriteTo(Log.Debug,
                    "Entered in Enable account using userSecurityQuestions for user :" + user.UserName);
                if (frameworkResult.Status == ResultStatus.Succeeded)
                {
                    await PrepareLockoutStateChangeAsync(user, null, false);
                    return await userManagementUnitOfWork.SaveChangesAsync();
                }

                return frameworkResult;
            }

            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserId);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while enabling user.");
            throw;
        }
    }

    /// <summary>
    /// Retrieves the user by id.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <returns>The operation result.</returns>
    public virtual async Task<UserModel> GetUserByIdAsync(Guid userId)
    {
        if (!userId.IsValid()) frameworkResultService.Throw(ApiErrorCodes.InvalidUserId);

        try
        {
            var users = await userManager.FindByIdAsync(userId.ToString());
            if (users != null) return await GetUserChildModels(users);

            return frameworkResultService.EmptyResult<UserModel>(ApiErrorCodes.NoRecordsFound);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while fetching User.");
            throw;
        }
    }

    /// <summary>
    /// Retrieves the user by name.
    /// </summary>
    /// <param name="userName">The user name.</param>
    /// <returns>The operation result.</returns>
    public virtual async Task<UserModel> GetUserByNameAsync(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName)) frameworkResultService.Throw(ApiErrorCodes.InvalidUsername);

        try
        {
            var users = await userManager.FindByNameAsync(userName);
            if (users != null)
            {
                loggerService.WriteTo(Log.Information, "Fetching user informatioin for User - " + userName);
                return await GetUserChildModels(users);
            }

            loggerService.WriteTo(Log.Information,
                "Fetching user informatioin - No record found for User - " + userName);
            return frameworkResultService.EmptyResult<UserModel>(ApiErrorCodes.NoRecordsFound);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while fetching User.");
            throw;
        }
    }

    /// <summary>
    /// Retrieves the user by email.
    /// </summary>
    /// <param name="email">The email.</param>
    /// <returns>The operation result.</returns>
    public virtual async Task<UserModel> GetUserByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) frameworkResultService.Throw(ApiErrorCodes.EmailRequired);

        try
        {
            var users = await userManager.FindByEmailAsync(email);
            if (users != null) return await GetUserChildModels(users);

            return frameworkResultService.EmptyResult<UserModel>(ApiErrorCodes.NoRecordsFound);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while fetching User.");
            throw;
        }
    }

    /// <summary>
    /// Retrieves the users for claim.
    /// </summary>
    /// <param name="claimType">The claim type.</param>
    /// <param name="claimValue">The claim value.</param>
    /// <returns>The matching collection of results.</returns>
    public virtual async Task<IList<UserModel>> GetUsersForClaimAsync(string claimType, string claimValue)
    {
        if (string.IsNullOrWhiteSpace(claimType)) frameworkResultService.Throw(ApiErrorCodes.InvalidClaimType);

        if (string.IsNullOrWhiteSpace(claimValue)) frameworkResultService.Throw(ApiErrorCodes.InvalidClaimValue);

        try
        {
            // Added for JsonDeserialization error.
            var claim = new Claim(claimType, claimValue);
            var usersList = await userManager.GetUsersForClaimAsync(claim);
            if (usersList.ContainsAny()) return mapper.Map<IList<Users>, IList<UserModel>>(usersList);

            return frameworkResultService.EmptyResult<IList<UserModel>>(ApiErrorCodes.NoRecordsFound);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while fetching User.");
            throw;
        }
    }

    /// <summary>
    /// Determines whether user exist.
    /// </summary>
    /// <param name="claimsPrincipal">The claims principal.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public virtual async Task<bool> IsUserExistsAsync(ClaimsPrincipal claimsPrincipal)
    {
        if (claimsPrincipal == null)
        {
            loggerService.WriteTo(Log.Error, ApiErrorCodes.InvalidClaimsPrincipal);
            return false;
        }

        try
        {
            var id = claimsPrincipal.Identity as ClaimsIdentity;
            if (id != null)
            {
                var claim = id.FindFirst(OpenIdConstants.ClaimTypes.Sub);
                if (claim != null)
                {
                    Users user = null;
                    if (claim.Value.IsGuid())
                    {
                        user = await userManager.FindByIdAsync(claim.Value);
                        if (user != null)
                        {
                            loggerService.WriteTo(Log.Debug, "Entered into Is User Exists for user :" + user.UserName);
                            return true;
                        }
                    }

                    user = await userManager.FindByNameAsync(claim.Value);
                    if (user != null)
                    {
                        loggerService.WriteTo(Log.Debug, "Entered into Is User Exists for user :" + user.UserName);
                        return true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while checking user exists.");
            throw;
        }

        return false;
    }

    /// <summary>
    /// Determines whether user exist.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public virtual async Task<bool> IsUserExistsAsync(Guid userId)
    {
        if (!userId.IsValid()) frameworkResultService.Throw(ApiErrorCodes.InvalidUserId);

        try
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                loggerService.WriteTo(Log.Debug, $"{user.UserName} Exists");
                return true;
            }
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while checking user exists.");
            throw;
        }

        return false;
    }

    /// <summary>
    /// Determines whether user exist.
    /// </summary>
    /// <param name="userName">The user name.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public virtual async Task<bool> IsUserExistsAsync(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName)) frameworkResultService.Throw(ApiErrorCodes.InvalidUserId);

        try
        {
            var user = await userManager.FindByNameAsync(userName);
            if (user != null)
            {
                //loggerService.WriteTo(Log.Debug, "Entered into Is User Exists");
                loggerService.WriteTo(Log.Debug, $"IsExists for {user.UserName}");
                return true;
            }
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while checking user exists.");
            throw;
        }

        return false;
    }

    /// <summary>
    /// Retrieves the all user.
    /// </summary>
    /// <returns>The matching collection of results.</returns>
    public virtual async Task<IList<UserDisplayModel>> GetAllUsersAsync()
    {
        try
        {
            var users = await userManagementUnitOfWork.UserRepository.GetAllUsersAsync();
            if (users != null)
            {
                loggerService.WriteTo(Log.Debug, "Entered in get all users");
                var userModel = mapper.Map<IList<Users>, IList<UserDisplayModel>>(users);
                return userModel;
            }

            return frameworkResultService.EmptyResult<IList<UserDisplayModel>>(ApiErrorCodes.NoRecordsFound);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to retrieve users.");
            throw;
        }
    }

    private async Task<Users> FindByIdAsync(Guid userId)
    {
        return await userManager.FindByIdAsync(userId.ToString());
    }

    private async Task AddPasswordHistory(Users usersEntity)
    {
        var passwordHistory = new PasswordHistory
        {
            UserId = usersEntity.Id,
            PasswordHash = usersEntity.PasswordHash,
            ChangedOn = DateTime.UtcNow,
            CreatedBy = usersEntity.CreatedBy
        };
        await userManagementUnitOfWork.PasswordHistoryRepository.InsertAsync(passwordHistory);
    }

    private async Task<bool> IsPasswordReused(Users user, string newPassword, int maxReuseCount)
    {
        if (maxReuseCount > 0)
        {
            var orderByDescendingResult =
                (from s in await userManagementUnitOfWork.PasswordHistoryRepository.GetAsync(x =>
                        x.UserId == user.Id)
                    orderby s.ChangedOn descending
                    select s.PasswordHash).Take(maxReuseCount);
            foreach (var passwordHash in orderByDescendingResult)
            {
                var passwordMatch = passwordHasher.VerifyHashedPassword(user, passwordHash, newPassword);
                if (passwordMatch == PasswordVerificationResult.Success) return true;
            }
        }

        return false;
    }

    private bool IsSamePassword(Users users, string existingPassword, string newPassword)
    {
        if (existingPassword != newPassword)
        {
            var passwordMatch =
                userManager.PasswordHasher.VerifyHashedPassword(users, existingPassword, newPassword);
            if (passwordMatch == PasswordVerificationResult.Success) return true;

            return false;
        }

        return true;
    }

    private FrameworkResult ValidateUser(UserModel user, UserConfig userConfig, bool isFromRegister)
    {
        if (string.IsNullOrWhiteSpace(user.UserName))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.UsernameRequired);

        if (user.UserName.Length < userConfig.MinUserNameLength ||
            user.UserName.Length >= userConfig.MaxUserNameLength)
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidLengthForUsername);

        if (string.IsNullOrWhiteSpace(user.FirstName))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.FirstnameRequired);

        if (user.FirstName.Length < userConfig.MinFirstAndLastNameLength ||
            user.FirstName.Length >= userConfig.MaxFirstAndLastNameLength)
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidLengthForFirstName);

        if (!string.IsNullOrWhiteSpace(user.LastName) &&
            (user.LastName.Length < userConfig.MinFirstAndLastNameLength ||
             user.LastName.Length >= userConfig.MaxFirstAndLastNameLength))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidLengthForLastName);

        // Password validation done in asp net core identity side.
        // Email alidation is done at HCL.CS.SF.
        var commonHelper = new UserManagementValidator();
        if (string.IsNullOrWhiteSpace(user.Email))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.EmailRequired);

        if (user.Email.Length > Constants.ColumnLength255)
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.EmailLengthTooLong);

        var isValidEmail = commonHelper.IsValidEmailAddress(user.Email);
        if (!isValidEmail) return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidEmailFormat);

        if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
        {
            var isValidPhoneNumber = commonHelper.IsValidPhoneNumber(
                user.PhoneNumber,
                userConfig.MinPhoneNumberLength,
                userConfig.MaxPhoneNumberLength);

            if (!isValidPhoneNumber)
                return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidLengthForPhoneNumber);
        }

        if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
        {
            var isValidPhoneNumber = commonHelper.IsValidPhoneNumber(
                user.PhoneNumber);

            if (!isValidPhoneNumber)
                return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidPhonenumber);
        }

        if (user.DateOfBirth != null)
        {
            var isValidDOB =
                commonHelper.IsValidDateOfBirth(user.DateOfBirth, userConfig.MinDOBYear, userConfig.MaxDOBYear);
            if (!isValidDOB) return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidDob);
        }

        if (user.TwoFactorEnabled && user.TwoFactorType == TwoFactorType.None)
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidTwoFactorType);

        if (!string.IsNullOrWhiteSpace(user.CreatedBy) && user.CreatedBy.Length > Constants.ColumnLength255)
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.CreatedByTooLong);

        if (!isFromRegister)
        {
            if (!string.IsNullOrWhiteSpace(user.ModifiedBy) && user.ModifiedBy.Length > Constants.ColumnLength255)
                return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.ModifiedByTooLong);

            if (!user.Id.IsValid()) return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserId);
        }

        return frameworkResultService.Succeeded();
    }

    private FrameworkResult ValidateUserSecurityQuestion(UserModel user, UserConfig userConfig, bool isFromRegister)
    {
        if (user.IdentityProviderType == IdentityProvider.Local && userConfig.MinNoOfQuestions > 0)
        {
            if (!user.UserSecurityQuestion.ContainsAny())
                return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.SecurityQuestionRequired);

            if (user.UserSecurityQuestion.Count < userConfig.MinNoOfQuestions)
            {
                var message =
                    string.Format(
                        resourceStringHandler.GetResourceString(ApiErrorCodes.InvalidSecurityQuestionCount),
                        userConfig.MinNoOfQuestions);
                return frameworkResultService.ConstructFailed(ApiErrorCodes.InvalidSecurityQuestionCount, message);
            }

            var duplicates = user.UserSecurityQuestion.GroupBy(x => x.SecurityQuestionId).Where(x => x.Skip(1).Any());
            if (duplicates.ContainsAny())
                return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.UniqueSecurityQuestion);

            foreach (var securityQuestion in user.UserSecurityQuestion)
            {
                if (string.IsNullOrWhiteSpace(securityQuestion.Answer))
                    return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidSecurityAnswer);

                if (userConfig.MinSecurityAnswersLength > 0 &&
                    securityQuestion.Answer.Length < userConfig.MinSecurityAnswersLength)
                {
                    var message = string.Format(
                        resourceStringHandler.GetResourceString(ApiErrorCodes.InvalidSecurityAnswerLength),
                        userConfig.MinSecurityAnswersLength);
                    return frameworkResultService.ConstructFailed(ApiErrorCodes.InvalidSecurityAnswerLength, message);
                }

                if (!securityQuestion.SecurityQuestionId.IsValid())
                    return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes
                        .UserSecurityQuestionSecurityIdRequired);

                if (securityQuestion.Answer.Length > Constants.ColumnLength255)
                    return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.SecurityQuestionAnswerTooLong);

                if (!string.IsNullOrWhiteSpace(securityQuestion.CreatedBy) &&
                    securityQuestion.CreatedBy.Length > Constants.ColumnLength255)
                    return frameworkResultService.Failed<FrameworkResult>(
                        ApiErrorCodes.SecurityQuestionCreatedbyTooLong);

                if (!isFromRegister)
                {
                    if (!securityQuestion.UserId.IsValid())
                        return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes
                            .UserSecurityQuestionUserIdRequired);

                    if (!string.IsNullOrWhiteSpace(securityQuestion.ModifiedBy) &&
                        securityQuestion.ModifiedBy.Length > Constants.ColumnLength255)
                        return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes
                            .SecurityQuestionModifiedbyTooLong);
                }
            }
        }

        return frameworkResultService.Succeeded();
    }

    private FrameworkResult ValidateUserClaims(UserModel user, bool isFromRegister)
    {
        if (user.UserClaims.ContainsAny())
            foreach (var claims in user.UserClaims)
            {
                if (string.IsNullOrWhiteSpace(claims.ClaimType))
                    return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.UserClaimTypeRequired);

                if (string.IsNullOrWhiteSpace(claims.ClaimValue))
                    return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.UserClaimValueRequired);

                if (!string.IsNullOrWhiteSpace(claims.CreatedBy) && claims.CreatedBy.Length > Constants.ColumnLength255)
                    return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.UserClaimCreatedbyTooLong);

                if (!isFromRegister)
                {
                    if (!claims.UserId.IsValid())
                        return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.UserClaimUserIdRequired);

                    if (!string.IsNullOrWhiteSpace(claims.ModifiedBy) &&
                        claims.ModifiedBy.Length > Constants.ColumnLength255)
                        return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.UserClaimModifiedbyTooLong);
                }
            }

        return frameworkResultService.Succeeded();
    }

    private FrameworkResult ValidatePassword(UserModel user)
    {
        if (string.IsNullOrWhiteSpace(user.Password))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.PasswordRequired);

        if (user.Password.Length > securityConfig.SystemSettings.PasswordConfig.MaxPasswordLength)
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.PasswordTooLong);

        if (user.Password.Contains(" "))
            return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.PasswordContainsSpace);

        return frameworkResultService.Succeeded();
    }

    private async Task<(bool, string)> FindUserByUserName(string userName)
    {
        var user = await userManagementUnitOfWork.UserRepository.FindByUserNameIncludingDeletedAsync(userName);
        if (user != null)
        {
            if (user.IsDeleted) return (true, ApiErrorCodes.UserIsAlreadyDeleted);

            return (true, ApiErrorCodes.UserAlreadyExists);
        }

        return (false, string.Empty);
    }

    private async Task<FrameworkResult> DeleteUserAsync(Users user)
    {
        if (user != null)
        {
            // Transaction scope required - because delete client token is hard delete. we can't use UOF here.
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                loggerService.WriteTo(Log.Debug, "Entered in delete user: " + user.UserName);

                // As identity doesn�t have any additional logic on delete claim API deleting claim is done via repository.
                var userClaims = await userManagementUnitOfWork.UserClaimRepository.GetClaimsAsync(user.Id);
                if (userClaims.ContainsAny())
                    await userManagementUnitOfWork.UserClaimRepository.DeleteAsync(userClaims);

                // Deleting user token if any.
                var userToken = await userManagementUnitOfWork.UserTokenRepository.GetUserTokenAsync(user.Id);
                if (userToken.ContainsAny()) await userManagementUnitOfWork.UserTokenRepository.DeleteAsync(userToken);

                // Deleting user role if any.
                var userroles = await userManagementUnitOfWork.UserRoleRepository.GetUserRoleAsync(user.Id);
                if (userroles.ContainsAny()) await userManagementUnitOfWork.UserRoleRepository.DeleteAsync(userroles);

                // Deleting user security question if any.
                var userSecurityQuestion =
                    await userManagementUnitOfWork.UserSecurityQuestionsRepository.GetAsync(x =>
                        x.UserId == user.Id);
                if (userSecurityQuestion.ContainsAny())
                    await userManagementUnitOfWork.UserSecurityQuestionsRepository.DeleteAsync(userSecurityQuestion
                        .ToList());

                // Deleting user password if any.
                var userPasswordHistory =
                    await userManagementUnitOfWork.PasswordHistoryRepository.GetAsync(x =>
                        x.UserId == user.Id);
                if (userPasswordHistory.ContainsAny())
                    await userManagementUnitOfWork.PasswordHistoryRepository.DeleteAsync(
                        userPasswordHistory.ToList());

                // Deleting user notification if any.
                var userNotification =
                    await userManagementUnitOfWork.NotificationRepository.GetAsync(x =>
                        x.UserId == user.Id);
                if (userNotification.ContainsAny())
                    await userManagementUnitOfWork.NotificationRepository.DeleteAsync(userNotification.ToList());

                // As identity doesn�t have any additional logic on delete API deleting user is done via repository.
                await userManagementUnitOfWork.UserRepository.DeleteAsync(user);

                var csresult = await userManagementUnitOfWork.SaveChangesAsync();
                if (csresult.Status == ResultStatus.Failed)
                {
                    loggerService.WriteTo(Log.Error, "User deletion failed for user: " + user.UserName);
                    return csresult;
                }

                var securityTokenList =
                    await securityTokenRepository.GetAsync(x => x.SubjectId == Convert.ToString(user.Id));
                if (securityTokenList.ContainsAny())
                {
                    await securityTokenRepository.DeleteAsync(securityTokenList);

                    csresult = await securityTokenRepository.SaveChangesWithHardDeleteAsync();
                    if (csresult.Status == ResultStatus.Failed)
                    {
                        loggerService.WriteTo(Log.Error, "User deletion failed for user: " + user.UserName);
                        return csresult;
                    }
                }

                transactionScope.Complete();
            }

            return frameworkResultService.Succeeded();
        }

        return frameworkResultService.Failed<FrameworkResult>(ApiErrorCodes.InvalidUserId);
    }


    private async Task<UserModel> GetUserChildModels(Users users)
    {
        // For Asp net identity tables include will not work hence getting child tables in separate call and binding in model.
        var userModel = mapper.Map<Users, UserModel>(users);
        var userClaims = await userManagementUnitOfWork.UserClaimRepository.GetClaimsAsync(users.Id);
        if (userClaims.ContainsAny())
        {
            var userClaimModel = mapper.Map<IList<UserClaims>, IList<UserClaimModel>>(userClaims);
            userModel.UserClaims = (List<UserClaimModel>)userClaimModel;
        }

        var userSecurityQuestion =
            await userManagementUnitOfWork.UserSecurityQuestionsRepository.GetAsync(x => x.UserId == users.Id);
        if (userSecurityQuestion.ContainsAny())
        {
            var userSecurityQuestions =
                mapper.Map<IList<UserSecurityQuestions>, IList<UserSecurityQuestionModel>>(userSecurityQuestion);
            userModel.UserSecurityQuestion = (List<UserSecurityQuestionModel>)userSecurityQuestions;
        }

        return userModel;
    }

    private void SetDefaultValuesForCreate(UserModel user)
    {
        if (user.IdentityProviderType == IdentityProvider.Local)
        {
            user.EmailConfirmed = !securityConfig.SystemSettings.UserConfig.RequireConfirmedEmail;
            user.PhoneNumberConfirmed = !securityConfig.SystemSettings.UserConfig.RequireConfirmedPhoneNumber;
        }

        user.LockoutEnd = null;
        user.LockoutEnabled = false;
        user.AccessFailedCount = 0;
        user.LastPasswordChangedDate = null;
        user.LastLoginDateTime = null;
        user.LastLogoutDateTime = null;
    }

    private UserModel SetDefaultValuesForUpdate(UserModel userModel, Users user)
    {
        if (user.IdentityProviderType == IdentityProvider.Local) userModel.Password = user.PasswordHash;

        userModel.EmailConfirmed = user.EmailConfirmed;
        userModel.PhoneNumberConfirmed = user.PhoneNumberConfirmed;
        userModel.LockoutEnd = user.LockoutEnd;
        userModel.LockoutEnabled = user.LockoutEnabled;
        userModel.AccessFailedCount = user.AccessFailedCount;
        userModel.RequiresDefaultPasswordChange = user.RequiresDefaultPasswordChange;
        userModel.LastPasswordChangedDate = user.LastPasswordChangedDate;
        userModel.LastLoginDateTime = user.LastLoginDateTime;
        userModel.LastLogoutDateTime = user.LastLogoutDateTime;
        userModel.IdentityProviderType = user.IdentityProviderType;

        if (userModel.Email != user.Email) userModel.EmailConfirmed = false;

        if (userModel.PhoneNumber != user.PhoneNumber) userModel.PhoneNumberConfirmed = false;

        return userModel;
    }
}
