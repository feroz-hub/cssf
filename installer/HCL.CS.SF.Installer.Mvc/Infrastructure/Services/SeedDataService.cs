/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Service.Implementation.Api.Wrappers;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCLCSSFInstallerMVC.Application.Abstractions;
using HCLCSSFInstallerMVC.Application.DTOs;
using HCLCSSFInstallerMVC.Infrastructure.Persistence.Data;
using HCLCSSFInstallerMVC.Infrastructure.Seeding;

namespace HCLCSSFInstallerMVC.Infrastructure.Services;

/// <summary>
/// Seeds the security framework database with master reference data, the initial admin user,
/// and the first OAuth client. Runs within a single database transaction to ensure atomicity.
/// </summary>
public sealed class SeedDataService : ISeedDataService
{
    /// <summary>Space-delimited default scopes assigned to the initial OAuth client.</summary>
    private static readonly string DefaultScopes = string.Join(" ", new[]
    {
        "openid",
        "email",
        "profile",
        "offline_access",
        "phone",
        "HCL.CS.SF.apiresource.manage",
        "HCL.CS.SF.client.manage",
        "HCL.CS.SF.user.read",
        "HCL.CS.SF.user.write",
        "HCL.CS.SF.role.manage",
        "HCL.CS.SF.identityresource.manage",
        "HCL.CS.SF.adminuser.manage",
        "HCL.CS.SF.securitytoken.manage",
        "HCL.CS.SF.notification.read",
        "HCL.CS.SF.notification.manage"
    });

    private readonly ILogger<SeedDataService> _logger;

    /// <summary>Initializes the seed service with logging.</summary>
    public SeedDataService(ILogger<SeedDataService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<SeedExecutionResultDto> SeedAsync(
        DatabaseConfigurationDto databaseConfiguration,
        SeedConfigurationDto seedConfiguration,
        CancellationToken cancellationToken)
    {
        try
        {
            var options = DatabaseProviderUtilities.BuildApplicationOptions(databaseConfiguration);

            await using var dbContext = new ApplicationDbContext(options);
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            if (await dbContext.Roles.AnyAsync(cancellationToken))
            {
                await transaction.RollbackAsync(cancellationToken);
                return new SeedExecutionResultDto
                {
                    Succeeded = false,
                    ErrorMessage = "Seed data already exists in the selected database."
                };
            }

            var apiResources = HCLCSSFMasterDataSeed.GetApiResourceEntityMaster();
            dbContext.ApiResources.AddRange(apiResources);

            var identityResources = HCLCSSFMasterDataSeed.CreateIdentityResourceModelMaster();
            dbContext.IdentityResources.AddRange(identityResources);

            var roles = HCLCSSFMasterDataSeed.CreateRolesMaster();
            dbContext.Roles.AddRange(roles);

            var securityQuestions = HCLCSSFMasterDataSeed.CreateSecurityQuestionsModelMaster();
            dbContext.SecurityQuestions.AddRange(securityQuestions);

            var adminRole = roles.First(r => r.Name == "HCLCSSFAdmin");
            var sfAdminClaims = HCLCSSFMasterDataSeed.CreateRoleClaims_HCLCSSFAdmin();
            foreach (var claim in sfAdminClaims) claim.RoleId = adminRole.Id;

            dbContext.RoleClaims.AddRange(sfAdminClaims);

            var userRole = roles.First(r => r.Name == "HCLCSSFUser");
            var sfUserClaims = HCLCSSFMasterDataSeed.CreateRoleClaims_HCLCSSFUser();
            foreach (var claim in sfUserClaims) claim.RoleId = userRole.Id;

            dbContext.RoleClaims.AddRange(sfUserClaims);

            var rentflowOwnerRole = roles.First(r => r.Name == "rentflow_owner");
            var rentflowOwnerClaims = HCLCSSFMasterDataSeed.CreateRoleClaims_RentFlowOwner();
            foreach (var claim in rentflowOwnerClaims) claim.RoleId = rentflowOwnerRole.Id;
            dbContext.RoleClaims.AddRange(rentflowOwnerClaims);

            var rentflowManagerRole = roles.First(r => r.Name == "rentflow_manager");
            var rentflowManagerClaims = HCLCSSFMasterDataSeed.CreateRoleClaims_RentFlowManager();
            foreach (var claim in rentflowManagerClaims) claim.RoleId = rentflowManagerRole.Id;
            dbContext.RoleClaims.AddRange(rentflowManagerClaims);

            var rentflowResidentRole = roles.First(r => r.Name == "rentflow_resident");
            var rentflowResidentClaims = HCLCSSFMasterDataSeed.CreateRoleClaims_RentFlowResident();
            foreach (var claim in rentflowResidentClaims) claim.RoleId = rentflowResidentRole.Id;
            dbContext.RoleClaims.AddRange(rentflowResidentClaims);

            var adminUser = BuildUser(seedConfiguration.AdminUser);
            dbContext.Users.Add(adminUser);

            var adminUserRole = HCLCSSFMasterDataSeed.CreateUserRoleModelMaster();
            adminUserRole.RoleId = adminRole.Id;
            adminUserRole.UserId = adminUser.Id;
            dbContext.UserRoles.Add(adminUserRole);

            var standardUserRole = HCLCSSFMasterDataSeed.CreateUserRoleModelMaster();
            standardUserRole.RoleId = userRole.Id;
            standardUserRole.UserId = adminUser.Id;
            dbContext.UserRoles.Add(standardUserRole);

            var (client, generatedClientSecret) = BuildClient(seedConfiguration.Client);
            dbContext.Clients.Add(client);

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new SeedExecutionResultDto
            {
                Succeeded = true,
                GeneratedClientId = client.ClientId,
                GeneratedClientSecret = generatedClientSecret
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Seed operation failed.");
            return new SeedExecutionResultDto
            {
                Succeeded = false,
                ErrorMessage = "Seed data generation failed. Verify supplied data and database state."
            };
        }
    }

    /// <summary>
    /// Builds a Users entity from the admin configuration, hashing the password with Argon2.
    /// </summary>
    private static Users BuildUser(AdminUserConfigurationDto userConfiguration)
    {
        var user = HCLCSSFMasterDataSeed.CreateUserModelMaster();
        user.UserName = userConfiguration.UserName;
        user.NormalizedUserName = userConfiguration.UserName.ToUpperInvariant();
        user.Email = userConfiguration.Email;
        user.NormalizedEmail = userConfiguration.Email.ToUpperInvariant();
        user.PhoneNumber = userConfiguration.PhoneNumber;
        user.FirstName = userConfiguration.FirstName.Trim();
        user.LastName = userConfiguration.LastName?.Trim() ?? string.Empty;
        user.IdentityProviderType = userConfiguration.IdentityProvider;

        var hasher = new Argon2PasswordHasherWrapper<Users>();
        user.PasswordHash = hasher.HashPassword(user, userConfiguration.Password);

        return user;
    }

    /// <summary>
    /// Builds a Clients entity from the client configuration, generating a cryptographic
    /// client ID and secret. The secret is SHA-256 hashed for storage; the plain-text
    /// value is returned for display to the administrator.
    /// </summary>
    private static (Clients Client, string GeneratedClientSecret) BuildClient(
        ClientConfigurationDto clientConfiguration)
    {
        var client = HCLCSSFMasterDataSeed.CreateClientMaster();
        client.ClientName = clientConfiguration.ClientName.Trim();
        client.ClientUri = clientConfiguration.ClientUri.Trim();
        var generatedClientSecret = GenerateSecret(32);
        client.ClientId = GenerateSecret(32);
        client.ClientSecret = generatedClientSecret.Sha256();
        client.ClientIdIssuedAt = ToUnixTime(DateTime.UtcNow);
        client.ClientSecretExpiresAt = ToUnixTime(DateTime.UtcNow.AddDays(100));
        client.SupportedGrantTypes = string.Join(" ", clientConfiguration.GrantTypes);
        client.SupportedResponseTypes = string.Join(" ", clientConfiguration.ResponseTypes);
        client.RedirectUris = clientConfiguration.RedirectUris
            .Select(uri => new ClientRedirectUris
                { RedirectUri = uri, CreatedOn = DateTime.UtcNow, CreatedBy = "HCLCSSFUser" })
            .ToList();
        client.PostLogoutRedirectUris = clientConfiguration.PostLogoutRedirectUris
            .Select(uri => new ClientPostLogoutRedirectUris
                { PostLogoutRedirectUri = uri, CreatedOn = DateTime.UtcNow, CreatedBy = "HCLCSSFUser" })
            .ToList();

        client.FrontChannelLogoutUri = string.IsNullOrWhiteSpace(clientConfiguration.FrontChannelLogoutUri)
            ? string.Empty
            : clientConfiguration.FrontChannelLogoutUri.Trim();
        client.BackChannelLogoutUri = string.IsNullOrWhiteSpace(clientConfiguration.BackChannelLogoutUri)
            ? string.Empty
            : clientConfiguration.BackChannelLogoutUri.Trim();

        if (string.IsNullOrWhiteSpace(client.FrontChannelLogoutUri)) client.FrontChannelLogoutSessionRequired = false;

        if (string.IsNullOrWhiteSpace(client.BackChannelLogoutUri)) client.BackChannelLogoutSessionRequired = false;

        client.AllowedScopes = clientConfiguration.UseDefaultScopes
            ? DefaultScopes
            : string.Join(" ",
                clientConfiguration.AllowedScopes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Distinct(StringComparer.OrdinalIgnoreCase));

        client.AllowedSigningAlgorithm = OpenIdConstants.Algorithms.RsaSha256;
        client.AccessTokenType = AccessTokenType.JWT;
        client.ApplicationType = ApplicationType.RegularWeb;

        return (client, generatedClientSecret);
    }

    /// <summary>Converts a DateTime to Unix epoch seconds.</summary>
    private static long ToUnixTime(DateTime value)
    {
        return Convert.ToInt64(value.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
    }

    /// <summary>Generates a cryptographically random Base64-encoded string of the specified byte length.</summary>
    private static string GenerateSecret(int length)
    {
        var bytes = new byte[length];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}
