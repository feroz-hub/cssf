/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HCLCSSFInstallerMVC.Infrastructure.Persistence.Migrations.MySql;

public partial class HCLCSSFMySqlV1 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            "HCL.CS.SF_ApiResources",
            table => new
            {
                Id = table.Column<Guid>(nullable: false),
                IsDeleted = table.Column<bool>(nullable: false),
                CreatedOn = table.Column<DateTime>(nullable: false),
                ModifiedOn = table.Column<DateTime>(nullable: true),
                CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                RowVersion = table.Column<DateTime>(rowVersion: true, nullable: true)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                Name = table.Column<string>(maxLength: 255, nullable: false),
                DisplayName = table.Column<string>(maxLength: 255, nullable: true),
                Description = table.Column<string>(nullable: true),
                Enabled = table.Column<bool>(nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_HCL.CS.SF_ApiResources", x => x.Id); });

        migrationBuilder.CreateTable(
            "HCL.CS.SF_AuditTrail",
            table => new
            {
                Id = table.Column<Guid>(nullable: false),
                CreatedOn = table.Column<DateTime>(nullable: false),
                CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                ActionType = table.Column<int>(maxLength: 50, nullable: false),
                TableName = table.Column<string>(maxLength: 255, nullable: true),
                OldValue = table.Column<string>(nullable: true),
                NewValue = table.Column<string>(nullable: true),
                AffectedColumn = table.Column<string>(nullable: true),
                ActionName = table.Column<string>(nullable: true)
            },
            constraints: table => { table.PrimaryKey("PK_HCL.CS.SF_AuditTrail", x => x.Id); });

        migrationBuilder.CreateTable(
            "HCL.CS.SF_Clients",
            table => new
            {
                Id = table.Column<Guid>(nullable: false),
                IsDeleted = table.Column<bool>(nullable: false),
                CreatedOn = table.Column<DateTime>(nullable: false),
                ModifiedOn = table.Column<DateTime>(nullable: true),
                CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                RowVersion = table.Column<DateTime>(rowVersion: true, nullable: true)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                ClientId = table.Column<string>(maxLength: 128, nullable: false),
                ClientName = table.Column<string>(maxLength: 255, nullable: true),
                ClientUri = table.Column<string>(nullable: true),
                ClientIdIssuedAt = table.Column<long>(nullable: false),
                ClientSecretExpiresAt = table.Column<long>(nullable: false),
                ClientSecret = table.Column<string>(maxLength: 128, nullable: true),
                LogoUri = table.Column<string>(nullable: true),
                TermsOfServiceUri = table.Column<string>(nullable: true),
                PolicyUri = table.Column<string>(nullable: true),
                RefreshTokenExpiration = table.Column<int>(nullable: false),
                AccessTokenExpiration = table.Column<int>(nullable: false),
                IdentityTokenExpiration = table.Column<int>(nullable: false),
                LogoutTokenExpiration = table.Column<int>(nullable: false),
                AuthorizationCodeExpiration = table.Column<int>(nullable: false),
                AccessTokenType = table.Column<int>(nullable: false),
                RequirePkce = table.Column<bool>(nullable: false),
                IsPkceTextPlain = table.Column<bool>(nullable: false),
                RequireClientSecret = table.Column<bool>(nullable: false),
                IsFirstPartyApp = table.Column<bool>(nullable: false),
                AllowOfflineAccess = table.Column<bool>(nullable: false),
                AllowedScopes = table.Column<string>(nullable: true),
                AllowAccessTokensViaBrowser = table.Column<bool>(nullable: false),
                ApplicationType = table.Column<int>(nullable: false),
                AllowedSigningAlgorithm = table.Column<string>(nullable: true),
                SupportedGrantTypes = table.Column<string>(nullable: true),
                SupportedResponseTypes = table.Column<string>(nullable: true),
                FrontChannelLogoutSessionRequired = table.Column<bool>(nullable: false),
                FrontChannelLogoutUri = table.Column<string>(nullable: true),
                BackChannelLogoutSessionRequired = table.Column<bool>(nullable: false),
                BackChannelLogoutUri = table.Column<string>(nullable: true)
            },
            constraints: table => { table.PrimaryKey("PK_HCL.CS.SF_Clients", x => x.Id); });

        migrationBuilder.CreateTable(
            "HCL.CS.SF_IdentityResources",
            table => new
            {
                Id = table.Column<Guid>(nullable: false),
                IsDeleted = table.Column<bool>(nullable: false),
                CreatedOn = table.Column<DateTime>(nullable: false),
                ModifiedOn = table.Column<DateTime>(nullable: true),
                CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                RowVersion = table.Column<DateTime>(rowVersion: true, nullable: true)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                Name = table.Column<string>(maxLength: 255, nullable: false),
                DisplayName = table.Column<string>(maxLength: 255, nullable: true),
                Description = table.Column<string>(nullable: true),
                Enabled = table.Column<bool>(nullable: false),
                Required = table.Column<bool>(nullable: false),
                Emphasize = table.Column<bool>(nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_HCL.CS.SF_IdentityResources", x => x.Id); });

        migrationBuilder.CreateTable(
            "HCL.CS.SF_Roles",
            table => new
            {
                Id = table.Column<Guid>(nullable: false),
                Name = table.Column<string>(maxLength: 255, nullable: false),
                NormalizedName = table.Column<string>(maxLength: 255, nullable: false),
                ConcurrencyStamp = table.Column<string>(maxLength: 255, nullable: false),
                Description = table.Column<string>(nullable: true),
                IsDeleted = table.Column<bool>(nullable: false),
                CreatedOn = table.Column<DateTime>(nullable: false),
                ModifiedOn = table.Column<DateTime>(nullable: true),
                CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                ModifiedBy = table.Column<string>(maxLength: 255, nullable: true)
            },
            constraints: table => { table.PrimaryKey("PK_HCL.CS.SF_Roles", x => x.Id); });

        migrationBuilder.CreateTable(
            "HCL.CS.SF_SecurityQuestions",
            table => new
            {
                Id = table.Column<Guid>(nullable: false),
                IsDeleted = table.Column<bool>(nullable: false),
                CreatedOn = table.Column<DateTime>(nullable: false),
                ModifiedOn = table.Column<DateTime>(nullable: true),
                CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                RowVersion = table.Column<DateTime>(rowVersion: true, nullable: true)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                Question = table.Column<string>(maxLength: 255, nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_HCL.CS.SF_SecurityQuestions", x => x.Id); });

        migrationBuilder.CreateTable(
            "HCL.CS.SF_SecurityTokens",
            table => new
            {
                Id = table.Column<Guid>(nullable: false),
                IsDeleted = table.Column<bool>(nullable: false),
                CreatedOn = table.Column<DateTime>(nullable: false),
                ModifiedOn = table.Column<DateTime>(nullable: true),
                CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                Key = table.Column<string>(nullable: true),
                TokenType = table.Column<string>(nullable: true),
                TokenValue = table.Column<string>(nullable: true),
                ClientId = table.Column<string>(nullable: true),
                SessionId = table.Column<string>(nullable: true),
                SubjectId = table.Column<string>(nullable: true),
                CreationTime = table.Column<DateTime>(nullable: false),
                ExpiresAt = table.Column<int>(nullable: false),
                ConsumedTime = table.Column<DateTime>(nullable: true)
            },
            constraints: table => { table.PrimaryKey("PK_HCL.CS.SF_SecurityTokens", x => x.Id); });

        migrationBuilder.CreateTable(
            "HCL.CS.SF_Users",
            table => new
            {
                Id = table.Column<Guid>(nullable: false),
                UserName = table.Column<string>(maxLength: 255, nullable: false),
                NormalizedUserName = table.Column<string>(maxLength: 255, nullable: false),
                Email = table.Column<string>(maxLength: 255, nullable: false),
                NormalizedEmail = table.Column<string>(maxLength: 255, nullable: false),
                EmailConfirmed = table.Column<bool>(nullable: false),
                PasswordHash = table.Column<string>(nullable: false),
                SecurityStamp = table.Column<string>(maxLength: 255, nullable: true),
                ConcurrencyStamp = table.Column<string>(maxLength: 255, nullable: true),
                PhoneNumber = table.Column<string>(maxLength: 15, nullable: true),
                PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                TwoFactorEnabled = table.Column<bool>(nullable: false),
                LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                LockoutEnabled = table.Column<bool>(nullable: false),
                AccessFailedCount = table.Column<int>(nullable: false),
                FirstName = table.Column<string>(maxLength: 255, nullable: false),
                LastName = table.Column<string>(maxLength: 255, nullable: true),
                DateOfBirth = table.Column<DateTime>(nullable: true),
                TwoFactorType = table.Column<int>(nullable: false),
                LastPasswordChangedDate = table.Column<DateTime>(nullable: true),
                RequiresDefaultPasswordChange = table.Column<bool>(nullable: true),
                LastLoginDateTime = table.Column<DateTime>(nullable: true),
                LastLogoutDateTime = table.Column<DateTime>(nullable: true),
                IdentityProviderType = table.Column<int>(nullable: false),
                IsDeleted = table.Column<bool>(nullable: false),
                CreatedOn = table.Column<DateTime>(nullable: false),
                ModifiedOn = table.Column<DateTime>(nullable: true),
                CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                ModifiedBy = table.Column<string>(maxLength: 255, nullable: true)
            },
            constraints: table => { table.PrimaryKey("PK_HCL.CS.SF_Users", x => x.Id); });

        migrationBuilder.CreateTable(
            "HCL.CS.SF_ApiResourceClaims",
            table => new
            {
                Id = table.Column<Guid>(nullable: false),
                IsDeleted = table.Column<bool>(nullable: false),
                CreatedOn = table.Column<DateTime>(nullable: false),
                ModifiedOn = table.Column<DateTime>(nullable: true),
                CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                RowVersion = table.Column<DateTime>(rowVersion: true, nullable: true)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                ApiResourceId = table.Column<Guid>(nullable: false),
                Type = table.Column<string>(maxLength: 255, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_HCL.CS.SF_ApiResourceClaims", x => x.Id);
                table.ForeignKey(
                    "FK_HCL.CS.SF_ApiResourceClaims_HCL.CS.SF_ApiResources_ApiResourceId",
                    x => x.ApiResourceId,
                    "HCL.CS.SF_ApiResources",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "HCL.CS.SF_ApiScopes",
            table => new
            {
                Id = table.Column<Guid>(nullable: false),
                IsDeleted = table.Column<bool>(nullable: false),
                CreatedOn = table.Column<DateTime>(nullable: false),
                ModifiedOn = table.Column<DateTime>(nullable: true),
                CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                RowVersion = table.Column<DateTime>(rowVersion: true, nullable: true)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                ApiResourceId = table.Column<Guid>(nullable: false),
                Name = table.Column<string>(maxLength: 255, nullable: false),
                DisplayName = table.Column<string>(maxLength: 255, nullable: true),
                Description = table.Column<string>(nullable: true),
                Required = table.Column<bool>(nullable: false),
                Emphasize = table.Column<bool>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_HCL.CS.SF_ApiScopes", x => x.Id);
                table.ForeignKey(
                    "FK_HCL.CS.SF_ApiScopes_HCL.CS.SF_ApiResources_ApiResourceId",
                    x => x.ApiResourceId,
                    "HCL.CS.SF_ApiResources",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "HCL.CS.SF_ClientPostLogoutRedirectUris",
            table => new
            {
                Id = table.Column<Guid>(nullable: false),
                IsDeleted = table.Column<bool>(nullable: false),
                CreatedOn = table.Column<DateTime>(nullable: false),
                ModifiedOn = table.Column<DateTime>(nullable: true),
                CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                RowVersion = table.Column<DateTime>(rowVersion: true, nullable: true)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                ClientId = table.Column<Guid>(nullable: false),
                PostLogoutRedirectUri = table.Column<string>(maxLength: 510, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_HCL.CS.SF_ClientPostLogoutRedirectUris", x => x.Id);
                table.ForeignKey(
                    "FK_HCL.CS.SF_ClientPostLogoutRedirectUris_HCL.CS.SF_Clients_ClientId",
                    x => x.ClientId,
                    "HCL.CS.SF_Clients",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "HCL.CS.SF_ClientRedirectUris",
            table => new
            {
                Id = table.Column<Guid>(nullable: false),
                IsDeleted = table.Column<bool>(nullable: false),
                CreatedOn = table.Column<DateTime>(nullable: false),
                ModifiedOn = table.Column<DateTime>(nullable: true),
                CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                RowVersion = table.Column<DateTime>(rowVersion: true, nullable: true)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                ClientId = table.Column<Guid>(nullable: false),
                RedirectUri = table.Column<string>(maxLength: 510, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_HCL.CS.SF_ClientRedirectUris", x => x.Id);
                table.ForeignKey(
                    "FK_HCL.CS.SF_ClientRedirectUris_HCL.CS.SF_Clients_ClientId",
                    x => x.ClientId,
                    "HCL.CS.SF_Clients",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "HCL.CS.SF_IdentityClaims",
            table => new
            {
                Id = table.Column<Guid>(nullable: false),
                IsDeleted = table.Column<bool>(nullable: false),
                CreatedOn = table.Column<DateTime>(nullable: false),
                ModifiedOn = table.Column<DateTime>(nullable: true),
                CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                RowVersion = table.Column<DateTime>(rowVersion: true, nullable: true)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                IdentityResourceId = table.Column<Guid>(nullable: false),
                Type = table.Column<string>(maxLength: 255, nullable: false),
                AliasType = table.Column<string>(maxLength: 255, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_HCL.CS.SF_IdentityClaims", x => x.Id);
                table.ForeignKey(
                    "FK_HCL.CS.SF_IdentityClaims_HCL.CS.SF_IdentityResources_IdentityResourceId",
                    x => x.IdentityResourceId,
                    "HCL.CS.SF_IdentityResources",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "HCL.CS.SF_RoleClaims",
            table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                RoleId = table.Column<Guid>(nullable: false),
                ClaimType = table.Column<string>(nullable: true),
                ClaimValue = table.Column<string>(nullable: true),
                IsDeleted = table.Column<bool>(nullable: false),
                CreatedOn = table.Column<DateTime>(nullable: false),
                ModifiedOn = table.Column<DateTime>(nullable: true),
                CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                RowVersion = table.Column<DateTime>(rowVersion: true, nullable: true)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_HCL.CS.SF_RoleClaims", x => x.Id);
                table.ForeignKey(
                    "FK_HCL.CS.SF_RoleClaims_HCL.CS.SF_Roles_RoleId",
                    x => x.RoleId,
                    "HCL.CS.SF_Roles",
                    "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            "HCL.CS.SF_Notification",
            table => new
            {
                Id = table.Column<Guid>(nullable: false),
                IsDeleted = table.Column<bool>(nullable: false),
                CreatedOn = table.Column<DateTime>(nullable: false),
                ModifiedOn = table.Column<DateTime>(nullable: true),
                CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                UserId = table.Column<Guid>(nullable: false),
                MessageId = table.Column<string>(maxLength: 255, nullable: false),
                Type = table.Column<int>(nullable: false),
                Activity = table.Column<string>(maxLength: 255, nullable: true),
                Status = table.Column<int>(maxLength: 255, nullable: false),
                Sender = table.Column<string>(maxLength: 255, nullable: false),
                Recipient = table.Column<string>(maxLength: 255, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_HCL.CS.SF_Notification", x => x.Id);
                table.ForeignKey(
                    "FK_HCL.CS.SF_Notification_HCL.CS.SF_Users_UserId",
                    x => x.UserId,
                    "HCL.CS.SF_Users",
                    "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            "HCL.CS.SF_PasswordHistory",
            table => new
            {
                Id = table.Column<Guid>(nullable: false),
                IsDeleted = table.Column<bool>(nullable: false),
                CreatedOn = table.Column<DateTime>(nullable: false),
                CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                UserID = table.Column<Guid>(nullable: false),
                ChangedOn = table.Column<DateTime>(nullable: false),
                PasswordHash = table.Column<string>(maxLength: 255, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_HCL.CS.SF_PasswordHistory", x => x.Id);
                table.ForeignKey(
                    "FK_HCL.CS.SF_PasswordHistory_HCL.CS.SF_Users_UserID",
                    x => x.UserID,
                    "HCL.CS.SF_Users",
                    "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            "HCL.CS.SF_UserClaims",
            table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                UserId = table.Column<Guid>(nullable: false),
                ClaimType = table.Column<string>(nullable: true),
                ClaimValue = table.Column<string>(nullable: true),
                IsAdminClaim = table.Column<bool>(nullable: false),
                IsDeleted = table.Column<bool>(nullable: false),
                CreatedOn = table.Column<DateTime>(nullable: false),
                ModifiedOn = table.Column<DateTime>(nullable: true),
                CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                RowVersion = table.Column<DateTime>(rowVersion: true, nullable: true)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_HCL.CS.SF_UserClaims", x => x.Id);
                table.ForeignKey(
                    "FK_HCL.CS.SF_UserClaims_HCL.CS.SF_Users_UserId",
                    x => x.UserId,
                    "HCL.CS.SF_Users",
                    "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            "HCL.CS.SF_UserLogins",
            table => new
            {
                LoginProvider = table.Column<string>(maxLength: 256, nullable: false),
                ProviderKey = table.Column<string>(maxLength: 256, nullable: false),
                UserId = table.Column<Guid>(nullable: false),
                ProviderDisplayName = table.Column<string>(nullable: true),
                Id = table.Column<Guid>(nullable: false),
                IsDeleted = table.Column<bool>(nullable: false),
                CreatedOn = table.Column<DateTime>(nullable: false),
                ModifiedOn = table.Column<DateTime>(nullable: true),
                CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                ModifiedBy = table.Column<string>(maxLength: 255, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_HCL.CS.SF_UserLogins", x => new { x.LoginProvider, x.ProviderKey, x.UserId });
                table.ForeignKey(
                    "FK_HCL.CS.SF_UserLogins_HCL.CS.SF_Users_UserId",
                    x => x.UserId,
                    "HCL.CS.SF_Users",
                    "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            "HCL.CS.SF_UserRoles",
            table => new
            {
                UserId = table.Column<Guid>(nullable: false),
                RoleId = table.Column<Guid>(nullable: false),
                Id = table.Column<Guid>(nullable: false),
                ValidFrom = table.Column<DateTime>(nullable: true),
                ValidTo = table.Column<DateTime>(nullable: true),
                IsDeleted = table.Column<bool>(nullable: false),
                CreatedOn = table.Column<DateTime>(nullable: false),
                ModifiedOn = table.Column<DateTime>(nullable: true),
                CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                RowVersion = table.Column<DateTime>(rowVersion: true, nullable: true)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_HCL.CS.SF_UserRoles", x => new { x.Id, x.UserId, x.RoleId });
                table.ForeignKey(
                    "FK_HCL.CS.SF_UserRoles_HCL.CS.SF_Roles_RoleId",
                    x => x.RoleId,
                    "HCL.CS.SF_Roles",
                    "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    "FK_HCL.CS.SF_UserRoles_HCL.CS.SF_Users_UserId",
                    x => x.UserId,
                    "HCL.CS.SF_Users",
                    "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            "HCL.CS.SF_UserSecurityQuestions",
            table => new
            {
                Id = table.Column<Guid>(nullable: false),
                IsDeleted = table.Column<bool>(nullable: false),
                CreatedOn = table.Column<DateTime>(nullable: false),
                ModifiedOn = table.Column<DateTime>(nullable: true),
                CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                RowVersion = table.Column<DateTime>(rowVersion: true, nullable: true)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                UserId = table.Column<Guid>(nullable: false),
                SecurityQuestionId = table.Column<Guid>(nullable: false),
                Answer = table.Column<string>(maxLength: 255, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_HCL.CS.SF_UserSecurityQuestions", x => x.Id);
                table.ForeignKey(
                    "FK_HCL.CS.SF_UserSecurityQuestions_HCL.CS.SF_SecurityQuestions_SecurityQuesti~",
                    x => x.SecurityQuestionId,
                    "HCL.CS.SF_SecurityQuestions",
                    "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    "FK_HCL.CS.SF_UserSecurityQuestions_HCL.CS.SF_Users_UserId",
                    x => x.UserId,
                    "HCL.CS.SF_Users",
                    "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            "HCL.CS.SF_UserTokens",
            table => new
            {
                UserId = table.Column<Guid>(nullable: false),
                LoginProvider = table.Column<string>(maxLength: 255, nullable: false),
                Name = table.Column<string>(maxLength: 255, nullable: false),
                Value = table.Column<string>(nullable: false),
                IsDeleted = table.Column<bool>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_HCL.CS.SF_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                table.ForeignKey(
                    "FK_HCL.CS.SF_UserTokens_HCL.CS.SF_Users_UserId",
                    x => x.UserId,
                    "HCL.CS.SF_Users",
                    "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            "HCL.CS.SF_ApiScopeClaims",
            table => new
            {
                Id = table.Column<Guid>(nullable: false),
                IsDeleted = table.Column<bool>(nullable: false),
                CreatedOn = table.Column<DateTime>(nullable: false),
                ModifiedOn = table.Column<DateTime>(nullable: true),
                CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                RowVersion = table.Column<DateTime>(rowVersion: true, nullable: true)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                ApiScopeId = table.Column<Guid>(nullable: false),
                Type = table.Column<string>(maxLength: 255, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_HCL.CS.SF_ApiScopeClaims", x => x.Id);
                table.ForeignKey(
                    "FK_HCL.CS.SF_ApiScopeClaims_HCL.CS.SF_ApiScopes_ApiScopeId",
                    x => x.ApiScopeId,
                    "HCL.CS.SF_ApiScopes",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            "IX_APIRES_CLM_RESID_TYPE",
            "HCL.CS.SF_ApiResourceClaims",
            new[] { "ApiResourceId", "Type" },
            unique: true);

        migrationBuilder.CreateIndex(
            "IX_APIRES_NAME",
            "HCL.CS.SF_ApiResources",
            "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            "IX_APISCO_CLM_SCOID_TYPE",
            "HCL.CS.SF_ApiScopeClaims",
            new[] { "ApiScopeId", "Type" },
            unique: true);

        migrationBuilder.CreateIndex(
            "IX_APISCO_SCOID_NAME",
            "HCL.CS.SF_ApiScopes",
            new[] { "ApiResourceId", "Name" },
            unique: true);

        migrationBuilder.CreateIndex(
            "IX_AUD_CBBY_ACTY",
            "HCL.CS.SF_AuditTrail",
            new[] { "CreatedBy", "ActionType" });

        migrationBuilder.CreateIndex(
            "IX_AUD_CRON_ACTY",
            "HCL.CS.SF_AuditTrail",
            new[] { "CreatedOn", "ActionType" });

        migrationBuilder.CreateIndex(
            "IX_AUD_CRON_CBBY",
            "HCL.CS.SF_AuditTrail",
            new[] { "CreatedOn", "CreatedBy" });

        migrationBuilder.CreateIndex(
            "IX_HCL.CS.SF_ClientPostLogoutRedirectUris_ClientId_PostLogoutRedirectU~",
            "HCL.CS.SF_ClientPostLogoutRedirectUris",
            new[] { "ClientId", "PostLogoutRedirectUri" },
            unique: true);

        migrationBuilder.CreateIndex(
            "IX_HCL.CS.SF_ClientRedirectUris_ClientId_RedirectUri",
            "HCL.CS.SF_ClientRedirectUris",
            new[] { "ClientId", "RedirectUri" },
            unique: true);

        migrationBuilder.CreateIndex(
            "IX_CLI_CLID_CLSEC",
            "HCL.CS.SF_Clients",
            new[] { "ClientId", "ClientSecret" },
            unique: true);

        migrationBuilder.CreateIndex(
            "IX_IDRESCLM_IDRESID_TYPE",
            "HCL.CS.SF_IdentityClaims",
            new[] { "IdentityResourceId", "Type" },
            unique: true);

        migrationBuilder.CreateIndex(
            "IX_IDRES_NAME",
            "HCL.CS.SF_IdentityResources",
            "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            "IX_NOTI_TYPE",
            "HCL.CS.SF_Notification",
            "Type");

        migrationBuilder.CreateIndex(
            "IX_HCL.CS.SF_Notification_UserId",
            "HCL.CS.SF_Notification",
            "UserId");

        migrationBuilder.CreateIndex(
            "IX_HCL.CS.SF_PasswordHistory_UserID",
            "HCL.CS.SF_PasswordHistory",
            "UserID");

        migrationBuilder.CreateIndex(
            "IX_HCL.CS.SF_RoleClaims_RoleId",
            "HCL.CS.SF_RoleClaims",
            "RoleId");

        migrationBuilder.CreateIndex(
            "RoleNameIndex",
            "HCL.CS.SF_Roles",
            "NormalizedName",
            unique: true);

        migrationBuilder.CreateIndex(
            "IX_SEC_QUESTION",
            "HCL.CS.SF_SecurityQuestions",
            "Question",
            unique: true);

        migrationBuilder.CreateIndex(
            "IX_HCL.CS.SF_UserClaims_UserId",
            "HCL.CS.SF_UserClaims",
            "UserId");

        migrationBuilder.CreateIndex(
            "IX_HCL.CS.SF_UserLogins_UserId",
            "HCL.CS.SF_UserLogins",
            "UserId");

        migrationBuilder.CreateIndex(
            "IX_HCL.CS.SF_UserRoles_RoleId",
            "HCL.CS.SF_UserRoles",
            "RoleId");

        migrationBuilder.CreateIndex(
            "IX_HCL.CS.SF_UserRoles_UserId",
            "HCL.CS.SF_UserRoles",
            "UserId");

        migrationBuilder.CreateIndex(
            "EmailIndex",
            "HCL.CS.SF_Users",
            "NormalizedEmail");

        migrationBuilder.CreateIndex(
            "UserNameIndex",
            "HCL.CS.SF_Users",
            "NormalizedUserName",
            unique: true);

        migrationBuilder.CreateIndex(
            "IX_USRSEC_QUEID",
            "HCL.CS.SF_UserSecurityQuestions",
            "SecurityQuestionId");

        migrationBuilder.CreateIndex(
            "IX_USRSEC_UID_QUEID",
            "HCL.CS.SF_UserSecurityQuestions",
            new[] { "UserId", "SecurityQuestionId" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            "HCL.CS.SF_ApiResourceClaims");

        migrationBuilder.DropTable(
            "HCL.CS.SF_ApiScopeClaims");

        migrationBuilder.DropTable(
            "HCL.CS.SF_AuditTrail");

        migrationBuilder.DropTable(
            "HCL.CS.SF_ClientPostLogoutRedirectUris");

        migrationBuilder.DropTable(
            "HCL.CS.SF_ClientRedirectUris");

        migrationBuilder.DropTable(
            "HCL.CS.SF_IdentityClaims");

        migrationBuilder.DropTable(
            "HCL.CS.SF_Notification");

        migrationBuilder.DropTable(
            "HCL.CS.SF_PasswordHistory");

        migrationBuilder.DropTable(
            "HCL.CS.SF_RoleClaims");

        migrationBuilder.DropTable(
            "HCL.CS.SF_SecurityTokens");

        migrationBuilder.DropTable(
            "HCL.CS.SF_UserClaims");

        migrationBuilder.DropTable(
            "HCL.CS.SF_UserLogins");

        migrationBuilder.DropTable(
            "HCL.CS.SF_UserRoles");

        migrationBuilder.DropTable(
            "HCL.CS.SF_UserSecurityQuestions");

        migrationBuilder.DropTable(
            "HCL.CS.SF_UserTokens");

        migrationBuilder.DropTable(
            "HCL.CS.SF_ApiScopes");

        migrationBuilder.DropTable(
            "HCL.CS.SF_Clients");

        migrationBuilder.DropTable(
            "HCL.CS.SF_IdentityResources");

        migrationBuilder.DropTable(
            "HCL.CS.SF_Roles");

        migrationBuilder.DropTable(
            "HCL.CS.SF_SecurityQuestions");

        migrationBuilder.DropTable(
            "HCL.CS.SF_Users");

        migrationBuilder.DropTable(
            "HCL.CS.SF_ApiResources");
    }
}
